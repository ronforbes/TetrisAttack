using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour 
{
	public enum BlockState
	{
		Static,
		Swapping,
	}

	public int Id;
	public int Flavor;
	public int X, Y;
	public BlockState State;
	public Swapper.SwapDirection Direction;
	public bool SwapFront;

	public const int FlavorCount = 5;
	
	int swapVelocity;

	// Use this for initialization
	void Start () {
	
	}

	public void InitializeStatic(int x, int y, int flavor)
	{
		X = x;
		Y = y;
		Flavor = flavor;

		State = BlockState.Static;

		Grid.AddBlock(x, y, this, GridElement.ElementState.Block);

		transform.position = new Vector3(X, Y, 0.0f);
	}

	// Update is called once per frame
	void Update () {

	}

	public void StartSwapping(Swapper.SwapDirection direction, bool swapFront)
	{
		State = BlockState.Swapping;

		Direction = direction;

		SwapFront = swapFront;

		Grid.ChangeState(X, Y, this, GridElement.ElementState.Immutable);
	}

	public void FinishSwapping(int swapX)
	{
		State = BlockState.Static;

		Direction = Swapper.SwapDirection.None;

		X = swapX;

		Grid.AddBlock(X, Y, this, GridElement.ElementState.Block);
	}
}
