using UnityEngine;
using System;
using System.Collections.Generic;


public class Obstacle
{
	
	
	public double xtile, ytile, depthshift, depth;
	public double speed, posX, posY;
	public double downY, upY, leftX, rightX;
	public double downYPos, upYPos, leftXPos, rightXPos;
	public float xiso, yiso;
	public bool upleft, downleft, upright, downright;
	public int type, width, currDir;
	public GameObject gfx;
	
	public Obstacle (int a): this(a, -100, -100)
	{
	}
	
	public Obstacle (int a, double x, double y)
	{
		type = a;
		switch(type) {
		case 1:
		case 2:
		case 3:
			gfx = OT.CreateObject("Bot1");
			setDir(3);
			width = 63;
			break;
		case 4:
			gfx = OT.CreateObject("Box1");
			width = 70;
			break;
		}
		setXY(x,y);
	}
	
	// Returns the name of the Tile currently occupied by the Obstacle
	public string onTile()	{
		string tileName = "tile_"+xtile+"_"+ytile;
		return tileName;
	}
	
	public void setXY(double x, double y) {
		xtile = x;
		ytile = y;
		
		posX = xtile * GameManager.getTileW ();
		posY = ytile * GameManager.getTileW ();
		setPos();
	}
	
	public void setX (double x)
	{
		posX = x;
		setPos();
	}

	public void setY (double y)
	{
		posY = y;
		setPos();
	}
	
	private void setPos() 
	{
		OTSprite os = gfx.GetComponent<OTSprite>();
		//calculate position in isometric view
		xiso = (float)(posX-posY);
		yiso = (float)((-posY-posX)/2);
		//update tob position
		Vector2 pos = new Vector2(xiso, yiso);
		os.position = pos;
		//calculate the tile where tobs center is
		xtile = Math.Floor((posX/GameManager.getTileW ()));
		ytile = Math.Floor((posY/GameManager.getTileW ()));
	}
	
	public void setDir(int dir)
	{
		currDir = dir;
		if (gfx != null) {
			OTSprite os = gfx.GetComponent<OTSprite>();
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
	
	public void setDepth(Dictionary<string, Tile> tileSheet)
	{
		OTSprite os = this.gfx.GetComponent<OTSprite>();
		
		foreach (var i in tileSheet) {
		OTSprite tos = i.Value.gfx.GetComponent<OTSprite>();
		if (tos.position.y > os.position.y || i.Value.walkable)
			tos.depth = 1;
		else
			tos.depth = -1;
		}
	}
}

