using UnityEngine;

using jmayberry.CustomAttributes;

public enum ConnectSpotType {
    Unknown,
    North,
    East,
    South,
    West,
}

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ConnectSpot : MonoBehaviour {
    [Required] public ConnectSpotType type;
    [Readonly] public Room room;
    [Readonly] public bool isUsed;

    void OnMouseDown() {
        RoomManager.instance.SwapConnectionSpot(this);
    }

    public void MarkUsed() {
        this.isUsed = true;
        this.gameObject.SetActive(false);
    }
}
