using UnityEngine;
using System.Collections;

public class WorldObject {

	public string Name { get; set; }
	public int PointX { get; set; }
	public int PointY { get; set; }
	public int Level { get; set; }

	public WorldObject(string name, int x, int y, int z)
	{
		this.Name = name;
		this.PointX = x;
		this.PointY = y;
		this.Level = z;
	}

	public void Move(int x, int y)
	{	
		if((PointX + x) < 0 || (PointY + y) < 0 || (PointX + x) >= MainGame.fullMap.Width || (PointY + y) >= MainGame.fullMap.Height)
		{
			return;
		}
		//Debug.Log((PointX + x) + "," + (PointY + y));
		if(MainGame.fullMap.GetTile((PointX + x),(PointY + y), 25).IsBlocked)
		{
			return;
		}
		MainGame.tmc.ClearPrevLoc(PointX, PointY);
		PointX += x;
		PointY += y;
	}
}
