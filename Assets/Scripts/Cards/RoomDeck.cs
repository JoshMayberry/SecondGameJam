using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CardDeck;

[CreateAssetMenu(fileName = "NewDeck", menuName = "Dungeon/Deck")]
public class RoomDeck : Deck<RoomType, RoomSomething> {

    public override void InitializeDrawPile() {
        RoomCardManager.instanceApplied.drawBackup.SetActive(true);
        RoomCardManager.instanceApplied.discardBackup.SetActive(true);
        base.InitializeDrawPile();
    }
}
