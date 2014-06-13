using UnityEngine;
using System;


public class Edge : IEquatable<Edge>, IMapItem
{
	public Edge(Corner begin, Corner end, Center left, Center right)
	{
		River = 0;
		VoronoiStart = begin;
		VoronoiEnd = end;
		
		DelaunayStart = left;
		DelaunayEnd = right;
		
		Midpoint = new Vector2((VoronoiStart.Point.x + VoronoiEnd.Point.x) / 2, (VoronoiStart.Point.y + VoronoiEnd.Point.y) / 2);
		Key = Midpoint.GetHashCode();
		Index = Midpoint.GetHashCode();
	}
	
	public Edge(int index, Corner begin, Corner end)
	{
		Index = index;
		River = 0;
		VoronoiStart = begin;
		VoronoiEnd = end;
		Midpoint = new Vector2((VoronoiStart.Point.x + VoronoiEnd.Point.x) / 2, (VoronoiStart.Point.y + VoronoiEnd.Point.y) / 2);
		Key = Midpoint.GetHashCode();
	}
	
	public int Index { get; set; }
	
	public int Key { get; set; }
	public Center DelaunayStart { get; set; }
	public Center DelaunayEnd { get; set; }// Delaunay edge
	public Corner VoronoiStart { get; set; }
	public Corner VoronoiEnd { get; set; }// Voronoi edge
	public Vector2 Midpoint { get; set; }  // halfway between v0,v1
	public int River { get; set; }  // volume of water, or 0
	public bool MapEdge = false;
	public Vector2 Point { get { return VoronoiStart.Point; } }
	
	public bool Coast
	{
		get
		{
			if (DelaunayStart != null && DelaunayEnd != null)
				return ((VoronoiStart.Coast) && (VoronoiEnd.Coast)
				        && !(DelaunayStart.Water && DelaunayEnd.Water)
				        && !(DelaunayStart.Land && DelaunayEnd.Land));
			return false;
		}
		
		set {  }
	}
	
	public Corner[] Corners { get { return new Corner[] {VoronoiStart, VoronoiEnd}; } }
	
	public double DiffX { get { return VoronoiEnd.Point.x - VoronoiStart.Point.x; } }
	public double DiffY { get { return VoronoiEnd.Point.y - VoronoiStart.Point.y; } }
	
	public bool Equals(Edge other)
	{
		return this.VoronoiStart.Equals(other.VoronoiStart) && 
			this.VoronoiEnd.Equals(other.VoronoiEnd);
	}
}
