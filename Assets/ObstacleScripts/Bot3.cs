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
	
	public int length {
		get { return 63; }
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
	
}
