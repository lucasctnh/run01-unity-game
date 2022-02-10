[System.Serializable]
public class SettingsSaveData {
	public float bgmVolume;
	public float sfxVolume;
	public bool isLowGraphics;

	public SettingsSaveData(float bgmVolume, float sfxVolume, bool isLowGraphics) {
		this.bgmVolume = bgmVolume;
		this.sfxVolume = sfxVolume;
		this.isLowGraphics = isLowGraphics;
	}
}
