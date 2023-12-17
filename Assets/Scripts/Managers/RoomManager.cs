using UnityEngine;

using jmayberry.CustomAttributes;
using UnityEngine.UI;
using jmayberry.CardDeck;

public enum ConstructMode {
	Nothing,
	Room,
	Monster,
	Loot,
	Effect,
}

// TODO: Rename to 'BuildManager'
public class RoomManager : MonoBehaviour {
	[Required] public Room roomEnterance;
	[Required] public Button construction_uiBuild;
	[Required] public Button construction_uiCancel;
	[Required] public Button construction_uiRotateClockwise;
	[Required] public Button construction_uiRotateCounterClockwise;

	private int spotConnectLayer;
	private int spotMonsterLayer;
	private int spotLootLayer;

	[Readonly] public ConstructMode currentConstructMode;
	[Readonly] public int rotationIndex;
    [Readonly] public Buildable currentBlueprint;
    [Readonly] public Spot currentTargetConnection;

	public static RoomManager instance { get; private set; }
	public void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one RoomManager in the scene.");
		}

		instance = this;

		this.spotConnectLayer = LayerMask.NameToLayer("Connect Spot");
		this.spotMonsterLayer = LayerMask.NameToLayer("Monster Spot");
		this.spotLootLayer = LayerMask.NameToLayer("Loot Spot");
	}

	public void Start() {
		this.SetConstructMode(ConstructMode.Nothing);
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

		var canConstruct = (this.currentConstructMode != ConstructMode.Nothing);
		this.construction_uiBuild.gameObject.SetActive(canConstruct);
		this.construction_uiCancel.gameObject.SetActive(canConstruct);
		this.construction_uiRotateClockwise.gameObject.SetActive(canConstruct);
		this.construction_uiRotateCounterClockwise.gameObject.SetActive(canConstruct);
		if (!canConstruct) {
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
		this.currentBlueprint.UseNextSpot();
		this.UpdateBlueprint();
	}

	public void OnBlueprint_RotateCounterClockwise() {
        this.currentBlueprint.UsePreviousSpot();
        this.UpdateBlueprint();
    }

	public void OnBlueprint_Build() {
        this.currentBlueprint.BuildBlueprint();
		this.currentBlueprint = null;
        this.currentTargetConnection = null;

        RoomCardManager.instanceApplied.selectedCard.PlayCard();
	}

	public void OnBlueprint_Cancel() {
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

        this.construction_uiBuild.interactable = true;
        this.currentBlueprint.ShowBlueprint(this.currentTargetConnection);
    }

	public void RemoveBlueprint() {
        if (this.currentBlueprint != null) {
			this.currentBlueprint.UnloadBlueprint();
			this.currentBlueprint = null;
        }
    }
}
