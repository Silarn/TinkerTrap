using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelLoader : MonoBehaviour {

	public Transform prefab;
	public int numberOfObjects;
	
	private static float tileW = 64;
	
	private int[,] map = new int[,] {{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 7}, {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7}, {1, 0, 0, 0, 8, 8, 8, 0, 0, 0, 0, 7}, {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7}, {1, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 7}, {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7}, {1, 0, 0, 0, 2, 0, 2, 0, 0, 0, 0, 7}, {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7}, {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7}, {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7}, {1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 7}, {1, 1, 1, 1, 1, 5, 1, 1, 3, 1, 1, 7}, {1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 7}, {1, 1, 1, 0, 1, 1, 1, 1, 2, 1, 1, 7}, {1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 7}, {1, 1, 1, 0, 4, 0, 0, 0, 0, 1, 1, 7}, {1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 7}, {1, 1, 1, 2, 1, 1, 1, 1, 5, 1, 1, 7}, {1, 1, 1, 0, 0, 4, 0, 0, 0, 1, 1, 7}, {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7}};
	private int[,] obsMap = new int[,] {{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 1, 0, 0, 2, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};
	private int[,][] connectionMap = new int[,][]
	{
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {13}, new int[] {12,13}, new int[] {11,12}, new int[] {10,11}, new int[] {9,10}, new int[] {8,9}, new int[] {7,8}, new int[] {6,7}, new int[] {5,6}, new int[] {4,5}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {4}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {2}, new int[] {0}, new int[] {0}, new int[] {1}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {3}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {1}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {2}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {3}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {1}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}},
		{new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}, new int[] {0}}
	};
	private Dictionary<string, AssemblyCSharp.Tile> gameB = new Dictionary<string, AssemblyCSharp.Tile>();
	private Dictionary<int, List<AssemblyCSharp.Tile>> gameCons = new Dictionary<int, List<AssemblyCSharp.Tile>>();
	
	private AssemblyCSharp.Obstacle player;
	
	void Start () {
		numberOfObjects = map.Length;
		
		int mapWidth = map.GetLength(0);
		int mapHeight = map.GetLength(1);

		OTSprite os;
		Vector2 pos;
		
		//loop to place tiles on stage
		for (int i = 0; i<mapHeight; ++i)
		{
			for (int j = 0; j<mapWidth; ++j)
			{	
				string squareName = "tile_"+j+"_"+i;
				
				gameB.Add(squareName, new AssemblyCSharp.Tile(map[j,i], j, i, OT.CreateObject("WorldTiles")));
					
				gameB[squareName].xiso = (-j+i)*tileW;
				gameB[squareName].yiso = (-j-i)*tileW/2F;
				
				pos = new Vector2 (gameB[squareName].xiso, gameB[squareName].yiso);
				os = gameB[squareName].gfx.GetComponent<OTSprite>();
				os.frameIndex = gameB[squareName].type;
				switch (os.frameIndex) {
				case 3:
					os.frameName = "Wall0";
					break;
				case 4:
					os.frameName = "WallB";
					break;
				case 5:
					os.frameName = "WallX";
					break;
				case 1:
					os.frameName = "Door0";
					break;
				}
				os.position = pos;
				
				if (gameB[squareName].walkable)
					os.depth = 1;
				else
					os.depth = -1;
				
				foreach (int k in connectionMap[j,i]) {
					List<AssemblyCSharp.Tile> tilelist;
					if (!gameCons.ContainsKey(k)) {
						tilelist = new List<AssemblyCSharp.Tile>();
						tilelist.Add(gameB[squareName]);
						gameCons.Add(k, tilelist);
					} else {
						tilelist = gameCons[k];
						tilelist.Add(gameB[squareName]);
					}
					gameB[squareName].addConnection(k, gameCons[k]);
				}
				
				/*for (k = 0; k < locksMap[i][j].length; k++) {
				    if (locksMap[i][j][0] != 0) {
                        if (gameBLock[String(locksMap[i][j][k])] == null) {
                            gameBLock[String(locksMap[i][j][k])] = new Array();
                        }
                        gameBLock[String(locksMap[i][j][k])].push(gameB[squareName]);
					}
                    gameB[squareName].setLock(locksMap[i][j][k]);
				}*/
				
				if (obsMap[j,i] != 0) {
					switch (obsMap[j,i]) {
						case 1:
							player = new AssemblyCSharp.Obstacle(0,OT.CreateObject("Bot1"), j, i); //Creates and sets player on tile
							break;
						/*case 2:
							gameBObs.push(new obstacle(2));
							gameBObs[gameBObs.length-1].xtile = i;
							gameBObs[gameBObs.length-1].ytile = j;
							break;*/
					}
//					player.setDepth (gameB);
				}
				
			}
		}
		//add the playeracter mc and insert at correct layer
//		os = player.gfx.GetComponent<OTSprite>();
//		
//		player.posX = -player.xtile*tileW;
//		player.posY = -player.ytile*tileW;
//		//calculate position in isometric view
//		player.xiso = (float)(player.posX-player.posY);
//		player.yiso = (float)((player.posX+player.posY)/2);
//		
//		pos = new Vector2(player.xiso, player.yiso);
//		os.position = pos;
//		//calculate the tile where players center is
//		player.xtile = -Math.Floor((player.posX)/32);
//		player.ytile = -Math.Floor((player.posY)/32);
		
//		foreach (var i in gameB) {
//			OTSprite tos = i.Value.gfx.GetComponent<OTSprite>();
//			if (tos.position.y > os.position.y || i.Value.walkable)
//				tos.depth = 1;
//			else
//				tos.depth = -1;
//		}
	}
	
	void Update() {
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
		{
			moveChar(player, 5.0, -1, 0);
			player.setDir(3);
		}
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) 
		{
			moveChar(player, 5.0, 1, 0);
			player.setDir(1);
		}

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) 
		{
			moveChar(player, 5.0, 0, -1);
			player.setDir(0);
		}
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) 
		{
			moveChar(player, 5.0, 0, 1);
			player.setDir(2);
		}
		if (Input.GetKey(KeyCode.Space))
		{
			interact();
		}
	}
	
	// Standard Unity Late Update, occurs after Update
	// Using this to redraw tile depths after positions are adjusted in the main Update
	// Also moves the LevelLoader object to remain centered on the player, which moves the child camera as well
	
	void LateUpdate() {
		OTSprite p = player.gfx.GetComponent<OTSprite>();
		Vector3 posUp = new Vector3(p.position.x, p.position.y, transform.localPosition.z);
		transform.localPosition = posUp;
		
		
		OTSprite os = player.gfx.GetComponent<OTSprite>();
		OTSprite tos;
		// Since Orthello automatically adjusts z depth based on y position for objects on the same layer,
		// all we have to do is make sure walls in front of the player are on a higher layer and walls behind the player are on a lower layer.
		// This may need some tweaking for movable obstacles, which use the same base class as the player (Obstacle).
		foreach (var i in gameB) {
			if (i.Value.gfx != null) {
				tos = i.Value.gfx.GetComponent<OTSprite>();
				if (tos.position.y > os.position.y || i.Value.walkable)
					tos.depth = 1;
				else
					tos.depth = -1;
			}
		}
	}
	
	public static float getTileW()
	{	return tileW;	}
	
	private void interact() {
		AssemblyCSharp.Tile atTile = null;
		
		if (player.currDir == 0) {
			atTile = gameB["tile_"+(player.ytile-1).ToString() +"_"+(player.xtile).ToString()];
		}
		if (player.currDir == 1) {
			atTile = gameB["tile_"+(player.ytile).ToString()+"_"+(player.xtile+1).ToString()];
		}
		if (player.currDir == 2) {
			atTile = gameB["tile_"+(player.ytile+1).ToString()+"_"+(player.xtile).ToString()];
		}
		if (player.currDir == 3) {
			atTile = gameB["tile_"+(player.ytile).ToString()+"_"+(player.xtile-1).ToString()];
		}
		
		/*if (atTile.lockGroup[0] != 0) {
			for (var i: int = 0; i < atTile.lockGroup.length; i++) {
				for (var j: int = 0; j < gameBLock[String(atTile.lockGroup[i])].length; j++) {
					if (gameBLock[String(atTile.lockGroup[i])][j].locked)
						return;
				}
			}
		}*/
		
		if (atTile != null)
			atTile.interact();
	}
	
	// getMyCorners is called to detect the player position and dimensions, checking if movement will carry the player into a new tile.
	// This data is used by the moveChar function to decide where to move the player.
	
	private void getMyCorners(AssemblyCSharp.Obstacle tob, double px, double py)
	{
		tob.downYPos = py+32-5;
		tob.upYPos = py+64;
		tob.leftXPos = px-5;
		tob.rightXPos = px+32;
		//find corner points
		tob.downY = -Math.Floor(tob.downYPos/tileW);
		tob.upY = -Math.Floor(tob.upYPos/tileW);
		tob.leftX = -Math.Floor(tob.leftXPos/tileW);
		tob.rightX = -Math.Floor(tob.rightXPos/tileW);
		//check if they are walls
		Console.WriteLine("{0}, {1}, {2}, {3}", tob.downY, tob.upY, tob.rightX, tob.leftX);
		if (tob.upY >= 0 || tob.downY < map.GetLength(1) ||
			tob.rightX >= 0 || tob.leftX < map.GetLength(0)) {
			tob.upleft = gameB["tile_"+tob.leftX+"_"+tob.upY].walkable;
			tob.downleft = gameB["tile_"+tob.leftX+"_"+tob.downY].walkable;
			tob.upright = gameB["tile_"+tob.rightX+"_"+tob.upY].walkable;
			tob.downright = gameB["tile_"+tob.rightX+"_"+tob.downY].walkable;
		}
		if (tob.upY < 0)
			tob.upleft = tob.upright = false;
		if (tob.downY >= map.GetLength(1))
			tob.downleft = tob.downright = false;
		if (tob.rightX < 0)
			tob.upright = tob.downright = false;
		if (tob.leftX >= map.GetLength(0))
			tob.upleft = tob.downleft = false;
		
	}
	
	// Move char handles all Obstacle movement. Currently this only means the player, but it is designed to pass a final speed backwards.
	// This is so that Obstacles can chain movement and reduce speed depending on the number of stacked Obstacles being pushed.
	
	private double moveChar(AssemblyCSharp.Obstacle tob, double speed, int dirx, int diry)
	{
		
		double speedAdj = speed;
		
		//vert movement
		//changing y with speed and taking old x
		getMyCorners(tob, tob.posX, tob.posY-speed*diry);
		
		//if going up
		if (diry == -1)
		{
			if (tob.upleft && tob.upright)
			{
				/*for (int i = 0; i < gameBObs.Length; i++) {
					if (gameBObs[i] != tob) {
						getMyCorners(gameBObs[i], gameBObs[i].posX, gameBObs[i].posY);
						if (Math.abs((gameBObs[i].upYPos+gameBObs[i].downYPos)/2 -
							 (tob.upYPos+tob.downYPos-1)/2) < (gameBObs[i].width+tob.width)/4 &&
							tob.xtile == gameBObs[i].xtile) {
							speedAdj = moveChar(gameBObs[i], speed/2, dirx, diry);
						}
					}
				}*/
				tob.setY((float)(tob.posY-speedAdj*diry));
			}
			else
			{
				//hit the wall, place tob near the wall
				tob.setY(-((float)((tob.ytile-1)*tileW/2+1)));
				speedAdj = 0;
			}
		}
		//if going down
		if (diry == 1)
		{
			if (tob.downleft && tob.downright)
			{
				/*for (i = 0; i < gameBObs.length; i++) {
					if (gameBObs[i] != tob) {
						getMyCorners(gameBObs[i], gameBObs[i].posX, gameBObs[i].posY);
						if (Math.abs((gameBObs[i].upYPos+gameBObs[i].downYPos)/2 -
							 (tob.upYPos+tob.downYPos-1)/2) <  (gameBObs[i].width+tob.width)/4 &&
							tob.xtile == gameBObs[i].xtile) {
							speedAdj = moveChar(gameBObs[i], speed/2, dirx, diry);
						}
					}
				}*/
				tob.setY((float)(tob.posY-speedAdj*diry));
			}
			else
			{
				tob.setY(-((float)((tob.ytile-1)*tileW/2+27)));
				speedAdj = 0;
			}
		}
		//horisontal movement
		//changing x with speed and taking old y
		getMyCorners(tob, tob.posX+speed*dirx, tob.posY);
		//if going left
		if (dirx == -1)
		{
			if (tob.downleft && tob.upleft)
			{
				/*for (i = 0; i < gameBObs.length; i++) {
					if (gameBObs[i] != tob) {
						getMyCorners(gameBObs[i], gameBObs[i].posX, gameBObs[i].posY);
						if (Math.abs((gameBObs[i].leftXPos+gameBObs[i].rightXPos)/2 -
							 (tob.leftXPos+tob.rightXPos-1)/2) <  (gameBObs[i].width+tob.width)/4 &&
							tob.ytile == gameBObs[i].ytile) {
							speedAdj = moveChar(gameBObs[i], speed/2, dirx, diry);
						}
					}
				}*/
				tob.setX((float)(tob.posX+speedAdj*dirx));
			}
			else
			{
				tob.setX(-((float)(tob.xtile*tileW/2-5)));
				speedAdj = 0;
			}
		}
		//if going right
		if (dirx == 1)
		{
			if (tob.upright && tob.downright)
			{
				/*for (i = 0; i < gameBObs.length; i++) {
					if (gameBObs[i] != tob) {
						getMyCorners(gameBObs[i], gameBObs[i].posX, gameBObs[i].posY);
						if (Math.abs((gameBObs[i].leftXPos+gameBObs[i].rightXPos)/2 -
							 (tob.leftXPos+tob.rightXPos-1)/2) <  (gameBObs[i].width+tob.width)/4 &&
							tob.ytile == gameBObs[i].ytile) {
							speedAdj = moveChar(gameBObs[i], speed/2, dirx, diry);
						}
					}
				}*/
				tob.setX((float)(tob.posX+speedAdj*dirx));
			}
			else
			{
				tob.setX(-((float)((tob.xtile)*tileW/2-31)));
				speedAdj = 0;
			}
		}
		
		return (speedAdj);
	}
}