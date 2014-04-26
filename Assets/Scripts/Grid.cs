using UnityEngine;
using System.Collections;

public class GridElement
{
	public enum ElementState
	{
		Empty,
		Block,
		Immutable
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

public class Grid : MonoBehaviour 
{
	public BlockManager BlockManager;

	public const int PlayWidth = 6;
	public const int PlayHeight = 45;
	public const int SafeHeight = 13;
	public const int GridSize = PlayWidth * PlayHeight;

	static GridElement[,] grid = new GridElement[Grid.PlayWidth, Grid.PlayHeight];

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

		int shortColumn = Random.Range(0, PlayWidth);

		for(int x = PlayWidth - 1; x >= 0; x--)
		{
			int height = (shortColumn == x ? 2 : 7) + Random.Range(0, 2);

			for(int y = height - 1; y >= 0; y--)
			{
				int flavor;
				do
				{
					flavor = Random.Range(0, Block.FlavorCount);

					if(!(Grid.StateAt(x, y + 1) == GridElement.ElementState.Empty) &&
					   Grid.BlockAt(x, y + 1).Flavor == flavor)
						continue;

					if(x == Grid.PlayWidth - 1)
						break;

					if(!(Grid.StateAt(x + 1, y) == GridElement.ElementState.Empty) &&
					   Grid.BlockAt(x + 1, y).Flavor == flavor)
						continue;

					break;
				} while(true);

				BlockManager.NewBlock(x, y, flavor);
			}
		}
	}

	public static GridElement.ElementState StateAt(int x, int y)
	{
		return grid[x, y].State;
	}

	public static Block BlockAt(int x, int y)
	{
		DebugUtilities.Assert(grid[x, y].Type == GridElement.ElementType.Block);

		return grid[x, y].Element as Block;
	}

	public static void ChangeState(int x, int y, Object element, GridElement.ElementState state)
	{
		DebugUtilities.Assert(grid[x, y].Element == element);

		grid[x, y].State = state;
	}

	public static void AddBlock(int x, int y, Block block, GridElement.ElementState state)
	{
		DebugUtilities.Assert(x < PlayWidth);
		DebugUtilities.Assert(y < PlayHeight);
		DebugUtilities.Assert(grid[x, y].State == GridElement.ElementState.Empty);

		grid[x, y].Element = block;
		grid[x, y].Type = GridElement.ElementType.Block;
		grid[x, y].State = state;
	}

	public static void Remove(int x, int y, Block block)
	{
		DebugUtilities.Assert(grid[x, y].Element == block);

		grid[x, y].Element = null;
		grid[x, y].Type = GridElement.ElementType.Empty;
		grid[x, y].State = GridElement.ElementState.Empty;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
