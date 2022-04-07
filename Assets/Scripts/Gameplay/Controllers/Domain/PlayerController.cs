using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour { // TODO: refactor
	public static event Action OnInvertedPosition;
	public static event Action OnReachedSwapPoint;
	public static event Action OnShouldResetCamera;
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

	[Tooltip("The throw back force to be applied horizontally when player dies")]
	[SerializeField] private float _rigibodyThrowbackForceLeft = 2000f;

	[Tooltip("The throw back force to be applied vertically when player dies")]
	[SerializeField] private float _rigibodyThrowbackForceUp = 500f;

	[Header("Animation Settings")]
	[Space]
	[Tooltip("The amount in seconds to have a chance of a new idle animation pop up")]
	[SerializeField] private float _idleAnimationRepeatRate = 5f;

	[Header("Components Reference")]
	[Space]
	[SerializeField] private CameraController _camera;
	[SerializeField] private Transform _swapBridgePoint;
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private Animator _animator;
	[SerializeField] private Renderer _renderer;

	private const float _GROUND_CHECK_RADIUS = .2f;

	private List<Material> _materials = new List<Material>();
	private Vector3 _initialPosition;
	private Quaternion _initialRotation;
	private bool _canTeleport = true;
	private bool _canDissolveDown = false;
	private bool _canDissolveUp = false;
	private bool _isHoldingJump = false;
	private float _airTime = 0f;
	private float _idleAnimationsTimer = 0;
	private int _gravityDirection = 1;
	private int _jumps = 1;

	private void OnEnable() {
		InputsController.OnJump += Jump;
		InputsController.OnSwitch += Switch;
		InputsController.OnHoldingJump += isHoldingJump => AssignHoldingJump(isHoldingJump);
		GameManager.OnPlay += StartRun;
		GameManager.OnGameOver += obj => PlayerDeath();
		SkinsSystem.OnEndOfChangeSkin += AssignMaterials;
		GameManager.OnPrepareContinue += ResetPlayerForReplay;
		GameManager.OnReplay += OnReplay;
	}

	private void OnDisable() {
		InputsController.OnJump -= Jump;
		InputsController.OnSwitch -= Switch;
		InputsController.OnHoldingJump -= isHoldingJump => AssignHoldingJump(isHoldingJump);
		GameManager.OnPlay -= StartRun;
		GameManager.OnGameOver -= obj => PlayerDeath();
		SkinsSystem.OnEndOfChangeSkin -= AssignMaterials;
		GameManager.OnPrepareContinue -= ResetPlayerForReplay;
		GameManager.OnReplay -= OnReplay;
	}

	private void Awake() => AssignMaterials();

	private void Start() {
		ResetPlayer();

		_initialPosition = transform.position;
		_initialRotation = transform.rotation;
	}

	private void FixedUpdate() {
		if (!GameManager.Instance.IsGamePlayable)
			return;

		HandleGravity();
		OnLanding();
		OnGround();
		OnAir();
	}

	private void Update() {
		if (!GameManager.Instance.isGameRunning)
			GameManager.CallRepeating(HandleIdleAnimation, ref _idleAnimationsTimer, _idleAnimationRepeatRate);

		if (_canDissolveDown)
			StartCoroutine(DissolveDown());
		if (_canDissolveUp)
			StartCoroutine(DissolveUp());

		if (HaveReachedSwapPoint())
			OnReachedSwapPoint?.Invoke();
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Finish"))
			GameManager.Instance.GameOver();
		if (other.gameObject.CompareTag("Water"))
			GameManager.Instance.GameOver(Sound.Type.WaterDrip);
	}

	private void OnCollisionEnter(Collision other) {
		if (other.gameObject.CompareTag("Lower Floor"))
			_animator.SetTrigger("Fall");
	}

	public void Jump() {
		if (_jumps > 0 && GameManager.Instance.IsGamePlayable) {
			_rigidbody.velocity = Vector3.up * _jumpForce * _gravityDirection;
			AudioManager.Instance.PlaySoundOneShot(Sound.Type.Jump, 2);
			_animator.SetBool("Jump", true);

			_jumps--;
		}
	}

	public void Switch() {
		if (IsGrounded && _canTeleport && GameManager.Instance.IsGamePlayable)
			StartDissolving();
	}

	public void RechargeJumps() {
		_animator.SetBool("Jump", false);
		_jumps = 1;
	}

	public void AssignMaterials() {
		_materials.Clear();

		foreach (Material material in _renderer.materials)
			_materials.Add(material);
	}

	private IEnumerator DissolveDown() {
		_canTeleport = false;
		_canDissolveDown = false;

		float height = 3.4f;
		while (height >= -1) {
			height -= Time.deltaTime * _teleportSpeed;
			SetCutoffHeights(height);
			yield return null;
		}

		yield return null;

		_canDissolveUp = true;
		InvertPosition();
	}

	private void SetCutoffHeights(float height) {
		foreach (Material material in _materials)
			material.SetFloat("_CutoffHeight", height);
	}

	private IEnumerator DissolveUp() {
		_canDissolveUp = false;

		float height = -1f;
		while (height <= 3.4f) {
			height += Time.deltaTime * _teleportSpeed;
			SetCutoffHeights(height);
			yield return null;
		}

		yield return null;

		_canTeleport = true;
	}

	private void StartRun() => _animator.SetTrigger("Run");

	private void HandleGravity() {
		if (_gravityDirection == 1) {
			if (_rigidbody.velocity.y < 0 && !_isHoldingJump)
				ApplyCustomGravityFall(_rigidbody);
			else if (_rigidbody.velocity.y > 0 && !_isHoldingJump)
				ApplyCustomGravityLowJumpFall(_rigidbody);
		}
		else {
			if (_rigidbody.velocity.y > 0 && !_isHoldingJump)
				ApplyCustomGravityFall(_rigidbody);
			else if (_rigidbody.velocity.y < 0 && !_isHoldingJump)
				ApplyCustomGravityLowJumpFall(_rigidbody);
		}
	}

	private void ApplyCustomGravityFall(Rigidbody rigidbody) {
		rigidbody.velocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
	}

	private void ApplyCustomGravityLowJumpFall(Rigidbody rigidbody) {
		rigidbody.velocity += Vector3.up * Physics.gravity.y * (_lowJumpMultiplier - 1) * Time.deltaTime;
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

	private void AssignHoldingJump(bool isHoldingJump) => _isHoldingJump = isHoldingJump;

	private void StartDissolving() => _canDissolveDown = true;

	private void InvertPosition() {
		OnInvertedPosition?.Invoke();
		AudioManager.Instance.PlaySoundOneShot(Sound.Type.Switch, 2);

		transform.Rotate(new Vector3(0, 0, 180), Space.Self);
		transform.position = new Vector3(transform.position.x, transform.position.y * -1, transform.position.z);

		if (GameManager.Instance.IsGamePlayable)
			InvertGravity();

		RechargeJumps(); // Just in case cuz sometimes the onGround wont recharge
	}

	private void InvertGravity() {
		_gravityDirection *= -1;
		Physics.gravity *= -1;
	}

	private void ResetGravity() {
		if (Physics.gravity.y > 0)
			InvertGravity();
	}

	private bool HaveReachedSwapPoint() {
		if (_swapBridgePoint.position.x <= transform.position.x)
			return true;

		return false;
	}

	private void HandleIdleAnimation() {
		float idleEventMarking = Random.Range(0f, 1f);
		if (idleEventMarking > 0f && idleEventMarking < 0.1)
			_animator.SetTrigger("IdleEvent1");
		if (idleEventMarking > 0.9 && idleEventMarking < 1)
			_animator.SetTrigger("IdleEvent2");
	}

	private void ResetPlayer() {
		ResetConstraint();
		ResetGravity();
	}

	private void PlayerDeath() {
		if (_animator != null)
			_animator.SetTrigger("Die");

		ResetGravity();
		ThrowPlayerBackwards();
	}

	private void ThrowPlayerBackwards() {
		DeconstraintPlayer();

		if (_rigidbody != null)
			_rigidbody.AddForce(Vector3.left * _rigibodyThrowbackForceLeft * Time.fixedDeltaTime +
				transform.up * _rigibodyThrowbackForceUp * Time.fixedDeltaTime, ForceMode.VelocityChange);
	}

	private void DeconstraintPlayer() {
		if (_rigidbody != null)
			_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}

	private void ResetConstraint() {
		if (_rigidbody != null)
			_rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
	}

	private void ResetPlayerForReplay() {
		_rigidbody.useGravity = false;
		_rigidbody.velocity = Vector3.zero;
		_rigidbody.detectCollisions = false;

		_animator.SetTrigger("Fall");

		OnShouldResetCamera?.Invoke();
		ResetPositionAndRotation();
		ResetConstraint();
		ResetGravity();
	}

	private void ResetPositionAndRotation() {
		transform.position = new Vector3(_initialPosition.x, 5.59f, _initialPosition.z);
		transform.rotation = _initialRotation;
	}

	private void OnReplay() {
		StartRun();
		_rigidbody.detectCollisions = true;
		_rigidbody.useGravity = true;
	}
}
