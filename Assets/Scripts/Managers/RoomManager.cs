using UnityEngine;

using jmayberry.CustomAttributes;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour {
	[Required] public Room roomEnterance;
	[Required] public Button construction_uiBuild;
	[Required] public Button construction_uiCancel;
	[Required] public Button construction_uiRotateClockwise;
	[Required] public Button construction_uiRotateCounterClockwise;

	private int buildSpotLayer;
	private int connectSpotLayer;

	[Readonly] public bool canConstruct;
	[Readonly] public int connectionIndex;
	[Readonly] public Room blueprintRoom;
	[Readonly] public ConnectSpot targetConnection;

	public static RoomManager instance { get; private set; }
	public void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one RoomManager in the scene.");
		}

		instance = this;

		this.buildSpotLayer = LayerMask.NameToLayer("Build Spot");
		this.connectSpotLayer = LayerMask.NameToLayer("Connect Spot");
	}

	public void Start() {
		this.SetBuildSpotVisibility(false);
		this.SetConnectSpotVisibility(false);
		this.ConstructionUI_SetVisibility(false);
	}

	// Use: https://discussions.unity.com/t/edit-camera-culling-mask/55812/3
	public void SetBuildSpotVisibility(bool state) {
		if (state) {
			Camera.main.cullingMask |= 1 << this.buildSpotLayer;
		}
		else {
			Camera.main.cullingMask &= ~(1 << this.buildSpotLayer);
		}
	}
	public void SetConnectSpotVisibility(bool state) {
		if (state) {
			Camera.main.cullingMask |= 1 << this.connectSpotLayer;
		}
		else {
			Camera.main.cullingMask &= ~(1 << this.connectSpotLayer);
		}
	}

	public void ConstructionUI_SetVisibility(bool state) {
		this.canConstruct = state;
		if (!state) {
			this.targetConnection = null;
			this.RemoveBlueprint();
        }

		this.construction_uiBuild.gameObject.SetActive(state);
		this.construction_uiCancel.gameObject.SetActive(state);
		this.construction_uiRotateClockwise.gameObject.SetActive(state);
		this.construction_uiRotateCounterClockwise.gameObject.SetActive(state);

		this.construction_uiBuild.interactable = false;
		this.construction_uiCancel.interactable = true;
		this.construction_uiRotateClockwise.interactable = false;
		this.construction_uiRotateCounterClockwise.interactable = false;
	}

	public void ConstructionUI_SetCanBuild(bool state) {
		this.construction_uiBuild.interactable = state;
	}
	public void ConstructionUI_SetCanCancel(bool state) {
		this.construction_uiCancel.interactable = state;
	}
	public void ConstructionUI_SetCanRotate(bool state) {
		this.construction_uiRotateClockwise.interactable = state;
		this.construction_uiRotateCounterClockwise.interactable = state;
	}

	public void OnBlueprint_RotateClockwise() {
		if (this.blueprintRoom == null) {
			return;
		}

		if (this.blueprintRoom.connectSpots.Count - 1 <= this.connectionIndex) {
			this.connectionIndex = 0;
		} else {
			this.connectionIndex++;
		}

		this.UpdateBlueprint();
	}

	public void OnBlueprint_RotateCounterClockwise() {
		if (this.blueprintRoom == null) {
			return;
		}

		if (this.blueprintRoom.connectSpots.Count - 1 < this.connectionIndex) {
			this.connectionIndex = 0;
		}
        else if (this.connectionIndex <= 0) {
			this.connectionIndex = this.blueprintRoom.connectSpots.Count - 1;
        }
        else {
			this.connectionIndex--;
		}

		this.UpdateBlueprint();
	}

	public void OnBlueprint_Build() {

		// Update connection spots
        this.blueprintRoom.connectSpots[this.connectionIndex].MarkUsed();
		this.blueprintRoom.SetConnectSpotVisibility(true);
        this.targetConnection.MarkUsed();
        this.targetConnection = null;

        this.blueprintRoom.Build();
        this.blueprintRoom = null;

        RoomCardManager.instanceApplied.selectedCard.PlayCard();
	}

	public void OnBlueprint_Cancel() {
		if (this.blueprintRoom != null) {
            this.blueprintRoom.SetConnectSpotVisibility(true);
        }

		if (this.targetConnection != null) {
			this.targetConnection.gameObject.SetActive(true);
		}

        RoomCardManager.instanceApplied.selectedCard.Deselect();
	}

	public void SwapConnectionSpot(ConnectSpot targetSpot) {
		if (!canConstruct) {
			Debug.LogWarning("Something tried to build when building was not permitted");
			return;
		}

		if (this.targetConnection != null) {
			this.targetConnection.gameObject.SetActive(true);
        }

		this.targetConnection = targetSpot;
		this.SwapBlueprint(RoomCardManager.instanceApplied.selectedCard);
	}

	public void SwapBlueprint(UiTowerCard card) {
        if (!canConstruct) {
            Debug.LogWarning("Something tried to build when building was not permitted");
            return;
        }

        this.RemoveBlueprint();

        RoomCardData cardData = (RoomCardData)card.card;
        this.blueprintRoom = Instantiate(cardData.roomPrefab, this.transform);
        this.connectionIndex = 0;

		this.ConstructionUI_SetCanRotate(cardData.canRotate);
        this.UpdateBlueprint();
    }

	public void UpdateBlueprint() {
		if (this.blueprintRoom == null) {
			Debug.LogWarning("No blueprint to update");
			return;
		}

		ConnectSpot blueprintConnection = this.blueprintRoom.connectSpots[this.connectionIndex];
        this.ConstructionUI_SetCanBuild(true);

		// Hide connect spots
        this.blueprintRoom.SetConnectSpotVisibility(false);
        this.targetConnection.gameObject.SetActive(false);

        // Line up rotation
        this.blueprintRoom.transform.rotation = Quaternion.identity;
        Quaternion targetRotation = Quaternion.Inverse(blueprintConnection.transform.rotation) * this.targetConnection.transform.rotation;
        this.blueprintRoom.transform.rotation = Quaternion.Euler(0, 0, targetRotation.eulerAngles.z + 180);

        // Line up position
        this.blueprintRoom.transform.position += this.targetConnection.transform.position - blueprintConnection.transform.position;
    }

	public void RemoveBlueprint() {
        if (this.blueprintRoom != null) {
            Destroy(this.blueprintRoom.gameObject);
        }

		// TODO: Spawn dust particles
    }
}
