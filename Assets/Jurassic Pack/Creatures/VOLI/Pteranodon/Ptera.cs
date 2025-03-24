using UnityEngine;

public class Ptera : Creature
{
	public Transform Root,Neck0,Neck1,Neck2,Neck3,Neck4,Neck5,Neck6,Right_Wing0,Left_Wing0,Right_Wing1,Left_Wing1,Right_Hand,Left_Hand, 
	Left_Hips,Right_Hips,Left_Leg,Right_Leg,Left_Foot,Right_Foot;
  public AudioClip Waterflush,Wind,Hit_jaw,Hit_head,Hit_tail,Smallstep,Smallsplash,Swallow,Idlecarn,Bite,Sniff2,Bigstep,Largesplash,Ptera1,Ptera2,Ptera3;

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 3); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Ptera1; break; case 1: painSnd=Ptera2; break; case 2: painSnd=Ptera3; break; }
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
			case "Bite": source[1].pitch=Random.Range(1.5f, 1.75f); source[1].PlayOneShot(Bite, 0.5f);
				lastframe=currframe; break;
			case "Sniff": source[1].pitch=Random.Range(1.5f, 1.75f);
				if(isInWater) source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
				else source[1].PlayOneShot(Sniff2, Random.Range(0.1f, 0.2f));
				lastframe=currframe; break;
			case "Die": source[1].pitch=Random.Range(0.8f, 1.0f); source[1].PlayOneShot(isOnWater|isInWater?Largesplash:Bigstep, 1.0f);
				lastframe=currframe; isDead=true; break;
			case "Food": source[0].pitch=Random.Range(3.0f, 3.25f); source[0].PlayOneShot(Swallow, 0.1f);
				lastframe=currframe; break;
			case "Repose": source[0].pitch=Random.Range(3.0f, 3.25f); source[0].PlayOneShot(Idlecarn, 0.25f);
				lastframe=currframe; break;
			case "Atk": int rnd1=Random.Range(0, 4); source[0].pitch=Random.Range(1.5f, 1.75f);
				if(rnd1==0) source[0].PlayOneShot(Ptera1, 1.0f);
				else if(rnd1==1) source[0].PlayOneShot(Ptera3, 1.0f);
				lastframe=currframe; break;
			case "Growl": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Ptera2, 1.0f);
				lastframe=currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		StatusUpdate(); if(!isActive | animSpeed==0.0f) { body.Sleep(); return; }
    Vector3 dir=-Root.right; anm.SetBool("OnGround", isOnGround);
		onReset=false; onAttack=false; isOnLevitation=false; isConstrained=false; onJump=false;

		if(useAI && health!=0) { AICore(1, 2, 3, 0, 4, 5, 6); }// CPU
		else if(health!=0) { GetUserInputs(1, 2, 3, 0, 4, 5, 6); }// Human
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); } //Dead

    //Set Y position
    if(isInWater && health>0) { body.drag=1; body.angularDrag=4; ApplyYPos(); anm.SetInteger ("Move", 1); }
    else if(isOnGround)
    {
      roll=Mathf.Lerp(roll, 0.0f, 0.1f); pitch=Mathf.Lerp(pitch, 0.0f, 0.1f);
      body.drag=4; body.angularDrag=4; ApplyYPos();
    }
    else
		{ 
			if(health>0) { body.drag=1; body.angularDrag=1; } //in air
			else if(isInWater) { body.drag=4; body.angularDrag=4; ApplyYPos(); }
			else { body.drag=1; body.angularDrag=1; ApplyGravity(); }
		} 

		//Stopped
		if(OnAnm.IsName("Ptera|IdleA") | OnAnm.IsName("Ptera|Die1") | OnAnm.IsName("Ptera|Die2") | OnAnm.IsName("Ptera|Fall"))
		{
      Move(Vector3.zero);
			if(OnAnm.IsName("Ptera|Die1")) { onReset=true; if(!isDead) { PlaySound("Growl", 1); PlaySound("Die", 11); } }
			else if(OnAnm.IsName("Ptera|Die2"))
			{ onReset=true; if(!isDead) PlaySound("Die", 0); }
			else if(OnAnm.IsName("Ptera|Fall"))
			{
				onReset=true; isOnLevitation=true;
				if(isInWater) anm.SetBool("OnGround", true);
				if(OnAnm.normalizedTime<0.1f) source[0].PlayOneShot(Ptera2, 0.5f);
			} 
		}

		//Forward
		else if(OnAnm.IsName("Ptera|Walk"))
		{
			Move(transform.forward, 10);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Running
		else if(OnAnm.IsName("Ptera|Run") | OnAnm.IsName("Ptera|FlightToRun") | OnAnm.IsName("Ptera|RunToFlight"))
		{
			isOnLevitation=true; Move(transform.forward, 100);
			PlaySound("Step", 5); PlaySound("Step", 6); PlaySound("Sniff", 7); PlaySound("Sniff", 8);
		}
		
		//Backward
		else if(OnAnm.IsName("Ptera|Walk-"))
		{
			Move(-transform.forward, 5);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}
		
		//Strafe/Turn right
		else if(OnAnm.IsName("Ptera|Strafe+"))
		{
			Move(transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}
		
		//Strafe/Turn left
		else if(OnAnm.IsName("Ptera|Strafe-"))
		{
			Move(-transform.right, 8);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Takeoff
		else if(OnAnm.IsName("Ptera|Takeoff"))
		{
			if(OnAnm.normalizedTime > 0.5) { isOnLevitation=true; onJump=true; Move(Vector3.up, 50); }
			PlaySound("Sniff", 7); PlaySound("Sniff", 8);
		}

		//Fly
		else if(OnAnm.IsName("Ptera|Flight") | OnAnm.IsName("Ptera|FlightGrowl") | OnAnm.IsName("Ptera|Glide") | OnAnm.IsName("Ptera|GlideGrowl"))
		{
			isOnLevitation=true;
			roll=Mathf.Lerp(roll, -spineX*10.0f, ang_T); pitch=Mathf.Lerp(pitch, Mathf.Clamp(anm.GetFloat("Pitch"),-0.75f, 1.0f)*90f, ang_T);
      Move(-Root.right, (200+Mathf.Abs(anm.GetFloat("Pitch")*100f)));
			if(OnAnm.IsName("Ptera|Flight")) { PlaySound("Sniff", 5); PlaySound("Sniff", 6); }
			else if(OnAnm.IsName("Ptera|FlightGrowl")) { PlaySound("Atk", 3); PlaySound("Sniff", 5); }
			else if(OnAnm.IsName("Ptera|GlideGrowl")) PlaySound("Growl", 2);
		}
		
		//Fly - Stationary
		else if(OnAnm.IsName("Ptera|Statio") | OnAnm.IsName("Ptera|StatioGrowl") | 
            OnAnm.IsName("Ptera|IdleD") | OnAnm.IsName("Ptera|FlyAtk"))
		{
			isOnLevitation=true;
			roll=Mathf.Lerp(roll, 0.0f, ang_T);  pitch=Mathf.Lerp(pitch, 0.0f, ang_T); 
      Move(Vector3.up, 100*-anm.GetFloat("Pitch")); //fly up/down
			if(isOnGround&&OnAnm.IsName("Ptera|FlyAtk")) Move(Vector3.up, 50);//takeoff
			if(anm.GetInteger("Move")>0 && anm.GetInteger("Move")<4) Move(transform.forward, 100); //fly forward
			else if(anm.GetInteger("Move")== -1) Move(-transform.forward, 100); //fly backward
			else if(anm.GetInteger("Move")== -10) Move(transform.right, 100); //fly right
			else if(anm.GetInteger("Move") == 10) Move(-transform.right, 100); //fly left

			if(OnAnm.IsName("Ptera|StatioGrowl")) PlaySound("Atk", 2);
			else if(OnAnm.IsName("Ptera|IdleD")) { PlaySound("Atk", 2); PlaySound("Step", 10); }
			else if(OnAnm.IsName("Ptera|FlyAtk")) { onAttack=true; PlaySound("Atk", 3); PlaySound("Bite", 8); }
			else { PlaySound("Sniff", 5); PlaySound("Sniff", 6); }
		}
	
		//Various
		else if(OnAnm.IsName("Ptera|Landing")) { isOnLevitation=true; PlaySound("Step", 2); PlaySound("Step", 3); }
		else if(OnAnm.IsName("Ptera|IdleB")) PlaySound("Atk", 2);
		else if(OnAnm.IsName("Ptera|IdleC")) { onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Ptera|EatA")) { onReset=true; isConstrained=true; PlaySound("Food", 1); }
		else if(OnAnm.IsName("Ptera|EatB")) { onReset=true; isConstrained=true; PlaySound("Bite", 0); }
		else if(OnAnm.IsName("Ptera|EatC")) onReset=true;
		else if(OnAnm.IsName("Ptera|ToSleep")){ onReset=true; isConstrained=true; }
		else if(OnAnm.IsName("Ptera|Sleep")) { onReset=true; isConstrained=true; PlaySound("Repose", 1); }
		else if(OnAnm.IsName("Ptera|Die-")) { isConstrained=true; PlaySound("Atk", 2);  isDead=false; }

		//Play wind sound based on speed
		if(isOnLevitation)
		{
			if(!source[2].isPlaying) source[2].PlayOneShot(Wind);
			source[2].volume=body.velocity.magnitude/(40*size);
			source[2].pitch=body.velocity.magnitude/(40*size);
		}
		else if(source[2].isPlaying) source[2].Pause();

    RotateBone(IkType.Flying, 32f);
	}

  //*************************************************************************************************************************************************
	// Bone rotation
	void LateUpdate()
	{
    if(!isActive) return; headPos=Head.GetChild(0).GetChild(0).position;
		Root.rotation*= Quaternion.Euler(roll, pitch, 0);
		Right_Wing0.rotation*= Quaternion.Euler(roll/2, Mathf.Clamp(roll, -35, 0), Mathf.Clamp(-pitch, -35, 0));
		Left_Wing0.rotation*= Quaternion.Euler(roll/2, Mathf.Clamp(-roll, -35, 0), Mathf.Clamp(pitch, 0, 35));
		Right_Wing0.GetChild(0).rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(pitch, 0, 90)+Mathf.Abs(roll)/2);
		Left_Wing0.GetChild(0).rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(-pitch, -90, 0)-Mathf.Abs(roll)/2);
		Right_Hand.rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(-pitch, -90, 0)-Mathf.Abs(roll));
		Left_Hand.rotation*= Quaternion.Euler(0, 0, Mathf.Clamp(pitch, 0, 90)+Mathf.Abs(roll));
		float headZ =headY*headX/yaw_Max;
		Neck0.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck1.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck2.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck3.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck4.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck5.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Neck6.rotation*= Quaternion.Euler(-headZ, -headY, headX);
		Head.rotation*= Quaternion.Euler(-headZ, -headY, headX);
    if(!isDead) Head.GetChild(0).transform.rotation*=Quaternion.Euler(0, lastHit, 0);
    //Check for ground layer
		GetGroundPos(IkType.Flying, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, Right_Wing0, Right_Wing1, Right_Hand, Left_Wing0, Left_Wing1, Left_Hand);
    anm.SetBool("OnGround", isOnGround);
	}
}










