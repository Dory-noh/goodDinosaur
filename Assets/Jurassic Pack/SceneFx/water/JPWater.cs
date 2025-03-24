using UnityEngine;
[ExecuteInEditMode]
public class JPWater : MonoBehaviour
{
	[Header("REFLECTION RENDERTEXTURE")]
  public Camera reflectionCamera=null;
	[Range(8, 1024)] public int resolution=128;
	public Material waterMat=null;
  private RenderTexture reflectionRt=null;
	private static bool onRend=false;
	[Header("UNDERWATER FX")]
	[SerializeField] bool useUnderwaterFx=true;
	public Light directionalLight;
	public FlareLayer sunflare;
	public AudioSource underwaterSnd;
	public Texture[] lightCookie;
	[SerializeField]  private float underwaterDensity=0.0f;
	private float startFogDensity=0.001f;
	private Vector3 startLightDir=Vector3.zero;
	private Color startFogColor=new Color (0.7f,0.9f,1.0f,1.0f);
	private float screenWaterY=1;

	[Header("WATER PARTICLES FX")]
	[SerializeField] bool useParticlesFx=true;
	public ParticleSystem ripples;
	public ParticleSystem splash;
	public AudioClip splashSnd;
	private float count=0;

  private void Start()
  {
		startFogDensity=RenderSettings.fogDensity;
		startFogColor=RenderSettings.fogColor;
    startLightDir=directionalLight.transform.forward;
  }

  private void OnWillRenderObject()
  {
		if(onRend | !Camera.current | !reflectionCamera | !waterMat) return;
    if(reflectionRt==null | (reflectionRt&& reflectionRt.width!=resolution))
		{ reflectionRt=RenderTexture.GetTemporary(resolution, resolution, 24); }
		Camera cam=Camera.current;

		Vector3 n=transform.up, p=transform.position; float l=-Vector3.Dot(n, p);
		Matrix4x4 m=new Matrix4x4
		{
			m00=1-2*n.x*n.x, m01=-2*n.x*n.y,  m02=-2*n.x*n.z,  m03=-2*l*n.x,
			m10=-2*n.x*n.y,	 m11=1-2*n.y*n.y, m12=-2*n.y*n.z,  m13=-2*l*n.y,
			m20=-2*n.x*n.z,  m21=-2*n.y*n.z,  m22=1-2*n.z*n.z, m23=-2*l*n.z,
			m30=0, m31=0, m32=0, m33=1
		};

		//Set Cam
		reflectionCamera.enabled=false;
		Vector3 n2 =-cam.worldToCameraMatrix.MultiplyVector(n).normalized;
		Vector4 clip=new Vector4(n2.x, n2.y, n2.z, -Vector3.Dot(cam.worldToCameraMatrix.MultiplyPoint((p+n)*0.9f), n2));
		reflectionCamera.projectionMatrix=cam.CalculateObliqueMatrix(clip);
		reflectionCamera.worldToCameraMatrix=cam.worldToCameraMatrix*m;
			
    reflectionCamera.clearFlags=cam.clearFlags;
    reflectionCamera.backgroundColor=cam.backgroundColor;
    reflectionCamera.farClipPlane=cam.farClipPlane;
    reflectionCamera.nearClipPlane=cam.nearClipPlane;
    reflectionCamera.orthographic=cam.orthographic;
    reflectionCamera.fieldOfView=cam.fieldOfView;
    reflectionCamera.aspect=cam.aspect;
    reflectionCamera.orthographicSize=cam.orthographicSize;
		reflectionCamera.targetTexture=reflectionRt;

		reflectionCamera.transform.SetPositionAndRotation(cam.transform.position, cam.transform.rotation);
    if(reflectionCamera.rect.size!=Vector2.one) return;
		GL.invertCulling=true; reflectionCamera.Render(); GL.invertCulling=false;
    
		waterMat.SetTexture("_ReflectionRT", reflectionRt);
		onRend=false;
  }

