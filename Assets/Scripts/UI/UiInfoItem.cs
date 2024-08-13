using UnityEngine;

using Flexalon;
using jmayberry.CustomAttributes;
using jmayberry.Spawner;

public class UiInfoItem : MonoBehaviour, ISpawnable {
	[Required] public TMPro.TextMeshProUGUI textKey;
	[Required] public TMPro.TextMeshProUGUI textValue;

	[Readonly] public FlexalonObject flexalonObject;

	public void Awake() {
		this.flexalonObject = this.GetComponent<FlexalonObject>();
	}

	public void OnDespawn(object spawner) {
		this.flexalonObject.SkipLayout = true;
	}

	public void OnSpawn(object spawner) {
		this.flexalonObject.SkipLayout = false;
	}

	public UiInfoItem SetKey(string text) {
		this.textKey.text = $"{text}:";
		return this;
	}

	public UiInfoItem SetValue(object text) {
		this.textValue.text = $"{text}";
		return this;
	}
}
