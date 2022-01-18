using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData {
	public int bestScore;
	public int coins;

	public SaveData(int bestScore, int coins) {
		this.bestScore = bestScore;
		this.coins = coins;
	}
}
