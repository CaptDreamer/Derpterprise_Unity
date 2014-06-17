using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	Camera cam;
	public int speed = 6;
	public TileMapController mapControl;

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
		if(Input.GetMouseButtonDown(0))
		{
			int xTile;
			int yTile;
			MainGame.tileMap.GetTileAtPosition(cam.ScreenToWorldPoint(Input.mousePosition), out xTile, out yTile);
			var tile = MainGame.fullMap.GetTile(xTile, yTile, MainGame.Level);
			Debug.Log(tile.Biome);
		}
	}
}
