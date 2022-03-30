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

	private void OnEnable() {
		PlayerController.OnInvertedPosition += InvertCamera;
		PlayerController.OnShouldResetCamera += ResetCamera;
	}

	private void OnDisable() {
		PlayerController.OnInvertedPosition -= InvertCamera;
		PlayerController.OnShouldResetCamera -= ResetCamera;
	}

	private void Start() => ResetTarget();

	private void Update() {
		if (GameManager.Instance.isGameRunning)
			transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, _smoothTime);
	}

	public void InvertCamera() => _targetPosition = InvertPosition();

	private Vector3 InvertPosition() {
		_targetPosition = new Vector3(_targetPosition.x, _targetPosition.y * -1, _targetPosition.z);
		return _targetPosition;
	}

	private void ResetTarget() => _targetPosition = _defaultPosition;

	private void ResetCamera() {
		transform.position = _defaultPosition;
		ResetTarget();
	}
}
