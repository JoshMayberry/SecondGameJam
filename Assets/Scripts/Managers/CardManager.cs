using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CardDeck;

public class RoomCardManager : CardManager<RoomType, RoomSomething> {
    public static RoomCardManager instance { get; private set; }
    private void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one RoomCardManager in the scene.");
        }

        instance = this;
    }
}