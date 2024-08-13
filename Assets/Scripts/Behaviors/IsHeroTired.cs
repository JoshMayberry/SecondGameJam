using UnityEngine;

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("jmayberry")]
[TaskDescription("If the hero is tired yet or not.")]
public class IsHeroTired : Conditional {
	[SerializeField] private Hero hero;
	[SerializeField] private HeroTiredReason tiredReason;

	public override void OnAwake() {
		base.OnAwake();
		this.hero = this.GetComponent<Hero>();
	}

	public override void OnStart() {
		this.tiredReason = HeroTiredReason.None;
		if (this.hero == null) {
			Debug.LogError($"No 'Hero' component found on '{this.gameObject?.name}'");
			return;
		}
	}

	public override TaskStatus OnUpdate() {
		if ((this.hero == null)) {
			return TaskStatus.Failure;
		}

		this.tiredReason = this.hero.GetTiredReason();
		this.hero.uiInfo.SetRow("Tired Reason", this.tiredReason);

		if (this.tiredReason != HeroTiredReason.None) {
			return TaskStatus.Success;
		}

		return TaskStatus.Failure;
	}

}
