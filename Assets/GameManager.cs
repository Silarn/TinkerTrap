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
	private static float tileW = 128;
																						
	private Dictionary<string, Tile> gameB = new Dictionary<string, Tile>();
	private Dictionary<int, List<Tile>> gameConsOut = new Dictionary<int, List<Tile>>();
	private Dictionary<int, List<Tile>> gameConsIn = new Dictionary<int, List<Tile>>();
	private Dictionary<int, List<Tile>> gameLocksOut = new Dictionary<int, List<Tile>>();
	private Dictionary<int, List<Tile>> gameLocksIn = new Dictionary<int, List<Tile>>();
	private List<Obstacle> gameObs = new List<Obstacle>();
	
	private bool running = false;
	private bool selection = false;
	private int stageSelect = 0;
	private bool paused = false;
	private int level = 0;
	private bool showMessages = true;
	private string filePath = "save.xml";
	private Finish refFinish;
	private Tile refSpawn;
	
	private List<Player> players = new List<Player>();
	private int activeBot = 1;
	private List<KeyCode> activeInputs = new List<KeyCode>();
	
	void Start () {
		Time.fixedDeltaTime = 1/30f;
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
	
	void StageSelectMenu() {
		//layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 400));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 200), "");
		
		if(GUI.Button(new Rect(55, 100, 180, 40), "New Game")) {
			// Proceed to SelectionMenu
			stageSelect = 1;
		}
		if (File.Exists(filePath))
			if(GUI.Button(new Rect(55, 140, 180, 40), "Continue")) {
				// Grayed out if no save file exisits.
				// Loads game at last level w/ same bots/updrades
				if (File.Exists(filePath)) 
					stageSelect = 2;
			}
//		if(GUI.Button(new Rect(55, 180, 180, 40), "Stage Select")) {
//			
//			stageSelect = 3;
//		}
		if(GUI.Button(new Rect(55, 220, 180, 40), "Back")) {
			selection = false;
			stageSelect = 0;
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
		string bot1Text = "Webelo";
		string bot2Text = "Hob";
		string bot3Text = "Hisco";
		if (players[0].level > 0)
			bot1Text = "UPGRADE: Webelo";
		if (players[1].level > 0)
			bot2Text = "UPGRADE: Hob";
		if (players[2].level > 0)
			bot3Text = "UPGRADE: Hisco";
		if (!running) {
		    if(GUI.Button(new Rect(55, 100, 180, 40), bot1Text)) {
				running = true;
				selection = false;
				activeBot = 1;
				level++;
				players[0].level++;
//				BuildLevel ("level1");
				BuildLevel ("level"+level);
				Save ();
		    }
		    if(GUI.Button(new Rect(55, 150, 180, 40), bot2Text)) {
				running = true;
				selection = false;
				activeBot = 2;
				level++;
				players[1].level++;
//				BuildLevel ("level1");
				BuildLevel ("level"+level);
				Save ();
		    }
		    if(GUI.Button(new Rect(55, 200, 180, 40), bot3Text)) {
				running = true;
				selection = false;
				activeBot = 3;
				level++;
				players[2].level++;
//				BuildLevel ("level1");
				BuildLevel ("level"+level);
				Save ();
		    }
		    //return to main menu
			if (level == 0)
		    	if(GUI.Button(new Rect(55, 250, 180, 40), "Return")) {
//		    		selection = false;
					stageSelect = 0;
		    }
		}
	   
	    //layout end
	    GUI.EndGroup();
	}
	
	// If you hit Escape while playing the game!
	void PauseMenu() {
	    //layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 300));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 270), "");
	   
	    //logo picture
	    GUI.Label(new Rect(94, 15, 300, 40), "Game Paused!");
	   
	    ///////main menu buttons
	    //game start button
	    if(GUI.Button(new Rect(55, 60, 180, 40), "Resume Game")) {
			paused = false;
	    }
		//gogo editor
	    if(GUI.Button(new Rect(55, 110, 180, 40), "Editor")) {
			Application.LoadLevel (1);
	    }
		//toggle tutorial
		if (showMessages) {
			if(GUI.Button(new Rect(55, 160, 180, 40), "Tutorial: On")) {
				showMessages = false;		}
		} else {
			if(GUI.Button(new Rect(55, 160, 180, 40), "Tutorial: Off")) {
				showMessages = true; 	}
		}
	    //quit button
	    if(GUI.Button(new Rect(55, 210, 180, 40), "Quit")) {
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
		
		if (showMessages && ((TileClass)gameB[players[activeBot-1].onTile()]).messages[activeBot-1].Count > 0) {
			GUI.Box (new Rect(Screen.width-(Screen.width/2)-5, 100, 300,(((TileClass)gameB[players[activeBot-1].onTile()]).messages[activeBot-1].Count * 60))," ");
			int stepMsgs = 0;
			foreach (KeyValuePair<int,string> kvp in ((TileClass)gameB[players[activeBot-1].onTile()]).messages[activeBot-1]) {
				if (players[activeBot-1].level >= kvp.Key) {
					GUI.Label(new Rect(Screen.width-(Screen.width/2),100 + stepMsgs,290,60), kvp.Value);
					stepMsgs += 60;
				}
			}
		}
		GUI.EndGroup ();
	}
	
	void Save()
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		using (XmlWriter writer = XmlWriter.Create(filePath,settings)) {
			writer.WriteStartDocument ();
			writer.WriteStartElement ("document");
			writer.WriteElementString ("Bot1", players[0].level.ToString());
			writer.WriteElementString ("Bot2", players[1].level.ToString());
			writer.WriteElementString ("Bot3", players[2].level.ToString());
			writer.WriteElementString ("Level", level.ToString());
			writer.WriteElementString ("Tutorial", showMessages.ToString());
			writer.WriteEndDocument ();
		};
//		PlayerPrefs.SetInt("Last Level", level);
//		PlayerPrefs.SetInt("Bot1 Level", players[0].level);
//		PlayerPrefs.SetInt("Bot2 Level", players[1].level);
//		PlayerPrefs.SetInt("Bot3 Level", players[2].level);
	}
	
	void LoadFromSave()
	{
		XmlReaderSettings settings = new XmlReaderSettings();
		settings.IgnoreWhitespace = true;
		
		using (XmlReader read = XmlReader.Create (filePath, settings)) {
			while (read.Read ()) {
				if (read.IsStartElement ())
				{	
					switch (read.Name)
					{
					case "Bot1":
						read.Read ();
						players[0].level = read.ReadContentAsInt();
						break;
					case "Bot2":
						read.Read ();
						players[1].level = read.ReadContentAsInt();
						break;
					case "Bot3":
						read.Read ();
						players[2].level = read.ReadContentAsInt();
						break;
					case "Level":
						read.Read ();
						level = read.ReadContentAsInt();
						break;
					case "Tutorial":
						read.Read ();
						showMessages = read.ReadContentAsBoolean();
						break;
					}
				}
			}
			read.Close ();
		}
		if (players[0].level > 0)
			activeBot = 1;
		else if (players[1].level > 0)
			activeBot = 2;
		else 
			activeBot = 3;
		running = true;
		selection = false;
		BuildLevel ("level"+level);
	}
	void ClearLevel()
	{
		if (gameB.Count > 0) {
			foreach (KeyValuePair<string,Tile> o in gameB) {
				Destroy(o.Value.graphic);
			}
			foreach (Obstacle ob in gameObs) {
				if (ob.GetType() != typeof(Player)) {
					Destroy(ob.graphic); 
				} else {
					ob.setXY(-100,-100); // Remove Player object from view
				}
			}
			gameB.Clear();
			gameConsIn.Clear();
			gameConsOut.Clear ();
			gameLocksIn.Clear();
			gameLocksOut.Clear ();
			gameObs.Clear();
		}
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
			int setBot = -1;
			int lvl = 0;
			bool consGroup = true;
			string squareName = "";
			string tileType = "Wall";
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
						tileType = read.ReadContentAsString ();
						break;
					case "tset":
						read.Read ();
						var tempTile = Type.GetType (tileType);
						gameB.Add (squareName, (Tile)Activator.CreateInstance(tempTile, new object[] {i,j,read.ReadContentAsInt ()}));
						if (tileType == "Finish")
							refFinish = (Finish)gameB[squareName];
						break;
					case "obs":
						if (!read.IsEmptyElement) {
							read.Read ();
							switch (read.ReadContentAsString()) {
							case "Spawn": // Player starting location, probably only for first level.
								refSpawn = gameB[squareName];
								players[activeBot-1].setXY(i,j);
								gameObs.Add (players[activeBot-1]);
								break;
							case "Box":
								gameObs.Add (new Box(1, i, j));
								break;
							case "Battery":
								gameObs.Add (new Battery(1, i, j));
								break;
							}
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
								if (!gameConsIn.ContainsKey(k)) {
									tilelist = new List<Tile>();
									tilelist.Add(gameB[squareName]);
									gameConsIn.Add(k, tilelist);
								} else {
									tilelist = gameConsIn[k];
									tilelist.Add(gameB[squareName]);
								}
							}
						} else {
							List<Tile> tilelist;
							int k = read.ReadContentAsInt();
							if (k != 0) {
								if (!gameLocksIn.ContainsKey(k)) {
									tilelist = new List<Tile>();
									tilelist.Add(gameB[squareName]);
									gameLocksIn.Add(k, tilelist);
								} else {
									tilelist = gameLocksIn[k];
									tilelist.Add(gameB[squareName]);
								}
							}
						}
						break;
					case "out":
						read.Read ();
						if (consGroup) {
							List<Tile> tilelist;
							int k = read.ReadContentAsInt();
							if (!gameConsOut.ContainsKey(k)) {
								tilelist = new List<Tile>();
								tilelist.Add(gameB[squareName]);
								gameConsOut.Add(k, tilelist);
							} else {
								tilelist = gameConsOut[k];
								tilelist.Add(gameB[squareName]);
							}
						} else {
							List<Tile> tilelist;
							int k = read.ReadContentAsInt();
							if (!gameLocksOut.ContainsKey(k)) {
								tilelist = new List<Tile>();
								tilelist.Add(gameB[squareName]);
								gameLocksOut.Add(k, tilelist);
							} else {
								tilelist = gameLocksOut[k];
								tilelist.Add(gameB[squareName]);
							}
						}
						break;
					case "tutorial":
						break;
					case "bot1":
						setBot = 0;
						break;
					case "bot2":
						setBot = 1;
						break;
					case "bot3":
						setBot = 2;
						break;
					case "level":
						read.Read ();
						lvl = int.Parse(read.Value);
						break;
					case "msg":
						read.Read ();
						string msg = read.Value;
						// Set msg to tile
						gameB[squareName].addMessage(setBot, lvl, msg);
						break;
					}
				}
			}
			read.Close ();
			foreach (int k in gameConsIn.Keys) {
				foreach (Tile t in gameConsIn[k]) {
					t.addConnection (k, gameConsIn[k], false);
					if (gameConsOut.ContainsKey (k))
						t.addConnection (k, gameConsOut[k], true);
				}
			}
			foreach (int k in gameLocksIn.Keys) {
				foreach (Tile t in gameLocksIn[k]) {
					t.addLock (k, gameLocksIn[k], false);
					if (gameLocksOut.ContainsKey (k))
						t.addLock (k, gameLocksOut[k], true);
				}
			}
		}
	}
	
	void OnGUI () {
		if (!running) {
			//load GUI skin
		    GUI.skin = skin;
			
		    if (selection) { //Goto Selection Menu
				if (stageSelect == 0)
					StageSelectMenu();
				else if (stageSelect == 1)
					SelectionMenu();
				else if (stageSelect == 2)
					LoadFromSave();
			} else //Goto Main Menu
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
		if (running && !selection) {
			if (!paused) {
				
				//Directional movement. Should this be limited to one direction at a time?
				if (Input.GetKeyDown(KeyCode.E))
				{	DoPrimary (); }
				
				// Scan for player interaction. This probably needs updating for different bot abilites.
				
				if (Input.GetKeyDown(KeyCode.Space))
				{	interact();	}
				
				// Bot selection... eventually this will only be if you have multiple robots active! (Remove comment below)
				
				if (Input.GetAxis("select1") == 1.0) {
					if (activeBot != 1 /*&& players[0].level >= 0*/) {
						if (!gameObs.Contains(players[0]) && TileClear(refSpawn.myName())) {
							players[0].setXY (refSpawn.xgrid,refSpawn.ygrid);
	//						players[0].setY (players[activeBot-1].posY);
	//						players[activeBot-1].setXY (-100,-100);
							players[0].setDir (0);
	//						gameObs.Remove (players[activeBot-1]);
							gameObs.Add (players[0]);
						}
						if (gameObs.Contains(players[0]))
							activeBot = 1;
					}
				}
				if (Input.GetAxis("select2") == 1.0) {
					if (activeBot != 2 /*&& players[1].level >= 0*/) {
						if (!gameObs.Contains(players[1]) && TileClear(refSpawn.myName())) {
							players[1].setXY (refSpawn.xgrid,refSpawn.ygrid);
	//						players[1].setY (players[activeBot-1].posY);
	//						players[activeBot-1].setXY (-100,-100);
							players[1].setDir (0);
	//						gameObs.Remove (players[activeBot-1]);
							gameObs.Add (players[1]);
							}
						if (gameObs.Contains(players[1]))
							activeBot = 2;
					}
				}
				if (Input.GetAxis("select3") == 1.0) {
					if (activeBot != 3 /*&& players[2].level >= 0*/) {
						if (!gameObs.Contains(players[2]) && TileClear(refSpawn.myName())) {
							players[2].setXY (refSpawn.xgrid,refSpawn.ygrid);
	//						players[2].setY (players[activeBot-1].posY);
	//						players[activeBot-1].setXY (-100,-100);
							players[2].setDir (players[activeBot-1].currDir);
	//						gameObs.Remove (players[activeBot-1]);
							gameObs.Add (players[2]);
						}
						if (gameObs.Contains(players[2]))
							activeBot = 3;
					}
				}
			}
			
			// Bring up or close the pause menu!
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				paused = !paused;
			}
		}
	}

	void FixedUpdate() {
		
		if (running && !selection) {
			if (!paused) {
				bool movement = false;
				if (!players[activeBot-1].inAction()) {
					double speed = 5;
					//Directional movement. Should this be limited to one direction at a time?
					/*if (((activeMovement.Contains(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))) 
						&& ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) || (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))))
						speed *= 0.7071;*/
					if (Input.GetAxis("horizontal") != 0.0) {
						moveChar(players[activeBot-1], speed, (int)(Math.Round(Input.GetAxis("horizontal"),MidpointRounding.AwayFromZero)), 0);
						movement = true;
					} else if (Input.GetAxis("vertical") != 0.0) {
						moveChar(players[activeBot-1], speed, 0, (int)(Math.Round(Input.GetAxis("vertical"),MidpointRounding.AwayFromZero)));
						movement = true;
					}
					
					players[activeBot-1].update (movement);
				}
				
				// Do round 1 of Tile updates. Final 'act' method called in LateUpdate();
				foreach (Tile t in gameB.Values) {
					t.update();
				}
				
				// Frame/time stepped move actions of activeBot here
				if (players[activeBot - 1].inAction() ) {
					if (players[activeBot-1].GetType () != typeof(Bot1)) {
						if (players[activeBot - 1].currDir == 0)
							moveChar(players[activeBot - 1],5,0,-1);
						else if (players[activeBot - 1].currDir == 1)
							moveChar(players[activeBot - 1],5,1,0);
						else if (players[activeBot - 1].currDir == 2)
							moveChar(players[activeBot - 1],5,0,1);
						else if (players[activeBot - 1].currDir == 3)
							moveChar(players[activeBot - 1],5,-1,0);
					}
				}
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
			
			// Check if level is complete
			if (refFinish.LevelComplete() == true) {
				//Do a thing
				running = false;
				selection = true;
				ClearLevel();
			}
			
			List<Obstacle> orderedObs = new List<Obstacle>(gameObs.Count);
			foreach (Obstacle i in gameObs) {
				int insertAfter = -1;
				for (int j = 0; j < orderedObs.Capacity; j++) {
					if (orderedObs.Count <= j)
						break;
					if (orderedObs[j].depth <= i.depth) // If the object is definitely 'above' the current item in the list, it needs to be inserted after.
						insertAfter = j;
				}
				if (orderedObs.Count-1 <= insertAfter)
					orderedObs.Add (i); //It's at the end of the list, add a new entry.
				else
					orderedObs.Insert (insertAfter+1, i); //It's within the list, insert it.
			}
			int depthLevel = -998; // -1000 is causing things to draw behind the camera. Silly Orthello.
			foreach (Obstacle i in orderedObs) {
				i.graphic.depth = depthLevel;
				depthLevel += 2; // Later obs in this list should be further back in space. But we need layers in between for the Tiles.
			}
			foreach (Tile i in gameB.Values) {
				if (i.walkable()) // Walkable tiles are always on the bottom.
					i.graphic.depth = 1000;
				else { // Similar checks to the obstacles above.
					foreach (Obstacle j in orderedObs) {
						if (j.depth >= i.depth ) {
							i.graphic.depth = j.graphic.depth-1;
							break;
						}
						if (orderedObs.IndexOf(j) == orderedObs.Count-1)
							i.graphic.depth = j.graphic.depth+1;
					}
				}
			}
		}
	}
	
	public static float getTileW()
	{	return tileW/2;	}
	
	// Determine if a Tile has an obstacle present
	private bool TileClear(string name) {
		if (!gameB.ContainsKey(name))
			return false;
		else {
			foreach (Obstacle ob in gameObs) {
				if (ob.onTile() == name || ob.onTileBotR() == name)
					return false;
			}
			return true;
		}
	}
	
	private Tile FacingTile(bool fromLeft, int distance) {
		string tarTile = "";
		if (fromLeft) {
			if (players[activeBot-1].currDir == 0) {
				tarTile = "tile_"+(int)(players[activeBot-1].leftX)+"_"+(int)(players[activeBot-1].upY-distance);
			} else if (players[activeBot-1].currDir == 1) {
				tarTile = "tile_"+(int)(players[activeBot-1].rightX+distance)+"_"+(int)(players[activeBot-1].upY);
			} else if (players[activeBot-1].currDir == 2) {
				tarTile = "tile_"+(int)(players[activeBot-1].leftX)+"_"+(int)(players[activeBot-1].downY+distance);
			} else if (players[activeBot-1].currDir == 3) {
				tarTile = "tile_"+(int)(players[activeBot-1].leftX-distance)+"_"+(int)(players[activeBot-1].upY);
			}
		} else {
			if (players[activeBot-1].currDir == 0) {
				tarTile = "tile_"+(int)(players[activeBot-1].rightX)+"_"+(int)(players[activeBot-1].upY-distance);
			} else if (players[activeBot-1].currDir == 1) {
				tarTile = "tile_"+(int)(players[activeBot-1].rightX+distance)+"_"+(int)(players[activeBot-1].downY);
			} else if (players[activeBot-1].currDir == 2) {
				tarTile = "tile_"+(int)(players[activeBot-1].rightX)+"_"+(int)(players[activeBot-1].downY+distance);
			} else if (players[activeBot-1].currDir == 3) {
				tarTile = "tile_"+(int)(players[activeBot-1].leftX-distance)+"_"+(int)(players[activeBot-1].downY);
			}
		}
		if (gameB.ContainsKey (tarTile))
			return gameB[tarTile];
		return null;
	}
	
	private void interact() {
		Tile target = FacingTile (true,1);
		if (target != null)
			target.interact();
	}
	
	// Check if the active player can grab an obstacle,
	// if so, grab it!
	
	private void DoPrimary() {
		if (players[activeBot-1].GetType() == typeof(Bot2)) {
			Tile left1 = FacingTile (true,1);
			Tile left2 = FacingTile (true,2);
			Tile right1 = FacingTile (false,1);
			Tile right2 = FacingTile (false,2);
			if (left1 == null || left2 == null || right1 == null || right2 == null)
				return;
			else {
				// Check that no obstacle is on the ending tiles
				if (TileClear(left2.myName()) && TileClear(right2.myName())) {
					((Bot2)players[activeBot-1]).primary(left1,left2,right1,right2);
					return;
				} else {
					return;
				}
//				foreach (Obstacle iob in gameObs) {
//					if (iob != players[activeBot-1]) {
//						string topLCorner = "tile_"+(int)(iob.xtile)+"_"+(int)(iob.ytile);
//						string botRCorner = "tile_"+(int)(iob.rightX)+"_"+(int)(iob.downY);
//						if (topLCorner == left2.myName() || botRCorner == right2.myName())
//							return;
//					}
//				}
//				((Bot2)players[activeBot-1]).primary(left1,left2,right1,right2);
//				return;
			}
		}
		Tile target = FacingTile (true,1);
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
			}
		}
		players[activeBot-1].primary(target);
	}
	
	private bool CanThisTurn(Obstacle ob) {
		if (ob.GetType() == typeof(Bot3)) {
			double[] checkPos = ((Bot3)ob).turnCorner;
			
			double downY = Math.Floor(checkPos[2]/(tileW/2));
			double upY = Math.Floor(checkPos[0]/(tileW/2));
			double leftX = Math.Floor(checkPos[3]/(tileW/2));
			double rightX = Math.Floor(checkPos[1]/(tileW/2));
			
			if (downY < mapHeight && upY >= 0 && leftX >= 0 && rightX < mapWidth) {
				if (!gameB["tile_"+leftX+"_"+upY].walkable(ob))
					return false;
				if (!gameB["tile_"+leftX+"_"+downY].walkable(ob))
					return false;
				if (!gameB["tile_"+rightX+"_"+upY].walkable(ob))
					return false;
				if (!gameB["tile_"+rightX+"_"+downY].walkable(ob))
					return false;
			}
			if (downY >= mapHeight)
				return false;
			if (upY < 0)
				return false;
			if (leftX < 0)
				return false;
			if (rightX >= mapWidth)
				return false;
			
		
			foreach (Obstacle o in gameObs) {
				if (o != ob) {
					getMyCorners(o, o.posX, o.posY);
					if (upY < o.upY && upY > o.downY && leftX < o.rightX && leftX > o.leftX)
						return false;
					if (upY < o.upY && upY > o.downY && rightX < o.rightX && rightX > o.leftX)
						return false;
					if (downY < o.upY && downY > o.downY && leftX < o.rightX && leftX > o.leftX)
						return false;
					if (downY < o.upY && downY > o.downY && rightX < o.rightX && rightX > o.leftX)
						return false;
				}
			}
		}
		return true;
	}
	
	// getMyCorners is called to detect the player position and dimensions, checking if movement will carry the player into a new tile.
	// This data is used by the moveChar function to decide where to move the player.
	
	private void getMyCorners(Obstacle tob, double px, double py)
	{
		tob.SetCorners();
		tob.downYPos += (py - tob.posY);
		tob.upYPos += (py - tob.posY);
		tob.leftXPos += (px - tob.posX);
		tob.rightXPos += (px - tob.posX);
		
//		tob.downYPos = py+tob.width/2-1;
//		tob.upYPos = py;
//		tob.leftXPos = px;
//		tob.rightXPos = px+tob.width/2-1;
		//find corner points
		tob.downY = Math.Floor(tob.downYPos/(tileW/2));
		tob.upY = Math.Floor(tob.upYPos/(tileW/2));
		tob.leftX = Math.Floor(tob.leftXPos/(tileW/2));
		tob.rightX = Math.Floor(tob.rightXPos/(tileW/2));
		//check if they are walls
		if (tob.downY < mapHeight && tob.upY >= 0 &&
			tob.leftX >= 0 && tob.rightX < mapWidth) {
			tob.upleft = gameB["tile_"+tob.leftX+"_"+tob.upY].walkable(tob);
			tob.downleft = gameB["tile_"+tob.leftX+"_"+tob.downY].walkable(tob);
			tob.upright = gameB["tile_"+tob.rightX+"_"+tob.upY].walkable(tob);
			tob.downright = gameB["tile_"+tob.rightX+"_"+tob.downY].walkable(tob);
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
		
		double baseSpeed = tob.getSpeed (speed);
		double speedAdj = baseSpeed;
		
		if (baseSpeed == 0)
			return 0.0;
		
		//vert movement
		//changing y with speed and taking old x
		getMyCorners(tob, tob.posX, tob.posY+baseSpeed*diry);
		
		//if going up
		if (diry == -1)
		{
			if (tob.GetType().IsSubclassOf(typeof(Player))) {
				if(tob.GetType() == typeof(Bot3)) {
					Bot3 b3 = (Bot3)tob;
					if (b3.currDir != 0) {
						if (!CanThisTurn(tob)) {
							if(b3.currDir == 2)
								speedAdj /= 2;
							else
								return speedAdj = 0.0;
						} else {
							b3.setDir(0);
							return 0.0;
						}
					}
				} else {
					((Player)tob).setDir(0);
				}
			}
			if (tob.upleft && tob.upright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1] && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos &&
							tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
				// Need alt case where if 'something' then baseSpeed is used (to ignore obstacles)
				tob.setY((float)(tob.posY+speedAdj*diry));
			}
			else
			{
				//hit the wall, place tob near the wall
				double yStart = tob.posY;
				tob.setY(((float)((tob.ytile)*(tileW/2))));
				double yShift = yStart-tob.posY;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, yShift, dirx, diry);
				string tarTile = "tile_"+(int)(tob.xtile)+"_"+(int)(tob.ytile-1);
				gameB[tarTile].interact (tob);
				speedAdj = 0;
			}
//			if (speedAdj != 0 && tob.GetType ().IsSubclassOf (typeof(Player))) {
//				Player p = (Player)tob;
//				p.setDir(0);
//			}
		}
		//if going down
		if (diry == 1)
		{
			if (tob.GetType().IsSubclassOf(typeof(Player))) {
				if(tob.GetType() == typeof(Bot3)) {
					Bot3 b3 = (Bot3)tob;
					if (b3.currDir != 2) {
						if (!CanThisTurn(tob)) {
							if(b3.currDir == 0)
								speedAdj /= 2;
							else
								return speedAdj = 0.0;
						} else {
							b3.setDir(2);
							return 0.0;
						}
					}
				} else {
					((Player)tob).setDir(2);
				}
			}
			if (tob.downleft && tob.downright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1] && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.downYPos > iob.upYPos && tob.upYPos < iob.downYPos &&
							tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
				tob.setY((float)(tob.posY+speedAdj*diry));
			}
			else
			{
				double yStart = tob.posY;
				tob.setY(((float)((tob.ytile+1)*(tileW/2)-(tob.width/2))));
				double yShift = tob.posY-yStart;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, yShift, dirx, diry);
				string tarTile = "tile_"+(int)(tob.xtile)+"_"+(int)(tob.ytile+1);
				gameB[tarTile].interact (tob);
				speedAdj = 0;
			}
