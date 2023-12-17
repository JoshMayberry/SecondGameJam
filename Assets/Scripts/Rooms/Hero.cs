using UnityEngine;
using Pathfinding;

using jmayberry.CustomAttributes;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public enum HeroType {
	Unknown,
	Mage_Black,
	Mage_White,
	Fighter_Fists,
	Fighter_Sword,
}

public class Hero : MonoBehaviour {
	[SerializeField] private AIPath aiPath;
	[SerializeField] private AIDestinationSetter aiDestinationSetter;
	[Required] public HeroType type;

	private List<Transform> destinations = new List<Transform>();
	private int currentDestinationIndex = 0;
	private float checkCooldown = 0.25f;

	private void Start() {
		this.PlanVisits();
		this.GoToNextDestination();
		StartCoroutine(CheckPathCompleteCoroutine());
		StartCoroutine(CheckSpriteDirectionCoroutine());
	}

	// TODO: Combine with Physics system
	// Use: [2D PATHFINDING - Enemy AI in Unity](https://www.youtube.com/watch?v=jvtFUfJ6CP8)
	private IEnumerator CheckSpriteDirectionCoroutine() {
		while (true) {
			yield return new WaitForSeconds(checkCooldown);
			if (aiPath.desiredVelocity.x >= 0.01f) {
				this.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			}
			else {
				this.transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
			}
		}
	}

	private IEnumerator CheckPathCompleteCoroutine() {
		while (true) {
			yield return new WaitForSeconds(checkCooldown);
			if (aiPath.reachedEndOfPath && currentDestinationIndex < destinations.Count) {
				currentDestinationIndex++;
				GoToNextDestination();
			}
		}
	}

	private void checkEndOfPath() {
		if (aiPath.reachedEndOfPath && currentDestinationIndex < destinations.Count) {
			currentDestinationIndex++;
			GoToNextDestination();
		}
	}

	private void PlanVisits() {
		var monsterSpots = RoomManager.instance.builtMonsterList;
		var lootSpots = RoomManager.instance.builtLootList;

		var allSpots = new List<(Transform, int)>();

		foreach (var monster in monsterSpots) {
			allSpots.Add((monster.transform, monster.cardData.heroInterest[type]));
		}

		foreach (var loot in lootSpots) {
			allSpots.Add((loot.transform, loot.cardData.heroInterest[type]));
		}

		destinations = allSpots.OrderByDescending(spot => spot.Item2)
							   .Take(3)
							   .Select(spot => spot.Item1)
							   .ToList();

		foreach (var item in destinations) {
			Debug.Log($"@PlanVisits; {item}");
		}
	}

	private void GoToNextDestination() {
		Debug.Log($"@GoToNextDestination; {currentDestinationIndex}; {destinations.Count}");
		if (currentDestinationIndex < destinations.Count) {
			aiDestinationSetter.target = destinations[currentDestinationIndex];
		}
		else {
			ExitDungeon();
		}
	}

	private void ExitDungeon() {
		Debug.Log($"@ExitDungeon");
		aiDestinationSetter.target = RoomManager.instance.roomEnterancePoint;
		StartCoroutine(WaitAndSetNextDestination(RoomManager.instance.heroSpawnPoint));
	}

	private IEnumerator WaitAndSetNextDestination(Transform nextDestination) {
		yield return new WaitForSeconds(checkCooldown);
		yield return new WaitUntil(() => aiPath.reachedEndOfPath);
		aiDestinationSetter.target = nextDestination;
		StartCoroutine(DespawnAfterExiting());
	}

	private IEnumerator DespawnAfterExiting() {
		Debug.Log($"@DespawnAfterExiting.1");
		yield return new WaitForSeconds(checkCooldown);
		yield return new WaitUntil(() => aiPath.reachedEndOfPath);
		Debug.Log($"@DespawnAfterExiting.2");
        //Destroy(gameObject);
    }
}
