using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
	public static GameManager Instance;
	public static event Action OnPlay;
	public static event Action OnPause;
	public static event Action<bool> OnGameOver;
	public static event Action<SaveData> OnAssignSaveData;
	public static event Action<int> OnUpdateBestScore;
	public static event Action<int> OnUpdateScore;
	public static event Action<int> OnUpdateCoins;
	public static event Action<int> OnUpdateFinalScore;

	public int BestScore { get { return _bestScore; } }
	public int Coins { get { return _coins; } }

	public bool isGameRunning = false;
	public bool isGamePaused = false;


	[Tooltip("Threshold amount in seconds which the score will keep increasing")]
	[SerializeField] private float _scoreRate = 1f;

	[SerializeField] private int _targetFrameRate = 60;

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

		LimitFrameRate();
	}

	private void Start() {
		SaveData data = SaveSystem.Load();
		if (data != null)
			AssignSaveData(data);

		UnfreezeTime();
	}

	private void Update() {
		if (isGameRunning)
			HandleScore();
	}

	public void Play() {
		if (!SkinsSystem.isCurrentSkinUnlocked)
			return; // TODO: disable button

		isGameRunning = true;
		isGamePaused = false;
		OnPlay?.Invoke();

		UnfreezeTime();
	}

	public void Pause() {
		isGameRunning = true;
		isGamePaused = true;
		OnPause?.Invoke();

		FreezeTime();
	}

	public void GameOver() {
		isGameRunning = false;
		isGamePaused = true;
		OnGameOver?.Invoke(IsThereNewBestScore());
		OnUpdateFinalScore?.Invoke(_score);

		SaveSystem.Save(_bestScore, _coins);

		FreezeTime();
	}

	public void PlayAgain() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

	public void IncreaseCoin() {
		_coins++;
		OnUpdateCoins?.Invoke(_coins);
	}

	public void SpendCoins(int price) {
		_coins -= price;
		OnUpdateCoins?.Invoke(_coins);

		SaveSystem.Save(_bestScore, _coins);
	}

	private void LimitFrameRate() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = _targetFrameRate;
	}

	private void AssignSaveData(SaveData data) {
		_bestScore = data.bestScore;
		_coins = data.coins;

		OnAssignSaveData?.Invoke(data);
	}

	private bool IsThereNewBestScore() {
		if (_score > _bestScore) {
			_bestScore = _score;
			OnUpdateBestScore?.Invoke(_bestScore);
			return true;
		}
		else
			return false;
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
		OnUpdateScore?.Invoke(_score);
	}
}
