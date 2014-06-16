using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BenTools.Mathematics;

public static class WorldGen {

	//variable for map size
	public static int width { get; set; }
	public static int height { get; set; }
	public static int MapX { get; set; }
	public static int MapY { get; set; }

	//complexity of map
	public static int DotCount { get; set; }

	//Island generator
	private static IIslandService IslandHandler;
	
	//Voronoi Graph Support
	public static VoronoiGraph voronoiMap { get; set; }

	//Rasterized map support
	public static GridMap fullMap { get; set; }

	//Vector map support
	public static Map AppMap { get; set; }

	//Clear previous map info
	public static void ResetMap()
	{
		WorldGen.AppMap.Centers.Clear();
		WorldGen.AppMap.Edges.Clear();
		WorldGen.AppMap.Corners.Clear();
	}
		
	/// <summary>
	/// Initializes the <see cref="WorldGen"/> class.
	/// </summary>
	public static void Start () {

		//Set the world to 250x250
		WorldGen.width = 250;
		WorldGen.height = WorldGen.width;

		//Set the Complexity to 1000
		WorldGen.DotCount = 1000;

		//Init the vector map
		WorldGen.AppMap = null;
		WorldGen.AppMap = new Map();

		//Create the Voronoi Graph
		CreateVoronoiGraph();

		//Create the Vector Map
		CleanUpGraph(true);

		//Create the Rasterized Map
		CreateGridMap ();

		//Load the actual game scene
		StartGame ();
	}

	/// <summary>
	/// Starts the game.
	/// </summary>
	static void StartGame ()
	{
		//Pass the finalized map to the game
		MainGame.fullMap = WorldGen.fullMap;

		//load the game level
		Application.LoadLevel (1);
	}

