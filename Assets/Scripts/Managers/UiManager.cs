using UnityEngine;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;

public class UiManager : MonoBehaviour {
	[Required] [SerializeField] private UiInfo uiInfoPrefab;
	[Required] [SerializeField] private GameObject heroInfoContainer;
	[Required] [SerializeField] private GameObject lootInfoContainer;
	[Required] [SerializeField] private GameObject monsterInfoContainer;
	public UnitySpawner<UiInfo> heroUiInfoSpawner;
	public UnitySpawner<UiInfo> lootUiInfoSpawner;
	public UnitySpawner<UiInfo> monsterUiInfoSpawner;

	public static UiManager instance { get; private set; }
	public void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one UiManager in the scene.");
		}

		instance = this;

		this.heroUiInfoSpawner = new UnitySpawner<UiInfo>(this.uiInfoPrefab);
	}

	public void SpawnHeroInfo(Hero hero) {
		UiInfo spawnling = this.heroUiInfoSpawner.Spawn(Vector3.zero, this.heroInfoContainer.transform);
		Debug.Log($"spawnling: {spawnling}, hero: {hero}");
		hero.uiInfo = spawnling;
		spawnling.SetData(hero.heroData);
	}

	public void DespawnHeroInfo(Hero hero) {
		if (hero.uiInfo != null) {
			this.heroUiInfoSpawner.Despawn(hero.uiInfo);
			hero.uiInfo = null;
		}
	}

	public void SpawnBuildableInfo(Buildable buildable) {
		UiInfo spawnling;

		switch (buildable.cardData.cardType) {
			case CardType.Loot:
				spawnling = this.lootUiInfoSpawner.Spawn(Vector3.zero, this.lootInfoContainer.transform);
				break;

			case CardType.Monster:
				spawnling = this.lootUiInfoSpawner.Spawn(Vector3.zero, this.monsterInfoContainer.transform);
				break;

			default:
				Debug.LogError($"Unexpected buildable type '{buildable.cardData.cardType}'");
				return;
		}

		buildable.uiInfo = spawnling;
		spawnling.SetData(buildable.cardData);
	}

	public void DespawnBuildableInfo(Buildable buildable) {
		if (buildable.uiInfo != null) {
			switch (buildable.cardData.cardType) {
				case CardType.Loot:
					this.lootUiInfoSpawner.Despawn(buildable.uiInfo);
					break;

				case CardType.Monster:
					this.monsterUiInfoSpawner.Despawn(buildable.uiInfo);
					break;

				default:
					Debug.LogError($"Unexpected buildable type '{buildable.cardData.cardType}'");
					return;
			}

			buildable.uiInfo = null;
		}
	}
}
