using UnityEngine;
using System.Collections;

public static class MainGame {

	static public GridMap fullMap;
	static public tk2dTileMap tileMap;
	static public int Level { get; set; }

	static MainGame()
	{
		Debug.Log ("Stuff Happens Here" + System.DateTime.Now);
	}
}