	/// <summary>
	/// Creates the grid map.
	/// </summary>
	static void CreateGridMap()
	{
		//init the worldmap and the tilesupport
		WorldGen.fullMap = new GridMap (WorldGen.MapX, WorldGen.MapY);
		List<Tile> empty = new List<Tile> ();

		//loop through all the tiles on the bottom floor
		for(int i = 0; i < WorldGen.MapX; i++)
		{
			for(int j = 0; j < WorldGen.MapY; j++)
			{	
				//set this tile's point data correctly
				WorldGen.fullMap.map[i,j,0].Point = new Vector2(i,j);

				//add the tile to the empty list (will be removed if not empty)
				empty.Add(WorldGen.fullMap.map[i,j,0]);

				//store the point location
				Vector3 p = new Vector3(i, j, 0);

				//Loop through all the map centers to see if this tile is in any of them
				foreach(Center c in WorldGen.AppMap.Centers.Values)
				{

					//if it is, set its data to that biome and elevation and go to the next tile (no need to loop through more polys)
					//also, remove this tile from the empty list as it is not empty
					if(c.Contains(p))
					{
						WorldGen.fullMap.map[i,j,0].Biome = c.Biome;
						WorldGen.fullMap.map[i,j,0].Color = c.PolygonBrush;
						WorldGen.fullMap.map[i,j,0].Elevation = (int)(c.Elevation * 50);
						empty.Remove(WorldGen.fullMap.map[i,j,0]);
						break;
					}
				}
			}
		}

		//all the tiles in here are likely Ocean tiles on the border, we'll get the nearest tiles info diagonally from this one.
		foreach(Tile tile in empty)
		{
			//init the distance counters
			int countX = ((int)tile.Point.x + 1) < WorldGen.MapX ? (int)tile.Point.x + 1 : WorldGen.MapX-1;
			int countY = ((int)tile.Point.y + 1) < WorldGen.MapY ? (int)tile.Point.y + 1 : WorldGen.MapY-1;
			int countL = ((int)tile.Point.x - 1) >= 0 ? (int)tile.Point.x - 1 : 0;
			int countD = ((int)tile.Point.y - 1) >= 0 ? (int)tile.Point.y - 1 : 0;

			//loop through all the tiles diagonally from this one, if found a non-empty tile, grab that tiles info and make this tile like it's closest neighbour
			while(tile.Biome == "Empty")
			{
				if(WorldGen.fullMap.map[countX,countY,0].Biome != "Empty")
				{
					tile.Biome = WorldGen.fullMap.map[countX,countY,0].Biome;
					tile.Color = WorldGen.fullMap.map[countX,countY,0].Color;
					tile.Elevation = WorldGen.fullMap.map[countX,countY,0].Elevation;
				} else if (WorldGen.fullMap.map[countX,countD,0].Biome != "Empty") {
					tile.Biome = WorldGen.fullMap.map[countX,countD,0].Biome;
					tile.Color = WorldGen.fullMap.map[countX,countD,0].Color;
					tile.Elevation = WorldGen.fullMap.map[countX,countD,0].Elevation;
				} else if (WorldGen.fullMap.map[countL,countY,0].Biome != "Empty") {
					tile.Biome = WorldGen.fullMap.map[countL,countY,0].Biome;
					tile.Color = WorldGen.fullMap.map[countL,countY,0].Color;
					tile.Elevation = WorldGen.fullMap.map[countL,countY,0].Elevation;
				} else if (WorldGen.fullMap.map[countL,countD,0].Biome != "Empty") {
					tile.Biome = WorldGen.fullMap.map[countL,countD,0].Biome;
					tile.Color = WorldGen.fullMap.map[countL,countD,0].Color;
					tile.Elevation = WorldGen.fullMap.map[countL,countD,0].Elevation;
				}

				//didn't find any tiles, increase the distance by 1 tile in all directions.
				countX = (countX + 1) < WorldGen.MapX ? countX + 1 : WorldGen.MapX-1;
				countY = (countY + 1) < WorldGen.MapY ? countY + 1 : WorldGen.MapY-1;
				countL = (countL - 1) >= 0 ? countL - 1 : 0;
				countD = (countD - 1) >= 0 ? countD - 1 : 0;

				//if we've reached the edges of the map in all directions, this tile is broken (Should not happen)
				if(countX == WorldGen.MapX-1 && countY == WorldGen.MapY-1 && countL == 0 && countD == 0)
					tile.Biome = "Broken";
			}
		}

		//loop through all the tiles and set their elevation accordingly.
		for(int i = 0; i < WorldGen.MapX; i++)
		{
			for(int j = 0; j < WorldGen.MapY; j++)
			{
				//init the elevation
				int elevation = WorldGen.fullMap.map[i,j,0].Elevation;

				//if the elevation is 0, there's nothing left to do.
				if(elevation > 0)
				{
					//Copy the tile to its correct elevation
					WorldGen.fullMap.map[i,j,elevation].Biome = WorldGen.fullMap.map[i,j,0].Biome;
					WorldGen.fullMap.map[i,j,elevation].Point = WorldGen.fullMap.map[i,j,0].Point;
					WorldGen.fullMap.map[i,j,elevation].Color = WorldGen.fullMap.map[i,j,0].Color;
					WorldGen.fullMap.map[i,j,elevation].Elevation = WorldGen.fullMap.map[i,j,0].Elevation;

					//loop through all the tiles underneath this tile and set them to underground tiles.
					for(int z = 0; z < elevation; z++)
					{
						WorldGen.fullMap.map[i,j,z].Biome = "Underground";
						WorldGen.fullMap.map[i,j,z].Color = Color.black;
						WorldGen.fullMap.map[i,j,z].Elevation = z;
						WorldGen.fullMap.map[i,j,z].Point = new Vector2(i,j);
					}

					//if the tile is an OceanFloor, we need to put the Ocean above it as well. loop through all the tiles above this one and set it to Ocean up to 25 (sealevel)
					if (WorldGen.fullMap.map[i,j,elevation].Biome == "OceanFloor")
					{
						for(int z = elevation + 1; z <= 25; z++)
						{
							WorldGen.fullMap.map[i,j,z].Biome = "Ocean";
							WorldGen.fullMap.map[i,j,z].Color = Color.blue;
							WorldGen.fullMap.map[i,j,z].Elevation = z;
							WorldGen.fullMap.map[i,j,z].Point = new Vector2(i,j);
						}
					}
				}
			}
		}
	}
//
//	/// <summary>
//	/// Checks if inside.
//	/// </summary>
//	/// <returns><c>true</c>, if if inside was checked, <c>false</c> otherwise.</returns>
//	/// <param name="point">Point.</param>
//	static bool checkIfInside(Vector3 point)
//	{
//		Vector3 direction = new Vector3 (0, 1, 0);
//
//		if(Physics.Raycast(point, direction, Mathf.Infinity) &&
//		   Physics.Raycast(point, -direction, Mathf.Infinity)) {
//			return true;
//		}
//
//		else return false;
//	}

