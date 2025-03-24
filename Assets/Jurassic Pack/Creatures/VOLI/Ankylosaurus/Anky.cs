using UnityEngine;

public class Anky : Creature
{
  public Transform Spine0,Spine1,Spine2,Spine3,Spine4,Neck0,Neck1,Neck2,Neck3,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5, 
	Left_Arm0,Right_Arm0,Left_Arm1,Right_Arm1,Left_Hand,Right_Hand,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot,Right_Foot;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Medstep,Medsplash,Idleherb,Sniff1,Chew,Largestep,Largesplash,Anky1,Anky2,Anky3,Anky4;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Anky1; break; case 1: painSnd=Anky2; break; case 2: painSnd=Anky3; break; case 3: painSnd=Anky4; break; }
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
			case "Die": source[0].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Largestep, 1.0f);
							lastframe=currframe; isDead=true; break;
			case "Sniff": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Sniff1, 0.5f);
							lastframe=currframe; break;
			case "Chew": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Chew, 0.5f);
							lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Idleherb, 0.25f);
							lastframe=currframe; break;
			case "Growl": int rnd=Random.Range (0, 4); source[0].pitch=Random.Range(1.0f, 1.25f);
						 if(rnd==0)source[0].PlayOneShot(Anky1, 1.0f);
						 else if(rnd==1)source[0].PlayOneShot(Anky2, 1.0f);
						 else if(rnd==2)source[0].PlayOneShot(Anky3, 1.0f);
						 else if(rnd==3)source[0].PlayOneShot(Anky4, 1.0f);
						 lastframe=currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate()
	{
		StatusUpdate(); if(!isActive | animSpeed==0.0f) { body.Sleep(); return; }
    anm.SetInteger("Delta", (int)delta);
		onReset=false; onAttack=false; onTailAttack=false; isConstrained= false;

		if(useAI && health!=0) { AICore(1, 2, 3, 0, 4, 5, 6); }// CPU
		else if(health!=0) { GetUserInputs(1, 2, 3, 0, 4, 5, 6); }// Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
    if(isOnGround | isInWater | isOnWater)
    {
      if(!isOnGround && !isInWater) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos();
    } else ApplyGravity();

		//Stopped
		if(OnAnm.IsName("Anky|Idle1A") | OnAnm.IsName("Anky|Idle2A") |
			OnAnm.IsName("Anky|Die1") | OnAnm.IsName("Anky|Die2"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Anky|Die1")) { onReset=true; if(!isDead) { PlaySound("Growl", 2); PlaySound("Die", 12); } }
			else if(OnAnm.IsName("Anky|Die2")) { onReset=true; if(!isDead) { PlaySound("Growl", 2); PlaySound("Die", 10); } }
		}

		//Forward
		if(OnAnm.IsName("Anky|Walk") | OnAnm.IsName("Anky|WalkGrowl") | OnAnm.IsName("Anky|Step1") |
			 OnAnm.IsName("Anky|Step2") | OnAnm.IsName("Anky|ToIdle2C") | OnAnm.IsName("Anky|ToEatA") |
			(OnAnm.IsName("Anky|ToEatC") && OnAnm.normalizedTime < 0.9))
		{
      if(!(OnAnm.IsName("Anky|Step1")|(OnAnm.IsName("Anky|Step2")) && OnAnm.normalizedTime > 0.8))
      Move(transform.forward, 15);
			if(OnAnm.IsName("Anky|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Anky|Walk")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else PlaySound("Step", 9);
		}

		//Running
		else if(OnAnm.IsName("Anky|Run") | OnAnm.IsName("Anky|RunGrowl"))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*10.0f, 0.1f), -20f, 20f);
			Move(transform.forward, 60);
			if(OnAnm.IsName("Anky|Run")) { PlaySound("Step", 3); PlaySound("Step", 9); }
			else { PlaySound("Growl", 2); PlaySound("Step", 3); PlaySound("Step", 9); }
		}
		
		//Backward
		else if(OnAnm.IsName("Anky|Step1-") | OnAnm.IsName("Anky|Step2-") | OnAnm.IsName("Anky|ToIdle1C") | OnAnm.IsName("Anky|ToSit1"))
		{
			Move(-transform.forward, 15);
			PlaySound("Step", 9);
		}

		//Strafe/Turn right
		else if(OnAnm.IsName("Anky|Strafe1-") | OnAnm.IsName("Anky|Strafe2+"))
		{
			Move(transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Strafe/Turn left
		else if(OnAnm.IsName("Anky|Strafe1+") | OnAnm.IsName("Anky|Strafe2-"))
		{
			Move(-transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

    //Attack Idle
    else if(OnAnm.IsName("Anky|AtkIdle") | OnAnm.IsName("Anky|AtkA") | OnAnm.IsName("Anky|AtkGrowl"))
    {
      onTailAttack=true; Move(Vector3.zero);
      if(OnAnm.IsName("Anky|AtkGrowl")) { onAttack=true; PlaySound("Growl", 2); PlaySound("Hit", 7); }
      else if(OnAnm.IsName("Anky|AtkA")) { onAttack=true; PlaySound("Growl", 2); PlaySound("Sniff", 3); }
    }

		//Attack 
		else if(OnAnm.IsName("Anky|AtkB-") | OnAnm.IsName("Anky|AtkB+"))
		{
      onTailAttack=true; Move(Vector3.zero);
			if(OnAnm.normalizedTime < 0.9)
			{
        
				if(OnAnm.IsName("Anky|AtkB-")) transform.rotation*= Quaternion.Euler(0, Mathf.Lerp(0, -10.0f, 0.5f),0);
				else if(OnAnm.IsName("Anky|AtkB+")) transform.rotation*= Quaternion.Euler(0, Mathf.Lerp(0, 10.0f, 0.5f), 0);
        onAttack=true; anm.SetFloat("Turn", transform.eulerAngles.y);
			}
			PlaySound("Hit", 8); PlaySound("Hit", 10); 
			PlaySound("Sniff", 3); PlaySound("Growl", 2);
		}
		
		//Various
		if(OnAnm.IsName("Anky|EatA")) PlaySound("Chew", 10);
		else if(OnAnm.IsName("Anky|EatB")) { PlaySound("Chew", 1); PlaySound("Chew", 4); PlaySound("Chew", 8); PlaySound("Chew", 12); }
		else if(OnAnm.IsName("Anky|EatC")) onReset=true;
		else if(OnAnm.IsName("Anky|ToSit")) isConstrained=true;
		else if(OnAnm.IsName("Anky|SitIdle")) isConstrained=true; 
		else if(OnAnm.IsName("Anky|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 2); }
		else if(OnAnm.IsName("Anky|SitGrowl")) { isConstrained=true; PlaySound("Growl", 2); PlaySound("Step", 8); }
		else if(OnAnm.IsName("Anky|Idle1B")) PlaySound("Growl", 2); 
		else if(OnAnm.IsName("Anky|Idle1C")) PlaySound("Growl", 2);
		else if(OnAnm.IsName("Anky|Idle2B")) PlaySound("Growl", 2);
		else if(OnAnm.IsName("Anky|Idle2C")) { onReset=true; PlaySound("Sniff", 1); }
		else if(OnAnm.IsName("Anky|Die1-")) { PlaySound("Growl", 3);  isDead=false; }
		else if(OnAnm.IsName("Anky|Die2-")) { PlaySound("Growl", 3);  isDead=false; }

    RotateBone(IkType.Quad, 40f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
		if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;
		float headZ =headY*headX/yaw_Max;
    Spine0.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine1.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine2.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine3.rotation*= Quaternion.Euler(0, 0, spineX);
    Spine4.rotation*= Quaternion.Euler(0, 0, spineX);
		Neck0.rotation*= Quaternion.Euler(headY, headZ, headX);
		Neck1.rotation*= Quaternion.Euler(headY, headZ, headX);
		Neck2.rotation*= Quaternion.Euler(headY, headZ, headX);
		Neck3.rotation*= Quaternion.Euler(headY, headZ, headX);
		Head.rotation*= Quaternion.Euler(headY, headZ, headX);
    float spineZ =spineY*spineX/yaw_Max;
		Tail0.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		Tail1.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		Tail2.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		Tail3.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		Tail4.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		Tail5.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		roll=Mathf.Lerp(roll, 0.0f, ang_T);
		Right_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Right_Arm0.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Arm1.rotation*= Quaternion.Euler(0, roll, 0);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(-lastHit, 0, 0);
		//Check for ground layer
		GetGroundPos(IkType.Quad, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, Right_Arm0, Right_Arm1, Right_Hand, Left_Arm0, Left_Arm1, Left_Hand, -0.4f*size);
	}
}




