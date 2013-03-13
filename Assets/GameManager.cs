using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;

public class GameManager : MonoBehaviour {

	public Transform prefab;
	public int numberOfObjects;
	public GUISkin skin;
	public Texture2D logoTexture;
	public Texture2D bot1Texture;
	public Texture2D bot2Texture;
	public Texture2D bot3Texture;
	
	
	private int mapWidth = 0;
	private int mapHeight = 0;
	private static float tileW = 64;
																						
	private Dictionary<string, Tile> gameB = new Dictionary<string, Tile>();
	private Dictionary<int, List<Tile>> gameCons = new Dictionary<int, List<Tile>>();
	private Dictionary<int, List<Tile>> gameLocks = new Dictionary<int, List<Tile>>();
	private List<Obstacle> gameObs = new List<Obstacle>();
	
	private bool running = false;
	private bool selection = false;
	private bool paused = false;
	private int level = 0;
	
	public  float updateInterval = 0.5F;
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	private float fps = 30;
	
	private List<Player> players = new List<Player>();
	private int activeBot = 1;
	
	void Start () {
		timeleft = updateInterval;
		players.Add (new Bot1()); // Webelo (Red, Lifter)
		players.Add (new Bot2()); // Hob (Yellow, Hoverer)
		players.Add (new Bot3()); // Hisco (Green, Wheelie)
	}
	
	void TopMenu() {
	    //layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 200));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 200), "");
	   
	    //title
	    GUI.Label(new Rect(15, 10, 300, 38), "TinkerTrap");
	   
	    ///////main menu buttons
	    //game start button
	    if(GUI.Button(new Rect(55, 60, 180, 40), "Start game")) {
			selection = true;
	    }
	    if(GUI.Button(new Rect(55, 110, 180, 40), "Editor")) {
			Application.LoadLevel (1);
	    }
	    //quit button
	    if(GUI.Button(new Rect(55, 160, 180, 40), "Quit")) {
	    	Application.Quit();
	    }
	   
	    //layout end
	    GUI.EndGroup();
	}
	
	void SelectionMenu() {
	    //layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 400));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 400), "");
	   
	    //title
	    GUI.Label(new Rect(15, 10, 300, 68), "Which robot will you make?");
		
	    ///////menu buttons
	    //selection options
		if (level == 0) {
		    if(GUI.Button(new Rect(55, 100, 180, 40), "Webelo")) {
				running = true;
				selection = false;
				activeBot = 1;
				level++;
				BuildLevel ("level1");
		    }
		    if(GUI.Button(new Rect(55, 150, 180, 40), "Hob")) {
				running = true;
				selection = false;
				activeBot = 2;
				level++;
				BuildLevel ("level1");
		    }
		    if(GUI.Button(new Rect(55, 200, 180, 40), "Hisco")) {
				running = true;
				selection = false;
				activeBot = 3;
				level++;
				BuildLevel ("level1");
		    }
		    //return to main menu
		    if(GUI.Button(new Rect(55, 250, 180, 40), "Return")) {
		    	selection = false;
		    }
		}
	   
	    //layout end
	    GUI.EndGroup();
	}
	
	// If you hit Escape while playing the game!
	void PauseMenu() {
	    //layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 200));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 200), "");
	   
	    //logo picture
	    GUI.Label(new Rect(34, 10, 300, 40), "Game Paused!");
	   
	    ///////main menu buttons
	    //game start button
	    if(GUI.Button(new Rect(55, 60, 180, 40), "Resume Game")) {
			paused = false;
	    }
		//gogo editor
	    if(GUI.Button(new Rect(55, 110, 180, 40), "Editor")) {
			Application.LoadLevel (1);
	    }
	    //quit button
	    if(GUI.Button(new Rect(55, 160, 180, 40), "Quit")) {
	    	Application.Quit();
	    }
	   
	    //layout end
	    GUI.EndGroup();
	}
	
	void GameOverlay() {
		// Covers entire screen...
		GUI.BeginGroup (new Rect(0, 0, Screen.width, Screen.height));
		
		GUI.Box (new Rect(5, 5, 133, 261), bot1Texture);
		GUI.Box (new Rect(138, 5, 133, 261), bot2Texture);
		GUI.Box (new Rect(271, 5, 133, 261), bot3Texture);
		GUI.Label (new Rect(5, 266, 133, 20), "1");
		GUI.Label (new Rect(138, 266, 133, 20), "2");
		GUI.Label (new Rect(271, 266, 133, 20), "3");
		GUI.EndGroup ();
	}
	
