using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {
	public RoomCard testRoom;

	public static DebugManager instance { get; private set; }
	private void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one DebugManager in the scene.");
		}

		instance = this;
	}

	public void OnTestSpawnRoom() {
		RoomManager.instance.SpawnRoom(testRoom, this.transform.position);
	}

	public void OnTestSpawnCard() {
		
	}
}
