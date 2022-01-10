using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;

public class InputsController : MonoBehaviour {
	public static event Action OnTouchInput;
	public static event Action OnClick;
	public static event Action OnDrag;

	public void TouchInput(InputAction.CallbackContext context) {
		OnTouchInput?.Invoke();
	}

	public void Click(InputAction.CallbackContext context) {
		OnClick?.Invoke();
	}

	public void Drag(InputAction.CallbackContext context) {
		OnDrag?.Invoke();
	}
}
