using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;


public interface IIslandService
{
	void CreateIsland ();
}

public class IslandService : IIslandService 
{
	private double MapMaxHeight = 0.0d;
	private double MapDeepest = 1.0d;
	private int MapX, MapY;

	public IslandService(int mapX, int mapY)
	{
		MapX = mapX;
		MapY = mapY;
	}

	public void CreateIsland()
	{
		foreach (var c in WorldGen.AppMap.Corners.Values)
		{
			c.Water = !InLand(c.Point); //calculate land&water corners
		}

		FixCentersFloodFillOceans ();

		foreach (var c in WorldGen.AppMap.Corners.Values)
		{
			c.Coast = (c.Touches.Any(x => x.Water) && c.Touches.Any(x => x.Land)) ? true : false;

			c.Water = (c.Touches.Any(x => x.Land)) ? false : true;

			c.Ocean = (c.Touches.All(x => x.Ocean)) ? true : false;
		}

		CalculateElevation ();

		RedistributeElevation ();

		CalculateDownslopes ();

		CalculateWatersheds ();

		CreateRivers ();

		CalculateCornerMoisture ();

		RedistributeMoisture();
		
		Smooth1();

		FixElevations();




		foreach (Center c in WorldGen.AppMap.Centers.Values)
		{
			c.Moisture = c.Corners.Sum (x => x.Moisture) / c.Corners.Count();

			c.OrderCorners();

			c.Elevation = c.Corners.Sum (x => x.Elevation) / c.Corners.Count;

			c.SetBiome();
		}
	}

	private void FixElevations()
	{
		foreach (Center c in WorldGen.AppMap.Centers.Values)
		{
			if(c.Ocean)
			{
				int elevation = (int)(c.Elevation * 50);
				Debug.Log("Ocean: " + elevation + ", " + c.Elevation);
				if(elevation > 25)
				{
					c.Elevation = 0.5d;
				}
			} else {
				int elevation = (int)(c.Elevation * 50);
				Debug.Log("Land: " + elevation + ", " + c.Elevation);
				if(elevation < 25)
				{
					c.Elevation = 0.5d;
				}
			}
		}
	}

	private void RedistributeMoisture()
        {
            var locations = WorldGen.AppMap.Corners.Values.OrderBy(x => x.Moisture).ToArray();

            for (int i = 0; i < locations.Count(); i++) 
            {
                locations[i].Moisture = (float) i/(locations.Count() - 1);
            }
        }
	private void CalculateCornerMoisture()
	{
		var queue = new Queue<Corner> ();

		foreach( var q in WorldGen.AppMap.Corners.Values)
		{
			if ((q.Water || q.River > 0) && !q.Ocean)
			{
				q.Moisture = q.River > 0 ? Mathf.Min (3.0f, (0.2f * (float)q.River)) : 1.0;
				queue.Enqueue(q);
			}
			else
			{
				q.Moisture = 0.0;
			}
		}

		while (queue.Count > 0)
		{
			var q = queue.Dequeue();

			foreach (var r in q.Adjacents)
			{
				var newMoisture = q.Moisture * 0.9;
				if (newMoisture > r.Moisture)
				{
					r.Moisture = newMoisture;
					queue.Enqueue(r);
				}
			}
		}

		foreach (var q in WorldGen.AppMap.Corners.Values)
		{
			if (q.Ocean || q.Coast)
			{
				q.Moisture = 1.0;
			}
		}
	}

	private void CreateRivers()
	{
		for(int i = 0; i < MapX / 2; i++)
		{
			Corner q = WorldGen.AppMap.Corners.Values.ElementAt(UnityEngine.Random.Range(0, WorldGen.AppMap.Corners.Values.Count - 1));

			if (q.Ocean || q.Elevation < 0.3 || q.Elevation > 0.9) continue;

			while (!q.Coast)
			{
				if(q == q.Downslope)
				{
					break;
				}

				Edge edge = q.Protrudes.FirstOrDefault(ed => ed.VoronoiStart == q.Downslope || ed.VoronoiEnd == q.Downslope);
				edge.River = edge.River + 1;
				q.River = q.River + 1;
				q.Downslope.River = q.Downslope.River + 1;
				q = q.Downslope;
			}
		}
	}

	private void CalculateWatersheds()
	{
		foreach (var q in WorldGen.AppMap.Corners.Values)
		{ 
			q.Watershed = q;

			if (!q.Ocean && !q.Coast)
			{
				q.Watershed = q.Downslope;
			}
		}

		for (int i = 0; i < 100; i++)
		{
			var changed = false;

			foreach (var q in WorldGen.AppMap.Corners.Values)
			{
				if (!q.Ocean && !q.Coast && !q.Watershed.Coast)
				{
					var r = q.Downslope.Watershed;

					if (!r.Ocean)
						q.Watershed = r;

					changed = true;
				}
			}

			if (!changed)
				break;
		}

		foreach (var q in WorldGen.AppMap.Corners.Values)
		{
			var r = q.Watershed;
			r.WatershedSize = 1 + r.WatershedSize;
		}
	}

	private void CalculateDownslopes()
	{
		foreach(Corner corner in WorldGen.AppMap.Corners.Values)
		{
			var buf = corner;

			foreach(var adj in corner.Adjacents)
			{
				if (adj.Elevation <= buf.Elevation)
				{
					buf = adj;
				}
			}

			corner.Downslope = buf;
		}
	}

