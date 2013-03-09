using System;
using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;

public class EditorTile : MonoBehaviour
{
    private struct variables
    {
        public int x;
        public int y;
        public int tileType;
        public int obsType;
        public List<int> connections;
        public List<int> lockGroups;

        public variables(int a, int b)
        {
            x = a;
            y = b;
            tileType = 1;
            obsType = 0;
            connections = new List<int>();
            lockGroups = new List<int>();
        }
		
        public variables(variables source) //copy constructor
        {
            x = source.x;
            y = source.y;
            tileType = source.tileType;
            obsType = source.obsType;
            connections = new List<int>(source.connections);
            lockGroups = new List<int>(source.lockGroups);
        }
    }
	
    private variables tileData;
	
	void Start() {
		createTile();
	}

    private void createTile() //: base()
    {
        tileData = new variables(0, 0);
		
        /*this.SetBounds(25 + a * 25, 40 + b * 25, 25, 25); // Location on window and size
        this.Text = tileData.tileType.ToString();*/
    }
	
	public int tileType {
		get { return tileData.tileType; }
		set { tileData.tileType = value; }
	}
	
	public int obsType {
		get { return tileData.obsType; }
		set { tileData.obsType = value; }
	}
	
	public int xPos {
		get { return tileData.x; }
		set { tileData.x = value; }
	}
	
	public int yPos {
		get { return tileData.y; }
		set { tileData.y = value; }
	}
	
	public List<int> consList {
		get { return tileData.connections; }
	}
	
	public void setElementConnection (int i) {
		tileData.connections.Add(i);
	}
	
    public void setConnections(string text)
    {
        string[] splitText = text.Split(',');
        if (text.Length > 0)
            tileData.connections.Clear();
        foreach (string str in splitText)
            if (str.Length > 0)
                tileData.connections.Add(int.Parse(str));
    }

	public List<int> locksList {
		get { return tileData.lockGroups; }
	}
	
	public void setElementLock (int i) {
		tileData.lockGroups.Add(i);
	}
	
    public void setLockGroups(string text)
    {
        string[] splitText = text.Split(',');
        if (text.Length > 0)
            tileData.lockGroups.Clear();
        foreach (string str in splitText)
            if (str.Length > 0)
                tileData.lockGroups.Add(int.Parse(str));
    }
}