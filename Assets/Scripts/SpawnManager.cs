using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SpawnManager : MonoBehaviour {
	[Tooltip("A list of every obstacle prefab to be randonly spawned")]
	[SerializeField] private List<Obstacle> _obstacles = new List<Obstacle>();

	[Tooltip("A delay time in seconds to wait before start spawning obstacles")]
	[SerializeField] private float _startDelay = 2;

	[Tooltip("The time in seconds in which the spawn will keep repeating")]
	[SerializeField] private float _repeatRate = 2;

	private ObjectPool<Obstacle> _pool;

	private void Start() {
		_pool = new ObjectPool<Obstacle>(CreateObstacle, OnGetObstacleFromPool, OnReleaseObstacleToPool,
		obst => Destroy(obst.gameObject), true, 6, 12);

		InvokeRepeating("SpawnObstacle", _startDelay, _repeatRate);
	}

	private Obstacle CreateObstacle() {
		int random = CreateRandomNumber();
        return Instantiate(_obstacles[random], CreateRandomPosition(random), _obstacles[random].transform.rotation);
	}

	private int CreateRandomNumber() {
		return Random.Range(0, _obstacles.Count);
	}

	private Vector3 CreateRandomPosition(int random) {
		return new Vector3(transform.position.x, _obstacles[random].transform.position.y, _obstacles[random].transform.position.z);
	}

	private void OnGetObstacleFromPool(Obstacle obstacle) => obstacle.gameObject.SetActive(true);

	private void OnReleaseObstacleToPool(Obstacle obstacle) {
		obstacle.transform.position = ResetPosition(obstacle);
		obstacle.gameObject.SetActive(false);
	}

	private Vector3 ResetPosition(Obstacle obstacle) {
		return new Vector3(transform.position.x, obstacle.transform.position.y, obstacle.transform.position.z);
	}

	private void SpawnObstacle() {
		Obstacle obstacle = _pool.Get();
		obstacle.GetComponent<Obstacle>().SetKill(Kill);
	}

	private void Kill(Obstacle obstacle) => _pool.Release(obstacle);
}
