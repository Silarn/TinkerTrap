using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Button : TileClass, Tile {

    private double usedLast = 0.0;
	
	public AudioClip[] audioUse = new AudioClip[2];

	public Button(int gx, int gy, int tSet) : base(gx, gy, tSet)
	{
		audioUse[0] = Resources.Load("Blip_Select") as AudioClip;
		audioUse[1] = Resources.Load("Blip_Select_lower") as AudioClip;
		gfx.AddComponent<AudioSource>().clip = audioUse[0];

		os.PlayOnce(this.GetType ().Name+tSet.ToString()+"_off");
		locked = true;
	}
	
	public override void interact()
	{
        if (usedLast + 0.5 < Time.time)
        {
            usedLast = Time.time;
            used = true;
        }
	}
	
	public override bool isActivated() {
		foreach (List<Tile> conList in locks[0].Values)
			foreach (Tile t in conList)
				if (!((TileClass)t).locked && t != this)
					return false;
		return true;
	}
	
	public override void act(List<Obstacle> objs) {
		if (used) {
			if (isActivated ()) {
				foreach(List<Tile> i in connections[0].Values)
					foreach(Tile j in i)
						j.interact ();
				powered = !powered;
				locked = !locked;
				if (powered) {
					gfx.GetComponent<AudioSource>().PlayOneShot(audioUse[0]);
					os.PlayOnce(this.GetType ().Name+tileSet+"_on");
				} else {
					gfx.GetComponent<AudioSource>().PlayOneShot(audioUse[1]);
					os.PlayOnce(this.GetType ().Name+tileSet+"_off");
				}
//				gfx.GetComponent<AudioSource>().Play();
			}
			used = false;
		}
	}
}
