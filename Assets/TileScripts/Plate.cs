using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plate : Floor, Tile {
	
	private bool wasOn = false;
	
	public AudioClip[] audioState = new AudioClip[2];

	public Plate(int gx, int gy, int tSet) : base(gx, gy, tSet)
	{
		audioState[0] = Resources.Load("beep_low") as AudioClip;
		audioState[1] = Resources.Load("beep_high") as AudioClip;
		gfx.AddComponent<AudioSource>().clip = audioState[0];
		
		os.PlayLoop ("Plate"+tSet.ToString()+"_off");
		locked = true;
	}
	
	public override void act(List<Obstacle> objs) {
		bool occupied = false;
		foreach (Obstacle i in objs)	
//			if (i.xtile == gridx && i.ytile == gridy)	{
			if (i.onTile() == myName() || i.onTileBotL() == myName() || i.onTileBotR() == myName() || i.onTileTopR() == myName())	{
				occupied = true;
				break;	//stop once any Obstacle matches Tile
			}
		if (locked == occupied) {
			switch (locked) {
				case true:
					if (!wasOn) {
						gfx.GetComponent<AudioSource>().PlayOneShot(audioState[0],0.1f);
						wasOn = true;
					}
					os.PlayLoop ("Plate"+tileSet.ToString()+"_on");
					break;
				case false:
					if (wasOn) {
						gfx.GetComponent<AudioSource>().PlayOneShot(audioState[1],0.05f);
						wasOn = false;
					}
					os.PlayLoop ("Plate"+tileSet.ToString()+"_off");
					break;
			}
			locked = !locked;
			powered = !powered;
		}
	}
}
