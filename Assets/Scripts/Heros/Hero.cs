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

public enum HeroExpectationType {
	Unknown,
	MonsterBattle,
	LootGained,
	TimeSpentMin,
	TimeSpentMax,
	HpCost,
	HpLeft,
}

public enum HeroTiredReason {
	None = 0,
	HPTooLow = 1,
	TooLongInDungeon = 2,
	TooLongWithoutMonster = 3,
	TooLongWithoutLoot = 4,
	TooLongWithoutOtherHero = 5,
	SawTooManyGraves = 6,
	SawTooManyOtherHeroes = 7,
}

[Serializable]
public class Venue {
	public Buildable buildable;
}

public class Hero : MonoBehaviour, ISpawnable {
	[Header("Main")]
	[Required] public HeroData heroData;
	public HeroJournal journal = new HeroJournal();

	[Header("Setup")]
	[SerializedDictionary("Hero Type", "Sprite")] public SerializedDictionary<HeroType, SpriteRenderer> possibleSprites;
	[Required][SerializeField] internal AIPath aiPath;
	[Required][SerializeField] private AIDestinationSetter aiDestinationSetter;
	[SerializeField] private float checkCooldown = 0.25f;
	private Vector3 initialScale;

	[Readonly] public SpriteRenderer sprite_current;

	[Header("Debugging")]
	[Readonly][SerializeField] private List<Venue> venueList = new List<Venue>();
	[Readonly][SerializeField] private int currentDestinationIndex = -1;
	[Readonly][SerializeField] internal UnitySpawner<Hero> spawner;
	[Readonly][SerializeField] private float health;
	[Readonly] public UiInfo uiInfo;

	static public HeroExpectation emptyExpectation = new HeroExpectation();

	void Awake() {
		this.initialScale = this.transform.localScale;

		Material material = new Material(this.possibleSprites[HeroType.Unknown].material); // Keep the materials each hero uses separate from each other.
		foreach (var item in this.possibleSprites) {
			item.Value.material = material; // Individual sprites in the same hero can share a material instance though
		}
	}

	void Start() {
		StartCoroutine(this.CheckSpriteDirectionCoroutine());
	}

	public void OnSpawn(object spawner) {
	}

	public void OnDespawn(object spawner) {
		this.journal.Finished();
		UiManager.instance.DespawnHeroInfo(this);
	}

	public void SetData(HeroData heroData) {
		if (this.uiInfo != null) {
			UiManager.instance.DespawnHeroInfo(this);
		}

		this.heroData = heroData;
		this.health = heroData.health;
		this.journal.Clear();
		this.currentDestinationIndex = -1;
		this.aiPath.canMove = true;
		this.UpdateSprite();

		UiManager.instance.SpawnHeroInfo(this);
	}

	internal void UpdateSprite() {
		Debug.Log($"Updating sprites for {heroData.title}; {this.possibleSprites.Count} are available");

		foreach (var item in this.possibleSprites) {
			bool state = item.Key == heroData.type;
			item.Value.gameObject.SetActive(state);
			if (state) {
				this.sprite_current = item.Value;
			}
		}

		this.UpdateSpriteEffect_ResetAll();
		StartCoroutine(this.CheckSpriteDirectionCoroutine());
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
		this.uiInfo.SetRow("Health", this.health);
		if (health <= 0.01f) {
			this.Die();
		}
		else if (amount < 0) {
			StartCoroutine(this.UpdateSpriteEffect_Hurt());
		}
		else {
			StartCoroutine(this.UpdateSpriteEffect_Heal());
		}
	}

	public void UpdateSpriteEffect_ResetAll() {
		this.sprite_current.material.SetFloat("_BurnFade", 0);
		this.sprite_current.material.SetFloat("_RainbowFade", 0);
		this.sprite_current.material.SetFloat("_VibrateFade", 0);
		this.sprite_current.material.SetFloat("_SineRotateFade", 0);
		this.sprite_current.material.SetFloat("_SineScaleFrequency", 0);
		this.sprite_current.material.SetFloat("_DirectionalGlowFadeFade", 1);
	}

	public void UpdateSpriteEffect_IsBusy(bool state) {
		this.sprite_current.material.SetFloat("_SineScaleFrequency", (state ? 6 : 0));
	}

