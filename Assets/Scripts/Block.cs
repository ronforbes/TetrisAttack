using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour 
{
	public enum BlockState
	{
		Static,
		Swapping,
		Falling,
		Dying,
	}

	public int Id;
	public int Flavor;
	public int X, Y;
	public BlockState State;
	public ComboTabulator CurrentCombo;
	public Swapper.SwapDirection Direction;
	public bool SwapFront;
	public float FallElapsed;
	public float DieElapsed;
	public Vector2 DyingAxis;

	public const int FlavorCount = 5;
	public const float FallDuration = 0.1f;
	public const float DieDuration = 1.5f;	

	BlockManager blockManager;
	Grid grid;
	Game game;

	// Use this for initialization
	void Start () {
		blockManager = GameObject.Find("Block Manager").GetComponent<BlockManager>();
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		game = GameObject.Find ("Game").GetComponent<Game>();
	}

	public void InitializeStatic(int x, int y, int flavor)
	{
		X = x;
		Y = y;
		Flavor = flavor;

		State = BlockState.Static;

		// Initializing Grid here again. It looks like Start isn't getting called until the next frame
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		grid.AddBlock(x, y, this, GridElement.ElementState.Block);

		transform.position = new Vector3(X, Y, 0.0f);
	}

	// Update is called once per frame
	void Update () {
		// don't update the creep row
		if(Y == 0)
			return;

		switch(State)
		{
		case BlockState.Static:
			// we may have to fall
			if(grid.StateAt(X, Y - 1) == GridElement.ElementState.Empty)
				StartFalling();
			break;
		case BlockState.Falling:
			FallElapsed += Time.deltaTime;

			if(FallElapsed >= FallDuration)
			{
				if(grid.StateAt(X, Y - 1) == GridElement.ElementState.Empty)
				{
					// shift our grid position down to the next row
					Y--;
					FallElapsed = 0.0f;

					grid.Remove(X, Y + 1, this);
					grid.AddBlock(X, Y, this, GridElement.ElementState.Falling);
				}
				else
				{
					// we've landed

					// change our state
					State = BlockState.Static;

					// update the grid
					grid.ChangeState(X, Y, this, GridElement.ElementState.Block);

					// register for elimination checking
					grid.RequestEliminationCheck(this);
				}
			}
			break;
		case BlockState.Dying:
			DieElapsed += Time.deltaTime;

			if(DieElapsed >= DieDuration)
			{
				// change the game state
				game.DyingCount--;

				// update the grid
				grid.Remove(X, Y, this);

				// tell our upward neighbor to fall (TODO: should be a combo fall)
				if(Y < Grid.PlayHeight - 1)
				{
					if(grid.StateAt(X, Y + 1) == GridElement.ElementState.Block)
						grid.BlockAt(X, Y + 1).StartFalling();
					// TODO: do the same for garbage
				}

				blockManager.DeleteBlock(this);
			}
			break;
		}
	}

	public void StartSwapping(Swapper.SwapDirection direction, bool swapFront)
	{
		State = BlockState.Swapping;

		Direction = direction;

		SwapFront = swapFront;

		grid.ChangeState(X, Y, this, GridElement.ElementState.Immutable);
	}

	public void FinishSwapping(int swapX)
	{
		State = BlockState.Static;

		Direction = Swapper.SwapDirection.None;

		X = swapX;

		grid.AddBlock(X, Y, this, GridElement.ElementState.Block);
	}

	public void StartFalling()
	{
		if(State != BlockState.Static)
			return;

		// change our state
		State = BlockState.Falling;

		FallElapsed = FallDuration;

		grid.ChangeState(X, Y, this, GridElement.ElementState.Falling);

		if(Y < Grid.PlayHeight - 1)
		{
			if(grid.StateAt(X, Y + 1) == GridElement.ElementState.Block)
				grid.BlockAt(X, Y + 1).StartFalling();
		}
	}

	public void StartDying(int sparkNumber)
	{
		// change the game state
		game.DyingCount++;

		State = BlockState.Dying;
		DieElapsed = 0.0f;

		grid.ChangeState(X, Y, this, GridElement.ElementState.Immutable);

		DyingAxis = Random.insideUnitCircle;
	}
}
