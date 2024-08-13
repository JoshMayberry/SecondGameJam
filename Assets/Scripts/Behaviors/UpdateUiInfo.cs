using UnityEngine;

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public enum UpdateUiInfoKinds {
	Hero = 0,
	Buildable = 1,
}

[TaskCategory("jmayberry")]
[TaskDescription("Updates some info on the UI.")]
public class UpdateUiInfo : Action {
	public SharedString key;
	public SharedString value;

	public SharedGameObject venueObject;
	[SerializeField] private Hero hero;
	[SerializeField] private Buildable buildable;

	public UpdateUiInfoKinds updateKind;

	public override void OnAwake() {
		base.OnAwake();
		this.hero = this.GetComponent<Hero>();
	}

	public override void OnStart() {
		switch (this.updateKind) {
			case UpdateUiInfoKinds.Hero:
				if (this.hero == null) {
					Debug.LogError($"No 'Hero' component found on '{this.gameObject?.name}'");
					return;
				}
				break;

			case UpdateUiInfoKinds.Buildable:
				if (this.venueObject.Value == null) {
					Debug.LogError($"No 'venueObject' given");
					return;
				}

				this.buildable = this.venueObject.Value.GetComponent<Buildable>();
				if (this.buildable == null) {
					Debug.LogError($"No 'Buildable' component found on '{this.venueObject.Value?.name}'");
					return;
				}
				break;
		}
	}

	public override TaskStatus OnUpdate() {
		switch (this.updateKind) {
			case UpdateUiInfoKinds.Hero:
				if (this.hero == null) {
					return TaskStatus.Failure;
				}

				this.hero.uiInfo.SetRow(key.Value, value.Value);
				break;

			case UpdateUiInfoKinds.Buildable:
				if (this.buildable == null) {
					return TaskStatus.Failure;
				}
				this.buildable.uiInfo.SetRow(key.Value, value.Value);
				break;
		}

		return TaskStatus.Success;
	}

}
