using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
	public static GameManager Instance;

	public bool isGameRunning = false;

	[SerializeField] private GameObject _menu;
	[SerializeField] private GameObject _pauseMenu;
	[SerializeField] private GameObject _gameOverMenu;
	[SerializeField] private GameObject _gameUI;
	[SerializeField] private TMP_Text _bestScoreText;
	[SerializeField] private TMP_Text _scoreText;
	[SerializeField] private TMP_Text _coinsText;

	[Tooltip("Threshold amount in seconds which the score will keep increasing")]
	[SerializeField] private float _scoreRate = 1f;

	private float _timer = 0f;
	private int _score = 0;
	private int _bestScore = 0;
	private int _coins = 0;

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() {
		SaveData data = SaveSystem.Load();
		if (data != null)
			AssignSaveData(data);

		UnfreezeTime();
		SetMenusVisibility(true, false, false);
	}

	private void Update() {
		if (isGameRunning)
			HandleScore();

		_gameUI.SetActive(isGameRunning);
	}

	public void Play() {
		isGameRunning = true;
		SetMenusVisibility(false, false, false);

		UnfreezeTime();
	}

	public void GameOver() {
		isGameRunning = false;
		SetMenusVisibility(false, false, true);

		if (IsThereNewBestScore())
			UpdateBestScore();
		SaveSystem.Save(_bestScore, _coins);

		FreezeTime();
	}

	public void Pause() {
		isGameRunning = false;
		SetMenusVisibility(false, true, false);

		FreezeTime();
	}

	public void PlayAgain() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

	public void IncreaseCoin() {
		_coins++;
		UpdateCoins();
	}

	private void AssignSaveData(SaveData data) {
		_bestScore = data.bestScore;
		_coins = data.coins;

		UpdateSavedPoints();
	}

	private void UpdateSavedPoints() {
		UpdateCoins();
		UpdateBestScore();
	}

	private void UpdateCoins() => _coinsText.text = "x " + _coins;

	private void UpdateBestScore() => _bestScoreText.text = "Best: " + _bestScore;

	private bool IsThereNewBestScore() {
		if (_score > _bestScore) {
			_bestScore = _score;
			return true;
		}
		else
			return false;
	}

	private void SetMenusVisibility(bool mainVisibility, bool pauseVisibility, bool gameOverVisibility) {
		_menu.SetActive(mainVisibility);
		_pauseMenu.SetActive(pauseVisibility);
		_gameOverMenu.SetActive(gameOverVisibility);
	}

	private void FreezeTime() => Time.timeScale = 0f;

	private void UnfreezeTime() => Time.timeScale = 1f;

	private void HandleScore() {
		_timer += Time.deltaTime;
		if (_timer > _scoreRate) {
			_timer = 0f;
			IncreaseScore();
		}
	}

	private void IncreaseScore() {
		_score++;
		UpdateScore();
	}

	private void UpdateScore() => _scoreText.text = "Score: " + _score;
}
