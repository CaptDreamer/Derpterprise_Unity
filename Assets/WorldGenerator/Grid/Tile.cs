using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using ProtoBuf;

[ProtoContract]
public class Tile  {

	[ProtoMember(1)]
	public string Biome { get; set; }
	//public Color Color { get; set; }
	[ProtoMember(2)]
	public int Elevation { get; set; }
	[ProtoMember(3)]
	public int PointX { get; set; }
	[ProtoMember(4)]
	public int PointY { get; set; }

	public Tile()
	{
		Biome = "Empty";
	}

//	private Tile (SerializationInfo info, StreamingContext ctxt)
//	{
//		this.Biome = info.GetString ("Biome");
//		this.Elevation = info.GetInt32 ("Elevation");
//		this.PointX = info.GetInt32 ("PointX");
//		this.PointY = info.GetInt32 ("PointY");
//	}
//	
//	//[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
//	[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
//	public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
//	{
//		info.AddValue ("Biome", this.Biome);
//		info.AddValue ("Elevation", this.Elevation);
//		info.AddValue ("PointX", this.PointX);
//		info.AddValue ("PointY", this.PointY);
//	}
}
