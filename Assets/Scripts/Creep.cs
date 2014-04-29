using UnityEngine;
using System.Collections;

public class Creep : MonoBehaviour 
{
	public BlockManager BlockManager;
	public Grid Grid;
	public float CreepElapsed;
	public const float CreepDuration = 10.0f;

	// Use this for initialization
	void Start () 
	{
		CreepElapsed = 0.0f;

		BlockManager.NewCreepRow();
	}
	
	// Update is called once per frame
	void Update () 
	{
		CreepElapsed += Time.deltaTime;

		if(CreepElapsed >= CreepDuration)
		{
			CreepElapsed = 0.0f;

			// shift everything up one grid row
			if(Grid.ShiftUp())
			{
				// create a new bottom creep row
				BlockManager.NewCreepRow();
				
				//link the elimination requests
				for(int x = 0; x < Grid.PlayWidth; x++)
				{
					//Grid.RequestEliminationCheck(Grid.BlockAt(x, 1));
				}
			}
		}
	}
}
