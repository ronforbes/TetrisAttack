using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour 
{
	public enum BlockState
	{
		Awakening,
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
	public float AwakenDuration;
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
	float popDuration;

	// Use this for initialization
	void Start() 
	{
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

	// Untested
	public void InitializeAwakening(int x, int y, int flavor, float popDuration, float awakenDuration, ComboTabulator combo, int popColor)
	{
		X = x;
		Y = y;
		Flavor = flavor;

		State = BlockState.Awakening;
		AwakenDuration = awakenDuration;
		this.popDuration = popDuration;
		//popDirection = BlockManager.GeneratePopDirection();
		//this.popColor = popColor;
		CurrentCombo = combo;

		CurrentCombo.IncrementInvolvement();

		//Game.AwakeningCount++;

		grid.AddBlock(x, y, this, GridElement.ElementState.Immutable);
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
			else
				return;
			break;
		case BlockState.Awakening: // Untested
			/* popElapsed += Time.deltaTime;
			 * 
			 * if(popElapsed >= popDuration)
			 * {
			 * 		// Play the pop sound
			 * }
			 * 
			 * if( and some more stuff )
			 */ 
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

				// tell our upward neighbor to fall
				if(Y < Grid.PlayHeight - 1)
				{
					if(grid.StateAt(X, Y + 1) == GridElement.ElementState.Block)
						grid.BlockAt(X, Y + 1).StartFalling(CurrentCombo);
					// TODO: do the same for garbage
				}

				CurrentCombo.DecrementInvolvement();

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

	public void StartFalling(ComboTabulator combo = null)
	{
		if(State != BlockState.Static)
			return;

		// change our state
		State = BlockState.Falling;

		FallElapsed = FallDuration;

		grid.ChangeState(X, Y, this, GridElement.ElementState.Falling);

		if(combo != null)
		{
			BeginComboInvolvement(combo);
		}

		if(Y < Grid.PlayHeight - 1)
		{
			if(grid.StateAt(X, Y + 1) == GridElement.ElementState.Block)
				grid.BlockAt(X, Y + 1).StartFalling(CurrentCombo);
		}
	}

	public void StartDying(ComboTabulator combo, int sparkNumber)
	{
		// change the game state
		game.DyingCount++;

		BeginComboInvolvement(combo);

		State = BlockState.Dying;
		DieElapsed = 0.0f;

		grid.ChangeState(X, Y, this, GridElement.ElementState.Immutable);

		DyingAxis = Random.insideUnitCircle;
	}

	public void BeginComboInvolvement(ComboTabulator combo)
	{
		if(CurrentCombo != null)
		{
			CurrentCombo.DecrementInvolvement();
		}

		CurrentCombo = combo;
		CurrentCombo.IncrementInvolvement();
	}

	public void EndComboInvolvement(ComboTabulator combo)
	{
		if(CurrentCombo != null && CurrentCombo == combo)
		{
			CurrentCombo.DecrementInvolvement();
			CurrentCombo = null;
		}
	}
}
