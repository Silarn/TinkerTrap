using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;

public class EditorScript : MonoBehaviour {
	
	// Main display and storage
	
	private List<List<GameObject>> map = new List<List<GameObject>>();
	private List<List<GameObject>> mapObs = new List<List<GameObject>>();
	private string[] tileList = new string[] {"Wall","Floor","Door","Button","Plate","Electrified", "Generator", "Source"};
	private string[] obsList = new string[] {"Spawn", "Box", "Battery"};
	private string activeSelection = "empty";
	private int activeSet = 0;
	private int gridW = 10;
	private int gridH = 10;
	private string tempW = "10";
	private string tempH = "10";
	private List<int> activeCons = new List<int>();
	private List<int> activeLocks = new List<int>();
	
	// Paint/Marquee switch and selection storage
	
	private bool paintMode = true;
	private List<GameObject> boxedSelection = new List<GameObject>();
	private bool showModeList = false;
	private int modeEntry = 0;
	private GUIContent[] modeDropdown = {new GUIContent("Paint"), new GUIContent("Marquee")};
	private bool modePicked = false;
	
	// For GUI display/control
	
	private bool guiError = false;
	private bool loadFile = false;
	private bool saveFile = false;
	private bool guiInput = false;
	private FileBrowser browser;
	private string guiErS = "";
	protected string filePath;
	protected string fileName = "level";
	private double fileVersion;
	
	// Set up basic GUI
	
	public GUIStyle activeButton;
	public GUIStyle passiveButton;
	public GUISkin mainSkin;
	
	// For connection/lock selection dropdowns
	
	private bool showConList = false;
	private bool showLockList = false;
	private bool showViewList = false;
	private int connectionEntry = 1;
	private int lockEntry = 1;
	private int viewEntry = 0;
	private GUIContent[] consDropdown = {new GUIContent("1")};
	private GUIContent[] locksDropdown = {new GUIContent("1")};
	private GUIContent[] viewDropdown = {new GUIContent("Show Active"), new GUIContent("Show All")};
	private bool conPicked = false;
	private bool lockPicked = false;
	private bool viewPicked = false;
	
	// Drawing lines
	
	private bool mouse0Active = true;
	private bool validAnchor = false; // Control variable to help set tracer line
	private Vector3 anchor;
	private GameObject anchorYX;
	private List<GameObject> lineList = new List<GameObject>();
	
	private GameObject line;
	// Use this for initialization
	void Start () {
		line = GameObject.Find ("mouseLine");
		line.GetComponent<LineRenderer>().SetColors(Color.white, Color.blue);
		SetGrid();
		
		// Sync active lists to dropdown content
		activeCons.Add(1);
		activeLocks.Add(1);
	}
	private bool Linkable(GameObject a, GameObject b)
	{
		bool linkable = true;
		if (a == b) 
			linkable = false;
		else if (a.GetComponent<EditorTile>().tileType == "Wall" || a.GetComponent<EditorTile>().tileType == "Floor")
			linkable = false;
		else if (b.GetComponent<EditorTile>().tileType == "Wall" || b.GetComponent<EditorTile>().tileType == "Floor")
			linkable = false;
		
		return linkable;
	}
	
	private bool validSave()
	{
		bool saveMe = true;
		int countPlayerSpawn = 0;
		
		foreach (List<GameObject> g in map)
		{	foreach (GameObject o in g)
			{
				if (o.GetComponent<EditorTile>().obsType == "Spawn")
					countPlayerSpawn++;
			}
		}
		if (countPlayerSpawn != 1)
			saveMe = false;
		return saveMe;
	}
	
	private void AddLineObject (GameObject a, GameObject b)
	{
		lineList.Add (new GameObject(lineList.Count.ToString()));
		lineList[lineList.Count - 1].AddComponent<LineRenderer>().SetVertexCount(2);
		lineList[lineList.Count - 1].GetComponent<LineRenderer>().material = new Material (Shader.Find("Particles/Additive"));
		lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetWidth (4f, 2f);
		lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetPosition(0,ReturnTileCenter(a.transform.position));
		lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetPosition(1,ReturnTileCenter(b.transform.position));
	}
	
	private void ClearLineObjects ()
	{
		while (lineList.Count > 0)
		{
			Destroy (lineList[lineList.Count - 1]);
			lineList.RemoveAt(lineList.Count - 1);
		}
	}
	
	private Vector3 ReturnTileCenter (Vector3 v)
	{
		Vector3 change = new Vector3(v.x+16,v.y-16,v.z-10);
		return change;
	}
	
	private void DrawMouse(Vector3 a, Vector3 b)
	{
		line.GetComponent<LineRenderer>().SetColors(Color.white, Color.blue);
		line.GetComponent<LineRenderer>().SetWidth(6f,2f);
		line.GetComponent<LineRenderer>().SetVertexCount(2);
		line.GetComponent<LineRenderer>().SetPosition(0, a);
		line.GetComponent<LineRenderer>().SetPosition(1, b);
	}
	
