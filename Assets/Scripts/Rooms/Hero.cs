using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

using jmayberry.Spawner;
using jmayberry.CustomAttributes;
using AYellowpaper.SerializedCollections;

public enum HeroType {
	Unknown,
	Mage_Black,
	Mage_White,
	Fighter_Fists,
	Fighter_Sword,
}

public enum HeroExpectation {
	Unknown,
	MonsterBattle,
	LootGained,
	TimeSpent,
	HpCost,
    HpLeft,
}

[Serializable]
public class Venue {
	public Buildable buildable;
	public RoomFills roomFills;
}

public enum VisitProgress {
	Unknown,
	Planning,
	Entering,
	Visiting,
	Exiting,
}

// TODO: Combine with Physics system
// Use: [2D PATHFINDING - Enemy AI in Unity](https://www.youtube.com/watch?v=jvtFUfJ6CP8)
public class Hero : MonoBehaviour, ISpawnable {
    [Header("Personality")]
    [Required] public HeroType type;
    [Required] public float health;
    private float healthMax;
    [SerializeField] private int destinationLimit = 3;
    [SerializedDictionary("Room Type", "Expectation")] public SerializedDictionary<HeroExpectation, float> interestExpectations;
    [SerializedDictionary("Room Type", "Expectation")] private SerializedDictionary<HeroExpectation, float> interestReality;
    [SerializedDictionary("Room Type", "Score Multiplier")] private SerializedDictionary<HeroExpectation, float> interestValues;

    [Header("For Score")]
    [SerializeField] private float timeEnter;
    [SerializeField] private float timeExit;
    [SerializeField] private List<Loot> lootGained = new List<Loot>();
    [SerializeField] private List<Monster> monstersFought = new List<Monster>();
    [SerializedDictionary("Hero Type", "Count")] public SerializedDictionary<HeroType, int> herosSeen;
    [SerializedDictionary("Hero Type", "Count")] public SerializedDictionary<HeroType, int> gravesSeen;
    [SerializedDictionary("Room Card", "Count")] public SerializedDictionary<RoomCardData, int> roomsVisited;

