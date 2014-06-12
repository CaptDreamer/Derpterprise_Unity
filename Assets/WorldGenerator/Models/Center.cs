using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class Center : IEquatable<Center>, IMapItem {

	public int Index { get; set; }

	public int Key { get; set; }
	public Vector2 Point { get; set; } //location
	public bool Water { get; set; }  // lake or ocean
	public bool Land { get { return !Water; } set { Water = !value; } }  // lake or ocean
	public bool Ocean { get; set; } //ocean
	public bool Coast { get; set; } //land poly touching an ocean
	public bool Border { get; set; } //at the edge of the map

	//remember to get a poly
	public Vector3 polyNorm { get; set; }
	public Color PolygonBrush { get; set; }

	private string _biome;
	public string Biome
	{
		get
		{
			return _biome;
		}
		set
		{
			if (value == "Ocean")
				PolygonBrush = new Color(  54/255f,  54/255f, 97/255f, 255/255f );
			if (value == "ShallowWater")
				PolygonBrush = Color.blue; //new Color(74/255f,74/255f,117/255f,255/255f);
			else if (value == "Marsh")
				PolygonBrush = new Color(196/255f,204/255f,187/255f ,255/255f);
			else if (value == "Lake")
				PolygonBrush = new Color(54/255f,54/255f,97/255f,255/255f);
			else if (value == "Beach")
				PolygonBrush = new Color(173/255f,161/255f,139/255f,255/255f);
			else if (value == "Snow")
				PolygonBrush = Color.white;
			else if (value == "Tundra")
				PolygonBrush = new Color(196/255f,204/255f,187/255f,255/255f);
			else if (value == "Bare")
				PolygonBrush = new Color(187/255f,187/255f,187/255f,255/255f);
			else if (value == "Scorched")
				PolygonBrush = new Color(153/255f,153/255f,153/255f,255/255f);
			else if (value == "Taiga")
				PolygonBrush = new Color(204/255f,212/255f,187/255f,255/255f);
			else if (value == "Shrubland")
				PolygonBrush = new Color(153/255f,166/255f,139/255f,255/255f);
			else if (value == "TemperateDesert")
				PolygonBrush = new Color(228/255f,232/255f,202/255f,255/255f);
			else if (value == "TemperateRainForest")
				PolygonBrush = new Color(84/255f,116/255f,88/255f,255/255f);
			else if (value == "TemperateDeciduousForest")
				PolygonBrush = new Color(119/255f,139/255f,85/255f,255/255f);
			else if (value == "Grassland")
				PolygonBrush = new Color(153/255f,180/255f,112/255f,255/255f);
			else if (value == "TropicalRainForest")
				PolygonBrush = new Color(112/255f,139/255f,85/255f,255/255f);
			else if (value == "TropicalSeasonalForest")
				PolygonBrush = new Color(85/255f,139/255f,85/255f,255/255f);
			else if (value == "SubtropicalDesert")
				PolygonBrush = Color.green; //new Color(172/255f,159/255f,139/255f,255/255f);

			_biome = value;
		}
	}

	public double Moisture { get; set; } //0.0 - 1.0
	public double Elevation { get; set; } // 0.0 - 1.0

	public HashSet<Center> Neighbours { get; set; }
	public HashSet<Edge> Borders { get; set; }
	public HashSet<Corner> Corners { get; set; }

	public Center(float x, float y)
	{
		polyNorm = new Vector3(0,0,0);
		Point = new Vector2(x,y);
		Key = Point.GetHashCode();
		Index = Point.GetHashCode();

		Water = Coast = Ocean = Border = false;
		Elevation = Moisture = 0.0d;

		Neighbours = new HashSet<Center>();
		Borders = new HashSet<Edge>();
		Corners = new HashSet<Corner>();
	}

	#region Methods
	public bool Equals(Center other)
	{
		return this.Point == other.Point;
	}

	public void AddBorder(Edge edge)
	{
		if(!Borders.Contains(edge))
			Borders.Add (edge);
	}

	public void AddCorner(Corner corner)
	{
		if (!Corners.Contains(corner))
			Corners.Add (corner);
	}

	public void AddNeighbour(Center center)
	{
		if (!Neighbours.Contains(center))
			Neighbours.Add(center);
	}

	public void OrderCorners()
	{
		Corner CurrentCorner = Corners.First();
		List<Corner> Ordered = new List<Corner>();
		Corner newdot;
		Edge ed;

		Ordered.Add(CurrentCorner);
		do
		{
			ed = CurrentCorner.Protrudes.FirstOrDefault(x => Borders.Contains(x) && !(Ordered.Contains(x.VoronoiStart) && (Ordered.Contains(x.VoronoiEnd))));

			if(ed != null)
			{
				newdot = ed.Corners.FirstOrDefault(x => !Ordered.Contains(x));
				Ordered.Add(newdot);
				CurrentCorner = newdot;
			}
		} while (ed != null);

		Corners.Clear();

		foreach (var corner in Ordered)
		{
			Corners.Add (corner);
		}
	}

	public void FixBorders()
	{
		var ms = from p in Borders.SelectMany(x => x.Corners)
				 group p by p.Point
				 into grouped
				 select new {point = grouped.Key, count = grouped.Count()};

		var fpoint = ms.FirstOrDefault(x => x.count == 1);
		var spoint = ms.LastOrDefault(x => x.count == 1);

		if(fpoint != null & spoint != null)
		{
			Corner p1 = Corners.FirstOrDefault(x => x.Point == fpoint.point);
			Corner p2 = Corners.FirstOrDefault(x => x.Point == spoint.point);

			if (p1 ==null || p2 == null)
				return;

			IFactory fact = new MapItemFactory();
			Edge e = fact.EdgeFactory(
				p1,
				p2,
				this, null);

			e.MapEdge = true;

			p1.Protrudes.Add(e);
			p2.Protrudes.Add(e);

			this.Border = this.Ocean = this.Water = true;
			e.VoronoiStart.Border = e.VoronoiEnd.Border = true;
			e.VoronoiStart.Elevation = e.VoronoiEnd.Elevation = 0.0d;

			this.Borders.Add(e);
		}
	}




	#endregion

	public void SetBiome()
	{
		if (Ocean && Elevation > -0.1d)
		{
			Biome = "ShallowWater";
		}
		else if (Water)
		{
			if (Elevation < 0.1) Biome = "Marsh";
			if (Elevation > 0.8) Biome = "Ice";
			Biome = "Lake";
		}
		else if (Coast)
		{
			Biome = "Beach";
		}
		else if (Elevation > 0.8)
		{
			if (Moisture > 0.50) Biome = "Snow";
			else if (Moisture > 0.33) Biome = "Tundra";
			else if (Moisture > 0.16) Biome = "Bare";
			else Biome = "Scorched";
		}
		else if (Elevation > 0.6)
		{
			if (Moisture > 0.66) Biome = "Taiga";
			else if (Moisture > 0.33) Biome = "Shrubland";
			else Biome = "TemperateDesert";
		}
		else if (Elevation > 0.3)
		{
			if (Moisture > 0.83) Biome = "TemperateRainForest";
			else if (Moisture > 0.50) Biome = "TemperateDeciduousForest";
			else if (Moisture > 0.16) Biome = "Grassland";
			else Biome = "TemperateDesert";
		}
		else
		{
			if (Moisture > 0.66) Biome = "TropicalRainForest";
			else if (Moisture > 0.33) Biome = "TropicalSeasonalForest";
			else if (Moisture > 0.16) Biome = "Grassland";
			else Biome = "SubtropicalDesert";
		}
	}
}
