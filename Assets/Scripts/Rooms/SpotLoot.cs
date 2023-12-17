using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CustomAttributes;

public enum SpotLootType {
	Unknown,
	All,
	Mage,
	Mage_Black,
	Mage_White,
	Fighter,
	Fighter_Fists,
	Fighter_Sword,
}

public class SpotLoot : Spot {
    [Required] public SpotLootType type;
   
	public override void OnMouseDown() {
        RoomManager.instance.SwapSpot(this);
    }
}
