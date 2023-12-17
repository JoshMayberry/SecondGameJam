using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class Room : Buildable {
    [SerializeField] private bool drawGizmos = false;
    [SerializeField] private float checkDistance = 1.25f; // How far in front of the spot to check
    [SerializeField] private Vector2 checkSize = new Vector2(1.25f, 1.25f); // How big of an area to check

    [Header("Enterence Only")]
	public Transform enterHere;

    void OnDrawGizmos() {
        if (!this.drawGizmos || !this.enabled) {
            return;
        }

        Gizmos.color = Color.red;
        foreach (Spot spot in this.connectSpotList) {
            if (spot == this.blueprintConnection) {
                continue;
            }

            Vector2 checkPosition = this.getEnoughSpacePosition(spot);
            Gizmos.DrawWireCube(checkPosition, this.checkSize);
            Gizmos.DrawLine(spot.transform.position, checkPosition);
        }

        Bounds bounds = this.GetComponent<Collider2D>().bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size * 1.1f); // Increase the size a bit so it does not get covered up by a box collider
    }

    public override bool IsPlacementAcceptable() {
        if (!base.IsPlacementAcceptable()) {
            return false;
        }

        if (!this.detectIfEnoughSpace()) {
            return false;
        }

        if (!this.detectIfNoBlockedExits()) {
            return false;
        }

        return true;
    }

    private bool detectIfEnoughSpace() {
        Collider2D myCollider = this.GetComponent<Collider2D>();
        if (myCollider == null) {
            Debug.LogWarning("No Collider2D found on the object");
            return false;
        }

        Bounds bounds = myCollider.bounds;
        Vector2 size = bounds.size;
        Vector2 position = bounds.center;

        Collider2D[] otherColliderList = Physics2D.OverlapBoxAll(position, size, myCollider.transform.eulerAngles.z);
        foreach (var otherCollider in otherColliderList) {
            if ((otherCollider != null) && (otherCollider.gameObject != this.gameObject)) {
                Debug.Log($"There is not enough space for this room; {otherCollider.name}; {otherCollider.tag}");
                return false;
            }
        }

        return true;
    }

    private Vector2 getEnoughSpacePosition(Spot spot) {
        Vector2 directionToCheck = spot.transform.up;
        return spot.transform.position + (Vector3)(directionToCheck * this.checkDistance);
    }

    private bool detectIfNoBlockedExits() {
        foreach (Spot spot in this.connectSpotList) {
            if (spot == this.blueprintConnection) {
                Debug.Log("Skipping blueprint connection");
                continue; // Do not check the spot that we are building from
            }

            Collider2D[] colliderList = Physics2D.OverlapBoxAll(this.getEnoughSpacePosition(spot), this.checkSize, spot.transform.eulerAngles.z);
            foreach (var collider in colliderList) {
                if ((collider != null) && (collider.gameObject != this.gameObject)) {
                    Debug.Log("There is something in the way of a build spot");
                    return false;
                }
            }
        }
        return true;
    }
}
