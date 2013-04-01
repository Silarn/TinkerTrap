using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Button : TileClass, Tile {

    private double usedLast = 0.0;

	public Button(int gx, int gy, int tSet) : base(gx, gy, tSet)
	{
		os.PlayOnce(this.GetType ().Name+tSet.ToString());
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
			/*if (active)
				//Button off state graphic goes here.
			else
				//Button on state graphic goes here.*/
			if (isActivated ()) {
				foreach(List<Tile> i in connections[0].Values)
					foreach(Tile j in i)
						j.interact ();
				powered = !powered;
				locked = !locked;
			}
			used = false;
		}
	}
}
