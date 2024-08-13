using System;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using jmayberry.Spawner;
using jmayberry.CustomAttributes;

[Serializable]
public class HeroWave : IWave<Hero> {
	public Hero[] possible;
	public int count;
	public float timeBetweenSpawns;
	public HeroData[] possibleData;

	int IWave<Hero>.count { get => count; set => count = value; }
	Hero[] IWave<Hero>.possible { get => possible; set => possible = value; }
	float IWave<Hero>.timeBetweenSpawns { get => timeBetweenSpawns; set => timeBetweenSpawns = value; }
}

public class HeroWaveManager : WaveManagerBase<Hero, HeroWave> {
	[Required][SerializeField] private Grave gravePrefab;
	private UnitySpawner<Grave> graveSpawner;

	[SerializedDictionary("Hero Type", "Hero Data")] public SerializedDictionary<string, HeroData> HeroDataCatalog;

	public static HeroWaveManager instance { get; private set; }
	public void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one HeroWaveManager in the scene.");
		}

		instance = this;

		this.graveSpawner = new UnitySpawner<Grave>(this.gravePrefab);
		AstarPath.active.Scan();
	}

	public override bool OnSpawn(Hero spawnling, HeroWave wave, int waveIndex, int spawnlingIndex) {
		if ((RoomManager.instance == null) || (RoomManager.instance.currentDungeonState != DungeonState.Open)) {
			this.spawner.Despawn(spawnling);
			return true;
		}

		HeroData heroData = wave.possibleData[UnityEngine.Random.Range(0, wave.possibleData.Length)];
		spawnling.spawner = this.spawner;
		spawnling.SetData(heroData);
		// TODO: Start or reset AI Behavior?

		return true;
	}

	internal void SpawnGrave(Transform location) {
		this.graveSpawner.Spawn(location);
	}

	public void UpdateSpawnRate() {
        float spawnRate = 15;
        spawnRate -= (RoomManager.instance.builtMonsterList.Count + RoomManager.instance.builtLootList.Count) / 3;
        spawnRate -= RoomManager.instance.builtRoomList.Count / 5;

		this.currentWave.timeBetweenSpawns = Math.Clamp(spawnRate, 4, 15);
    }
}
