using UnityEngine;
using UnityEngine.UI;
using TMPro;

using jmayberry.Spawner;
using jmayberry.CustomAttributes;

public class HeroReview : MonoBehaviour, ISpawnable {
    [Readonly] public int score;

    [Required] public TMP_Text scoreText;
    [Required] public TMP_Text contentsText;
    [Required] public Image heroImage;

    public void OnSpawn(object spawner) {}

    public void OnDespawn(object spawner) {}

    public void PostReview(HeroType heroType, int score, string contents) {
        this.score = score;
        this.scoreText.text = $"{score}";
        this.contentsText.text = contents;

        this.heroImage.sprite = ReviewBoard.instance.spriteCatalogue[heroType];
    }
}
