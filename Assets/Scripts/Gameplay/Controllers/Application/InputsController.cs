using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputsController : MonoBehaviour {
	public static event Action OnJump;
	public static event Action<bool> OnHoldingJump;
	public static event Action<float> OnSwitch;
	public static event Action OnButtonJump;
	public static event Action OnButtonSwitch;

	private float _yDrag = 0f;

	public void ButtonJumpPointerDown(BaseEventData eventData) {
		OnButtonJump?.Invoke();
		OnHoldingJump?.Invoke(true);
	}

	public void ButtonJumpPointerUp(BaseEventData eventData) => OnHoldingJump?.Invoke(false);

	public void ButtonSwitch() => OnButtonSwitch?.Invoke();

	public void Jump(InputAction.CallbackContext context) {
		if (context.performed && GameManager.Instance.IsGamePlayable)
			OnJump?.Invoke();

		HoldingJump(context);
	}

	private void HoldingJump(InputAction.CallbackContext context) {
		if (context.performed)
			OnHoldingJump?.Invoke(true);
		if (context.canceled)
			OnHoldingJump?.Invoke(false);
	}

	public void Switch(InputAction.CallbackContext context) {
		if (context.performed && GameManager.Instance.IsGamePlayable) {
			_yDrag = context.ReadValue<float>();
			if (_yDrag != 0)
				OnSwitch?.Invoke(_yDrag);
		}
	}
}
