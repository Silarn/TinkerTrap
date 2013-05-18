using System;
using UnityEngine;

public class Bot3 : Player, Obstacle
{	// Primary Ability: Dash facing direction
	private bool dashing = false;
	private double endCooldown = 0.0;
	private double endDash = 0.0;
	private const double COOLDOWN = 2;
	private const double DURATION = 0.5;	
	
	public Charge chargeTile = null;
	private double chargeEnd = 0.0;
	
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
	
	public override void update(bool input) {
		if (moving || moveIntro) {
			if (!input) {
				stopping = true;
			}
		}
		if (moveTry && input)
			stopping = false;
		if (!os.isPlaying || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Charged_MvStop") || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Charged_Idle") ||
			os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Charged_Move") || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_MvStop") ||
			os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Idle") || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Move")) {
			animPlay = false;
			string suffix = "";
			string dirAnim = dirStr(currDir);
			if (turning) {
				if (idle) {
					if (animDir == currDir-1 || animDir == currDir+3)
						suffix = "TurnRtSt";
					else if (animDir == currDir+1 || animDir == currDir-3)
						suffix = "TurnLftSt";
					else if (Math.Abs (animDir-currDir) == 2)
						suffix = "TurnRvSt";
				} else {
					if (animDir == currDir-1 || animDir == currDir+3)
						suffix = "TurnRtMv";
					else if (animDir == currDir+1 || animDir == currDir-3)
						suffix = "TurnLftMv";
					else if (Math.Abs (animDir-currDir) == 2)
						suffix = "TurnRvMv";
				}
				dirAnim = dirStr(animDir);
				if (os.isPlaying)
					animDir = currDir;
				turning = false;
			} else if (stopping) {
				if (!os.isPlaying) {
					if (!playstop && !idle) {
						suffix = "MvStop";
						animDir = currDir;
						playstop = true;
					} else {
						suffix = "Idle";
						animDir = currDir;
						stopping = false;
						idle = true;
						playstop = false;
					}
				}
			} else if (moveTry) {
				if (idle) {
					suffix = "MvInt";
					animDir = currDir;
					idle = false;
				} else if (!os.isPlaying) {
					suffix = "Move";
					animDir = currDir;
					moving = true;
				}
				moveTry = false;
			}
			if (suffix != "") {
				if (chargeTile != null)
					os.PlayOnce (this.GetType ().Name + dirAnim +"_Charged_"+suffix);
				else
					os.PlayOnce (this.GetType ().Name + dirAnim +"_"+suffix);
			}
		}
	}
	
	public Charge ChargeSource {
		get { return chargeTile; }
		set { chargeTile = value;
			  chargeEnd = Time.time+8; }
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
		//Remove charge, if it still exists, after timeframe
		if (chargeTile != null && Time.time >= chargeEnd) {
			chargeTile = null;	}
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
