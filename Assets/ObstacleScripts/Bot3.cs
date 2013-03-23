using System;
using UnityEngine;

public class Bot3 : Player, Obstacle
{
	private bool dashing = false;
	private double endCooldown = 0.0;
	private double endDash = 0.0;
	private int frameCount = 0;
//	public int dashDir = 0;
	
	public Bot3 () : base(3)
	{
	}
	
	public Bot3 (double x, double y) : base(3,x,y)
	{
	}
	
	public override int width {
		get { return 40; }
	}
	
	public override double getSpeed (double speed)
	{
		return (Math.Floor (speed*1.5));
	}
	
	public override void primary (Tile a)
	{
		if (!dashing && Time.time >= endCooldown)
		{
			dashing = true;
			endCooldown = Time.time+2;
			endDash = Time.time+0.75;
			actDir = currDir;
		}
	}
	
	public override bool act()
	{
		//Dash Conditions
		if (Time.time >= endDash){
			dashing = false;
			return false;
		} else 
			return true;
	}
}
