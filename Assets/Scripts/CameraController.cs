using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[Tooltip("The amount of smootheness for the camera movement")]
	[SerializeField] private float _smoothTime = .3f;

	private Vector3 _playerPosition = Vector3.zero;
	private Vector3 _targetPosition = Vector3.zero;
	private Vector3 _velocity = Vector3.zero;
	private int _currentDirection = 1;

	private void OnEnable() {
		// InputsController.OnTouchInput += Jump;
		InputsController.OnMove += FollowPlayer;
	}

	private void OnDisable() {
		// InputsController.OnTouchInput -= Jump;
		InputsController.OnMove -= FollowPlayer;
	}

	private void Start() {
		_playerPosition = transform.position;
		_targetPosition = transform.position;
	}

	private void Update() {
		transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, _smoothTime);
	}

	private void FollowPlayer(float yDrag) {
		int newDirection = (yDrag > 0) ? 1 : -1;
		if (newDirection != _currentDirection && PlayerController.isGrounded) {
			_targetPosition = InvertPlayerPosition();
			AssignNewDirection(newDirection);
		}
	}

	private void AssignNewDirection(int newDirection) {
		_currentDirection = newDirection;
	}

	private Vector3 InvertPlayerPosition() {
		_playerPosition = new Vector3(_playerPosition.x, _playerPosition.y * -1, _playerPosition.z);
		return _playerPosition;
	}
}
