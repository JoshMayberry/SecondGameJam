using UnityEngine;

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("jmayberry")]
[TaskDescription("Hero spends time at the venue.")]
public class EnjoyVenue : Action {
	public SharedGameObject venueObject;
	public SharedGameObjectList finishedObjectList;
	public float venueTimeFallback = 2f; // In seconds
	[SerializeField] private Hero hero;
	[SerializeField] private Buildable buildable;
	[SerializeField] private float timeStarted;
	[SerializeField] private float timeRequired;


	public override void OnAwake() {
		base.OnAwake();
		this.hero = this.GetComponent<Hero>();
	}

	public override void OnStart() {
		if (this.hero == null) {
			Debug.LogError($"No 'Hero' component found on '{this.gameObject?.name}'");
			return;
		}

		if (this.venueObject.Value == null) {
			Debug.LogError($"No 'venueObject' given");
			return;
		}

		this.buildable = this.venueObject.Value.GetComponent<Buildable>();
		if (this.buildable == null) {
			Debug.LogError($"No 'Buildable' component found on '{this.venueObject.Value?.name}'");
			return;
		}

		this.timeStarted = Time.time;
		var timeRequiredMin = this.buildable.cardData.minTime;
		var timeRequiredMax = this.buildable.cardData.maxTime;
		this.timeRequired = ((timeRequiredMin == 0) || (timeRequiredMax == 0)) ? this.venueTimeFallback : Random.Range(timeRequiredMin, timeRequiredMax);

		this.hero.UpdateSpriteEffect_IsBusy(true);

		switch (this.buildable.cardData.cardType) {
			case CardType.Loot:
				// TODO: Make the hero jump up and down or something
				this.hero.uiInfo.SetRow("Status", "Getting Loot");
				break;

			case CardType.Monster:
				// TODO: Play hero and monster fight animations
				this.hero.uiInfo.SetRow("Status", "Fighting Monster");
				break;

			default:
				// TODO: Make the hero pace around a bit
				this.hero.uiInfo.SetRow("Status", "Enjoying Venue");
				break;
		}
	}

	public override TaskStatus OnUpdate() {
		if ((this.hero == null) || (this.buildable == null)) {
			return TaskStatus.Failure;
		}

		float percentFinished = (Time.time - this.timeStarted) / this.timeRequired;
		if (percentFinished < 1) {
			this.hero.UpdateVenueTimePercentage(percentFinished);
			return TaskStatus.Running;
		}

		this.hero.UpdateVenueTimePercentage(1);
		this.hero.UpdateSpriteEffect_IsBusy(false);
		this.buildable.IsUsed(this.hero);

		this.finishedObjectList.Value.Add(this.venueObject.Value);
		this.venueObject.Value = null;
		return TaskStatus.Success;
	}

}
