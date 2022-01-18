using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {
	public static event Action<bool> OnChangeSkin;

	[SerializeField] private GameObject _initMenu;
	[SerializeField] private TMP_Text _initBestScoreText;
	[SerializeField] private TMP_Text _initCoinsText;
	[SerializeField] private GameObject _pauseMenu;
	[SerializeField] private GameObject _gameOverMenu;
	[SerializeField] private GameObject _withNewBestScoreGroup;
	[SerializeField] private GameObject _belowBestScoreGroup;
	[SerializeField] private TMP_Text _finalScoreText;
	[SerializeField] private TMP_Text _bestScoreText;
	[SerializeField] private GameObject _gameUI;
	[SerializeField] private TMP_Text _scoreText;
	[SerializeField] private TMP_Text _coinsText;

	private void OnEnable() {
		GameManager.OnPlay += OnPlay;
		GameManager.OnPause += OnPause;
		GameManager.OnGameOver += isThereNewBestScore => OnGameOver(isThereNewBestScore);
		GameManager.OnAssignSaveData += data => UpdateSavedPoints(data);
		GameManager.OnUpdateBestScore += bestScore => UpdateBestScore(bestScore);
		GameManager.OnUpdateScore += score => UpdateScore(score);
		GameManager.OnUpdateCoins += coins => UpdateCoins(coins);
		GameManager.OnUpdateFinalScore += finalScore => UpdateFinalScore(finalScore);
	}

	private void OnDisable() {
		GameManager.OnPlay -= OnPlay;
		GameManager.OnPause -= OnPause;
		GameManager.OnGameOver -= isThereNewBestScore => OnGameOver(isThereNewBestScore);
		GameManager.OnAssignSaveData -= data => UpdateSavedPoints(data);
		GameManager.OnUpdateBestScore -= bestScore => UpdateBestScore(bestScore);
		GameManager.OnUpdateScore -= score => UpdateScore(score);
		GameManager.OnUpdateCoins -= coins => UpdateCoins(coins);
		GameManager.OnUpdateFinalScore -= finalScore => UpdateFinalScore(finalScore);
	}

	private void Start() => SetMenusVisibility(true, false, false);

	private void Update() => _gameUI.SetActive(GameManager.Instance.isGameRunning);

	public void ChangeSkinByLeft() => OnChangeSkin?.Invoke(false);

	public void ChangeSkinByRight() => OnChangeSkin?.Invoke(true);

	private void OnPlay() => SetMenusVisibility(false, false, false);

	private void OnPause() => SetMenusVisibility(false, true, false);

	private void OnGameOver(bool isThereNewBestScore) {
		SetMenusVisibility(false, false, true);
		ShowScoreGroup(isThereNewBestScore);
	}

	private void SetMenusVisibility(bool mainVisibility, bool pauseVisibility, bool gameOverVisibility) {
		if (_initMenu != null)
			_initMenu.SetActive(mainVisibility);
		if (_pauseMenu != null)
			_pauseMenu.SetActive(pauseVisibility);
		if (_gameOverMenu != null)
			_gameOverMenu.SetActive(gameOverVisibility);
	}

	private void ShowScoreGroup(bool isThereNewBestScore) {
		if (_withNewBestScoreGroup != null)
			_withNewBestScoreGroup.SetActive(isThereNewBestScore);
		if (_belowBestScoreGroup != null)
			_belowBestScoreGroup.SetActive(!isThereNewBestScore);
	}

	private void UpdateSavedPoints(SaveData data) {
		UpdateCoins(data.coins);
		UpdateBestScore(data.bestScore);
	}

	private void UpdateCoins(int coins) {
		_initCoinsText.text = "x " + coins;
		_coinsText.text = "x " + coins;
	}

	private void UpdateBestScore(int bestScore) {
		 _initBestScoreText.text = "Best: " + bestScore;
		 _bestScoreText.text = "Best: " + bestScore;
	}

	private void UpdateScore(int score) => _scoreText.text = "Score: " + score;

	private void UpdateFinalScore(int finalScore) => _finalScoreText.text = "You did: " + finalScore;
}