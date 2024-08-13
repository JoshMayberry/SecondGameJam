using UnityEngine;

using jmayberry.CardDeck;
using jmayberry.CustomAttributes;
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
	AddCarpet,
}

[CreateAssetMenu(fileName = "NewRoom", menuName = "Dungeon/Room")]
public class RoomCardData : CardData<CardType, CardSideEffect> {
	[SerializeField] public ConstructMode constructMode;
	[SerializeField] public CardSideEffect effectType;
	[SerializeField] public CardType cardType;
	[SerializeField] public Buildable blueprint;
	[Required] public Sprite uiHead;
	[SerializedDictionary("Expectation", "Fills")] public SerializedDictionary<HeroExpectationType, HeroExpectation> roomProvides;
	public float minTime; // How much time this takes to do (minimum) in seconds
	public float maxTime; // How much time this takes to do (maximum) in seconds
	public float rechargeTime; // How much time this takes to recharge (in seconds)

	[Header("For Monsters")]
	public float attackDamage = 1;
	public float level = 1;
}
