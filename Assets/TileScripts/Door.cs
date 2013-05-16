using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Door : TileClass, Tile {

	private bool open = false;
	private bool botAccess = false;
	private double openUntil = 0.0;
	private float delayTime = 0f;
	private Bot3 b3;
	
	public Door(int gx, int gy, int tSet) : base(gx, gy, tSet) {
		os.frameName = "Door"+tileSet.ToString ()+"_op_00000";
		os.Stop ();
	}
	
	public override bool walkable() {
		if (os.isPlaying)
			return false;
		return open;
	}
	public override void interact(Obstacle a) {
		if (a.GetType() == typeof(Bot3)) {
			b3 = (Bot3)a;
			if (b3.charged != this && b3.charge > 0) {
				b3.charged = this;
				botAccess = true;
//				openUntil = Time.time+5;
				b3.charge = 0;
			}
		}
	}
	
	public override bool walkable (Obstacle o)
	{
		if (o.GetType () == typeof(Box))
			return false;
		return this.walkable ();
	}
	
	public override void update() {
		if (isActivated()) {
			used = true;
		} else {
			used = false;
		}
//		if (Time.time >= openUntil) {
//			botAccess = false;
//		}
		if (b3 != null && b3.charged != this) {
			botAccess = false;
		}
		if (open != powered && delayTime <= Time.time)
			powered = !powered;
	}
	
	public override void act(List<Obstacle> objs) {
		if ((used || botAccess) && !open) {
			os.PlayOnce("Door"+tileSet.ToString ());
			open = true;
			delayTime = Time.time + .4f;
		} else if ((!used && !botAccess) && open) {
			os.PlayOnceBackward("Door"+tileSet.ToString ());
			open = false;
			delayTime = Time.time + .6f;
		}
	}
}