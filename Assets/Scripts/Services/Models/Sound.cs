using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound {
	public enum Type {
		BGM,
		Jump,
		Switch,
		Death,
		CoinPickUp
	}

	public Type soundType;
	public AudioClip audioClip;
}
