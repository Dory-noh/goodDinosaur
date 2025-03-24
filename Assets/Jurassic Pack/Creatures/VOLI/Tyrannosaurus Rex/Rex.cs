using UnityEngine;

public class Rex : Creature
{
	public Transform Spine0,Spine1,Spine2,Neck0,Neck1,Neck2,Tail2,Tail3,Tail4,Tail5,Tail6,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot0,Right_Foot0;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Bigstep,Largesplash,Largestep,Idlecarn,Bite,Swallow,Sniff1,Rex1,Rex2,Rex3,Rex4,Rex5;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 3); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Rex2; break; case 1: painSnd=Rex3; break; case 2: painSnd=Rex4; break; }
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
				else if(isOnGround) source[1].PlayOneShot(Bigstep, Random.Range(0.25f, 0.5f));
				lastframe=currframe; break;
			case "Bite": source[1].pitch=Random.Range(0.5f, 0.75f); source[1].PlayOneShot(Bite, 2.0f);
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Largestep, 1.0f);
				lastframe=currframe; isDead=true; break; 
			case "Food": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Swallow, 0.5f);
				lastframe=currframe; break;
			case "Sniff": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Sniff1, 0.5f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(0.75f, 1.25f); source[0].PlayOneShot(Idlecarn, 0.25f);
				lastframe=currframe; break;
			case "Atk": int rnd1=Random.Range (0, 2); source[0].pitch=Random.Range(0.75f, 1.75f);
				if(rnd1==0) source[0].PlayOneShot(Rex3, 0.5f);
				else source[0].PlayOneShot(Rex4,0.5f);
				lastframe=currframe; break;
			case "Growl": int rnd2=Random.Range (0, 3); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd2==0)source[0].PlayOneShot(Rex1, 1.0f);
				else if(rnd2==1) source[0].PlayOneShot(Rex2, 1.0f);
				else source[0].PlayOneShot(Rex5, 1.0f);
				lastframe=currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate() 
	{
		StatusUpdate(); if(!isActive | animSpeed==0.0f) { body.Sleep(); return; }
		onReset=false; onAttack=false; isConstrained= false;

		if(useAI && health!=0) { AICore(1, 2, 3, 4, 5, 6, 7); } // CPU
		else if(health!=0) { GetUserInputs(1, 2, 3, 4, 5, 6, 7); } // Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); } //Dead

    //Set Y position
    if(isOnGround | isInWater | isOnWater)
    {
      if(!isOnGround && !isInWater) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos();
    } else ApplyGravity();

		//Stopped
		if(OnAnm.IsName("Rex|Idle1A") | OnAnm.IsName("Rex|Idle2A") | OnAnm.IsName("Rex|Die1") | OnAnm.IsName("Rex|Die2"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Rex|Die1")) { onReset=true; if(!isDead) { PlaySound("Atk", 2); PlaySound("Die", 12); } }
			else if(OnAnm.IsName("Rex|Die2")) { onReset=true; if(!isDead) { PlaySound("Atk", 2); PlaySound("Die", 10); } }
		}

		//End Forward
		else if(OnAnm.normalizedTime > 0.5 && (OnAnm.IsName("Rex|Step1+")| OnAnm.IsName("Rex|Step2+") |
		        OnAnm.IsName("Rex|ToIdle1C") | OnAnm.IsName("Rex|ToIdle2B") | OnAnm.IsName("Rex|ToIdle2D") | OnAnm.IsName("Rex|ToEatA") |
		        OnAnm.IsName("Rex|ToEatC") | OnAnm.IsName("Rex|StepAtk1") | OnAnm.IsName("Rex|StepAtk2")))
			PlaySound("Step", 9);

		//Forward
		else if(OnAnm.IsName("Rex|Walk") | OnAnm.IsName("Rex|WalkGrowl") | (OnAnm.normalizedTime < 0.5 &&
		   (OnAnm.IsName("Rex|Step1+") | OnAnm.IsName("Rex|Step2+") | OnAnm.IsName("Rex|ToIdle2B") |
		   OnAnm.IsName("Rex|ToIdle1C") | OnAnm.IsName("Rex|ToIdle2D") | OnAnm.IsName("Rex|ToEatA") | OnAnm.IsName("Rex|ToEatC")) ) )
		{
      Move(transform.forward, 50);
			if(OnAnm.IsName("Rex|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Rex|Walk")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else { PlaySound("Step", 8); PlaySound("Step", 12); }
		}

		//Run
		else if(OnAnm.IsName("Rex|Run") | OnAnm.IsName("Rex|RunGrowl") | OnAnm.IsName("Rex|WalkAtk1") | OnAnm.IsName("Rex|WalkAtk2") |
		   (OnAnm.normalizedTime < 0.6 && (OnAnm.IsName("Rex|StepAtk1") | OnAnm.IsName("Rex|StepAtk2"))))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*5.0f, 0.05f), -20f, 20f);
			Move(transform.forward, 128);
			if(OnAnm.IsName("Rex|RunGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Rex|Run")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Rex|StepAtk1") | OnAnm.IsName("Rex|StepAtk2")) { onAttack=true; PlaySound("Atk", 2); PlaySound("Bite", 5); }
			else { onAttack=true; PlaySound("Atk", 2); PlaySound("Step", 6); PlaySound("Bite", 9); PlaySound("Step", 13); }
		}

		//Backward
		else if((OnAnm.normalizedTime > 0.4 && OnAnm.normalizedTime < 0.8) && (OnAnm.IsName("Rex|Step1-") | OnAnm.IsName("Rex|Step2-") | OnAnm.IsName("Rex|ToSleep2")))
		{
			Move(-transform.forward, 50);
			PlaySound("Step", 12);
		}

		//Strafe/Turn right
		else if(OnAnm.IsName("Rex|Strafe1-") | OnAnm.IsName("Rex|Strafe2+"))
		{
			Move(transform.right,25);
			PlaySound("Step", 6); PlaySound("Step", 13);
		}

		//Strafe/Turn left
		else if(OnAnm.IsName("Rex|Strafe1+") | OnAnm.IsName("Rex|Strafe2-"))
		{
			Move(-transform.right, 25);
			PlaySound("Step", 6); PlaySound("Step", 13);
		}

    //Idle Attack
    else if(OnAnm.IsName("Rex|IdleAtk1") | OnAnm.IsName("Rex|IdleAtk2"))
		{ 
      onAttack=true; Move(Vector3.zero);
      PlaySound("Atk", 1); PlaySound("Step", 3); PlaySound("Bite", 6);
    } 

		//Various
		else if(OnAnm.IsName("Rex|EatA")) { onReset=true; isConstrained=true; PlaySound("Food", 4); PlaySound("Bite", 5); }
		else if(OnAnm.IsName("Rex|EatB") | OnAnm.IsName("Rex|EatC")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Rex|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 2); }
		else if(OnAnm.IsName("Rex|ToSleep1") | OnAnm.IsName("Rex|ToSleep2")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Rex|ToIdle2A")) { Move(Vector3.zero); PlaySound("Sniff", 1); }
		else if(OnAnm.IsName("Rex|Idle1B")) { Move(Vector3.zero); PlaySound("Growl", 2); }
		else if(OnAnm.IsName("Rex|Idle1C")) { Move(Vector3.zero); PlaySound("Sniff", 4); PlaySound("Sniff", 7); PlaySound("Sniff", 10);}
		else if(OnAnm.IsName("Rex|Idle2B")) { Move(Vector3.zero); onReset=true; PlaySound("Bite", 4); PlaySound("Bite", 6); PlaySound("Bite", 8);}
		else if(OnAnm.IsName("Rex|Idle2C")) { Move(Vector3.zero); PlaySound("Growl", 2); }
		else if(OnAnm.IsName("Rex|Idle2D")) { Move(Vector3.zero); onReset=true; PlaySound("Atk", 2); }
		else if(OnAnm.IsName("Rex|Die1-")) { isConstrained=true; PlaySound("Growl", 3); isDead=false; }
		else if(OnAnm.IsName("Rex|Die2-")) { isConstrained=true; PlaySound("Growl", 3); isDead=false; }

    RotateBone(IkType.LgBiped, 65f);
	}

