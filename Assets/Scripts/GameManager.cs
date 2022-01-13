using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {
	public static GameManager Instance;

	public bool isGameRunning = false;

	[SerializeField] private GameObject _menu;
	[SerializeField] private TMP_Text _scoreText;

	[Tooltip("Threshold amount in seconds which the score will keep increasing")]
	[SerializeField] private float _scoreRate = 1f;

	private float _timer = 0f;
	private int _score = 0;

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	private void Update() {
		if (isGameRunning)
			HandleScore();
	}

	public void Play() {
		isGameRunning = true;
		_menu.SetActive(false);
	}

	public void GameOver() {
		isGameRunning = false;
		_menu.SetActive(true);
	}

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
