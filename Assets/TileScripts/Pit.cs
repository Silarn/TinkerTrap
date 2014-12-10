using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pit : Floor, Tile {
	
	Bot1 b1; //Need to know which obstacle is being grabbed by Bot1, so this tile returns walkable true
	
	public Pit(int gx, int gy, int tSet) : base(gx, gy, tSet) {}
	
	
	public override bool walkable(Obstacle b) {
		if (b.GetType() == typeof(Bot2))
			if (((Bot2)b).inAction())
				return true;
		if (!b.GetType().IsSubclassOf(typeof(Player)))
			return true;

		return false;
	}
}
