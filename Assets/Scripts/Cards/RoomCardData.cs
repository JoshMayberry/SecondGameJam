using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CardDeck;
using AYellowpaper.SerializedCollections;
using System;

public enum CardType {
	NormalRoom,
	Entrance,
	BossRoom,
	Hallway,
	Monster,
	Loot,
	Effect,
}

public enum CardSideEffect {
	None,
	DestroyRoom,
	AddCarpet,
}

[CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon/Room")]
public class RoomCardData : CardData<CardType, CardSideEffect> {
	[SerializeField] public ConstructMode constructMode;
	[SerializeField] public CardSideEffect effectType;
	[SerializeField] public CardType cardType;
	[SerializeField] public Buildable blueprint;
	[SerializedDictionary("Expectation", "Fills")] public SerializedDictionary<HeroExpectationType, HeroExpectation> roomProvides;

	[Header("For Monsters")]
	public float attackDamage = 1;
	public float level = 1;
}
