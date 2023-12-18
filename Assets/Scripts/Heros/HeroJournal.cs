using System;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;

[Serializable]
public class HeroJournal {
	[SerializeField] public List<RoomCardData> lootGained = new List<RoomCardData>();
	[SerializeField] public List<RoomCardData> monstersFought = new List<RoomCardData>();
	[SerializedDictionary("Hero Type", "Count")] public SerializedDictionary<HeroType, int> herosSeen;
	[SerializedDictionary("Hero Type", "Count")] public SerializedDictionary<HeroType, int> gravesSeen;
	[SerializedDictionary("Room Card", "Count")] public SerializedDictionary<RoomCardData, int> roomsVisited;
	[SerializedDictionary("Expectation", "Amount")] public SerializedDictionary<HeroExpectationType, float> experiences;

	public void Clear() {
		this.lootGained.Clear();
		this.monstersFought.Clear();
		this.herosSeen.Clear();
		this.gravesSeen.Clear();
		this.roomsVisited.Clear();
		this.experiences.Clear();
	}
}