	/// <summary>
	/// Creates the voronoi graph.
	/// </summary>
	static void CreateVoronoiGraph() 
	{
		//Set map sizes
		WorldGen.MapX = WorldGen.width;
		WorldGen.MapY = WorldGen.height;

		//Create the IslandHandler
		WorldGen.IslandHandler = new IslandService (WorldGen.MapX, WorldGen.MapY);

		//init the points hash
		var points = new HashSet<BenTools.Mathematics.Vector>();

		//for each dot in the complexity setting create random dots that are within the map and add them to the points list
		for(int i = 0; i < WorldGen.DotCount; i++)
		{
			points.Add(new BenTools.Mathematics.Vector(Random.Range(0,WorldGen.MapX), Random.Range(0,WorldGen.MapY)));
		}

		//init the map
		WorldGen.voronoiMap = null;

		//iterate on the graph 3 times (more is square, less is chaotic)
		for(int i = 0; i < 3; i++)
		{
			//set the map to the points by computing the graph
			WorldGen.voronoiMap = Fortune.ComputeVoronoiGraph(points);

			//for each point in the list, do some error checking and reprocessing
			foreach(BenTools.Mathematics.Vector vector in points)
			{
				double v0 = 0.0d;
				double v1 = 0.0d;
				int say = 0;
				foreach(VoronoiEdge edge in WorldGen.voronoiMap.Edges)
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

				if (((v0 / say) < WorldGen.MapX) && ((v0 / say) > 0))
				{
					vector[0] = v0 / say;
				}

				if (((v1 / say) < WorldGen.MapY) && ((v1 / say) > 0))
				{
					vector[1] = v1 / say;
				}
			}
		}

		//after 3 runs our grid should be good and we can save the final map
		WorldGen.voronoiMap = Fortune.ComputeVoronoiGraph(points);
	}

	/// <summary>
	/// Create the vector map from the voronoi information
	/// </summary>
	/// <param name="fix">If set to <c>true</c> fix.</param>
	static void CleanUpGraph(bool fix = false)
	{
		//initialize the factory
		IFactory fact = new MapItemFactory();

		//for all the edges, create corners and centers
		foreach (VoronoiEdge edge in WorldGen.voronoiMap.Edges)
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

		//Once the vector grid is created, build the island from the datapoints
		WorldGen.IslandHandler.CreateIsland();
	}

	/// <summary>
	/// Fixs the points.
	/// </summary>
	/// <returns><c>true</c>, if points was fixed, <c>false</c> otherwise.</returns>
	/// <param name="edge">Edge.</param>
	static private bool FixPoints(VoronoiEdge edge)
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
							if (i > 0 && i < WorldGen.MapY)
								edge.VVertexB = new BenTools.Mathematics.Vector(0, i);
							
						}
						else
						{
							i = (WorldGen.MapX - b) / slope;
							if (i > 0 && i < WorldGen.MapY)
								edge.VVertexB = new BenTools.Mathematics.Vector(i, WorldGen.MapY);
							
						}
					}
					else
					{
						if (WorldGen.MapX - x3 > y3)
						{
							i = (-b / slope);
							if (i > 0 && i < WorldGen.MapX)
								edge.VVertexB = new BenTools.Mathematics.Vector(i, 0);
						}
						else
						{
							i = (WorldGen.MapX * slope) + b;
							if (i > 0 && i < WorldGen.width)
								edge.VVertexB = new BenTools.Mathematics.Vector(WorldGen.MapX, i);
							
						}
					}				
				}
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Check to see if the dot is in the map
	/// </summary>
	/// <returns><c>true</c>, if in map was doted, <c>false</c> otherwise.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	static private bool DotInMap(double x, double y)
	{
		if (x == double.NaN || y == double.NaN)
			return false;
		return (x > 0 && x < WorldGen.MapX) && (y > 0 && y < WorldGen.MapY);
	}
}
