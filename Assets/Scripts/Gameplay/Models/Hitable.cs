using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hitable : MonoBehaviour {
	[Tooltip("The percentage chance of actually spawning that hitable")]
	[Range(0f, 1f)]
	public float spawnRate = 1f;

	[Tooltip("This custom speed will be added to the base speed")]
	[SerializeField] private float _customSpeedMultiplier = 0f;

	protected Action<Hitable> _killAction;

	private void FixedUpdate() => transform.Translate(Vector3.left * (GameManager.Instance.playerSpeed + _customSpeedMultiplier)
		* Time.fixedDeltaTime, Space.World);

	protected virtual void OnCollisionEnter(Collision other) {}

	protected virtual void OnTriggerEnter(Collider other) {}

	public abstract Vector3 GenerateRandomPosition(float horizontalPosition);

	public void SetKill(Action<Hitable> killAction) => _killAction = killAction;
}