	private void RedistributeElevation()
	{
		double scaleFactor = 1.1;
		var locations = WorldGen.AppMap.Corners.Values.OrderBy (x => x.Elevation).ToArray (); //Where (x => !x.Ocean)

		for (int i = 0; i < locations.Count(); i++)
		{
			double y = (double)i / (locations.Count() - 1);

			var x = 1.04880885 - Math.Sqrt(scaleFactor * (1 - y));
			if (x > 1.0)
				x = 1.0;
			locations[i].Elevation = x;
		}
	}

	private void CalculateElevation()
	{
		var queue = new Queue<Corner> ();

		foreach (var q in WorldGen.AppMap.Corners.Values)
		{
			if (q.Border)
			{
				q.Elevation = 0.0;
				queue.Enqueue(q);
			}else {
				q.Elevation = double.MaxValue;
			}

			while (queue.Count > 0)
			{
				var corner = queue.Dequeue();

				foreach(var adj in corner.Adjacents)
				{
					double newElevation = 0.01 + corner.Elevation;

//					if (!corner.Water && !adj.Water)
//					{
						newElevation += 1;
//					}

					if (newElevation < adj.Elevation)
					{
						adj.Elevation = newElevation;
						queue.Enqueue(adj);
					}
				}
			}
		}
	}

	private void Smooth1()
	{
		var ordered = new List<Corner>();
		var first = WorldGen.AppMap.Corners.Values.First(x => x.Coast && x.Touches.Any(z=>z.Ocean));
		var start = first;
		ordered.Add(first);
		var next = first.Adjacents.First(x => x.Coast);

		while (next != start)
		{
			var nexte = next.Protrudes.FirstOrDefault(x => x.Coast && (x.VoronoiStart != ordered.Last() && x.VoronoiEnd != ordered.Last()));
			ordered.Add(next);
			next = nexte.VoronoiStart == next ? nexte.VoronoiEnd : nexte.VoronoiStart;
		}

		for (int a = 0; a < 2; a++)
		{
			for (int i = 2; i < ordered.Count - 2; i++)
			{
				ordered[i].Point = PointOnCurve(ordered[i - 2].Point, ordered[i - 1].Point, ordered[i + 1].Point,
											 ordered[i + 2].Point, 0.5f);
			}
		}
	}

	public Vector2 PointOnCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		Vector2 ret = new Vector2();
		
		float t2 = t * t;
		float t3 = t2 * t;

		ret.x = 0.5f * ((2.0f * p1.x) +
		(-p0.x + p2.x) * t +
		(2.0f * p0.x - 5.0f * p1.x + 4 * p2.x - p3.x) * t2 +
		(-p0.x + 3.0f * p1.x - 3.0f * p2.x + p3.x) * t3);

		ret.y = 0.5f * ((2.0f * p1.y) +
		(-p0.y + p2.y) * t +
		(2.0f * p0.y - 5.0f * p1.y + 4 * p2.y - p3.y) * t2 +
		(-p0.y + 3.0f * p1.y - 3.0f * p2.y + p3.y) * t3);

		return ret;
	}
	private void FixCentersFloodFillOceans()
	{
		foreach (var ct in WorldGen.AppMap.Centers.Values)
		{
			ct.FixBorders(); //Fix edges at map vorder, set "border" and "ocean" values
			ct.OrderCorners(); //Order corners clockwise

			//if it touches any water corner, it's water; there will be leftovers
			ct.Water = (ct.Corners.Any(x => x.Water)) ? true : false;
		}

		var Oceans = new Queue<Center> ();
		//start with oceans at the borders
		foreach (Center c in WorldGen.AppMap.Centers.Values.Where(c => c.Ocean))
		{
			Oceans.Enqueue(c);
		}

		//floodfill oceans
		while (Oceans.Count > 0)
		{
			Center c = Oceans.Dequeue();

			foreach(Center n in c.Neighbours.Where(x => !x.Ocean))
			{
				if (n.Corners.Any(x => x.Water))
				{
					n.Ocean = true;
					if (!Oceans.Contains(n))
						Oceans.Enqueue(n);
				} else {
					n.Coast = true;
				}
			}
		}
	}

	private bool InLand(Vector2 p)
	{
		return IsLandShape (new Vector2 ((float)(2 * (p.x / MapX - 0.5)), (float)(2 * (p.y / MapY - 0.5))));
	}

	private bool IsLandShape(Vector2 point)
	{
		double ISLAND_FACTOR = 1.07;
		int bumps = UnityEngine.Random.Range(1, 6);
		double startAngle = UnityEngine.Random.value * 2 * Mathf.PI;
		double dipAngle = UnityEngine.Random.value * 2 * Mathf.PI;
		double dipWidth = UnityEngine.Random.Range (2, 7) / 10;

		double angle = Mathf.Atan2 (point.y, point.x);
		double length = 0.5 * (Mathf.Max (Mathf.Abs (point.x), Mathf.Abs (point.y)) + GetPointLength (point));

		double r1 = 0.5 + 0.40 * Mathf.Sin ((float)startAngle + bumps * (float)angle + Mathf.Cos ((bumps + 3) * (float)angle));
		double r2 = 0.7 - 0.20 * Mathf.Sin ((float)startAngle + bumps * (float)angle - Mathf.Cos ((bumps + 2) * (float)angle));
		if(Mathf.Abs((float)angle - (float)dipAngle) < dipWidth
		   || Mathf.Abs ((float)angle - (float)dipAngle + 2 * Mathf.PI) < dipWidth
		   || Mathf.Abs ((float)angle - (float)dipAngle - 2 * Mathf.PI) < dipWidth)
		{
			r1 = r2 = 0.2;
		}
		return (length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));
	}

	private double GetPointLength(Vector2 point)
	{
		return Mathf.Sqrt((Mathf.Pow(point.x, 2)) + (Mathf.Pow(point.y, 2)));
	}
}
