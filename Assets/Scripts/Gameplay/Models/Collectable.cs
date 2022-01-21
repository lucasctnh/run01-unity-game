using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Collectable : Hitable {

	[Tooltip("The minimum positions of the area where the collectable can spawn")]
	[SerializeField] private Vector2 _min;

	[Tooltip("The maximum positions of the area where the collectable can spawn")]
	[SerializeField] private Vector2 _max;

	protected override void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			_killAction(this);
			GameManager.Instance.IncreaseCoin();
		}
	}

	public override Vector3 CreateRandomPosition() {
		return new Vector3(Random.Range(_min.x, _max.x), Random.Range(_min.y, _max.y), transform.position.z);
	}
}
