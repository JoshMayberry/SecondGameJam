using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

using jmayberry.Spawner;
using jmayberry.CustomAttributes;
using AYellowpaper.SerializedCollections;
using UnityEngine.SocialPlatforms.Impl;
using UnityEditor;

public enum HeroType {
	Unknown,
	Mage_Black,
	Mage_White,
	Fighter_Fists,
	Fighter_Sword,
}

public enum HeroExpectationType {
	Unknown,
	MonsterBattle,
	LootGained,
	TimeSpentMin,
	TimeSpentMax,
	HpCost,
	HpLeft,
}

[Serializable]
public class Venue {
	public Buildable buildable;
	public SerializedDictionary<HeroExpectationType, HeroExpectation> heroExpectationCatalogue;
}

// TODO: Combine with Physics system
// Use: [2D PATHFINDING - Enemy AI in Unity](https://www.youtube.com/watch?v=jvtFUfJ6CP8)
public class Hero : MonoBehaviour, ISpawnable {
	[Header("Main")]
	[Required] public HeroData heroData;
	public HeroJournal journal = new HeroJournal();

	[Header("Setup")]
	[SerializedDictionary("Hero Type", "Sprite")] public SerializedDictionary<HeroType, GameObject> possibleSprites;
	[Required] [SerializeField] private AIPath aiPath;
	[Required] [SerializeField] private AIDestinationSetter aiDestinationSetter;
	[SerializeField] private float checkCooldown = 0.25f;
	//[SerializeField] private float venueTimeFallback = 2f; // In seconds
	private Vector3 initialScale;
	private float health;

	[Header("Debugging")]
	[Readonly] [SerializeField] private List<Venue> venueList = new List<Venue>();
	[Readonly] [SerializeField] private int currentDestinationIndex = -1;
	[Readonly] [SerializeField] internal UnitySpawner<Hero> spawner;
	[Readonly] private float timeEnter;
	[Readonly] private float timeExit;
	[Readonly] private float timeSpent;

	void Awake() {
		this.initialScale = this.transform.localScale;
	}

	public void OnSpawn(object spawner) {}

	public void SetData(HeroData heroData) {
		this.heroData = heroData;
		this.health = heroData.health;
		this.timeEnter = 0;
		this.timeExit = 0;
		this.timeSpent = 0;
		this.journal.Clear();
		this.currentDestinationIndex = -1;

		foreach (var item in this.possibleSprites) {
			item.Value.SetActive(item.Key == this.heroData.type);
		}
	}

