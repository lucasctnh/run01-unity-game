using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound {
	public enum Type {
		None,
		BGM1,
		BGM2,
		BGM3,
		Jump,
		Switch,
		Death,
		CoinPickUp,
		WaterDrip
	}

	public Type soundType;
	public AudioClip audioClip;
}