	void BuildLevel(string map)
	{
		TextAsset file = (TextAsset) Resources.Load (map, typeof(TextAsset));
		OTSprite os;
		Vector2 pos;
		XmlReaderSettings settings = new XmlReaderSettings();
		settings.IgnoreWhitespace = true;
		using (XmlReader read = XmlReader.Create(new StringReader(file.text))) {
			int i = -1;
			int j = -1;
			bool consGroup = true;
			string squareName = "";
			while (read.Read ()) {
				if (read.IsStartElement ())
				{
					switch (read.Name)
					{
					case "width":
						read.Read ();
						mapWidth = read.ReadContentAsInt();
						break;
					case "height":
						read.Read ();
						mapHeight = read.ReadContentAsInt();
						break;
					case "y":
						j++;
						i = -1;
						break;
					case "x":
						i++;
						break;
					case "type":
						read.Read ();
						squareName = "tile_"+i+"_"+j;
						switch (read.ReadContentAsInt ()) {
						case 0:
							gameB.Add(squareName,(Tile)(new Floor(i,j,0)));
							break;
						case 1:
						case 7:
							gameB.Add (squareName, (Tile)(new Wall(i,j,0)));
							break;
						case 2:
							gameB.Add (squareName, (Tile)(new Plate(i,j,0)));
							break;
						case 3:
						case 6:
							gameB.Add (squareName, (Tile)(new Button(i,j,0)));
							break;
						case 4:
							gameB.Add (squareName, (Tile)(new Door(i,j,1)));
							break;
						case 5:
							gameB.Add (squareName, (Tile)(new Door(i,j,0)));
							break;
						}
						break;
					case "obs":
						read.Read ();
						Debug.Log ("Reading obstacles...");
						switch (read.ReadContentAsInt()) {
						case 1: // Player starting location, probably only for first level.
							Debug.Log ("Player found!");
							players[activeBot-1].setXY(i,j);
							gameObs.Add (players[activeBot-1]);
							break;
						case 4:
							Debug.Log ("Box found!");
							gameObs.Add (new Box(4, i, j));
							break;
						}
						break;
					case "connections":	
						consGroup = true;
						break;
					case "locks":
						consGroup = false;
						break;
					case "in":
						read.Read ();
						if (consGroup) {
							List<Tile> tilelist;
							int k = read.ReadContentAsInt();
							if (k != 0) {
								if (!gameCons.ContainsKey(k)) {
									tilelist = new List<Tile>();
									tilelist.Add(gameB[squareName]);
									gameCons.Add(k, tilelist);
								} else {
									tilelist = gameCons[k];
									tilelist.Add(gameB[squareName]);
								}
								gameB[squareName].addConnection(k, gameCons[k], false);
							}
						} else {
							List<Tile> tilelist;
							int k = read.ReadContentAsInt();
							if (k != 0) {
								if (!gameLocks.ContainsKey(k)) {
									tilelist = new List<Tile>();
									tilelist.Add(gameB[squareName]);
									gameLocks.Add(k, tilelist);
								} else {
									tilelist = gameLocks[k];
									tilelist.Add(gameB[squareName]);
								}
								gameB[squareName].addLock(k, gameLocks[k], false);
							}
						}
						break;
					case "out":
						read.Read ();
						if (consGroup) {
							List<Tile> tilelist;
							int k = read.ReadContentAsInt();
							if (k != 0) {
								if (!gameCons.ContainsKey(k)) {
									tilelist = new List<Tile>();
									tilelist.Add(gameB[squareName]);
									gameCons.Add(k, tilelist);
								} else {
									tilelist = gameCons[k];
									tilelist.Add(gameB[squareName]);
								}
								gameB[squareName].addConnection(k, gameCons[k], true);
							}
						} else {
							List<Tile> tilelist;
							int k = read.ReadContentAsInt();
							if (k != 0) {
								if (!gameLocks.ContainsKey(k)) {
									tilelist = new List<Tile>();
									tilelist.Add(gameB[squareName]);
									gameLocks.Add(k, tilelist);
								} else {
									tilelist = gameLocks[k];
									tilelist.Add(gameB[squareName]);
								}
								gameB[squareName].addLock(k, gameLocks[k], true);
							}
						}
						break;
					}
				}
			}
			read.Close ();
		}
	}
	
