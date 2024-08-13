using UnityEngine;

using jmayberry.CustomAttributes;

public class UiButtonToolbar : MonoBehaviour {
	[Readonly] [SerializeField] private UiButton currentToolbarItem;

	internal void OnClickItem(UiButton newToolbarItem) {
		if (this.currentToolbarItem != null) {
			this.currentToolbarItem.TurnOff();

			if (this.currentToolbarItem == newToolbarItem) {
				this.currentToolbarItem = null;
				return;
			}
		}

		this.currentToolbarItem = newToolbarItem;
		this.currentToolbarItem.TurnOn();
	}
}