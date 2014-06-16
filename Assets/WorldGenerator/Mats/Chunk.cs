using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chunk : MonoBehaviour {
	
	private List<Vector3> newVertices = new List<Vector3>();
	private List<int> newTriangles = new List<int>();
	private List<Vector2> newUV = new List<Vector2>();
	private List<Color> newColor = new List<Color> ();
	
	private float tUnit = 0.25f;
//	private Vector2 tLBlue = new Vector2 (0, 0);
//	private Vector2 tPuke = new Vector2 (0, 1);
//	private Vector2 tForest = new Vector2 (0, 2);
//	private Vector2 tPurple = new Vector2 (0, 3);
//	private Vector2 tDBlue = new Vector2 (1, 0);
//	private Vector2 tBrown = new Vector2 (1, 1);
//	private Vector2 tBurgundy = new Vector2 (1, 2);
//	private Vector2 tTeal = new Vector2 (1, 3);
//	private Vector2 tDPurple = new Vector2 (2, 0);
//	private Vector2 tGreen = new Vector2 (2, 1);
//	private Vector2 tYellow = new Vector2 (2, 2);
//	private Vector2 tBlue = new Vector2 (2, 3);
//	private Vector2 tWhite = new Vector2 (3, 0);
//	private Vector2 tGray = new Vector2 (3, 1);
//	private Vector2 tDGray = new Vector2 (3, 2);
//	private Vector2 tBlack = new Vector2 (3, 3);


	
	private Mesh mesh;
	private MeshCollider col;
	
	private int faceCount;

	public int chunkSize=16;
	public int chunkX;
	public int chunkY;
	public int chunkZ;

	public GridMap map;

	void Start () { 

	}

	void UpdateMesh()
	{ 
		mesh = GetComponent<MeshFilter> ().mesh;
		mesh.Clear ();
		mesh.vertices = newVertices.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.colors = newColor.ToArray ();
		mesh.Optimize ();
		mesh.RecalculateNormals ();
//		for(int i = 0; i < newTriangles.Count; i++)
//		{
//			Debug.Log( mesh.triangles[i]);
//		}



		//col.sharedMesh=null;
		//col.sharedMesh=mesh;
		
		newVertices.Clear();
		newUV.Clear();
		newTriangles.Clear();
		
		faceCount=0; //Fixed: Added this thanks to a bug pointed out by ratnushock!
	}
	
	public void GenerateMesh(){
		
		for (int x=0; x<chunkSize; x++)
		{
			for (int y=0; y<chunkSize; y++)
			{
				for (int z=0; z<chunkSize; z++)
				{
					//This code will run for every block in the chunk
					
					if(Block(x,y,z)!="Empty")
					{
						//If the block is solid
						
						if( Block(x,y+1,z) == "Empty")
						{
							//Block above is air
							CubeTop(x,y,z, ColorGet(x,y,z));
						}
						
						if( Block(x,y-1,z) == "Empty")
						{
							//Block below is air
							CubeBot(x,y,z, ColorGet(x,y,z));
						}
						
						if( Block(x+1,y,z) == "Empty")
						{
							//Block east is air
							CubeEast(x,y,z, ColorGet(x,y,z));
						}
						
						if( Block(x-1,y,z) == "Empty")
						{
							//Block west is air
							CubeWest(x,y,z, ColorGet(x,y,z));
						}
						
						if( Block(x,y,z+1) == "Empty")
						{
							//Block north is air
							CubeNorth(x,y,z, ColorGet(x,y,z));
						}
						
						if( Block(x,y,z-1) == "Empty")
						{
							//Block south is air
							CubeSouth(x,y,z, ColorGet(x,y,z));
						}
					}
				}
			}
		}
		
		UpdateMesh ();
	}
	
	
	void CubeTop (int x, int y, int z, Color biome) 
	{
		newVertices.Add(new Vector3 (x,  y,  z + 1));
		newVertices.Add(new Vector3 (x + 1, y,  z + 1));
		newVertices.Add(new Vector3 (x + 1, y,  z ));
		newVertices.Add(new Vector3 (x,  y,  z ));

		Cube (biome);
	}

	
	void CubeNorth (int x, int y, int z, Color biome) 
	{
		newVertices.Add(new Vector3 (x + 1, y-1, z + 1));
		newVertices.Add(new Vector3 (x + 1, y, z + 1));
		newVertices.Add(new Vector3 (x, y, z + 1));
		newVertices.Add(new Vector3 (x, y-1, z + 1));

		Cube (biome);
	}
	
	void CubeEast (int x, int y, int z, Color biome) 
	{
		newVertices.Add(new Vector3 (x + 1, y - 1, z));
		newVertices.Add(new Vector3 (x + 1, y, z));
		newVertices.Add(new Vector3 (x + 1, y, z + 1));
		newVertices.Add(new Vector3 (x + 1, y - 1, z + 1));

		Cube (biome);
	}
	
	void CubeSouth (int x, int y, int z, Color biome) 
	{
		newVertices.Add(new Vector3 (x, y - 1, z));
		newVertices.Add(new Vector3 (x, y, z));
		newVertices.Add(new Vector3 (x + 1, y, z));
		newVertices.Add(new Vector3 (x + 1, y - 1, z));

		Cube (biome);
	}
	
	void CubeWest (int x, int y, int z, Color biome) 
	{
		newVertices.Add(new Vector3 (x, y- 1, z + 1));
		newVertices.Add(new Vector3 (x, y, z + 1));
		newVertices.Add(new Vector3 (x, y, z));
		newVertices.Add(new Vector3 (x, y - 1, z));

		Cube (biome);
	}
	
	void CubeBot (int x, int y, int z, Color biome) 
	{
		newVertices.Add(new Vector3 (x,  y-1,  z ));
		newVertices.Add(new Vector3 (x + 1, y-1,  z ));
		newVertices.Add(new Vector3 (x + 1, y-1,  z + 1));
		newVertices.Add(new Vector3 (x,  y-1,  z + 1));

		Cube (biome);
	}

	void Cube (Color b) {
		newTriangles.Add(faceCount * 4  ); //1
		newTriangles.Add(faceCount * 4 + 1 ); //2
		newTriangles.Add(faceCount * 4 + 2 ); //3
		newTriangles.Add(faceCount * 4  ); //1
		newTriangles.Add(faceCount * 4 + 2 ); //3
		newTriangles.Add(faceCount * 4 + 3 ); //4

		Vector2 texturePos = new Vector2 (0, 1);

		newColor.Add (b);
		newColor.Add (b);
		newColor.Add (b);
		newColor.Add (b);

		newUV.Add(new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y));
		newUV.Add(new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y + tUnit));
		newUV.Add(new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y + tUnit));
		newUV.Add(new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y));
		
		faceCount++; // Add this line

	}

