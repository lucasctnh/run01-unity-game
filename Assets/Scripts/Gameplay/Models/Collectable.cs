using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Collectable : Hitable {
	[Tooltip("The minimum and maximum vertical positions of the area where the collectable can spawn")]
	[SerializeField] private Vector2 _posYMinMax;

	protected override void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Hitable Destroyer"))
			_killAction(this);
		if (other.gameObject.CompareTag("Player")) {
			AudioManager.Instance.PlaySoundOneShot(Sound.Type.CoinPickUp, 2);
			GameManager.Instance.IncreaseCoin();
			_killAction(this);
		}
	}

	public override Vector3 GenerateRandomPosition(float horizontalPosition) {
		float posY = Random.Range(_posYMinMax.x, _posYMinMax.y);
		while (posY < 2 && posY > -2) // exclude bridge thickness
			posY = Random.Range(_posYMinMax.x, _posYMinMax.y);

		return new Vector3(horizontalPosition, posY, transform.position.z);
	}
}
