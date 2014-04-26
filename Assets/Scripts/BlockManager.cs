using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour {
	public Block BlockPrefab;
	public int BlockCount;
	public Block[] BlockStore = new Block[BlockStoreSize];
	public bool[] StoreMap = new bool[BlockStoreSize];

	public const int BlockStoreSize = Grid.GridSize;

	// Use this for initialization
	void Start () {

	}

	public void NewBlock(int x, int y, int flavor)
	{
		if(BlockCount == BlockStoreSize)
			return;

		if(StoreMap.Length == 0)
			StoreMap = new bool[BlockStoreSize];

		int id = FindFreeId();
		AllocateId(id);
		Block block = BlockStore[id];

		block.InitializeStatic(x, y, flavor);
	}

	int FindFreeId()
	{
		int n;
		for(n = 0; n < BlockStoreSize; n++)
		{
			if(StoreMap[n] == false)
			{
				break;
			}
		}
		return n;
	}

	void AllocateId(int id)
	{
		DebugUtilities.Assert(!StoreMap[id]);
		StoreMap[id] = true;

		BlockStore[id] = Instantiate(BlockPrefab, Vector3.zero, Quaternion.identity) as Block;
		BlockStore[id].transform.parent = transform;

		BlockCount++;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
