using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour {
	public static event Action<bool> OnChangeSkin;

	[Header("Initial Screen")]
	[Space]
	[SerializeField] private GameObject _initMenu;
	[SerializeField] private TMP_Text _initBestScoreText;
	[SerializeField] private TMP_Text _initCoinsText;
	[SerializeField] private Button _initSettingsButton;
	[SerializeField] private Button _playButton;
	[SerializeField] private Button _arrowLeftButton;
	[SerializeField] private Button _arrowRightButton;
	[SerializeField] private Button _unlockButton;

	[Header("Game-Over Screen")]
	[Space]
	[SerializeField] private GameObject _gameOverMenu;
	[SerializeField] private GameObject _withNewBestScoreGroup;
	[SerializeField] private GameObject _belowBestScoreGroup;
	[SerializeField] private GameObject _continueGroup;
	[SerializeField] private GameObject _restartButton;
	[SerializeField] private TMP_Text _finalScoreText;
	[SerializeField] private TMP_Text _bestScoreText;

	[Header("In-Game Screen")]
	[Space]
	[SerializeField] private GameObject _gameUI;
	[SerializeField] private TMP_Text _scoreText;
	[SerializeField] private TMP_Text _coinsText;
	[SerializeField] private GameObject _switch;
	[SerializeField] private GameObject _jump;
	[SerializeField] private Button _settingsButton;

	[Header("Pause Menu Screen")]
	[Space]
	[SerializeField] private GameObject _pauseMenu;
	[SerializeField] private Slider _bgmSlider;
	[SerializeField] private Slider _sfxSlider;
	[SerializeField] private GameObject _lowGraphicsSign;

	[Header("Continue Screen")]
	[Space]
	[SerializeField] private GameObject _continueMenu;

	private bool _isPointerOnPauseMenu = false;

	private void OnEnable() {
		GameManager.OnPlay += OnPlay;
		GameManager.OnPause += OnPause;
		GameManager.OnResume += OnResume;
		GameManager.OnGameOver += isThereNewBestScore => OnGameOver(isThereNewBestScore);
		GameManager.OnAssignSaveData += data => UpdateSavedPoints(data);
		GameManager.OnUpdateBestScore += bestScore => UpdateBestScore(bestScore);
		GameManager.OnUpdateScore += score => UpdateScore(score);
		GameManager.OnUpdateCoins += coins => UpdateCoins(coins);
		GameManager.OnUpdateFinalScore += finalScore => UpdateFinalScore(finalScore);
		GameManager.OnUpdateVolume += (track, volume) => UpdateVolumeSlider(track, volume);
		GameManager.OnChangedQuality += SelectPauseMenu;
		GameManager.OnPrepareContinue += () => ContinueHubVisibility(true);
		GameManager.OnContinue += HideGameOverMenu;
		GameManager.OnReplay += () => ContinueHubVisibility(false);
	}

	private void OnDisable() {
		GameManager.OnPlay -= OnPlay;
		GameManager.OnPause -= OnPause;
		GameManager.OnResume -= OnResume;
		GameManager.OnGameOver -= isThereNewBestScore => OnGameOver(isThereNewBestScore);
		GameManager.OnAssignSaveData -= data => UpdateSavedPoints(data);
		GameManager.OnUpdateBestScore -= bestScore => UpdateBestScore(bestScore);
		GameManager.OnUpdateScore -= score => UpdateScore(score);
		GameManager.OnUpdateCoins -= coins => UpdateCoins(coins);
		GameManager.OnUpdateFinalScore -= finalScore => UpdateFinalScore(finalScore);
		GameManager.OnUpdateVolume -= (track, volume) => UpdateVolumeSlider(track, volume);
		GameManager.OnChangedQuality -= SelectPauseMenu;
		GameManager.OnPrepareContinue -= () => ContinueHubVisibility(true);
		GameManager.OnContinue -= HideGameOverMenu;
		GameManager.OnReplay -= () => ContinueHubVisibility(false);
	}

	private void Start() => SetMenusVisibility(true, false, false);

	private void Update() {
		_playButton.interactable = SkinsSystem.isCurrentSkinUnlocked;
		HandleButtonsInteractibility(GameManager.Instance.isGamePaused);

		_gameUI.SetActive(GameManager.Instance.isGameRunning);
		_lowGraphicsSign.SetActive(GameManager.Instance.isCurrentlyLowGraphics);
	}

	public void ChangeSkinByLeft() => OnChangeSkin?.Invoke(false);

	public void ChangeSkinByRight() => OnChangeSkin?.Invoke(true);

	public void PlayButtonClick() => AudioManager.Instance.PlaySoundOneShot(Sound.Type.UIClick, 2);

	public void OnDeselectPauseMenu() {
		if (!_isPointerOnPauseMenu) {
			PlayButtonClick();
			GameManager.Instance.Resume();
		}
	}

	public void OnPointerEnterPauseMenu() => _isPointerOnPauseMenu = true;

	public void OnPointerExitPauseMenu() => _isPointerOnPauseMenu = false;

	public void SelectPauseMenu() => EventSystem.current.SetSelectedGameObject(_pauseMenu);

	private void OnPlay() => SetMenusVisibility(false, false, false);

	private void OnPause() {
		SelectPauseMenu();

		SetMenusVisibility(false, true, false);

		if (!GameManager.Instance.isGameRunning)
			SetMenuVisibility(_initMenu, true);
	}

	private void OnResume() {
		if (GameManager.Instance.isGamePaused) // Avoid OnResume showing InitMenu when its called after OnGameOver (cuz of coroutine)
			SetMenuVisibility(_initMenu, false);
		else if (!GameManager.Instance.isGameRunning)
			SetMenuVisibility(_initMenu, true);

		SetMenuVisibility(_pauseMenu, false);

		SaveSystem.SaveSettings(AudioManager.Instance.GetTrackVolume(1), AudioManager.Instance.GetTrackVolume(2), GameManager.Instance.isCurrentlyLowGraphics);
	}

	private void OnGameOver(bool isThereNewBestScore) {
		float willThereBeAContinue = Random.Range(0f, 1f);
		if (GameManager.Instance.isFirstLose || willThereBeAContinue < GameManager.Instance.continueChance)
			ChangeContinueGroupVisibility(true);
		else
			ChangeContinueGroupVisibility(false);

		SetMenusVisibility(false, false, true);
		ShowScoreGroup(isThereNewBestScore);

		GameManager.Instance.isFirstLose = false;

		AudioManager.Instance.PauseAllTracks();
	}

	private void ChangeContinueGroupVisibility(bool visibility) {
		if (_restartButton != null)
			_restartButton.SetActive(!visibility);

		if (_continueGroup != null)
			_continueGroup.SetActive(visibility);
	}

	private void SetMenusVisibility(bool mainVisibility, bool pauseVisibility, bool gameOverVisibility) {
		SetMenuVisibility(_initMenu, mainVisibility);
		SetMenuVisibility(_pauseMenu, pauseVisibility);
		SetMenuVisibility(_gameOverMenu, gameOverVisibility);
	}

	private void SetMenuVisibility(GameObject menu, bool visibility) {
		if (menu != null)
			menu.SetActive(visibility);
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
		_initCoinsText.text = coins.ToString();
		_coinsText.text = coins.ToString();
	}

	private void UpdateBestScore(int bestScore) {
		_initBestScoreText.text = bestScore.ToString();
		_bestScoreText.text = "Your best is: " + bestScore;
	}

	private void UpdateScore(int score) => _scoreText.text = score.ToString();

	private void UpdateFinalScore(int finalScore) => _finalScoreText.text = "You did: " + finalScore;

	private void UpdateVolumeSlider(int track, float volume) {
		if (track == 1)
			_bgmSlider.value = volume;
		else if (track == 2)
			_sfxSlider.value = volume;
	}

	private void HandleButtonsInteractibility(bool isPaused) {
		_initSettingsButton.interactable = !isPaused;
		_arrowLeftButton.interactable = !isPaused;
		_arrowRightButton.interactable = !isPaused;
		_unlockButton.interactable = !isPaused;

		if (_switch.GetComponent<Button>() != null)
			_switch.GetComponent<Button>().interactable = !isPaused;
		if (_jump.GetComponent<Button>() != null)
			_jump.GetComponent<Button>().interactable = !isPaused;
	}

	private void ContinueHubVisibility(bool visibility) {
		if (_switch != null)
			_switch.SetActive(!visibility);
		if (_jump != null)
			_jump.SetActive(!visibility);

		ChangeContinueMenuVisibility(visibility);
	}

	private void HideGameOverMenu() => SetMenuVisibility(_gameOverMenu, false);

	private void ChangeContinueMenuVisibility(bool visibility) {
		if (_continueMenu != null)
			_continueMenu.SetActive(visibility);
	}
}