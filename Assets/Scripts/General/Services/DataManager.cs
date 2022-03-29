using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour { // TODO: maybe put all data here?
	public static DataManager Instance;

	public bool IsFirstLose = true;

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}
}
