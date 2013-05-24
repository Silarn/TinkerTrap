using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : ObstacleClass, Obstacle
{
	public int currDir = -1;
	public int animDir = -1;
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
	public bool onActiveElec = false;
	public Dictionary<string,int> pathDir = new Dictionary<string, int>();
	public List<string> pathOrder = new List<string>();
	
	public Player() : base(1)
	{
		os.PlayLoop (this.GetType ().Name + dirStr (currDir)+"_Idle");
		setDir (0);
	}
	
	public Player(double x, double y) : base(1,x,y)
	{
		os.PlayLoop (this.GetType ().Name + dirStr (currDir)+"_Idle");
		setDir (0);
	}
	
	public override int width {
		get { return 63; }
	}
	
	public virtual void setDir(int dir)
	{
		int startDir = currDir;
		if (animDir == -1)
			animDir = dir;
		else
			moveTry = true;
		currDir = dir;
		if (currDir != startDir)
			turning = true;
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
	public virtual void ResetTargetObstacle(Obstacle ob) {
		ob.setXY(ob.spawnX,ob.spawnY);
	}
	
	// ADD: Method to determine coordinates, currently in GameManager.moveChar
	// ADD: Method to determine coordinate area to perform a turn.
	
	public virtual void update(bool input) {
		if (moving || moveIntro) {
			if (!input) {
				stopping = true;
			}
		}
		if (moveTry && input)
			stopping = false;
		if (!os.isPlaying || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_MvStop") || os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Idle") ||
			os.animationFrameset.Equals (this.GetType ().Name + dirStr (animDir)+"_Move")) {
			animPlay = false;
			if (turning) {
				if (idle) {
					if (animDir == currDir-1 || animDir == currDir+3)
						os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRtSt");
					else if (animDir == currDir+1 || animDir == currDir-3)
						os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnLftSt");
					else if (Math.Abs (animDir-currDir) == 2)
						os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRvSt");
				} else {
					if (animDir == currDir-1 || animDir == currDir+3)
						os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRtMv");
					else if (animDir == currDir+1 || animDir == currDir-3)
						os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnLftMv");
					else if (Math.Abs (animDir-currDir) == 2)
						os.PlayOnce (this.GetType ().Name + dirStr (animDir)+"_TurnRvMv");
				}
				if (os.isPlaying)
					animDir = currDir;
				turning = false;
			} else if (stopping) {
				if (!os.isPlaying) {
					if (!playstop && !idle) {
						os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_MvStop");
						animDir = currDir;
						playstop = true;
					} else {
						os.PlayLoop (this.GetType ().Name + dirStr (currDir)+"_Idle");
						animDir = currDir;
						stopping = false;
						idle = true;
						playstop = false;
					}
				}
			} else if (moveTry) {
				if (idle) {
					os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_MvInt");
					animDir = currDir;
					idle = false;
				} else if (!os.isPlaying) {
					os.PlayOnce (this.GetType ().Name + dirStr (currDir)+"_Move");
					animDir = currDir;
					moving = true;
				}
				moveTry = false;
			}
		}
	}
}

