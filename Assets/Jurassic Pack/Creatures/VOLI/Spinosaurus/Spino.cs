using UnityEngine;

public class Spino : Creature
{
	public Transform Spine0,Spine1,Spine2,Neck0,Neck1,Neck2,Tail2,Tail3,Tail4,Tail5,Tail6,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot0,Right_Foot0;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Bigstep,Largesplash,Largestep,Idlecarn,Bite,Swallow,Sniff1,Spino1,Spino2,Spino3,Spino4;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Spino1; break; case 1: painSnd=Spino2; break; case 2: painSnd=Spino3; break; case 3: painSnd=Spino4; break; }
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
			case "Food": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Swallow, 0.1f);
				lastframe=currframe; break;
			case "Sniff": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Sniff1, 0.5f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(0.75f, 1.25f); source[0].PlayOneShot(Idlecarn, 0.25f);
				lastframe=currframe; break;
			case "Atk": int rnd1=Random.Range (0, 2); source[0].pitch=Random.Range(0.75f, 1.75f);
				if(rnd1==0)source[0].PlayOneShot(Spino3, 0.5f);
				else source[0].PlayOneShot(Spino4, 0.5f);
				lastframe=currframe; break;
			case "Growl": int rnd2=Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd2==0)source[0].PlayOneShot(Spino1, 1.0f);
				else source[0].PlayOneShot(Spino2, 1.0f);
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

		if(useAI && health!=0) { AICore(1, 2, 3, 4, 5, 6, 7); }// CPU
		else if(health!=0) { GetUserInputs(1, 2, 3, 4, 5, 6, 7); }// Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
    if(isOnGround | isInWater | isOnWater)
    {
      if(!isOnGround && !isInWater) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos();
    } else ApplyGravity();

		//Stopped
		if(OnAnm.IsName("Spino|Idle1A") | OnAnm.IsName("Spino|Idle2A") |
			OnAnm.IsName("Spino|Die1") | OnAnm.IsName("Spino|Die2"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Spino|Die1")) { onReset=true; if(!isDead) { PlaySound("Atk", 2); PlaySound("Die", 12); } }
			else if(OnAnm.IsName("Spino|Die2")) { onReset=true; if(!isDead) { PlaySound("Atk", 2); PlaySound("Die", 10); } }
		}

		//End Forward
		else if(OnAnm.normalizedTime > 0.5 && (OnAnm.IsName("Spino|Step1+")| OnAnm.IsName("Spino|Step2+") |
		        OnAnm.IsName("Spino|ToIdle1C") | OnAnm.IsName("Spino|ToIdle2B") | OnAnm.IsName("Spino|ToIdle2D") | OnAnm.IsName("Spino|ToEatA") |
		        OnAnm.IsName("Spino|ToEatC") | OnAnm.IsName("Spino|StepAtk1") | OnAnm.IsName("Spino|StepAtk2")))
			PlaySound("Step", 9);

		//Forward
		else if(OnAnm.IsName("Spino|Walk") | OnAnm.IsName("Spino|WalkGrowl") | (OnAnm.normalizedTime < 0.5 &&
		   (OnAnm.IsName("Spino|Step1+") | OnAnm.IsName("Spino|Step2+") | OnAnm.IsName("Spino|ToIdle2B") |
		   OnAnm.IsName("Spino|ToIdle1C") | OnAnm.IsName("Spino|ToIdle2D") | OnAnm.IsName("Spino|ToEatA") | OnAnm.IsName("Spino|ToEatC")) ) )
		{
      Move(transform.forward, 50);
			if(OnAnm.IsName("Spino|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Spino|Walk")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else { PlaySound("Step", 8); PlaySound("Step", 12); }
		}

		//Run
		else if(OnAnm.IsName("Spino|Run") | OnAnm.IsName("Spino|RunGrowl") | OnAnm.IsName("Spino|WalkAtk1") | OnAnm.IsName("Spino|WalkAtk2") |
		   (OnAnm.normalizedTime < 0.6 && (OnAnm.IsName("Spino|StepAtk1") | OnAnm.IsName("Spino|StepAtk2"))))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*5.0f, 0.05f), -20f, 20f);
			Move(transform.forward, 128);
			if(OnAnm.IsName("Spino|RunGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Spino|Run")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(OnAnm.IsName("Spino|StepAtk1") | OnAnm.IsName("Spino|StepAtk2")) { onAttack=true; PlaySound("Atk", 2); PlaySound("Bite", 5); }
			else { onAttack=true; PlaySound("Atk", 2); PlaySound("Step", 6); PlaySound("Bite", 9); PlaySound("Step", 13); }
		}

		//Backward
		else if((OnAnm.normalizedTime > 0.4 && OnAnm.normalizedTime < 0.8) && (OnAnm.IsName("Spino|Step1-") | OnAnm.IsName("Spino|Step2-") | OnAnm.IsName("Spino|ToSleep2")))
		{
			Move(-transform.forward, 50);
			PlaySound("Step", 12);
		}

		//Strafe/Turn right
		else if(OnAnm.IsName("Spino|Strafe1-") | OnAnm.IsName("Spino|Strafe2+"))
		{
			Move(transform.right, 25);
			PlaySound("Step", 6); PlaySound("Step", 13);
		}

		//Strafe/Turn left
		else if(OnAnm.IsName("Spino|Strafe1+") | OnAnm.IsName("Spino|Strafe2-"))
		{
			Move(-transform.right, 25);
			PlaySound("Step", 6); PlaySound("Step", 13);
		}

		else if(OnAnm.IsName("Spino|IdleAtk1") | OnAnm.IsName("Spino|IdleAtk2"))
		{ 
      onAttack=true; Move(Vector3.zero);
      PlaySound("Atk", 1); PlaySound("Step", 3); PlaySound("Bite", 6);
    } 

		//Various
		else if(OnAnm.IsName("Spino|EatA")) { onReset=true; isConstrained=true; PlaySound("Food", 4); PlaySound("Bite", 5); }
		else if(OnAnm.IsName("Spino|EatB") | OnAnm.IsName("Spino|EatC")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Spino|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 2); }
		else if(OnAnm.IsName("Spino|ToSleep1") | OnAnm.IsName("Spino|ToSleep2")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Spino|ToIdle2A")) PlaySound("Sniff", 1);
		else if(OnAnm.IsName("Spino|Idle1B")) PlaySound("Growl", 2);
		else if(OnAnm.IsName("Spino|Idle1C")) { PlaySound("Sniff", 4); PlaySound("Sniff", 7); PlaySound("Sniff", 10);}
		else if(OnAnm.IsName("Spino|Idle2B")) { onReset=true; PlaySound("Bite", 4); PlaySound("Bite", 6); PlaySound("Bite", 8);}
		else if(OnAnm.IsName("Spino|Idle2C")) PlaySound("Growl", 2);
		else if(OnAnm.IsName("Spino|Idle2D")) { onReset=true; PlaySound("Atk", 2); }
		else if(OnAnm.IsName("Spino|Die1-")) { isConstrained=true; PlaySound("Growl", 3);  isDead=false; }
		else if(OnAnm.IsName("Spino|Die2-")) { isConstrained=true; PlaySound("Growl", 3);  isDead=false; }

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

