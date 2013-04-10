using System;
using UnityEngine;

public class Bot3 : Player, Obstacle
{	// Primary Ability: Dash facing direction
	private bool dashing = false;
	private double endCooldown = 0.0;
	private double endDash = 0.0;
	private const double COOLDOWN = 2;
	private const double DURATION = 0.5;			
	
	public Bot3 ()
	{
	}
	
	public Bot3 (double x, double y)
	{
	}
	
	public override int width {
		get { return 40; }
	}
	
	public override int length {
		get { return 63; }
	}
	
	public virtual double[] turnCorner {
		get {
			// return same as SetCorners, except backwardes
			double[] testPos = new double[4];
			if (currDir == 0 || currDir == 2) {
				testPos[0] = posY+(length/2)/2;
				testPos[2] = upYPos+width/2-1;
				testPos[3] = posX;
				testPos[1] = leftXPos+length/2-1;
			} else { // switch using length/width;
				testPos[0] = posY;
				testPos[2] = upYPos+length/2-1;
				testPos[3] = posX+(length/2)/2;
				testPos[1] = leftXPos+width/2-1;
			}
			return testPos;
			}	
	}
	
	public override double getSpeed (double speed)
	{
		if (dashing)
			return (Math.Floor (speed*2.6)); // speed of 5 -> 9.6
		return (Math.Floor (speed*1.5));
	}
	
//	public override void setDir (int dir)
//	{
//		// Check to see what direction changes are allowed
//	}
	
	public override void primary (Tile a)
	{
		if (!dashing && Time.time >= endCooldown)
		{
			dashing = true;
			endCooldown = Time.time+COOLDOWN;
			endDash = Time.time+DURATION;
		}
	}
	
	public override bool inAction()
	{
		//Dash Conditions
		if (!dashing || Time.time >= endDash){
			dashing = false;
			return false;
		} else 
			return true;
	}
	
	public override void endAction() {
		if (dashing) {
			dashing = false;
		}
	}
	
	public override void SetCorners() { 
		if (currDir == 0 || currDir == 2) {
			upYPos = posY;
			downYPos = upYPos+length/2-1;
			leftXPos = posX+(length/2)/2;
			rightXPos = leftXPos+width/2-1;
		} else { // switch using length/width for downY and rightX
			upYPos = posY+(length/2)/2-1;
			downYPos = upYPos+width/2-1;
			leftXPos = posX;
			rightXPos = leftXPos+length/2-1;
		}
	}
	
}
