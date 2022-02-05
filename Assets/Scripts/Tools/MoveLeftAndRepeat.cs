using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLeftAndRepeat : MonoBehaviour {
	[SerializeField] private bool isUI = true;

	private Vector3 _startPos;
	private float _repeatWidth;

	private void Start() {
		if (isUI) {
        	_startPos = transform.localPosition;
			_repeatWidth = GetComponent<RectTransform>().rect.width / 2;
		}
		else {
        	_startPos = transform.position;
			_repeatWidth = GetComponent<BoxCollider>().size.x / 2 - GetComponent<BoxCollider>().center.x;
		}
	}

	private void FixedUpdate() {
		if (!GameManager.Instance.isGameRunning)
			return;

		transform.Translate(Vector3.left * GameManager.Instance.playerSpeed * Time.fixedDeltaTime, Space.World);

        if (isUI && transform.localPosition.x < -_repeatWidth)
			transform.localPosition = _startPos;
		else if (!isUI && transform.position.x < _startPos.x - _repeatWidth)
			transform.position = _startPos;
	}
}
