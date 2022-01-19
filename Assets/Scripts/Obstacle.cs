using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Obstacle : Hitable {
	[Tooltip("The x-axis position for the obstacles spawn")]
	[SerializeField] private float _horizontalPosition;


	[Tooltip("A list of the possible y-axis positions for the obstacles")]
	[SerializeField] private List<float> _verticalPositions = new List<float>();

	protected override void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player"))
			GameManager.Instance.GameOver();
		if (other.gameObject.CompareTag("Obstacle Destroyer"))
			_killAction(this);
	}

	private int CreateRandomNumber() {
		return Random.Range(0, _verticalPositions.Count);
	}

	public override Vector3 CreateRandomPosition() {
		return new Vector3(_horizontalPosition, _verticalPositions[CreateRandomNumber()], transform.position.z);
	}
}
