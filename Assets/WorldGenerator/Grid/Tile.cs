using UnityEngine;
using System.Collections;

public class Tile  {

	public string Biome { get; set; }
	public Color Color { get; set; }
	public int Elevation { get; set; }
	public Vector2 Point { get; set; }

	public Tile()
	{
		Biome = "Empty";
	}
}
