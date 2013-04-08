using System;
using UnityEngine;

public abstract class Player : ObstacleClass, Obstacle
{
	public int currDir, animDir;
	protected bool animPlay = false;
	protected bool moveIntro = false;
	protected bool moveTry = false;
	protected bool moving = false;
	protected bool turning = false;
	protected bool idle = true;
	protected bool stopping = false;
	protected bool playstop = false;
	protected float stopTime = Time.time;
	public int level = -1;
	
	public Player() : base(1)
	{
		setDir (0);
	}
	
	public Player(double x, double y) : base(1,x,y)
	{
		setDir (0);
	}
	
	public override int width {
		get { return 63; }
	}
	
	public virtual void setDir(int dir)
	{
		currDir = dir;
		os.PlayLoop (this.GetType ().Name+dirStr (currDir)+"_Idle");
	}
	
	protected string dirStr(int a) {
		switch (a) {
		default:
			return "UpRt";
			break;
		case 1:
			return "DnRt";
			break;
		case 2:
			return "DnLft";
			break;
		case 3:
			return "UpLft";
			break;
		}
	}
	
	public virtual void primary(Tile a) {}
	public virtual void primary(Tile a, Obstacle b)
	{
		this.primary(a);
	}
	// ADD: Method to determine coordinates, currently in GameManager.moveChar
	// ADD: Method to determine coordinate area to perform a turn.
	
	public virtual void update(bool input) {}
}

