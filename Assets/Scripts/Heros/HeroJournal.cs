using System;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using jmayberry.CustomAttributes;

[Serializable]
public class HeroJournal {
	[SerializeField] public List<RoomCardData> lootGained = new List<RoomCardData>();
	[SerializeField] public List<RoomCardData> monstersFought = new List<RoomCardData>();
	[SerializedDictionary("Hero", "Count")] public SerializedDictionary<Hero, int> herosSeen;
	[SerializedDictionary("Grave", "Count")] public SerializedDictionary<Grave, int> gravesSeen;
	[SerializedDictionary("Room Card", "Count")] public SerializedDictionary<RoomCardData, int> roomsVisited;
	[SerializedDictionary("Expectation", "Amount")] public SerializedDictionary<HeroExpectationType, float> experiences;
	[SerializeField] public float timeEnter;
	[SerializeField] public float timeExit;
	[SerializeField] public float timeLastMonster;
	[SerializeField] public float timeLastLoot;
	[SerializeField] public float timeLastHero;

	public void Clear() {
		this.timeEnter = Time.time;
		this.timeExit = 0;
		this.timeLastMonster = 0;
		this.timeLastLoot = 0;
		this.timeLastHero = 0;
		this.lootGained.Clear();
		this.monstersFought.Clear();
		this.herosSeen.Clear();
		this.gravesSeen.Clear();
		this.roomsVisited.Clear();
		this.experiences.Clear();
	}

	public void Finished() {
		this.timeExit = Time.time;
	}

	public float GetTimeSpentInDungeon() {
		if (this.timeExit != 0) {
			return this.timeExit - this.timeEnter;
		}

		return Time.time - this.timeEnter;
	}
	public float GetTimeSince(float timestamp) {
		if (timestamp == 0) {
			return GetTimeSpentInDungeon();
		}
		if (this.timeExit != 0) {
			return this.timeExit - timestamp;
		}
		return Time.time - timestamp;
	}

	public float GetTimeSinceLastMonster() {
		return this.GetTimeSince(this.timeLastMonster);
	}

	public float GetTimeSinceLastLoot() {
		return this.GetTimeSince(this.timeLastLoot);
	}

	public float GetTimeSinceLastHero() {
		return this.GetTimeSince(this.timeLastHero);
	}

	public void SawHero(Hero hero) {
		this.herosSeen.TryGetValue(hero, out int timesSeen);
		this.herosSeen[hero] = timesSeen + 1;
	}

	public void SawGrave(Grave grave) {
		this.gravesSeen.TryGetValue(grave, out int timesSeen);
		this.gravesSeen[grave] = timesSeen + 1;
	}
}
