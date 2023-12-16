using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using jmayberry.CardDeck;
using jmayberry.CustomAttributes;

public class UiTowerCard : Card<RoomType, RoomSomething> {
	[Required] public Image selectionSprite;

	public void OnCardClicked() {
		switch (this.currentState) {
			case CardState.InDraw:
				this.Deselect();
				RoomCardManager.instance.OnDrawCards();
                break;

			case CardState.InHand:
				this.Select();
				break;

			case CardState.InDiscard:
				this.Deselect();
				RoomCardManager.instance.OnDiscardHand();
                break;

			default:
				Debug.Log($"Unknown card state {this.currentState}");
				break;
		}
	}

	public override void PlayCard(IGameContext<RoomType, RoomSomething> context) {
		this.Deselect();
		base.PlayCard(context);
    }

    public void Select() {
		if (RoomCardManager.instanceApplied.selectedCard != null) {
			RoomCardManager.instanceApplied.selectedCard.selectionSprite.enabled = false;
        }

		this.selectionSprite.enabled = true;
        RoomCardManager.instanceApplied.selectedCard = this;
        RoomManager.instance.SetConnectSpotVisibility(true);
        RoomManager.instance.ConstructionUI_SetVisibility(true);

		if (RoomManager.instance.targetConnection != null) {
            RoomManager.instance.SwapBlueprint(this);
        }
    }

    public void Deselect() {
        if (RoomCardManager.instanceApplied.selectedCard != null) {
            RoomCardManager.instanceApplied.selectedCard.selectionSprite.enabled = false;
        }

        this.selectionSprite.enabled = false;
        RoomCardManager.instanceApplied.selectedCard = null;
        RoomManager.instance.SetConnectSpotVisibility(false);
        RoomManager.instance.ConstructionUI_SetVisibility(false);
        EventSystem.current.SetSelectedGameObject(null);
    }
}
