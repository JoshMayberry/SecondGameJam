using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Buildable {
	private void OnTriggerEnter(Collider other) {
		switch(other.tag) {
			case "Hero":
				Hero hero = other.GetComponent<Hero>();
				if (hero != null) {
					hero.Damage(this.cardData.attackDamage);
				}
				break;
		}
	}
}
