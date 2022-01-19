using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hitable : MonoBehaviour {
	[Tooltip("The velocity which the obstacle will continuously move towards the player")]
	public float moveLeftVelocity = 20f;

	protected Action<Hitable> _killAction;

	private void Update() => transform.Translate(Vector3.left * moveLeftVelocity * Time.deltaTime, Space.World);

	protected abstract void OnTriggerEnter(Collider other);

	public void SetKill(Action<Hitable> killAction) => _killAction = killAction;

	public abstract Vector3 CreateRandomPosition();
}
