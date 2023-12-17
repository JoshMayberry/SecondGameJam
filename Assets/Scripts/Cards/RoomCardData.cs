using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CardDeck;
using AYellowpaper.SerializedCollections;

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

[CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon/Room")]
public class RoomCardData : CardData<CardType, CardSideEffect> {
    [SerializeField] public ConstructMode constructMode;
    [SerializeField] public CardSideEffect effectType;
    [SerializeField] public CardType cardType;
    [SerializeField] public Buildable blueprint;
	[SerializedDictionary("Hero Type", "Amount")] public SerializedDictionary<HeroType, int> heroInterest;
}
