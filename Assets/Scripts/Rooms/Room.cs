using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;
using jmayberry.CustomAttributes;

public class Room : MonoBehaviour, ISpawnable {
	[Readonly] public RoomCardData cardData;

	public void OnDespawn(object spawner) {}

	public void OnSpawn(object spawner) {}

	public void setCardData(RoomCardData cardData) {
		this.cardData = cardData;
    }
}
