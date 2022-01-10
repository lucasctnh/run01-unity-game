using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	[SerializeField] private float _jumpForce = 30f;

	private void OnEnable() {
		InputsController.OnTouchInput += Jump;
		InputsController.OnClick += Jump;
		// InputsController.OnDrag += ;
	}

	private void OnDisable() {
		InputsController.OnTouchInput -= Jump;
		InputsController.OnClick -= Jump;
		// InputsController.OnDrag -= ;
	}

	private void Jump() {
		GetComponent<Rigidbody>().AddForce(new Vector3(0, _jumpForce, 0) * Time.deltaTime, ForceMode.Impulse);
	}
}
