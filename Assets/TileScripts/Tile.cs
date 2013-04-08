using UnityEngine;
using System;
using System.Collections.Generic;

public interface Tile {
	
	/*public Tile (int t, int gx, int gy) {
		
		switch (type) {
		case 2: // Pressure floor tile.
			locked = true;
			walkable = true;
			frameName = "Plate0";
			break;
		case 0: // Plain floor.
			frameName = "Ground0";
			walkable = true;
			break;
		case 8: // Finish zone floor tiles, needs graphic.
			walkable = true;
			frameName = "Ground0";
			break;
		case 4: // Left/Right Door
			walkable = false;
			frameName = "Door2";
			break;
		case 5: // Up/Down Door
			walkable = false;
			frameName = "Door0";
			break;
		case 1: // Plain wall.
			walkable = false;
			frameName = "Wall0";
			break;
		case 3:
		case 6: // Button wall needs 2nd graphic.
			locked = true;
			walkable = false;
			frameName = "WallB";
			break;
		case 7: // "See through" wall (so player is not hidden) needs updated graphic.
			walkable = false;
			frameName = "WallX";
			break;
		}
	}*/
	
	bool walkable ();
	
	bool walkable (Obstacle o);
	
	int depth {get;}
	
	OTAnimatingSprite graphic {
		get;
	}
	
	void addMessage(int bot, int lvl, string msg);
	
	void addConnection(int k, List<Tile> l, bool isSource);
	
	void addLock(int k, List<Tile> l, bool isSource);
	
	// Called to update status when activated by the player.
	void interact();
	
	// Called when requires activation by a specific Obs...
	void interact(Obstacle a);
	
	// Called to update status when acted upon by another object.
	void update();
	
	// Called with the game Update function.
	void act(List<Obstacle> objs);
	
	string myName();
	int xgrid{get;}
	int ygrid{get;}
}

