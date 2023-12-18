using UnityEngine;
using System;

using jmayberry.CustomAttributes;

public interface IBuildLocation {
	Spot GetBuildSpot(int index);
}

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class Spot : MonoBehaviour {
	[Readonly] public Buildable container;
	[Readonly] public bool isUsed;

	abstract public void OnMouseDown();

	public void MarkUsed() {
		this.isUsed = true;
		this.gameObject.SetActive(false);
	}

	public void SetVisibility(bool state) {
		this.gameObject.SetActive(state && !this.isUsed);
	}

	public Vector3 GetPosition() {
		return this.transform.position;
	}

	public Quaternion GetRotation() {
		return this.transform.rotation;
	}

	public void SetContainer(Buildable container) {
		this.container = container;
	}
}