	internal void PlanVisits() {
		var monsterSpots = RoomManager.instance.builtMonsterList;
		var lootSpots = RoomManager.instance.builtLootList;

		var possibleVenueList = new List<Venue>();
		foreach (var monster in monsterSpots) {
			possibleVenueList.Add(new Venue{ buildable = monster, heroExpectationCatalogue = monster.cardData.roomProvides });
		}
		foreach (var loot in lootSpots) {
			possibleVenueList.Add(new Venue { buildable = loot, heroExpectationCatalogue = loot.cardData.roomProvides });
		}

		this.venueList = possibleVenueList.OrderByDescending(venue => (int)Math.Round(
			this.getScore(venue.heroExpectationCatalogue, HeroExpectationType.MonsterBattle) + 
			this.getScore(venue.heroExpectationCatalogue, HeroExpectationType.LootGained) + 
			this.getScore(venue.heroExpectationCatalogue, HeroExpectationType.TimeSpentMin) + // If it is expected to be quick
			this.getScore(venue.heroExpectationCatalogue, HeroExpectationType.TimeSpentMax) + // If it is expected to be long
			this.getScore(venue.heroExpectationCatalogue, HeroExpectationType.HpCost) +
			this.getScore(venue.heroExpectationCatalogue, HeroExpectationType.HpLeft)
		))
			.Take(this.heroData.destinationLimit)
			.ToList();

		this.timeEnter = Time.time;
		StartCoroutine(this.CheckPathCompleteCoroutine());
		StartCoroutine(this.CheckSpriteDirectionCoroutine());
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
			yield return this.ExitDungeon();
		}
	}

	private IEnumerator EnjoyVenue() {
		Venue venue = this.venueList[this.currentDestinationIndex];
		//yield return new WaitForSeconds(venue.heroExpectation.fills.GetValueOrDefault(HeroExpectationType.TimeSpent, this.venueTimeFallback));
		yield return new WaitForSeconds(2);

		foreach (var item in venue.heroExpectationCatalogue) {
			this.IncrementValue(this.journal.experiences, item.Key, item.Value.expValue);
		}

		if (venue.buildable is Monster) {
			this.journal.monstersFought.Add(venue.buildable.cardData);
		}
		else if (venue.buildable is Loot) {
			this.journal.lootGained.Add(venue.buildable.cardData);
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
		this.timeSpent = this.timeExit - this.timeEnter;

		var expectationScoreCatalogue = new Dictionary<HeroExpectationType, float> {
			{HeroExpectationType.MonsterBattle, this.getScore(this.journal.experiences, HeroExpectationType.MonsterBattle)},
			{HeroExpectationType.LootGained, this.getScore(this.journal.experiences, HeroExpectationType.LootGained)},
			{HeroExpectationType.TimeSpentMin, this.getTimeSpentMinScore()},
			{HeroExpectationType.TimeSpentMax, this.getTimeSpentMaxScore()},
			{HeroExpectationType.HpCost, this.getScore(this.journal.experiences, HeroExpectationType.HpCost)},
			{HeroExpectationType.HpLeft, this.getScore(this.journal.experiences, HeroExpectationType.HpLeft)},
		};

		var score = Math.Clamp((int)Math.Round(expectationScoreCatalogue.ToList().Sum(pair => pair.Value)), -10, 1000);
		Debug.Log(score);
		var text = this.GenerateReviewText(expectationScoreCatalogue);
		ReviewBoard.instance.LeaveReview(this.heroData.type, score, text);
	}

	private float getScore(SerializedDictionary<HeroExpectationType, HeroExpectation> experience, HeroExpectationType expectationType) {
		var expectation = this.heroData.expectation.GetValueOrDefault(expectationType, default);
		var reality = experience.GetValueOrDefault(expectationType, default);

		return (reality.expValue >= expectation.expValue) ? expectation.scorePositive : -expectation.scoreNegative;
    }

    private float getScore(SerializedDictionary<HeroExpectationType, float> experience, HeroExpectationType expectationType) {
        var expectation = this.heroData.expectation.GetValueOrDefault(expectationType, default);
        var reality = experience.GetValueOrDefault(expectationType, default);

        return (reality >= expectation.expValue) ? expectation.scorePositive : -expectation.scoreNegative;
    }

    private float getTimeSpentMinScore() {
        var expectation = this.heroData.expectation.GetValueOrDefault(HeroExpectationType.TimeSpentMin, default);
        if (expectation.expValue == 0) {
            return 0;
        }

        return (this.timeSpent >= expectation.expValue) ? expectation.scorePositive : -expectation.scoreNegative;
    }

    private float getTimeSpentMaxScore() {
        var expectation = this.heroData.expectation.GetValueOrDefault(HeroExpectationType.TimeSpentMin, default);
        if (expectation.expValue == 0) {
            return 0;
        }

        return (this.timeSpent <= expectation.expValue) ? expectation.scorePositive : -expectation.scoreNegative;
    }

	private KeyValuePair<HeroExpectationType, float> getMostValued(
		Dictionary<HeroExpectationType, float> expectationScoreCatalogue,
		KeyValuePair<HeroExpectationType, float> mostSatisfying,
		KeyValuePair<HeroExpectationType, float> leastSatisfying
	) {
		foreach (var item in expectationScoreCatalogue.OrderByDescending(pair => pair.Value).Take(3)) {
			if ((item.Key != mostSatisfying.Key) && (item.Key != leastSatisfying.Key)) {
				return item;
			}
		}

		return mostSatisfying;
	}

	private string GenerateReviewText(Dictionary<HeroExpectationType, float> expectationScoreCatalogue) {

		var expectationScoreList = expectationScoreCatalogue.OrderByDescending(pair => pair.Value).ToList();

		// Get things to talk about
		var mostSatisfying = expectationScoreList[0];
		var leastSatisfying = expectationScoreList[expectationScoreList.Count - 1];
		var mostValued = this.getMostValued(expectationScoreCatalogue, mostSatisfying, leastSatisfying);

		// Make sure the same thing is not repeated
		var filteredScoreList = new List<KeyValuePair<HeroExpectationType, float>> { mostValued };
		if (leastSatisfying.Key != mostValued.Key) {
			filteredScoreList.Add(leastSatisfying);
		}
		if ((mostSatisfying.Key != mostValued.Key) && (mostSatisfying.Key != leastSatisfying.Key)) {
			filteredScoreList.Add(mostSatisfying);
		}

		// Build string
		List<string> textParts = new List<string>();
		foreach (var item in filteredScoreList) {
			textParts.Add(this.DescribeAspect(item.Key, (int)Math.Round(item.Value)));
		}

		switch (filteredScoreList.Count) {
			case 1:
				Debug.Log($"Hero wrote a review about {filteredScoreList[0].Key}");
				break;

			case 2:
				Debug.Log($"Hero wrote a review about {filteredScoreList[0].Key} and {filteredScoreList[1].Key}");
				break;

			case 3:
				Debug.Log($"Hero wrote a review about {filteredScoreList[0].Key}, {filteredScoreList[1].Key}, and {filteredScoreList[2].Key}");
				break;

			default:
				Debug.LogError("This should never happen");
				break;
		}

		return string.Join("\n", textParts);
	}

	private string DescribeAspect(HeroExpectationType aspect, int score) {
		switch (aspect) {
			case HeroExpectationType.LootGained:
				return DescribeLootGained(score);

			case HeroExpectationType.MonsterBattle:
				return DescribeMonsterBattle(score);

			case HeroExpectationType.HpCost:
				return DescribeHpCost(score);

			case HeroExpectationType.HpLeft:
				return DescribeHpLeft(score);

            case HeroExpectationType.TimeSpentMin:
                return DescribeTimeSpentMin(score);

            case HeroExpectationType.TimeSpentMax:
                return DescribeTimeSpentMax(score);

            default:
				return "";
		}
	}

    private string DescribeLootGained(int score) {
		var randomChoice = UnityEngine.Random.Range(0, 3);
        if (this.journal.lootGained.Count == 0) {
            switch (randomChoice) {
				case 0: return "Where's the treasure!?";
				case 1: return "Not a coin to be found...";
                default:  return "I couldn't find any loot.";
            }
        }

        var randomLoot = this.journal.lootGained[UnityEngine.Random.Range(0, this.journal.lootGained.Count - 1)];
        switch (score) {
            case 0:
                switch (randomChoice) {
                    case 0: return "I couldn't find any loot at all.";
					case 1: return "The chests were empty. Such a disappointment!";
					default: return "Searched everywhere, but it was all for naught.";
                };
            case 1:
                switch (randomChoice) {
                    case 0: return $"The loot? Just some dusty old {randomLoot.title}";
					case 1: return "Only managed to scrape together some meager trinkets.";
                    default: return "The loot was laughably scanty. Hardly worth mentioning.";
                };
            case 2:
                switch (randomChoice) {
                    case 0: return "Found a few items, but nothing to write home about.";
                    case 1: return $"A meager collection, including a worn-out {randomLoot.title}.";
                    default: return "Managed to gather only a handful of unremarkable items.";
                }
            case 3:
                switch (randomChoice) {
                    case 0: return "The loot was modest; a few items barely worth mentioning.";
                    case 1: return $"Came across a decent {randomLoot.title}, but that's about it.";
                    default: return "An average haul, nothing particularly exciting.";
                }
            case 4:
                switch (randomChoice) {
                    case 0: return "The loot was satisfactory, though I've seen better.";
                    case 1: return $"Found a somewhat interesting {randomLoot.title}.";
                    default: return "A fair amount of loot, but nothing extraordinary.";
                }
            case 5:
                switch (randomChoice) {
                    case 0: return "A decent haul, including a noteworthy {randomLoot.title}.";
                    case 1: return $"Gathered a respectable amount of loot, like a fine {randomLoot.title}.";
                    default: return "The loot was quite satisfying, with several valuable finds.";
                }
            case 6:
                switch (randomChoice) {
                    case 0: return "The loot was better than expected, especially the {randomLoot.title}.";
                    case 1: return $"Quite a good haul, including an impressive {randomLoot.title}.";
                    default: return "Pleased with the loot, particularly a {randomLoot.rarity} item.";
                }
            case 7:
                switch (randomChoice) {
                    case 0: return $"Found some excellent items, like a high-quality {randomLoot.title}.";
                    case 1: return "A rewarding loot experience, with several valuable pieces.";
                    default: return $"The loot was impressive, especially a {randomLoot.rarity} {randomLoot.title}.";
                }
            case 8:
                switch (randomChoice) {
                    case 0: return $"Struck quite lucky with a {randomLoot.rarity} {randomLoot.title}.";
                    case 1: return "A fantastic loot haul, more than I could have hoped for.";
                    default: return $"The treasures were abundant, including a coveted {randomLoot.title}.";
                }
            case 9:
                switch (randomChoice) {
                    case 0: return $"Almost perfect! Found an incredible {randomLoot.rarity} {randomLoot.title}.";
                    case 1: return "An amazing haul, just shy of legendary.";
                    default: return $"So close to perfection, especially with the {randomLoot.title} find.";
                }
            case 10:
                switch (randomChoice) {
                    case 0: return $"Struck gold! Found a {randomLoot.rarity} {randomLoot.title}, the jackpot of loot!";
                    case 1: return $"The haul was legendary! Among the treasures was a {randomLoot.rarity} {randomLoot.title}.";
                    default: return $"Unbelievable loot, including a priceless {randomLoot.title}!";
                }
            default:
                return $"Found some interesting items, including a curious {randomLoot.title}.";
        }
    }

    private string DescribeMonsterBattle(int score) {
        var randomChoice = UnityEngine.Random.Range(0, 3);
        if (this.journal.monstersFought.Count == 0) {
            switch (randomChoice) {
                case 0: return "It was so quiet...";
                case 1: return "I didn't see a single monster.";
                default: return "No mobs...";
            }
        }

        var randomMonster = this.journal.monstersFought[UnityEngine.Random.Range(0, this.journal.monstersFought.Count)];
        switch (score) {
            case 0:
                switch (randomChoice) {
                    case 0: return "The dungeon was eerily quiet, not a monster in sight.";
                    case 1: return "Expected a challenge, but the halls were empty.";
                    default: return "A walk in the park. Not a single monster to be found.";
                }
            case 1:
                switch (randomChoice) {
                    case 0: return $"Encountered a {randomMonster.title}, but it was hardly a threat.";
                    case 1: return $"A few weak monsters, like a level {randomMonster.level} {randomMonster.title}. Nothing exciting.";
                    default: return "The monsters were more like pests. Barely worth the effort.";
                }
            case 2:
                switch (randomChoice) {
                    case 0: return $"Met a couple of {randomMonster.title}s. Minor distractions at best.";
                    case 1: return $"A {randomMonster.title} tried to put up a fight. Cute effort.";
                    default: return $"The monsters, including a {randomMonster.title}, were not much of a challenge.";
                }
            case 3:
                switch (randomChoice) {
                    case 0: return $"Had a fun scuffle with a {randomMonster.title}. Slightly entertaining.";
                    case 1: return $"The {randomMonster.title} put up a little fight. A decent warm-up.";
                    default: return $"Encountered some spirited {randomMonster.title}s. A bit of a challenge.";
                }
            case 4:
                switch (randomChoice) {
                    case 0: return $"A few {randomMonster.title}s gave me a good workout.";
                    case 1: return $"Met a challenging {randomMonster.title}. Kept me on my toes.";
                    default: return $"The monsters, especially a {randomMonster.title}, were getting tougher.";
                }
            case 5:
                switch (randomChoice) {
                    case 0: return $"Fought a worthy {randomMonster.title}. A balanced match.";
                    case 1: return $"The {randomMonster.title} made me work for the win. Good fight!";
                    default: return $"The monsters, including a tough {randomMonster.title}, provided a solid challenge.";
                }
            case 6:
                switch (randomChoice) {
                    case 0: return $"A fierce {randomMonster.title} gave me a run for my money.";
                    case 1: return $"Had to push my limits against a {randomMonster.title}. Rewarding battle.";
                    default: return $"The monsters were formidable, especially the {randomMonster.title}.";
                }
            case 7:
                switch (randomChoice) {
                    case 0: return $"Battled a fearsome {randomMonster.title}. Quite the adrenaline rush.";
                    case 1: return $"A {randomMonster.title} tested my skills. Thrilling combat!";
                    default: return $"The encounters, like the one with a {randomMonster.title}, were intense.";
                }
            case 8:
                switch (randomChoice) {
                    case 0: return $"A challenging fight against a {randomMonster.title}. Nearly had me.";
                    case 1: return $"The {randomMonster.title} was a worthy adversary. An epic duel!";
                    default: return $"Each monster, especially the {randomMonster.title}, was a serious contender.";
                }
            case 9:
                switch (randomChoice) {
                    case 0: return $"An intense showdown with a {randomMonster.title}. Almost didn't make it.";
                    case 1: return $"Survived a brutal fight with a {randomMonster.title}. Exhilarating!";
                    default: return $"Fought tooth and nail against formidable foes like the {randomMonster.title}.";
                }
            case 10:
                switch (randomChoice) {
                    case 0: return $"Faced off against a legendary {randomMonster.title}! A battle for the ages.";
                    case 1: return $"A ferocious {randomMonster.rarity} {randomMonster.title} tested my limits. What a fight!";
                    default: return $"Survived an epic clash with a {randomMonster.title} of level {randomMonster.level}. Unforgettable!";
                }
            default:
                return $"Encountered some challenging foes, including a {randomMonster.title} that kept me on my toes.";
        }
    }

    private string DescribeHpCost(int score) {
        var hpCost = this.heroData.health - this.health;
        var randomChoice = UnityEngine.Random.Range(0, 3);

        switch (score) {
            case 0:
                switch (randomChoice) {
                    case 0: return "Barely a scratch on me. Was that supposed to be a challenge?";
                    case 1: return "Walked out as healthy as I went in. Too easy!";
                    default: return "Hardly worth calling it a battle. Didn't lose a single HP.";
                }
            case 1:
                switch (randomChoice) {
                    case 0: return "A few bruises, nothing more. Expected tougher battles.";
                    case 1: return "Minimal effort required. Lost just a bit of health.";
                    default: return "The danger was almost non-existent. A slight scratch at best.";
                }
            case 2:
                switch (randomChoice) {
                    case 0: return "Some minor wounds, but nothing to fret over.";
                    case 1: return "Got a few scrapes and bruises, but it was mostly under control.";
                    default: return "A little roughed up, but still in good shape.";
                }
            case 3:
                switch (randomChoice) {
                    case 0: return "Had to exert myself a bit there. Took a few hits.";
                    case 1: return "Definitely faced some resistance. Lost a bit of health.";
                    default: return "Felt the heat of battle this time, but nothing too bad.";
                }
            case 4:
                switch (randomChoice) {
                    case 0: return "A solid challenge, left with some notable battle scars.";
                    case 1: return "The opposition was decent; lost a fair chunk of health.";
                    default: return "Had to work for it this time. Took a good amount of damage.";
                }
            case 5:
                switch (randomChoice) {
                    case 0: return "That was a balanced challenge, but it did cost me some health.";
                    case 1: return "Right in the middle of tough and easy. Lost half my health.";
                    default: return "It was a fair fight. Ended up at half my health.";
                }
            case 6:
                switch (randomChoice) {
                    case 0: return "Things are getting serious. Lost more than half my health.";
                    case 1: return "Definitely not a walk in the park. The damage was significant.";
                    default: return "It was a tough one, took quite a beating.";
                }
            case 7:
                switch (randomChoice) {
                    case 0: return "That was hard-hitting. My health took a serious dive.";
                    case 1: return "Faced some heavy hitters. Health's way down.";
                    default: return "It was a struggle to keep up. Health's pretty low.";
                }
            case 8:
                switch (randomChoice) {
                    case 0: return "Barely made it through that. Health's dangerously low.";
                    case 1: return "It was a brutal encounter. Took major damage.";
                    default: return "Survived, but just barely. Health's almost depleted.";
                }
            case 9:
                switch (randomChoice) {
                    case 0: return "On the brink of collapse. That was insanely tough.";
                    case 1: return "It was an all-out battle. Health's nearly gone.";
                    default: return "Fought tooth and nail. Left with a sliver of health.";
                }
            case 10:
                switch (randomChoice) {
                    case 0: return $"Survived by the skin of my teeth, health whittled down to {this.health}. What an ordeal!";
                    case 1: return $"That was intense! Ended up with just {this.health} HP. Never been so close!";
                    default: return $"It was a fight to remember, hanging on with just {this.health} health left. Truly epic!";
                }
            default:
                return $"Lost some health, but nothing too serious. Ended up with {this.health} HP.";
        }
    }

    private string DescribeHpLeft(int score) {
        var hpLeftPercentage = (int)Math.Round((this.health / this.heroData.health) * 100);

        var randomChoice = UnityEngine.Random.Range(0, 3);
        switch (score) {
            case 0:
                switch (randomChoice) {
                    case 0: return "Barely made it out alive, clinging to my last breath.";
                    case 1: return $"Survived, but just barely. Only {hpLeftPercentage}% health left.";
                    default: return "It was a close call; I'm lucky to be alive.";
                }
            case 1:
                switch (randomChoice) {
                    case 0: return "Struggled through and came out quite battered.";
                    case 1: return $"Left with more wounds than I'd like, at just {hpLeftPercentage}% health.";
                    default: return "I'm wounded and weary but still standing.";
                }
            case 2:
                switch (randomChoice) {
                    case 0: return "Took quite a few hits, but I'm still here.";
                    case 1: return $"A rough adventure, down to {hpLeftPercentage}% health.";
                    default: return "Survived, though I've had better days.";
                }
            case 3:
                switch (randomChoice) {
                    case 0: return "That was challenging, but I've seen worse.";
                    case 1: return $"Got some scrapes and bruises, at about {hpLeftPercentage}% health.";
                    default: return "Tough, but I held my own.";
                }
            case 4:
                switch (randomChoice) {
                    case 0: return "It was a decent challenge, but I managed.";
                    case 1: return $"Still standing with around {hpLeftPercentage}% health left.";
                    default: return "Not too bad; I've weathered the storm.";
                }
            case 5:
                switch (randomChoice) {
                    case 0: return "A fair challenge, but nothing I couldn't handle.";
                    case 1: return $"Halfway through, sitting at {hpLeftPercentage}% health.";
                    default: return "Balanced on the edge, but stable.";
                }
            case 6:
                switch (randomChoice) {
                    case 0: return "Felt the heat, but kept my cool.";
                    case 1: return $"Sustained some hits, but still strong at {hpLeftPercentage}% health.";
                    default: return "Took a few knocks, but nothing serious.";
                }
            case 7:
                switch (randomChoice) {
                    case 0: return "Handled myself well, only a few minor scrapes.";
                    case 1: return $"Pretty solid performance, at {hpLeftPercentage}% health.";
                    default: return "A few bruises, but mostly in good shape.";
                }
            case 8:
                switch (randomChoice) {
                    case 0: return "Came through with flying colors, just a scratch here and there.";
                    case 1: return $"In great shape, with {hpLeftPercentage}% health remaining.";
                    default: return "A few close calls, but I'm mostly unscathed.";
                }
            case 9:
                switch (randomChoice) {
                    case 0: return "Nearly perfect, just a nick or two.";
                    case 1: return $"Almost untouched, at a healthy {hpLeftPercentage}% health.";
                    default: return "Smooth sailing with barely a mark on me.";
                }
            case 10:
                switch (randomChoice) {
                    case 0: return "Came out without a scratch. As fit as I ever was!";
                    case 1: return $"At full strength, with 100% health remaining. Couldn't be better!";
                    default: return "Unscathed and in peak condition. Ready for another adventure!";
                }
            default:
                return $"Left with {hpLeftPercentage}% health. Could've been worse, could've been better.";
        }
    }

    private string DescribeTimeSpentMin(int score) {
        var randomChoice = UnityEngine.Random.Range(0, 3);
        switch (score) {
            case 0:
                switch (randomChoice) {
                    case 0: return "Barely stepped in before stepping out. Hardly an adventure.";
                    case 1: return "Was that it? I must have blinked and missed it.";
                    default: return "I was in and out in no time. Disappointingly brief.";
                }
            case 1:
                switch (randomChoice) {
                    case 0: return "Just as I started to explore, it was already time to leave.";
                    case 1: return "Wished I had more time in there, it ended too soon.";
                    default: return "The visit was fleeting. Left wanting more.";
                }
            case 2:
                switch (randomChoice) {
                    case 0: return "It felt rushed. I had barely started exploring.";
                    case 1: return "Left too soon, barely got to see anything.";
                    default: return "The whole thing was over before I knew it.";
                }
            case 3:
                switch (randomChoice) {
                    case 0: return "Wish I had a bit more time in there, it ended abruptly.";
                    case 1: return "It was over too quickly to fully enjoy.";
                    default: return "Just when I was getting into it, it was time to go.";
                }
            case 4:
                switch (randomChoice) {
                    case 0: return "Started to be fun, but then it was over too soon.";
                    case 1: return "The visit felt short. Just a little more time would have been great.";
                    default: return "It ended just as I was starting to enjoy myself.";
                }
            case 5:
                switch (randomChoice) {
                    case 0: return "A brief but enjoyable adventure.";
                    case 1: return "Good for a quick visit, though a bit more time wouldn't have hurt.";
                    default: return "Short and sweet, but left me wanting a bit more.";
                }
            case 6:
                switch (randomChoice) {
                    case 0: return "Time well spent, though a little more wouldn't have gone amiss.";
                    case 1: return "Felt like I left just as things were getting good.";
                    default: return "Satisfyingly brief, yet could have used a few more moments.";
                }
            case 7:
                switch (randomChoice) {
                    case 0: return "A decent amount of time, though a tad more would have been perfect.";
                    case 1: return "Almost the right length. Just a smidge more time needed.";
                    default: return "Left with a small desire for a little more time.";
                }
            case 8:
                switch (randomChoice) {
                    case 0: return "Great timing, though a tiny bit more wouldn't have hurt.";
                    case 1: return "Felt just a bit short of the ideal duration.";
                    default: return "Very close to perfect, but slightly rushed at the end.";
                }
            case 9:
                switch (randomChoice) {
                    case 0: return "A quick but satisfying jaunt. Could've used a bit more.";
                    case 1: return "Nearly perfect in length, but I craved a little more.";
                    default: return "Left just on the cusp of wanting more. So close to perfect.";
                }
            case 10:
                switch (randomChoice) {
                    case 0: return "In and out with perfect timing. Couldn't have planned it better.";
                    case 1: return "The duration was just right – long enough to enjoy, quick enough to keep it exciting.";
                    default: return "Hit the sweet spot of adventure timing. Satisfyingly concise.";
                }
            default:
                return "The timing felt off, like there was more to be had.";
        }
    }

    private string DescribeTimeSpentMax(int score) {
        var randomChoice = UnityEngine.Random.Range(0, 3);
        switch (score) {
            case 0:
                switch (randomChoice) {
                    case 0: return "I felt like I was trapped in there forever!";
                    case 1: return "Time dragged on endlessly, it was too much.";
                    default: return "The visit overstayed its welcome by far.";
                }
            case 1:
                switch (randomChoice) {
                    case 0: return "The time spent was excessive and tiring.";
                    case 1: return "It felt way longer than necessary.";
                    default: return "I lingered far longer than I wanted.";
                }
            case 2:
                switch (randomChoice) {
                    case 0: return "The visit felt prolonged and somewhat boring.";
                    case 1: return "It was a bit too lengthy for my taste.";
                    default: return "Stayed longer than I planned, it dragged a bit.";
                }
            case 3:
                switch (randomChoice) {
                    case 0: return "Some parts were fun, but overall, it was too long.";
                    case 1: return "Enjoyable, but it started to feel lengthy.";
                    default: return "Time well spent, though a tad extended.";
                }
            case 4:
                switch (randomChoice) {
                    case 0: return "A good stay, but could have been shorter.";
                    case 1: return "Liked it, yet it overstretched a bit.";
                    default: return "Overall nice, but slightly longer than ideal.";
                }
            case 5:
                switch (randomChoice) {
                    case 0: return "The time spent was just about right.";
                    case 1: return "Not too long, not too short - it was perfect.";
                    default: return "Just the right amount of time for a good adventure.";
                }
            case 6:
                switch (randomChoice) {
                    case 0: return "A bit lengthy, but worth every minute.";
                    case 1: return "Time well-spent, though slightly on the longer side.";
                    default: return "Long, but filled with enjoyable moments.";
                }
            case 7:
                switch (randomChoice) {
                    case 0: return "The longer stay allowed for more discoveries.";
                    case 1: return "Stayed quite a bit, but it was rewarding.";
                    default: return "Time flew by with all the exciting encounters.";
                }
            case 8:
                switch (randomChoice) {
                    case 0: return "I lost track of time exploring every corner!";
                    case 1: return "Extended stay, but it felt like an epic journey.";
                    default: return "The longer adventure was filled with thrilling moments.";
                }
            case 9:
                switch (randomChoice) {
                    case 0: return "Almost lost track of time, it was so engaging!";
                    case 1: return "A long adventure, but every moment was a delight!";
                    default: return "Extended my stay because I didn't want to leave!";
                }
            case 10:
                switch (randomChoice) {
                    case 0: return "Every moment was a thrill, didn't notice the time!";
                    case 1: return "The time flew by in the blink of an eye, it was a blast!";
                    default: return "I lost track of time in the best way possible!";
                }
            default:
                return "I took the time to enjoy myself, but maybe a bit too long.";
        }
    }

    private void IncrementValue(SerializedDictionary<HeroExpectationType, float> catalogue, HeroExpectationType key, float amount = 0) {
		if (!catalogue.ContainsKey(key)) {
			catalogue[key] = default;
		}

		catalogue[key] += amount;
	}

	private void IncrementValue(SerializedDictionary<HeroType, int> catalogue, HeroType key, int amount = 0) {
		if (!catalogue.ContainsKey(key)) {
			catalogue[key] = default;
		}

		catalogue[key] += amount;
	}

	private void IncrementValue(SerializedDictionary<RoomCardData, int> catalogue, RoomCardData key, int amount = 0) {
		if (!catalogue.ContainsKey(key)) {
			catalogue[key] = default;
		}

		catalogue[key] += amount;
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
		HeroWaveManager.instance.SpawnGrave(this.transform);
		this.spawner.Despawn(this);
	}

	public void SeeGrave(HeroType deadHeroType) {
		this.IncrementValue(this.journal.gravesSeen, deadHeroType);
	}

	public void EnteredRoom(Room room) {
		this.IncrementValue(this.journal.roomsVisited, room.cardData);
	}

	private void OnTriggerEnter(Collider other) {
		switch (other.tag) {
			case "Hero":
				Hero hero = other.GetComponent<Hero>();
				if (hero != null) {
					this.IncrementValue(this.journal.herosSeen, hero.heroData.type);
				}
				break;
		}
	}
}
