using UnityEngine;

public class Para : Creature
{
	public Transform Spine0,Spine1,Spine2,Spine3,Spine4,Neck0,Neck1,Neck2,Neck3,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Tail6,Tail7,Tail8, 
	Left_Arm0,Right_Arm0,Left_Arm1,Right_Arm1,Left_Hand,Right_Hand,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot,Right_Foot;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Medstep,Medsplash,Sniff2,Chew,Largestep,Largesplash,Idleherb,Para1,Para2,Para3,Para4;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Para1; break; case 1: painSnd=Para2; break; case 2: painSnd=Para3; break; case 3: painSnd=Para4; break; }
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
				else if(isOnWater) source[1].PlayOneShot(Medsplash, Random.Range(0.25f, 0.5f));
				else if(isOnGround) source[1].PlayOneShot(Medstep, Random.Range(0.25f, 0.5f));
				lastframe=currframe; break;
			case "Hit": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Largestep, 1.0f);
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Largestep, 1.0f);
				lastframe=currframe; isDead=true; break;
			case "Sniff": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Sniff2, 0.5f);
				lastframe=currframe; break;
			case "Chew": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Chew, 0.5f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Idleherb, 0.25f);
				lastframe=currframe; break;
			case "Growl": int rnd1=Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd1==0)source[0].PlayOneShot(Para1, 1.0f);
				else source[0].PlayOneShot(Para2, 1.0f);
				lastframe=currframe; break;
			case "Call": int rnd2=Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd2==0)source[0].PlayOneShot(Para3, 1.0f);
				else source[0].PlayOneShot(Para4, 1.0f);
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

		if(useAI && health!=0) { AICore(1, 2, 3, 4, 5, 6, 7); } // CPU
		else if(health!=0) { GetUserInputs(1, 2, 3, 4, 5, 6, 7, 4); }// Human
		else { anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
    if(isOnGround | isInWater | isOnWater)
    {
      if(!isOnGround && !isInWater) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos();
    } else ApplyGravity();

		//Stopped
		if(OnAnm.IsName("Para|Idle1A") | OnAnm.IsName("Para|Idle2A") |
			 OnAnm.IsName("Para|Die1") | OnAnm.IsName("Para|Die2"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Para|Die1")){ onReset=true; if(!isDead) { PlaySound("Growl", 2); PlaySound("Die", 12); }}
			else if(OnAnm.IsName("Para|Die2")){ onReset=true; if(!isDead) { PlaySound("Growl", 2); PlaySound("Die", 10); } }
		}
		
		//Forward
		else if(OnAnm.IsName("Para|Walk") | OnAnm.IsName("Para|WalkGrowl") | OnAnm.IsName("Para|Step1") | OnAnm.IsName("Para|Step2") |
		   OnAnm.IsName("Para|ToEatA") | OnAnm.IsName("Para|ToEatC") | OnAnm.IsName("Para|ToIdle1D"))
		{
      if(!(OnAnm.IsName("Para|Step1")|(OnAnm.IsName("Para|Step2")) && OnAnm.normalizedTime > 0.8))
      Move(transform.forward, 15);
			if(OnAnm.IsName("Para|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Para|Walk")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else PlaySound("Step", 9);
		}

		//Running
		else if(OnAnm.IsName("Para|Run") | OnAnm.IsName("Para|RunGrowl"))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*10.0f, 0.1f), -20f, 20f);
			Move(transform.forward, 80);
			if(OnAnm.IsName("Para|RunGrowl")) { PlaySound("Growl", 2); PlaySound("Step", 5); PlaySound("Step", 12); }
			else { PlaySound("Step", 5); PlaySound("Step", 12);}
		}
		
		//Backward
		else if(OnAnm.IsName("Para|Step1-") | OnAnm.IsName("Para|Step2-") | OnAnm.IsName("Para|ToSit1"))
		{
			Move(-transform.forward, 15);
			PlaySound("Step", 9);
		}
		
		//Strafe/Turn right
		else if(OnAnm.IsName("Para|Strafe1+") | OnAnm.IsName("Para|Strafe2+"))
		{
			Move(transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}
		
		//Strafe/Turn left
		else if(OnAnm.IsName("Para|Strafe1-") |OnAnm.IsName("Para|Strafe2-"))
		{
			Move(-transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Various
		else if(OnAnm.IsName("Para|EatA")) PlaySound("Chew", 10);
		else if(OnAnm.IsName("Para|EatB")) { PlaySound("Chew", 1); PlaySound("Chew", 4); PlaySound("Chew", 8); PlaySound("Chew", 12); }
		else if(OnAnm.IsName("Para|EatC")) onReset=true;
		else if(OnAnm.IsName("Para|ToSit")) isConstrained=true;
		else if(OnAnm.IsName("Para|SitIdle")) isConstrained=true;
		else if(OnAnm.IsName("Para|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 2); }
		else if(OnAnm.IsName("Para|SitGrowl")) { isConstrained=true; PlaySound("Growl", 2); }
		else if(OnAnm.IsName("Para|Idle1B")) PlaySound("Growl", 2);
		else if(OnAnm.IsName("Para|Idle1C")) PlaySound("Call", 1);
		else if(OnAnm.IsName("Para|Idle1D")) { onReset=true; PlaySound("Sniff", 1); }
		else if(OnAnm.IsName("Para|Idle2B")) PlaySound("Growl", 2);
		else if(OnAnm.IsName("Para|Idle2C")) PlaySound("Call", 2);
		else if(OnAnm.IsName("Para|ToRise1") | OnAnm.IsName("Para|ToRise2")) { onReset=true; PlaySound("Sniff", 3); PlaySound("Growl", 1); }
		else if(OnAnm.IsName("Para|ToRise1-") | OnAnm.IsName("Para|ToRise2-")) { onReset=true; PlaySound("Hit", 7); }
		else if(OnAnm.IsName("Para|Rise1Growl")) { onReset=true; PlaySound("Call", 1); }
		else if(OnAnm.IsName("Para|Rise2Growl")) { onReset=true; PlaySound("Growl", 1); }
    else if(OnAnm.IsName("Para|Rise1Idle") | OnAnm.IsName("Para|Rise2Idle")) onReset=true;
		else if(OnAnm.IsName("Para|Die1-") | OnAnm.IsName("Para|Die2-")) { PlaySound("Growl", 3); isDead=false; }

    RotateBone(IkType.Quad, 50f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
    if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;
    Spine0.rotation*= Quaternion.Euler(0, 0, spineX);
		Spine1.rotation*= Quaternion.Euler(0, 0, spineX);
		Spine2.rotation*= Quaternion.Euler(0, 0, spineX);
		Spine3.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine4.rotation*= Quaternion.Euler(0, 0, spineX);
    Neck0.rotation*= Quaternion.Euler(0, headX, headX);
		Neck1.rotation*= Quaternion.Euler(0, headX, headX);
		Neck2.rotation*= Quaternion.Euler(headY, headX, headX);
		Neck3.rotation*= Quaternion.Euler(headY*1.5f, headX, headX);
		Head.rotation*= Quaternion.Euler(headY*2.0f, headX, headX);
		Tail0.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail1.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail2.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail3.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail4.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail5.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail6.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail7.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail8.rotation*= Quaternion.Euler(0, 0, -spineX);
		Right_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Right_Arm0.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Arm1.rotation*= Quaternion.Euler(0, roll, 0);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(-lastHit, 0, 0);
		//Check for ground layer
		GetGroundPos(IkType.Quad, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, Right_Arm0, Right_Arm1, Right_Hand, Left_Arm0, Left_Arm1, Left_Hand, -0.5f*size);
	}
}
