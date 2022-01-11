using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
	[Tooltip("A list of every obstacle prefab to be randonly spawned")]
	[SerializeField] private List<GameObject> _obstacles = new List<GameObject>();

	[Tooltip("A delay time in seconds to wait before start spawning obstacles")]
	[SerializeField] private float _startDelay = 2;

	[Tooltip("The time in seconds in which the spawn will keep repeating")]
	[SerializeField] private float _repeatRate = 2;

	private void Start() {
		InvokeRepeating("SpawnObstacle", _startDelay, _repeatRate);
	}

	private void SpawnObstacle() {
		int random = Random.Range(0, _obstacles.Count);
		Vector3 spawnPosition = new Vector3(transform.position.x, _obstacles[random].transform.position.y, _obstacles[random].transform.position.z);
        Instantiate(_obstacles[random], spawnPosition, _obstacles[random].transform.rotation);
	}
}
