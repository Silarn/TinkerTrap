using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generator : TileClass, Tile {
	
	private Battery bat = null;
	private bool botCharge = false;
	private double endTime = 0.0;
	
	public Generator(int gx, int gy, int tSet) : base(gx, gy, tSet)
	{
		os.PlayOnce(this.GetType ().Name+tSet.ToString()+"_off");
		powered = false;
	}
	
	public override void interact(Obstacle a)
	{
		if (a.GetType () == typeof(Battery) && bat == null) {
			used = true;
			bat = (Battery)a;
			bat.charge(this);
		} else if (a.GetType () == typeof(Bot2)) {
			botCharge = true;
			endTime = Time.time+8;
		}
	}
	
	public void batteryLoss(Battery b) {
		if (b == bat)
		{
			used = true;
			bat = null;
		}
	}
	
	public override void act(List<Obstacle> objs) {
		if (botCharge == true) {
			if (powered == false) {
				os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_on");
				powered = true;
			}
			if (Time.time >= endTime) {
				endTime = 0;
				botCharge = false;
				if (bat == null) {
					powered = false;
					os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_off");
				}
			}
		}
		if (used) {
			if (!powered && bat.charging(this)) {
				os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_on");
				used = false;
				powered = true;
			} else if (bat == null && endTime == 0) {
				os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_off");
				used = false;
				powered = false;
			}
		}
	}
}
