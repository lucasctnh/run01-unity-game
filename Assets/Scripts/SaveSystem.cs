using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem {
	private static string _path = Application.persistentDataPath + "/save.lrn";

	public static void Save(int score, int coins) {
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(_path, FileMode.Create);

		SaveData data = new SaveData(score, coins);

		formatter.Serialize(stream, data);
		stream.Close();
	}

	public static SaveData Load() {
		if (File.Exists(_path)) {
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(_path, FileMode.Open);

			SaveData data = formatter.Deserialize(stream) as SaveData;
			stream.Close();

			return data;
		} else {
			Debug.LogError("Save file not found in " + _path);
			return null;
		}
	}
}
