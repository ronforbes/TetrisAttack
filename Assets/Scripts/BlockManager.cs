using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour {
	public Block BlockPrefab;

	public int BlockCount;
	public Block[] BlockStore = new Block[BlockStoreSize];
	public bool[] StoreMap = new bool[BlockStoreSize];
	public int[] LastRowCreep = new int[Grid.PlayWidth];
	public int[] SecondToLastRowCreep = new int[Grid.PlayWidth];

	public const int BlockStoreSize = Grid.GridSize;

	int lastFlavorCreep, secondToLastFlavorCreep;
	
	// Use this for initialization
	void Start () 
	{
		lastFlavorCreep = secondToLastFlavorCreep = 0;

		LastRowCreep = new int[Grid.PlayWidth];
		SecondToLastRowCreep = new int[Grid.PlayWidth];

		for(int x = 0; x < Grid.PlayWidth; x++)
		{
			LastRowCreep[x] = 0;
			SecondToLastRowCreep[x] = 0;
		}
	}

	public void NewBlock(int x, int y, int flavor)
	{
		if(BlockCount == BlockStoreSize)
			return;

		int id = FindFreeId();
		AllocateId(id);
		Block block = BlockStore[id];

		block.InitializeStatic(x, y, flavor);
	}

	public void NewBlock(int x, int y, int flavor, float popDuration, float awakenDuration, ComboTabulator combo, int popColor)
	{
		if(BlockCount == BlockStoreSize)
			return;

		int id = FindFreeId();
		AllocateId(id);
		Block block = BlockStore[id];

		block.InitializeAwakening(x, y, flavor, popDuration, awakenDuration, combo, popColor);
	}

	public void NewCreepRow()
	{
		for(int x = 0; x < Grid.PlayWidth; x++)
		{
			NewCreepBlock(x);
		}
	}

	public void NewCreepBlock(int x)
	{
		int flavor = 0;

		if(LastRowCreep.Length == 0)
			LastRowCreep = new int[Grid.PlayWidth];
		if(SecondToLastRowCreep.Length == 0)
			SecondToLastRowCreep = new int[Grid.PlayWidth];

		do
		{
			flavor = Random.Range(0, Block.FlavorCount);
		} while((flavor == lastFlavorCreep && lastFlavorCreep == secondToLastFlavorCreep) ||
			(flavor == LastRowCreep[x] && LastRowCreep[x] == SecondToLastRowCreep[x]));

		SecondToLastRowCreep[x] = LastRowCreep[x];
		LastRowCreep[x] = flavor;

		secondToLastFlavorCreep = lastFlavorCreep;
		lastFlavorCreep = flavor;

		NewBlock(x, 0, flavor);
	}

	// Untested
	public void NewAwakeningBlock(int x, int y, float popDuration, float awakenDuration, ComboTabulator combo, int popColor)
	{
		int flavor;

		do
		{
			flavor = Random.Range(0, Block.FlavorCount);
		} while((flavor == lastFlavorCreep && lastFlavorCreep == secondToLastFlavorCreep) ||
		        (flavor == LastRowCreep[x] && LastRowCreep[x] == SecondToLastRowCreep[x]));

		SecondToLastRowCreep[x] = LastRowCreep[x];
		LastRowCreep[x] = flavor;

		secondToLastFlavorCreep = lastFlavorCreep;
		lastFlavorCreep = flavor;

		NewBlock(x, y, flavor, popDuration, awakenDuration, combo, popColor);
	}

	public void DeleteBlock(Block block)
	{
		FreeId(block.Id);
	}

	public void ShiftUp()
	{
		for(int n = 0; n < BlockStoreSize; n++)
		{
			if(StoreMap[n])
			{
				BlockStore[n].Y++;
			}
		}
	}

	public bool FlavorMatch(Block block1, Block block2)
	{
		return block1.Flavor == block2.Flavor;
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
		BlockStore[id].Id = id;
		BlockStore[id].transform.parent = transform;

		BlockCount++;
	}

	void FreeId(int id)
	{
		DebugUtilities.Assert(StoreMap[id]);
		StoreMap[id] = false;

		Destroy(BlockStore[id].gameObject);

		BlockCount--;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
