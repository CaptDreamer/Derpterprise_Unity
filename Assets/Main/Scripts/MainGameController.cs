using UnityEngine;
using System.Collections;

public class MainGameController : MonoBehaviour {

	float nextMove;


	// Use this for initialization
	void Start () {
		nextMove = Time.time + 1;
	}
	
	// Update is called once per frame
	void Update () {

		if(MainGame.gameState == GameState.Playing && Time.time > nextMove)
		{
			MainGame.Update();
			nextMove = Time.time + 1;;
		}
	}

}
