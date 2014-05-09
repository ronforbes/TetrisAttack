using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour 
{
	public int DyingCount;
	public int Score;

	// Use this for initialization
	void Start () {
	
	}

	public void Lose()
	{
		print ("Game over");
	}

	// Update is called once per frame
	void Update () {
	
	}
}