    [Header("Setup")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private float checkCooldown = 0.25f;
    [SerializeField] private float venueTimeFallback = 2f; // In seconds
    [SerializeField] private float scoreMultiplierFallback = 2f; // In seconds
    private Vector3 initialScale;

    [Header("Debugging")]
    [SerializeField] private VisitProgress currentVisitProgress;
    [SerializeField] private List<Venue> venueList = new List<Venue>();
    [SerializeField] private int currentDestinationIndex = -1;
    [SerializeField] private UnitySpawner<Hero> spawner;


    void Awake() {
		this.healthMax = this.health;
		this.initialScale = this.transform.localScale;
    }

    public void OnSpawn(object spawner) {
        if (spawner is UnitySpawner<Hero> heroSpawner) {
            this.spawner = heroSpawner;
        }

        this.Reset();
        this.PlanVisits();

        this.timeEnter = Time.time;
        StartCoroutine(CheckPathCompleteCoroutine());
        StartCoroutine(CheckSpriteDirectionCoroutine());
    }

    void Reset() {
        this.health = this.healthMax;
        this.timeEnter = 0;
        this.timeExit = 0;
        this.monstersFought.Clear();
        this.lootGained.Clear();
        this.currentDestinationIndex = -1;
    }

	private float weighVenue(Venue venue) {
        // TODO: Check the contents of this dictionary against `this.interestExpectations`
        //return Math.Max(venue.roomFills.fills.GetValueOrDefault(HeroExpectation.LootGained, 0), venue.roomFills.fills.GetValueOrDefault(HeroExpectation.MonsterBattle, 0));

        return UnityEngine.Random.Range(0f, 100f);
	}

	private void PlanVisits() {
		var monsterSpots = RoomManager.instance.builtMonsterList;
		var lootSpots = RoomManager.instance.builtLootList;

		var possibleVenueList = new List<Venue>();
		foreach (var monster in monsterSpots) {
			possibleVenueList.Add(new Venue{ buildable = monster, roomFills = monster.cardData.heroInterest[type] });
		}
		foreach (var loot in lootSpots) {
			possibleVenueList.Add(new Venue { buildable = loot, roomFills = loot.cardData.heroInterest[type] });
		}

		this.venueList = possibleVenueList.OrderByDescending(this.weighVenue)
			.Take(this.destinationLimit)
			.ToList();
    }

    private IEnumerator CheckPathCompleteCoroutine() {
        yield return this.GoToNextDestination();

        while (true) {
            yield return new WaitForSeconds(this.checkCooldown);
            yield return new WaitUntil(() => this.aiPath.reachedEndOfPath);
            if (this.currentDestinationIndex < this.venueList.Count) {
                yield return this.EnjoyVenue();
            }

            yield return this.GoToNextDestination();
        }
    }

    private IEnumerator GoToNextDestination() {
        this.currentDestinationIndex++;
		if (this.currentDestinationIndex < this.venueList.Count) {
            this.aiDestinationSetter.target = this.venueList[currentDestinationIndex].buildable.transform;
            yield return new WaitForSeconds(2);

            yield return null;
		} else {
            yield return ExitDungeon();
        }
	}

	private IEnumerator EnjoyVenue() {
		Venue venue = this.venueList[this.currentDestinationIndex];
        //yield return new WaitForSeconds(venue.roomFills.fills.GetValueOrDefault(HeroExpectation.TimeSpent, this.venueTimeFallback));
        yield return new WaitForSeconds(2);

        foreach (var item in venue.roomFills.fills) {
			if (!this.interestReality.ContainsKey(item.Key)) {
				this.interestReality[item.Key] = 0;
            }

			this.interestReality[item.Key] += item.Value;
        }

		if (venue.buildable is Monster) {
			monstersFought.Add((Monster)venue.buildable);
		}
		else if (venue.buildable is Loot) {
            lootGained.Add((Loot)venue.buildable);
        }

		yield return null;
    }

	private IEnumerator ExitDungeon() {
		// Go to Enterance
		this.aiDestinationSetter.target = RoomManager.instance.roomEnterancePoint;
		yield return new WaitForSeconds(this.checkCooldown);
		yield return new WaitUntil(() => this.aiPath.reachedEndOfPath);

        // Go to Map Edge
        aiDestinationSetter.target = RoomManager.instance.heroSpawnPoint;
		yield return new WaitForSeconds(this.checkCooldown);
		yield return new WaitUntil(() => this.aiPath.reachedEndOfPath);
		this.spawner.Despawn(this);
	}

    public void OnDespawn(object spawner) {
        this.timeExit = Time.time;

        //HeroReview review = new HeroReview {
        //    timeSpent = this.timeExit - this.timeEnter,
        //    lootGained = this.lootGained.Count,
        //    monstersFought = this.monstersFought.Count,
        //    healthLost = this.healthMax - this.health,
        //    healthLeft = this.health
        //};

        //review.score = this.CalculateScore(review);
        //review.description = this.GenerateReviewDescription(review);

        //HeroWaveManager.instance.LeaveReview(review);
    }

	private float getScoreMultiplier(HeroExpectation expectationType) {
		return this.interestValues.GetValueOrDefault(expectationType, this.scoreMultiplierFallback);
    }

    private float multiplyScore(float score, HeroExpectation expectationType) {
        return score * getScoreMultiplier(expectationType);
    }

    private float divideScore(float score, HeroExpectation expectationType) {
		float value = getScoreMultiplier(expectationType);
		if ((value <= 0.01f) && (value >= 0.01f)) {
			return 0;
		}

		return score / value;
    }

    private int CalculateScore(HeroReview review) {
        float score = 0;

        score += this.multiplyScore(review.lootGained, HeroExpectation.LootGained);
        score += this.multiplyScore(review.monstersFought, HeroExpectation.MonsterBattle);

        score -= this.divideScore(review.timeSpent, HeroExpectation.TimeSpent);
        score -= this.divideScore(review.healthLost, HeroExpectation.HpCost);

        return (int)Math.Round(Mathf.Clamp(score, 0, 10));
    }

    private string GenerateReviewDescription(HeroReview review) {
        // Generate a text description based on the hero's experience
        string description = "I spent " + review.timeSpent + " seconds in the dungeon. ";
        if (review.lootGained > 0) {
            description += $"Found {review.lootGained} loot!";
            //description += $"Found a {this.lootGained[0].cardData.title}!";
        }
        if (review.monstersFought > 0) {
            description += $"Battled with {review.monstersFought} monsters.";
        }
        if (review.healthLost > this.healthMax / 2) {
            description += "It was a tough fight. ";
        }
        else {
            description += "The challenges were manageable. ";
        }

        return description;
    }

    // Makes the hero face the correct direction
    private IEnumerator CheckSpriteDirectionCoroutine() {
        while (true) {
            yield return new WaitForSeconds(this.checkCooldown);
            this.transform.localScale = new Vector3(((this.aiPath.desiredVelocity.x < -0.01f) ? -1 : 1) * this.initialScale.x, this.initialScale.y, this.initialScale.z);
        }
    }

    public void Damage(float amount) {
        this.health -= amount;
        if (health <= 0.01f) {
            this.Die();
        }
    }

    public void Die() {
        this.spawner.Despawn(this);
    }

    public void SeeGrave(HeroType deadHeroType) {
        if (!this.gravesSeen.ContainsKey(deadHeroType)) {
            this.gravesSeen[deadHeroType] = 0;
        }

        this.gravesSeen[deadHeroType] += 1;
    }

    public void EnteredRoom(Room room) {
        if (!this.roomsVisited.ContainsKey(room.cardData)) {
            this.roomsVisited[room.cardData] = 0;
        }

        this.roomsVisited[room.cardData] += 1;
    }

    private void OnTriggerEnter(Collider other) {
        switch (other.tag) {
            case "Hero":
                Hero hero = other.GetComponent<Hero>();
                if (hero != null) {
                    if (!this.herosSeen.ContainsKey(hero.type)) {
                        this.herosSeen[hero.type] = 0;
                    }

                    this.herosSeen[hero.type] += 1;
                }
                break;
        }
    }
}
