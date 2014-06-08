using UnityEngine;
using System.Collections;

public class WorldGen : MonoBehaviour {

	public static tk2dTileMap testMap;

	// Use this for initialization
	void Start () {
		testMap = this.gameObject.GetComponent<tk2dTileMap>();
		testMap.name = "Test Map";
		testMap.width = 10;
		testMap.height = 10;
		testMap.Build ();
		for (int i = 1; i<=10; i++)
		{
			for (int j = 1; j<=10; j++)
			{

				testMap.SetTile(i,j,0, Random.Range(0,4));
			}
		}
		testMap.Build ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void GetTile(Vector3 point)
	{
		//Debug.Log(point);
		int tileX;
		int tileY;
		WorldGen.testMap.GetTileAtPosition (point, out tileX, out tileY);
		Debug.Log (tileX + ", " + tileY);
		WorldGen.SwapTile (tileX, tileY);
	}

	public static void SwapTile(int x, int y)
	{
		WorldGen.testMap.SetTile(x, y, 0, Random.Range(0,4));
		WorldGen.testMap.Build ();
	}
}
