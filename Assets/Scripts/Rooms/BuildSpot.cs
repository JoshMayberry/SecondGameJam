using UnityEngine;
using jmayberry.CustomAttributes;

public enum BuildSpotType {
    Unknown,
    Monster,
    Loot,
}

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BuildSpot : MonoBehaviour {
    [Required] public BuildSpotType type;
    [Readonly] public Room room;
    [Readonly] public bool isUsed;

    void OnMouseDown() {
        Debug.Log("Clicked Build!");
    }
}
