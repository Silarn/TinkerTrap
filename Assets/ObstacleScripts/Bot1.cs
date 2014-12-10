using System;
using System.Collections.Generic;
using UnityEngine;

public class Bot1 : Player, Obstacle
{
	public bool grabbing = false;
	public bool extendingArms = false;
	public bool retractArms = false;
	public bool startExtArms = false;
	public double extendDist = 0;
	public const int EXTEND_MAX = 192;
	public const int STEP_SIZE = 10;
	public Hands hands;
	public Obstacle grabbed;
	public AudioSource audioGrab;
	
	public Bot1 ()
	{
		hands = new Hands(this);
		
		audioGrab = gfx.AddComponent<AudioSource>();
		audioGrab.clip = Resources.Load ("clank") as AudioClip;
	}
	
	public Bot1 (double x, double y)
	{
//		arm.Add(new BotArm(x,y));
//		arm.Add(new BotArm(x,y));
			
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
		if (!grabbing && !startExtArms && !extendingArms && !retractArms) {
			base.update (input);
		}
		if (startExtArms && !os.isPlaying) {
			Debug.Log ("Show Hands");
			startExtArms = false;
			extendingArms = true;
			
//			hands.PlaceHands();
			hands.os.visible = true;
			hands.os.PlayOnce("Hands" + dirStr (currDir));
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
	public override void ResetTargetObstacle(Obstacle ob) {
		if (grabbing) { 
			Release();	}
		base.ResetTargetObstacle(ob);
	}
	
	public void secondary () {
		if (level > 0){
//			hands.os.visible = true;
			hands.setX (posX);
			hands.setY (posY);
			
			os.PlayOnce(this.GetType().Name + dirStr(currDir)+"_Ext");
//			hands.os.PlayOnce("Hands" + dirStr (currDir));
			
//			extendingArms = true;
			startExtArms = true;
			Debug.Log("extendingArms TRUE");
		}
	}
	
	public void Grab (Obstacle a) {
		if (!grabbing && !a.GetType().IsSubclassOf(typeof(Player))) {
			gfx.GetComponent<AudioSource>().Play ();
			grabbing = true;
			grabbed = a;
			if (!inAction())
				os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_Grab");
		}
	}
	
	public void Release() {
		grabbing = false;
		grabbed = null;
		if (!inAction())
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
	
	public double ExtendArmsStep() {
		if (extendingArms){
			extendDist += STEP_SIZE;
			if (extendDist > EXTEND_MAX) {
				extendingArms = false;
				retractArms = true;
			}
		}
		if (retractArms){
			extendDist -= STEP_SIZE;
			if (extendDist <= 0) {
				retractArms = false;
				hands.os.visible = false;
				if (grabbing)
					os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_Grab");
			}
		}
//		hands.setX(leftXPos);
//		hands.setY(upYPos);
		
		return extendDist;
	}
	
	public bool ExtendArmsAction (List<Obstacle> gObs) {			
		
		return true;
	}
}
