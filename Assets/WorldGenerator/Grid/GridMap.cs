﻿using UnityEngine;
using System.Collections;

public class GridMap {

	public Tile[,,] map { get; set; }

	public GridMap (int width, int height)
	{
		map = new Tile[width, height, 50];

		for(int i = 0; i < width; i++)
		{
			for(int j = 0; j < height; j++)
			{
				for(int k = 0; k < 50; k++)
				{
					map[i,j,k] = new Tile();
				}
			}
		}
	}
}