//*************************************************************************************************************************************************
// Bone rotation
	void LateUpdate()
	{
    if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;

		Spine0.rotation*= Quaternion.AngleAxis(headX, Vector3.forward)*Quaternion.AngleAxis(-headY, Vector3.right);
		Spine2.rotation*= Quaternion.AngleAxis(headX, Vector3.forward)*Quaternion.AngleAxis(-headY, Vector3.right);
		Neck0.rotation*= Quaternion.AngleAxis(headX, Vector3.forward)*Quaternion.AngleAxis(-headY, Vector3.right);
		Neck1.rotation*= Quaternion.AngleAxis(headX, Vector3.forward)*Quaternion.AngleAxis(-headY, Vector3.right);
		Neck2.rotation*= Quaternion.AngleAxis(headX, Vector3.forward)*Quaternion.AngleAxis(-headY, Vector3.right);
		Head.rotation*= Quaternion.AngleAxis(headX, Vector3.forward)*Quaternion.AngleAxis(-headY, Vector3.right);
		Tail2.rotation*= Quaternion.AngleAxis(-spineX, Vector3.forward);
		Tail3.rotation*= Quaternion.AngleAxis(-spineX, Vector3.forward);
		Tail4.rotation*= Quaternion.AngleAxis(-spineX, Vector3.forward);
		Tail5.rotation*= Quaternion.AngleAxis(-spineX, Vector3.forward);
		Tail6.rotation*= Quaternion.AngleAxis(-spineX, Vector3.forward);

		Right_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(lastHit, 0, 0);
		//Check for ground layer
		GetGroundPos(IkType.LgBiped, Right_Hips, Right_Leg, Right_Foot0, Left_Hips, Left_Leg, Left_Foot0);
	}
}

