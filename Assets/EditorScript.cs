using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

public class EditorScript : MonoBehaviour {
	
	private List<List<GameObject>> map = new List<List<GameObject>>();
	private List<List<GameObject>> mapObs = new List<List<GameObject>>();
	private int activeSelection = 0;
	private int gridW = 10;
	private int gridH = 10;
	private string tempW = "10";
	private string tempH = "10";
	private bool guiError = false;
	private bool loadFile = false;
	private bool saveFile = false;
	private bool guiInput = false;
	private FileBrowser browser;
	private string guiErS = "";
	protected string filePath;
	protected string fileName = "level";
	
	public GUIStyle activeButton;
	public GUIStyle passiveButton;
	public GUISkin mainSkin;
	public Texture2D[] buttonGfx;
	
	public string connections;
	public string lockGroups;
	
	private bool validAnchor = false; // Control variable to help set tracer line
	private Vector3 anchor;
	
	private GameObject line;
	// Use this for initialization
	void Start () {
		line = GameObject.Find ("mouseLine");
		SetGrid();
	}
	
	private Vector3 ReturnTileCenter (Vector3 v)
	{
		Vector3 change = new Vector3(v.x+16,v.y-16,v.z-10);
		return change;
	}
	
	private void DrawVerticies(Vector3 a, Vector3 b)
	{
		line.GetComponent<LineRenderer>().SetPosition(0, a);
		line.GetComponent<LineRenderer>().SetPosition(1, b);
	}
	
	
	private void DrawConnections(int num)
	{
		foreach (List<GameObject> g in map)
		{
			foreach (GameObject o in g)
			{
				// Checking all tiles for num, using consList
				if ( o.GetComponent<EditorTile>().consList.Contains(num))
				{
					int count = 0;
					o.GetComponent<LineRenderer>().SetVertexCount(3);
					o.GetComponent<LineRenderer>().SetPosition(count,ReturnTileCenter(o.transform.position));
					foreach (List<GameObject> g2 in map)
					{
						foreach (GameObject o2 in g2)
						{
							// Check all tiles again for num, using consList
							if (o2.GetComponent<EditorTile>().consList.Contains(num))
							{
								// With another matching tile, set another LineRender point, and then return to source tile again
								o.GetComponent<LineRenderer>().SetVertexCount(count + 3);
								o.GetComponent<LineRenderer>().SetPosition(++count,ReturnTileCenter(o2.transform.position));
								o.GetComponent<LineRenderer>().SetPosition(++count,ReturnTileCenter(o.transform.position));
							}
						}
					}
				}
				else {
					o.GetComponent<LineRenderer>().SetVertexCount(1);
					o.GetComponent<LineRenderer>().SetPosition(0,o.transform.position);
				}
			}
		}
	}
	
