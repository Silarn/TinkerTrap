using UnityEngine;
using System;
using System.Collections.Generic;


public abstract class ObstacleClass
{
	public double tileX, tileY;
	public double xPos, yPos;
	public double[] dirTile = new double[4];
	public double[] dirPos = new double[4];
	public float xiso, yiso;
	public bool[] canGo = new bool[4];
	public int type;
	public GameObject gfx;
	public OTSprite os;
	
	public ObstacleClass (int a): this(a, -100, -100)
	{
	}
	
	public ObstacleClass (int a, double x, double y)
	{
		type = a;
		if (this.GetType ().IsSubclassOf (typeof(Player)))
			gfx = OT.CreateObject ("Bots");
		else
			gfx = OT.CreateObject ("Obs");
		os = gfx.GetComponent<OTSprite>();
		setXY(x,y);
	}
	
	
	public double posX { set { xPos = value; } get { return xPos; } }
	public double posY { set { yPos = value; } get { return yPos; } }
	public double xtile { set { tileX = value; } get { return tileX; } }
	public double ytile { set { tileY = value; } get { return tileY; } }
	public double downY { set {dirTile[2] = value;} get {return dirTile[2];} }
	public double upY { set {dirTile[0] = value;} get {return dirTile[0];} }
	public double leftX { set {dirTile[3] = value;} get {return dirTile[3];} }
	public double rightX { set {dirTile[1] = value;} get {return dirTile[1];} }
	public double downYPos { set {dirPos[2] = value;} get {return dirPos[2];} }
	public double upYPos { set {dirPos[0] = value;} get {return dirPos[0];} }
	public double leftXPos { set {dirPos[3] = value;} get {return dirPos[3];} }
	public double rightXPos { set {dirPos[1] = value;} get {return dirPos[1];} }
	public bool upright { get {return canGo[0];} set {canGo[0] = value;} }
	public bool downright { get {return canGo[1];} set {canGo[1] = value;} }
	public bool downleft { get {return canGo[2];} set {canGo[2] = value;} }
	public bool upleft { get {return canGo[3];} set {canGo[3] = value;} }
	
	public abstract int width { get; }
	
	public virtual bool inAction() { return false;}
	
	public virtual double getSpeed (double speed)
	{
		return Math.Floor (speed);
	}
	
	public virtual double getSpeed (double speed, Obstacle source)
	{
		if (this.GetType ().IsSubclassOf (typeof(Player)))	// No pushing Players. 
			return 0.0;
		return Math.Floor (speed/2);						// Speed is always Floored. If the adjustment
	}														// is lower than 1, no movement.
	
	// Returns the name of the Tile currently occupied by the Obstacle
	public string onTile()	{
		string tileName = "tile_"+xtile+"_"+ytile;
		return tileName;
	}
	
	public void setXY(double x, double y) {
		tileX = x;
		tileY = y;
		
		xPos = xtile * GameManager.getTileW ();
		yPos = ytile * GameManager.getTileW ();
		setPos();
	}
	
	public void setX (double x)
	{
		xPos = x;
		setPos();
	}

	public void setY (double y)
	{
		yPos = y;
		setPos();
	}
	
	private void setPos() 
	{
		//calculate position in isometric view
		xiso = (float)(posX-posY);
		yiso = (float)((-posY-posX)/2);
		//update tob position
		Vector2 pos = new Vector2(xiso, yiso);
		os.position = pos;
		os.position = pos;
		//calculate the tile where tobs center is
		xtile = Math.Floor((posX/GameManager.getTileW ()));
		ytile = Math.Floor((posY/GameManager.getTileW ()));
	}
	
	public OTSprite graphic {
		get { return os; }
	}
	
	public void setDepth(Dictionary<string, Tile> tileSheet)
	{
		foreach (var i in tileSheet) {
			if (i.Value.graphic.position.y > os.position.y || i.Value.walkable((Obstacle)this))
				i.Value.graphic.depth = 1;
			else
				i.Value.graphic.depth = -1;
		}
	}
}

