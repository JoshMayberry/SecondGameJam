using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CardDeck;

public enum RoomType {
    Normal,
    Loot,
    Battle,
    Entrance,
    Boss,
    Hallway,
    Monster,
}

public enum RoomSomething {
    Unknown,
}

[CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon/Room")]
public class RoomCardData : CardData<RoomType, RoomSomething> {
    [SerializeField] public RoomType type;
    [SerializeField] public Room roomPrefab;

    [SerializeField] public bool canRotate;
    [SerializeField] public bool canConnectN;
    [SerializeField] public bool canConnectE;
    [SerializeField] public bool canConnectS;
    [SerializeField] public bool canConnectW;
}
