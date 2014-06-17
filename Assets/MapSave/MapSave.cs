using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using ProtoBuf;

public static class MapSave  {

	//GridMap saveMap;

	public static void SaveMap(GridMap map)
	{
//		IFormatter formatter = new BinaryFormatter ();
//		Stream stream = new FileStream ("MapSave.bin", FileMode.Create, FileAccess.Write, FileShare.None);
//		formatter.Serialize (stream, map);
//		stream.Close ();

//		Stream stream = null;
//		stream = File.Open ("MapSave.bin", FileMode.Create);
//		BinaryFormatter bFormatter = new BinaryFormatter ();
//		bFormatter.Serialize (stream, map);
//		stream.Close ();

		using (var file = File.Create("Assets/MapSave/Saves/MapSave.bin")) {
			Serializer.Serialize<GridMap>(file, map);
		}
	}


		
	public static GridMap LoadMap()
	{
//		IFormatter formatter = new BinaryFormatter ();
//		Stream stream = new FileStream ("MapSave.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
//		GridMap map = (GridMap)formatter.Deserialize (stream);
//		stream.Close ();
//		return map;

//		Stream stream = null;
//		stream = File.Open ("MapSave.bin", FileMode.Open);
//		BinaryFormatter bFormatter = new BinaryFormatter ();
//		GridMap mapDeserialized = (GridMap)bFormatter.Deserialize (stream);
//		stream.Close ();
//		return mapDeserialized;

		GridMap loadMap;
		using (var file = File.OpenRead("Assets/MapSave/Saves/MapSave.bin")) {
			loadMap = Serializer.Deserialize<GridMap>(file);
		}
		return loadMap;
	}
}
