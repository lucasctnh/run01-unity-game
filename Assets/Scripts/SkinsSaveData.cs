using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkinsSaveData {
	public bool[] skinsUnlocked;

	public SkinsSaveData(bool[] skinsUnlocked) => this.skinsUnlocked = skinsUnlocked;
}
