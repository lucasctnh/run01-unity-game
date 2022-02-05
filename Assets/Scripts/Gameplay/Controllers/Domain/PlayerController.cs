using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour { // TODO: refactor this ugly mess
	public static event Action AfterSwitch;
	public static bool IsGrounded { get; private set; }

	[Header("Ground Settings")]
	[Tooltip("A position marking where to check if the player is grounded")]
	[SerializeField] private Transform _groundCheck;

	[Tooltip("A mask determining what is ground to the character")]
	[SerializeField] private LayerMask _groundLayerMask;

	[Header("Movement Settings")]
	[Space]
	[Tooltip("Amount of force added when the player jumps")]
	[SerializeField] private float _jumpForce = 10f;

	[Tooltip("Amount of force multiplied in gravity when player is falling and not holding jump")]
	[SerializeField] private float _lowJumpMultiplier = 2f;

	[Tooltip("Amount of force multiplied in gravity when player is falling")]
	[SerializeField] private float _fallMultiplier = 2.5f;

	[Tooltip("The speed multiplier for dissolving shader to complete")]
	[SerializeField] private float _teleportSpeed = 15f;

	[Header("Components Reference")]
	[Space]
	[SerializeField] private CameraController _camera;
	[SerializeField] private Renderer _body;
	[SerializeField] private Renderer _eyeL;
	[SerializeField] private Renderer _eyeR;

	private const float _GROUND_CHECK_RADIUS = .2f;

	private List<Material> _materials = new List<Material>();
	private bool _canTeleport = true;
	private bool _canDissolveDown = false;
	private bool _canDissolveUp = false;
	private float _airTime = 0f;
	private int _gravityDirection = 1;
	private int _jumps = 1;
	private bool _isHoldingJump = false;

	private void OnEnable() {
		InputsController.OnJump += Jump;
		InputsController.OnHoldingJump += isHoldingJump => AssignHoldingJump(isHoldingJump);
		InputsController.OnSwitch += VerifySwitch;
		InputsController.OnButtonJump += ButtonJump;
		InputsController.OnButtonSwitch += ButtonSwitch;
		GameManager.OnPlay += OnPlay;
	}

	private void OnDisable() {
		InputsController.OnJump -= Jump;
		InputsController.OnHoldingJump -= isHoldingJump => AssignHoldingJump(isHoldingJump);
		InputsController.OnSwitch -= VerifySwitch;
		InputsController.OnButtonJump -= ButtonJump;
		InputsController.OnButtonSwitch -= ButtonSwitch;
		GameManager.OnPlay -= OnPlay;
	}

	private void Awake() {
		foreach (Material material in _body.materials)
			_materials.Add(material);
		_materials.Add(_eyeL.material);
		_materials.Add(_eyeR.material);
	}

	private void Start() => ResetGravity();

	private void FixedUpdate() {
		if (!GameManager.Instance.isGameRunning || GameManager.Instance.isGamePaused)
			return;

		HandleGravity();
		OnLanding();
		OnGround();
		OnAir();
	}

	private void Update() {
		if (_canDissolveDown)
			DissolveDown();
		if (_canDissolveUp)
			DissolveUp();
	}

	public void ButtonJump() {
		if (_jumps > 0 && GameManager.Instance.isGameRunning && !GameManager.Instance.isGamePaused) {
			GetComponent<Rigidbody>().velocity = Vector3.up * _jumpForce * _gravityDirection;
			AudioManager.Instance.PlaySoundOneShot(Sound.Type.Jump, 2);
			GetComponent<Animator>().SetBool("Jump", true);

			_jumps--;
		}
	}

	public void ButtonSwitch() {
		if (IsGrounded && _canTeleport && GameManager.Instance.isGameRunning && !GameManager.Instance.isGamePaused) {
			StartDissolving();
			_gravityDirection *= -1;
		}
	}

	public void RechargeJumps() {
		GetComponent<Animator>().SetBool("Jump", false);
		_jumps = 1;
	}

	private async void DissolveDown() {
		_canTeleport = false;
		_canDissolveDown = false;

		float height = 3.4f;
		while (height >= -1) {
			height -= Time.deltaTime * _teleportSpeed;
			SetCutoffHeights(height);
			await Task.Yield();
		}

		await Task.Yield();

		_canDissolveUp = true;
		InvertPosition();
	}

	private void SetCutoffHeights(float height) {
		foreach (Material material in _materials)
			material.SetFloat("_CutoffHeight", height);
	}

	private async void DissolveUp() {
		_canDissolveUp = false;

		float height = -1f;
		while (height <= 3.4f) {
			height += Time.deltaTime * _teleportSpeed;
			SetCutoffHeights(height);
			await Task.Yield();
		}

		await Task.Yield();

		_canTeleport = true;
	}

	private void OnPlay() => GetComponent<Animator>().SetTrigger("Run");

	private void HandleGravity() {
		Rigidbody rb = GetComponent<Rigidbody>();
		if (_gravityDirection == 1) {
			if (rb.velocity.y < 0 && !_isHoldingJump)
				ApplyCustomGravityFall(rb);
			else if (rb.velocity.y > 0 && !_isHoldingJump)
				ApplyCustomGravityLowJumpFall(rb);
		}
		else {
			if (rb.velocity.y > 0 && !_isHoldingJump)
				ApplyCustomGravityFall(rb);
			else if (rb.velocity.y < 0 && !_isHoldingJump)
				ApplyCustomGravityLowJumpFall(rb);
		}
	}

	private void ApplyCustomGravityFall(Rigidbody rb) {
		rb.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
	}

	private void ApplyCustomGravityLowJumpFall(Rigidbody rb) {
		rb.velocity += Vector3.up * Physics.gravity.y * (_lowJumpMultiplier - 1) * Time.deltaTime;
	}

	private void OnLanding() {
		if (_airTime > 0 && IsGrounded) {
			RechargeJumps();
			_airTime = 0;
		}
	}

	private void OnGround() => IsGrounded = Physics.CheckSphere(_groundCheck.position, _GROUND_CHECK_RADIUS, _groundLayerMask);

	private void OnAir() {
		if (!IsGrounded)
			_airTime += Time.deltaTime;
	}

	private void Jump() {
		if (_jumps > 0) {
			GetComponent<Rigidbody>().velocity = Vector3.up * _jumpForce * _gravityDirection;
			AudioManager.Instance.PlaySoundOneShot(Sound.Type.Jump, 2);
			GetComponent<Animator>().SetBool("Jump", true);

			_jumps--;
		}
	}

	private void AssignHoldingJump(bool isHoldingJump) => _isHoldingJump = isHoldingJump;

	private void VerifySwitch(float yDrag) {
		int newGravityDirection = (yDrag > 0) ? 1 : -1;
		if (_gravityDirection != newGravityDirection && IsGrounded && _canTeleport) {
			StartDissolving();
			AssignNewGravityDirection(newGravityDirection);
		}
	}

	private void AssignNewGravityDirection(int newGravityDirection) => _gravityDirection = newGravityDirection;

	private void StartDissolving() => _canDissolveDown = true;

	private void InvertPosition() {
		AfterSwitch?.Invoke();
		AudioManager.Instance.PlaySoundOneShot(Sound.Type.Switch, 2);

		transform.Rotate(new Vector3(0, 0, 180), Space.Self);
		transform.position = new Vector3(transform.position.x, transform.position.y * -1, transform.position.z);

		InvertGravity();
		RechargeJumps(); // Just in case cuz sometimes the onGround wont recharge
	}

	private void InvertGravity() => Physics.gravity *= -1;

	private void ResetGravity() {
		if (Physics.gravity.y > 0)
			InvertGravity();
	}
}
