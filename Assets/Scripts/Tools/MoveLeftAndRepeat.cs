using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLeftAndRepeat : MonoBehaviour {
	[Range(0f, 1f)]
	[SerializeField] private float _customSpeed = 1f;

	private Vector3 _startPos;
	private float _repeatWidth;

	private void Start() {
        _startPos = transform.position;
		_repeatWidth = GetComponent<BoxCollider>().size.x / 2 - GetComponent<BoxCollider>().center.x;
	}

	private void FixedUpdate() {
		if (!GameManager.Instance.isGameRunning)
			return;

		transform.Translate(Vector3.left * GameManager.Instance.playerSpeed * _customSpeed * Time.fixedDeltaTime, Space.World);

		if (transform.position.x < _startPos.x - _repeatWidth)
			transform.position = _startPos;
	}
}
