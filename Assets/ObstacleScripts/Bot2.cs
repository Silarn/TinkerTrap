using System;
using UnityEngine;

public class Bot2 : Player, Obstacle
{
	private bool hovering = false;
	private double targetX, targetY;
	private string hoverTargetTopL = "";
	private string hoverTargetBotR = "";
		
	public Bot2 ()
	{
	}

	public Bot2 (double x, double y)
	{
	}
	
	public void primary(Tile a, Tile b)
	{
		if (level >= 1 && !hovering) {
			if ( (a.GetType().IsSubclassOf(typeof(Floor)) || a.GetType() == typeof(Floor)) 
				&& b.GetType() != typeof(Pit) && b.walkable() ) {
				hoverTargetTopL = b.myName(); // Well that didn't work. Check for clearance using getCorners instead ?
				targetX = (double)b.xgrid();
				targetY = (double)b.ygrid();
				hovering = true;
			}
		}
	}
	
	public override bool inAction()
	{
		//Hover Conditions
		if (hovering){
			if ( (currDir == 1  || currDir == 2 ) && !onTile().Equals(hoverTargetTopL))
				return true;
			else if ( (currDir == 0  || currDir == 3 ) && !onTileBotR().Equals(hoverTargetTopL))
				return true;
			else 
				return hovering = false;
		} else {
//			hoverTarget = "";
			hovering = false;
			return false;
		}
		
	}
}
