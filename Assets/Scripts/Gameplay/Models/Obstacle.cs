using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Obstacle : Hitable {
	[Tooltip("The minimum angle threshold which colliding with obstacles will result in gameover")]
	[SerializeField] private float _angleThreshold = 40f;

	protected override void OnCollisionEnter(Collision other) {
		if ((other.gameObject.CompareTag("Hitable Destroyer") || other.gameObject.CompareTag("Collapse Destroyer")) && !isReleased)
			_killAction(this);
		if (other.gameObject.CompareTag("Player")) {
			for (int i = 0; i < other.contacts.Length; i++) {
				float currentAngleRefUp = Vector3.Angle(other.contacts[i].normal, Vector3.up);
				float currentAngleRefDown = Vector3.Angle(other.contacts[i].normal, Vector3.down);

				if (currentAngleRefUp <= _angleThreshold || currentAngleRefDown <= _angleThreshold) {
					other.gameObject.GetComponent<PlayerController>().RechargeJumps();
					break;
				}
				else
					GameManager.Instance.GameOver(Sound.Type.Death);
			}
		}
	}

	public override Vector3 GenerateRandomPosition(float horizontalPosition) {
		return new Vector3(horizontalPosition, transform.position.y, transform.position.z);
	}
}
