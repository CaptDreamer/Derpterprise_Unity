﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BenTools.Mathematics;

public class WorldGen : MonoBehaviour {

	public GameObject dot; 
	public GameObject dotCorner; 
	public GameObject line;
	public GameObject poly;

	public static int width;
	int height;
	int MapX, MapY;

	private IIslandService IslandHandler;

	public int DotCount;

	VoronoiGraph voronoiMap;

	public static Map AppMap { get; set; }

	public static void ResetMap()
	{
		WorldGen.AppMap.Centers.Clear();
		WorldGen.AppMap.Edges.Clear();
		WorldGen.AppMap.Corners.Clear();
	}
		
	// Use this for initialization
	void Start () {
		//initialize
		width = 250;
		height = width;
		WorldGen.AppMap = null;
		WorldGen.AppMap = new Map();
		
		//Create a Voronoi Graph
		CreateVoronoiGraph();

		//Create the polygon class from the graph
		CleanUpGraph(true);

		//Display the points
		//DisplayPoints();
		DisplayPoly ();

		// Display Rivers
		DisplayRivers();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void CreateVoronoiGraph() 
	{
		MapX = width;
		MapY = height;
		IslandHandler = new IslandService (MapX, MapY);

		var points = new HashSet<BenTools.Mathematics.Vector>();
		for(int i = 0; i < DotCount; i++)
		{
			points.Add(new BenTools.Mathematics.Vector(Random.Range(0,MapX), Random.Range(0,MapY)));
		}

		voronoiMap = null;
		for(int i = 0; i < 3; i++)
		{
			voronoiMap = Fortune.ComputeVoronoiGraph(points);
			foreach(BenTools.Mathematics.Vector vector in points)
			{
				double v0 = 0.0d;
				double v1 = 0.0d;
				int say = 0;
				foreach(VoronoiEdge edge in voronoiMap.Edges)
				{
					if(edge.LeftData == vector || edge.RightData == vector)
					{
						double p0 = (edge.VVertexA[0] + edge.VVertexB[0]) / 2;
						double p1 = (edge.VVertexA[1] + edge.VVertexB[1]) / 2;
						v0 += double.IsNaN(p0) ? 0 : p0;
						v1 += double.IsNaN(p1) ? 0 : p1;
						say++;
					}
				}

				if (((v0 / say) < MapX) && ((v0 / say) > 0))
				{
					vector[0] = v0 / say;
				}

				if (((v1 / say) < MapY) && ((v1 / say) > 0))
				{
					vector[1] = v1 / say;
				}
			}
		}

		voronoiMap = Fortune.ComputeVoronoiGraph(points);
	}

	void CleanUpGraph(bool fix = false)
	{
		IFactory fact = new MapItemFactory();
		
		foreach (VoronoiEdge edge in voronoiMap.Edges)
		{		

			if (fix)
			{
				if (!FixPoints(edge))
					continue;
			}

			Corner c1 = fact.CornerFactory(edge.VVertexA[0], edge.VVertexA[1]);
			Corner c2 = fact.CornerFactory(edge.VVertexB[0], edge.VVertexB[1]);
			Center cntrLeft = fact.CenterFactory(edge.LeftData[0], edge.LeftData[1]);
			Center cntrRight = fact.CenterFactory(edge.RightData[0], edge.RightData[1]);
			
			c1.AddAdjacent(c2);
			c2.AddAdjacent(c1);
			
			cntrRight.Corners.Add(c1);
			cntrRight.Corners.Add(c2);
			
			cntrLeft.Corners.Add(c1);
			cntrLeft.Corners.Add(c2);
			
			Edge e = fact.EdgeFactory(c1, c2, cntrLeft, cntrRight);
			
			
			cntrLeft.Borders.Add(e);
			cntrRight.Borders.Add(e);
			
			cntrLeft.Neighbours.Add(cntrRight);
			cntrRight.Neighbours.Add(cntrLeft);
			
			c1.AddProtrudes(e);
			c2.AddProtrudes(e);
			c1.AddTouches(cntrLeft);
			c1.AddTouches(cntrRight);
			c2.AddTouches(cntrLeft);
			c2.AddTouches(cntrRight);
		}
		
		foreach (Corner q in WorldGen.AppMap.Corners.Values)
		{
		
			if (!q.Border)
			{
				var point = new Vector2(0, 0);
				foreach (Center c in q.Touches)
				{
					point.x += c.Point.x;
					point.y += c.Point.y;
				}
				point.x = point.x / q.Touches.Count;
				point.y = point.y / q.Touches.Count;
				q.Point = point;
			}
		}
		
		//c.Value.FixBorders();
		//c.SetEdgeAreas();
		//c.Value.OrderCorners();
		IslandHandler.CreateIsland();
	}

	private bool FixPoints(VoronoiEdge edge)
	{
		double x1 = edge.VVertexA[0];
		double y1 = edge.VVertexA[1];
		
		double x2 = edge.VVertexB[0];
		double y2 = edge.VVertexB[1];
		
		
		
		//if both ends are in map, not much to do
		if ((DotInMap(x1, y1) && DotInMap(x2, y2)))
			return true;
		
		//if one end is out of map
		if ((DotInMap(x1, y1) && !DotInMap(x2, y2)) || (!DotInMap(x1, y1) && DotInMap(x2, y2)))
		{
			double b = 0.0d, slope = 0.0d;
			
			//and that point is actually a number ( not going to infinite ) 
			if (!(double.IsNaN(x2) || double.IsNaN(y2)))
			{
				slope = ((y2 - y1) / (x2 - x1));
				
				b = edge.VVertexA[1] - (slope * edge.VVertexA[0]);
				
				// y = ( slope * x ) + b
				
				
				if (edge.VVertexA[0] < 0)
					edge.VVertexA = new Vector(0, b);
				
				if (edge.VVertexA[0] > WorldGen.width)
					edge.VVertexA = new Vector(WorldGen.width, (WorldGen.width * slope) + b);
				
				if (edge.VVertexA[1] < 0)
					edge.VVertexA = new Vector((-b / slope), 0);
				
				if (edge.VVertexA[1] > WorldGen.width)
					edge.VVertexA = new Vector((WorldGen.width - b) / slope, WorldGen.width);
				
				
				
				if (edge.VVertexB[0] < 0)
					edge.VVertexB = new Vector(0, b);
				
				if (edge.VVertexB[0] > WorldGen.width)
					edge.VVertexB = new Vector(WorldGen.width, (WorldGen.width * slope) + b);
				
				if (edge.VVertexB[1] < 0)
					edge.VVertexB = new Vector((-b / slope), 0);
				
				if (edge.VVertexB[1] > WorldGen.width)
					edge.VVertexB = new Vector((WorldGen.width - b) / slope, WorldGen.width);
				
			}
			else
			{
				//and if that end is actually not a number ( going go infinite )
				if (double.IsNaN(x2) || double.IsNaN(y2))
				{
					var x3 = (edge.LeftData[0] + edge.RightData[0]) / 2;
					var y3 = (edge.LeftData[1] + edge.RightData[1]) / 2;
					
					slope = ((y3 - y1) / (x3 - x1));
					
					slope = Mathf.Abs((float)slope);
					
					b = edge.VVertexA[1] - (slope * edge.VVertexA[0]);
					
					// y = ( slope * x ) + b
					var i = 0.0d;
					
					if(x3 < y3)
					{
						if(WorldGen.width - x3 > y3)
						{
							i = b;
							if (i > 0 && i < MapY)
								edge.VVertexB = new BenTools.Mathematics.Vector(0, i);
							
						}
						else
						{
							i = (MapX - b) / slope;
							if (i > 0 && i < MapY)
								edge.VVertexB = new BenTools.Mathematics.Vector(i, MapY);
							
						}
					}
					else
					{
						if (MapX - x3 > y3)
						{
							i = (-b / slope);
							if (i > 0 && i < MapX)
								edge.VVertexB = new BenTools.Mathematics.Vector(i, 0);
						}
						else
						{
							i = (MapX * slope) + b;
							if (i > 0 && i < width)
								edge.VVertexB = new BenTools.Mathematics.Vector(MapX, i);
							
						}
					}				
				}
			}
			return true;
		}
		return false;
	}
	
	private bool DotInMap(double x, double y)
	{
		if (x == double.NaN || y == double.NaN)
			return false;
		return (x > 0 && x < MapX) && (y > 0 && y < MapY);
	}

	void DisplayPoints()
	{
		//Debug.Log("Starting Display");
		foreach (Center o in WorldGen.AppMap.Centers.Values)
		{
			//Debug.Log("Displaying Center");
			Vector2 point = o.Point;
			GameObject centerGO = (GameObject)Instantiate(dot,new Vector3(point.x,point.y, 0),Quaternion.identity);
			centerGO.name = "Center: " + o.Point;
		}
		
		foreach (Corner c in WorldGen.AppMap.Corners.Values)
		{
			//Debug.Log("Displaying Corner");
			Vector2 point = c.Point;
			if(float.IsNaN(point.x) || float.IsNaN(point.y)) 
			{
				Debug.Log("Corner Error - X: " + point.x + " Y: " + point.y);
				c.Point.Set(0, 0);
				point = c.Point;
			}
			GameObject CornerGO = (GameObject)Instantiate(dotCorner,new Vector3(point.x,point.y, 0),Quaternion.identity);
			CornerGO.name = "Corner: " + c.Point;
		}
		
		foreach (Edge ed in WorldGen.AppMap.Edges.Values)
		{
			Vector2 point = ed.Point;
			if(float.IsNaN(point.x) || float.IsNaN(point.y)) 
			{
				Debug.Log("Line Error - X: " + point.x + " Y: " + point.y);
				ed.Point.Set(0, 0);
				point = ed.Point;
			}
			GameObject lineGO = (GameObject)Instantiate(line, new Vector3(point.x,point.y,10),Quaternion.identity);
			lineGO.name = "Edge: " + ed.Point;
			LineRenderer lr = lineGO.GetComponent<LineRenderer>();
			lr.SetPosition(0,new Vector3(ed.VoronoiStart.Point.x,ed.VoronoiStart.Point.y,10));
			lr.SetPosition(1,new Vector3(ed.VoronoiEnd.Point.x,ed.VoronoiEnd.Point.y,10));
		}
	}

	void DisplayPoly()
	{
		foreach(Center c in WorldGen.AppMap.Centers.Values)
		{
//			if(c.Point.x < 0 || c.Point.x > MapX || c.Point.y < 0 || c.Point.y > MapY)
//				throw new UnityException();

			int elevation = (int)(c.Elevation * 50);
			//Debug.Log(elevation);

			GameObject polyGO = (GameObject)Instantiate(poly,new Vector3(c.Point.x, c.Point.y, -elevation), Quaternion.identity);
			Mesh mesh = new Mesh();
			polyGO.GetComponent<MeshFilter>().mesh = mesh;

			Vector3[] verts = new Vector3[c.Corners.Count];
			Vector2[] vert2d = new Vector2[c.Corners.Count];

			int cornerCount = 0;
			foreach(Corner corner in c.Corners)
			{
//				if(corner.Point.x < 0 || corner.Point.x > MapX || corner.Point.y < 0 || corner.Point.y > MapY)
//					throw new UnityException();
				verts[cornerCount] = new Vector3(corner.Point.x - c.Point.x, corner.Point.y - c.Point.y, -elevation);
				vert2d[cornerCount] = new Vector2(corner.Point.x, corner.Point.y);
				cornerCount++;
			}

			Triangulator tr = new Triangulator(vert2d);
			int[] tris = tr.Triangulate();

			Color newColor = c.PolygonBrush;
			//Debug.Log(c.Biome + " " + newColor);
			polyGO.renderer.material.color = newColor;

			mesh.vertices = verts;
			mesh.uv = vert2d;
			mesh.triangles = tris;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			mesh.name = "Mesh: " + c.Point;
			string biome;
			if (c.Ocean)
				polyGO.name = "Ocean, " + elevation;
			else
				polyGO.name = c.Biome + ", " + elevation;
		}
	}

	void DisplayRivers()
	{
		foreach (Edge ed in WorldGen.AppMap.Edges.Values)
		{
			if (ed.River > 0)
			{
				Vector2 point = ed.Point;
				if(float.IsNaN(point.x) || float.IsNaN(point.y)) 
				{
					Debug.Log("Line Error - X: " + point.x + " Y: " + point.y);
					ed.Point.Set(0, 0);
					point = ed.Point;
				}
				GameObject lineGO = (GameObject)Instantiate(line, new Vector3(point.x,point.y,-150),Quaternion.identity);
				lineGO.name = "River: " + ed.Point;
				LineRenderer lr = lineGO.GetComponent<LineRenderer>();
				lr.SetPosition(0,new Vector3(ed.VoronoiStart.Point.x,ed.VoronoiStart.Point.y,-150));
				lr.SetPosition(1,new Vector3(ed.VoronoiEnd.Point.x,ed.VoronoiEnd.Point.y,-150));
				lr.SetWidth(ed.River, ed.River);
				lr.SetColors(Color.blue, Color.blue);
				lr.material = new Material (Shader.Find("Particles/Alpha Blended"));
				lr.material.color = Color.blue;
			}
		}
	}
}
