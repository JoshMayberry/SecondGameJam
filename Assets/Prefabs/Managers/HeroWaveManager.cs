using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;

[System.Serializable]
public class HeroWave : IWave<Hero> {
	public Hero[] possible;
	public int count;
	public float timeBetweenSpawns;

	int IWave<Hero>.count { get => count; set => count = value; }
	Hero[] IWave<Hero>.possible { get => possible; set => possible = value; }
	float IWave<Hero>.timeBetweenSpawns { get => timeBetweenSpawns; set => timeBetweenSpawns = value; }
}

[System.Serializable]
public class HeroReview {
	public int score; // Between 0 and 10
	public string description;
	public float timeSpent; // In Seconds
	public int lootGained;
	public int monstersFought;
	public float healthLost;
	public float healthLeft;
}

public class HeroWaveManager : WaveManagerBase<Hero, HeroWave> {
	public List<HeroReview> reviewList;

	public static HeroWaveManager instance { get; private set; }
	public void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one RoomManager in the scene.");
		}

		instance = this;
	}
	public override bool OnSpawn(Hero spawnling, HeroWave wave, int waveIndex, int spawnlingIndex) {
		return true;
	}

	internal void LeaveReview(HeroReview review) {
		// TODO: Store this in a list
	}
}
