using UnityEngine;

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;

[TaskCategory("jmayberry")]
[TaskDescription("Hero despawns.")]
public class Despawn : Action {
	[SerializeField] private Hero hero;

	public override void OnAwake() {
		base.OnAwake();
		this.hero = this.GetComponent<Hero>();
	}

	public override void OnStart() {
		if (this.hero == null) {
			Debug.LogError($"No 'Hero' component found on '{this.gameObject?.name}'");
			return;
		}
	}

	public override TaskStatus OnUpdate() {
		if (this.hero == null) {
			return TaskStatus.Failure;
		}

		if (this.hero.spawner != null) {
			this.hero.spawner.Despawn(this.hero);
		}
		else {
			GameObject.Destroy(this.hero.gameObject);
			this.hero = null;
		}
		return TaskStatus.Success;
	}
}
