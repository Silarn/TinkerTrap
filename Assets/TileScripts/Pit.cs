using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pit : Floor, Tile {
	
	public Pit(int gx, int gy, int tSet) : base(gx, gy, tSet) {}
	
	
	public override bool walkable(Obstacle b) {
		if (b.GetType() == typeof(Bot2))
			if (((Bot2)b).inAction())
				return true;
		return false;
	}
}
