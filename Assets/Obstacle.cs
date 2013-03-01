using UnityEngine;
using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class Obstacle
	{
		
		
		public double xtile, ytile, depthshift, depth;
		public double speed, posX, posY;
		public double downY, upY, leftX, rightX;
		public double downYPos, upYPos, leftXPos, rightXPos;
		public float xiso, yiso;
		public bool upleft, downleft, upright, downright;
		public int type, currDir;
		public GameObject gfx;
		
		public Obstacle (int a, GameObject b)
		{
			setDir(3);
			type = a;
			gfx = b;
		}
		
		public Obstacle (int a, GameObject b, double x, double y)
		{
			currDir = 3;
			type = a;
			gfx = b;
			xtile = x;
			ytile = y;
			
			posX = -xtile * LevelLoader.getTileW ();
			posY = -ytile * LevelLoader.getTileW ();
			setPos();
		}
	
		public void setX (float x)
		{
			posX = (double)x;
			setPos();
		}
	
		public void setY (float y)
		{
			posY = (double)y;
			setPos();
		}
		
		private void setPos() 
		{
			OTSprite os = gfx.GetComponent<OTSprite>();
			//calculate position in isometric view
			xiso = (float)(posX-posY);
			yiso = (float)((posX+posY)/2);
			//update tob position
			Vector2 pos = new Vector2(xiso, yiso);
			os.position = pos;
			//calculate the tile where tobs center is
			xtile = -Math.Floor((posX/(os.size.x/2)));
			ytile = -Math.Floor((posY/(os.size.x/2)));
		}
		
		public void setDir(int dir)
		{
			currDir = dir;
			if (gfx != null) {
				OTSprite os = gfx.GetComponent<OTSprite>();
				switch (currDir) {
				case 0:
					os.frameIndex = 2;
					break;
				case 3:
					os.frameIndex = 0;
					break;
				case 2:
					os.frameIndex = 1;
					break;
				case 1:
					os.frameIndex = 3;
					break;
				}
			}
		}
		
		public void setDepth(Dictionary<string, AssemblyCSharp.Tile> tileSheet)
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
}

