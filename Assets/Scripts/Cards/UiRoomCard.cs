using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CardDeck;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UiTowerCard : Card<RoomType, RoomSomething> {
    public void OnCardClicked() {
        switch (this.currentState) {
            case CardState.InDraw:
                EventSystem.current.SetSelectedGameObject(null);
                RoomCardManager.instance.OnDrawCards();
                break;

            case CardState.InHand:
                break;

            case CardState.InDiscard:
                EventSystem.current.SetSelectedGameObject(null);
                RoomCardManager.instance.OnDiscardHand();
                break;

            default:
                Debug.Log($"Unknown card state {this.currentState}");
                break;
        }
    }
}
