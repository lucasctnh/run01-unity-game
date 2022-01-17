using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {
	[SerializeField] private Transform _camera;

	private Vector3 _position;

	private void Start() {
		_position = transform.position;
	}

	private void Update() {
		_position.y = _camera.position.y;
		transform.position = _position;
	}
}
