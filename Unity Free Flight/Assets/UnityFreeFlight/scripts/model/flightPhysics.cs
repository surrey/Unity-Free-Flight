﻿using UnityEngine;
using System.Collections;

public class FlightPhysics {

	public FlightBody FlightBody{ get; set; }

	private float formDrag;
	private float liftInducedDrag;

	public float FormDrag{ 
		get {return formDrag;}
	}
	public float LiftInducedDrag { 
		get {return liftInducedDrag;}
	}


	public float getLift(float velocity, float pressure, float area, float liftCoff) {
		pressure = 1.225f;
		float lift = velocity * velocity * pressure * area * liftCoff;
		return lift;
	}
	
	
	public float getDrag(float velocity, float pressure, float area, float dragCoff, float lift, float aspectR) {
		//wing span efficiency value
		float VSEV = .9f;
		pressure = 1.225f;
		
		liftInducedDrag = (lift*lift) / (.5f * pressure * velocity * velocity * area * Mathf.PI * VSEV * aspectR);
		formDrag = .5f * pressure * velocity * velocity * area * dragCoff;
		return LiftInducedDrag + FormDrag;
	}

	//When we do a turn, we don't just want to rotate our character. We want their
	//velocity to match the direction they are facing. 
	public Vector3 getDirectionalVelocity(Quaternion theCurrentRotation, Vector3 theCurrentVelocity) {
		Vector3 vel = theCurrentVelocity;
		
		vel = (theCurrentRotation * Vector3.forward).normalized * theCurrentVelocity.magnitude;	
		//Debug.Log (string.Format ("velocity: {0}, New Velocity {1} mag1: {2}, mag2 {3}", theCurrentVelocity, vel, theCurrentVelocity.magnitude, vel.magnitude));
		return vel;
	}

	//Get new yaw and roll, store the value in newRotation
	public Quaternion getBankedTurnRotation(Quaternion theCurrentRotation) {
		//Quaternion getBankedTurnRotation(float curZRot, float curLift, float curVel, float mass) {
		// The physics of a banked turn is as follows
		//  L * Sin(0) = M * V^2 / r
		//	L is the lift acting on the aircraft
		//	θ0 is the angle of bank of the aircraft
		//	m is the mass of the aircraft
		//	v is the true airspeed of the aircraft
		//	r is the radius of the turn	
		//
		// Currently, we'll keep turn rotation simple. The following is not based on the above, but it provides
		// A pretty snappy mechanism for getting the job done.
		//Apply Yaw rotations. Yaw rotation is only applied if we have angular roll. (roll is applied directly by the 
		//player)
		Quaternion angVel = Quaternion.identity;
		//Get the current amount of Roll, it will determine how much yaw we apply.
		float zRot = Mathf.Sin (theCurrentRotation.eulerAngles.z * Mathf.Deg2Rad) * Mathf.Rad2Deg;
		//We don't want to change the pitch in turns, so we'll preserve this value.
		float prevX = theCurrentRotation.eulerAngles.x;
		//Calculate the new rotation. The constants determine how fast we will turn.
		Vector3 rot = new Vector3(0, -zRot * 0.8f, -zRot * 0.5f) * Time.deltaTime;
		
		//Apply the new rotation 
		angVel.eulerAngles = rot;
		angVel *= theCurrentRotation;	
		angVel.eulerAngles = new Vector3(prevX, angVel.eulerAngles.y, angVel.eulerAngles.z);
		
		//Done!
		return angVel;	
	}

	
	public float getLiftCoefficient(float angleDegrees) {
		float cof;
		//			if(angleDegrees > 40.0f)
		//				cof = 0.0f;
		//			if(angleDegrees < 0.0f)
		//				cof = angleDegrees/90.0f + 1.0f;
		//			else
		//				cof = -0.0024f * angleDegrees * angleDegrees + angleDegrees * 0.0816f + 1.0064f;
		//Formula based on theoretical thin airfoil theory. We get a very rough estimate here,
		//and this does not take into account wing aspect ratio
		cof = 2 * Mathf.PI * angleDegrees * Mathf.Deg2Rad;
		return cof;
		
	}

	public float getDragCoefficient(float angleDegrees) {
		float cof;
		//if(angleDegrees < -20.0f)
		//	cof = 0.0f;
		//else
		cof = .0039f * angleDegrees * angleDegrees + .025f;
		return cof;
	}


	//Return angle of attack based on objects current directional Velocity and rotation
	public float getAngleOfAttack(Quaternion theCurrentRotation, Vector3 theCurrentVelocity) {
		//Angle of attack is basically the angle air strikes a wing. Imagine a plane flying 
		//at exact level altitude into a stable air mass. The air passes over the wing very
		//efficiently, so we have an AOA of zero. When the plane pitches back, air starts to
		//strike the bottom of the wing, creating more drag and lift. The angle of pitch 
		//relative to the airmass is called angle of attack. 
		float theAngleOfAttack;
		//The direction we are going
		Vector3 dirVel;
		
		//We need speed in order to get directional velocity.
		if (theCurrentVelocity != Vector3.zero) {
			//Find the direction we are going
			dirVel = Quaternion.LookRotation(theCurrentVelocity) * Vector3.up;
		} else {
			//This has the effect of 'imagining' the craft is on a level flight
			//moving forward. Since angle of attack means nothing at zero speed,
			//this is simply a way to visualize it when we are dead stopped.
			dirVel = Vector3.up;
		}
		
		//		Debug.Log(string.Format ("Directional Velocity : {0}", dirVel));
		
		//Find the rotation directly in front of us
		Vector3 forward = theCurrentRotation * Vector3.forward;
		//The dot product returns a positive or negative float if we are 'pitched up' towards
		//our air mass, or 'pitched down into' our airmass. Remember that our airmass also has
		//a velocity coming towards us, which is somewhere between coming directly at us in
		//level flight, or if we are falling directly towards the ground, it is coming directly
		//below us. 
		
		//The dot product always returns between -1 to 1, so taking the ArcSin will give us
		//a reasonable angle of attack. Remember to convert to degrees from Radians. 
		theAngleOfAttack = Mathf.Asin(Vector3.Dot(forward, dirVel)) * Mathf.Rad2Deg;
		return theAngleOfAttack;
	}

}
