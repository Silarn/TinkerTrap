using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generator : TileClass, Tile {
	
	private Battery bat = null;
	private bool botCharge = false;
	private double endTime = 0.0;
	private Bot3 b3;
	
	public AudioClip audioPowered;
	
	public Generator(int gx, int gy, int tSet) : base(gx, gy, tSet)
	{
		audioPowered = Resources.Load ("elec_buzz") as AudioClip;
		gfx.AddComponent<AudioSource>().clip = audioPowered;
		
		os.PlayOnce(this.GetType ().Name+tSet.ToString()+"_off");
		powered = false;
		locked = true;
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
		} else if (a.GetType() == typeof(Bot3)) {
			b3 = (Bot3)a;
			b3.charged = this;
			b3.charge = 0.0;
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
		if (b3 != null && b3.charged == this) {
			if (powered == false) {
				os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_on");
				powered = true;
			}
		} else {
			if (powered == true) {
				powered = false;
					os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_off");
			}
		}
		if (botCharge == true) {
			if (powered == false) {
				os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_on");
				powered = true;
				locked = false;
			}
			if (Time.time >= endTime) {
				endTime = 0;
				botCharge = false;
				if (bat == null) {
					powered = false;
					locked = true;
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
		if (powered) {
			if (!gfx.GetComponent<AudioSource>().isPlaying)
					gfx.GetComponent<AudioSource>().Play();
		} else {
			if (gfx.GetComponent<AudioSource>().isPlaying)
					gfx.GetComponent<AudioSource>().Stop();
		}
	}
}