//			if (speedAdj != 0 && tob.GetType ().IsSubclassOf (typeof(Player))) {
//				Player p = (Player)tob;
//				p.setDir(2);
//			}
		}
		//horizontal movement
		//changing x with speed and taking old y
		getMyCorners(tob, tob.posX+baseSpeed*dirx, tob.posY);
		//if going left
		if (dirx == -1)
		{
			if (tob.GetType().IsSubclassOf(typeof(Player))) {
				if(tob.GetType() == typeof(Bot3)) {
					Bot3 b3 = (Bot3)tob;
					if (b3.currDir != 3) {
						if (!CanThisTurn(tob)) {
							if(b3.currDir == 1)
								speedAdj /= 2;
							else
								return speedAdj = 0.0;
						} else {
							b3.setDir(3);
							return 0.0;
						}
					}
				} else {
					((Player)tob).setDir(3);
				}
			}
			if (tob.downleft && tob.upleft)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1] && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos &&
							tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
				tob.setX((float)(tob.posX+speedAdj*dirx));
			}
			else
			{
				double xStart = tob.posX;
				tob.setX(((float)(tob.xtile*(tileW/2))));
				double xShift = xStart-tob.posX;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, xShift, dirx, diry);
				string tarTile = "tile_"+(int)(tob.xtile-1)+"_"+(int)(tob.ytile);
				gameB[tarTile].interact (tob);
				speedAdj = 0;
			}
