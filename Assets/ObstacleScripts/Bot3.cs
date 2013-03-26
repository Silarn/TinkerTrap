using System;
using UnityEngine;

public class Bot3 : Player, Obstacle
{	// Primary Ability: Dash facing direction
	private bool dashing = false;
	private double endCooldown = 0.0;
	private double endDash = 0.0;
	private int dashDir = 0;
	private const double STEPDIST = 6.4; // Before x1.5
	private const double COOLDOWN = 2;
	private const double DURATION = 0.75;

	public Bot3 () : base(3)
	{
	}
	
	public Bot3 (double x, double y) : base(3,x,y)
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
			dashDir = currDir;
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
	
	public double STEP(){return STEPDIST;}
	public int DashDir() {return dashDir;}
}
