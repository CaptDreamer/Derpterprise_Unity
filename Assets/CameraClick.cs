using UnityEngine;
using System.Collections;

public class CameraClick : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) 
		{
			Debug.Log("Clicked");
			Ray ray = this.camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit))
			{
				//Debug.Log(hit.point);
				WorldGen.GetTile(hit.point);
				
			}
		}
	}
}
