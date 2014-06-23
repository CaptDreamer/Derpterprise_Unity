using UnityEngine;
using System.Collections;

public class TileMapController : MonoBehaviour {

	public tk2dTileMap tileMap;

	int level;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartGame()
	{
		//Load the last saved map
		MainGame.fullMap = MapSave.LoadMap ();
		MainGame.tileMap = tileMap;
		BuildLevel (25);
		MainGame.Level = 25;
	}

	public void BuildLevel(int level)
	{
		this.level = level;
		MainGame.Level = level;
		for(int i = 0; i < MainGame.fullMap.Width; i++)
		{
			for(int j = 0; j < MainGame.fullMap.Height; j++)
			{
				if(MainGame.fullMap.GetTile(i, j, level).Underground)
				{
					int tile = tileMap.SpriteCollectionInst.GetSpriteIdByName("Underground-square");
					tileMap.SetTile(i,j,0,tile);
				} else {
					int tile = tileMap.SpriteCollectionInst.GetSpriteIdByName(MainGame.fullMap.GetTile(i, j, level).Biome + "-square");
					tileMap.SetTile(i,j,0,tile);
				}
			}
		}

		tileMap.Build ();
	}

	public void UpLevel()
	{
		BuildLevel (level + 1);
		//MainGame.Level++;
	}

	public void DownLevel()
	{
		BuildLevel (level - 1);
		//MainGame.Level--;
	}

	public void DrawWorldObj()
	{
		foreach(WorldObject obj in MainGame.characters)
		{
			//Debug.Log(obj.PointX + "," + obj.PointY);
			tileMap.SetTile(obj.PointX, obj.PointY, 1, 20);
		}
		tileMap.Build ();
	}

	public void ClearPrevLoc(int x, int y)
	{
		tileMap.ClearTile (x, y, 1);
	}

}
