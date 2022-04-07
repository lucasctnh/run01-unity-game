using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinsSystem : MonoBehaviour {
	public static event Action OnEndOfChangeSkin;
	public static bool isCurrentSkinUnlocked = true;

	[SerializeField] private Renderer _playerRenderer;
	[SerializeField] private List<Skin> _skins = new List<Skin>();
	[SerializeField] private GameObject _unlockGroup;
	[SerializeField] private Button _unlockButton;
	[SerializeField] private TMP_Text _unlockPhraseText;

	private int _currentIndex = 0;

	private void OnEnable() => UIManager.OnChangeSkin += direction => OnChangeSkin(direction);

	private void OnDisable() => UIManager.OnChangeSkin -= direction => OnChangeSkin(direction);

	private void Start() {
		_currentIndex = 0;
		HandleUnlockGroup();

		SkinsSaveData data = SaveSystem.LoadSkins();
		if (data != null)
			AssignSkinsSaveData(data);
	}


	public void VerifyConditionForAmount() {
		Condition condition = _skins[_currentIndex].unlockCondition;
		if (condition.conditionByAmount.isAmountCoin)
			_unlockButton.enabled = condition.ConditionForAmountGreaterThanTarget(GameManager.Instance.Coins,
				condition.conditionByAmount.targetAmount);
		else
			_unlockButton.enabled = condition.ConditionForAmountGreaterThanTarget(GameManager.Instance.BestScore,
				condition.conditionByAmount.targetAmount);
	}

	public void UnlockSkin() {
		_skins[_currentIndex].unlockCondition.isUnlocked = true;
		if (_skins[_currentIndex].unlockCondition.conditionByAmount.isAmountCoin)
			GameManager.Instance.SpendCoins(_skins[_currentIndex].unlockCondition.conditionByAmount.targetAmount);

		ChangeUnlockGroupVisibility(false);
		UpdateIsCurrentSkinUnlocked();

		SaveSystem.SaveSkins(GetUnlockedArray());
	}

	private void AssignSkinsSaveData(SkinsSaveData data) {
		for (int i = 0; i < _skins.Count; i++)
			_skins[i].unlockCondition.isUnlocked = data.skinsUnlocked[i];
	}

	private bool[] GetUnlockedArray() {
		bool[] unlockedArray = new bool[_skins.Count];
		for (int i = 0; i < _skins.Count; i++)
			unlockedArray[i] = _skins[i].unlockCondition.isUnlocked;
		return unlockedArray;
	}

	private void OnChangeSkin(bool direction) {
		WalkOnSkinList(direction);
		HandleUnlockGroup();

		if (_playerRenderer != null)
			_playerRenderer.materials = _skins[_currentIndex].skinsMaterials;

		OnEndOfChangeSkin?.Invoke();
	}

	private void WalkOnSkinList(bool direction) {
		if (direction)
			_currentIndex++;
		else
			_currentIndex--;

		if (_currentIndex < 0)
			_currentIndex = _skins.Count - 1;

		if (_currentIndex > _skins.Count - 1)
			_currentIndex = 0;
	}

	private void HandleUnlockGroup() {
		if (_unlockGroup != null)
			ChangeUnlockGroupVisibility(!_skins[_currentIndex].unlockCondition.isUnlocked);

		_skins[_currentIndex].unlockCondition.condition.Invoke();

		UpdateIsCurrentSkinUnlocked();
		UpdateUnlockPhrase();
	}

	private void ChangeUnlockGroupVisibility(bool visibility) => _unlockGroup.SetActive(visibility);

	private void UpdateIsCurrentSkinUnlocked() => isCurrentSkinUnlocked = _skins[_currentIndex].unlockCondition.isUnlocked;

	private void UpdateUnlockPhrase() => _unlockPhraseText.text = _skins[_currentIndex].unlockCondition.phrase;
}
