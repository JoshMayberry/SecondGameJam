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
}

public enum RoomSomething {
    Unknown,
}

[CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon/Room")]
public class RoomCard : Card<RoomType, RoomSomething> {
    [SerializeField] public RoomType type;
    [SerializeField] public Room roomPrefab;

    [SerializeField] public bool canRotate;
    [SerializeField] public bool canConnectN;
    [SerializeField] public bool canConnectE;
    [SerializeField] public bool canConnectS;
    [SerializeField] public bool canConnectW;
}
