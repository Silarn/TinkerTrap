using System;
using UnityEngine;

public class Box : ObstacleClass, Obstacle
{
	public Box (int a) : base(a)
	{
	}
	
	public Box (int a, double x, double y) : base(a,x,y)
	{
	}
	
	public override int width {
		get { return 70; }
	}
}