	void OnGUI () {
		if (!running) {
			//load GUI skin
		    GUI.skin = skin;
			
		    if (selection) //Goto Selection Menu
				SelectionMenu();
			else //Goto Main Menu
			    TopMenu();
		} else if (paused) {
				
		    //load GUI skin
		    GUI.skin = skin;
		   
		    //execute theFirstMenu function
		    PauseMenu();
		} else {
			GameOverlay();
		}
	}
	
	void Update() {
		
		// track fps to adjust player movement speed
	    timeleft -= Time.deltaTime;
	    accum += Time.timeScale/Time.deltaTime;
	    ++frames;
		if( timeleft <= 0.0 ) {
		    // display two fractional digits (f2 format)
			float fps = accum/frames;
			//	DebugConsole.Log(format,level);
	        timeleft = updateInterval;
	        accum = 0.0F;
	        frames = 0;
    	}
		
		if (running && !selection) {
			if (!paused) {
				
				//Directional movement. Should this be limited to one direction at a time?
				if (Input.GetKeyDown (KeyCode.E)) {
					DoPrimary ();
				}
				
				if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
				{
					moveChar(players[activeBot-1], 5.0, -1, 0);
					players[activeBot-1].setDir(3);
				}
				if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) 
				{
					moveChar(players[activeBot-1], 5.0, 1, 0);
					players[activeBot-1].setDir(1);
				}
		
				if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) 
				{
					moveChar(players[activeBot-1], 5.0, 0, -1);
					players[activeBot-1].setDir(0);
				}
				if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) 
				{
					moveChar(players[activeBot-1], 5.0, 0, 1);
					players[activeBot-1].setDir(2);
				}
				
				// Scan for player interaction. This probably needs updating for different bot abilites.
				
				if (Input.GetKeyDown(KeyCode.Space))
				{	interact();	}
				
				// Bot selection... eventually this will only be if you have multiple robots active!
				
				if (Input.GetKeyDown (KeyCode.Alpha1)) {
					if (activeBot != 1) {
						players[0].setX (players[activeBot-1].posX);
						players[0].setY (players[activeBot-1].posY);
						players[activeBot-1].setXY (-100,-100);
						players[0].setDir (players[activeBot-1].currDir);
						gameObs.Remove (players[activeBot-1]);
						gameObs.Add (players[0]);
						activeBot = 1;
					}
				}
				if (Input.GetKeyDown (KeyCode.Alpha2)) {
					if (activeBot != 2) {
						players[1].setX (players[activeBot-1].posX);
						players[1].setY (players[activeBot-1].posY);
						players[activeBot-1].setXY (-100,-100);
						players[1].setDir (players[activeBot-1].currDir);
						gameObs.Remove (players[activeBot-1]);
						gameObs.Add (players[1]);
						activeBot = 2;
					}
				}
				if (Input.GetKeyDown (KeyCode.Alpha3)) {
					if (activeBot != 3) {
						players[2].setX (players[activeBot-1].posX);
						players[2].setY (players[activeBot-1].posY);
						players[activeBot-1].setXY (-100,-100);
						players[2].setDir (players[activeBot-1].currDir);
						gameObs.Remove (players[activeBot-1]);
						gameObs.Add (players[2]);
						activeBot = 3;
					}
				}
				
