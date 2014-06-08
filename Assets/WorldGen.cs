using UnityEngine;
using System.Collections;

public class WorldGen : MonoBehaviour {

	public static tk2dTileMap testMap;
	public float[,] heights;


	// Use this for initialization
	void Start () {
		testMap = this.gameObject.GetComponent<tk2dTileMap>();
		testMap.name = "Test Map";
		testMap.width = 256;
		testMap.height = 256;
		testMap.Build ();
//		for (int i = 1; i<=255; i++)
//		{
//			for (int j = 1; j<=255; j++)
//			{
//
//				testMap.SetTile(i,j,0, Random.Range(0,4));
//			}
//		}
//		testMap.Build ();
		GenerateHeights (testMap, 10f);
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

	public void GenerateHeights(tk2dTileMap terrain,float tileSize)
	{
		float max = 0;
		float min = 100;
		float red;
		float green;
		float blue;
		float brown;

		heights = new float[terrain.width, terrain.height];
		
		for (int i = 0; i < terrain.width; i++)
		{
			for (int k = 0; k < terrain.height; k++)
			{
				heights[i, k] = Mathf.PerlinNoise(((float)i / (float)terrain.width) * tileSize, ((float)k / (float)terrain.height) * tileSize)/10.0f;
				if (heights[i, k] > max) { max = heights[i, k]; }
				if (heights[i, k] < min) { min = heights[i, k]; } 
			}
		}

		Debug.Log ("Max: " + max + " Min: " + min);
		red = (max - min) / 4;
		green = ((max - min) / 4) * 2;
		blue = ((max - min) / 4) * 3;
		brown = ((max - min) / 4) * 4;
		Debug.Log ("Red: " + red + " Green: " + green + "Blue: " + blue + " Brown: " + brown);
		for (int i = 0; i < terrain.width; i++)
		{
			for (int k = 0; k < terrain.height; k++)
			{
				if (heights[i, k] < red)
				{
					terrain.SetTile(i, k, 0, 0);
				} else {
					if (heights[i, k] < green)
					{
						terrain.SetTile(i, k, 0, 1);
					} else {
						if (heights[i, k] < blue)
						{
							terrain.SetTile(i,k, 0, 2);
						} else {
							terrain.SetTile(i,k,0,3);
						}
					}
				}
			}
		}
		terrain.Build();
	}
}
