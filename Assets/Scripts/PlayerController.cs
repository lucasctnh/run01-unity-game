using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
	[Header("Ground Settings")]
	[Tooltip("A position marking where to check if the player is grounded")]
	[SerializeField] private Transform _groundCheck;
	[Tooltip("A mask determining what is ground to the character")]
	[SerializeField] private LayerMask _groundLayerMask;

	[Header("Movement Settings")]
	[Space]
	[Tooltip("Amount of force added when the player jumps")]
	[SerializeField] private float _jumpForce = 10f;

	private const float _GROUND_CHECK_RADIUS = .2f;

	private bool _isGrounded = false;
	private float _airTime = 0f;
	private int _gravityDirection = 1;
	private int _jumps = 1;

	private void OnEnable() {
		// InputsController.OnTouchInput += Jump;
		InputsController.OnClick += Jump;
		InputsController.OnDrag += VerifyDrag;
	}

	private void OnDisable() {
		// InputsController.OnTouchInput -= Jump;
		InputsController.OnClick -= Jump;
		InputsController.OnDrag -= VerifyDrag;
	}

	private void FixedUpdate() {
		OnLanding();
		OnGround();
		OnAir();
	}

	private void Jump() {
		if (_jumps > 0) {
			GetComponent<Rigidbody>().AddForce(new Vector3(0, _jumpForce * _gravityDirection, 0), ForceMode.VelocityChange);
			_jumps--;
		}
	}

	private void OnLanding() {
		if (_airTime > 0 && _isGrounded) {
			RechargeJumps();
			_airTime = 0;
		}
	}

	// To ensure the jump will be called only one time
	private void RechargeJumps() {
		_jumps = 1;
	}

	private void OnGround() {
		_isGrounded = Physics.CheckSphere(_groundCheck.position, _GROUND_CHECK_RADIUS, _groundLayerMask);
	}

	private void OnAir() {
		if (!_isGrounded)
			_airTime += Time.deltaTime;
	}

	private void VerifyDrag(float yDrag) {
		int newGravityDirection = (yDrag > 0) ? 1 : -1;
		if (_gravityDirection != newGravityDirection && _isGrounded)
			InvertPosition();

		AssignNewGravityDirection(yDrag);
	}

	private void AssignNewGravityDirection(float yDrag) {
		_gravityDirection = (yDrag > 0) ? 1 : -1;
	}

	private void InvertPosition() {
		transform.Rotate(new Vector3(180, 0, 0), Space.Self);
		transform.position = new Vector3(transform.position.x, transform.position.y * -1, transform.position.z);

		InvertGravity();
		RechargeJumps(); // Just in case cuz sometimes the onGround wont recharge
	}

	private void InvertGravity() {
		Physics.gravity *= -1;
	}
}
