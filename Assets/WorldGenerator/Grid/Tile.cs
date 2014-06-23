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
	[ProtoMember(2)]
	public int Elevation { get; set; }
	[ProtoMember(3)]
	public int PointX { get; set; }
	[ProtoMember(4)]
	public int PointY { get; set; }
	[ProtoMember(5)]
	public bool IsBlocked { get; set; }
	[ProtoMember(6)]
	public string Mineral { get; set; }
	[ProtoMember(7)]
	public bool Underground { get; set; }

	public Tile()
	{
		this.Biome = "Empty";
		this.IsBlocked = false;
		this.Mineral = "Air";
		this.Underground = false;
	}
}
