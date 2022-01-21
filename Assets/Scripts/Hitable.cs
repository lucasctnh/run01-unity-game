using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hitable : MonoBehaviour {
	public static float moveLeftSpeed;

	protected Action<Hitable> _killAction;

	[Tooltip("The initial speed which all hitable objects will continuously move towards the player")]
	[SerializeField] private float _initialMoveLeftSpeed = 15f;

	private void Start() => moveLeftSpeed = _initialMoveLeftSpeed;

	private void FixedUpdate() => transform.Translate(Vector3.left * moveLeftSpeed * Time.deltaTime, Space.World);

	protected abstract void OnTriggerEnter(Collider other);

	public void SetKill(Action<Hitable> killAction) => _killAction = killAction;

	public abstract Vector3 CreateRandomPosition();
}
