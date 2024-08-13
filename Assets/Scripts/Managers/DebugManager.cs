using jmayberry.CardDeck;
using jmayberry.CustomAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {

	[Required] public PileHandRoom PileHand;
	[Required] public PileDrawRoom PileDraw;
	[Required] public PileDiscardRoom PileDiscard;

	public static DebugManager instance { get; private set; }
	private void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one DebugManager in the scene.");
		}

		instance = this;
	}

	public void StartNormal() {
		TutorialManager.instance.StartScene(TutorialSceneNames.None);
	}

	public void StartIntro() {
		TutorialManager.instance.StartScene(TutorialSceneNames.Intro);
	}

	public void TogglePause() {
		MyGameManager.myInstance.Pause(!MyGameManager.myInstance.is_paused);
	}

	public void RefreshCardFlexalon() {
		this.PileHand.UpdateFlexalonGap();
	}
}
