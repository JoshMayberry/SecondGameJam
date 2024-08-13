using UnityEngine;

using jmayberry.CustomAttributes;
using AYellowpaper.SerializedCollections;
using System;

[Serializable]
public class HeroExpectation {
	[Tooltip("How much this is valued")] public float expValue;
	[Tooltip("How much a positive experience impacts things")] public float scorePositive;
	[Tooltip("How much a negative experience impacts things")] public float scoreNegative;
}

[CreateAssetMenu(fileName = "NewHero", menuName = "Dungeon/Hero")]
public class HeroData : ScriptableObject {
	[Required] public string title;
	[Required] public string description;
	[Required] public HeroType type;
	[Required] public float health = 5;
	[Required] public float speed = 2;
	[Required] public Sprite uiHead;
	[SerializedDictionary("Type", "Expectation")] public SerializedDictionary<HeroExpectationType, HeroExpectation> expectation;
	[SerializedDictionary("Type", "Tolerance")] public SerializedDictionary<HeroTiredReason, float> toleranceLimits;
}
