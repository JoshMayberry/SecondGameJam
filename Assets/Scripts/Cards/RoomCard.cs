using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using jmayberry.CardDeck;
using jmayberry.CustomAttributes;

// TODO: rename to "buildCard"
public class RoomCard : Card<CardType, CardSideEffect> {
	[Required] public Image selectionSprite;
	[Readonly] public RoomCardData cardDataApplied;

	public override void SetCard(CardData<CardType, CardSideEffect> cardData) {
		base.SetCard(cardData);
		this.cardDataApplied = (RoomCardData)cardData;

		if (this.cardDataApplied.constructMode == ConstructMode.Nothing) {
			Debug.LogWarning($"CardData missing construction type: {this.cardDataApplied.title}");
		}

        if ((this.cardDataApplied.blueprint == null) && (this.cardDataApplied.cardType != CardType.Effect)) {
            Debug.LogWarning($"CardData missing blueprint: {this.cardDataApplied.title}");
        }

    }

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

	public override void PlayCard(IGameContext<CardType, CardSideEffect> context) {
		this.Deselect();
		base.PlayCard(context);
	}

	public void Select() {
		if (RoomCardManager.instanceApplied.selectedCard != null) {
			RoomCardManager.instanceApplied.selectedCard.selectionSprite.enabled = false;
		}

		this.selectionSprite.enabled = true;
		RoomCardManager.instanceApplied.selectedCard = this;
		RoomManager.instance.SetConstructMode(this.cardDataApplied.constructMode);
		RoomManager.instance.SwapBlueprint(this);
	}

	public void Deselect() {
		if (RoomCardManager.instanceApplied.selectedCard != null) {
			RoomCardManager.instanceApplied.selectedCard.selectionSprite.enabled = false;
		}

		this.selectionSprite.enabled = false;
		RoomCardManager.instanceApplied.selectedCard = null;
		RoomManager.instance.SetConstructMode(ConstructMode.Nothing);

		EventSystem.current.SetSelectedGameObject(null);
	}
}
