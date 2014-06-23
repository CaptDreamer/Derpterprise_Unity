using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	Camera cam;
	public int speed = 6;
	public TileMapController mapControl;
	public GUIText guiTextOB;

	// Use this for initialization
	void Start () {
		cam = this.gameObject.camera;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetAxis("Horizontal") != 0)
		{
			cam.transform.Translate((Input.GetAxis("Horizontal") * Vector3.right) * Time.deltaTime * speed );
		}
		if(Input.GetAxis("Vertical") != 0)
		{
			cam.transform.Translate((Input.GetAxis("Vertical") * Vector3.up) * Time.deltaTime * speed);
		}
		if(Input.GetKeyDown(KeyCode.Period))
		{
			Debug.Log("Going Up");
			mapControl.UpLevel();
		}
		if(Input.GetKeyDown(KeyCode.Comma))
		{
			Debug.Log("Going Down");
			mapControl.DownLevel();
		}

		int xTile;
		int yTile;
		MainGame.tileMap.GetTileAtPosition(cam.ScreenToWorldPoint(Input.mousePosition), out xTile, out yTile);
		var tile = MainGame.fullMap.GetTile(xTile, yTile, MainGame.Level);
		string guiString = "Biome: " +tile.Biome + "\n\r" +
						   "Material: " + tile.Mineral + "\n\r" +
						   "Underground? " + tile.Underground + "\n\r" +
						   "Blocked? " + tile.IsBlocked + "\n\r" +
						   "Location: " + tile.PointX + ", " + tile.PointY + "\n\r" +
						   "Elevation: " + tile.Elevation;
		guiTextOB.text = guiString;
		guiTextOB.transform.position = cam.ScreenToViewportPoint(Input.mousePosition);

	}
}
