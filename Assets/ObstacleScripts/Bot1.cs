using System;
using System.Collections.Generic;
using UnityEngine;

public class Bot1 : Player, Obstacle
{
	public bool grabbing = false;
	public bool extendingArms = false;
	public bool retractArms = false;
	public int extendDist = 0;
	public const int EXTEND_MAX = 192;
	public const int STEP_SIZE = 10;
//	public List<BotArm> arm = new List<BotArm>();
	public Obstacle grabbed;
	
	public Bot1 ()
	{
//		arm.Add(new BotArm(1));
//		arm.Add(new BotArm(1));
		
		os.PlayLoop (this.GetType ().Name + dirStr (currDir)+"_Idle");
	}
	
	public Bot1 (double x, double y)
	{
//		arm.Add(new BotArm(x,y));
//		arm.Add(new BotArm(x,y));
			
		os.PlayLoop (this.GetType ().Name + dirStr (currDir)+"_Idle");
	}
	
	public override int width {
		get { return 70; }
	}
	
	public override void setDir(int dir)
	{
		int startDir = currDir;
		if (animDir == null)
			animDir = dir;
		else
			moveTry = true;
		if (!grabbing) {
			currDir = dir;
		}
		if (currDir != startDir)
			turning = true;
	}
	
	public override void update(bool input)
	{
		if (moving || moveIntro) {
			if (!input) {
				stopping = true;
			}
		}
		if (moveTry && input)
			stopping = false;
		if (!os.isPlaying || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_MvStop") || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Idle")) {
			animPlay = false;
			if (!grabbing) {
				if (turning) {
					if (idle) {
						if (animDir == currDir-1 || animDir == currDir+3)
							os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRtSt");
						else if (animDir == currDir+1 || animDir == currDir-3)
							os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnLftSt");
						else if (Math.Abs (animDir-currDir) == 2)
							os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRvSt");
					} else {
						if (animDir == currDir-1 || animDir == currDir+3)
							os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRtMv");
						else if (animDir == currDir+1 || animDir == currDir-3)
							os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnLftMv");
						else if (Math.Abs (animDir-currDir) == 2)
							os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRvMv");
					}
					if (os.isPlaying)
						animDir = currDir;
					turning = false;
				} else if (stopping) {
					if (!os.isPlaying) {
						if (!playstop && !idle) {
							os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_MvStop");
							animDir = currDir;
							playstop = true;
						} else {
							os.PlayLoop (this.GetType ().Name + dirStr (currDir)+"_Idle");
							animDir = currDir;
							stopping = false;
							idle = true;
							playstop = false;
						}
					}
				} else if (moveTry) {
					if (idle) {
						os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_MvInt");
						animDir = currDir;
						idle = false;
					} else {
						os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_Move");
						animDir = currDir;
						moving = true;
					}
					moveTry = false;
				}
			}
		}
	}
	
	public override void primary (Tile a)
	{
		if (grabbing)
			Release ();
	}
	
	public override void primary(Tile a, Obstacle b)
	{
		if (!b.GetType ().IsSubclassOf (typeof(Player))) {
			if (grabbing) {
				Release ();
			} else {
				Grab (b);
			}
		}	
	}
	
	public void secondary () {
		if (level > 0){
			extendingArms = true;
			Debug.Log("extendingArms TRUE");
		}
	}
	
	public void Grab (Obstacle a) {
		if (!grabbing) {
			grabbing = true;
			grabbed = a;
			os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_Grab");
		}
	}
	
	public void Release() {
		grabbing = false;
		grabbed = null;
		os.PlayOnceBackward (this.GetType ().Name + dirStr (currDir)+"_Grab");
		if (extendingArms){
			extendingArms = false;
			retractArms = true;
		}
	}
	
	public override bool inAction() {
		if (extendingArms || retractArms/* || animPlay*/)
			return true;
		else 
			return false;
	}
	
	public override void endAction() {
		if (extendingArms){
			extendingArms = false;
			retractArms = true;
		}
//		retractArms = false;
	}
	
	public int ExtendArmsStep() {
		if (extendingArms){
			extendDist += STEP_SIZE;
			if (extendDist > EXTEND_MAX) {
				extendingArms = false;
				retractArms = true;
			}
		}
		if (retractArms){
			extendDist -= STEP_SIZE;
			if (extendDist < 0)
				retractArms = false;
		}
		return extendDist;
	}
	
	public bool ExtendArmsAction (List<Obstacle> gObs) {			// Return true if there is a object to move? Could just check grabbed.
		if (upleft || upright || downleft || downright){	//remove this when arm objects in place
			return extendingArms = false;
		}
		if (extendingArms && !grabbing) {
			foreach (Obstacle o in gObs) {
				if (o != this) {
//					getMyCorners(o, o.posX, o.posY);
					if (upYPos < o.downYPos && upYPos > o.upYPos && leftXPos < o.rightXPos && leftXPos > o.leftXPos)
						Grab(o);
					if (downYPos < o.downYPos && downYPos > o.upYPos && leftXPos < o.rightXPos && leftXPos > o.leftXPos)
						Grab(o);
					if (upYPos < o.downYPos && upYPos > o.upYPos && rightXPos < o.rightXPos && rightXPos > o.leftXPos)
						Grab(o);
					if (downYPos < o.downYPos && downYPos > o.upYPos && rightXPos < o.rightXPos && rightXPos > o.leftXPos)
						Grab(o);
				}
			}
			if (grabbing){
				extendingArms = false;
				return true;
			}
		} else if (extendingArms && grabbing) {
			foreach (Obstacle o in gObs) {
				if (o != this && o != grabbed) {
//					getMyCorners(o, o.posX, o.posY);
					if (grabbed.upYPos < o.downYPos && grabbed.upYPos > o.upYPos && grabbed.leftXPos < o.rightXPos && grabbed.leftXPos > o.leftXPos)
						return extendingArms = false;
					if (grabbed.downYPos < o.downYPos && grabbed.downYPos > o.upYPos && grabbed.leftXPos < o.rightXPos && grabbed.leftXPos > o.leftXPos)
						return extendingArms = false;
					if (grabbed.upYPos < o.downYPos && grabbed.upYPos > o.upYPos && grabbed.rightXPos < o.rightXPos && grabbed.rightXPos > o.leftXPos)
						return extendingArms = false;
					if (grabbed.downYPos < o.downYPos && grabbed.downYPos > o.upYPos && grabbed.rightXPos < o.rightXPos && grabbed.rightXPos > o.leftXPos)
						return extendingArms = false;
				}
			}
			return true;
		}
		
		return false;
	}
}
