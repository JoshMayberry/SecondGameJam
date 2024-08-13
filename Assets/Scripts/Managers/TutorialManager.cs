using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;
using jmayberry.CustomAttributes;

public enum TutorialSceneNames {
	None = 0,
	Intro = 1,
}

public class TutorialManager : MonoBehaviour {
	[Header("Setup")]
	[Required] public Animator CinemamachineAnimator;
	[Required] public CinemachineVirtualCamera normalCamera;
	[Required] public CinemachineVirtualCamera tutorialCamera_1;
	[Required] public CinemachineVirtualCamera tutorialCamera_2;
	[Required] public Hero tutorialHero;

	[Required] public RectTransform CanvasCard;
	[Required] public RectTransform CanvasConstruction;
	[Required] public RectTransform CanvasChat;
	[Required] public RectTransform CanvasReview;

	[Header("Conversations")]
	[EntryFilter(Variant = EntryVariant.Event)] public EntryReference conversationIntro;

	[Header("Debugging")]
	[Readonly] bool lastUsedTutorialCamera1 = false;

	public static TutorialManager instance { get; private set; }
	private void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one TutorialManager in the scene.");
		}

		instance = this;
	}

	private void Start() {
		HeroWaveManager.instance.is_paused = true; // FOR DEBUGGING
		this.tutorialHero.SetData(HeroWaveManager.instance.HeroDataCatalog["Mage"]);
	}

	// See: [How to Blend/Switch Between Cinemachine Cameras](https://www.youtube.com/watch?v=Ri8PEbD4w8A&ab_channel=samyam)
	public void StartScene(TutorialSceneNames sceneName) {
		this.normalCamera.Priority = 0;
		this.tutorialCamera_1.Priority = 0;
		this.tutorialCamera_2.Priority = 0;

		switch (sceneName) {
			case TutorialSceneNames.None:
				this.normalCamera.Priority = 1;
				this.CinemamachineAnimator.Play("Normal Camera");
				this.StartScene_None();
				break;

			case TutorialSceneNames.Intro:
				SwitchTutorialCamera(2, new Vector3(-15.26f, 3.07f, -10f));
				this.StartScene_Intro();
				break;
		}
	}

	internal void SwitchTutorialCamera(float orthoSize, Vector3 position) {
		CinemachineVirtualCamera tutorialCamera = (lastUsedTutorialCamera1 ? this.tutorialCamera_2 : this.tutorialCamera_1);

		tutorialCamera.Priority = 1;
		tutorialCamera.m_Lens.OrthographicSize = orthoSize;
		tutorialCamera.transform.position = position;

		this.CinemamachineAnimator.Play(lastUsedTutorialCamera1 ? "Tutorial Camera 2" : "Tutorial Camera 1");
		this.lastUsedTutorialCamera1 = !this.lastUsedTutorialCamera1;
	}

	private void StartScene_None() {
		HeroWaveManager.instance.is_paused = false;
		RoomManager.instance.roomEnterance.gameObject.SetActive(true);
		RoomCardManager.instance.gameObject.SetActive(true);
		ReviewBoard.instance.gameObject.SetActive(true);
		DialogManager.instance.gameObject.SetActive(true);
		this.CanvasCard.gameObject.SetActive(true);
		this.CanvasConstruction.gameObject.SetActive(true);
		this.CanvasChat.gameObject.SetActive(true);
		this.CanvasReview.gameObject.SetActive(true);
	}

	private void StartScene_Intro() {
		HeroWaveManager.instance.is_paused = true;
		RoomManager.instance.roomEnterance.gameObject.SetActive(false);
		HeroWaveManager.instance.gameObject.SetActive(false);
		RoomCardManager.instance.gameObject.SetActive(false);
		ReviewBoard.instance.gameObject.SetActive(false);
		this.CanvasCard.gameObject.SetActive(false);
		this.CanvasConstruction.gameObject.SetActive(false);
		this.CanvasReview.gameObject.SetActive(false);

		DialogManager.instance.gameObject.SetActive(true);
		this.CanvasChat.gameObject.SetActive(true);

		DialogManager.instance.TryStartSequence(conversationIntro);


		// Hero spawns in and expresses disappointment about the current state of the dungeon

		// Hero leaves and posts a review about his feelings towards the dungeon

		// Demon Lord appears and tells the player to look at the posted review

		// Player looks at the review and the Demon Lord is upset at the hero, then decides to improve the dungeon

		// Player builds a room

		// Player places a monster in the room

		// Hero comes back and complains about lack of loot

		// Player places some loot

		// Normal game starts
	}
}


public class TutorialScene {
	[Required] public float start_lerpDuration = 1f;
	[Required] public float camera_orthoSize;
	[Required] public Vector3 camera_Position;

	internal void SetCameraParams(CinemachineVirtualCamera virtualCamera) {
		virtualCamera.m_Lens.OrthographicSize = camera_orthoSize;
		virtualCamera.transform.position = camera_Position;
	}
}