	//UNDERWATER EFFECT
	private void FixedUpdate ()
	{
		Camera cam=Camera.current;
		if(!Application.isPlaying | !useUnderwaterFx | !cam) return;

    //Get screen water altitude
    float d_l=cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane)).y;
		float u_l=cam.ScreenToWorldPoint(new Vector3(0, Screen.height, cam.nearClipPlane)).y;
		float d_r=cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, cam.nearClipPlane)).y;
		float u_r=cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane)).y;
		screenWaterY=Mathf.Clamp( (Mathf.Min(d_l, d_r)-transform.position.y) / (Mathf.Min(d_l, d_r) - Mathf.Min(u_l, u_r)) , -16.0f, 16.0f);
    //Get water material color
    Color32 col=Color32.Lerp(waterMat.GetColor("_ReefCol"), waterMat.GetColor("_Col")*1.5f, screenWaterY/4f);
    //Fog color & density
    RenderSettings.fogColor=Color32.Lerp(startFogColor, col, Mathf.Clamp01(screenWaterY));
    RenderSettings.fogDensity=Mathf.Lerp(startFogDensity, underwaterDensity, Mathf.Clamp01(screenWaterY));
    cam.backgroundColor=RenderSettings.fogColor;
		if(screenWaterY>0.5f)
		{ 
      if(!underwaterSnd.isPlaying)
      {
        underwaterSnd.Play(); // play underwater sound
        sunflare.enabled=false; //Disable sun flare
        cam.clearFlags=CameraClearFlags.SolidColor; // Set CameraClearFlags
			  directionalLight.transform.forward=-Vector3.up; // Set light direction
      }
      if(lightCookie.Length>0)
      directionalLight.cookie=lightCookie[Mathf.FloorToInt((Time.fixedTime*16)%lightCookie.Length)]; //Animate light cookie
		}
    else if(underwaterSnd.isPlaying)
    {
      underwaterSnd.Stop(); //Stop underwater sound 
      sunflare.enabled=true; //Enable sun flare
      cam.clearFlags=CameraClearFlags.Skybox;	// Reset CameraClearFlags
      directionalLight.transform.forward=startLightDir; // Reset light direction
      directionalLight.cookie=null; // Remove light cookie
    }
	}


	//PARTICLES EFFECT
	private void OnTriggerStay(Collider col) { if(!useParticlesFx) return; WaterParticleFX(col, ripples); }
	private void OnTriggerExit(Collider col) { if(!useParticlesFx) return; WaterParticleFX(col, splash); }
	private void OnTriggerEnter(Collider col) { if(!useParticlesFx) return; WaterParticleFX(col, splash); }

	private void WaterParticleFX(Collider col, ParticleSystem particleFx)
	{
		count+=Time.fixedDeltaTime; ParticleSystem particle; Creature cs;

		//Has a Rigidbody component ?
		if(col.transform.root.GetComponent<Rigidbody>())
		{
			//Is a JP Creature?
			if(col.transform.root.CompareTag("Creature"))
			{
				cs=col.transform.root.GetComponent<Creature>(); //Get creature script
				cs.waterY=transform.position.y; //water altitude
				if(!cs.isVisible) return; //Check if creature is visible
				if(particleFx==ripples && count<cs.loop%10) return; //prevent particle overflow
				SkinnedMeshRenderer rend= cs.rend[0];

				//Check if the creature bounds are in contact with the water surface
				if(rend.bounds.Contains(new Vector3(col.transform.position.x, transform.position.y, col.transform.position.z)))
				{
					//Check if the creature are in motion
					if(!cs.anm.GetInteger("Move").Equals(0) | (cs.canFly && cs.isOnLevitation) | cs.onJump | cs.onAttack)
					{
						if(particleFx==splash&&(!cs.isOnGround | cs.onJump))
						{
							col.transform.root.GetComponents<AudioSource>()[1].pitch=Random.Range(0.5f,0.75f);
							col.transform.root.GetComponents<AudioSource>()[1].PlayOneShot(splashSnd,Random.Range(0.5f,0.75f));
						} else particleFx=ripples;
					}
					else return;
					
					//Spawn particle 
					Vector3 pos=new Vector3(rend.bounds.center.x, cs.waterY+0.01f, rend.bounds.center.z); //spawn position
					particle=Instantiate(particleFx, pos, Quaternion.identity) as ParticleSystem;
					particle.gameObject.hideFlags=HideFlags.HideAndDontSave;
					particle.transform.localScale=rend.bounds.size/10f; //Set particle size relative to creature size x
					Destroy(particle.gameObject, 3.0f); //Destroy particle after 3 sec
					count=0;
				}
			}
		}
	}

}