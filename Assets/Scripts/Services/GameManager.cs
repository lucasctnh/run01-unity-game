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
	public static event Action<bool> OnMute;

	public int BestScore { get { return _bestScore; } }
	public int Coins { get { return _coins; } }

	public bool isGameRunning = false;
	public bool isGamePaused = false;


	[Tooltip("Amount of speed which the skybox will rotate")]
	[SerializeField] private float _skyboxSpeed = 8f;

	[Tooltip("Threshold amount in seconds which the score will keep increasing")]
	[SerializeField] private float _scoreRate = 1f;

	[Tooltip("Amount in seconds which the score will keep increasing")]
	[SerializeField] private float _increaseDificultyRate = 2f;

	[Tooltip("The relative player speed, i.e. the speed of the objects coming towards the player")]
	public float playerSpeed = 15f;

	[Tooltip("Amount of speed which the hitable object will increase over time")]
	[SerializeField] private float _speedIncrease = .5f;

	[SerializeField] private int _targetFrameRate = 60;

	private float _difficultyTimer = 0f;
	private float _scoreTimer = 0f;
	private int _score = 0;
	private int _bestScore = 0;
	private int _coins = 0;
	private bool _isMuted = false;

	public static void CallRepeating(Action action, ref float timer, float repeatRate) {
		timer -= Time.deltaTime;
		if (timer < 0) {
			timer = repeatRate;
			action();
		}
	}

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;

		LimitFrameRate();
	}

	private void Start() {
		AudioManager.Instance.PlaySound(Sound.Type.BGM, 1);

		SaveData data = SaveSystem.Load();
		if (data != null)
			AssignSaveData(data);

		UnfreezeTime();
	}

	private void Update() {
		if (isGameRunning && !isGamePaused) {
			CallRepeating(IncreaseScore, ref _scoreTimer, _scoreRate);
			RenderSettings.skybox.SetFloat("_Rotation", Time.time * _skyboxSpeed);
		}
	}

	private void LateUpdate() {
		if (isGameRunning && !isGamePaused)
		CallRepeating(IncreaseDificulty, ref _difficultyTimer, _increaseDificultyRate);
	}

	public void Play() {
		isGameRunning = true;
		isGamePaused = false;
		OnPlay?.Invoke();

		UnfreezeTime();
	}

	public void Pause() {
		isGameRunning = false;
		isGamePaused = true;
		OnPause?.Invoke();

		FreezeTime();
	}

	public void GameOver() {
		if (!isGameRunning && isGamePaused) // to avoid GameOver being called twice // TODO: refactor
			return;

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

	public void Mute() {
		_isMuted = !_isMuted;
		OnMute?.Invoke(_isMuted);

		if (_isMuted)
			AudioManager.Instance.MuteAll();
		else
			AudioManager.Instance.UnmuteAll();
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

	private void FreezeTime() {
		Time.timeScale = 0f;
		AudioManager.Instance.StopSound(1);
	}

	private void UnfreezeTime() {
		Time.timeScale = 1f;
		AudioManager.Instance.PlaySound(Sound.Type.BGM, 1);
	}

	private void IncreaseScore() {
		_score++;
		OnUpdateScore?.Invoke(_score);
	}

	private void IncreaseDificulty() => playerSpeed += _speedIncrease;
}
