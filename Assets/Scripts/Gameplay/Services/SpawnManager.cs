using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour {
	[Tooltip("The hitable prefab")]
	[SerializeField] private Hitable[] _hitablePrefabs;

	[Tooltip("A delay time in seconds to wait before start spawning collectables")]
	[SerializeField] private float _startDelay = 2;

	[Tooltip("The time in seconds in which the spawn will keep repeating")]
	[SerializeField] private float _repeatRate = 2;

	private List<ObjectPool<Hitable>> _pools = new List<ObjectPool<Hitable>>();
	private float _timer = 0f;

	private void Start() {
		foreach (Hitable hitablePrefab in _hitablePrefabs)
			_pools.Add(new ObjectPool<Hitable>(() => CreateHitable(hitablePrefab), OnGetHitableFromPool, OnReleaseHitableToPool,
				hitable => Destroy(hitable.gameObject), true, 6, 12));

		_timer = _startDelay;
	}

	private void Update() {
		if (GameManager.Instance.isGameRunning)
			GameManager.CallRepeating(SpawnHitable, ref _timer, _repeatRate);
	}

	private Hitable CreateHitable(Hitable hitablePrefab) {
		return Instantiate(hitablePrefab, GenerateRandomPosition(hitablePrefab), hitablePrefab.transform.rotation);
	}

	private Vector3 GenerateRandomPosition(Hitable hitable) {
		return hitable.GenerateRandomPosition(transform.position.x);
	}

	private void OnGetHitableFromPool(Hitable hitable) => hitable.gameObject.SetActive(true);

	private void OnReleaseHitableToPool(Hitable hitable) {
		hitable.transform.position = GenerateRandomPosition(hitable);
		hitable.gameObject.SetActive(false);
	}

	private void SpawnHitable() {
		ObjectPool<Hitable> pool = GetRandomPool();
		Hitable hitable = pool.Get();
		hitable.GetComponent<Hitable>().SetKill(hitable => Kill(pool, hitable));
	}

	private ObjectPool<Hitable> GetRandomPool() {
		return _pools[Random.Range(0, _pools.Count)];
	}

	private void Kill(ObjectPool<Hitable> pool, Hitable hitable) => pool.Release(hitable);
}
