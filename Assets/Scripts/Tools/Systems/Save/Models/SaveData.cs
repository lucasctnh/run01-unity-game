[System.Serializable]
public class SaveData {
	public int bestScore;
	public int coins;

	public SaveData(int bestScore, int coins) {
		this.bestScore = bestScore;
		this.coins = coins;
	}
}
