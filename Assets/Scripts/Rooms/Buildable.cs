using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;
using jmayberry.CustomAttributes;

// TODO: Split construction functions and Buildable functions into separate files? Prefabs would need both...

public class Buildable : MonoBehaviour, ISpawnable {
	[SerializeField] public List<Spot> connectSpotList = new List<Spot>();
	[SerializeField] public List<Spot> upgradeSpotList = new List<Spot>();
    [SerializeField] public List<Sprite> upgradeSprites = new List<Sprite>();
	[Required][SerializeField] private SpriteRenderer spriteRenderer;

    [Readonly] public RoomCard card;
	public RoomCardData cardData;
	[Readonly] private int upgradeIndex = 0;
	[Readonly] private int connectSpotIndex;
    [Readonly] public bool isBlueprint = true;
	[Readonly] public Spot targetConnection;
	[Readonly] public Spot blueprintConnection;
	[Readonly] public Buildable constructed;
	[Readonly] public Buildable blueprintBuildingMe;

    public virtual void Awake() {
		if (this.connectSpotList.Count == 0) {
			Debug.LogWarning("Buildable missing connection spots");
        }

        foreach (Spot buildSpot in this.connectSpotList) {
            buildSpot.SetContainer(this);
        }

        foreach (Spot buildSpot in this.upgradeSpotList) {
            buildSpot.SetContainer(this);
        }

		this.UpdateSprite();
    }

    public bool HasMultipleSpots() {
		if (!this.isBlueprint) {
			Debug.LogWarning("Do not plan using a construction; plan using a blueprint.");
			return false;
		}

		return (this.connectSpotList.Count > 1);
	}

	public void SetSpotVisibility(bool state) {
		if (this.isBlueprint) {
			Debug.LogWarning("Do not work on the blueprint; work on a construction.");
			return;
		}

		foreach (Spot availableSpot in this.connectSpotList) {
			availableSpot.SetVisibility(state);
		}
	}

	public void UseNextSpot() {
		if (!this.isBlueprint) {
			Debug.LogWarning("Do not plan using a construction; plan using a blueprint.");
			return;
		}

		if (this.connectSpotList.Count - 1 <= this.connectSpotIndex) {
			this.connectSpotIndex = 0;
		}
		else {
			this.connectSpotIndex++;
		}
	}

	public void UsePreviousSpot() {
		if (!this.isBlueprint) {
			Debug.LogWarning("Do not plan using a construction; plan using a blueprint.");
			return;
		}

		if (this.connectSpotList.Count - 1 < this.connectSpotIndex) {
			this.connectSpotIndex = 0;
		}
		else if (this.connectSpotIndex <= 0) {
			this.connectSpotIndex = this.connectSpotList.Count - 1;
		}
		else {
			this.connectSpotIndex--;
		}
	}

	public Spot GetBuildSpot(int index) {
		if (this.isBlueprint) {
			Debug.LogWarning("Do not work on the blueprint; work on a construction.");
			return null;
		}

		if (this.connectSpotList.Count - 1 < index) {
			Debug.LogWarning($"index exceeded number of spots; {index}");
			return this.connectSpotList[0];
		}
		
		return this.connectSpotList[index];
	}

	public void SetCard(RoomCard card) {
		this.SetCard(card, (RoomCardData)card.card);
	}

	public void SetCard(RoomCard card, RoomCardData cardData) {
		this.card = card;
		this.cardData = cardData;
		this.connectSpotIndex = 0;
		this.UnloadBlueprint();
	}

	public virtual bool IsPlacementAcceptable() {
		if (this.isBlueprint) {
			Debug.LogWarning("Do not work on the blueprint; work on a construction.");
			return false;
		}

		return true;
	}

	public virtual void OnDespawn(object spawner) { }

	public virtual void OnSpawn(object spawner) { }

	public virtual bool ShowBlueprint(Spot targetConnection) {
		if (!this.isBlueprint) {
			Debug.LogWarning("Do not plan using a construction; plan using a blueprint.");
			return false;
		}

		this.targetConnection = targetConnection;

		// Be sure to work on the constructed buildable- not on the blueprint
		if (this.constructed == null) {
			this.constructed = Instantiate(this);
			this.constructed.cardData = this.cardData;
			this.constructed.isBlueprint = false;
			this.constructed.blueprintBuildingMe = this;
        }

        this.blueprintConnection = this.constructed.GetBuildSpot(this.connectSpotIndex);
		this.constructed.targetConnection = this.targetConnection;

        // Hide connect spots
        this.constructed.SetSpotVisibility(false);
		this.targetConnection.SetVisibility(false);

		// Line up rotation
		this.constructed.transform.rotation = Quaternion.identity;
		Quaternion targetRotation = this.targetConnection.GetRotation() * Quaternion.Inverse(this.blueprintConnection.GetRotation());
		this.constructed.transform.rotation = Quaternion.Euler(0, 0, targetRotation.eulerAngles.z + 180);

		// Line up position
		this.constructed.transform.position += this.targetConnection.GetPosition() - this.blueprintConnection.GetPosition();

		return this.constructed.IsPlacementAcceptable();
	}

	public virtual void UnloadBlueprint() {
		if (!this.isBlueprint) {
			Debug.LogWarning("Do not plan using a construction; plan using a blueprint.");
			return;
		}

		if (this.targetConnection != null) {
			this.targetConnection.SetVisibility(true);
		}

		if (this.constructed != null) {
			Destroy(this.constructed.gameObject);
			this.constructed = null;

			// TODO: Spawn teleport lines
		}
	}

	public virtual void BuildBlueprint() {
		if (!this.isBlueprint) {
			Debug.LogWarning("Do not plan using a construction; plan using a blueprint.");
			return;
		}

		this.targetConnection.MarkUsed();
		this.blueprintConnection.MarkUsed();
		this.constructed.SetSpotVisibility(true);
		this.constructed.enabled = false; // Turn off the Buildable Script now that it is built
		this.constructed = null;

        // TODO: Spawn dust particles
        // TODO: Change transparency or something to be solid
    }

    public void DoUpgrade(Spot spot) {
        spot.MarkUsed();

		this.upgradeIndex++;
		this.UpdateSprite();
    }

	public void UpdateSprite() {
        if (this.upgradeSprites.Count - 1 >= this.upgradeIndex) {
            this.spriteRenderer.sprite = this.upgradeSprites[this.upgradeIndex];
        }
    }
}
