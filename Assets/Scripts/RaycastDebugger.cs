using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDebugger : MonoBehaviour {
    void Update() {
        if (Input.GetMouseButtonDown(0)) { // Checks if the left mouse button was clicked
            CastRay();
        }
    }

    void CastRay() {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

        Debug.Log($"Raycast hit {hits.Length} objects:");
        foreach (var hit in hits) {
            Debug.Log(hit.collider.gameObject.name + " at " + hit.point);
        }
    }
}
