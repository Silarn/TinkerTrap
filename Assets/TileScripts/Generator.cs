using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Generator : TileClass, Tile {
	
	private Battery bat = null;
	private bool botCharge = false;
	private double endTime = 0.0;
	private Charge chargeTile = null;
	
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
			bat = (Battery)a;
			bat.charge(this);
		} else if (a.GetType () == typeof(Bot2)) {
			botCharge = true;
			endTime = Time.time+9;
		} else if (a.GetType() == typeof(Bot3)) {
			Bot3 b3 = (Bot3)a;
			if (b3.ChargeSource != null) {
				if (chargeTile == null) {
					b3.ChargeSource.setTile (this);
					chargeTile = b3.ChargeSource;
					b3.ChargeSource = null;
				}
			}
		}
	}
	
	public void batteryLoss(Battery b) {
		if (b == bat)
			bat = null;
	}
	
	public void removeCharge() {
		chargeTile = null;
	}
	
	public override void act(List<Obstacle> objs) {
		if (chargeTile != null || bat != null) {
			if (powered == false) {
				os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_on");
				powered = true;
				locked = false;
			}
		} else if (botCharge == false) {
			if (powered == true) {
				powered = false;
				locked = true;
				os.PlayOnce(this.GetType ().Name+tileSet.ToString()+"_off");
			}
		} else if (botCharge == true) {
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
		if (powered) {
			if (!gfx.GetComponent<AudioSource>().isPlaying)
					gfx.GetComponent<AudioSource>().Play();
		} else {
			if (gfx.GetComponent<AudioSource>().isPlaying)
					gfx.GetComponent<AudioSource>().Stop();
		}
	}
}
