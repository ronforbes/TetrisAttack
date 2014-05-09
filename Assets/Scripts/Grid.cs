using UnityEngine;
using System.Collections;

public class GridElement
{
	public enum ElementState
	{
		Empty,
		Block,
		Immutable,
		Falling
	}

	public enum ElementType
	{
		Empty,
		Block
	}

	public ElementState State;
	public ElementType Type;
	public Object Element;
}

public class CheckRegistryElement
{
	public bool Mark;
	public ComboTabulator Combo;
}

public class Grid : MonoBehaviour 
{
	public BlockManager BlockManager;
	public ComboManager ComboManager;

	public const int PlayWidth = 6;
	public const int PlayHeight = 45;
	public const int SafeHeight = 13;
	public const int GridSize = PlayWidth * PlayHeight;
	public const int MinimumPatternLength = 3;

	GridElement[,] grid = new GridElement[Grid.PlayWidth, Grid.PlayHeight];
	CheckRegistryElement[] checkRegistry = new CheckRegistryElement[BlockManager.BlockStoreSize];
	int checkCount;
	int topOccupiedRow;
	int topEffectiveRow;

	// Use this for initialization
	void Start () 
	{
		for(int x = 0; x < PlayWidth; x++)
		{
			for(int y = 0; y < PlayHeight; y++)
			{
				grid[x, y] = new GridElement();
				grid[x, y].State = GridElement.ElementState.Empty;
				grid[x, y].Type = GridElement.ElementType.Empty;
				grid[x, y].Element = null;
			}
		}

		for(int n = 0; n < BlockManager.BlockStoreSize; n++)
		{
			checkRegistry[n] = new CheckRegistryElement();
		}

		int shortColumn = Random.Range(0, PlayWidth);

		for(int x = PlayWidth - 1; x >= 0; x--)
		{
			int height = (shortColumn == x ? 2 : 7) + Random.Range(0, 2);

			if(height - 1 > topOccupiedRow)
				topOccupiedRow = height - 1;

			for(int y = height - 1; y >= 1; y--)
			{
				int flavor;
				do
				{
					flavor = Random.Range(0, Block.FlavorCount);

					if(!(StateAt(x, y + 1) == GridElement.ElementState.Empty) &&
					   BlockAt(x, y + 1).Flavor == flavor)
						continue;

					if(x == Grid.PlayWidth - 1)
						break;

					if(!(StateAt(x + 1, y) == GridElement.ElementState.Empty) &&
					   BlockAt(x + 1, y).Flavor == flavor)
						continue;

					break;
				} while(true);

				// setup creep creation state
				if(y == 2)
					BlockManager.SecondToLastRowCreep[x] = flavor;
				if(y == 1)
					BlockManager.LastRowCreep[x] = flavor;

				// create the block
				BlockManager.NewBlock(x, y, flavor);
			}
		}

		topEffectiveRow = topOccupiedRow;
	}

	public GridElement.ElementState StateAt(int x, int y)
	{
		return grid[x, y].State;
	}

	public Block BlockAt(int x, int y)
	{
		DebugUtilities.Assert(grid[x, y].Type == GridElement.ElementType.Block);

		return grid[x, y].Element as Block;
	}

	public bool MatchAt(int x, int y, Block block)
	{
		DebugUtilities.Assert(grid[x, y].State == GridElement.ElementState.Block);

		return BlockManager.FlavorMatch(block, grid[x, y].Element as Block);
	}

	public void ChangeState(int x, int y, Object element, GridElement.ElementState state)
	{
		DebugUtilities.Assert(grid[x, y].Element == element);

		grid[x, y].State = state;
	}

	public void AddBlock(int x, int y, Block block, GridElement.ElementState state)
	{
		DebugUtilities.Assert(x < PlayWidth);
		DebugUtilities.Assert(y < PlayHeight);
		DebugUtilities.Assert(grid[x, y].State == GridElement.ElementState.Empty);

		grid[x, y].Element = block;
		grid[x, y].Type = GridElement.ElementType.Block;
		grid[x, y].State = state;
	}

	public void Remove(int x, int y, Block block)
	{
		DebugUtilities.Assert(grid[x, y].Element == block);

		grid[x, y].Element = null;
		grid[x, y].Type = GridElement.ElementType.Empty;
		grid[x, y].State = GridElement.ElementState.Empty;
	}

	public void RequestEliminationCheck(Block block, ComboTabulator combo = null)
	{
		checkRegistry[block.Id].Mark = true;
		checkRegistry[block.Id].Combo = combo;
		checkCount++;
	}

