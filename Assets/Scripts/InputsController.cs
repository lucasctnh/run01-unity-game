using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Profiling;

public class InputsController : MonoBehaviour {
	public static event Action OnTouchInput;
	public static event Action OnClick;
	public static event Action<float> OnDrag;

	private float _yDrag = 0f;

	public void TouchInput(InputAction.CallbackContext context) {
		OnTouchInput?.Invoke();
	}

	public void Click(InputAction.CallbackContext context) {
		if (context.performed)
			OnClick?.Invoke();
	}

	public void Drag(InputAction.CallbackContext context) {
		if (context.performed)
			_yDrag = context.ReadValue<Vector2>().y;
		if (context.canceled && _yDrag != 0)
			OnDrag?.Invoke(_yDrag);
	}
}
