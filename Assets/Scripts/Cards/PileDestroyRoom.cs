using UnityEngine;

using Flexalon;
using BehaviorDesigner.Runtime.Tasks;
using jmayberry.CardDeck;

[RequiredComponent(typeof(FlexalonFlexibleLayout))]
public class PileDestroyRoom : PileDestroy<CardType, CardSideEffect> {
	public FlexalonFlexibleLayout flexalonLayout;
	[SerializeField] private float overlapModifier = 167;

	public void Awake() {
		this.flexalonLayout = this.GetComponent<FlexalonFlexibleLayout>();
	}

	public override bool MoveToPile(Card<CardType, CardSideEffect> uiCard) {
		if (!base.MoveToPile(uiCard)) {
			return false;
		}

		FlexalonObject cardFlexalon = uiCard.gameObject.GetComponent<FlexalonObject>();

		cardFlexalon.Offset = uiCard.gameObject.transform.localPosition;
		cardFlexalon.Rotation = uiCard.gameObject.transform.localRotation;
		cardFlexalon.Scale = uiCard.gameObject.transform.localScale;
		//cardFlexalon.Offset = this.GetStackChaosPosition(0, 0, 0);
		//cardFlexalon.Rotation = this.GetStackChaosRotation(0, 0, 0);
		//cardFlexalon.Scale = this.initialCardScale;

		this.UpdateFlexalonGap();
		return true;
	}

	public void UpdateFlexalonGap() {
		Canvas.ForceUpdateCanvases();
		this.flexalonLayout.ForceUpdate();

		float totalCardWidth = 0f;
		foreach (Transform card in this.flexalonLayout.transform) {
			RectTransform cardPieceRect = card.transform.GetChild(0) as RectTransform;
			if (cardPieceRect != null) {
				totalCardWidth += cardPieceRect.rect.width;
			}
		}

		int cardCount = this.flexalonLayout.transform.childCount;
		float containerWidth = ((RectTransform)this.flexalonLayout.transform).rect.width;
		float requiredGap = (containerWidth - totalCardWidth) / (cardCount - 1);
		if (requiredGap < 0f) {
			requiredGap -= overlapModifier;
		}

		this.flexalonLayout.Gap = Mathf.Min(0, requiredGap);
	}
}
