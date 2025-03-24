using UnityEngine;

public class Comp : Creature
{
	public Transform Spine0,Spine1,Spine2,Spine3,Spine4,Spine5,Neck0,Neck1,Neck2,Neck3,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Tail6,Tail7,Tail8,
  Arm1,Arm2,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot0,Right_Foot0;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Smallstep,Smallsplash,Bite,Comp1,Comp2,Comp3,Comp4,Comp5;
  Vector3 dir=Vector3.zero;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Comp1; break; case 1: painSnd=Comp2; break; case 2: painSnd=Comp3; break; case 3: painSnd=Comp4; break; }
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
				else if(isOnWater) source[1].PlayOneShot(Smallsplash, Random.Range(0.25f, 0.5f));
				else if(isOnGround) source[1].PlayOneShot(Smallstep, Random.Range(0.25f, 0.5f));
				lastframe=currframe; break;
			case "Bite": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(Bite, 0.5f);
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(0.8f, 1.0f); source[1].PlayOneShot(isOnWater|isInWater?Smallsplash:Smallstep, 1.0f);
				lastframe=currframe; isDead=true; break;
			case "Call": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Comp4, 1.0f);
				lastframe=currframe; break;
			case "Atk": int rnd1=Random.Range (0, 3); source[0].pitch=Random.Range(1.0f, 1.75f);
				if(rnd1==0)source[0].PlayOneShot(Comp2, 1.0f);
				else if(rnd1==1)source[0].PlayOneShot(Comp3, 1.0f);
				else if(rnd1==2) source[0].PlayOneShot(Comp5, 1.0f);
				lastframe=currframe; break;
			case "Growl": int rnd2=Random.Range (0, 5); source[0].pitch=Random.Range(1.0f, 1.75f);
				if(rnd2==0)source[0].PlayOneShot(Comp1, 1.0f);
				else if(rnd2==1)source[0].PlayOneShot(Comp2, 1.0f);
				else if(rnd2==2)source[0].PlayOneShot(Comp3, 1.0f);
				else if(rnd2==3)source[0].PlayOneShot(Comp4, 1.0f);
				else if(rnd2==4)source[0].PlayOneShot(Comp5, 1.0f);
				lastframe=currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate()
	{
		StatusUpdate(); if(!isActive | animSpeed==0.0f) { body.Sleep(); return; }
		onReset=false; onAttack=false; isConstrained=false;

		if(useAI && health!=0) { AICore(1, 2, 3, 4, 5, 6, 7); }// CPU
		else if(health!=0) { GetUserInputs(1, 2, 3, 4, 5, 6, 7); }// Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }//Dead

    //Set Y position
    if(isOnGround | isInWater | isOnWater)
    {
      if(!isOnGround && !isInWater) { body.drag=1; body.angularDrag=1; } else { body.drag=4; body.angularDrag=4; }
      ApplyYPos(); anm.SetBool("OnGround", true);
      dir=new Vector3(transform.forward.x, 0, transform.forward.z);
    }
    else { ApplyGravity(0.5f); anm.SetBool("OnGround", false); }

		//Stopped
		if(OnAnm.IsName("Comp|IdleA") | OnAnm.IsName("Comp|Die"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Comp|Die")) { onReset=true; if(!isDead) { PlaySound("AtkB", 2); PlaySound("Die", 12); } }
		}

    //Jump
		else if(OnAnm.IsName("Comp|IdleJumpStart") | OnAnm.IsName("Comp|RunJumpStart") | OnAnm.IsName("Comp|JumpIdle") |
			OnAnm.IsName("Comp|IdleJumpEnd") | OnAnm.IsName("Comp|RunJumpEnd") | OnAnm.IsName("Comp|JumpAtk"))
		{
			if(OnAnm.IsName("Comp|IdleJumpStart") | OnAnm.IsName("Comp|RunJumpStart"))
			{
				if(OnAnm.normalizedTime > 0.4) Move(Vector3.up, 1.5f, true); else onJump=true;
        if(anm.GetInteger("Move").Equals(2)) Move(dir, 80);
        else if(anm.GetInteger("Move").Equals(1)) Move(dir,32);
        PlaySound("Step", 1); PlaySound("Step", 2);
			}
			else if(OnAnm.IsName("Comp|IdleJumpEnd") | OnAnm.IsName("Comp|RunJumpEnd"))
			{ 
        if(OnAnm.IsName("Comp|RunJumpEnd")) Move(dir, 80);
        body.velocity=new Vector3(body.velocity.x, 0.0f, body.velocity.z); onJump=false;
				PlaySound("Step", 3); PlaySound("Step", 4); 
			}
      else if(OnAnm.IsName("Comp|JumpAtk"))
      {
        if(anm.GetInteger("Move").Equals(1) | anm.GetInteger("Move").Equals(2)) Move(Vector3.Lerp(dir, Vector3.zero, 0.5f), 80);
        onAttack=true; PlaySound("AtkB", 1); PlaySound("Bite", 9);
        body.velocity=new Vector3(body.velocity.x, body.velocity.y>0?body.velocity.y:0, body.velocity.z);
      }
      else if(!anm.GetInteger("Move").Equals(0)) Move(Vector3.Lerp(dir, Vector3.zero, 0.5f), 80);
		}

		//Forward
		else if(OnAnm.IsName("Comp|Walk"))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*15.0f, 0.1f), -30f, 30f);
			Move(transform.forward, 20);
			PlaySound("Step", 8); PlaySound("Step", 9);
		}

		//Running
		else if(OnAnm.IsName("Comp|Run") | OnAnm.IsName("Comp|RunGrowl") | OnAnm.IsName("Comp|RunAtk1") |
		   (OnAnm.IsName("Comp|RunAtk2") && OnAnm.normalizedTime < 0.9) |
		   (OnAnm.IsName("Comp|IdleAtk3") && OnAnm.normalizedTime > 0.5 && OnAnm.normalizedTime < 0.9))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*15.0f, 0.1f), -30f, 30f);
			Move(transform.forward, 80);
			if(OnAnm.IsName("Comp|Run")){ PlaySound("Step", 4); PlaySound("Step", 12); }
			else if(OnAnm.IsName("Comp|RunGrowl")) { PlaySound("Atk", 2); PlaySound("Step", 4); PlaySound("Step", 12); }
			else if( OnAnm.IsName("Comp|RunAtk1")) { onAttack=true; PlaySound("Atk", 2); PlaySound("Step", 4); PlaySound("Step", 12); }
			else if( OnAnm.IsName("Comp|RunAtk2")| OnAnm.IsName("Comp|IdleAtk3"))
			{ onAttack=true; PlaySound("Atk", 2); PlaySound("Step", 4); PlaySound("Bite", 9); PlaySound("Step", 12); }
		}
		
		//Backward
		else if(OnAnm.IsName("Comp|Walk-"))
		{
			Move(-transform.forward, 16);
			PlaySound("Step", 8); PlaySound("Step", 9);
		}

		//Strafe/Turn right
		else if(OnAnm.IsName("Comp|Strafe-"))
		{
			Move(transform.right, 16);
			PlaySound("Step", 8); PlaySound("Step", 9);
		}
		
		//Strafe/Turn left
		else if(OnAnm.IsName("Comp|Strafe+"))
		{
			Move(-transform.right, 16);
			PlaySound("Step", 8); PlaySound("Step", 9);
		}

		//Various
		else if(OnAnm.IsName("Comp|IdleAtk3")) { onAttack=true; Move(Vector3.zero); PlaySound("Atk", 1); }
		else if(OnAnm.IsName("Comp|GroundAtk")) { onAttack=true; PlaySound("Atk", 2); PlaySound("Bite", 4); }
		else if(OnAnm.IsName("Comp|IdleAtk1") | OnAnm.IsName("Comp|IdleAtk2"))
		{ onAttack=true; Move(Vector3.zero); PlaySound("Atk", 2); PlaySound("Bite", 9); }
		else if(OnAnm.IsName("Comp|ToSleep")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Comp|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 1); }
		else if(OnAnm.IsName("Comp|EatA")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Comp|EatB")) { onReset=true; isConstrained=true; PlaySound("Bite", 3); }
		else if(OnAnm.IsName("Comp|EatC")) onReset=true;
		else if(OnAnm.IsName("Comp|IdleB")) { onReset=true; PlaySound("Atk", 1); }
		else if(OnAnm.IsName("Comp|IdleC")) { onReset=true; isConstrained=true; PlaySound("Step", 2); }
		else if(OnAnm.IsName("Comp|IdleD")) PlaySound("Growl", 1);
		else if(OnAnm.IsName("Comp|IdleE")) { PlaySound("Call", 1); PlaySound("Call", 4); PlaySound("Call", 8); }
		else if(OnAnm.IsName("Comp|Die-")) { onReset=true; PlaySound("Atk", 1);  isDead=false; }

    RotateBone(IkType.SmBiped, 60f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
		if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;
		float headZ =-headY*headX/yaw_Max;
		Spine0.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine1.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine2.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine3.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine4.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Spine5.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Arm1.rotation*=Quaternion.Euler(headY*8, 0, 0);
		Arm2.rotation*=Quaternion.Euler(0, headY*8, 0);
		Neck0.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Neck1.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Neck2.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Neck3.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Head.rotation*=Quaternion.Euler(-headY, headZ, headX);
		Tail0.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail1.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail2.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail3.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail4.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail5.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail6.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail7.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail8.rotation*=Quaternion.Euler(0, 0, -spineX);
		Right_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Hips.rotation*= Quaternion.Euler(0, roll, 0);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(lastHit, 0, 0);
		//Check for ground layer
		GetGroundPos(IkType.SmBiped, Right_Hips, Right_Leg, Right_Foot0, Left_Hips, Left_Leg, Left_Foot0); 
	}
}
