using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Condition {
	[Serializable]
	public struct ConditionByAmount {
		public bool isConditionByAmount;
		public bool isAmountCoin;
		public int targetAmount;
	}

	public string phrase;
	public bool isUnlocked;
	public UnityEvent condition;
	public ConditionByAmount conditionByAmount;

	public bool ConditionForAmountGreaterThanTarget(int currentAmount, int targetAmount) {
		return currentAmount >= targetAmount;
	}
}
