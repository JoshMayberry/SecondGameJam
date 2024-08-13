using UnityEngine;
using System.Collections;
using jmayberry.EventSequencer;
using jmayberry.TypewriterHelper.Entries;
using System.Security.Cryptography.X509Certificates;
using System;
using UnityEngine.SocialPlatforms.Impl;

public enum MyActionType {
	None = 0,
	HeroSetType = 5,
	HeroEntersDungeon = 1,
	HeroLeavesDungeon = 2,
	EnableFeature = 3,
	DungeonSetState = 4,
}

public class MyActionEntry : BaseActionEntry<MyActionType> {
	public int ActionNumber;
	[TextArea] public string ActionText;

	public override IEnumerator DoAction(IContext context) {
		switch (this.action) {
			case MyActionType.HeroEntersDungeon:
				Debug.Log("Make the hero enter the dungeon");
				break;

			case MyActionType.HeroLeavesDungeon:
				//yield return TutorialManager.instance.tutorialHero.ExitDungeon();

				if (this.ActionText != "") {
					ReviewBoard.instance.LeaveReview(TutorialManager.instance.tutorialHero.heroData.type, ActionNumber, ActionText);
				}
				break;

			case MyActionType.HeroSetType:
				HeroWaveManager.instance.HeroDataCatalog.TryGetValue(this.ActionText, out HeroData heroData);
				if (heroData == null) {
					Debug.LogError($"Unknown hero type: '{this.ActionText}'");
					break;
				}

				TutorialManager.instance.tutorialHero.SetData(heroData);
				break;

			case MyActionType.EnableFeature:
				bool activeState = (this.ActionNumber != -1);
				foreach (var featureName in this.ActionText.Split("\n")) {
					switch (featureName) {
						case "RoomManager":
							RoomManager.instance.roomEnterance.gameObject.SetActive(activeState);
							break;
						case "HeroWaveManager":
							HeroWaveManager.instance.gameObject.SetActive(activeState);
							break;
						case "RoomCardManager":
							RoomCardManager.instance.gameObject.SetActive(activeState);
							break;
						case "ReviewBoard":
							ReviewBoard.instance.gameObject.SetActive(activeState);
							break;
						case "CanvasCard":
							TutorialManager.instance.CanvasCard.gameObject.SetActive(activeState);
							break;
						case "CanvasConstruction":
							TutorialManager.instance.CanvasConstruction.gameObject.SetActive(activeState);
							break;
						case "CanvasReview":
							TutorialManager.instance.CanvasReview.gameObject.SetActive(activeState);
							break;
						case "CanvasChat":
							TutorialManager.instance.CanvasChat.gameObject.SetActive(activeState);
							break;
						default:
							Debug.LogError($"Unknown feature to enable: '{featureName}'");
							break;
					}
				}
				break;

			case MyActionType.DungeonSetState:
				if (Enum.TryParse(this.ActionText, out DungeonState dungeonState)) {
					RoomManager.instance.SetDungeonState(dungeonState);
				}
				else {
					Debug.LogError($"Unknown dungeon state: '{this.ActionText}'");
				}
				break;
		}

		yield return null;
	}
}