	private void SetGrid() {
		for (int i = 0; i < gridH; i++) {
			if (map.Count == i) {
				map.Add (new List<GameObject>());
				mapObs.Add (new List<GameObject>());
			}
			for (int j = 0; j < gridW; j++) {
				if (map[i].Count == j) {
					map[i].Add(OT.CreateObject ("builderSprite"));
					mapObs[i].Add(OT.CreateObject ("builderSprite"));
					map[i][j].AddComponent<EditorTile>();
					map[i][j].AddComponent<LineRenderer>();
					map[i][j].GetComponent<OTSprite>().position = new Vector2(j*32f,i*-32f);
					mapObs[i][j].GetComponent<OTSprite>().position = new Vector2(j*32f,i*-32f);
					map[i][j].GetComponent<OTSprite>().frameName = "wall";
					mapObs[i][j].GetComponent<OTSprite>().frameName = "empty";
					mapObs[i][j].GetComponent<OTSprite>().depth = -1;
				}
			}
			if (gridW < map[i].Count) {
				foreach (GameObject t in map[i].GetRange (gridW,map[i].Count-gridW))
					Destroy (t);
				foreach (GameObject o in mapObs[i].GetRange (gridW,map[i].Count-gridW))
					Destroy (o);
				map[i].RemoveRange (gridW,map[i].Count-gridW);
				mapObs[i].RemoveRange (gridW,mapObs[i].Count-gridW);
			}
		}
		if (gridH < map.Count) {
			foreach (List<GameObject> l in map.GetRange (gridH,map.Count-gridH))
				foreach (GameObject t in l)
					Destroy (t);
			foreach (List<GameObject> l in mapObs.GetRange (gridH,map.Count-gridH))
				foreach (GameObject o in l)
					Destroy (o);
			map.RemoveRange (gridH,map.Count-gridH);
			mapObs.RemoveRange (gridH,map.Count-gridH);
		}
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButton (0) && !guiError && !loadFile && !saveFile && !guiInput)
		{
			Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
			int selectX = (int)(Math.Floor (mouseLocation.x/32));
			int selectY = (int)(Math.Floor (mouseLocation.y/-32));
			if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
				SetTypeByDraw(map[selectY][selectX]);
				SetGraphics(map[selectY][selectX], mapObs[selectY][selectX]);
			}
		}
		if (Input.GetMouseButtonDown (1) && !guiError && !loadFile && !saveFile && !guiInput)
		{
			Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
			int selectX = (int)(Math.Floor (mouseLocation.x/32));
			int selectY = (int)(Math.Floor (mouseLocation.y/-32));
			if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
				validAnchor = true;
				anchor = ReturnTileCenter(map[selectY][selectX].transform.position);
				DrawConnections(1); //Test tiles with connection 1
			}
		}
		if (Input.GetMouseButton (1) && !guiError && !loadFile && !saveFile && !guiInput && validAnchor == true)
		{
			Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
			DrawVerticies(anchor,mouseLocation);
		}
		if (Input.GetMouseButtonUp (1) && !guiError && !loadFile && !saveFile && !guiInput)
		{
			Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
			int selectX = (int)(Math.Floor (mouseLocation.x/32));
			int selectY = (int)(Math.Floor (mouseLocation.y/-32));
			validAnchor = false;
			if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
				DrawVerticies(anchor,ReturnTileCenter(map[selectY][selectX].transform.position));
			}
			else{
				DrawVerticies(anchor,anchor); // Removing line from view
			}
		}
	}
	
	void LateUpdate () {
			transform.position = new Vector3(camera.orthographicSize*((float)(Screen.width)/(float)(Screen.height))-20, -camera.orthographicSize*(1-150f/Screen.height), transform.position.z);
	}
	
	private void SetTypeByDraw(GameObject a)
	{
		switch (activeSelection) {
		case 0:
			a.GetComponent<EditorTile>().tileType = 1;
			break;
		case 1:
			a.GetComponent<EditorTile>().tileType = 0;
			break;
		case 2:
			a.GetComponent<EditorTile>().tileType = 3;
			break;
		case 3:
			a.GetComponent<EditorTile>().tileType = 2;
			break;
		case 4:
			a.GetComponent<EditorTile>().tileType = 4;
			break;
		case 5:
			a.GetComponent<EditorTile>().tileType = 5;
			break;
		case 6:
			a.GetComponent<EditorTile>().tileType = 7;
			break;
		case 7:
			a.GetComponent<EditorTile>().obsType = 1;
			break;
		case 8:
			a.GetComponent<EditorTile>().obsType = 4;
			break;
		case 9:
			a.GetComponent<EditorTile>().obsType = 0;
			break;
		case 20:
			a.GetComponent<EditorTile>().setConnections(connections);
			break;
		case 21:
			a.GetComponent<EditorTile>().setLockGroups(lockGroups);
			break;
		}
	}
	
	private void SetGraphics(GameObject a, GameObject b)
	{
		switch (a.GetComponent<EditorTile>().tileType) {
		case 1:
			a.GetComponent<OTSprite>().frameName = "wall";
			break;
		case 0:
			a.GetComponent<OTSprite>().frameName = "floor";
			break;
		case 3:
			a.GetComponent<OTSprite>().frameName = "button";
			break;
		case 2:
			a.GetComponent<OTSprite>().frameName = "plate";
			break;
		case 4:
			a.GetComponent<OTSprite>().frameName = "doorRL";
			break;
		case 5:
			a.GetComponent<OTSprite>().frameName = "doorUD";
			break;
		case 7:
			a.GetComponent<OTSprite>().frameName = "dangerFloor";
			break;
		}
		switch (a.GetComponent<EditorTile>().obsType) {
		case 0:
			b.GetComponent<OTSprite>().frameName = "empty";
			break;
		case 1:
			b.GetComponent<OTSprite>().frameName = "player";
			break;
		case 4:
			b.GetComponent<OTSprite>().frameName = "box";
			break;
		}
	}
	
	private FileBrowser BrowserSetup()
	{
		FileBrowser browser;
		if (loadFile) {
			browser = new FileBrowser(
				new Rect(Screen.width/2-300, 100, 600, 300),
				"Choose XML File",
				ReturnLoadPath
			);
			browser.SelectionPattern = "*.xml";
			browser.BrowserType = FileBrowserType.File;
		} else {
			browser = new FileBrowser(
				new Rect(Screen.width/2-300, 200, 600, 300),
				"Choose Location and Name",
				ReturnSavePath
			);
			browser.BrowserType = FileBrowserType.Directory;
		}
		return browser;
	}
	
	private void ReturnLoadPath(string path)
	{
		filePath = path;
	}
	
	private void ReturnSavePath(string path)
	{
		filePath = path + "/" + fileName + ".xml";
	}
	
	private void WriteXML()
    {
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		using (XmlWriter writer = XmlWriter.Create(filePath,settings)) {
			writer.WriteStartDocument ();
			writer.WriteStartElement ("document");
			writer.WriteElementString ("width", gridW.ToString ());
			writer.WriteElementString ("height", gridH.ToString ());
			foreach (List<GameObject> i in map) {
				writer.WriteStartElement ("y");
				foreach (GameObject j in i) {
					writer.WriteStartElement ("x");
					writer.WriteElementString ("type",j.GetComponent<EditorTile>().tileType.ToString ());
					writer.WriteElementString ("obs",j.GetComponent<EditorTile>().obsType.ToString ());
					writer.WriteStartElement ("connections");
					foreach (int cons in j.GetComponent<EditorTile>().consList) {
						writer.WriteElementString ("int", cons.ToString());
					}
					writer.WriteEndElement ();
					writer.WriteStartElement ("locks");
					foreach (int locks in j.GetComponent<EditorTile>().locksList) {
						writer.WriteElementString ("int", locks.ToString());
					}
					writer.WriteEndElement ();
					writer.WriteEndElement ();
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndDocument ();
		}
		filePath = null;
    }
	
	private void LoadLevel()
	{
		XmlReaderSettings settings = new XmlReaderSettings();
		settings.IgnoreWhitespace = true;
		using (XmlReader read = XmlReader.Create (filePath, settings)) {
			int i = -1;
			int j = -1;
			bool consGroup = true;
			while (read.Read ()) {
				if (read.IsStartElement ())
				{
					switch (read.Name)
					{
					case "width":
						read.Read ();
						tempW = read.Value;
						gridW = int.Parse (tempW);
						break;
					case "height":
						read.Read ();
						tempH = read.Value;
						gridH = int.Parse (tempH);
						SetGrid();
						break;
					case "y":
						if (j >= 0)
							SetGraphics(map[j][i],mapObs[j][i]);
						j++;
						i = -1;
						Console.WriteLine (j.ToString());
						break;
					case "x":
						if (i >= 0 || (j == gridH-1 && i == gridW-1))
							SetGraphics(map[j][i],mapObs[j][i]);
						else
							SetGraphics(map[j][gridW-1],mapObs[j][gridW-1]);
						Console.WriteLine (i.ToString());
						i++;
						break;
					case "type":
						read.Read ();
						map[j][i].GetComponent<EditorTile>().tileType = int.Parse (read.Value);
						break;
					case "obs":
						read.Read ();
						map[j][i].GetComponent<EditorTile>().obsType = int.Parse (read.Value);
						break;
					case "connections":	
						consGroup = true;
						break;
					case "locks":
						consGroup = false;
						break;
					case "int":
						read.Read ();
						if (consGroup)
							map[j][i].GetComponent<EditorTile>().setElementConnection(read.ReadContentAsInt());
						else
							map[j][i].GetComponent<EditorTile>().setElementLock(read.ReadContentAsInt());
						break;
					}
				}
			}
			read.Close ();
			filePath = null;
		}
	}
	
	void OnGUI () {
		guiInput = false;
		GUI.skin = mainSkin;
		GUI.BeginGroup (new Rect(0,0,Screen.width,Screen.height));
		GUI.Label (new Rect(5,5,60,30), "Width:");
		GUI.Label (new Rect(70,5,60,30), "Height:");
		tempW = GUI.TextField (new Rect(5,35,30,30), tempW, 2);
		tempH = GUI.TextField (new Rect(70,35,30,30), tempH, 2);
		if (GUI.Button (new Rect(135,35,60,30), "Apply")) {
			guiInput = true;
			int intTempH = 0;
			int intTempW = 0;
			try {
				intTempW = int.Parse (tempW);
				intTempH = int.Parse (tempH);
				if (intTempW*intTempH <= 0) {
					tempW = gridW.ToString ();
					tempH = gridH.ToString ();
					guiError = true;
					guiErS = "Greater than zero, please.";
				} else {
					gridW = intTempW;
					gridH = intTempH;
					SetGrid();
				}
			} catch (FormatException b) {
				tempW = gridW.ToString ();
				tempH = gridH.ToString ();
				guiError = true;
				guiErS = "Only whole numbers, please.";
			}
		}
		
		if (guiError) {
			GUI.Label (new Rect (5, 70, 100, 60), guiErS);
			if (GUI.Button (new Rect(5, 135, 60, 30), "Okay.")) {
				guiInput = true;
				guiError = false;
				guiErS = "";
			}
		}
		
		GUI.Label (new Rect(Screen.width-(32*2)-10,5,50,30), "Tiles:");
		for (int i = 0; i < buttonGfx.Length-3; i++) {
			GUIStyle buttonStyle;
			if (i == activeSelection)
				buttonStyle = activeButton;
			else
				buttonStyle = passiveButton;
			if (GUI.Button (new Rect(Screen.width-(32*(2-i%2))-10,(32+5)*(i/2)+40,32,32), buttonGfx[i], buttonStyle)) {
				guiInput = true;
				activeSelection = i;
			}
		}
		
		GUI.Label (new Rect(Screen.width-(32*2)-10,(32+5)*(buttonGfx.Length/2-1)+40,70,30), "Spawns:");
		for (int i = buttonGfx.Length-3; i < buttonGfx.Length; i++) {
			GUIStyle buttonStyle;
			if (i == activeSelection)
				buttonStyle = activeButton;
			else
				buttonStyle = passiveButton;
			if (GUI.Button (new Rect(Screen.width-(32*(2-(i+3)%2))-10,(32+5)*((i+3)/2)+40,32,32), buttonGfx[i], buttonStyle)) {
				guiInput = true;
				activeSelection = i;
			}
		}
		
		if (GUI.Button (new Rect(200,5,90,60), "Save") && (!saveFile && !loadFile) ) { // Changed '||' to '&&' so filebrower won't open both load and save windows
			guiInput = true;
			saveFile = true;
			browser = BrowserSetup ();
		}
		
		if (GUI.Button (new Rect(295,5,90,60), "Load") && (!saveFile && !loadFile) ) { // Changed '||' to '&&' so filebrower won't open both load and save windows
			guiInput = true;
			loadFile = true;
			browser = BrowserSetup ();
		}
		
		GUI.Label (new Rect(Screen.width-(32*2)-40,(32+5)*(buttonGfx.Length/2)+200,90,30), "Connections:");
		connections = GUI.TextField(new Rect(Screen.width-(32*2)-40,(32+5)*(buttonGfx.Length/2)+240,90,30),connections);
		connections = Regex.Replace(connections, @"[^,0-9]", "");
		if (GUI.Button (new Rect(Screen.width-(32*2)-80,(32+5)*(buttonGfx.Length/2)+240,40,30), "Set"))
			activeSelection = 20;
		
		GUI.Label (new Rect(Screen.width-(32*2)-40,(32+5)*(buttonGfx.Length/2)+280,150,30), "Lock Groups:");
		lockGroups = GUI.TextField(new Rect(Screen.width-(32*2)-40,(32+5)*(buttonGfx.Length/2)+320,90,30),lockGroups);
		lockGroups = Regex.Replace(lockGroups, @"[^,0-9]", "");
		if (GUI.Button (new Rect(Screen.width-(32*2)-80,(32+5)*(buttonGfx.Length/2)+320,40,30), "Set"))
			activeSelection = 21;
		
		if (loadFile) {
			browser.OnGUI ();
			GUI.TextField (new Rect(Screen.width/2-300, 410, 500, 30), browser.CurrentDirectory);
			if (filePath != null) {
				loadFile = false;
				LoadLevel();
			}
		}
		
		if (saveFile) {
			browser.OnGUI ();
			fileName = GUI.TextField (new Rect(Screen.width/2-300, 510, 300, 30), fileName);
			GUI.Label (new Rect(Screen.width/2, 510, 40, 30), ".xml");
			if (filePath != null) {
				saveFile = false;
				WriteXML ();
			}
		}
		
		if (GUI.Button (new Rect(390,5,90,60), "Return to Game")) {
			guiInput = true;
			Application.LoadLevel (0);
		}
		
		GUI.EndGroup ();
	}
}

