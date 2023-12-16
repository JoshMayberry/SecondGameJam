using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BuildSpot : MonoBehaviour {

    void OnMouseDown() {
        Debug.Log("Clicked!");
    }
}
