using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class TileClass : Tile
{
	public bool powered = false;
	public bool locked = false;
	public bool used = false;
	public int wait = 0;
	public bool hold = false;
	public int nextWait;
	public float xiso, yiso;
	public int[] lockGroup;
	public int gridx, gridy;
	public GameObject gfx;
	
	protected int tileSet;
	protected OTAnimatingSprite os;
	protected Dictionary<int, List<Tile>>[] connections = new []
	{	new Dictionary<int, List<Tile>>(),
		new Dictionary<int, List<Tile>>()	};
	protected Dictionary<int,List<Tile>>[] locks = new []
	{	new Dictionary<int, List<Tile>>(),
		new Dictionary<int, List<Tile>>()	};
	
	public TileClass (int gx, int gy, int tSet)
	{
		gridx = gx;
		gridy = gy;
		tileSet = tSet;
		gfx = OT.CreateObject("WorldTiles");
		os = gfx.GetComponent<OTAnimatingSprite>();
		xiso = (-gy+gx)*GameManager.getTileW();
		yiso = (-gy-gx)*GameManager.getTileW()/2F;
		os.position = new Vector2 (xiso, yiso);
		os.PlayOnce (this.GetType().Name+tSet.ToString());
		
		if (walkable())
			os.depth = 1;
	}
	
	public virtual bool walkable () {
		return false;
	}
	
	public virtual bool walkable (Obstacle o) {
		return walkable ();
	}
	
	public OTSprite graphic {
		get { return os; }
	}
	public int xgrid() {return gridx; }
	
	public int ygrid() {return gridy; }
	
	// Returns the name of the tile
	public string myName()	{
		string tileName = "tile_"+gridx+"_"+gridy;
		return tileName;
	}
	
	public void addConnection(int k, List<Tile> l, bool isSource)
	{
		if (isSource) {
			if (!connections[0].ContainsKey(k))	//this is an out node
				connections[0].Add (k,l); 		//create key if it didn't exist
			else 
				connections[0][k] = l;			//replace list if key already exists
		} else {
			if (!connections[1].ContainsKey(k))	//this is an in node
				connections[1].Add (k,l);
			else 
				connections[1][k] = l;
		}
	}
	
	public void addLock(int k, List<Tile> l, bool isSource)
	{
		if (isSource) {					//this is an out node
			if (!locks[0].ContainsKey(k))
				locks[0].Add (k,l); 	//create key if it didn't exist
			else 
				locks[0][k] = l;		//replace list if key already exists
		} else {						//this is an in node
			if (!locks[1].ContainsKey(k))
				locks[1].Add (k,l);
			else 
				locks[1][k] = l;
		}
	}
	
	// This function detects if the tile is considered active by looking at the powered state of all 'connections'
	// tiles in the group. Things like buttons and pressure plates provide power.
	public virtual bool isActivated() {
		
		// Also add loop to check locks here. All tiles within a lock group must be 'unlocked' for those tiles
		// to remain 'powered,' whereas only one member of a connection group must be active to power the rest.
		
		foreach (List<Tile> conList in locks[0].Values)
			foreach (Tile t in conList)
				if (((TileClass)t).locked && t != this)
					return false;
		foreach (List<Tile> conList in connections[0].Values)
			foreach (Tile t in conList)
				if (((TileClass)t).powered && t != this)
					return true;
		return false;
	}
	
	public virtual void interact() {}
	
	public virtual void interact(Obstacle a)
	{
		this.interact ();
	}
	
	public virtual void update() {}
	
	public virtual void act(List<Obstacle> objs) {}
}

