using UnityEngine;

using jmayberry.CustomAttributes;
using AYellowpaper.SerializedCollections;
using System;

[Serializable]
public class HeroExpectation {
	public float expValue; // How much this is valued
	public float scorePositive; // How much a positive experience impacts things
	public float scoreNegative;// How much a negative experience impacts things
}

[CreateAssetMenu(fileName = "NewHero", menuName = "Dungeon/Hero")]
public class HeroData : ScriptableObject {
	[Required] public string title;
	[Required] public string description;
	[Required] public HeroType type;
	[Required] public float health = 5;
	[Required] public float speed = 2;
	[SerializeField] public int destinationLimit = 3;
	[SerializedDictionary("Type", "Expectation")] public SerializedDictionary<HeroExpectationType, HeroExpectation> expectation;

}
