using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
  #region VARIABLES
  const string TIP1=
	"Disable creatures management.\n"+
	"Creatures A.I. still work, player inputs, camera behavior and GUI features are disabled.\n"+
	"Useful if you want to use a third party asset e.g. fps controller. "+
	"However, manager component still to be attached to the MainCam. ";
	[Header("JURASSIC PACK MANAGER")]
	[Tooltip(TIP1)]
	public bool useManager=true;
	[Tooltip("Instructs the game to try to render at a specified frame rate.\n -1 value is the platform's default frame rate.")]
	[SerializeField] int FPS_Limit=60;
	[SerializeField] bool showGUI=true;
	[SerializeField] bool showFPS=true;
	public Texture2D helpscreen;
	public Texture2D icons;
	[SerializeField] bool invertYAxis=false;
	[SerializeField] [Range(0.1f, 16.0f)] float sensivity=5f;
	public AudioClip windSnd;
	[Space (10)]
	[Header("GLOBAL CREATURES SETTINGS")]
	[Tooltip("Add your creatures prefabs here, this will make it spawnable during game.")]
	public List<GameObject> collectionList;
  const string TIP2="Inverse Kinematics - Accurate feet placement on ground";
  [Tooltip(TIP2)]
	public bool useIK;
	[Tooltip("Creatures will be active even if they are no longer visible. (performance may be affected).")]
	public bool realtimeGame;
  [Tooltip("Countdown to destroy the creature after his dead. Put 0 to cancels the countdown, the body will remain on the scene without disappearing.")]
  public int timeAfterDead=10000;
	const string TIP3=
	"ENABLED : allow creatures to walk on all kind of collider. (more expensive).\n"+"\n"+
	"DISABLED : creatures can only walk on Terrain collider (faster).\n";
	[Tooltip(TIP3)]
	public bool useRaycast;
  [Tooltip("Layer used for water.")]
	public int waterLayer;
	[Tooltip("Unity terrain tree layer, the layer must be defined into tree model prefab")]
	public int treeLayer;
  [Tooltip("The maximium walkable slope before the creature start slipping.")]
  [Range(0.1f, 1.0f)] public float MaxSlope=0.75f;
  [Tooltip("Water plane altitude")]
  public float waterAlt=55;
  [Tooltip("Blood particle for creatures")]
  public ParticleSystem blood;

	[HideInInspector] public List<GameObject> creaturesList, playersList; //list of all creatures/players in game
	[HideInInspector] public int selected, cameraMode=1, message=0; //creature index, camera mode, game messages
  //Terrain datas
	[HideInInspector] public Terrain t=null;
  [HideInInspector] public TerrainData tdata=null;
  [HideInInspector] public Vector3 tpos=Vector3.zero;
  [HideInInspector] public float tres=0;

	[HideInInspector] public int toolBarTab=-1, addCreatureTab=-2, count=0; //toolbar tab
  private bool browser =false; //creature browser
	private Vector2 scroll1=Vector2.zero, scroll2=Vector2.zero; //Scroll position
	private float vx, vy, vz=25; //camera angle/zoom
	private float mouseX,mouseY,wheel,horizontal,vertical; //Axis
	private bool mouse1,mouse2;	//mouse buttons
	private bool space,shift,control,keyC,keyX,keyY,escape; //keyboard buttons
	private float timer, frame, fps; //fps counter
	private Rigidbody body;
	private AudioSource source;
	private bool spawnAI=true, rndSkin=true,  rndSize=true, rndSetting=true;
	private int rndSizeSpan=1;
  #endregion
  #region START
	void Awake()
	{
		//Find all JP creatures/players prefab in scene
		GameObject[] creatures= GameObject.FindGameObjectsWithTag("Creature");
		GameObject[] players= GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject element in creatures )
		{ 
			if(!element.name.EndsWith("(Clone)")) creaturesList.Add(element.gameObject); //Add to list
			else Destroy(element.gameObject); //Delete unwanted ghost objects in hierarchy
		}
		foreach (GameObject element in players ) {playersList.Add(element.gameObject); }//Add to list

		if(useManager)
		{
			Cursor.visible=false; Cursor.lockState=CursorLockMode.Locked;
			body=transform.root.GetComponent<Rigidbody>();
			source=transform.root.GetComponent<AudioSource>();
		}

		//Get terrain datas
		if(Terrain.activeTerrain)
		{
			t =Terrain.activeTerrain;
      tdata=t.terrainData;
      tpos=t.GetPosition();
      tres=tdata.heightmapResolution;
		}

	  //Layers left-shift
	   treeLayer=(1 << treeLayer);
  }
  #endregion
  #region CAMERA BEHAVIOR
  void Update()
	{
		if(!useManager) return;
		//Fps counter
		if(showFPS) { frame += 1.0f; timer += Time.deltaTime; if(timer>=1.0f) { fps=frame; timer=0.0f; frame=0.0f; } }
		Application.targetFrameRate=FPS_Limit;
		

		mouseX=Input.GetAxis("Mouse X");
		mouseY=Input.GetAxis("Mouse Y");
		wheel=Input.GetAxis("Mouse ScrollWheel");
		horizontal=Input.GetAxis("Horizontal");
		vertical=Input.GetAxis("Vertical");
		mouse1=Input.GetKey(KeyCode.Mouse1);
		mouse2=Input.GetKey(KeyCode.Mouse2);
		space=Input.GetKey(KeyCode.Space);
		shift=Input.GetKey(KeyCode.LeftShift);
		control=Input.GetKey(KeyCode.LeftControl);
		keyC=Input.GetKeyDown(KeyCode.C);
		keyX=Input.GetKeyDown(KeyCode.X);
		keyY=Input.GetKeyDown(KeyCode.Y);
		escape=Input.GetKeyDown(KeyCode.Escape);

		
		//Lock/Unlock cursor
		if(Application.isEditor)
		{
			if(escape && toolBarTab==-1) { Cursor.lockState=CursorLockMode.None; toolBarTab=1; }
			else if(escape && toolBarTab!=-1) { Cursor.lockState=CursorLockMode.None; toolBarTab=-1; }
			else if(toolBarTab==-1) Cursor.lockState=CursorLockMode.Locked;
		}
		else
		{
			if(Cursor.lockState==CursorLockMode.None && escape) Cursor.lockState=CursorLockMode.Locked;
			else if(escape) Cursor.lockState=CursorLockMode.None;
		}

		//Creature select (Shortcut Key)
		if(keyX) { if(selected>0) selected--; else selected=creaturesList.Count-1; }
		else if(keyY) { if(selected<creaturesList.Count-1) selected++; else selected=0; }
		if(keyC) { if(cameraMode==2) cameraMode=0; else cameraMode++; } //Change View (Shortcut Key)
	}


	void FixedUpdate()
	{
		if(!useManager) return;

		Creature creature=null;
		//If creature not found, switch to free camera mode
		if(creaturesList.Count==0) cameraMode=0;
		else if(!creaturesList[selected] | !creaturesList[selected].activeInHierarchy) cameraMode=0;
		else creature=creaturesList[selected].GetComponent<Creature>(); //Get creature script

		//Prevent camera from going into terrain
		if(t && (t.SampleHeight(transform.root.position)+t.GetPosition().y)>transform.root.position.y-1.0f)
		{
			body.velocity=new Vector3(body.velocity.x, 0, body.velocity.z);
			transform.root.position=new Vector3(transform.root.position.x, (t.SampleHeight(transform.root.position)+t.GetPosition().y)+1.0f, transform.root.position.z);
		}

		switch(cameraMode)
		{
		//Free
		case 0:
			if(source.clip==null) source.clip=windSnd; else if(source.clip==windSnd)
			{
				if(source.isPlaying) { source.volume=body.velocity.magnitude/128; source.pitch=source.volume; }
				else source.PlayOneShot(windSnd);
			}

			Vector3 dir; float y;
			if(shift) body.mass=0.025f; else body.mass=0.1f; body.drag=1.0f;
			if(Cursor.lockState==CursorLockMode.Locked | mouse2)
			{
				vx+=mouseX*sensivity; //rotate cam X axe
				vy=Mathf.Clamp(invertYAxis?vy+mouseY*sensivity:vy-mouseY*sensivity, -89.9f, 89.9f); //rotate cam Y axe
				transform.rotation=Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(vx, Vector3.up)*Quaternion.AngleAxis(vy, Vector3.right), 0.1f);
			}
			
			if(space) y=1; else if(control) y=-1; else y=0;
			dir=transform.rotation*new Vector3(horizontal, y, vertical); //move
			body.AddForce(dir*(transform.root.position-(transform.root.position+dir)).magnitude);
		break;
		//Follow camera
		case 1:
			body.mass=1.0f; body.drag=10.0f; float size=creature.withersSize;
			if(Cursor.lockState==CursorLockMode.Locked | mouse2)
			{
				if(mouse1)
				{
					vx=creaturesList[selected].transform.eulerAngles.y; //lock camera to creature angle
					if(creature.isOnLevitation)
					{ vy=Mathf.Clamp(Mathf.Lerp(vy, creature.anm.GetFloat("Pitch")*90, 0.01f), -45f, 90f); }//pitch flying creature with camera axe
					 else
					{ vy=Mathf.Clamp(invertYAxis?vy-mouseY*sensivity : vy+mouseY*sensivity, -90f, 90f); } //rotate cam Y axe
				}
				else if(!mouse2 | Cursor.lockState!=CursorLockMode.Locked)
				{
					vx+=mouseX*sensivity; //rotate cam X axe
					vy=Mathf.Clamp(invertYAxis?vy-mouseY*sensivity:vy+mouseY*sensivity, -90f, 90f); //rotate cam Y axe
				}
			}
			vz=Mathf.Clamp(vz-wheel*10, size, size*32f); //zoom cam Z axe
			transform.rotation=Quaternion.Lerp(transform.rotation, Quaternion.Euler(vy, vx, 0.0f), 0.1f);
			Vector3 pos=((creaturesList[selected].transform.root.position+Vector3.up*size*1.5f)-transform.root.position)-transform.forward*vz;
			body.AddForce(pos*128f);
		break;
		// POV camera
		case 2:
			size=creature.withersSize;
			transform.rotation=Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((creaturesList[selected].transform.root.position+Vector3.up*size*1.5f)-transform.root.position), 0.1f);
		break;
		default: cameraMode=0; break;
		}
	}
  #endregion
  #region DRAW GUI
  void OnGUI ()
	{
		if(!useManager) return;

		float sw=Screen.width, sh=Screen.height;

		Creature creature=null;
		if(creaturesList.Count>0 && creaturesList[selected] && creaturesList[selected].activeInHierarchy)
			creature=creaturesList[selected].GetComponent<Creature>();  //Get creature script

		GUIStyle style=new GUIStyle("box") { fontSize=16 };
		if(Cursor.lockState==CursorLockMode.None)
		{
			//Show creature info on toolbar & Camera mode select
			if(creature&&cameraMode!=0)
			{
				GUI.Box(new Rect(0, 0, sw, 50), creaturesList[selected].name);
				GUI.color=Color.yellow; if(GUI.Button(new Rect(0,5, (sw/16)-4, 20), "Free")) cameraMode=0;
				if(cameraMode==1) GUI.color=Color.green; if(GUI.Button(new Rect((sw/16)*1.5f, 5, (sw/16)-4, 20), "Follow")) cameraMode=1; GUI.color=Color.yellow;
				if(cameraMode==2) GUI.color=Color.green; if(GUI.Button(new Rect((sw/16)*3.0f, 5, (sw/16)-4, 20), "POV")) cameraMode=2;
			}
			else
			{
				GUI.Box(new Rect(0, 0, sw, 50), "", style);
				if(creature)
				{
					GUI.color=Color.green; GUI.Button(new Rect(0,5, (sw/16)-4, 20), "Free"); GUI.color=Color.yellow;
					if(GUI.Button(new Rect((sw/16)*1.5f, 5, (sw/16)-4, 20), "Follow")) cameraMode=1;
					if(GUI.Button(new Rect((sw/16)*3.0f, 5, (sw/16)-4, 20), "POV")) cameraMode=2; 
				}
			}
			GUI.color=Color.white;

			Cursor.visible=true;
			//Toolbar tabs
			if(!showGUI) GUI.Box(new Rect(0, 0, sw, 50), ""); 
			string[] toolbarStrings=new string[] {"File", "Creatures", "Options", "Help"};
			GUI.color=Color.yellow; toolBarTab=GUI.Toolbar(new Rect(0, 30, sw, 20), toolBarTab, toolbarStrings); GUI.color=Color.white;

			switch(toolBarTab)
			{
			//File
			case 0: GUI.Box (new Rect(0, 50, sw, sh-50), "", style);
				if(GUI.Button(new Rect((sw/2)-60, (sh/2)-35, 120, 30), "Reset")) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //Reset				
				if(GUI.Button (new Rect((sw/2)-60, (sh/2)+5, 120, 30), "Quit")) Application.Quit(); //Quit
			break;
			//Creatures
			case 1:
			if(creature)
			{
				// Creature box
				GUI.Box (new Rect(0, 50, sw*0.25f, (sh*0.75f)-50),"");

				// Creature name - rename field
				string rename=creaturesList[selected].name;
				creaturesList[selected].name=GUI.TextField(new Rect(25, 50, sw*0.25f-25, 25), rename, style);

				//Delete button
				if(GUI.Button (new Rect(0, 50, 25, 25), "X"))
				{
					Destroy(creaturesList[selected].gameObject);  creaturesList.RemoveAt(selected);
					if(selected>0) selected--; else if(creaturesList.Count>0) selected=creaturesList.Count-1; else return;
				}

				//Browse Creatures
				if(browser)
				{
					if(GUI.Button (new Rect(0, 75, (sw*0.25f), 25), "Close Browser" )) browser=false;
					GUI.Box (new Rect(0, 100, sw*0.25f, (sh*0.75f)-100),"Creatures : "+creaturesList.Count);
					scroll1=GUI.BeginScrollView(new Rect(0, 130, sw*0.25f, (sh*0.75f)-140), scroll1, new Rect(0, 0, 270, creaturesList.Count*40), false, true);

					int end_i=creaturesList.Count, start=Mathf.RoundToInt(scroll1.y/40);
					end_i=Mathf.Clamp(end_i, start , start+Mathf.RoundToInt((sh*0.75f)/40));
		
					for(int i=start; i<end_i; i++)
					{
						float line=40*i;
						//green light for current selected creature
						if(selected!=i) GUI.color=Color.gray; else GUI.color=Color.white;
						//draw creature array num
						GUI.Label (new Rect(5, line, sw*0.25f-30, 25), (i+1)+". " );
						// delete button
						if(GUI.Button (new Rect(30, line, 20, 20), "X" ))
						{
							if(i<=selected && selected>0 | selected==creaturesList.Count-1) selected--;
							Destroy(creaturesList[i].gameObject);  creaturesList.RemoveAt(i); return;
						}
						//button show creature name/select creature
						if(GUI.Button (new Rect(50, line, 140, 20), creaturesList[i].name )) { selected=i; cameraMode=1; }
						if(GUI.Button (new Rect(190, line, 40, 20), "Edit" )) { selected=i;  browser=false; }
						//get creature script
						Creature Script= creaturesList[i].gameObject.GetComponent<Creature>();
						//show current creature behavior
						GUI.Label (new Rect(235, line, sw*0.25f-30, 25), Script.behavior+"  "+Script.behaviorCount);
						//browser health bar
						Rect bar= new Rect(0, 0, 0.1f, 0.1f);
						GUI.color=Color.black; GUI.DrawTextureWithTexCoords(new Rect(30, line+22, 50, 4), icons, bar, false);
						GUI.DrawTextureWithTexCoords(new Rect(85, line+22, 50, 4), icons, bar, false);
						GUI.color=Color.green; GUI.DrawTextureWithTexCoords(new Rect(30, line+22, Script.health/2, 4), icons, bar, false); 
						GUI.color=Color.yellow; GUI.DrawTextureWithTexCoords(new Rect(85,line+22, Script.food/2, 4), icons, bar, false);
						if(!Script.canSwim)
						{
							GUI.color=Color.black; GUI.DrawTextureWithTexCoords(new Rect(140, line+22, 50, 4), icons, bar, false);
							GUI.DrawTextureWithTexCoords(new Rect(195, line+22, 50, 4), icons, bar, false);
							GUI.color=Color.cyan; GUI.DrawTextureWithTexCoords(new Rect(140, line+22, Script.water/2, 4), icons, bar, false); 
							GUI.color=Color.gray; GUI.DrawTextureWithTexCoords(new Rect(195,line+22, Script.stamina/2, 4), icons, bar, false); 
						}
					}
					GUI.EndScrollView();
				}
				else
				{
					//Start browse
					if(GUI.Button (new Rect((sw*0.25f)/4, 75, (sw*0.25f)/2, 20), "Browse : "+(selected+1)+"/"+creaturesList.Count )) browser=true;

					//Creature select
					if(GUI.Button (new Rect(0, 75,  (sw*0.25f)/4, 20), "<<")) 	{ if(selected>0) selected--; else selected=creaturesList.Count-1; } 
					if(GUI.Button (new Rect(((sw*0.25f)/4)*3, 75, (sw*0.25f)/4, 20), ">>")) { if(selected < creaturesList.Count-1) selected++; else selected=0; }

					scroll1=GUI.BeginScrollView(new Rect(0, 110, sw*0.25f, (sh*0.75f)-110), scroll1, new Rect(0, 0, 0, 430), false, true);

					//AI on/off
					if(creature.useAI)
					{
						GUI.color=Color.gray; if(GUI.Button (new Rect(sw*0.25f/2, 0, sw*0.25f/2-20, 25), "Player")) creature.SetAI(false);
						GUI.color=Color.green; GUI.Box (new Rect(10, 0, sw*0.25f/2-10, 25), "A.I. : "+creature.behavior );
					}
					else
					{
						GUI.color=Color.green; GUI.Box (new Rect(sw*0.25f/2, 0, sw*0.25f/2-20, 25), "Player");
						GUI.color=Color.gray; if(GUI.Button (new Rect(10, 0, sw*0.25f/2-10, 25), "A.I." )) creature.SetAI(true);
					}

					GUI.color=Color.white;

					//Model materials
					int body= creature.bodyTexture.GetHashCode();
					int eyes= creature.eyesTexture.GetHashCode();
					if(GUI.Button (new Rect(10, 30, sw*0.25f-30, 25), "Body Skin : "+creature.bodyTexture))
					{ if(body<2) body++; else body=0; creature.SetMaterials(body, eyes); }
				
					if(GUI.Button (new Rect(10, 60, sw*0.25f-30, 25), "Eyes Skin : "+creature.eyesTexture))
					{ if(eyes<15)eyes++; else eyes=0; creature.SetMaterials(body, eyes); }

					//Model scale
					float Scale=creaturesList[selected].transform.localScale.x;
					GUI.Box(new Rect(10, 90, sw*0.25f-30, 25), "Scale : "+Mathf.Round(Scale*100)/100);
					Scale=GUI.HorizontalSlider(new Rect(10, 110, sw*0.25f-30, 25), creaturesList[selected].transform.localScale.x, 0.1f, 1.0f);
					if(Scale!=creaturesList[selected].transform.localScale.x) creaturesList[selected].SendMessage("SetScale", Mathf.Round(Scale*100)/100);
					//Animation speed
					GUI.Box(new Rect(10, 125, sw*0.25f-30, 25), "Animation Speed : "+Mathf.Round(creature.animSpeed*100)/100);
					creature.animSpeed=GUI.HorizontalSlider(new Rect(10, 145, sw*0.25f-30, 25), creature.animSpeed, 0.0f, 2.0f);
					//Health
					GUI.Box(new Rect(10, 160, sw*0.25f-30, 25), "Health : "+Mathf.Round(creature.health*10)/10);
					creature.health=GUI.HorizontalSlider(new Rect(10, 180, sw*0.25f-30, 25), creature.health,  0.0f, 100f);
					//Food
					GUI.Box(new Rect(10, 200, sw*0.25f-30, 20), "Food : "+Mathf.Round(creature.food*10)/10);
					creature.food=GUI.HorizontalSlider(new Rect(10, 220, sw*0.25f-30, 20), creature.food, 0.0f, 100f);
					//Water
					GUI.Box(new Rect(10, 240, sw*0.25f-30, 20), "Water : "+Mathf.Round(creature.water*10)/10);
					creature.water=GUI.HorizontalSlider(new Rect(10, 260, sw*0.25f-30, 20), creature.water,  0.0f, 100f);
					//Stamina
					GUI.Box(new Rect(10, 280, sw*0.25f-30, 20), "Stamina : "+Mathf.Round(creature.stamina*10)/10);
					creature.stamina=GUI.HorizontalSlider(new Rect(10, 300, sw*0.25f-30, 20), creature.stamina,  0.0f, 100f);
		
					//Damage
				  GUI.Box(new Rect(10, 320, sw*0.25f-30, 20), "Damages X"+Mathf.Round(creature.damageMultiplier*100)/100);
					creature.damageMultiplier=GUI.HorizontalSlider(new Rect(10, 340, sw*0.25f-30, 20), creature.damageMultiplier, 1, 10);
					//Armor
					GUI.Box(new Rect(10, 360, sw*0.25f-30, 20), "Armor X"+Mathf.Round(creature.armorMultiplier*100)/100);
					creature.armorMultiplier=GUI.HorizontalSlider(new Rect(10, 380, sw*0.25f-30, 20), creature.armorMultiplier, 1, 10);

					GUI.EndScrollView();

				}
			}
			else GUI.Box (new Rect(0, 50, sw*0.25f, (sh*0.75f)-50), "None", style);

			//Add new creature
			GUI.color=Color.yellow;
			if(addCreatureTab==-2)
			{
				if(GUI.Button (new Rect(0, sh*0.75f, sw*0.25f, 25), "")) addCreatureTab=-1;
				GUI.Box(new Rect(0, sh*0.75f, sw/4, sh/4), "Add a new creature", style);
			}
			else if(addCreatureTab==-1)
			{
				if(GUI.Button (new Rect(sw-25, 50, 25, 25), "X")) addCreatureTab=-2;
				GUI.Box(new Rect(0, sh*0.75f, sw/4, sh/4), "Spawn Settings", style);
				GUI.color=Color.white;
				scroll2=GUI.BeginScrollView(new Rect(0,  (sh*0.75f)+40, sw*0.25f, (sh*0.25f)-40), scroll2, new Rect(0, 0, 0, 130), false, true);
				//AI
				GUI.Box(new Rect(10, 0, sw*0.25f-30, 25), "");
				spawnAI= GUI.Toggle (new Rect(18, 0, 120, 25), spawnAI, " Spawn with AI ");
				//Random
				GUI.Box(new Rect(10, 30, sw*0.25f-30, 25), "");
				rndSkin= GUI.Toggle (new Rect(18, 30, 100, 25), rndSkin, " Random skin");
				//Random size
				GUI.Box(new Rect(10, 60, sw*0.25f-30, 25), "");
				rndSize= GUI.Toggle (new Rect(18, 60, 100, 25), rndSize, " Random size");
				if(rndSize)
				{
						if(GUI.Button(new Rect(130, 60, sw*0.25f-150, 25), "Span : "+rndSizeSpan.ToString()))
					{ if(rndSizeSpan<5) rndSizeSpan++; else rndSizeSpan=1; }
				}
				//Random setting
				GUI.Box(new Rect(10, 90, sw*0.25f-30, 25), "");
				rndSetting= GUI.Toggle (new Rect(18, 90, sw*0.25f-30, 25), rndSetting, " Random status settings");
				GUI.EndScrollView();
				GUI.Box(new Rect(sw/4, 50, sw*0.75f, sh-50), "Select a specie. "+collectionList.Count+" creature(s) available.", style);
        
        for(int i=0; i<collectionList.Count; i++)
        { 
          int maxline=1*((int)sh/36); int column=i/maxline; int line=i-(maxline*column);
          if(collectionList[i].GetComponent<Creature>().herbivorous) GUI.color=Color.green;
          else if(collectionList[i].GetComponent<Creature>().canFly) GUI.color=Color.yellow;
          else if(collectionList[i].GetComponent<Creature>().canSwim) GUI.color=Color.cyan;
          else GUI.color=new Color(1.0f, 0.6f, 0.0f);
          //Spawn
          if(GUI.Button (new Rect((sw/4)+15+(200*column), 100+(30*line), 180, 25), collectionList[i].name))
          {
						//Prevent spawning a creature under the ground
						Vector3 spawnPos=transform.position+transform.forward*10;
						if(t && (t.SampleHeight(spawnPos)+t.GetPosition().y)>spawnPos.y-1.0f)
						spawnPos=new Vector3(spawnPos.x, (t.SampleHeight(spawnPos)+t.GetPosition().y)+1.0f, spawnPos.z);
		
						//Spawn
            GameObject spawncreature=Instantiate(collectionList[i] ,spawnPos, Quaternion.identity);
					  Creature script=spawncreature.GetComponent<Creature>();

					  if(!spawnAI) cameraMode=1; script.useAI=spawnAI;
					  if(rndSkin) { script.SetMaterials(Random.Range(0, 3), Random.Range(0, 16)); }
					  if(rndSize) { script.SetScale( 0.5f+Random.Range((float) rndSizeSpan/-10, (float) rndSizeSpan/10)); } else script.SetScale(0.5f);
					  if(rndSetting)
					  {
						  script.health =100; script.stamina =Random.Range(0,100);
						  script.food =Random.Range(0,100); script.water =Random.Range(0,100);
					  }

						spawncreature.name=collectionList[i].name;
					  creaturesList.Add(spawncreature.gameObject); selected=creaturesList.IndexOf(spawncreature.gameObject); //add creature to creature list
          }
        }
        GUI.color=Color.white;
			}
			break;
			//Options
			case 2: 
			GUI.Box (new Rect(0, 50, sw, sh-50), "Options", style);
			//Screen
			GUI.Box(new Rect((sw/2)-225, (sh/2)-110, 150, 220), "Screen", style);
			bool fullScreen=Screen.fullScreen; fullScreen= GUI.Toggle (new Rect((sw/2)-220, (sh/2)-80, 140, 20), fullScreen, " Fullscreen");
			if(fullScreen!=Screen.fullScreen) Screen.fullScreen=!Screen.fullScreen;
			showFPS= GUI.Toggle (new Rect((sw/2)-220, (sh/2)-40, 140, 20), showFPS, " Show Fps");
			showGUI= GUI.Toggle (new Rect((sw/2)-220, (sh/2), 140, 20), showGUI, " Show GUI");
			//Controls
			GUI.Box(new Rect((sw/2)-75, (sh/2)-110, 150, 220), "Controls", style);
			invertYAxis=GUI.Toggle (new Rect((sw/2)-70, (sh/2)-80, 140, 20), invertYAxis, " Invert Y Axe");
			GUI.Label(new Rect((sw/2)-70, (sh/2)-40, 140, 20), "Sensivity");
			sensivity=GUI.HorizontalSlider(new Rect((sw/2)-70, (sh/2), 140, 20), sensivity, 0.1f, 10.0f);
			//Creatures
			GUI.Box(new Rect((sw/2)+75, (sh/2)-110, 150, 220), "Creatures", style);
			useIK= GUI.Toggle (new Rect((sw/2)+80, (sh/2)-80, 140, 20), useIK, " Use IK");
			useRaycast= GUI.Toggle (new Rect((sw/2)+80, (sh/2)-40, 140, 20), useRaycast, " Use Raycast");
			realtimeGame= GUI.Toggle (new Rect((sw/2)+80, (sh/2), 140, 20), realtimeGame, " Realtime Game");
			break;
			//Help
			case 3: GUI.Box (new Rect(0, 50, sw, sh-50), "Controls", style);	
				GUI.DrawTexture(new Rect(0, 50, sw, sh-100), helpscreen); 
			break;
			}
		} else Cursor.visible=false;


		if(creature)
		{
			if(showGUI)
			{
				// Health bar
				if(cameraMode==1)
				{
					Rect ico1=new Rect(0, 0.5f, 0.5f, 0.5f), ico2=new Rect(0.5f, 0.5f, 0.5f, 0.5f), ico3=new Rect(0.5f, 0, 0.5f, 0.5f), ico4 =new Rect(0, 0, 0.5f, 0.5f), bar=new Rect(0, 0, 0.1f, 0.1f);
					GUI.color=Color.white; //Icons
					GUI.DrawTextureWithTexCoords(new Rect(sw/4, sh/1.1f, sw/48, sw/48), icons, ico1, true);  //health icon
					GUI.DrawTextureWithTexCoords(new Rect(sw/2, sh/1.1f, sw/48, sw/48), icons, ico2, true); //food icon
				  GUI.DrawTextureWithTexCoords(new Rect(sw/2, sh/1.05f, sw/48, sw/48), icons, ico3, true); //water icon
					GUI.DrawTextureWithTexCoords(new Rect(sw/4, sh/1.05f, sw/48, sw/48), icons, ico4, true); //sleep icon

					GUI.color=Color.black; //bar background
					GUI.DrawTextureWithTexCoords(new Rect(sw/3.5f, sh/1.09f, (sw*0.002f)*100, sh/100), icons, bar, false);
					GUI.DrawTextureWithTexCoords(new Rect(sw/1.85f, sh/1.09f, (sw*0.002f)* 100, sh/100), icons, bar, false);
				  GUI.DrawTextureWithTexCoords(new Rect(sw/1.85f, sh/1.04f, (sw*0.002f)*100, sh/100), icons, bar, false); 
					GUI.DrawTextureWithTexCoords(new Rect(sw/3.5f, sh/1.04f, (sw*0.002f)*100, sh/100), icons, bar, false);

          //health bar
					if((creature.food==0.0f | creature.stamina==0.0f | creature.water==0.0f) && creature.loop<=25) GUI.color=Color.red; else GUI.color=Color.green;
					GUI.DrawTextureWithTexCoords(new Rect(sw/3.5f, sh/1.09f, (sw*0.002f)*creature.health, sh/100), icons, bar, false);
          //food bar
					if(creature.food<25f && creature.loop<=25) GUI.color=Color.red; else GUI.color=Color.yellow; 
					GUI.DrawTextureWithTexCoords(new Rect(sw/1.85f, sh/1.09f, (sw*0.002f)*creature.food, sh/100), icons, bar, false);
          //water bar
					if(creature.water<25f && creature.loop<=25) GUI.color=Color.red; else GUI.color=Color.cyan;
					GUI.DrawTextureWithTexCoords(new Rect(sw/1.85f, sh/1.04f, (sw*0.002f)*creature.water, sh/100), icons, bar, false);
          //sleep bar
					if(creature.stamina<25f && creature.loop<=25) GUI.color=Color.red; else GUI.color=Color.gray;
					GUI.DrawTextureWithTexCoords(new Rect(sw/3.5f, sh/1.04f, (sw*0.002f)*creature.stamina, sh/100), icons, bar, false);
				}
			}
		}

		//Fps
		GUI.color=Color.white;
		if(showFPS) GUI.Label(new Rect(sw-60, 1, 55, 20), "Fps : "+ fps);

		//Messages
		if(message!=0)
		{
			count++;
			if(message==1) GUI.Box(new Rect((sw/2)-120, sh/2, 240, 25), "Nothing to eat or drink...", style);
      else if(message==2)
      {
        GUI.color=Color.yellow;
        GUI.Box(new Rect((sw/2)-140, sh/2, 280, 25), "AI/IK Script Extension Asset Required", style);
        GUI.color=Color.white;
        if(GUI.Button (new Rect((sw/2)-140, sh/2+25, 280, 25), "Get it : www.assetstore.unity3d.com"))
        Application.OpenURL("https://assetstore.unity.com/packages/3d/characters/animals/jp-script-extension-94813");
      }
			if(count>512) { count=0; message=0; }
		}
	}
  #endregion
}
