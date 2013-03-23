using UnityEngine;
using System;
using System.Collections.Generic;


public interface Obstacle
{
	/* Returns the name of the Tile currently occupied by the Obstacle
	public string onTile()	{
		string tileName = "tile_"+xtile+"_"+ytile;
		return tileName;
	}*/
	
	int width { get; }
	
	double posX { get; set; }
	double posY { get; set; }
	double xtile { get; set; }
	double ytile { get; set; }
	double downY { get; set; }
	double upY { get; set; }
	double leftX { get; set; }
	double rightX { get; set; }
	double downYPos { get; set; }
	double upYPos { get; set; }
	double leftXPos { get; set; }
	double rightXPos { get; set; }
	bool upright { get; set; }
	bool downright { get; set; }
	bool downleft { get; set; }
	bool upleft { get; set; }
	
	bool act();
	
	void setXY(double x, double y);
	
	void setX (double x);

	void setY (double y);
	
	double getSpeed (double speed);
	double getSpeed (double speed, Obstacle source);
	
	void setDepth(Dictionary<string, Tile> tileSheet);
}

