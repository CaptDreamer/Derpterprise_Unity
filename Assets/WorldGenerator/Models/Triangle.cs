using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Triangle
{
	public List<Corner> Corners { get; set; }
	public List<Edge> Edges { get; set; }
	public List<Triangle> Neightbours { get; set; }
	public Vector2 Point { get; set; }
	public Center Parent { get; set; }

	public Triangle()
	{
		Corners = new List<Corner>();
		Edges = new List<Edge>();
		Neightbours = new List<Triangle>();

	}
}

