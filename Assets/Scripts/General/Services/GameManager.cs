using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public enum Stages {
		GameNotStarted,
		DynamicCars,
		CarCrashAndCrystals,
		BridgeCollapse,
		EveryStage
	}

	public static GameManager Instance;
	public static event Action OnPlay;
	public static event Action OnPause;
	public static event Action OnResume;
	public static event Action<bool> OnGameOver;
	public static event Action<SaveData> OnAssignSaveData;
	public static event Action<int, float> OnUpdateVolume;
	public static event Action<int> OnUpdateBestScore;
	public static event Action<int> OnUpdateScore;
	public static event Action<int> OnUpdateCoins;
	public static event Action<int> OnUpdateFinalScore;
	public static event Action OnChangedQuality;
	public static event Action OnPrepareContinue;
	public static event Action OnContinue;
	public static event Action OnReplay;

	public Stages CurrentStage { get { return _stage; } }
	public int BestScore { get { return _bestScore; } }
	public int Coins { get { return _coins; } }
	public bool IsGamePlayable { get { return (isGameRunning && !isGamePaused); } }

	[Header("Game Settings")]
	[Space]
	[Tooltip("If the game is running or not (correlates to the movement of the player and objects)")]
	public bool isGameRunning = false;

	[Tooltip("If the game is paused or not")]
	public bool isGamePaused = false;

	[Tooltip("The relative player speed, i.e. the speed of the objects coming towards the player")]
	public float playerSpeed = 12.5f;

	[Tooltip("Amount of speed which the relative player speed will increase over time")]
	[SerializeField] private float _speedIncrease = .1f;

	[Tooltip("Threshold amount in seconds which the game will repeat the increase of score")]
	[SerializeField] private float _scoreRate = 1f;

	[Tooltip("Threshold amount in seconds which the game will repeat the increase of difficulty")]
	[SerializeField] private float _dificultyRate = 2f;

	[Tooltip("Amount in seconds which the hitable object will increase it's spawn rate over time")]
	[SerializeField] private float _spawnRateIncrease = .005f;

	[Header("Stages Settings")]
	[Space]
	[Tooltip("The threshold amount of Score to change from stage 1 to stage 2")]
	[SerializeField] private int _stage1Threshold = 60;

	[Tooltip("The threshold amount of Score to change from stage 2 to stage 3")]
	[SerializeField] private int _stage2Threshold = 120;

	[Tooltip("The threshold amount of Score to go back from stage 3 to stage 1")]
	[SerializeField] private int _remakeStageThreshold = 240;

	[Header("Overall Settings")]
	[Space]
	[Tooltip("If the game settings is currently on low graphics or not")]
	public bool isCurrentlyLowGraphics = false;

	[Tooltip("If its the first lose of the player since openning of app or not")]
	public bool isFirstLose = true;

	[Tooltip("The chance of appearing the 2nd life button")]
	public float continueChance = .2f;

	[Tooltip("Amount of speed which the skybox will rotate")]
	[SerializeField] private float _skyboxSpeed = .8f;

	[Tooltip("Duration in seconds to blend from one skybox to another")]
	[SerializeField] private float _skyboxBlendDuration = 2f;

	[Tooltip("Duration in seconds for the end transition to finish")]
	[SerializeField] private float _endTransitionDuration = 1f;

	[Tooltip("Duration in seconds for the scale transition to finish")]
	[SerializeField] private float _scaleTransitionDuration = .5f;

	[Tooltip("The targeted fps limit")]
	[SerializeField] private int _targetFrameRate = 60;

	[Header("Components References")]
	[Space]
	[SerializeField] private RenderPipelineAsset _lowGraphicsPipeline;
	[SerializeField] private RenderPipelineAsset _mediumGraphicsPipeline;
	[SerializeField] private SpawnManager _obstacleSpawnManager;
	[SerializeField] private List<GameObject> _waters = new List<GameObject>();
	[SerializeField] private List<Color> _skyboxColors = new List<Color>();
	[SerializeField] private GameObject _bridgeCommon;
	[SerializeField] private GameObject _bridgeDamaged;
	[SerializeField] private Animator _endTransitionAnimator;
	[SerializeField] private Animator _pauseAnimator;
	[SerializeField] private Animator _gameOverAnimator;

	private Stages _stage = Stages.GameNotStarted;
	private Stages _lastStage = Stages.GameNotStarted;
	private bool _canUpdateSkybox = false;
	private bool _hasSwapedBridges = false;
	private bool _canSwapBridges = false;
	private float _skyboxLerpFactor = 0f;
	private float _difficultyTimer = 0f;
	private float _scoreTimer = 0f;
	private int _score = 0;
	private int _scoreShadow = 0; // for handling stage changes
	private int _bestScore = 0;
	private int _coins = 0;
	private int _lastStageIndex = 0;

	public static void CallRepeating(Action action, ref float timer, float repeatRate) {
		timer -= Time.deltaTime;
		if (timer < 0) {
			timer = repeatRate;
			action();
		}
	}

	private void OnEnable() {
		PlayerController.OnReachedSwapPoint += SwapBridge;
		OnUpdateScore += score => IncreaseShadowScore();
	}

	private void OnDisable() {
		PlayerController.OnReachedSwapPoint -= SwapBridge;
		OnUpdateScore -= score => IncreaseShadowScore();
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
		InitializeReset();

		SaveData data = SaveSystem.Load();
		if (data != null)
			AssignSaveData(data);

		SettingsSaveData settingsData = SaveSystem.LoadSettings();
		if (settingsData != null)
			AssignSettings(settingsData);
		else
			InitializeTrackVolumes();

		UnfreezeTime();
	}

	private void Update() {
		if (isGameRunning) {
				RenderSettings.skybox.SetFloat("_Rotation", Time.time * _skyboxSpeed);
			if (!isGamePaused) {
				CallRepeating(IncreaseScore, ref _scoreTimer, _scoreRate);

				if (_canUpdateSkybox)
					UpdateSkybox();
			}

			HandleStages();
		}
	}

	private void LateUpdate() {
		if (IsGamePlayable)
			CallRepeating(IncreaseDificulty, ref _difficultyTimer, _dificultyRate);
	}

	public void Play() {
		AudioManager.Instance.PlaySound(Sound.Type.BGM1, 1);

		isGameRunning = true;

		OnPlay?.Invoke();

		UnfreezeTime();
	}

	public void Replay() {
		AudioManager.Instance.ResumeTrack(1);

		isGamePaused = false;

		OnReplay?.Invoke();
	}

	public void Pause() {
		// if (isGamePaused)
		// 	return; // HIDE SETTINGS BUTTON

		AudioManager.Instance.PauseAllTracks();

		isGamePaused = true;

		StartCoroutine(TransitionedPause());
	}

	public void Resume() {
		AudioManager.Instance.ResumeAllTracks();

		isGamePaused = false;

		StartCoroutine(TransitionedResume());
	}

	public void GameOver(Sound.Type gameOverSound = Sound.Type.None) {
		AudioManager.Instance.PlaySoundOneShot(gameOverSound, 2);

		if (!IsGamePlayable) // to avoid GameOver more than once
			return;

		isGameRunning = false;
		isGamePaused = true;

		OnGameOver?.Invoke(IsThereNewBestScore());
		OnUpdateFinalScore?.Invoke(_score);

		SaveSystem.Save(_bestScore, _coins);
	}

	public void PlayAgain() {
		UnfreezeTime();
		StartCoroutine(ReloadSceneAfterTransition());
	}

	public void IncreaseCoin() {
		_coins++;
		OnUpdateCoins?.Invoke(_coins);
	}

	public void SpendCoins(int price) {
		_coins -= price;
		OnUpdateCoins?.Invoke(_coins);

		SaveSystem.Save(_bestScore, _coins);
	}

	public void ChangeBGMVolume(float value) {
		OnUpdateVolume?.Invoke(1, value);
		OnUpdateVolume?.Invoke(3, value);
	}

	public void ChangeSFXVolume(float value) => OnUpdateVolume?.Invoke(2, value);

	public void ChangeQuality() => StartCoroutine(ChangeAndAssignQuality());

	public void PrepareContinue() => StartCoroutine(PrepareContinueAfterFade());

	public void Continue() => StartCoroutine(ContinueAfterFade());

	private IEnumerator PrepareContinueAfterFade() {
		yield return FadeTransition("out");
		OnPrepareContinue?.Invoke();
	}

	private IEnumerator ContinueAfterFade() {
		AudioManager.Instance.ResumeTrack(2);
		isGameRunning = true;
		yield return FadeTransition("in");
		yield return TransitionedContinue();
	}

	private void AssignQuality() {
		if (isCurrentlyLowGraphics)
			ChangeQualityToLow();
		else
			ChangeQualityToMedium();

		ChangeWaterReflectionsResolution(isCurrentlyLowGraphics);
		OnChangedQuality?.Invoke();
	}

	private IEnumerator ChangeAndAssignQuality() {
		yield return FadeTransition("out");

		isCurrentlyLowGraphics = !isCurrentlyLowGraphics;

		if (isCurrentlyLowGraphics)
			ChangeQualityToLow();
		else
			ChangeQualityToMedium();

		ChangeWaterReflectionsResolution(isCurrentlyLowGraphics);
		OnChangedQuality?.Invoke();

		yield return FadeTransition("in");
	}

	private void ChangeQualityToLow() {
		QualitySettings.SetQualityLevel(0);
		QualitySettings.renderPipeline = _lowGraphicsPipeline;
	}

	private void ChangeQualityToMedium() {
		QualitySettings.SetQualityLevel(2);
		QualitySettings.renderPipeline = _mediumGraphicsPipeline;
	}

	private void LimitFrameRate() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = _targetFrameRate;
	}

	private IEnumerator ReloadSceneAfterTransition() { // TODO: on UIManager?
		_endTransitionAnimator.SetTrigger("FadeOut");
		yield return new WaitForSecondsRealtime(_endTransitionDuration);

		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private IEnumerator FadeTransition(string fadeType) {
		if (fadeType == "in") {
			yield return null;
			_endTransitionAnimator.SetTrigger("FadeIn");
		}
		else if (fadeType == "out")
			_endTransitionAnimator.SetTrigger("FadeOut");

		yield return new WaitForSecondsRealtime(_endTransitionDuration);
	}

	private IEnumerator TransitionedPause() { // TODO: refactor (maybe TransitionedFunction and general transitions)
		OnPause?.Invoke();
		yield return WaitScaleUpTransition(_pauseAnimator);
		FreezeTime();
	}

	private IEnumerator TransitionedResume() {
		UnfreezeTime();
		yield return WaitScaleDownTransition(_pauseAnimator);
		OnResume?.Invoke();
	}

	private IEnumerator TransitionedContinue() {
		yield return WaitScaleDownTransition(_gameOverAnimator);
		OnContinue?.Invoke();
	}

	private IEnumerator WaitScaleUpTransition(Animator animator) {
		animator.SetTrigger("ScaleUpBouncy");
		yield return new WaitForSeconds(_scaleTransitionDuration);
	}

	private IEnumerator WaitScaleDownTransition(Animator animator) {
		animator.SetTrigger("ScaleDown");
		yield return new WaitForSeconds(_scaleTransitionDuration);
	}

	private void InitializeTrackVolumes() {
		InitializeTrackVolume(1);
		InitializeTrackVolume(2);
		InitializeTrackVolume(3);
	}

	private void InitializeTrackVolume(int track) => OnUpdateVolume?.Invoke(track, AudioManager.Instance.GetTrackVolume(track));

	private void AssignSaveData(SaveData data) {
		_bestScore = data.bestScore;
		_coins = data.coins;

		OnAssignSaveData?.Invoke(data);
	}

	private void AssignSettings(SettingsSaveData data) {
		isCurrentlyLowGraphics = data.isLowGraphics;
		AssignQuality();

		OnUpdateVolume?.Invoke(1, data.bgmVolume);
		OnUpdateVolume?.Invoke(2, data.sfxVolume);
		OnUpdateVolume?.Invoke(3, data.bgmVolume);
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

	private void IncreaseScore() {
		_score++;
		OnUpdateScore?.Invoke(_score);
	}

	private void IncreaseDificulty() {
		playerSpeed += _speedIncrease;
		if (_obstacleSpawnManager.repeatRate > 1f)
			_obstacleSpawnManager.repeatRate -= _spawnRateIncrease;
	}

	private void HandleStages() {
		if (_scoreShadow > _remakeStageThreshold)
			RemakeStages();
		if (_scoreShadow > _stage2Threshold && _scoreShadow <= _remakeStageThreshold && _stage != Stages.BridgeCollapse) {
			ChangeState(Stages.BridgeCollapse, Sound.Type.BGM3);
			_canSwapBridges = true;
		}
		if ((_scoreShadow > _stage1Threshold && _scoreShadow <= _stage2Threshold) && _stage != Stages.CarCrashAndCrystals)
			ChangeState(Stages.CarCrashAndCrystals, Sound.Type.BGM2);
		if ((_scoreShadow > 0 && _scoreShadow <= _stage1Threshold) && _stage != Stages.DynamicCars) {
			ChangeState(Stages.DynamicCars, Sound.Type.BGM1);
			_canSwapBridges = true;
		}
	}

	private void ChangeState(Stages newStage, Sound.Type newBGM) {
		AudioManager.Instance.ChangeSoundWithFade(newBGM, 1);

		_hasSwapedBridges = false;

		_lastStage = _stage;
		_stage = newStage;

		_lastStageIndex = (int)_lastStage - 1 <= 0 ? 0 : (int)_lastStage - 1;

		Hitable.currentStage = _stage;
		_obstacleSpawnManager.repeatRate = 3f;

		if (_lastStage != Stages.GameNotStarted) {
			_canUpdateSkybox = true;
			HandleWater();
		}
	}

	private void HandleWater() {
		if (_lastStageIndex == 2)
			_waters[0].SetActive(true);
		else
			_waters[_lastStageIndex+1].SetActive(true);
	}

	private void ResetSkybox() {
		RenderSettings.skybox.SetColor("_Tint", _skyboxColors[0]);

		for (int i = 0; i < _waters.Count; i++) {
			if (i == 0)
				_waters[i].SetActive(true);
			else
				_waters[i].SetActive(false);

			_waters[i].GetComponent<Renderer>()?.material.SetFloat("_Alpha", 1);
		}
	}

	private void UpdateSkybox() {
		int nextStageIndex = (int)_stage - 1;
		int repeatWaterIndex = _lastStageIndex == 2 ? 0 : _lastStageIndex + 1;

		if (_skyboxLerpFactor < _skyboxBlendDuration) {
			_skyboxLerpFactor += Time.deltaTime;
			RenderSettings.skybox.SetColor("_Tint", Color.Lerp(_skyboxColors[_lastStageIndex], _skyboxColors[nextStageIndex], _skyboxLerpFactor));

				_waters[_lastStageIndex].GetComponent<Renderer>()?.material.SetFloat("_Alpha", Mathf.Lerp(1, 0, _skyboxLerpFactor));
				_waters[repeatWaterIndex].GetComponent<Renderer>()?.material.SetFloat("_Alpha", Mathf.Lerp(0, 1, _skyboxLerpFactor));
		}
		else {
			if (_waters[_lastStageIndex].GetComponent<Renderer>()?.material.GetFloat("_Alpha") <= 0)
				_waters[_lastStageIndex].SetActive(false);

			_canUpdateSkybox = false;
			_skyboxLerpFactor = 0f;
		}
	}

	private void ChangeWaterReflectionsResolution(bool isLowGraphics) {
		foreach (GameObject water in _waters) {
			if (isLowGraphics)
				water.GetComponent<ReflectionProbe>().resolution = 16;
			else
				water.GetComponent<ReflectionProbe>().resolution = 64;

			UpdateWaterReflectionProbe(water.GetComponent<ReflectionProbe>());
		}
	}

	private void UpdateWaterReflectionProbe(ReflectionProbe probe) {
		if (!probe.isActiveAndEnabled) {
			probe.gameObject.SetActive(true);
			probe.RenderProbe();
			probe.gameObject.SetActive(false);
		}
		else
			probe.RenderProbe();
	}

	private void InitializeReset() {
		ResetSkybox();
		ResetBridges();
	}

	private void ResetBridges() {
		_hasSwapedBridges = false;
		_canSwapBridges = false;
		_bridgeCommon.SetActive(true);
		_bridgeDamaged.SetActive(false);
	}

	private void SwapBridge() {
		if (!_hasSwapedBridges && _canSwapBridges) {
			if (_stage == Stages.BridgeCollapse) {
				_bridgeCommon.SetActive(false);
				_bridgeDamaged.SetActive(true);
			}
			else if (_stage == Stages.DynamicCars){
				_bridgeCommon.SetActive(true);
				_bridgeDamaged.SetActive(false);
			}

			_hasSwapedBridges = true;
		}
	}

	private void IncreaseShadowScore() => _scoreShadow++;

	private void RemakeStages() {
		_scoreShadow = 0;
		_stage1Threshold += 30;
		_stage2Threshold += 30;
		_remakeStageThreshold += 30;
	}
}