	private void DragBox(Vector3 a, Vector3 c)
	{	/* a b
		 * d c
		 */
		line.GetComponent<LineRenderer>().SetColors(Color.cyan,Color.cyan);
		line.GetComponent<LineRenderer>().SetWidth(2f,2f);
		line.GetComponent<LineRenderer>().SetVertexCount(5);
		line.GetComponent<LineRenderer>().SetPosition(0,a);
		
		Vector3 b = new Vector3(c.x,a.y,a.z);
		line.GetComponent<LineRenderer>().SetPosition(1,b);
		
		line.GetComponent<LineRenderer>().SetPosition(2,c);
		
		Vector3 d = new Vector3(a.x,c.y,a.z);
		line.GetComponent<LineRenderer>().SetPosition(3,d);
		
		line.GetComponent<LineRenderer>().SetPosition(4,a);
	}
	
	private void ClearBoxedSelection()
	{
		while (boxedSelection.Count > 0)
		{
			boxedSelection.RemoveAt(boxedSelection.Count - 1);
		}
	}
	
	private void SetBoxedSelection(GameObject a, GameObject b)
	{
		ClearBoxedSelection();
		int startX = (int)a.transform.position.x/32;
		int startY = (int)(a.transform.position.y/-32);
		int endX = (int)b.transform.position.x/32;
		int endY = (int)b.transform.position.y/-32;
		
		bool stop = false;
		
		while (!stop)
		{
			bool nextL = false;
			int X = startX;
			while (!nextL)
			{
				boxedSelection.Add (map[startY][X]);
				if (X ==  endX)
					nextL = true;
				else
					if (startX < endX)
						X++;
					else 
						X--;
			}
			if (startY == endY)
				stop = true;
			else
				if (startY < endY)
					startY++;
				else
					startY--;
		}
	}
	// Set first empty active as selection, otherwise create new active as selection
	private void CheckForEmptyActive()
	{
		int emptyActive = -1;
		
		if (activeSelection == "conn")
		{
			foreach (int act in activeCons)
			{
				bool linkFound = false;
				foreach (List<GameObject> g in map)
				{
					foreach (GameObject o in g)
					{
						if (o.GetComponent<EditorTile>().consOut.Contains(act))
						{
							linkFound = true;
						}
					}
				}
				if (!linkFound){
					emptyActive = act;
					break;
				}
			}
			if (emptyActive < 0)
			{
				activeCons.Add(activeCons.Count + 1);
				consDropdown = new GUIContent[activeCons.Count];
				for (int i = 0; i < consDropdown.Length; i++)
					consDropdown[i] = new GUIContent(activeCons[i].ToString());
				connectionEntry = activeCons[activeCons.Count - 1];
			}
			else{
				connectionEntry = activeCons[emptyActive - 1];
			}
		}
		else if (activeSelection == "lock")
		{
			foreach (int act in activeLocks)
			{
				bool linkFound = false;
				foreach (List<GameObject> g in map)
				{
					foreach (GameObject o in g)
					{
						if (o.GetComponent<EditorTile>().locksOut.Contains(act))
						{
							linkFound = true;
						}
					}
				}
				if (!linkFound){
					emptyActive = act;
					break;
				}
			}
			if (emptyActive < 0)
			{
				activeLocks.Add(activeLocks.Count + 1);
				locksDropdown = new GUIContent[activeLocks.Count];
				for (int i = 0; i < locksDropdown.Length; i++)
					locksDropdown[i] = new GUIContent(activeLocks[i].ToString());
				lockEntry = activeLocks[activeLocks.Count - 1];
			}
			else {
				lockEntry = activeLocks[emptyActive - 1];
			}
		}
	}
	
	private void DrawLinks()
	{
		ClearLineObjects();
		if (viewEntry == 0) // "Show Active"
		{
			if (activeSelection == "conn")
				DrawConnections(connectionEntry);
			else if (activeSelection == "lock")
				DrawLocks(lockEntry);
		}
		else if (viewEntry == 1) // "Show All"
		{
			foreach (int c in activeCons)
				DrawConnections(c);
			foreach (int l in activeLocks)
				DrawLocks(l);
		}
	}
	
	private void CheckAllLinks() {
		if (viewEntry == 0) {
			viewEntry = 1;
			DrawLinks();
			viewEntry = 0;
			DrawLinks();
		} else
			DrawLinks();	
	}
	
	private void DrawConnections(int num)
	{
		bool outFound = false;
		foreach (List<GameObject> g in map)
		{
			foreach (GameObject o in g)
			{
				// Checking all tiles for num, using consOut
				if ( o.GetComponent<EditorTile>().consOut.Contains(num))
				{
					outFound = true;
					bool inFound = false;
					foreach (List<GameObject> g2 in map)
					{
						foreach (GameObject o2 in g2)
						{
							// Check all tiles again for num, using consIn
							if (o2.GetComponent<EditorTile>().consIn.Contains(num))
							{
								inFound = true;
								AddLineObject(o,o2);
								if (num == connectionEntry) // Visual distinction for modifiable entry
								{
									lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetColors(new Color(197f/255f,244f/255f,184f/255f),Color.green);
									lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetWidth(6f, 2f);
								}
								else
									lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetColors (new Color(197f/255f,244f/255f,184f/255f), new Color(22f/255f,148f/255f,64f/255f));
							}
						}
					}
					if (!inFound) {
						o.GetComponent<EditorTile>().consOut.Remove(num); // Removes lone Outs
					}
				}
			}
		}
		if (!outFound) { // If no Outs found, check for and remove any Ins
			foreach (List<GameObject> g in map)
			{
			foreach (GameObject o in g)
				{
					if (o.GetComponent<EditorTile>().consIn.Contains(num))
						o.GetComponent<EditorTile>().consIn.Remove (num);
				}
			}
		}
	}
	
