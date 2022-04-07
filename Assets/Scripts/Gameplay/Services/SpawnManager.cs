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
	[SerializeField] private float _startDelay = 4;

	[Tooltip("The time in seconds in which the spawn will keep repeating")]
	public float repeatRate = 3;

	private List<ObjectPool<Hitable>> _pools = new List<ObjectPool<Hitable>>();
	private float _timer = 0f;
	private bool _isSelectingPool = true;

	private void Start() {
		foreach (Hitable hitablePrefab in _hitablePrefabs)
			_pools.Add(new ObjectPool<Hitable>(() => CreateHitable(hitablePrefab), OnGetHitableFromPool, OnReleaseHitableToPool,
				hitable => Destroy(hitable.gameObject), true, 6, 12));

		_timer = _startDelay;
	}

	private void Update() {
		if (GameManager.Instance.isGameRunning)
			GameManager.CallRepeating(SpawnHitable, ref _timer, repeatRate);
	}

	private Hitable CreateHitable(Hitable hitablePrefab) {
		return Instantiate(hitablePrefab, GenerateRandomPosition(hitablePrefab), hitablePrefab.transform.rotation);
	}

	private Vector3 GenerateRandomPosition(Hitable hitable) {
		return hitable.GenerateRandomPosition(transform.position.x);
	}

	private void OnGetHitableFromPool(Hitable hitable) {
		hitable.isReleased = false;
		hitable.gameObject.SetActive(true);
	}

	private void OnReleaseHitableToPool(Hitable hitable) {
		hitable.isReleased = true;
		hitable.transform.position = GenerateRandomPosition(hitable);
		hitable.gameObject.SetActive(false);
	}

	private void SpawnHitable() {
		while (_isSelectingPool) {
			ObjectPool<Hitable> pool = GetRandomPool();
			Hitable hitable = pool.Get();
			hitable.GetComponent<Hitable>().SetKill(hitable => Kill(pool, hitable));

			SetHitableVolume(hitable);

			if (!CheckShouldSpawn(hitable))
				pool.Release(hitable);
		}

		_isSelectingPool = true;
	}

	private void SetHitableVolume(Hitable hitable) {
		AudioSource[] audioSources = hitable.GetComponents<AudioSource>();

		if (audioSources != null && audioSources.Length != 0) {
			foreach (AudioSource audioSource in audioSources) {
				float obstacleVolume = AudioManager.Instance.GetTrackVolume(2) == 0 ? 0 : AudioManager.Instance.GetTrackVolume(2) + .2f;
				audioSource.volume = obstacleVolume;
			}
		}
	}

	private bool CheckShouldSpawn(Hitable hitable) {
		if (hitable.SpawnRate <= 0f)
			return false;
		else if (Random.Range(0f, 1f) <= hitable.SpawnRate) {
			_isSelectingPool = false;
			return true;
		}
		else
			return false;
	}

	private ObjectPool<Hitable> GetRandomPool() {
		return _pools[Random.Range(0, _pools.Count)];
	}

	private void Kill(ObjectPool<Hitable> pool, Hitable hitable) => pool.Release(hitable);
}