				// Do round 1 of Tile updates. Final 'act' method called in LateUpdate();
				foreach (Tile t in gameB.Values) {
					t.update();
				}
			}
			
			// Bring up or close the pause menu!
			if (Input.GetKeyDown (KeyCode.Escape))
			{
				paused = !paused;
			}
		}
	}
	
	// Standard Unity Late Update, occurs after Update
	// Using this to redraw tile depths after positions are adjusted in the main Update
	// Also moves the GameManager object to remain centered on the player, which moves the child camera as well
	
	void LateUpdate() {
		if (running) {
			OTSprite p = players[activeBot-1].gfx.GetComponent<OTSprite>();
			Vector3 posUp = new Vector3(p.position.x, p.position.y, transform.localPosition.z);
			transform.localPosition = posUp;
			
			// Do round 2 of Tile updates. Initial 'update' method called in Update();
			foreach (Tile t in gameB.Values) {
				t.act(gameObs);
			}
			
			OTSprite ps = players[activeBot-1].gfx.GetComponent<OTSprite>();
			OTSprite iSp;
			OTSprite jSp;
		}
	}
	
	public static float getTileW()
	{	return tileW;	}
	
	private Tile FacingTile() {
		string tarTile = "";
		if (players[activeBot-1].currDir == 0) {
			tarTile = "tile_"+(int)(players[activeBot-1].xtile)+"_"+(int)(players[activeBot-1].ytile-1);
		}
		if (players[activeBot-1].currDir == 1) {
			tarTile = "tile_"+(int)(players[activeBot-1].xtile-1)+"_"+(int)(players[activeBot-1].ytile);
		}
		if (players[activeBot-1].currDir == 2) {
			tarTile = "tile_"+(int)(players[activeBot-1].xtile)+"_"+(int)(players[activeBot-1].ytile+1);
		}
		if (players[activeBot-1].currDir == 3) {
			tarTile = "tile_"+(int)(players[activeBot-1].xtile+1)+"_"+(int)(players[activeBot-1].ytile);
		}
		
		if (gameB.ContainsKey (tarTile))
			return gameB[tarTile];
		return null;
	}
	
	private void interact() {
		Tile target = FacingTile ();
		if (target != null)
			target.interact();
	}
	
	// Check if the active player can grab an obstacle,
	// if so, grab it!
	
	private void DoPrimary() {
		Tile target = FacingTile ();
		if (target == null)
			return;
		getMyCorners(players[activeBot-1], players[activeBot-1].posX, players[activeBot-1].posY);
		foreach (Obstacle iob in gameObs) {
			if (iob != players[activeBot-1]) {
				switch(players[activeBot-1].currDir) {
				case 0:
					getMyCorners (iob,iob.posX,iob.posY+5);
					if ( players[activeBot-1].upYPos < iob.downYPos && players[activeBot-1].downYPos > iob.upYPos &&
						players[activeBot-1].leftXPos < iob.rightXPos && players[activeBot-1].rightXPos > iob.leftXPos) {
						players[activeBot-1].primary(target, iob);
						return;
					}
					break;
				case 1:
					getMyCorners (iob,iob.posX-5,iob.posY);
					if ( players[activeBot-1].rightXPos > iob.leftXPos && players[activeBot-1].leftXPos < iob.rightXPos &&
						players[activeBot-1].upYPos < iob.downYPos && players[activeBot-1].downYPos > iob.upYPos) {
						players[activeBot-1].primary(target, iob);
						return;
					}
					break;
				case 2:
					getMyCorners (iob,iob.posX,iob.posY-5);
					if ( players[activeBot-1].downYPos > iob.upYPos && players[activeBot-1].upYPos < iob.downYPos &&
						players[activeBot-1].leftXPos < iob.rightXPos && players[activeBot-1].rightXPos > iob.leftXPos) {
						players[activeBot-1].primary(target, iob);
						return;
					}
					break;
				case 3:
					getMyCorners (iob,iob.posX+5,iob.posY);
					if ( players[activeBot-1].leftXPos < iob.rightXPos && players[activeBot-1].rightXPos > iob.leftXPos &&
						players[activeBot-1].upYPos < iob.downYPos && players[activeBot-1].downYPos > iob.upYPos) {
						players[activeBot-1].primary(target, iob);
						return;
					}
					break;
				}
				players[activeBot-1].primary(target);
			}
		}
	}
	
	// getMyCorners is called to detect the player position and dimensions, checking if movement will carry the player into a new tile.
	// This data is used by the moveChar function to decide where to move the player.
	
	private void getMyCorners(Obstacle tob, double px, double py)
	{
		tob.downYPos = py+tob.width/2-1;
		tob.upYPos = py;
		tob.leftXPos = px;
		tob.rightXPos = px+tob.width/2-1;
		//find corner points
		tob.downY = Math.Floor(tob.downYPos/tileW);
		tob.upY = Math.Floor(tob.upYPos/tileW);
		tob.leftX = Math.Floor(tob.leftXPos/tileW);
		tob.rightX = Math.Floor(tob.rightXPos/tileW);
		//check if they are walls
		if (tob.downY < mapHeight && tob.upY >= 0 &&
			tob.leftX >= 0 && tob.rightX < mapWidth) {
			tob.upleft = gameB["tile_"+tob.leftX+"_"+tob.upY].walkable();
			tob.downleft = gameB["tile_"+tob.leftX+"_"+tob.downY].walkable();
			tob.upright = gameB["tile_"+tob.rightX+"_"+tob.upY].walkable();
			tob.downright = gameB["tile_"+tob.rightX+"_"+tob.downY].walkable();
		}
		if (tob.downY >= mapHeight)
			tob.downleft = tob.downright = false;
		if (tob.upY < 0)
			tob.upleft = tob.upright = false;
		if (tob.leftX < 0)
			tob.upleft = tob.downleft = false;
		if (tob.rightX >= mapWidth)
			tob.upright = tob.downright = false;
		
	}
	
	// Move char handles all Obstacle movement. Currently this only means the player, but it is designed to pass a final speed backwards.
	// This is so that Obstacles can chain movement and reduce speed depending on the number of stacked Obstacles being pushed.
	
	private double moveChar(Obstacle tob, double speed, int dirx, int diry)
	{
		
		double speedAdj = 30/fps*speed;
		
		//vert movement
		//changing y with speed and taking old x
		getMyCorners(tob, tob.posX, tob.posY+speed*diry);
		
		//if going up
		if (diry == -1)
		{
			if (tob.upleft && tob.upright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1]) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos &&
							tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos) {
							speedAdj = moveChar(iob, speedAdj/2, dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, speedAdj/2, dirx, diry);
				tob.setY((float)(tob.posY+speedAdj*diry));
			}
			else
			{
				//hit the wall, place tob near the wall
				double yStart = tob.posY;
				tob.setY(((float)((tob.ytile)*tileW)));
				double yShift = yStart-tob.posY;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, yShift, dirx, diry);
				speedAdj = 0;
			}
		}
		//if going down
		if (diry == 1)
		{
			if (tob.downleft && tob.downright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1]) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.downYPos > iob.upYPos && tob.upYPos < iob.downYPos &&
							tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos) {
							speedAdj = moveChar(iob, speedAdj/2, dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, speedAdj/2, dirx, diry);
				tob.setY((float)(tob.posY+speedAdj*diry));
			}
			else
			{
				double yStart = tob.posY;
				tob.setY(((float)((tob.ytile+1)*tileW-(tob.width/2))));
				double yShift = tob.posY-yStart;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, yShift, dirx, diry);
				speedAdj = 0;
			}
		}
		//horizontal movement
		//changing x with speed and taking old y
		getMyCorners(tob, tob.posX+speed*dirx, tob.posY);
		//if going left
		if (dirx == -1)
		{
			if (tob.downleft && tob.upleft)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1]) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos &&
							tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos) {
							speedAdj = moveChar(iob, speedAdj/2, dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, speedAdj/2, dirx, diry);
				tob.setX((float)(tob.posX+speedAdj*dirx));
			}
			else
			{
				double xStart = tob.posX;
				tob.setX(((float)(tob.xtile*tileW)));
				double xShift = xStart-tob.posX;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, xShift, dirx, diry);
				speedAdj = 0;
			}
		}
		//if going right
		if (dirx == 1)
		{
			if (tob.upright && tob.downright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1]) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.rightXPos > iob.leftXPos && tob.leftXPos < iob.rightXPos &&
							tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos) {
							speedAdj = moveChar(iob, speedAdj/2, dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, speedAdj/2, dirx, diry);
				tob.setX((float)(tob.posX+speedAdj*dirx));
			}
			else
			{
				double xStart = tob.posX;
				tob.setX(((float)((tob.xtile+1)*tileW-(tob.width/2))));
				double xShift = tob.posX-xStart;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, xShift, dirx, diry);
				speedAdj = 0;
			}
		}
		
		return (speedAdj);
	}
}