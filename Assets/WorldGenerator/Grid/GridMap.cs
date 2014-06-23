using UnityEngine;
using System;
using System.Collections;
using ProtoBuf;

[ProtoContract]
//[ProtoInclude(4, typeof(Tile))]
public class GridMap {

	[ProtoMember(1)]
	public Tile[] Map { get; set; }
	[ProtoMember(2)]
	public int Width { get; set; }
	[ProtoMember(3)]
	public int Height { get; set; }

	public GridMap()
	{
	}

	public GridMap (int width, int height)
	{
		Map = new Tile[width*height*50];
		Width = width;
		Height = height;

		for(int i = 0; i < width; i++)
		{
			for(int j = 0; j < height; j++)
			{
				for(int k = 0; k < 50; k++)
				{
					Map [(i * this.Width + j) * 50 + k] = new Tile();
				}
			}
		}
	}

	public Tile GetTile(int x, int y, int z)
	{
		return Map [(x * this.Width + y) * 50 + z];
	}
}
