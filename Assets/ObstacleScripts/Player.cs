using System;
using UnityEngine;

public abstract class Player : ObstacleClass, Obstacle
{
	public int currDir;
	
	public Player(int a) : base(a)
	{
	}
	
	public Player(int a, double x, double y) : base(a,x,y)
	{
	}
	
	public override int width {
		get { return 63; }
	}
	
	public virtual void setDir(int dir)
	{
		currDir = dir;
		if (gfx != null) {
			switch (currDir) {
			case 0:
				os.frameName = "Bot"+type+"UpRt";
				break;
			case 3:
				os.frameName = "Bot"+type+"UpLft";
				break;
			case 2:
				os.frameName = "Bot"+type+"DnLft";
				break;
			case 1:
				os.frameName = "Bot"+type+"DnRt";
				break;
			}
		}
	}
	
	public virtual void primary(Tile a) {}
	public virtual void primary(Tile a, Obstacle b) {}
}

