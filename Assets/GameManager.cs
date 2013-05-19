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
	public Texture2D bot1Blueprint, bot2Blueprint, bot3Blueprint;
	public Texture2D overlayActive;
	public Texture2D overlayAbility;
	
	public GUIStyle buttonStyle;
	
	private int mapWidth = 0;
	private int mapHeight = 0;
	private static float tileW = 128;
																						
	private Dictionary<string, Tile> gameB = new Dictionary<string, Tile>();
	private Dictionary<int, List<Tile>> gameConsOut = new Dictionary<int, List<Tile>>();
	private Dictionary<int, List<Tile>> gameConsIn = new Dictionary<int, List<Tile>>();
	private Dictionary<int, List<Tile>> gameLocksOut = new Dictionary<int, List<Tile>>();
	private Dictionary<int, List<Tile>> gameLocksIn = new Dictionary<int, List<Tile>>();
	private List<Obstacle> gameObs = new List<Obstacle>();
	private List<Dictionary<string, double>> messagesDisplay = new List<Dictionary<string, double>>(3);
	
	private bool running = false;
	private bool selection = false;
	private int stageSelect = 0;
	private bool paused = false;
	private int level = 0;
	private bool showMessages = true;
	private string filePath = "save.xml";
	private Finish refFinish;
	private Tile refSpawn;
	private bool cheats = false;
	private double lastClick = 0.0;
	private bool determineAbility = false;
	
	private GameObject bot1Port;
	private GameObject bot2Port;
	private GameObject bot3Port;
	private OTAnimatingSprite bot1PSp;
	private OTAnimatingSprite bot2PSp;
	private OTAnimatingSprite bot3PSp;
	
	private List<Player> players = new List<Player>();
	private int activeBot = 1;
	private List<KeyCode> activeInputs = new List<KeyCode>();
	
	void Start () {
		Time.fixedDeltaTime = 1/30f;
		players.Add (new Bot1()); // Webelo (Red, Lifter)
		players.Add (new Bot2()); // Hob (Yellow, Hoverer)
		players.Add (new Bot3()); // Hisco (Green, Wheelie)
		
//		camera.GetComponent<AudioListener>().enabled = false;
		
		messagesDisplay.Add(new Dictionary<string, double>());
		messagesDisplay.Add(new Dictionary<string, double>());
		messagesDisplay.Add(new Dictionary<string, double>());
		
		overlayAbility = Resources.Load("overlayActive") as Texture2D;
		
		bot1Port = OT.CreateObject ("PortraitSprite");
		bot2Port = OT.CreateObject ("PortraitSprite");
		bot3Port = OT.CreateObject ("PortraitSprite");
		bot1PSp = bot1Port.GetComponent<OTAnimatingSprite>();
		bot2PSp = bot2Port.GetComponent<OTAnimatingSprite>();
		bot3PSp = bot3Port.GetComponent<OTAnimatingSprite>();
		bot1PSp.visible = false;
		bot2PSp.visible = false;
		bot3PSp.visible = false;
	}
	
	void TopMenu() {
	    //layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 300, 50, 600, 600));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 200), "");
	   
	    //title
	   	GUI.Box(new Rect(300-logoTexture.width/2,10,logoTexture.width,logoTexture.height),logoTexture);
	    ///////main menu buttons
	    //game start button
	    if(GUI.Button(new Rect(162.5f, 20+logoTexture.height, 275, 75), "Start game", buttonStyle)) {
			selection = true;
			stageSelect = 0;
	    }
		if(GUI.Button(new Rect(162.5f, 100+logoTexture.height, 275, 75), "Controls", buttonStyle)) {
			selection = true;
			stageSelect = 4;
	    }
//	    if(GUI.Button(new Rect(162.5f, 100+logoTexture.height, 275, 75), "Editor", buttonStyle)) {
//			Application.LoadLevel (1);
//	    }
	    //quit button
	    if(GUI.Button(new Rect(162.5f, 180+logoTexture.height, 275, 75), "Quit", buttonStyle)) {
	    	Application.Quit();
	    }
	   
	    //layout end
	    GUI.EndGroup();
	}
	
	void ControlsMenu() {
		//layout
		GUI.BeginGroup(new Rect(Screen.width / 2 - 350, 50, 700, 600));
		
		GUI.Label (new Rect(0,0,300,500),"Keyboard Controls\n\nMovement\n Forward: \tW / UpArrow\n Back: \t\t\tS / DownArrow\n Left: \t\t\tA / LeftArrow\n Right: \t\t\tD / RightArrow\n\n" +
			"Abilities\n Primary: \tE\n Secondary: \tR");
		
		GUI.Label (new Rect(400,0,300,500),"Mouse Controls\n\nMovement\n Hold the left mouse button down on the screen in the direction you want to travel.\n\n\n" +
			"Abilities\n Primary: Click once on the robot\n Secondary: Double-click on the robot");
		
		if (GUI.Button(new Rect(200,500,300,80),"Back",buttonStyle)) {
			selection = false;
		}
		
		GUI.EndGroup();
	}
	void StageSelectMenu() {
		//layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 400));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 200), "");
		
		if(GUI.Button(new Rect(12.5f, 100, 275, 75), "New Game", buttonStyle)) {
			// Proceed to SelectionMenu
			stageSelect = 1;
			//Reset to defaults
			level = 0;
			foreach (Player p in players)
				p.level = -1;
		}
		if (File.Exists(filePath))
			if(GUI.Button(new Rect(12.5f, 180, 275, 75), "Continue", buttonStyle)) {
				// Grayed out if no save file exisits.
				// Loads game at last level w/ same bots/updrades
				if (File.Exists(filePath)) 
					stageSelect = 2;
			}
//		if(GUI.Button(new Rect(55, 180, 180, 40), "Stage Select")) {
//			
//			stageSelect = 3;
//		}
		if(GUI.Button(new Rect(12.5f, 260, 275, 75), "Back", buttonStyle)) {
			selection = false;
			stageSelect = 0;
		}
		
		//layout end
	    GUI.EndGroup();
	}
	void SelectionMenu() {
	    //layout start
	    GUI.BeginGroup(new Rect(Screen.width/2-450, 50, 900, 800));
	   
	    //title
	    GUI.Label(new Rect(15, 10, 300, 68), "Which robot will you make?");
		
	    ///////menu buttons
	    //selection options
		if (!running) {
			Rect selectBot1 = new Rect(0, 100, 300, 400);
			Rect selectBot2 = new Rect(300, 100, 300, 400);
			Rect selectBot3 = new Rect(600, 100, 300, 400);
		    if(GUI.Button(selectBot1, bot1Blueprint)) {
				running = true;
				selection = false;
				activeBot = 1;
				level++;
				players[0].level++;
				BuildLevel ("level"+level);
				Save ();
		    }
			if (selectBot1.Contains(Event.current.mousePosition)){
				if (players[0].level == -1)
					GUI.Label(new Rect(50,500, 200,200),"This robot can push boxes. It has the ability to grab and pull objects.");
				else
					GUI.Label(new Rect(50,500, 200,200),"This upgrade will allow the robot to extend its arms up to 3 tiles.");
			}
		    if(GUI.Button(selectBot2, bot2Blueprint)) {
				running = true;
				selection = false;
				activeBot = 2;
				level++;
				players[1].level++;
				BuildLevel ("level"+level);
				Save ();
		    }
			if (selectBot2.Contains(Event.current.mousePosition)){
				if (players[1].level == -1)
					GUI.Label(new Rect(350,500, 200,200),"This robot can not push boxes. It has the ability to charge generators it touches " +
														"and can traverse electrified tiles.");
				else
					GUI.Label(new Rect(350,500, 200,200),"Gain the ability to hover over short distances to get over small obstacles and pits.");
			}
		    if(GUI.Button(selectBot3, bot3Blueprint)) {
				running = true;
				selection = false;
				activeBot = 3;
				level++;
				players[2].level++;
				BuildLevel ("level"+level);
				Save ();
		    }
			if (selectBot3.Contains(Event.current.mousePosition)){
				if (players[2].level == -1)
					GUI.Label(new Rect(650,500, 200,200),"This robot can push boxes. It has the ability to sprint for short periods of time.");
				else
					GUI.Label(new Rect(650,500, 200,200),"Upgrade wheel(s) to gain speed on certain tiles and lessen the penalty for pushing objects.");
			}
		    //return to main menu
			if (level == 0)
		    	if(GUI.Button(new Rect(720, 600, 180, 40), "Return")) {
//		    		selection = false;
					stageSelect = 0;
		    }
		}
	   
	    //layout end
	    GUI.EndGroup();
	}
	
	void VictoryMenu() {
		//layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 400));
		
		//the menu background box
	    GUI.Box(new Rect(0, 50, 300, 300), "You're a winner!");
		
		if (GUI.Button (new Rect(55,110,180,40),"Main Menu", buttonStyle)) {
			stageSelect = 0;
			selection = false;
			level = 0;
			foreach (Player p in players)
				p.level = -1;
		}
		
		//layout end
	    GUI.EndGroup();
	}
	// If you hit Escape while playing the game!
	void PauseMenu() {
	    //layout start
	    GUI.BeginGroup(new Rect(Screen.width / 2 - 150, 50, 300, 400));
	   
	    //the menu background box
	    GUI.Box(new Rect(0, 0, 300, 350), "");
	   
	    //logo picture
	    GUI.Label(new Rect(94, 15, 300, 40), "Game Paused!");
	   
	    ///////main menu buttons
	    //game start button
	    if(GUI.Button(new Rect(55, 60, 180, 40), "Resume Game", buttonStyle)) {
			paused = false;
	    }
		//restart level
		if(GUI.Button(new Rect(55, 110, 180, 40), "Restart Level", buttonStyle)) {
			paused = false;
			ClearLevel();
			BuildLevel("level"+level);
	    }
		//gogo editor
	    if(GUI.Button(new Rect(55, 160, 180, 40), "Editor", buttonStyle)) {
			Application.LoadLevel (1);
	    }
		//toggle tutorial
		if (showMessages) {
			if(GUI.Button(new Rect(55, 210, 180, 40), "Tutorial: On", buttonStyle)) {
				showMessages = false;		}
		} else {
			if(GUI.Button(new Rect(55, 210, 180, 40), "Tutorial: Off", buttonStyle)) {
				showMessages = true; 	}
		}
	    //quit button
	    if(GUI.Button(new Rect(55, 260, 180, 40), "Main Menu", buttonStyle)) {
	    	paused = false;
			running = false;
			selection = false;
			stageSelect = 0;
//			level = 0;
			ClearLevel ();
	    }
	   
	    //layout end
	    GUI.EndGroup();
	}
	
	void GameOverlay() {
		// Covers entire screen...
		GUI.BeginGroup (new Rect(0, 0, Screen.width, Screen.height));
		
		//GUI.DrawTexture (new Rect(5+133*(activeBot-1), 5, 133, 133), overlayActive); // Box position emphasises activeBot
		
		//GUI.Label (new Rect(10, 246, 133, 20), "L: "+players[0].level);
		
		//GUI.Label (new Rect(143, 246, 133, 20), "L: "+players[1].level);
		
		//GUI.Label (new Rect(276, 246, 133, 20), "L: "+players[2].level);
		
		// Show when mouse is over location to activate abilities
		Vector2 botOnScreen = Camera.main.WorldToScreenPoint(players[activeBot-1].os.position);
		Rect overBot = new Rect(botOnScreen.x,botOnScreen.y-128,128,128);
		if (overBot.Contains(Event.current.mousePosition)) {
			GUI.DrawTexture(overBot,overlayAbility);
		}
		// Tutorial Message Box
		if (showMessages && ((TileClass)gameB[players[activeBot-1].onTile()]).messages[activeBot-1].Count > 0) {
			TileClass curTile = (TileClass)gameB[players[activeBot-1].onTile()];
			foreach (KeyValuePair<int,string> kvp in ((TileClass)gameB[players[activeBot-1].onTile()]).messages[activeBot-1]) {
				if (players[activeBot-1].level >= kvp.Key){
					if (!curTile.msgsRead[activeBot-1].ContainsKey(kvp.Key)){
						curTile.msgsRead[activeBot-1].Add(kvp.Key,Time.time);
						if (!messagesDisplay[activeBot-1].ContainsKey(kvp.Value))
							messagesDisplay[activeBot-1].Add(kvp.Value, Time.time);
					}
				}
			}
		}
		if (showMessages && messagesDisplay[activeBot-1].Count > 0) {
			int step = 0;
			List<KeyValuePair<string,double>> tempVals = new List<KeyValuePair<string, double>>(messagesDisplay[activeBot-1]);
			foreach (KeyValuePair<string,double> kvp in tempVals) {
				if (kvp.Value > Time.time-30) {
					if (GUI.Button(new Rect(Screen.width-(Screen.width/3)-5,55 + step,290,kvp.Key.Length/2), "")) { 	//Change if text extends past box
						messagesDisplay[activeBot-1][kvp.Key] = Time.time-30;
					}
					GUI.Label(new Rect(Screen.width-(Screen.width/3),52 + step,290,70), kvp.Key);
					step += kvp.Key.Length/2;																	//Change if there is overlapping
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
			writer.WriteElementString ("Tutorial", showMessages.ToString().ToLower());
			writer.WriteEndDocument ();
		};
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
		if (players[0].level > -1)
			activeBot = 1;
		else if (players[1].level > -1)
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
				if (!ob.GetType().IsSubclassOf(typeof(Player))) {
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
		// Check if next level exists
		if (Resources.Load("level"+(level+1)) == null) {
			stageSelect = 3;
		}
	}
	
	void BuildLevel(string map)
	{
//		map = "level2";	//load this
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
								getMyCorners(players[activeBot-1],players[activeBot-1].posX,players[activeBot-1].posY);
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
		messagesDisplay[activeBot-1].Add("These messages can be removed early by clicking on them. Or turned off from the paused screen.",Time.time);
	}
	
	void OnGUI () {
		if (!running) {
			bot1PSp.Stop();
			bot1PSp.visible = false;
			bot2PSp.Stop();
			bot2PSp.visible = false;
			bot3PSp.Stop();
			bot3PSp.visible = false;
			//load GUI skin
		    GUI.skin = skin;
			
		    if (selection) { //Goto Selection Menu
				if (stageSelect == 0)
					StageSelectMenu();
				else if (stageSelect == 1)
					SelectionMenu();
				else if (stageSelect == 2)
					LoadFromSave();
				else if (stageSelect == 3)
					VictoryMenu();
				else if (stageSelect == 4)
					ControlsMenu();
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
				
				// Auto-complete level
				if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.O)) {
					refFinish.SkipLevel();
				}
				// Upgrade all bots
				if (!cheats && Input.GetKey(KeyCode.U) && Input.GetKey(KeyCode.P)) {
					cheats = true;
					foreach (Player p in players)
						p.level = 10;
				}
				if (Input.GetKeyDown(KeyCode.E))
				{	DoPrimary (); }
				
				if (Input.GetMouseButtonDown(0)) {
					Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
					mouseLocation.x -= 64;
					mouseLocation.y -= 64;
					if (Vector2.Distance(mouseLocation,players[activeBot-1].os.position) < 50) {
//					if(Math.Abs(mouseLocation.x-64 - players[activeBot-1].xiso) < 64 && Math.Abs(mouseLocation.y-64 - players[activeBot-1].yiso) < 64){
						if (determineAbility) {
							determineAbility = false;
							DoSecondary();
						} else {
								determineAbility = true;
								lastClick = Time.time;
						}
					}
				}
				
				if (Input.GetKeyDown (KeyCode.R) && !players[activeBot-1].inAction())
				{	DoSecondary(); }
				// Scan for player interaction. This probably needs updating for different bot abilites.
				
				if (Input.GetKeyDown(KeyCode.Space))
				{	interact();	}
				
				// Bot selection... eventually this will only be if you have multiple robots active! (Remove comment below)
				if (!players[activeBot-1].inAction()) {
					if (Input.GetAxis("select1") == 1.0) {
						if (activeBot != 1 && players[0].level >= 0) {
							if (!gameObs.Contains(players[0]) && TileClear(refSpawn.myName())) {
								players[0].setXY (refSpawn.xgrid,refSpawn.ygrid);
								players[0].setDir (0);
								gameObs.Add (players[0]);
								getMyCorners(players[0],players[0].posX,players[0].posY);
							}
							if (gameObs.Contains(players[0])) {
								activeBot = 1;
							}
						}
					}
					if (Input.GetAxis("select2") == 1.0) {
						if (activeBot != 2 && players[1].level >= 0) {
							if (!gameObs.Contains(players[1]) && TileClear(refSpawn.myName())) {
								players[1].setXY (refSpawn.xgrid,refSpawn.ygrid);
								players[1].setDir (0);
								gameObs.Add (players[1]);
								getMyCorners(players[1],players[1].posX,players[1].posY);
							}
							if (gameObs.Contains(players[1])) {
								activeBot = 2;
							}
						}
					}
					if (Input.GetAxis("select3") == 1.0) {
						if (activeBot != 3 && players[2].level >= 0) {
							if (!gameObs.Contains(players[2]) && TileClear(refSpawn.myName())) {
								players[2].setXY (refSpawn.xgrid,refSpawn.ygrid);
								players[2].setDir (0);
								gameObs.Add (players[2]);
								getMyCorners(players[2],players[2].posX,players[2].posY);
							}
							if (gameObs.Contains(players[2])) {
								activeBot = 3;
							}
						}
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
			// Bot Selection Overlay
			if (!paused) {
				bool movement = false;
				if (!players[activeBot-1].inAction() && !players[activeBot-1].onActiveElec) {
					double speed = 5;
					//Directional movement. Should this be limited to one direction at a time?
					if (Input.GetAxis("horizontal") != 0.0) {
						moveChar(players[activeBot-1], speed, (int)(Math.Round(Input.GetAxis("horizontal"),MidpointRounding.AwayFromZero)), 0);
						movement = true;
					} else if (Input.GetAxis("vertical") != 0.0) {
						moveChar(players[activeBot-1], speed, 0, (int)(Math.Round(Input.GetAxis("vertical"),MidpointRounding.AwayFromZero)));
						movement = true;
					} else if (Input.GetMouseButton(0)) {
						Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
						double locX = Input.mousePosition.x - Screen.width/2;
						double locY = Input.mousePosition.y - Screen.height/2;
						
						Debug.Log("X = "+locX+", Y = "+locY);
						if (!(Math.Abs(mouseLocation.x-64 - players[activeBot-1].xiso) < 64 && Math.Abs(mouseLocation.y-64 - players[activeBot-1].yiso) < 64)) {
							if (locX > 0 && locY > 0)
								moveChar(players[activeBot-1],speed,0,-1);
							else if (locX < 0 && locY < 0)
								moveChar(players[activeBot-1],speed,0,1);
							else if (locX > 0 && locY < 0)
								moveChar(players[activeBot-1],speed,1,0);
							else if (locX < 0 && locY > 0)
								moveChar(players[activeBot-1],speed,-1,0);
							movement = true;
						}
					}					
					players[activeBot-1].update (movement);
				}
				
				if (determineAbility && (Time.time - lastClick > 0.2)) {
					determineAbility = false;
					DoPrimary();
				}
				
				// Do round 1 of Tile updates. Final 'act' method called in LateUpdate();
				foreach (Tile t in gameB.Values) {
					t.update();
				}
				
				// Frame/time stepped move actions of activeBot here. 
				foreach (Player p in players) {
					if (p.GetType() != typeof(Bot2)) {			// Push player back when on a powered Elec tile
						bool needToMove = false;
						if ( gameB[p.onTile()].GetType() == typeof(Electrified)) {
							if (((Electrified)gameB[p.onTile()]).on)
								needToMove = true;
						}
						if ( !needToMove && gameB[p.onTileTopR()].GetType() == typeof(Electrified)) {
							if (((Electrified)gameB[p.onTileTopR()]).on)
								needToMove = true;
						}
						if ( !needToMove && gameB[p.onTileBotL()].GetType() == typeof(Electrified)) {
							if (((Electrified)gameB[p.onTileBotL()]).on)
								needToMove = true;
						}
						if ( !needToMove && gameB[p.onTileBotR()].GetType() == typeof(Electrified)) {
							if (((Electrified)gameB[p.onTileBotR()]).on)
								needToMove = true;
						}

						if (needToMove) {
							bool setStart = false;
							while (!setStart) {
								if (p.onTile() == p.pathOrder[p.pathOrder.Count-1] || p.onTileTopR() == p.pathOrder[p.pathOrder.Count-1]
									|| p.onTileBotL() == p.pathOrder[p.pathOrder.Count-1] || p.onTileBotR() == p.pathOrder[p.pathOrder.Count-1]) {
									setStart = true;
								} else {	
									p.pathDir.Remove(p.pathOrder[p.pathOrder.Count-1]);
									p.pathOrder.RemoveAt(p.pathOrder.Count-1);
								}
							}
							p.endAction();
							p.onActiveElec = true;
							switch (p.pathDir[p.pathOrder[p.pathOrder.Count-1]]) {
							case 0: moveChar(p,10,0,-1);
									break;
							case 1:	moveChar(p,10,1,0);
									break;
							case 2: moveChar(p,10,0,1);
									break;
							case 3: moveChar(p,10,-1,0);
									break;
							}
						} else {
							p.onActiveElec = false;	
						}
					}
					if (p.inAction() ) {								//Actives Bot abilites such as Dash(Bot3) and Hover(Bot2)
						Debug.Log ("Player ability interaction");
						if (p.GetType () != typeof(Bot1)) {
							if (p.currDir == 0)
								moveChar(p,5,0,-1);
							else if (p.currDir == 1)
								moveChar(p,5,1,0);
							else if (p.currDir == 2)
								moveChar(p,5,0,1);
							else if (p.currDir == 3)
								moveChar(p,5,-1,0);
						} else {
							Bot1 b1 = (Bot1)p;
							if (!b1.startExtArms) {
								double step = b1.ExtendArmsStep();
								switch(p.currDir){
									case 0:	
											b1.hands.setY (b1.posY-step);
//											moveChar(b1.hands,10,0,-1);
											break;
									case 1: 
											b1.hands.setX (b1.posX+step);
//											moveChar(b1.hands,10,1,0);
											break;
									case 2: 
											b1.hands.setY (b1.posY+step);
//											moveChar(b1.hands,10,0,1);
											break;
									case 3: 
											b1.hands.setX (b1.posX-step);
//											moveChar(b1.hands,10,-1,0);
											break;
								}
								getMyCorners(b1.hands,b1.hands.posX,b1.hands.posY);
								if (!b1.hands.downleft || !b1.hands.downright || !b1.hands.upleft || !b1.hands.upright) {
									interact();
									b1.endAction();
								}
								if (b1.extendingArms){
									//ToDo: Move arms out
									if (b1.grabbing){
	//									Move b1.grabbed out until max distance or until released
										double spd = 1;
										switch(b1.currDir){
											case 0:	spd = moveChar(b1.grabbed,10,0,-1);
													break;
											case 1: spd = moveChar(b1.grabbed,10,1,0);
													break;
											case 2: spd = moveChar(b1.grabbed,10,0,1);
													break;
											case 3: spd = moveChar(b1.grabbed,10,-1,0);
													break;
										}
										b1.extendDist = b1.extendDist - 10 + spd;
										if (spd == 0)
											b1.endAction();
									} else {
	//									Move arms out until obstacle (when they exist)
	//									if Obstacle is grabbable {
										foreach (Obstacle o in gameObs) {
											if (o.GetType() != typeof(Bot1)) {
												getMyCorners(o, o.posX, o.posY);
												if (b1.hands.upYPos <= o.downYPos && b1.hands.upYPos >= o.upYPos && b1.hands.leftXPos <= o.rightXPos && b1.hands.leftXPos >= o.leftXPos){
													b1.Grab(o);
													b1.endAction();
												} else if (b1.hands.downYPos <= o.downYPos && b1.hands.downYPos >= o.upYPos && b1.hands.leftXPos <= o.rightXPos && b1.hands.leftXPos >= o.leftXPos) {
													b1.Grab(o);
													b1.endAction();
												} else if (b1.hands.upYPos <= o.downYPos && b1.hands.upYPos >= o.upYPos && b1.hands.rightXPos <= o.rightXPos && b1.hands.rightXPos >= o.leftXPos) {
													b1.Grab(o);
													b1.endAction();
												} else if (b1.hands.downYPos <= o.downYPos && b1.hands.downYPos >= o.upYPos && b1.hands.rightXPos <= o.rightXPos && b1.hands.rightXPos >= o.leftXPos){
													b1.Grab(o);
													b1.endAction();
												}
											}
//											if (b1.grabbing)
//												b1.endAction();
										}
									}
								} else if (b1.retractArms) {
									//ToDo: move arms back in
									if (b1.grabbing) {
										double mov = 0.0;
	//									Move back with b1.grabbed until interacting with bot1
										switch(b1.currDir){
											case 0:	mov = moveChar(b1.grabbed,10,0,1);
													break;
											case 1: mov = moveChar(b1.grabbed,10,-1,0);
													break;
											case 2: mov = moveChar(b1.grabbed,10,0,-1);
													break;
											case 3: mov = moveChar(b1.grabbed,10,1,0);
													break;
										}
										b1.extendDist = b1.extendDist + 10 - mov;
										if (mov == 0) {
											b1.grabbed = null;
											b1.grabbing = false;
										}
									} 
								} 	
								getMyCorners(b1,b1.posX,b1.posY);
							}
						}
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
			
			if (players[0].level >= 0)
				bot1PSp.visible = true;
			else
				bot1PSp.visible = false;
			if (players[1].level >= 0)
				bot2PSp.visible = true;
			else
				bot2PSp.visible = false;
			if (players[2].level >= 0)
				bot3PSp.visible = true;
			else
				bot3PSp.visible = false;
			
			Vector3 topCorner = camera.ScreenToWorldPoint (new Vector3(5, Screen.height-5, 0));
			bot1PSp.position = new Vector2(topCorner.x, topCorner.y);
			bot2PSp.position = new Vector2(topCorner.x+128+5, topCorner.y);
			bot3PSp.position = new Vector2(topCorner.x+256+10, topCorner.y);
			bot1PSp.depth = -998;
			bot2PSp.depth = -998;
			bot3PSp.depth = -998;
			
			if (activeBot == 1) {
				if (!bot1PSp.isPlaying)
					bot1PSp.PlayLoop ("Bot1");
				bot2PSp.PlayOnce ("Bot2");
				bot3PSp.PlayOnce ("Bot3");
			} else if (activeBot == 2) {
				bot1PSp.PlayOnce ("Bot1");
				if (!bot2PSp.isPlaying)
					bot2PSp.PlayLoop ("Bot2");
				bot3PSp.PlayOnce ("Bot3");
			} else {
				bot1PSp.PlayOnce ("Bot1");
				bot2PSp.PlayOnce ("Bot2");
				if (!bot3PSp.isPlaying)
					bot3PSp.PlayLoop ("Bot3");
			}
			
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
			orderedObs.Add(((Bot1)players[0]).hands);
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
				if (ob.onTile() == name || ob.onTileBotR() == name || ob.onTileTopR() == name || ob.onTileBotL() == name)
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
				tarTile = "tile_"+(int)(players[activeBot-1].rightX)+"_"+(int)(players[activeBot-1].downY+distance);
			} else if (players[activeBot-1].currDir == 3) {
				tarTile = "tile_"+(int)(players[activeBot-1].leftX-distance)+"_"+(int)(players[activeBot-1].downY);
			}
		} else {
			if (players[activeBot-1].currDir == 0) {
				tarTile = "tile_"+(int)(players[activeBot-1].rightX)+"_"+(int)(players[activeBot-1].upY-distance);
			} else if (players[activeBot-1].currDir == 1) {
				tarTile = "tile_"+(int)(players[activeBot-1].rightX+distance)+"_"+(int)(players[activeBot-1].downY);
			} else if (players[activeBot-1].currDir == 2) {
				tarTile = "tile_"+(int)(players[activeBot-1].leftX)+"_"+(int)(players[activeBot-1].downY+distance);
			} else if (players[activeBot-1].currDir == 3) {
				tarTile = "tile_"+(int)(players[activeBot-1].leftX-distance)+"_"+(int)(players[activeBot-1].upY);
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
	
	private void DoSecondary() {
		Debug.Log ("Secondary");
		if (players[activeBot-1].GetType() == typeof(Bot1)){
			Bot1 b1 = (Bot1)players[activeBot-1];
			b1.secondary();
			
		}
	}
	
	private bool CanThisTurn(Obstacle ob, int dir) {
		bool turn = true;
		if (ob.GetType() == typeof(Bot3)) {
			Bot3 b3 = (Bot3)ob;
			b3.TurnCorners();
			
			double upY = Math.Floor(b3.upYPos2/(tileW/2));
			double downY = Math.Floor(b3.downYPos2/(tileW/2));
			double leftX = Math.Floor(b3.leftXPos2/(tileW/2));
			double rightX = Math.Floor(b3.rightXPos2/(tileW/2));
			
			bool[] open = new bool[4] {true, true, true, true};
			
			if (downY < mapHeight && upY >= 0 && leftX >= 0 && rightX < mapWidth) {
				if (!gameB["tile_"+leftX+"_"+upY].walkable(ob))
					open[0] = turn = false;
				if (!gameB["tile_"+leftX+"_"+downY].walkable(ob))
					open[3] = turn = false;
				if (!gameB["tile_"+rightX+"_"+upY].walkable(ob))
					open[1] = turn = false;
				if (!gameB["tile_"+rightX+"_"+downY].walkable(ob))
					open[2] = turn = false;
			}
			if (downY >= mapHeight)
				open[2] = open[3] = turn=  false;
			if (upY < 0)
				open[0] = open[1] = turn = false;
			if (leftX < 0)
				open[0] = open[3] = turn = false;
			if (rightX >= mapWidth)
				open[1] = open[2] = turn = false;
		
			foreach (Obstacle o in gameObs) {
				if (o != ob) {
					getMyCorners(o, o.posX, o.posY);
					if (b3.upYPos2 < o.downYPos && b3.upYPos2 > o.upYPos && b3.leftXPos2 < o.rightXPos && b3.leftXPos2 > o.leftXPos)
						open[0] = turn = false;
					if (b3.downYPos2 < o.downYPos && b3.downYPos2 > o.upYPos && b3.leftXPos2 < o.rightXPos && b3.leftXPos2 > o.leftXPos)
						open[3] = turn =  false;
					if (b3.upYPos2 < o.downYPos && b3.upYPos2 > o.upYPos && b3.rightXPos2 < o.rightXPos && b3.rightXPos2 > o.leftXPos)
						open[1] = turn = false;
					if (b3.downYPos2 < o.downYPos && b3.downYPos2 > o.upYPos && b3.rightXPos2 < o.rightXPos && b3.rightXPos2 > o.leftXPos)
						open[2] = turn = false;
				}
			}
			if (!turn) {
				if ((dir == 3 && open[dir] && open[0]) || (dir != 3 && open[dir] && open[dir+1])) {
					turn = true;
				}
			}
		}
		return turn;
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
		{	//Pre-move check for players
			if (tob.GetType().IsSubclassOf(typeof(Player))) {
				if(!((Player)tob).onActiveElec) {
					if(tob.GetType() == typeof(Bot3)) {
						Bot3 b3 = (Bot3)tob;
						if (b3.currDir != 0) {
							if (!CanThisTurn(tob, 0)) {
								if(b3.currDir == 2)
									speedAdj /= 2;
								else
									return 0.0;
							} else {
								b3.setDir(0);
								return 0.0;
							}
						}
					} else {
						((Player)tob).setDir(0);
					}
				}
			}
			if (tob.upleft && tob.upright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && (iob != players[activeBot-1] || iob.inAction()) && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos &&
							tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
							if (((Bot1)players[0]).grabbed == iob && tob != players[0] && speedAdj > 0 && !players[0].inAction())
								players[0].primary(FacingTile(true,0));
						}
					}
				}
				//post-move player updates
				if (tob.GetType().IsSubclassOf(typeof(Player))) {
					//electric tile path pushback set/clear
					if (tob.GetType() != typeof(Bot2) && !((Player)tob).onActiveElec) {
						if (gameB[tob.onTile()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTile())) {
								((Player)tob).pathDir.Add(tob.onTile(),2); 
								((Player)tob).pathOrder.Add (tob.onTile());
							}
						} else if (gameB[tob.onTileTopR()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileTopR())) {
								((Player)tob).pathDir.Add(tob.onTileTopR(),2);
								((Player)tob).pathOrder.Add (tob.onTileTopR());
							}
						} else if (gameB[tob.onTileTopR()].GetType() != typeof(Electrified) && gameB[tob.onTile()].GetType() != typeof(Electrified) &&
							gameB[tob.onTileBotL()].GetType() != typeof(Electrified) && gameB[tob.onTileBotR()].GetType() != typeof(Electrified)) {
							((Player)tob).pathDir.Clear();
							((Player)tob).pathOrder.Clear();
						} else {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTile())) {
								((Player)tob).pathDir.Add(tob.onTile(),2); 
								((Player)tob).pathOrder.Add (tob.onTile());
							}
						}
					}
					if (tob.GetType () == typeof(Bot1)){
//						foreach (Box a in ((Bot1)tob).arm)
//							a.setXY(tob.posX,tob.posY);
						if (((Bot1)tob).grabbing)
							speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
					}
				}
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
				if(!((Player)tob).onActiveElec) {
					if(tob.GetType() == typeof(Bot3)) {
						Bot3 b3 = (Bot3)tob;
						if (b3.currDir != 2) {
							if (!CanThisTurn(tob, 2)) {
								if(b3.currDir == 0)
									speedAdj /= 2;
								else
									return 0.0;
							} else {
								b3.setDir(2);
								return 0.0;
							}
						}
					} else {
						((Player)tob).setDir(2);
					}
				}
			}
			if (tob.downleft && tob.downright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && (iob != players[activeBot-1] || iob.inAction()) && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.downYPos > iob.upYPos && tob.upYPos < iob.downYPos &&
							tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
							if (((Bot1)players[0]).grabbed == iob && tob != players[0] && speedAdj > 0 && !players[0].inAction())
								players[0].primary(FacingTile(true,0));
						}
					}
				}
				if (tob.GetType().IsSubclassOf(typeof(Player))) {
					//electric tile path pushback set/clear
					if (tob.GetType() != typeof(Bot2) && !((Player)tob).onActiveElec) {
						if (gameB[tob.onTileBotL()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileBotL())) {
								((Player)tob).pathDir.Add(tob.onTileBotL(),0); 
								((Player)tob).pathOrder.Add (tob.onTileBotL());
							}
						} else if (gameB[tob.onTileBotR()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileBotR())) {
								((Player)tob).pathDir.Add(tob.onTileBotR(),0);
								((Player)tob).pathOrder.Add (tob.onTileBotR());
							}
						} else if (gameB[tob.onTileTopR()].GetType() != typeof(Electrified) && gameB[tob.onTile()].GetType() != typeof(Electrified) &&
							gameB[tob.onTileBotL()].GetType() != typeof(Electrified) && gameB[tob.onTileBotR()].GetType() != typeof(Electrified)) {
							((Player)tob).pathDir.Clear();
							((Player)tob).pathOrder.Clear();
						} else {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileBotL())) {
								((Player)tob).pathDir.Add(tob.onTileBotL(),0); 
								((Player)tob).pathOrder.Add (tob.onTileBotL());
							}
						}
					}
					if (tob.GetType () == typeof(Bot1))
						if (((Bot1)tob).grabbing)
							speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
				}
				tob.setY((float)(tob.posY+speedAdj*diry));
			}
			else
			{
				double yStart = tob.posY;
				tob.setY(((float)((tob.ytile+1)*(tileW/2)-(tob.length/2))));
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
				if(!((Player)tob).onActiveElec) {
					if(tob.GetType() == typeof(Bot3)) {
						Bot3 b3 = (Bot3)tob;
						if (b3.currDir != 3) {
							if (!CanThisTurn(tob, 3)) {
								if(b3.currDir == 1)
									speedAdj /= 2;
								else
									return 0.0;
							} else {
								b3.setDir(3);
								return 0.0;
							}
						}
					} else {
						((Player)tob).setDir(3);
					}
				}
			}
			if (tob.downleft && tob.upleft)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && (iob != players[activeBot-1] || iob.inAction()) && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.leftXPos < iob.rightXPos && tob.rightXPos > iob.leftXPos &&
							tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
							if (((Bot1)players[0]).grabbed == iob && tob != players[0] && speedAdj > 0 && !players[0].inAction())
								players[0].primary(FacingTile(true,0));
						}
					}
				}
				if (tob.GetType().IsSubclassOf(typeof(Player))) {
					//electric tile path pushback set/clear
					if (tob.GetType() != typeof(Bot2) && !((Player)tob).onActiveElec) {
						if (gameB[tob.onTile()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTile())) {
								((Player)tob).pathDir.Add(tob.onTile(),1); 
								((Player)tob).pathOrder.Add (tob.onTile());
							}
						} else if (gameB[tob.onTileBotL()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileBotL())) {
								((Player)tob).pathDir.Add(tob.onTileBotL(),1);
								((Player)tob).pathOrder.Add (tob.onTileBotL());
							}
						} else if (gameB[tob.onTileTopR()].GetType() != typeof(Electrified) && gameB[tob.onTile()].GetType() != typeof(Electrified) &&
							gameB[tob.onTileBotL()].GetType() != typeof(Electrified) && gameB[tob.onTileBotR()].GetType() != typeof(Electrified)) {
							((Player)tob).pathDir.Clear();
							((Player)tob).pathOrder.Clear();
						} else {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTile())) {
								((Player)tob).pathDir.Add(tob.onTile(),1); 
								((Player)tob).pathOrder.Add (tob.onTile());
							}
						}
					}
					if (tob.GetType () == typeof(Bot1))
						if (((Bot1)tob).grabbing)
							speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
				}
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
				if(!((Player)tob).onActiveElec) {
					if(tob.GetType() == typeof(Bot3)) {
						Bot3 b3 = (Bot3)tob;
						if (b3.currDir != 1) {
							if (!CanThisTurn(tob, 1)) {
								if(b3.currDir == 3)
									speedAdj /= 2;
								else
									return 0.0;
							} else {
								b3.setDir(1);
								return 0.0;
							}
						}
					} else {
						((Player)tob).setDir(1);
					}
				}
			}
			if (tob.upright && tob.downright)
			{
				foreach (Obstacle iob in gameObs) {
					if (iob != tob && (iob != players[activeBot-1] || iob.inAction()) && tob.vertLift == iob.vertLift) {
						getMyCorners(iob, iob.posX, iob.posY);
						if ( tob.rightXPos > iob.leftXPos && tob.leftXPos < iob.rightXPos &&
							tob.upYPos < iob.downYPos && tob.downYPos > iob.upYPos) {
							speedAdj = moveChar(iob, iob.getSpeed (speedAdj,tob), dirx, diry);
							if (((Bot1)players[0]).grabbed == iob && tob != players[0] && speedAdj > 0 && !players[0].inAction())
								players[0].primary(FacingTile(true,0));
						}
					}
				}
				if (tob.GetType().IsSubclassOf(typeof(Player))) {
					//electric tile path pushback set/clear
					if (tob.GetType() != typeof(Bot2) && !((Player)tob).onActiveElec) {
						if (gameB[tob.onTileBotR()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileBotR())) {
								((Player)tob).pathDir.Add(tob.onTileBotR(),3); 
								((Player)tob).pathOrder.Add (tob.onTileBotR());
							}
						} else if (gameB[tob.onTileTopR()].GetType() == typeof(Electrified)) {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileTopR())) {
								((Player)tob).pathDir.Add(tob.onTileTopR(),3);
								((Player)tob).pathOrder.Add (tob.onTileTopR());
							}
						} else if (gameB[tob.onTileTopR()].GetType() != typeof(Electrified) && gameB[tob.onTile()].GetType() != typeof(Electrified) &&
							gameB[tob.onTileBotL()].GetType() != typeof(Electrified) && gameB[tob.onTileBotR()].GetType() != typeof(Electrified)) {
							((Player)tob).pathDir.Clear();
							((Player)tob).pathOrder.Clear();
						} else {
							if (!((Player)tob).pathDir.ContainsKey(tob.onTileBotR())) {
								((Player)tob).pathDir.Add(tob.onTileBotR(),3); 
								((Player)tob).pathOrder.Add (tob.onTileBotR());
							}
						}
					}
					if (tob.GetType () == typeof(Bot1))
						if (((Bot1)tob).grabbing)
							speedAdj = moveChar (((Bot1)tob).grabbed, ((Bot1)tob).grabbed.getSpeed (speedAdj,tob), dirx, diry);
				}
				tob.setX((float)(tob.posX+speedAdj*dirx));
			}
			else
			{
				double xStart = tob.posX;
				tob.setX(((float)((tob.xtile+1)*(tileW/2)-(tob.length/2))));
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
