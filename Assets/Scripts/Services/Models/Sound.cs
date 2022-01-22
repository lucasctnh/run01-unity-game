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
	}

	public Type soundType;
	public AudioClip audioClip;
}
