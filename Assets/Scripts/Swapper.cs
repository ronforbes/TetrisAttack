using UnityEngine;
using System.Collections;

public class Swapper : MonoBehaviour 
{
	public enum SwapDirection
	{
		None,
		Left,
		Right
	}
	
	public float SwapElapsed;
	public float SwapDuration = 0.1f;
	public Grid Grid;

	bool swapping;
	Block selectedBlock;
	Block leftBlock;
	Block rightBlock;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit))
			{
				if(hit.collider.name == "Cube")
				{
					Block block = hit.collider.gameObject.transform.parent.gameObject.GetComponent<Block>();
					if(Grid.StateAt(block.X, block.Y) == GridElement.ElementState.Block)
					{
						selectedBlock = block;
					}
				}
			}
		}
		
		if(selectedBlock != null && Grid.StateAt(selectedBlock.X, selectedBlock.Y) == GridElement.ElementState.Block)
		{
			float leftEdge = selectedBlock.transform.position.x - selectedBlock.transform.localScale.x / 2;
			float rightEdge = selectedBlock.transform.position.x + selectedBlock.transform.localScale.x / 2;
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = Camera.main.transform.position.z * -1;

			if(Camera.main.ScreenToWorldPoint(mousePosition).x < leftEdge)
			{
				if(selectedBlock.X - 1 >= 0)
				{
					if(Grid.StateAt(selectedBlock.X, selectedBlock.Y) == GridElement.ElementState.Block)
					{
						if(Grid.BlockAt(selectedBlock.X, selectedBlock.Y) != null)
						{
							rightBlock = Grid.BlockAt(selectedBlock.X, selectedBlock.Y);
						}
					}

					if(Grid.StateAt(selectedBlock.X - 1, selectedBlock.Y) == GridElement.ElementState.Block)
					{
						if(Grid.BlockAt(selectedBlock.X - 1, selectedBlock.Y) != null)
						{
							leftBlock = Grid.BlockAt(selectedBlock.X - 1, selectedBlock.Y);
						}
					}
				}
			}
			
			if(Camera.main.ScreenToWorldPoint(mousePosition).x > rightEdge)
			{
				if(selectedBlock.X + 1 < Grid.PlayWidth)
				{
					if(Grid.StateAt(selectedBlock.X, selectedBlock.Y) == GridElement.ElementState.Block)
					{
						if(Grid.BlockAt(selectedBlock.X, selectedBlock.Y) != null)
						{
							leftBlock = Grid.BlockAt(selectedBlock.X, selectedBlock.Y);
						}
					}

					if(Grid.StateAt(selectedBlock.X + 1, selectedBlock.Y) == GridElement.ElementState.Block)
					{
						if(Grid.BlockAt(selectedBlock.X + 1, selectedBlock.Y) != null)
						{
							rightBlock = Grid.BlockAt(selectedBlock.X + 1, selectedBlock.Y);
						}
					}
				}
			}

			if(leftBlock)
				leftBlock.StartSwapping(SwapDirection.Right, leftBlock == Grid.BlockAt(selectedBlock.X, selectedBlock.Y));
			if(rightBlock)
				rightBlock.StartSwapping(SwapDirection.Left, rightBlock == Grid.BlockAt(selectedBlock.X, selectedBlock.Y));
			if(leftBlock || rightBlock)
			{
				swapping = true;
				SwapElapsed = 0.0f;
			}
		}
		
		if(Input.GetMouseButtonUp(0))
		{
			selectedBlock = null;
		}

		if(swapping)
		{
			SwapElapsed += Time.deltaTime;

			if(SwapElapsed >= SwapDuration)
			{
				swapping = false;

				if(leftBlock)
					Grid.Remove(leftBlock.X, leftBlock.Y, leftBlock);
				if(rightBlock)
					Grid.Remove(rightBlock.X, rightBlock.Y, rightBlock);

				if(leftBlock)
					leftBlock.FinishSwapping(leftBlock.X + 1);
				if(rightBlock)
					rightBlock.FinishSwapping(rightBlock.X - 1);

				if(leftBlock)
				{
					Grid.RequestEliminationCheck(leftBlock);
				}
				if(rightBlock)
				{
					Grid.RequestEliminationCheck(rightBlock);
				}

				leftBlock = null;
				rightBlock = null;
			}
		}
	}
}
