using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Condition {
	[Serializable]
	public struct ConditionByAmount {
		[Tooltip("Wheter the condition is by amount or not")]
		public bool isConditionByAmount;

		[Tooltip("Check for amount of coins, uncheck for amount of score")]
		public bool isAmountCoin;

		[Tooltip("The targeted amount")]
		public int targetAmount;
	}

	public string phrase;
	public bool isUnlocked;

	[Tooltip("Function event that will verify the condition and set the unlock button enabled mode accordingly")]
	public UnityEvent condition;
	public ConditionByAmount conditionByAmount;

	public bool ConditionForAmountGreaterThanTarget(int currentAmount, int targetAmount) {
		return currentAmount >= targetAmount;
	}
}