	private void DrawLocks(int num)
	{
		bool outFound = false;
		foreach (List<GameObject> g in map)
		{
			foreach (GameObject o in g)
			{
				// Checking all tiles for num, using locksOut
				if ( o.GetComponent<EditorTile>().locksOut.Contains(num))
				{
					outFound = true;
					bool inFound = false;
					foreach (List<GameObject> g2 in map)
					{
						foreach (GameObject o2 in g2)
						{
							// Check all tiles again for num, using locksin
							if (o2.GetComponent<EditorTile>().locksIn.Contains(num))
							{
								inFound = true;
								AddLineObject(o,o2);
								if (num == lockEntry) // Visual distinction for modifiable entry
								{
									lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetColors(new Color(197f/255f,244f/255f,184f/255f),new Color(206f/255f,58f/255f,32f/255f,255f/255f));
									lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetWidth(6f, 2f);
								}
								else
									lineList[lineList.Count - 1].GetComponent<LineRenderer>().SetColors (new Color(236f/255f,243f/255f,183f/255f,255f/255f), new Color(106f/255f,58f/255f,32f/255f,255f/255f));
							}
						}
					}
					if (!inFound) {
						o.GetComponent<EditorTile>().locksOut.Remove(num); // Removes lone Outs
					}
				}
			}
		}
		if (!outFound) { // If no Outs found, check for and remove any Ins
			foreach (List<GameObject> g in map)
			{
			foreach (GameObject o in g)
				{
					if (o.GetComponent<EditorTile>().locksIn.Contains(num))
						o.GetComponent<EditorTile>().locksIn.Remove (num);
				}
			}
		}
	}
	
