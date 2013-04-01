using System;
using UnityEngine;

public class Battery : ObstacleClass, Obstacle
{
	private Generator gen;
	
	public Battery (int a) : base(a)
	{
	}
	
	public Battery (int a, double x, double y) : base(a,x,y)
	{
	}

	public override int width {
		get { return 42; }
	}
	
	public override double getSpeed (double speed, Obstacle source)
	{
		return speed;
	}
	
	private void onMove() {
		if (gen != null) {
			gen.batteryLoss (this);
			gen = null;
		}
	}
	
	public void setX (double x)
	{
		onMove ();
		base.setX (x);
	}
	
	public void setY (double x)
	{
		onMove ();
		base.setY (x);
	}
	
	public void setXY (double x, double y)
	{
		onMove ();
		base.setXY (x, y);
	}
	
	public void charge (Generator a) {
		if (gen == null)
			gen = a;
	}
	public bool charging (Generator a) {
		if (gen == null) {
			return false;
		} else if ( a == gen )
			return true;
		else
			return false;
	}
}

