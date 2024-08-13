using System;
using UnityEngine;
using UnityEngine.UI;

using jmayberry.CustomAttributes;
using UnityEngine.EventSystems;
using Flexalon;

[RequireComponent(typeof(Image))]
public class UiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
	[SerializeField] private UiButtonToolbar toolbar;
	[SerializeField] private Canvas CanvasToToggle;
	[SerializeField] private FlexalonObject FlexalonObjectToToggle;
	[Readonly] [SerializeField] private Image myImage;
	[Readonly] [SerializeField] private Material materialToUse;
	[Readonly] [SerializeField] private bool isOn;

	public void Awake() {
		this.myImage = this.GetComponent<Image>();
		this.myImage.material = new Material(this.myImage.material); // Keep the materials each buttons uses separate from each other.
		this.myImage.material.SetFloat("_Brightness", 1);
		this.TurnOff();
	}

	public void OnPointerEnter(PointerEventData eventData) {
		this.myImage.material.SetFloat("_Brightness", 2);
	}

	public void OnPointerExit(PointerEventData eventData) {
		this.myImage.material.SetFloat("_Brightness", 1);
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (this.toolbar != null) {
			this.toolbar.OnClickItem(this);
			return;
		}

		if (this.isOn) {
			this.TurnOff();
		}
		else {
			this.TurnOn();
		}
	}

	public void TurnOn() {
		this.isOn = true;
		this.myImage.material.SetFloat("_PixelOutlineFade", 1);

		if (this.CanvasToToggle != null) {
			this.CanvasToToggle.gameObject.SetActive(true);
		}

		if (this.FlexalonObjectToToggle != null) {
			this.FlexalonObjectToToggle.gameObject.SetActive(true);
			this.FlexalonObjectToToggle.SkipLayout = false;
		}
	}

	public void TurnOff() {
		this.isOn = false;
		this.myImage.material.SetFloat("_PixelOutlineFade", 0);

		if (this.CanvasToToggle != null) {
			this.CanvasToToggle.gameObject.SetActive(false);
		}

		if (this.FlexalonObjectToToggle != null) {
			this.FlexalonObjectToToggle.SkipLayout = true;
			this.FlexalonObjectToToggle.gameObject.SetActive(false);
		}
	}
}
