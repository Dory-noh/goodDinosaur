using UnityEngine;

public class Dime : Creature
{
	public Transform Spine0,Spine1,Spine2,Spine3,Neck0,Neck1,Neck2,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Tail6,Tail7,Tail8, 
	Left_Arm0,Right_Arm0,Left_Arm1,Right_Arm1,Left_Hand,Right_Hand,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot,Right_Foot;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Medstep,Medsplash,Sniff2,Bite,Swallow,Largestep,Largesplash,Idlecarn,Dime1,Dime2,Dime3,Dime4;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Dime1; break; case 1: painSnd=Dime2; break; case 2: painSnd=Dime3; break; case 3: painSnd=Dime4; break; }
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
			case "Bite": source[1].pitch=Random.Range(0.75f, 1.0f); source[1].PlayOneShot(Bite, 0.5f);
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Largestep, 1.0f);
				lastframe=currframe; isDead=true; break;
			case "Food": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Swallow, 0.75f);
				lastframe=currframe; break;
			case "Sniff": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Sniff2, 0.5f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Idlecarn, 0.25f);
				lastframe=currframe; break;
			case "Atk": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Dime2, 1.0f);
				lastframe=currframe; break;
			case "Growl": int rnd2=Random.Range (0, 3); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd2==0)source[0].PlayOneShot(Dime1, 1.0f);
				if(rnd2==1)source[0].PlayOneShot(Dime3, 1.0f);
				else source[0].PlayOneShot(Dime4, 1.0f);
				lastframe=currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		StatusUpdate(); if(!isActive | animSpeed==0.0f) { body.Sleep(); return; }
    onReset=false; onAttack=false; isConstrained= false;
		
		if(useAI && health!=0) { AICore(1, 2, 3, 0, 4, 5, 6); } // CPU
		else if(health!=0) { GetUserInputs(1, 2, 3, 0, 4, 5, 6); } // Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); } //Dead

    //Set Y position
    if(isOnGround | isInWater | isOnWater)
    {
      if(!isOnGround && !isInWater) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos();
    } else ApplyGravity();

		//Stopped
		if(OnAnm.IsName("Dime|Idle1A") | OnAnm.IsName("Dime|Idle2A") |
       OnAnm.IsName("Dime|Die1") | OnAnm.IsName("Dime|Die2") )
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Dime|Die1")) { onReset=true; if(!isDead) { PlaySound("Atk", 1); PlaySound("Die", 12); } }
			else if(OnAnm.IsName("Dime|Die2")) { onReset=true; if(!isDead) { PlaySound("Atk", 1); PlaySound("Die", 12); } }
		}

		//Forward
		else if(OnAnm.IsName("Dime|Walk") | OnAnm.IsName("Dime|WalkGrowl") |
		       (OnAnm.IsName("Dime|Step1") && OnAnm.normalizedTime < 0.7) | (OnAnm.IsName("Dime|Step2") && OnAnm.normalizedTime < 0.7) |
           (OnAnm.IsName("Dime|StepAtk1") && OnAnm.normalizedTime < 0.7) | (OnAnm.IsName("Dime|StepAtk2") && OnAnm.normalizedTime < 0.7) |
           (OnAnm.IsName("Dime|ToIdle1C") && OnAnm.normalizedTime < 0.7))
		{
			Move(transform.forward, 18);
			if(OnAnm.IsName("Dime|WalkGrowl")) { PlaySound("Growl", 2); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Dime|Walk")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Dime|StepAtk1") | OnAnm.IsName("Dime|StepAtk2"))
			{ onAttack=true; PlaySound("Atk", 2); PlaySound("Bite", 4); } else PlaySound("Step", 8);
		}

		//Running
		else if(OnAnm.IsName("Dime|Run") | OnAnm.IsName("Dime|RunGrowl") | OnAnm.IsName("Dime|WalkAtk"))
		{
			Move(transform.forward, 60);
			if(OnAnm.IsName("Dime|WalkAtk")) { onAttack=true; PlaySound("Atk", 2); PlaySound("Bite", 4); }
			else if(OnAnm.IsName("Dime|RunGrowl")) { PlaySound("Growl", 2); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Dime|Run")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else PlaySound("Step", 8);
		}
		
		//Backward
		else if(OnAnm.IsName("Dime|Step1-") | OnAnm.IsName("Dime|Step2-") | OnAnm.IsName("Dime|ToSleep2") | OnAnm.IsName("Dime|ToIdle2C") |
		        OnAnm.IsName("Dime|ToEatA") | OnAnm.IsName("Dime|ToEatC"))
		{
			Move(-transform.forward, 15);
			PlaySound("Step", 8);
		}

		//Strafe/Turn right
		else if(OnAnm.IsName("Dime|Strafe1-") | OnAnm.IsName("Dime|Strafe2+"))
		{
			Move(transform.right, 8);
			PlaySound("Step", 6); PlaySound("Step", 13);
		}

		//Strafe/Turn left
		else if(OnAnm.IsName("Dime|Strafe1+") | OnAnm.IsName("Dime|Strafe2-"))
		{
			Move(-transform.right, 8);
			PlaySound("Step", 6); PlaySound("Step", 13);
		}

		//Various
		else if(OnAnm.IsName("Dime|EatA")) { onReset=true; isConstrained=true; PlaySound("Food", 4); }
		else if(OnAnm.IsName("Dime|EatB") | OnAnm.IsName("Dime|EatC")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Dime|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 2); }
		else if(OnAnm.IsName("Dime|ToSleep1") | OnAnm.IsName("Dime|ToSleep2")) onReset=true;
		else if(OnAnm.IsName("Dime|ToSleep-")) { isConstrained=true; PlaySound("Sniff", 2); }
		else if(OnAnm.IsName("Dime|Idle1B")) PlaySound("Growl", 1);
		else if(OnAnm.IsName("Dime|Idle1C")) { PlaySound("Sniff", 4); PlaySound("Sniff", 7); PlaySound("Sniff", 10);}
		else if(OnAnm.IsName("Dime|Idle2B")) PlaySound("Growl", 1);
		else if(OnAnm.IsName("Dime|Idle2C")) { onReset=true; PlaySound("Sniff", 1); }
		else if(OnAnm.IsName("Dime|Die1-")) { isConstrained=true; PlaySound("Growl", 3);  isDead=false;}
		else if(OnAnm.IsName("Dime|Die2-")) { isConstrained=true; PlaySound("Growl", 3);  isDead=false; }

    RotateBone(IkType.Convex, 48f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
		if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;
		Neck0.rotation*= Quaternion.Euler(0, -headY, -headX);
		Neck1.rotation*= Quaternion.Euler(0, -headY, -headX);
		Neck2.rotation*= Quaternion.Euler(0, -headY, -headX);
		Head.rotation*= Quaternion.Euler(0, -headY, -headX);
    Spine0.rotation*= Quaternion.Euler(0, 0, -spineX);
		Spine1.rotation*= Quaternion.Euler(0, 0, -spineX);
		Spine2.rotation*= Quaternion.Euler(0, 0, -spineX);
		Spine3.rotation*= Quaternion.Euler(0, 0, -spineX);
		Tail0.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail1.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail2.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail3.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail4.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail5.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail6.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail7.rotation*= Quaternion.Euler(0, 0, spineX);
		Tail8.rotation*= Quaternion.Euler(0, 0, spineX);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(0, lastHit, 0);
		//Check for ground layer
		GetGroundPos(IkType.Convex, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, Right_Arm0, Right_Arm1, Right_Hand, Left_Arm0, Left_Arm1, Left_Hand);
	}
}



