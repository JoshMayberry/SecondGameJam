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
}

[Serializable]
public class RoomFills {
	[SerializedDictionary("Expectation", "Amount")] public SerializedDictionary<HeroExpectation, float> fills;
}

[CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon/Room")]
public class RoomCardData : CardData<CardType, CardSideEffect> {
    [SerializeField] public ConstructMode constructMode;
    [SerializeField] public CardSideEffect effectType;
    [SerializeField] public CardType cardType;
    [SerializeField] public Buildable blueprint;
	[SerializedDictionary("Hero Type", "Amount")] public SerializedDictionary<HeroType, RoomFills> heroInterest;

    [Header("For Monsters")]
    public float attackDamage = 1;
}
