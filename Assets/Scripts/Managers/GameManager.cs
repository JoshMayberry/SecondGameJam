using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using jmayberry.CustomAttributes;
using jmayberry.GeneralInfrastructure.Manager;
using BehaviorDesigner.Runtime;
using Pathfinding;
using Unity.VisualScripting;

public class MyGameManager : GameManager {
	public static MyGameManager myInstance { get; private set; }
	public override void Awake() {
		base.Awake();

		if (myInstance != null) {
			Debug.LogError("Found more than one MyGameManager in the scene.");
		}

		myInstance = this;
	}

	public void Pause(bool state) {
		this.is_paused = state;

		if (HeroWaveManager.instance != null) {
			HeroWaveManager.instance.is_paused = this.is_paused;
			foreach (var spawnling in HeroWaveManager.instance.spawner) {
				spawnling.aiPath.canMove = !this.is_paused;
			}
		}

		if (BehaviorManager.instance != null) {
			BehaviorManager.instance.UpdateInterval = (this.is_paused ? UpdateIntervalType.Manual : UpdateIntervalType.EveryFrame);
		}

		if (TutorialManager.instance != null) {
			TutorialManager.instance.tutorialHero.aiPath.canMove = !this.is_paused;
		}
	}
}
