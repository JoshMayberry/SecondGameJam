using UnityEngine;
using jmayberry.CustomAttributes;

public enum SpotMonsterType {
	Unknown,
	Mob,
	MiniBoss,
	Guest,
	DemonLord,
}


public class SpotMonster : Spot {
    [Required] public SpotMonsterType type;

	public override void OnMouseDown() {
        RoomManager.instance.SwapSpot(this);
	}
}
