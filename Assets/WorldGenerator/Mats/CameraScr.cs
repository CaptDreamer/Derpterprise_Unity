using UnityEngine;
using System.Collections;

public class CameraScr : MonoBehaviour {

	Vector3 around;

	// Use this for initialization
	void Start () {
		around = new Vector3 (125, 125, 0);
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.RotateAround (around, new Vector3 (0, 0, 1), 1f);
	}
}
