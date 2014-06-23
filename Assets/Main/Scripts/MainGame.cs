using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MainGame {

	static public GridMap fullMap;
	static public tk2dTileMap tileMap;
	static public int Level { get; set; }
	static public TileMapController tmc;
	static public List<WorldObject> characters;
	static public GameState gameState;

	static MainGame()
	{
		StartGame ();
		//int timer = 100000000;
		gameState = GameState.Playing;
	}

	public static void Update()
	{
		foreach(WorldObject wo in characters)
		{
			int xD = Random.Range(-1,2);
			int yD = Random.Range(-1,2);
			//Debug.Log(xD + "," + yD);
			wo.Move(xD,yD);
		}
		
		tmc.DrawWorldObj();
	}

	static void StartGame()
	{
		//Set the gamestate
		gameState = GameState.Paused;

		//set the TileMapController
		tmc = GameObject.Find ("TileMapController").GetComponent<TileMapController> ();
		tmc.StartGame ();
		
		//Find a random square of Beach
		List<Tile> beaches = new List<Tile> ();
		for(int i = 0; i < fullMap.Width; i++)
		{
			for(int j = 0; j < fullMap.Height; j++)
			{
				if(fullMap.GetTile(i,j,25).Biome == "Beach" && !fullMap.GetTile(i,j,25).IsBlocked)
				{
					beaches.Add(fullMap.GetTile(i,j,25));
				}
			}
		}
		Tile startTile = beaches[Random.Range(0, beaches.Count)];
		
		startTile.Biome = "Start Location";
		Vector3 tilePos = tileMap.GetTilePosition(startTile.PointX, startTile.PointY);
		tilePos.z = Camera.main.transform.position.z;
		Camera.main.transform.position = tilePos;

		//Initialize the character list
		characters = new List<WorldObject> ();
		WorldObject dwarf = new WorldObject ("Dwarf", startTile.PointX, startTile.PointY, 25);
		characters.Add (dwarf);
		//tmc.DrawWorldObj ();
	}
}