	private void SetGrid() {
		bool downSized = false;
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
					map[i][j].GetComponent<OTSprite>().position = new Vector2(j*32f,i*-32f);
					mapObs[i][j].GetComponent<OTSprite>().position = new Vector2(j*32f,i*-32f);
					map[i][j].GetComponent<OTSprite>().frameName = "ed_Wall0";
					mapObs[i][j].GetComponent<OTSprite>().frameName = "empty";
					mapObs[i][j].GetComponent<OTSprite>().depth = -1;
				}
			}
			if (gridW < map[i].Count) {
				downSized = true;
				foreach (GameObject t in map[i].GetRange (gridW,map[i].Count-gridW))
					Destroy (t);
				foreach (GameObject o in mapObs[i].GetRange (gridW,map[i].Count-gridW))
					Destroy (o);
				map[i].RemoveRange (gridW,map[i].Count-gridW);
				mapObs[i].RemoveRange (gridW,mapObs[i].Count-gridW);
			}
		}
		if (gridH < map.Count) {
			downSized = true;
			foreach (List<GameObject> l in map.GetRange (gridH,map.Count-gridH))
				foreach (GameObject t in l)
					Destroy (t);
			foreach (List<GameObject> l in mapObs.GetRange (gridH,map.Count-gridH))
				foreach (GameObject o in l)
					Destroy (o);
			map.RemoveRange (gridH,map.Count-gridH);
			mapObs.RemoveRange (gridH,map.Count-gridH);
		}
		// Redraw links on downsize to clear any resulting broken links.
		if (downSized == true) 
			CheckAllLinks();
	}
	
	// Update is called once per frame
	void Update() {
		if (!paintMode && activeSelection != "conn" && activeSelection != "lock" && !guiError && !loadFile && !saveFile && !guiInput) {
			if (Input.GetMouseButtonDown (0)) {
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					line.GetComponent<LineRenderer>().SetColors(Color.white, Color.blue);
					validAnchor = true;
					anchorYX = map[selectY][selectX];
					anchor = ReturnTileCenter(map[selectY][selectX].transform.position);
				}
			} 
			else if (Input.GetMouseButtonUp (0) && validAnchor == true) {
				validAnchor = false;
			}
			if (Input.GetMouseButton (0) && validAnchor == true) {
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					SetBoxedSelection(anchorYX,map[selectY][selectX]);
					Vector3 pointB = new Vector3(map[selectY][selectX].transform.position.x,map[selectY][selectX].transform.position.y,map[selectY][selectX].transform.position.z);
					if (anchor.x < map[selectY][selectX].transform.position.x)
						if (anchor.y > map[selectY][selectX].transform.position.y)
							DragBox(new Vector3(anchor.x - 16,anchor.y + 16, anchor.z),new Vector3(pointB.x + 32, pointB.y - 32, pointB.z));
						else
							DragBox(new Vector3(anchor.x - 16,anchor.y - 16, anchor.z),new Vector3(pointB.x + 32, pointB.y, pointB.z));
					else
						if (anchor.y > map[selectY][selectX].transform.position.y)
							DragBox(new Vector3(anchor.x + 16,anchor.y + 16, anchor.z),new Vector3(pointB.x, pointB.y - 32, pointB.z));
						else
							DragBox(new Vector3(anchor.x + 16,anchor.y - 16, anchor.z),new Vector3(pointB.x, pointB.y, pointB.z));
				}
			}
			
		} else if (tileList.Contains (activeSelection) && !guiError && !loadFile && !saveFile && !guiInput)
		{
			if (Input.GetMouseButton (0)) {
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					SetTypeByDraw(map[selectY][selectX]);
					SetGraphics(map[selectY][selectX], mapObs[selectY][selectX]);
				}
			} else if (Input.GetMouseButton (1)) {
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					map[selectY][selectX].GetComponent<EditorTile>().ClearConstraints();
					SetTypeByDraw(map[selectY][selectX]);
					SetGraphics(map[selectY][selectX], mapObs[selectY][selectX]);
					CheckAllLinks();
				}
			}
		} else if (obsList.Contains (activeSelection) && !guiError && !loadFile && !saveFile && !guiInput) {
			if (Input.GetMouseButton (0)) {
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					SetObsByDraw(map[selectY][selectX]);
					SetGraphics(map[selectY][selectX], mapObs[selectY][selectX]);
				}
			} else if (Input.GetMouseButton (1)) {
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					map[selectY][selectX].GetComponent<EditorTile>().ClearConstraints();
					SetObsByDraw(map[selectY][selectX]);
					SetGraphics(map[selectY][selectX], mapObs[selectY][selectX]);
					CheckAllLinks();
				}
			}
		} else if (activeSelection == "empty" && Input.GetMouseButton (0) && !guiError && !loadFile && !saveFile && !guiInput) {
			Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
			int selectX = (int)(Math.Floor (mouseLocation.x/32));
			int selectY = (int)(Math.Floor (mouseLocation.y/-32));
			if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
				map[selectY][selectX].GetComponent<EditorTile>().obsType = "";
				SetGraphics(map[selectY][selectX], mapObs[selectY][selectX]);
			}
		} else if (Input.GetMouseButtonDown (0) && !guiError && !loadFile && !saveFile && !guiInput) {
			Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
			int selectX = (int)(Math.Floor (mouseLocation.x/32));
			int selectY = (int)(Math.Floor (mouseLocation.y/-32));
			if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
				mouse0Active = true;
				line.GetComponent<LineRenderer>().SetColors(Color.white, Color.blue);
				validAnchor = true;
				anchorYX = map[selectY][selectX];
				anchor = ReturnTileCenter(map[selectY][selectX].transform.position);
			}
		} else if (activeSelection == "conn" || activeSelection == "lock"){
			if (Input.GetMouseButtonDown (1) && !guiError && !loadFile && !saveFile && !guiInput)
			{
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					mouse0Active = false;
					line.GetComponent<LineRenderer>().SetColors(Color.white, Color.red);
					validAnchor = true;
					anchorYX = map[selectY][selectX];
					anchor = ReturnTileCenter(map[selectY][selectX].transform.position);
				}
			}
			if ((Input.GetMouseButton (0) || Input.GetMouseButton(1)) && !guiError && !loadFile && !saveFile && !guiInput && validAnchor == true)
			{
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				DrawMouse(anchor,mouseLocation);
			}
			if (mouse0Active && Input.GetMouseButtonUp (0) && !guiError && !loadFile && !saveFile && !guiInput)
			{
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				validAnchor = false;
				if (Input.GetKey(KeyCode.LeftControl)){ // Holding control creates new link in a new group
					if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
						if (activeSelection == "conn" && Linkable(map[selectY][selectX], anchorYX)){
							CheckForEmptyActive();
							// Set link
							map[selectY][selectX].GetComponent<EditorTile>().consIn.Add(connectionEntry);
							anchorYX.GetComponent<EditorTile>().consOut.Add(connectionEntry);
							DrawLinks();
						}
						else if (activeSelection == "lock" && Linkable(map[selectY][selectX], anchorYX)) {
							CheckForEmptyActive();
							//Set link
							map[selectY][selectX].GetComponent<EditorTile>().locksIn.Add(lockEntry);
							anchorYX.GetComponent<EditorTile>().locksOut.Add (lockEntry);
							DrawLinks();
						}
					}
				}
				else {
					if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
						// Set the connection (20) or lockgroup (21)
						if (activeSelection == "conn" && Linkable(map[selectY][selectX], anchorYX)) 
						{	if (!map[selectY][selectX].GetComponent<EditorTile>().consIn.Contains(connectionEntry)&& !map[selectY][selectX].GetComponent<EditorTile>().consOut.Contains(connectionEntry)) //Don't add node if one already exists
								map[selectY][selectX].GetComponent<EditorTile>().consIn.Add(connectionEntry);
							if (!anchorYX.GetComponent<EditorTile>().consOut.Contains(connectionEntry) && !anchorYX.GetComponent<EditorTile>().consIn.Contains(connectionEntry))
								anchorYX.GetComponent<EditorTile>().consOut.Add(connectionEntry);
							DrawLinks();
						} 
						else if (activeSelection == "lock" && Linkable(map[selectY][selectX], anchorYX))
						{	if (!map[selectY][selectX].GetComponent<EditorTile>().locksIn.Contains(lockEntry) && !map[selectY][selectX].GetComponent<EditorTile>().locksOut.Contains(lockEntry))
								map[selectY][selectX].GetComponent<EditorTile>().locksIn.Add(lockEntry);
							if (!anchorYX.GetComponent<EditorTile>().locksOut.Contains(lockEntry) && !anchorYX.GetComponent<EditorTile>().locksIn.Contains(lockEntry))
								anchorYX.GetComponent<EditorTile>().locksOut.Add (lockEntry);
							DrawLinks();
						}
					}
				}
				DrawMouse(anchor,anchor); // Removing mouse line from view
			}
			else if (!mouse0Active && Input.GetMouseButtonUp (1) && !guiError && !loadFile && !saveFile && !guiInput) 
			{
				Vector3 mouseLocation = camera.ScreenToWorldPoint (Input.mousePosition);
				int selectX = (int)(Math.Floor (mouseLocation.x/32));
				int selectY = (int)(Math.Floor (mouseLocation.y/-32));
				validAnchor = false;
				if ((selectY >= 0 && selectY < gridH) && (selectX >= 0 && selectX < gridW)) {
					// Remove connection or lockgroup
					if (activeSelection == "conn") {
						if (map[selectY][selectX].GetComponent<EditorTile>().consIn.Contains(connectionEntry))
							map[selectY][selectX].GetComponent<EditorTile>().consIn.Remove(connectionEntry);
						if (anchorYX.GetComponent<EditorTile>().consOut.Contains(connectionEntry))
							anchorYX.GetComponent<EditorTile>().consOut.Remove(connectionEntry);
						DrawLinks();
					} else if (activeSelection == "lock") {
						if (map[selectY][selectX].GetComponent<EditorTile>().locksIn.Contains(lockEntry))
							map[selectY][selectX].GetComponent<EditorTile>().locksIn.Remove(lockEntry);
						if (anchorYX.GetComponent<EditorTile>().locksOut.Contains(lockEntry))
							anchorYX.GetComponent<EditorTile>().locksOut.Remove(lockEntry);
						DrawLinks();
					}							
				}
				DrawMouse(anchor,anchor);
			}
		}
	}
	
	void LateUpdate () {
			transform.position = new Vector3(camera.orthographicSize*((float)(Screen.width)/(float)(Screen.height))-20, -camera.orthographicSize*(1-150f/Screen.height), transform.position.z);
	}
	
	private void SetTypeByDraw(GameObject a)
	{
		//Stop the case where an obstacle is present and the pending tile type is a wall/door
		if(!(!(a.GetComponent<EditorTile>().obsType == "") && (activeSelection == "Wall" || activeSelection == "Door" || /*activeSelection == "Electrified" ||*/
																activeSelection == "Button" || activeSelection == "Generator" /*|| activeSelection == "Source"*/)))
		{	// Stop Wall or Floor values if any in/out Link is present
			if (!((activeSelection == "Wall" || activeSelection == "Floor")  && (a.GetComponent<EditorTile>().consIn.Count != 0 || a.GetComponent<EditorTile>().consOut.Count != 0 
				|| a.GetComponent<EditorTile>().locksIn.Count != 0 || a.GetComponent<EditorTile>().locksOut.Count != 0))) {
				a.GetComponent<EditorTile>().tileType = activeSelection;
				a.GetComponent<EditorTile>().tileSet = activeSet;
			}
		}
	}
	
	private void SetObsByDraw(GameObject a)
	{
		//Stop the case where the tile type is a wall/door
		if (!(a.GetComponent<EditorTile>().tileType == "Wall" || a.GetComponent<EditorTile>().tileType == "Door" || /*a.GetComponent<EditorTile>().tileType == "Electrified" ||*/
				a.GetComponent<EditorTile>().tileType == "Button" || a.GetComponent<EditorTile>().tileType == "Generator" /*|| a.GetComponent<EditorTile>().tileType == "Source"*/))
		{
			a.GetComponent<EditorTile>().obsType = activeSelection;
		}
	}
	
	private void SetGraphics(GameObject a, GameObject b)
	{
		a.GetComponent<OTSprite>().frameName =
			"ed_"+a.GetComponent<EditorTile>().tileType+
			a.GetComponent<EditorTile>().tileSet.ToString ();
		
		if (a.GetComponent<EditorTile>().obsType == "")
			b.GetComponent<OTSprite>().frameName = "empty";
		else {
			b.GetComponent<OTSprite>().frameName =
				"ed_"+a.GetComponent<EditorTile>().obsType+"0";
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
		if (filePath == null)
			loadFile = false;
		filePath = path;
	}
	
	private void ReturnSavePath(string path)
	{
		if (filePath == null)
			saveFile = false;
		filePath = path + "/" + fileName + ".xml";
	}
	
	private void WriteXML()
    {
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		using (XmlWriter writer = XmlWriter.Create(filePath,settings)) {
			writer.WriteStartDocument ();
			writer.WriteStartElement ("document");
			writer.WriteElementString ("version", "3.0");
			writer.WriteElementString ("width", gridW.ToString ());
			writer.WriteElementString ("height", gridH.ToString ());
			foreach (List<GameObject> i in map) {
				writer.WriteStartElement ("y");
				foreach (GameObject j in i) {
					writer.WriteStartElement ("x");
					writer.WriteElementString ("type",j.GetComponent<EditorTile>().tileType);
					writer.WriteElementString ("tset",j.GetComponent<EditorTile>().tileSet.ToString ());
					writer.WriteElementString ("obs",j.GetComponent<EditorTile>().obsType);
					writer.WriteStartElement ("connections");
					foreach (int cons in j.GetComponent<EditorTile>().consIn) {
						writer.WriteElementString ("in", cons.ToString());
					}
					foreach (int cons in j.GetComponent<EditorTile>().consOut) {
						writer.WriteElementString ("out", cons.ToString());
					}
					writer.WriteEndElement ();
					writer.WriteStartElement ("locks");
					foreach (int locks in j.GetComponent<EditorTile>().locksIn) {
						writer.WriteElementString ("in", locks.ToString());
					}
					foreach (int locks in j.GetComponent<EditorTile>().locksOut) {
						writer.WriteElementString ("out", locks.ToString());
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
		activeCons.Clear ();
		activeLocks.Clear ();
		foreach (List<GameObject> t in map) {
			foreach (GameObject i in t) {
				i.GetComponent<EditorTile>().consIn.Clear ();
				i.GetComponent<EditorTile>().consOut.Clear ();
				i.GetComponent<EditorTile>().locksIn.Clear ();
				i.GetComponent<EditorTile>().locksOut.Clear ();
			}
		}
		using (XmlReader read = XmlReader.Create (filePath, settings)) {
			int i = -1;
			int j = -1;
			bool consGroup = true;
			while (read.Read ()) {
				if (read.IsStartElement ())
				{
					switch (read.Name)
					{
					case "version":
						read.Read ();
						fileVersion = read.ReadContentAsDouble ();
						break;
					case "width":
						if (fileVersion <= 2.0) {
							read.Close ();
							LoadOldLevel ();
							break;
						}
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
						map[j][i].GetComponent<EditorTile>().tileType = read.Value;
						break;
					case "tset":
						read.Read ();
						map[j][i].GetComponent<EditorTile>().tileSet = int.Parse (read.Value);
						break;
					case "obs":
						if (!read.IsEmptyElement) {
							read.Read ();
							map[j][i].GetComponent<EditorTile>().obsType = read.Value;
						}
						break;
					case "connections":
						Debug.Log ("This is a connection...");
						consGroup = true;
						break;
					case "locks":
						Debug.Log ("This is a lock...");
						consGroup = false;
						break;
					case "in":
						read.Read ();
						int node = read.ReadContentAsInt();
						if (consGroup) {
							Debug.Log ("Writing in con...");
							map[j][i].GetComponent<EditorTile>().consIn.Add(node);
							if (!activeCons.Contains (node))
								activeCons.Add (node);
						} else {
							Debug.Log ("Writing in lock...");
							map[j][i].GetComponent<EditorTile>().locksIn.Add(node);
							if (!activeLocks.Contains (node))
								activeLocks.Add (node);
						}
						break;
					case "out":
						read.Read ();
						node = read.ReadContentAsInt();
						if (consGroup) {
							Debug.Log ("Writing out con...");
							map[j][i].GetComponent<EditorTile>().consOut.Add(node);
							if (!activeCons.Contains (node))
								activeCons.Add (node);
						} else {
							Debug.Log ("Writing out lock...");
							map[j][i].GetComponent<EditorTile>().locksOut.Add(node);
							if (!activeLocks.Contains (node))
								activeLocks.Add (node);
						}
						break;
					}
				}
			}
			read.Close ();
			filePath = null;
		}
	}
	
	// Old level loader for conversion purposes
	
	private void LoadOldLevel()
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
						int type = int.Parse (read.Value);
						string tstr = "Wall";
						int tset = 0;
						switch (type) {
						case 0:
							tstr = "Floor";
							break;
						case 2:
							tstr = "Plate";
							break;
						case 3:
						case 6:
							tstr = "Button";
							break;
						case 4:
							tstr = "Door";
							tset = 1;
							break;
						case 5:
							tstr = "Door";
							break;
						}
						map[j][i].GetComponent<EditorTile>().tileType = tstr;
						map[j][i].GetComponent<EditorTile>().tileSet = tset;
						break;
					case "obs":
						read.Read ();
						type = int.Parse (read.Value);
						tstr = "";
						tset = 0;
						switch (type) {
						case 1:
							tstr = "Spawn";
							break;
						case 4:
							tstr = "Box";
							break;
						}
						map[j][i].GetComponent<EditorTile>().obsType = tstr;
						break;
					case "connections":	
						consGroup = true;
						break;
					case "locks":
						consGroup = false;
						break;
					case "int":
						read.Read ();
						int node = read.ReadContentAsInt();
						if (consGroup) {
							map[j][i].GetComponent<EditorTile>().consIn.Add(node);
							map[j][i].GetComponent<EditorTile>().consOut.Add(node);
							if (!activeCons.Contains (node))
								activeCons.Add (node);
						} else {
							map[j][i].GetComponent<EditorTile>().locksIn.Add(node);
							map[j][i].GetComponent<EditorTile>().locksOut.Add(node);
							if (!activeLocks.Contains (node))
								activeLocks.Add (node);
						}
						break;
					case "in":
						read.Read ();
						node = read.ReadContentAsInt();
						if (consGroup) {
							map[j][i].GetComponent<EditorTile>().consIn.Add(node);
							if (!activeCons.Contains (node))
								activeCons.Add (node);
						} else {
							map[j][i].GetComponent<EditorTile>().locksIn.Add(node);
							if (!activeLocks.Contains (node))
								activeLocks.Add (node);
						}
						break;
					case "out":
						read.Read ();
						node = read.ReadContentAsInt();
						if (consGroup) {
							map[j][i].GetComponent<EditorTile>().consOut.Add(node);
							if (!activeCons.Contains (node))
								activeCons.Add (node);
						} else {
							map[j][i].GetComponent<EditorTile>().locksOut.Add(node);
							if (!activeLocks.Contains (node))
								activeLocks.Add (node);
						}
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
			GUI.Label (new Rect (Screen.width/2 - 150, 70, 300, 100), guiErS, "box");
			if (GUI.Button (new Rect(Screen.width/2 - 30, 135, 60, 30), "Okay.")) {
				guiInput = true;
				guiError = false;
				guiErS = "";
			}
		}
		
		if (Popup.List (new Rect(Screen.width - 149,5,70,30), ref showModeList, ref modeEntry, modeDropdown[modeEntry], modeDropdown, activeButton)) {
			modePicked = true;
			activeSelection = "";
			if (modeEntry == 0) {
				paintMode = true;
				line.GetComponent<LineRenderer>().SetVertexCount(0);
				ClearBoxedSelection();
			} else {
				paintMode = false;
			}
		} else
			modePicked = false;
		
		GUI.Label (new Rect(Screen.width - 78,5,72,190), "Tiles", "box");
		//GUI.Label (new Rect(Screen.width-(32*2)-10,5,50,30), "Tiles:");
		for (int i = 0; i < tileList.Length; i++) {
			GUIStyle buttonStyle;
			if (tileList[i] == activeSelection)
				buttonStyle = activeButton;
			else
				buttonStyle = passiveButton;
			Texture tex;
			int tSet = 0;
			if (activeSelection.Equals(tileList[i])) {
				if (Resources.Load ("Editor/ed_"+tileList[i]+(activeSet+1).ToString ()) == null) {
					tex = (Texture) Resources.Load ("Editor/ed_"+tileList[i]+0.ToString (), typeof(Texture));
				} else {
					tex = (Texture) Resources.Load ("Editor/ed_"+tileList[i]+(activeSet+1).ToString (), typeof(Texture));
					tSet = activeSet+1;
				}
			} else
				tex = (Texture) Resources.Load ("Editor/ed_"+tileList[i]+0.ToString (), typeof(Texture));
			if (GUI.Button (new Rect(Screen.width-(32*(2-i%2))-10,(32+5)*(i/2)+40,32,32), tex, buttonStyle)) {
				guiInput = true;
				activeSelection = tileList[i];
				activeSet = tSet;
				if (!paintMode && boxedSelection.Count > 0){
					if (Event.current.button == 0) {
						foreach (GameObject o in boxedSelection){
							SetTypeByDraw(o);
							SetGraphics(o,mapObs[(int)o.transform.position.y/-32][(int)o.transform.position.x/32]);
						}
					} else if (Event.current.button == 1) {
						foreach (GameObject o in boxedSelection){
							o.GetComponent<EditorTile>().ClearConstraints();
							SetTypeByDraw(o);
							SetGraphics(o,mapObs[(int)o.transform.position.y/-32][(int)o.transform.position.x/32]);
						}
						CheckAllLinks();
					}
				} else if (paintMode) {
					// Something to show when selection is active
				}
			}
		}
		
		GUI.Label (new Rect(Screen.width - 78,(32+5)*(tileList.Length/2+1)+40,72,120), "Spawns", "box");
		//GUI.Label (new Rect(Screen.width-(32*2)-10,(32+5)*(tileList.Length/2+1)+40,70,30), "Spawns:");
		for (int i = 0; i < obsList.Length+1; i++) {
			Texture tex;
			int oSet = 0;
			GUIStyle buttonStyle;
			if (i == obsList.Length) {
				buttonStyle = passiveButton;
				tex = (Texture) Resources.Load ("Editor/empty", typeof(Texture));
				if (GUI.Button (new Rect(Screen.width-(32*(2-(i+1)%2))-10,(32+5)*((tileList.Length+i+4)/2)+40,32,32), tex, buttonStyle)) {
					guiInput = true;
					activeSelection = "empty";
					if (!paintMode && boxedSelection.Count > 0){
						foreach (GameObject o in boxedSelection){
							o.GetComponent<EditorTile>().obsType = "";
							SetGraphics(o,mapObs[(int)o.transform.position.y/-32][(int)o.transform.position.x/32]);
						}
					}
				}
			} else {
				if (obsList[i].Equals (activeSelection))
					buttonStyle = activeButton;
				else
					buttonStyle = passiveButton;
				if (activeSelection.Equals(obsList[i])) {
					if (Resources.Load ("Editor/ed_"+obsList[i]+(activeSet+1).ToString ()) == null)
						tex = (Texture) Resources.Load ("Editor/ed_"+obsList[i]+activeSet.ToString (), typeof(Texture));
					else
						tex = (Texture) Resources.Load ("Editor/ed_"+obsList[i]+(activeSet+1).ToString (), typeof(Texture));
				} else
					tex = (Texture) Resources.Load ("Editor/ed_"+obsList[i]+0.ToString (), typeof(Texture));
				if (GUI.Button (new Rect(Screen.width-(32*(2-(i+1)%2))-10,(32+5)*((tileList.Length+i+4)/2)+40,32,32), tex, buttonStyle)) {
					guiInput = true;
					activeSelection = obsList[i];
					activeSet = oSet;
					if (!paintMode && boxedSelection.Count > 0){
						if (Event.current.button == 0) {
							foreach (GameObject o in boxedSelection){
								SetObsByDraw(o);
								SetGraphics(o,mapObs[(int)o.transform.position.y/-32][(int)o.transform.position.x/32]);
							}
						} else if (Event.current.button == 1) {
							foreach (GameObject o in boxedSelection){
								o.GetComponent<EditorTile>().ClearConstraints();
								SetObsByDraw(o);
								SetGraphics(o,mapObs[(int)o.transform.position.y/-32][(int)o.transform.position.x/32]);
							}
							CheckAllLinks();
						}
					}
				}
			}
		}
		
		if (GUI.Button (new Rect(200,5,90,60), "Save") && (!saveFile && !loadFile) ) { // Changed '||' to '&&' so filebrower won't open both load and save windows
			if (validSave()) {
				guiInput = true;
				saveFile = true;
				browser = BrowserSetup ();
			} else {
				guiError = true;
				guiErS = "Spawn must be present and unique.";
			}
		}
		
		if (GUI.Button (new Rect(295,5,90,60), "Load") && (!saveFile && !loadFile) ) { // Changed '||' to '&&' so filebrower won't open both load and save windows
			guiInput = true;
			loadFile = true;
			browser = BrowserSetup ();
		}
		
		if (Popup.List (new Rect(Screen.width - 124,(32+5)*(tileList.Length/2+obsList.Length/2)+170,100,25), ref showViewList, ref viewEntry, viewDropdown[viewEntry], viewDropdown, activeButton)) {
			viewPicked = true;
			DrawLinks();
		} else
			viewPicked = false;
		
		GUI.Label (new Rect(Screen.width - 147,(32+5)*(tileList.Length/2+obsList.Length/2)+208,141,75), "Connections", "box");
		//GUI.Label (new Rect(Screen.width-(32*2)-40,(32+5)*(tileList.Length/2+obsList.Length/2)+200,90,30), "Connections:");
	
		if (Popup.List (new Rect(Screen.width - 99,(32+5)*(tileList.Length/2+obsList.Length/2)+240,90,30), ref showConList, ref connectionEntry, new GUIContent(connectionEntry.ToString()), consDropdown, activeButton)) {
			conPicked = true;
			connectionEntry = int.Parse (consDropdown[connectionEntry].text);
			activeSelection = "conn";
			ClearBoxedSelection();
			DrawLinks();
		} else
			conPicked = false;
	
		if (GUI.Button (new Rect(Screen.width - 144,(32+5)*(tileList.Length/2+obsList.Length/2)+240,45,30), "New")) {
			activeSelection = "conn";
			ClearBoxedSelection();
			CheckForEmptyActive();
			DrawLinks();
		}
		
		GUI.Label (new Rect(Screen.width - 147,(32+5)*(tileList.Length/2+obsList.Length/2)+288,141,75), "Lock Groups", "box");
		//GUI.Label (new Rect(Screen.width-(32*2)-40,(32+5)*(tileList.Length/2+obsList.Length/2)+280,150,30), "Lock Groups:");
		
		if (Popup.List (new Rect(Screen.width - 99,(32+5)*(tileList.Length/2+obsList.Length/2)+320,90,30), ref showLockList, ref lockEntry, new GUIContent(lockEntry.ToString()), locksDropdown, activeButton)) {
			lockPicked = true;
			lockEntry = int.Parse (locksDropdown[lockEntry].text);
			activeSelection = "lock";
			ClearBoxedSelection();
			DrawLinks();
		} else
			lockPicked = false;
		
		if (GUI.Button (new Rect(Screen.width - 144,(32+5)*(tileList.Length/2+obsList.Length/2)+320,45,30), "New")) {
			activeSelection = "lock";
			ClearBoxedSelection();
			CheckForEmptyActive();
			DrawLinks();
		}
		
		if (loadFile) {
			Debug.Log ("Trying to load file loader window...");
			browser.OnGUI ();
			GUI.TextField (new Rect(Screen.width/2-300, 410, 500, 30), browser.CurrentDirectory);
			if (filePath != null) {
				loadFile = false;
				LoadLevel();
				
				consDropdown = new GUIContent[activeCons.Count];
				for (int i = 0; i < consDropdown.Length; i++)
					consDropdown[i] = new GUIContent(activeCons[i].ToString());
			
				locksDropdown = new GUIContent[activeLocks.Count];
				for (int i = 0; i < locksDropdown.Length; i++)
					locksDropdown[i] = new GUIContent(activeLocks[i].ToString());
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

