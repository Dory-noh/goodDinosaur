using UnityEngine;

public class Brach : Creature
{
	public Transform Spine0,Spine1,Spine2,Spine3,Spine4,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Tail6,Tail7,Tail8, 
	Neck0,Neck1,Neck2,Neck3,Neck4,Neck5,Neck6,Neck7,Neck8,Neck9,Neck10,Neck11,Neck12,Neck13,Neck14,Neck15,Neck16, 
	Left_Arm0,Right_Arm0,Left_Arm1,Right_Arm1,Left_Hand,Right_Hand,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot,Right_Foot;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Largestep,Largesplash,Idleherb,Chew,Brach1,Brach2,Brach3,Brach4;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Brach1; break; case 1: painSnd=Brach2; break; case 2: painSnd=Brach3; break; case 3: painSnd=Brach4; break; }
		ManageCollision(col, source, painSnd, Hit_jaw, Hit_head, Hit_tail);
	}
	void PlaySound(string name, int time)
	{
		if(time==currframe && lastframe!=currframe)
		{
			switch (name)
			{
			case "Step": source[1].pitch=Random.Range(0.75f, 1.25f);
				if(isInWater) source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
				else if(isOnWater) source[1].PlayOneShot(Largesplash, Random.Range(0.25f, 0.5f));
				else if(isOnGround) source[1].PlayOneShot(Largestep, Random.Range(0.25f, 0.5f));
				lastframe=currframe; break;
			case "Hit": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Largestep, 1.5f);
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Largestep, 1.0f);
				lastframe=currframe; isDead=true; break;
			case "Chew": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Chew, 0.75f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Idleherb, 0.25f);
				lastframe=currframe; break;
			case "Growl": int rnd=Random.Range (0, 4); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd==0)source[0].PlayOneShot(Brach1, 1.0f);
				else if(rnd==1)source[0].PlayOneShot(Brach2, 1.0f);
				else if(rnd==2)source[0].PlayOneShot(Brach3, 1.0f);
				else if(rnd==3)source[0].PlayOneShot(Brach4, 1.0f);
				lastframe=currframe; break;
			}
		}
	}
	
	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		StatusUpdate(); if(!isActive | animSpeed==0.0f) { body.Sleep(); return; }
		onReset=false; isConstrained=false;

		if(useAI && health!=0) { AICore(1, 4, 0, 0, 2, 3, 5); }// CPU
		else if(health!=0) { GetUserInputs(1, 4, 0, 0, 2, 3, 5, 4); }// Human
		else { anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
    if(isOnGround | isInWater | isOnWater)
    {
      if(!isOnGround && !isInWater) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos();
    } else ApplyGravity();

		//Stopped
		if(OnAnm.IsName("Brach|IdleA") | OnAnm.IsName("Brach|Die"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Brach|Die")) { onReset=true; if(!isDead) { PlaySound("Growl", 3); PlaySound("Die", 12); } }
		}
	
		//Forward
		else if(OnAnm.IsName("Brach|Walk") | OnAnm.IsName("Brach|WalkGrowl"))
		{
      Move(transform.forward, 15);
			if(OnAnm.IsName("Brach|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 5); PlaySound("Step", 12); }
			else { PlaySound("Step", 5); PlaySound("Step", 12); }
		}

		//Run
		else if(OnAnm.IsName("Brach|Run") | OnAnm.IsName("Brach|RunGrowl"))
		{
			Move(transform.forward, 30);
			if(OnAnm.IsName("Brach|RunGrowl")) { PlaySound("Growl", 2); PlaySound("Step", 5); PlaySound("Step", 12); }
			else { PlaySound("Step", 5); PlaySound("Step", 12); }
		}

		//Backward
		else if(OnAnm.IsName("Brach|Walk-") | OnAnm.IsName("Brach|WalkGrowl-"))
		{
			Move(-transform.forward, 15);
			if(OnAnm.IsName("Brach|WalkGrowl-")) { PlaySound("Growl", 4); PlaySound("Step", 5); PlaySound("Step", 12); }
			else { PlaySound("Step", 5); PlaySound("Step", 12); }
		}

		//Strafe/Turn right
		else if(OnAnm.IsName("Brach|Strafe-"))
		{
			Move(transform.right, 5);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Strafe/Turn left
		else if(OnAnm.IsName("Brach|Strafe+"))
		{
			Move(-transform.right, 5);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Various
		else if(OnAnm.IsName("Brach|EatA")) PlaySound("Chew", 10);
		else if(OnAnm.IsName("Brach|EatB")) onReset=true;
		else if(OnAnm.IsName("Brach|EatC")) { onReset=true; PlaySound("Chew", 1); PlaySound("Chew", 4); PlaySound("Chew", 8); PlaySound("Chew", 12); }
		else if(OnAnm.IsName("Brach|ToSit")) isConstrained=true;
		else if(OnAnm.IsName("Brach|ToSit-")) isConstrained=true;
		else if(OnAnm.IsName("Brach|SitIdle")) isConstrained=true;
		else if(OnAnm.IsName("Brach|Sleep") | OnAnm.IsName("Brach|ToSleep") ) { onReset=true; isConstrained=true; PlaySound("Repose", 2); }
		else if(OnAnm.IsName("Brach|SitGrowl")) { PlaySound("Growl", 7); isConstrained=true; }
		else if(OnAnm.IsName("Brach|IdleB")) PlaySound("Growl", 2);
		else if(OnAnm.IsName("Brach|RiseIdle")) onReset=true;
		else if(OnAnm.IsName("Brach|RiseGrowl")) { onReset=true; PlaySound("Growl", 2); }
		else if(OnAnm.IsName("Brach|ToRise")) { onReset=true; PlaySound("Growl", 5); }
		else if(OnAnm.IsName("Brach|ToRise-")) { onReset=true; PlaySound("Growl", 1); PlaySound("Hit", 4);}
		else if(OnAnm.IsName("Brach|Die-")) { PlaySound("Growl", 3);  isDead=false; }

		RotateBone(IkType.Quad, 40f, 0.0f, true, 0.25f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
    if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;
		float headZ =-headY*headX/yaw_Max;
    Spine0.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine1.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine2.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine3.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine4.rotation*= Quaternion.Euler(0, 0, spineX);
		Neck0.rotation*= Quaternion.Euler(0, 0, headX);
		Neck1.rotation*= Quaternion.Euler(0, 0, headX);
		Neck2.rotation*= Quaternion.Euler(0, 0, headX);
		Neck3.rotation*= Quaternion.Euler(0, 0, headX);
		Neck4.rotation*= Quaternion.Euler(0, 0, headX);
		Neck5.rotation*= Quaternion.Euler(0, 0, headX);
		Neck6.rotation*= Quaternion.Euler(0, 0, headX);
		Neck7.rotation*= Quaternion.Euler(0, 0, headX);
		Neck8.rotation*= Quaternion.Euler(headY, 0, 0);
		Neck9.rotation*= Quaternion.Euler(headY, 0, 0);
		Neck10.rotation*= Quaternion.Euler(headY, 0, 0);
		Neck11.rotation*= Quaternion.Euler(headY, 0, 0);
		Neck12.rotation*= Quaternion.Euler(headY, 0, 0);
		Neck13.rotation*= Quaternion.Euler(headY, 0, 0);
		Neck14.rotation*= Quaternion.Euler(headY, 0, 0);
		Neck15.rotation*= Quaternion.Euler(headY, headZ, headX);
		Neck16.rotation*= Quaternion.Euler(headY, headZ, headX);
		Head.rotation*= Quaternion.Euler(headY, headZ, headX);
		Tail0.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail1.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail2.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail3.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail4.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail5.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail6.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail7.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail8.rotation*= Quaternion.Euler(0, 0, -spineX);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(-lastHit, 0, 0);

		//Check for ground layer
		GetGroundPos(IkType.Quad, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, Right_Arm0, Right_Arm1, Right_Hand, Left_Arm0, Left_Arm1, Left_Hand, -0.6f*size);
	}
}