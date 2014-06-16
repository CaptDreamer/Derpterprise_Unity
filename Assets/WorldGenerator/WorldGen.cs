using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BenTools.Mathematics;

public class WorldGen : MonoBehaviour {

	public GameObject chunk;
	public GameObject[,,] chunks;
	public int chunkSize=16;

	public static int width;
	int height;
	int MapX, MapY;

	private IIslandService IslandHandler;

	public int DotCount;

	VoronoiGraph voronoiMap;
	GridMap fullMap;

	public static Map AppMap { get; set; }

	public static void ResetMap()
	{
		WorldGen.AppMap.Centers.Clear();
		WorldGen.AppMap.Edges.Clear();
		WorldGen.AppMap.Corners.Clear();
	}
		
	// Use this for initialization
	void Start () {
		width = 250;
		height = width;
		WorldGen.AppMap = null;
		WorldGen.AppMap = new Map();
		CreateVoronoiGraph();
		CleanUpGraph(true);
		CreateGridMap ();
		ExtraChunky ();
	}

	void ExtraChunky()
	{
		chunks=new GameObject[Mathf.FloorToInt(width/chunkSize),
		                      Mathf.FloorToInt(width/chunkSize),
		                      Mathf.FloorToInt(50/chunkSize)];
		
		for (int x=0; x<chunks.GetLength(0); x++)
		{
			for (int y=0; y<chunks.GetLength(1); y++)
			{
				for (int z=0; z<chunks.GetLength(2); z++)
				{
					chunks[x,y,z]= Instantiate(chunk,
					                           new Vector3(x*chunkSize,y*chunkSize,z*chunkSize),
					                           new Quaternion(0,0,0,0)) as GameObject;
					
					Chunk newChunkScript= chunks[x,y,z].GetComponent("Chunk") as Chunk;
			   
					newChunkScript.map=fullMap;
					newChunkScript.chunkSize=chunkSize;
					newChunkScript.chunkX=x*chunkSize;
					newChunkScript.chunkY=y*chunkSize;
					newChunkScript.chunkZ=z*chunkSize;
					newChunkScript.GenerateMesh();
				}
			}
		}
	}

	void CreateGridMap()
	{
		fullMap = new GridMap (MapX, MapY);
		List<Tile> empty = new List<Tile> ();

		for(int i = 0; i < MapX; i++)
		{
			for(int j = 0; j < MapY; j++)
			{	
				fullMap.map[i,j,0].Point = new Vector2(i,j);
				empty.Add(fullMap.map[i,j,0]);
				Vector3 p = new Vector3(i, j, 0);
				foreach(Center c in WorldGen.AppMap.Centers.Values)
				{
					if(c.Contains(p))
					{
						fullMap.map[i,j,0].Biome = c.Biome;
						fullMap.map[i,j,0].Color = c.PolygonBrush;
						fullMap.map[i,j,0].Elevation = (int)(c.Elevation * 50);
						empty.Remove(fullMap.map[i,j,0]);
						break;
					}
				}
			}
		}

		foreach(Tile tile in empty)
		{
			int countX = ((int)tile.Point.x + 1) < MapX ? (int)tile.Point.x + 1 : MapX-1;
			int countY = ((int)tile.Point.y + 1) < MapY ? (int)tile.Point.y + 1 : MapY-1;
			int countL = ((int)tile.Point.x - 1) >= 0 ? (int)tile.Point.x - 1 : 0;
			int countD = ((int)tile.Point.y - 1) >= 0 ? (int)tile.Point.y - 1 : 0;

			while(tile.Biome == "Empty")
			{
				if(fullMap.map[countX,countY,0].Biome != "Empty")
				{
					tile.Biome = fullMap.map[countX,countY,0].Biome;
					tile.Color = fullMap.map[countX,countY,0].Color;
					tile.Elevation = fullMap.map[countX,countY,0].Elevation;
				} else if (fullMap.map[countX,countD,0].Biome != "Empty") {
					tile.Biome = fullMap.map[countX,countD,0].Biome;
					tile.Color = fullMap.map[countX,countD,0].Color;
					tile.Elevation = fullMap.map[countX,countD,0].Elevation;
				} else if (fullMap.map[countL,countY,0].Biome != "Empty") {
					tile.Biome = fullMap.map[countL,countY,0].Biome;
					tile.Color = fullMap.map[countL,countY,0].Color;
					tile.Elevation = fullMap.map[countL,countY,0].Elevation;
				} else if (fullMap.map[countL,countD,0].Biome != "Empty") {
					tile.Biome = fullMap.map[countL,countD,0].Biome;
					tile.Color = fullMap.map[countL,countD,0].Color;
					tile.Elevation = fullMap.map[countL,countD,0].Elevation;
				}

				countX = (countX + 1) < MapX ? countX + 1 : MapX-1;
				countY = (countY + 1) < MapY ? countY + 1 : MapY-1;
				countL = (countL - 1) >= 0 ? countL - 1 : 0;
				countD = (countD - 1) >= 0 ? countD - 1 : 0;
				
				if(countX == MapX-1 && countY == MapY-1 && countL == 0 && countD == 0)
					tile.Biome = "Broken";
			}
		}

		for(int i = 0; i < MapX; i++)
		{
			for(int j = 0; j < MapY; j++)
			{
				int elevation = fullMap.map[i,j,0].Elevation;
				if(elevation > 0)
				{
					fullMap.map[i,j,elevation].Biome = fullMap.map[i,j,0].Biome;
					fullMap.map[i,j,elevation].Point = fullMap.map[i,j,0].Point;
					fullMap.map[i,j,elevation].Color = fullMap.map[i,j,0].Color;
					fullMap.map[i,j,elevation].Elevation = fullMap.map[i,j,0].Elevation;
					for(int z = 0; z < elevation; z++)
					{
						fullMap.map[i,j,z].Biome = "Underground";
						fullMap.map[i,j,z].Color = Color.black;
					}
					if (fullMap.map[i,j,elevation].Biome == "OceanFloor")
					{
						for(int z = elevation + 1; z <= 25; z++)
						{
							fullMap.map[i,j,z].Biome = "Ocean";
							fullMap.map[i,j,z].Color = Color.blue;
						}
					}
				}
			}
		}
	}

	bool checkIfInside(Vector3 point)
	{
		Vector3 direction = new Vector3 (0, 1, 0);

		if(Physics.Raycast(point, direction, Mathf.Infinity) &&
		   Physics.Raycast(point, -direction, Mathf.Infinity)) {
			return true;
		}

		else return false;
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
}
