using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using jmayberry.CustomAttributes;
using jmayberry.CardDeck;
using TMPro;
using System;

public enum ConstructMode {
	Nothing,
	Room,
	Monster,
	Loot,
	Effect,
}

public enum DungeonState {
	Open,
	Closed,
	Broken,
	TutorialOpen,
	TutorialClosed,
}

// TODO: Rename to 'BuildManager'
public class RoomManager : MonoBehaviour {
	[Required] public Room roomEnterance;
	[Required] public Transform heroSpawnPoint;
	[Required] public Transform roomEnterancePoint;
	[Required] public Button construction_uiBuild;
	[Required] public Button construction_uiCancel;
	[Required] public Button construction_uiRotateClockwise;
	[Required] public Button construction_uiRotateCounterClockwise;
	[Required] public Button construction_uiNewGame;
    [Required] public TMP_Text construction_uiErrorMessage;
	[Required] public Transform gameBoundary;
	[Required] public Grave gravePrefab;

	private int spotConnectLayer;
	private int spotMonsterLayer;
	private int spotLootLayer;
	private int spotUpgradeLayer;

    [Readonly] public ConstructMode currentConstructMode;
	[Readonly] public int rotationIndex;
	[Readonly] public Buildable currentBlueprint;
	[Readonly] public DungeonState currentDungeonState = DungeonState.Open;
	[Readonly] public Spot currentTargetConnection;
	//[SerializedDictionary("Construction Type", "Amount")] public SerializedDictionary<ConstructMode, List<Buildable>> heroInterest = new SerializedDictionary<ConstructMode, List<Buildable>>();

	[Readonly] public List<Room> builtRoomList = new List<Room>();
	[Readonly] public List<Monster> builtMonsterList = new List<Monster>();
	[Readonly] public List<Loot> builtLootList = new List<Loot>();

