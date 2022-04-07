using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputsController : MonoBehaviour {
	public static event Action OnJump;
	public static event Action OnSwitch;
	public static event Action<bool> OnHoldingJump;

	public void ButtonJumpPointerDown(BaseEventData eventData) {
		OnJump?.Invoke();
		OnHoldingJump?.Invoke(true);
	}

	public void ButtonJumpPointerUp(BaseEventData eventData) => OnHoldingJump?.Invoke(false);

	public void ButtonSwitch() => OnSwitch?.Invoke();
}
