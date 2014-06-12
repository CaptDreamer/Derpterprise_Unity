using System.Collections.Generic;
using UnityEngine;

class CenterComparer : IEqualityComparer<Center>
{
	public CenterComparer() { }
	public bool Equals(Center x, Center y)
	{
		//return (x.Point.X == y.Point.X) && (x.Point.Y == y.Point.Y);
		return GetHashCode(x) == GetHashCode(y);
	}
	
	public int GetHashCode(Center obj)
	{
		return obj.Point.GetHashCode();
	}
}

class CornerComparer : IEqualityComparer<Corner>
{
	public CornerComparer() { }
	public bool Equals(Corner x, Corner y)
	{
		return x.Point.Equals(y.Point);
	}
	
	public int GetHashCode(Corner obj)
	{
		return obj.Point.GetHashCode();
	}
}

class EdgeComparer : IEqualityComparer<Edge>
{
	public EdgeComparer() { }
	public bool Equals(Edge x, Edge y)
	{
		return x.Midpoint == y.Midpoint;
	}
	
	public int GetHashCode(Edge obj)
	{
		return obj.Midpoint.GetHashCode();
	}
}

public interface IFactory
{
	Center CenterFactory(double ax, double ay);
	Edge EdgeFactory(Corner begin, Corner end, Center Left, Center Right);
	Corner CornerFactory(double ax, double ay);
}

public class MapItemFactory : IFactory
{
	#region Implementation of IFactory
	
	public Center CenterFactory(double ax, double ay)
	{
		Vector2 p = new Vector2((float)ax,(float) ay);
		int hash = p.GetHashCode();
		if(WorldGen.AppMap.Centers.ContainsKey(hash))
		{
			return WorldGen.AppMap.Centers[hash];
		}
		else
		{
			var nc = new Center((float)ax,(float) ay);
			WorldGen.AppMap.Centers.Add(nc.Key, nc);
			return nc;
		}
	}
	
	public Edge EdgeFactory(Corner begin, Corner end, Center Left, Center Right)
	{
		Vector2 p = new Vector2((begin.Point.x + end.Point.x) / 2, (begin.Point.x + end.Point.x) / 2);
		int hash = p.GetHashCode();
		if (WorldGen.AppMap.Edges.ContainsKey(hash))
		{
			return WorldGen.AppMap.Edges[hash];
		}
		else
		{
			var nc = new Edge(begin,end,Left,Right);
			Debug.Log(nc.Key);
			WorldGen.AppMap.Edges.Add(nc.Key, nc);
			return nc;
		}
	}
	
	public Corner CornerFactory(double ax, double ay)
	{
		Vector2 p = new Vector2((float)ax,(float) ay);
		int hash = p.GetHashCode();
		if (WorldGen.AppMap.Corners.ContainsKey(hash))
		{
			return WorldGen.AppMap.Corners[hash];
		}
		else
		{
			var nc = new Corner(ax, ay);
			WorldGen.AppMap.Corners.Add(nc.Key, nc);
			return nc;
		}
	}
	
	#endregion
}