	public bool CheckSafeHeightViolation()
	{
		return topEffectiveRow >= SafeHeight - 1;
	}

	// Update is called once per frame
	void Update () 
	{
		// process elimination check requests

		// loop through the request registry
		for(int n = 0; checkCount > 0; n++)
		{
			if(checkRegistry[n].Mark)
			{
				checkRegistry[n].Mark = false;
				checkCount--;

				Block block = BlockManager.BlockStore[n];

				// ensure that the block is still static
				if(block.State != Block.BlockState.Static)
					continue;

				// use the block's combo, if it has one
				HandleEliminationCheckRequest(block, block.CurrentCombo != null ? block.CurrentCombo : checkRegistry[n].Combo);
			}
		}

		// update top occupied row
		topOccupiedRow++;
		bool flag = true;
		do
		{
			topOccupiedRow--;
			for(int x = 0; x < Grid.PlayWidth; x++)
			{
				if(StateAt(x, topOccupiedRow) != GridElement.ElementState.Empty)
				{
					flag = false;
					break;
				}
			}
		} while(flag);

		// update top effective row
		topEffectiveRow++;
		flag = true;
		do
		{
			topEffectiveRow--;
			for(int x = 0; x < Grid.PlayWidth; x++)
			{
				if(grid[x, topEffectiveRow].Type != GridElement.ElementType.Empty)
				{
					flag = false;
					break;
				}
			}
		} while(flag);
	}

	void HandleEliminationCheckRequest(Block block, ComboTabulator combo)
	{
		int x = block.X;
		int y = block.Y;

		// look in four directions for matching lines

		int left = x;
		while(left > 0)
		{
			if(StateAt(left - 1, y) != GridElement.ElementState.Block)
				break;
			if(!MatchAt(left - 1, y, block))
				break;
			left--;
		}

		int right = x + 1;
		while(right < PlayWidth)
		{
			if(StateAt(right, y) != GridElement.ElementState.Block)
				break;
			if(!MatchAt(right, y, block))
				break;
			right++;
		}

		int bottom = y;
		while(bottom > 1)
		{
			if(StateAt(x, bottom - 1) != GridElement.ElementState.Block)
				break;
			if(!MatchAt(x, bottom - 1, block))
				break;
			bottom--;
		}

		int top = y + 1;
		while(top < PlayHeight)
		{
			if(StateAt (x, top) != GridElement.ElementState.Block)
				break;
			if(!MatchAt(x, top, block))
				break;
			top++;
		}

		int width = right - left;
		int height = top - bottom;
		int magnitude = 0;
		bool horizontalPattern = false;
		bool verticalPattern = false;

		if(width >= MinimumPatternLength)
		{
			horizontalPattern = true;
			magnitude += width;
		}

		if(height >= MinimumPatternLength)
		{
			verticalPattern = true;
			magnitude += height;
		}

		if(!horizontalPattern && !verticalPattern)
		{
			block.EndComboInvolvement(combo);
			return;
		}

		if(combo == null)
		{
			combo = ComboManager.NewComboTabulator();
		}

		// if pattern matches both directions
		if(horizontalPattern && verticalPattern)
			magnitude--;

		// kill the pattern's blocks and look for touching garbage
		block.StartDying(combo, magnitude);

		if(horizontalPattern)
		{
			// kill the pattern's blocks
			for(int killX = left; killX < right; killX++)
			{
				if(killX != x)
				{
					BlockAt(killX, y).StartDying(combo, magnitude);
				}
			}
		}

		if(verticalPattern)
		{
			// kill the pattern's blocks
			for(int killY = bottom; killY < top; killY++)
			{
				if(killY != y)
				{
					BlockAt(x, killY).StartDying(combo, magnitude);
				}
			}
		}

		combo.ReportElimination(magnitude, block);
	}

	public bool ShiftUp()
	{
		if(topOccupiedRow == PlayHeight - 1)
			return false;

		// shift the grid
		for(int y = topOccupiedRow + 1; y >= 0; y--)
		{
			for(int x = 0; x < PlayWidth; x++)
			{
				grid[x, y + 1].Element = grid[x, y].Element;
				grid[x, y + 1].Type = grid[x, y].Type;
				grid[x, y + 1].State = grid[x, y].State;
			}
		}

		// otherwise the assert will tag us
		for(int x = 0; x < PlayWidth; x++)
		{
			grid[x, 0].State = GridElement.ElementState.Empty;
		}

		topOccupiedRow++;
		topEffectiveRow++;

		BlockManager.ShiftUp();

		return true;
	}
}
