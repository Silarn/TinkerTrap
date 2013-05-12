using System;
using UnityEngine;

public class Bot3 : Player, Obstacle
{	// Primary Ability: Dash facing direction
	private bool dashing = false;
	private double endCooldown = 0.0;
	private double endDash = 0.0;
	private const double COOLDOWN = 2;
	private const double DURATION = 0.5;	
	
	public double charge = 0.0;
	
	public double upYPos2;
	public double downYPos2;
	public double leftXPos2;
	public double rightXPos2;
	
	public AudioClip audioDash;
	
	public Bot3 ()
	{
		audioDash = Resources.Load ("propel") as AudioClip;
		gfx.AddComponent<AudioSource>().clip = audioDash;
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
	
	public void Charge() {
		if (level > 0 && charge < 5) {
			charge+= 0.5;
			if (charge >= 5) {
				charge = 5;
				Debug.Log ("Bot 3 now fully charged");
			}
		}
	}
	
	public void Dissipate() {
		if (charge > 0)
			charge-= 0.033;
	}
	public void TurnCorners() {
			// return same as SetCorners, except backwardes
			if (currDir == 0 || currDir == 2) {
				upYPos2 = posY+(length/4)/2;
				downYPos2 = upYPos+width/2-1;
				leftXPos2 = posX;
				rightXPos2 = leftXPos+length/2-1;
			} else { // switch using length/width;
				upYPos2 = posY;
				downYPos2 = upYPos+length/2-1;
				leftXPos2 = posX+(length/4)/2;
				rightXPos2 = leftXPos+width/2-1;
			}
	}
	
	public override double getSpeed (double speed)
	{
		if (dashing)
			return (Math.Floor (speed*2.6)); // speed of 5 -> 9.6
		return (Math.Floor (speed*1.5));
	}
	
	public override void primary (Tile a)
	{
		if (!dashing && Time.time >= endCooldown)
		{
			gfx.GetComponent<AudioSource>().Play ();
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
			leftXPos = posX+(length/4)/2;
			rightXPos = leftXPos+width/2-1;
		} else { // switch using length/width for downY and rightX
			upYPos = posY+(length/4)/2;
			downYPos = upYPos+width/2-1;
			leftXPos = posX;
			rightXPos = leftXPos+length/2-1;
		}
	}
	
}
