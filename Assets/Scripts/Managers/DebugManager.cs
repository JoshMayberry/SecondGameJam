using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {

	public static DebugManager instance { get; private set; }
	private void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one DebugManager in the scene.");
		}

		instance = this;
    }

    public void SetBuildSpotsOn() {
        RoomManager.instance.SetBuildSpotVisibility(true);
    }

    public void SetBuildSpotsOff() {
        RoomManager.instance.SetBuildSpotVisibility(false);
    }
}
