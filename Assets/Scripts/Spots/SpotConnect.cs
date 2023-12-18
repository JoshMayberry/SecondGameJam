using UnityEngine;

using jmayberry.CustomAttributes;

//public enum SpotConnectType {
//	Unknown,
//	North,
//	East,
//	South,
//	West,
//}

public class SpotConnect : Spot {
    //[Required] public SpotConnectType type;

    public override void OnMouseDown() {
		RoomManager.instance.SwapSpot(this);
	}
}
