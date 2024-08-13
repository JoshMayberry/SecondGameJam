using UnityEngine;
using UnityEngine.UI;

using Flexalon;
using AYellowpaper.SerializedCollections;
using jmayberry.CustomAttributes;
using jmayberry.Spawner;

public class UiInfo : MonoBehaviour, ISpawnable {
	[Required] public UiInfoItem uiInfoItemPrefab;
	[Required] public Image headImage;

	[Readonly] public FlexalonObject flexalonObject;
	[Readonly] public UnitySpawner<UiInfoItem> spawner;
	[SerializedDictionary("Row Name", "UiInfoItem")] public SerializedDictionary<string, UiInfoItem> rowCatalog = new SerializedDictionary<string, UiInfoItem>();

	public void OnDespawn(object spawner) {
		this.flexalonObject.SkipLayout = true;
		this.rowCatalog.Clear();
		this.spawner.DespawnAll();
	}

	public void OnSpawn(object spawner) {
		if (this.flexalonObject == null) {
			this.flexalonObject = this.GetComponent<FlexalonObject>();
			this.spawner = new UnitySpawner<UiInfoItem>(this.uiInfoItemPrefab);
		}
		this.flexalonObject.SkipLayout = false;
	}

	public void SetData(HeroData heroData) {
		this.headImage.sprite = heroData.uiHead;

		this.AddRow("Name", heroData.title);
		this.AddRow("Health", heroData.health);
		this.AddRow("Loot Found", 0);
		this.AddRow("Monsters Fought", 0);
		this.AddRow("Tired Reason", 0);
		this.AddRow("State", "Unknown");
	}

	public void SetData(RoomCardData venueData) {
		this.headImage.sprite = venueData.uiHead;

		this.AddRow("Name", venueData.title);
		if (venueData.cardType == CardType.Monster) {
			this.AddRow("Level", venueData.level);
			this.AddRow("Attack", venueData.attackDamage);
		}
		this.AddRow("Rarity", venueData.rarity);
		this.AddRow("Cost", venueData.cost);
		this.AddRow("State", "Unknown");
	}

	private void AddRow(string key, object value) {
		UiInfoItem rowItem = this.spawner.Spawn();
		rowItem.SetKey(key);
		rowItem.SetValue(value);
		this.rowCatalog[key] = rowItem;
	}

	public void SetRow(string key, object value) {
		if (!this.rowCatalog.ContainsKey(key)) {
			this.AddRow(key, value);
			return;
		}

		UiInfoItem rowItem = this.rowCatalog[key];
		rowItem.SetValue(value);
	}
}
