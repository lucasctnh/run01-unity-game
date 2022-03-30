using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hitable : MonoBehaviour {
	public static GameManager.Stages currentStage;

	public float SpawnRate { get { return _spawnRate * _spawnRateMultiplier; } }

	[HideInInspector] public bool isReleased = false;

	[Tooltip("This custom speed will be added to the base speed")]
	[SerializeField] private float _customSpeedMultiplier = 0f;

	[Tooltip("The stage where the hitable will be spawned")]
	[SerializeField] private GameManager.Stages _stage;

	[Tooltip("If the hitable has a secondary stage where will also be spawned")]
	[SerializeField] private bool _hasSecondaryStage = false;

	[Tooltip("The secondary stage where the hitable will be spawned if enabled")]
	[SerializeField] private GameManager.Stages _secondaryStage;

	[Tooltip("The percentage chance of actually spawning that hitable")]
	[Range(0f, 1f)]
	[SerializeField] private float _spawnRateMultiplier = 1f;

	protected Action<Hitable> _killAction;

	private Rigidbody _rigidbody;
	private float _spawnRate = 1f;

	private void OnEnable() => SetSpawnRate();

	private void Start() => _rigidbody = GetComponent<Rigidbody>();

	private void FixedUpdate() {
		if (GameManager.Instance.isGameRunning) {
			Vector3 direction = Vector3.left * (GameManager.Instance.playerSpeed + _customSpeedMultiplier) * Time.fixedDeltaTime;
			Vector3 position = transform.position + direction;
			_rigidbody.MovePosition(position);
		}
	}

	protected virtual void OnCollisionEnter(Collision other) { }

	protected virtual void OnTriggerEnter(Collider other) { }

	public abstract Vector3 GenerateRandomPosition(float horizontalPosition);

	public void SetKill(Action<Hitable> killAction) => _killAction = killAction;

	private void SetSpawnRate() {
		if (_stage == currentStage || (_hasSecondaryStage && _secondaryStage == currentStage))
			_spawnRate = 1f;
		else
			_spawnRate = 0f;

		if (_stage == GameManager.Stages.EveryStage)
			_spawnRate = 1f;
	}
}
