using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grave : MonoBehaviour {
    public float stayOnFieldFor = 8;
    public HeroType heroType;

    void Start() {
        StartCoroutine(this.RemoveGrave());
    }

    IEnumerator RemoveGrave() {
        yield return new WaitForSeconds(this.stayOnFieldFor);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        switch (other.tag) {
            case "Hero":
                Hero hero = other.GetComponent<Hero>();
                if (hero != null) {
                    hero.SeeGrave(this.heroType);
                }
                break;
        }
    }
}
