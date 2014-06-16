using UnityEngine;
using System.Collections;

public class TileMapController : MonoBehaviour {

	public tk2dTileMap tileMap;

	int level;

	// Use this for initialization
	void Start () {
		BuildLevel (25);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void BuildLevel(int level)
	{
		this.level = level;
		for(int i = 0; i < MainGame.fullMap.Width; i++)
		{
			for(int j = 0; j < MainGame.fullMap.Height; j++)
			{
				tileMap.SetTile(i,j,0,0);
				tileMap.ColorChannel.SetColor(i,j,MainGame.fullMap.map[i,j,level].Color);
			}
		}

		tileMap.Build ();
	}

	public void UpLevel()
	{
		BuildLevel (level + 1);
	}

	public void DownLevel()
	{
		BuildLevel (level - 1);
	}

}
