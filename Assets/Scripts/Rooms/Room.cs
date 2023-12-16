using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;
using jmayberry.CustomAttributes;

public class Room : MonoBehaviour, ISpawnable {
	[Readonly] public RoomCardData cardData;
	[Readonly] public bool isBlueprint = true;

    public List<ConnectSpot> connectSpots = new List<ConnectSpot>();
	public List<BuildSpot> buildSpots = new List<BuildSpot>();

    [Header("Enterence Only")]
	public Transform enterHere;

    public void Awake() {
        foreach (ConnectSpot connectSpot in this.connectSpots) {
            connectSpot.room = this;
        }

        foreach (BuildSpot buildSpot in this.buildSpots) {
            buildSpot.room = this;
        }
    }

    public void OnDespawn(object spawner) {}

	public void OnSpawn(object spawner) {}

	public void setCardData(RoomCardData cardData) {
		this.cardData = cardData;
    }

    public void SetConnectSpotVisibility(bool state) {
        foreach (ConnectSpot availableSpot in this.connectSpots) {
            if (!availableSpot.isUsed) {
                availableSpot.gameObject.SetActive(state);
            }
        }
    }

    public void Build() {
		this.isBlueprint = false;
        // TODO: Spawn dust particles
        // TODO: Change transparency or something to be solid
    }
}