	public static RoomManager instance { get; private set; }
	public void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one RoomManager in the scene.");
		}

		instance = this;

		this.spotConnectLayer = LayerMask.NameToLayer("Connect Spot");
		this.spotMonsterLayer = LayerMask.NameToLayer("Monster Spot");
		this.spotLootLayer = LayerMask.NameToLayer("Loot Spot");
		this.spotUpgradeLayer = LayerMask.NameToLayer("Upgrade Spot");
    }

	public void Start() {
		this.SetConstructMode(ConstructMode.Nothing);
	}

	public void Update() {
		if (this.currentConstructMode == ConstructMode.Nothing) {
			return;
		}

		if (Input.GetMouseButtonDown(0)) {
			this.DoubleCheckForSpot();
		}
	}

	public void SetConstructMode(ConstructMode mode) {
		if (this.currentTargetConnection != null) {
			if (this.currentConstructMode != mode) {
				this.currentTargetConnection.SetVisibility(true);
				this.currentTargetConnection = null;
			}
		}

		this.currentConstructMode = mode;
		SetLayerVisibility(this.spotConnectLayer, this.currentConstructMode == ConstructMode.Room);
		SetLayerVisibility(this.spotMonsterLayer, this.currentConstructMode == ConstructMode.Monster);
		SetLayerVisibility(this.spotLootLayer, this.currentConstructMode == ConstructMode.Loot);
		SetLayerVisibility(this.spotUpgradeLayer, this.currentConstructMode == ConstructMode.Effect);

        var canConstruct = (this.currentConstructMode != ConstructMode.Nothing);
		this.gameBoundary.gameObject.SetActive(canConstruct);
		this.construction_uiBuild.gameObject.SetActive(canConstruct);
		this.construction_uiCancel.gameObject.SetActive(canConstruct);
		this.construction_uiRotateClockwise.gameObject.SetActive(canConstruct);
		this.construction_uiRotateCounterClockwise.gameObject.SetActive(canConstruct);

		if (!canConstruct) {
			this.SetErrorMessage("");
			if (this.currentTargetConnection != null) {
				this.currentTargetConnection.SetVisibility(true);
			}
			this.RemoveBlueprint();
		}

		this.construction_uiCancel.interactable = true;
		this.construction_uiBuild.interactable = false;
		this.construction_uiRotateClockwise.interactable = false;
		this.construction_uiRotateCounterClockwise.interactable = false;
	}

	// Use: https://discussions.unity.com/t/edit-camera-culling-mask/55812/3
	private void SetLayerVisibility(int layerNumber, bool state) {
		if (state) {
			Camera.main.cullingMask |= 1 << layerNumber;
		}
		else {
			Camera.main.cullingMask &= ~(1 << layerNumber);
		}
	}

	public void OnBlueprint_RotateClockwise() {
		this.SetErrorMessage("");
		this.currentBlueprint.UseNextSpot();
        this.UpdateBlueprint();
	}

	public void OnBlueprint_RotateCounterClockwise() {
		this.SetErrorMessage("");
		this.currentBlueprint.UsePreviousSpot();
		this.UpdateBlueprint();
	}

	public void OnBlueprint_Build() {
		this.SetErrorMessage("");

        switch (this.currentConstructMode) {
            case ConstructMode.Room:
				this.builtRoomList.Add((Room)this.currentBlueprint.constructed);
				break;

			case ConstructMode.Monster:
				this.builtMonsterList.Add((Monster)this.currentBlueprint.constructed);
				break;

			case ConstructMode.Loot:
				this.builtLootList.Add((Loot)this.currentBlueprint.constructed);
				break;
		}

		this.currentBlueprint.BuildBlueprint();
		this.currentBlueprint = null;
		this.currentTargetConnection = null;

		if (this.currentConstructMode == ConstructMode.Room) {
			AstarPath.active.Scan();
		}

		HeroWaveManager.instance.UpdateSpawnRate();
		this.construction_uiNewGame.gameObject.SetActive(true);
        RoomCardManager.instanceApplied.selectedCard.PlayCard();
	}

	public void OnBlueprint_Cancel() {
		this.SetErrorMessage("");
		RoomCardManager.instanceApplied.selectedCard.Deselect();
    }

	public void SwapSpot(Spot targetSpot) {
		if (this.currentConstructMode == ConstructMode.Nothing) {
			Debug.LogWarning("Something tried to build when building was not permitted");
			return;
		}

		if (this.currentTargetConnection != null) {
			this.currentTargetConnection.SetVisibility(true);
		}

		this.currentTargetConnection = targetSpot;
		this.SwapBlueprint(RoomCardManager.instanceApplied.selectedCard);
	}

	public void SwapBlueprint(RoomCard card) {
		if (this.currentConstructMode == ConstructMode.Nothing) {
			Debug.LogWarning("Something tried to build when building was not permitted");
			return;
		}

		this.RemoveBlueprint();

		var cardData = (RoomCardData)card.card;
		if (this.currentConstructMode == ConstructMode.Effect) {
			this.ApplyEffect(cardData);
			return;
		}

		this.currentBlueprint = cardData.blueprint;
		this.currentBlueprint.SetCard(card, cardData);

		this.UpdateBlueprint();
	}

	public void UpdateBlueprint() {
		if (this.currentTargetConnection == null) {
			return;
		}

		if (this.currentBlueprint == null) {
			Debug.LogWarning("No blueprint to update");
			return;
		}

		bool canRotate = this.currentBlueprint.HasMultipleSpots();
		this.construction_uiRotateClockwise.interactable = canRotate;
		this.construction_uiRotateCounterClockwise.interactable = canRotate;

		this.construction_uiBuild.interactable = this.currentBlueprint.ShowBlueprint(this.currentTargetConnection);
	}

	public void RemoveBlueprint() {
		if (this.currentBlueprint != null) {
			this.currentBlueprint.UnloadBlueprint();
			this.currentBlueprint = null;
		}
	}

	public bool isSpotClickable(Spot spot) {
		switch (this.currentConstructMode) {
			case ConstructMode.Room:
				return spot is SpotConnect;

			case ConstructMode.Monster:
				return spot is SpotMonster;

            case ConstructMode.Loot:
				return spot is SpotLoot;

            case ConstructMode.Effect:
				return spot is SpotUpgrade;

			default:
				return false;
        }
    }

	// Sometimes build spots don't register because the mouse click gets absorbed by something else
	public void DoubleCheckForSpot() {
		Vector3 mousePos = Input.mousePosition;
		Ray ray = Camera.main.ScreenPointToRay(mousePos);
		RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

		foreach (var hit in hits) {
			if (hit.collider.tag == "BuildSpot") {
				Spot spot = hit.collider.GetComponent<Spot>();
				if ((spot != null) && this.isSpotClickable(spot)) {
					Debug.Log($"Raycast active missed Build Spot; {spot.gameObject.name}; {spot.gameObject.activeSelf}");
					this.SwapSpot(spot);
                }
			}
		}
	}

	public void SetErrorMessage(string text="") {
		this.construction_uiErrorMessage.text = text;
		this.construction_uiErrorMessage.gameObject.SetActive(text != "");
	}

	public void ApplyEffect(RoomCardData cardData) {
		if (this.currentTargetConnection == null) {
			return;
		}

		switch (cardData.effectType) {
			case CardSideEffect.AddCarpet:
                this.currentTargetConnection.container.DoUpgrade(this.currentTargetConnection);
                RoomCardManager.instanceApplied.selectedCard.PlayCard();
                break;

			default:
				Debug.LogError($"Unknown effect '{cardData.effectType}' on {cardData.title}");
				break;
		}
	}

	internal void SetDungeonState(DungeonState dungeonState) {
		this.currentDungeonState = dungeonState;
		// TODO: Animate an open sign in front
	}
}
