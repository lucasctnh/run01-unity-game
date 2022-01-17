using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hitable : MonoBehaviour {
	[Tooltip("The velocity which the obstacle will continuously move towards the player")]
	[SerializeField] protected float _moveLeftVelocity = 20f;

	protected Action<Hitable> _killAction;

	private void Update() => transform.Translate(Vector3.left * _moveLeftVelocity * Time.deltaTime, Space.World);

	protected abstract void OnCollisionEnter(Collision other);

	public void SetKill(Action<Hitable> killAction) => _killAction = killAction;

	public abstract Vector3 CreateRandomPosition();
}
