using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[Tooltip("The amount of smootheness for the camera movement")]
	[SerializeField] private float _smoothTime = .3f;

	[Tooltip("The default position for when the game starts running")]
	[SerializeField] private Vector3 _defaultPosition;

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

	private void Start() => _targetPosition = _defaultPosition;

	private void Update() {
		if (GameManager.Instance.isGameRunning)
			transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, _smoothTime);
	}

	public void InvertCamera() => _targetPosition = InvertDefaultPosition();

	private void FollowPlayer(float yDrag) {
		int newDirection = (yDrag > 0) ? 1 : -1;
		if (newDirection != _currentDirection && PlayerController.IsGrounded) {
			_targetPosition = InvertDefaultPosition();
			AssignNewDirection(newDirection);
		}
	}

	private void AssignNewDirection(int newDirection) => _currentDirection = newDirection;

	private Vector3 InvertDefaultPosition() {
		_defaultPosition = new Vector3(_defaultPosition.x, _defaultPosition.y * -1, _defaultPosition.z);
		return _defaultPosition;
	}
}
