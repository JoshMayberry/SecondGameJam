using UnityEngine;

using Flexalon;
using BehaviorDesigner.Runtime.Tasks;
using jmayberry.CardDeck;

public class PileDrawRoom : PileDraw<CardType, CardSideEffect> {
	public FlexalonFlexibleLayout flexalonLayout;
	[SerializeField] internal float overlapModifier = 167;
	[SerializeField] internal float flexalonWidth = 1f;

	public void Awake() {
		this.flexalonLayout = this.GetComponent<FlexalonFlexibleLayout>();
	}

	public override bool MoveToPile(Card<CardType, CardSideEffect> uiCard) {
		if (!base.MoveToPile(uiCard)) {
			return false;
		}

		FlexalonObject cardFlexalon = uiCard.gameObject.GetComponent<FlexalonObject>();
		//cardFlexalon.Width = this.flexalonWidth;
		//cardFlexalon.WidthType = SizeType.Fill;


		cardFlexalon.Offset = RoomCardManager.SafeVector(uiCard.gameObject.transform.localPosition);
		cardFlexalon.Rotation = uiCard.gameObject.transform.localRotation;
		cardFlexalon.Scale = uiCard.gameObject.transform.localScale;
		//cardFlexalon.Offset = this.GetStackChaosPosition(0, 0, 0);
		//cardFlexalon.Rotation = this.GetStackChaosRotation(0, 0, 0);
		//cardFlexalon.Scale = this.initialCardScale;

		return true;
	}
}