//			if (speedAdj != 0 && tob.GetType ().IsSubclassOf (typeof(Player))) {
//				Player p = (Player)tob;
//				p.setDir(3);
//			}
		}
		//if going right
		if (dirx == 1)
		{
			if (tob.GetType().IsSubclassOf(typeof(Player))) {
				if(tob.GetType() == typeof(Bot3)) {
					Bot3 b3 = (Bot3)tob;
					if (b3.currDir != 1) {
						if (!CanThisTurn(tob)) {
							if(b3.currDir == 3)
								speedAdj /= 2;
							else
								return speedAdj = 0.0;
						} else {
							b3.setDir(1);
							return 0.0;
						}
					}
				} else {
					((Player)tob).setDir(1);
				}
			}
			if (tob.upright && tob.downright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && iob != players[activeBot-1] && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.rightXPos > iob.leftXPos && tob.leftXPos < iob.rightXPos &&
							tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
						}
					}
				}
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
				tob.setX((float)(tob.posX+speedAdj*dirx));
			}
			else
			{
				double xStart = tob.posX;
				tob.setX(((float)((tob.xtile+1)*(tileW/2)-(tob.width/2))));
				double xShift = tob.posX-xStart;
				if (tob.GetType () == typeof(Bot1))
					if (((Bot1)tob).grabbing)
						moveChar (((Bot1)tob).grabbed, xShift, dirx, diry);
				string tarTile = "tile_"+(int)(tob.xtile+1)+"_"+(int)(tob.ytile);
				gameB[tarTile].interact (tob);
				speedAdj = 0;
			}
//			if (speedAdj != 0 && tob.GetType ().IsSubclassOf (typeof(Player))) {
//				Player p = (Player)tob;
//				p.setDir(1);
//			}
		}
		if (speedAdj == 0)
			tob.endAction();
		
		return (speedAdj);
	}
}