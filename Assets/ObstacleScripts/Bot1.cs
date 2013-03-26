using System;
using UnityEngine;

public class Bot1 : Player, Obstacle
{
	public bool grabbing = false;
	public bool extendingArms = false;
	public Obstacle grabbed;
	
	public Bot1 () : base(1)
	{
	}
	
	public Bot1 (double x, double y) : base(1,x,y)
	{
	}
	
	public override void setDir(int dir)
	{
		if (!grabbing) {
			base.setDir(dir);
		}
	}
	
	public override void primary (Tile a)
	{
		if (grabbing)
			Release ();
	}
	
	public override void primary(Tile a, Obstacle b)
	{
		if (!b.GetType ().IsSubclassOf (typeof(Player))) {
			if (!grabbing)
				Grab (b);
			else
				Release ();
		}	
	}
	
	public void Grab (Obstacle a) {
		grabbing = true;
		grabbed = a;
	}
	
	public void Release() {
		grabbing = false;
		grabbed = null;
	}
	
	public override bool inAction() {
		if (extendingArms)
			return true;
		else
			return false;
	}
}
