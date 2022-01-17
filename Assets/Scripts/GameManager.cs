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
	[SerializeField] private TMP_Text _scoreText;
	[SerializeField] private TMP_Text _coinText;

	[Tooltip("Threshold amount in seconds which the score will keep increasing")]
	[SerializeField] private float _scoreRate = 1f;

	private float _timer = 0f;
	private int _score = 0;
	private int _coin = 0;

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() => UnfreezeTime();

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

		FreezeTime();
	}

	public void Pause() {
		isGameRunning = false;
		SetMenusVisibility(false, true, false);

		FreezeTime();
	}

	public void PlayAgain() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

	public void IncreaseCoin() {
		_coin++;
		_coinText.text = "x " + _coin;
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
		_scoreText.text = "Score: " + _score;
	}
}
