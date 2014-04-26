using UnityEngine;
using System.Collections;

public class Displayer : MonoBehaviour {
	public BlockManager BlockManager;
	public Swapper Swapper;

	Color[] blockColors = new Color[Block.FlavorCount];

	const float gridElementLength = 1.0f;

	// Use this for initialization
	void Start () {
		blockColors[0] = new Color(0.73f, 0.0f,  0.73f);
		blockColors[1] = new Color(0.2f,  0.2f,  0.8f);
		blockColors[2] = new Color(0.0f,  0.6f,  0.05f);
		blockColors[3] = new Color(0.85f, 0.85f, 0.0f);
		blockColors[4] = new Color(1.0f,  0.4f,  0.0f);
	}
	
	// Update is called once per frame
	void Update () {
		DrawBlocks();
	}

	void DrawBlocks()
	{
		int c = BlockManager.BlockCount;
		for(int n = 0; c > 0; n++)
		{
			if(BlockManager.StoreMap[n])
			{
				Block block = BlockManager.BlockStore[n];
				c--;

				if(block.Y > Grid.SafeHeight)
					continue;

				DrawBlock(block);
			}
		}
	}

	void DrawBlock(Block block)
	{
		float x, y;
		float dX = 0.0f;

		x = block.X * gridElementLength;
		y = block.Y * gridElementLength;

		switch(block.State)
		{
		case Block.BlockState.Static:
			block.transform.Find("Cube").renderer.material.color = blockColors[block.Flavor];
			
			block.transform.position = new Vector3(x, y, 0.0f);
			block.transform.rotation = Quaternion.identity;
			break;

		case Block.BlockState.Swapping:
			if(block.Direction == Swapper.SwapDirection.Left)
			{
				dX = -gridElementLength / 2.0f;
			}

			if(block.Direction == Swapper.SwapDirection.Right)
			{
				dX = gridElementLength / 2.0f;
			}

			block.transform.Find("Cube").renderer.material.color = blockColors[block.Flavor];

			//block.transform.position = Vector3.Slerp(block.transform.position)

			Vector3 center = new Vector3(x + dX, y, 0.0f);
			if(block.SwapFront)
			{
				center -= new Vector3(0, 0, -0.01f);
			}
			else
			{
				center -= new Vector3(0, 0, 0.01f);
			}

			Vector3 fromRelCenter = new Vector3(x, y, 0.0f) - center;
			Vector3 toRelCenter = new Vector3(x + dX * 2.0f, y, 0.0f) - center;
			float time = Swapper.SwapElapsed / Swapper.SwapDuration;
			block.transform.position = Vector3.Slerp(fromRelCenter, toRelCenter, time);
			block.transform.position += center;

			break;
		}
	}
}
