using UnityEngine;

public class Dilo : Creature
{
	public Transform Spine0,Spine1,Spine2,Spine3,Spine4,Spine5,Neck0,Neck1,Neck2,Neck3,Tail0,Tail1,Tail2,Tail3,Tail4,Tail5,Tail6,Tail7,Tail8,Tail9,Tail10,Tail11,
  Arm1,Arm2,Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot0,Right_Foot0;
  public AudioClip Waterflush,Hit_jaw,Hit_head,Hit_tail,Smallstep,Smallsplash,Idlecarn,Swallow,Bite,Dilo1,Dilo2,Dilo3,Dilo4,Dilo5,Dilo6;
  Vector3 dir=Vector3.zero;
  
	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Dilo3; break; case 1: painSnd=Dilo4; break; case 2: painSnd=Dilo5; break; case 3: painSnd=Dilo6; break; }
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
			case "Food": source[0].pitch=Random.Range(3.0f, 3.5f); source[0].PlayOneShot(Swallow, 0.025f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(3.0f, 3.5f); source[0].PlayOneShot(Idlecarn, 0.25f);
				lastframe=currframe; break;
			case "AtkA": int rnd3=Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd3==0)source[0].PlayOneShot(Dilo4, 1.0f);
				else if(rnd3==1)source[0].PlayOneShot(Dilo5, 1.0f);
				lastframe=currframe; break;
			case "AtkB": int rnd2=Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd2==0)source[0].PlayOneShot(Dilo3, 1.0f);
				else if(rnd2==1)source[0].PlayOneShot(Dilo6, 1.0f);
				lastframe=currframe; break;
			case "Growl": int rnd1=Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd1==0)source[0].PlayOneShot(Dilo1, 1.0f);
				else if(rnd1==1)source[0].PlayOneShot(Dilo2, 1.0f);
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
    else { ApplyGravity(); anm.SetBool("OnGround", false); }

		//Stopped
		if(OnAnm.IsName("Dilo|IdleA") | OnAnm.IsName("Dilo|Die"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Dilo|Die")) { onReset=true; if(!isDead) { PlaySound("AtkB", 2); PlaySound("Die", 12); } }
		}

		//Jump
		else if(OnAnm.IsName("Dilo|IdleJumpStart") | OnAnm.IsName("Dilo|RunJumpStart") | OnAnm.IsName("Dilo|JumpIdle") |
			OnAnm.IsName("Dilo|IdleJumpEnd") | OnAnm.IsName("Dilo|RunJumpEnd") | OnAnm.IsName("Dilo|JumpAtk"))
		{
			if(OnAnm.IsName("Dilo|IdleJumpStart") | OnAnm.IsName("Dilo|RunJumpStart"))
			{
				if(OnAnm.normalizedTime > 0.4) Move(Vector3.up, 3, true); else onJump=true;
        if(anm.GetInteger("Move").Equals(2)) Move(dir, 160);
        else if(anm.GetInteger("Move").Equals(1)) Move(dir,32);
        
        PlaySound("Step", 1); PlaySound("Step", 2);
			}
			else if(OnAnm.IsName("Dilo|IdleJumpEnd") | OnAnm.IsName("Dilo|RunJumpEnd"))
			{ 
        if(OnAnm.IsName("Dilo|RunJumpEnd")) Move(dir, 160);
        body.velocity=new Vector3(body.velocity.x, 0.0f, body.velocity.z); onJump=false;
				PlaySound("Step", 3); PlaySound("Step", 4); 
			}
      else if(OnAnm.IsName("Dilo|JumpAtk"))
      {
        if(anm.GetInteger("Move").Equals(1)|anm.GetInteger("Move").Equals(2)) Move(Vector3.Lerp(dir, Vector3.zero,0.5f), 160);
        onAttack=true; PlaySound("AtkB", 1); PlaySound("Bite", 9);
        body.velocity=new Vector3(body.velocity.x, body.velocity.y>0?body.velocity.y:0, body.velocity.z);
      
      }
      else if(!anm.GetInteger("Move").Equals(0)) Move(Vector3.Lerp(dir, Vector3.zero,0.5f), 160);
		}

		//Forward
		else if(OnAnm.IsName("Dilo|Walk") | OnAnm.IsName("Dilo|WalkGrowl"))
		{
			Move(transform.forward, 32);
			if(OnAnm.IsName("Dilo|Walk")){ PlaySound("Step", 6); PlaySound("Step", 14);}
			else if(OnAnm.IsName("Dilo|WalkGrowl")) { PlaySound("AtkA", 2); PlaySound("Step", 6); PlaySound("Step", 14); }
		}

		//Running
		else if(OnAnm.IsName("Dilo|Run") | OnAnm.IsName("Dilo|RunGrowl") | OnAnm.IsName("Dilo|RunAtk1") |
		   (OnAnm.IsName("Dilo|RunAtk2") && OnAnm.normalizedTime < 0.9) |
		   (OnAnm.IsName("Dilo|IdleAtk3") && OnAnm.normalizedTime > 0.5 && OnAnm.normalizedTime < 0.9))
		{
      roll=Mathf.Clamp(Mathf.Lerp(roll, spineX*15.0f, 0.1f), -30f, 30f);
			Move(transform.forward, 160);
			if(OnAnm.IsName("Dilo|Run")){ PlaySound("Step", 4); PlaySound("Step", 12); }
			else if(OnAnm.IsName("Dilo|RunGrowl")) { PlaySound("AtkB", 2); PlaySound("Step", 4); PlaySound("Step", 12); }
			else if( OnAnm.IsName("Dilo|RunAtk1")) { onAttack=true; PlaySound("AtkB", 2); PlaySound("Step", 4); PlaySound("Step", 12); }
			else if( OnAnm.IsName("Dilo|RunAtk2")| OnAnm.IsName("Dilo|IdleAtk3"))
			{ onAttack=true; PlaySound("Growl", 2); PlaySound("Step", 4); PlaySound("Bite", 9); PlaySound("Step", 12); }
		}
		
		//Backward
		else if(OnAnm.IsName("Dilo|Walk-") | OnAnm.IsName("Dilo|WalkGrowl-"))
		{
			if(OnAnm.normalizedTime > 0.25 && OnAnm.normalizedTime < 0.45| 
			 OnAnm.normalizedTime > 0.75 && OnAnm.normalizedTime < 0.9) Move(-transform.forward, 32);
			if(OnAnm.IsName("Dilo|WalkGrowl-")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else { PlaySound("Step", 6); PlaySound("Step", 13); }
		}

		//Strafe/Turn right
		else if(OnAnm.IsName("Dilo|Strafe-"))
		{
			Move(transform.right, 16);
			PlaySound("Step", 6); PlaySound("Step", 14);
		}
		
		//Strafe/Turn left
		else if(OnAnm.IsName("Dilo|Strafe+"))
		{
			Move(-transform.right, 16);
			PlaySound("Step", 6); PlaySound("Step", 14);
		}

		//Various
    else if(OnAnm.IsName("Dilo|IdleAtk3"))  { onAttack=true; Move(Vector3.zero); PlaySound("AtkA", 1); }
		else if(OnAnm.IsName("Dilo|GroundAtk")) { onAttack=true; PlaySound("AtkB", 2); PlaySound("Bite", 4); }
		else if(OnAnm.IsName("Dilo|IdleAtk1") | OnAnm.IsName("Dilo|IdleAtk2"))
		{ onAttack=true; Move(Vector3.zero); PlaySound("AtkA", 2); PlaySound("Bite", 9); }
		else if(OnAnm.IsName("Dilo|ToSleep")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Dilo|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 1); }
		else if(OnAnm.IsName("Dilo|EatA")) { onReset=true; isConstrained=true; PlaySound("Food", 1); }
		else if(OnAnm.IsName("Dilo|EatB")) { onReset=true; isConstrained=true; PlaySound("Bite", 3); }
		else if(OnAnm.IsName("Dilo|EatC")) onReset=true;
		else if(OnAnm.IsName("Dilo|IdleC")) PlaySound("Growl", 1);
		else if(OnAnm.IsName("Dilo|IdleD")) PlaySound("Growl", 1);
		else if(OnAnm.IsName("Dilo|IdleE")) { onReset=true; PlaySound("Bite", 4); PlaySound("Bite", 7); PlaySound("Bite", 9); }
		else if(OnAnm.IsName("Dilo|Die-")) { onReset=true; PlaySound("AtkA", 1);  isDead=false; }

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
    Tail9.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail10.rotation*=Quaternion.Euler(0, 0, -spineX);
		Tail11.rotation*=Quaternion.Euler(0, 0, -spineX);
		Right_Hips.rotation*= Quaternion.Euler(-roll, 0, 0);
		Left_Hips.rotation*= Quaternion.Euler(0, roll, 0);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(lastHit, 0, 0);
		//Check for ground layer
		GetGroundPos(IkType.SmBiped, Right_Hips, Right_Leg, Right_Foot0, Left_Hips, Left_Leg, Left_Foot0);
	}

}