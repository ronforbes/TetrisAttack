using UnityEngine;
using System.Collections;

public class ComboManager : MonoBehaviour 
{
	public Game Game;

	ComboTabulator[] tabulatorStore = new ComboTabulator[comboTabulatorStoreSize];
	bool[] storeMap = new bool[comboTabulatorStoreSize];
	int comboCount;

	const int comboTabulatorStoreSize = 8;

	// Use this for initialization
	void Start () 
	{
		for(int n = 0; n < comboTabulatorStoreSize; n++)
		{
			storeMap[n] = false;
			tabulatorStore[n] = new ComboTabulator();
			tabulatorStore[n].Id = n;
		}
	}

	public ComboTabulator NewComboTabulator()
	{
		int id = FindFreeId();
		AllocateId(id);
		ComboTabulator combo = tabulatorStore[id];

		combo.Initialize();
		return combo;
	}

	public void DeleteComboTabulator(ComboTabulator combo)
	{
		FreeId(combo.Id);
	}

	int FindFreeId()
	{
		int n;
		for(n = 0; n < comboTabulatorStoreSize; n++)
		{
			if(storeMap[n] == false)
				break;
		}

		return n;
	}

	void AllocateId(int id)
	{
		DebugUtilities.Assert(!storeMap[id]);
		storeMap[id] = true;
		comboCount++;
	}

	void FreeId(int id)
	{
		DebugUtilities.Assert(storeMap[id]);
		storeMap[id] = false;
		comboCount--;
	}

	// Update is called once per frame
	void Update () 
	{
		int c = comboCount;
		for(int n = 0; c > 0; n++)
		{
			if(!storeMap[n])
				continue;

			ComboTabulator combo = tabulatorStore[n];
			c--;

			if(combo.InvolvementCount == 0)
			{
				FreeId(n);
			}
			else
			{
				if(combo.EliminationJustOccurred)
				{
					// notify the score
					Game.Score++;

					combo.EliminationJustOccurred = false;
				}
			}
		}
	}
}
