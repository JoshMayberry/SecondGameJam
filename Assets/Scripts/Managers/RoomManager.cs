using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;

public class RoomManager : MonoBehaviour {
	[Required] public Room roomPrefab;
	[Required] public Sprite wallSprite;
	[Required] public Sprite doorNSprite;
	[Required] public Sprite doorWSprite;
	[Required] public Sprite doorESprite;
	[Required] public Sprite doorSSprite;
	[Required] public BuildSpot buildSpotPrefab;

	private UnitySpawner<Room> roomSpawner;

	public static RoomManager instance { get; private set; }
	private void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one RoomManager in the scene.");
		}

		instance = this;
	}

	public void SpawnRoom(RoomCard roomData, Vector2 position) {
		Room room = this.roomSpawner.Spawn(position, this.transform);
		room.setCard(roomData);
	}
}
