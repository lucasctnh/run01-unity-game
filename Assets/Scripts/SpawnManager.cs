using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour {
	[Tooltip("The obstacle prefab")]
	[SerializeField] private Obstacle _obstaclePrefab;

	[Tooltip("A list of the possible y-axis positions for the obstacles")]
	[SerializeField] private List<float> _verticalObstaclePositions = new List<float>();

	[Tooltip("A delay time in seconds to wait before start spawning obstacles")]
	[SerializeField] private float _startDelay = 2;

	[Tooltip("The time in seconds in which the spawn will keep repeating")]
	[SerializeField] private float _repeatRate = 2;

	private ObjectPool<Obstacle> _pool;
	private float _timer = 0f;

	private void Start() {
		_pool = new ObjectPool<Obstacle>(CreateObstacle, OnGetObstacleFromPool, OnReleaseObstacleToPool,
		obst => Destroy(obst.gameObject), true, 6, 12);

		_timer = _startDelay;
	}

	private void Update() {
		if (GameManager.Instance.isGameRunning)
			CallRepeating(SpawnObstacle);
	}

	private void CallRepeating(Action action) {
		_timer -= Time.deltaTime;
		if (_timer < 0) {
			_timer = _repeatRate;
			action();
		}
	}

	private Obstacle CreateObstacle() {
		int random = CreateRandomNumber();
		return Instantiate(_obstaclePrefab, CreateRandomPosition(random), _obstaclePrefab.transform.rotation);
	}

	private int CreateRandomNumber() {
		return Random.Range(0, _verticalObstaclePositions.Count);
	}

	private Vector3 CreateRandomPosition(int random) {
		return new Vector3(transform.position.x, _verticalObstaclePositions[random], _obstaclePrefab.transform.position.z);
	}

	private void OnGetObstacleFromPool(Obstacle obstacle) => obstacle.gameObject.SetActive(true);

	private void OnReleaseObstacleToPool(Obstacle obstacle) {
		obstacle.transform.position = CreateRandomPosition(CreateRandomNumber());
		obstacle.gameObject.SetActive(false);
	}

	private void SpawnObstacle() {
		Obstacle obstacle = _pool.Get();
		obstacle.GetComponent<Obstacle>().SetKill(Kill);
	}

	private void Kill(Obstacle obstacle) => _pool.Release(obstacle);
}