//	void Cube (string b) {
//		
//		newTriangles.Add(faceCount * 4  ); //1
//		newTriangles.Add(faceCount * 4 + 1 ); //2
//		newTriangles.Add(faceCount * 4 + 2 ); //3
//		newTriangles.Add(faceCount * 4  ); //1
//		newTriangles.Add(faceCount * 4 + 2 ); //3
//		newTriangles.Add(faceCount * 4 + 3 ); //4
//
//		Vector2 texturePos;
//		//Debug.Log (b);
//		switch(b)
//		{
//		case "Ocean":
//			texturePos = tDBlue;
//			break;
//
//		case "OceanFloor":
//			texturePos = tTeal;
//			break;
//
//		case "Marsh":
//			texturePos = tLBlue;
//			break;
//
//		case "Lake":
//			texturePos = tBlue;
//			break;
//
//		case "Beach":
//			texturePos = tYellow;
//			break;
//
//		case "Snow":
//			texturePos = tWhite;
//			break;
//
//		case "Tundra":
//			texturePos = tDPurple;
//			break;
//
//		case "Bare":
//			texturePos = tBrown;
//			break;
//
//		case "Scorched":
//			texturePos = tBurgundy;
//			break;
//			
//		case "Taiga":
//			texturePos = tDGray;
//			break;
//			
//		case "Shrubland":
//			texturePos = tGreen;
//			break;
//			
//		case "TemperateDesert":
//			texturePos = tGray;
//			break;
//			
//		case "TemperateRainForest":
//			texturePos = tForest;
//			break;
//			
//		case "TemperateDeciduousForest":
//			texturePos = tForest;
//			break;
//			
//		case "TropicalRainForest":
//			texturePos = tPuke;
//			break;
//			
//		case "TropicalSeasonalForest":
//			texturePos = tForest;
//			break;
//		
//		case "SubtropicalDesert":
//			texturePos = tWhite;
//			break;
//		
//		case "Underground":
//			texturePos = tBlack;
//			break;
//
//		default:
//			texturePos = tPurple;
//			break;
//		}
//
//		newUV.Add(new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y));
//		newUV.Add(new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y + tUnit));
//		newUV.Add(new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y + tUnit));
//		newUV.Add(new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y));
//		
//		faceCount++; // Add this line
//	}

	string Block(int x, int y, int z){
		int xC = x + chunkX;
		int yC = y + chunkY;
		int zC = z + chunkZ;
		if (xC >= WorldGen.width || xC < 0
						|| yC >= WorldGen.width || yC < 0
						|| zC >= 50 || zC < 0)
						return "Empty";
		//Debug.Log (WorldGen.width);
		//Debug.Log (xC + "," + yC + "," + zC);
		return map.map[xC,yC,zC].Biome;
	}

	Color ColorGet(int x, int y, int z){
		int xC = x + chunkX;
		int yC = y + chunkY;
		int zC = z + chunkZ;
		if (xC >= WorldGen.width || xC < 0
		    || yC >= WorldGen.width || yC < 0
		    || zC >= 50 || zC < 0)
			return new Color (0,0,0,0);
		//Debug.Log (WorldGen.width);
		//Debug.Log (xC + "," + yC + "," + zC);
		return map.map[xC,yC,zC].Color;
	}

}