	public IEnumerator UpdateSpriteEffect_Attacking() {
		this.sprite_current.material.SetFloat("_SineRotateFade", 1);
		this.sprite_current.material.SetFloat("_SineRotateFrequency", 10);
		yield return new WaitForSeconds(0.2f);
		this.sprite_current.material.SetFloat("_SineRotateFrequency", 0);
		this.sprite_current.material.SetFloat("_SineRotateFade", 0);
	}

	public IEnumerator UpdateSpriteEffect_Hurt() {
		this.sprite_current.material.SetFloat("_VibrateFade", 1);
		this.sprite_current.material.SetFloat("_VibrateFrequency", 100);
		yield return new WaitForSeconds(0.2f);
		this.sprite_current.material.SetFloat("_VibrateFrequency", 0);
		this.sprite_current.material.SetFloat("_VibrateFade", 0);
	}

	public IEnumerator UpdateSpriteEffect_Heal() {
		this.sprite_current.material.SetFloat("_RainbowFade", 1);
		yield return new WaitForSeconds(0.2f);
		this.sprite_current.material.SetFloat("_RainbowFade", 0);
	}

	public void Die() {
		StartCoroutine(this.UpdateSpriteEffect_Die());
	}

	public IEnumerator UpdateSpriteEffect_Die() {
		HeroWaveManager.instance.SpawnGrave(this.transform); // TODO: Make sure this grave is hidden by the character when it appears
		this.sprite_current.material.SetFloat("_BurnFade", 1);

		LeanTween.value(gameObject, -1, 1, 1f).setOnUpdate((float val) => {
			this.sprite_current.material.SetFloat("_BurnSwirlFactor", val);
		});
		yield return new WaitForSeconds(0.2f);

		LeanTween.value(gameObject, 4, 6, 0.5f).setOnUpdate((float val) => {
			this.sprite_current.material.SetFloat("_BurnRadius", val);
		});
		yield return new WaitForSeconds(0.2f);

		LeanTween.value(gameObject, 1, -1, 0.6f).setOnUpdate((float val) => {
			this.sprite_current.material.SetFloat("_DirectionalGlowFadeFade", val);
		});
		yield return new WaitUntil(() => !LeanTween.isTweening(gameObject));

		this.sprite_current.material.SetFloat("_BurnFade", 0);
		this.spawner.Despawn(this);
	}

	internal void UpdateVenueTimePercentage(float percentFinished) {
		// TODO: Show percentage complete somehow subtle
	}

	internal HeroTiredReason GetTiredReason() {
		foreach (var item in this.heroData.toleranceLimits) {
			if (item.Value == 0) {
				continue;
			}

			switch (item.Key) {
				case HeroTiredReason.HPTooLow:
					//Debug.Log($"Check {item.Key}: {this.health}; {item.Value}");
					if (this.health < item.Value) {
						return item.Key;
					}
					break;

				case HeroTiredReason.SawTooManyGraves:
					//Debug.Log($"Check {item.Key}: {this.journal.gravesSeen.Count}; {item.Value}");
					if (this.journal.gravesSeen.Count > item.Value) {
						return item.Key;
					}
					break;

				case HeroTiredReason.SawTooManyOtherHeroes:
					//Debug.Log($"Check {item.Key}: {this.journal.herosSeen.Count}; {item.Value}");
					if (this.journal.herosSeen.Count > item.Value) {
						return item.Key;
					}
					break;

				case HeroTiredReason.TooLongInDungeon:
					//Debug.Log($"Check {item.Key}: {this.journal.GetTimeSpentInDungeon()}; {item.Value}");
					if (this.journal.GetTimeSpentInDungeon() > item.Value) {
						return item.Key;
					}
					break;

				case HeroTiredReason.TooLongWithoutMonster:
					//Debug.Log($"Check {item.Key}: {this.journal.GetTimeSinceLastMonster()}; {item.Value}");
					if (this.journal.GetTimeSinceLastMonster() > item.Value) {
						return item.Key;
					}
					break;

				case HeroTiredReason.TooLongWithoutLoot:
					//Debug.Log($"Check {item.Key}: {this.journal.GetTimeSinceLastLoot()}; {item.Value}");
					if (this.journal.GetTimeSinceLastLoot() > item.Value) {
						return item.Key;
					}
					break;

				case HeroTiredReason.TooLongWithoutOtherHero:
					//Debug.Log($"Check {item.Key}: {this.journal.GetTimeSinceLastHero()}; {item.Value}");
					if (this.journal.GetTimeSinceLastHero() > item.Value) {
						return item.Key;
					}
					break;
			}
		}

		return HeroTiredReason.None;
	}
}
