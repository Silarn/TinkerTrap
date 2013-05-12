using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Charge : Floor, Tile {
	
	bool occupied = false;
	Bot3 refBot3;
	
	public Charge(int gx, int gy, int tSet) : base(gx, gy, tSet) {}
	
	
	public override void act(List<Obstacle> objs) {
		if (refBot3 == null) {
			foreach (Obstacle i in objs)
				if (i.GetType() == typeof(Bot3))	{
					refBot3 = (Bot3)i;
					break;
			}
		} else {
			if (refBot3.onTile() == myName() || refBot3.onTileBotL() == myName() || refBot3.onTileBotR() == myName() || refBot3.onTileTopR() == myName())
				occupied = true;
			else
				occupied = false;
		}
	}
	
	public override void update ()
	{
		if (occupied)
			refBot3.Charge();
	}
}

