using System;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;
using jmayberry.CustomAttributes;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;

public class ReviewBoard : MonoBehaviour {
	[Required][SerializeField] private Transform heroReviewContainer;
	[Required][SerializeField] private HeroReview heroReviewPrefab;
	[Required][SerializeField] private Image toggleShowReviewsButton;
	[Required][SerializeField] private Transform reviewBoardContainer;
    private UnitySpawner<HeroReview> heroReviewSpawner;
	[SerializedDictionary("Hero Type", "Review")] public SerializedDictionary<HeroType, List<HeroReview>> reviews = new SerializedDictionary<HeroType, List<HeroReview>>();
	[SerializedDictionary("Hero Type", "Sprite")] public SerializedDictionary<HeroType, Sprite> spriteCatalogue = new SerializedDictionary<HeroType, Sprite>();

	[Readonly][SerializeField] private bool isShowingBoard;
    public int keepReviewLimit = 3;
	HeroReview bestReview;

	public static ReviewBoard instance { get; private set; }
	public void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one ReviewBoard in the scene.");
		}

		instance = this;

		this.heroReviewSpawner = new UnitySpawner<HeroReview>(this.heroReviewPrefab);
	}

	internal void LeaveReview(HeroType heroType, int score, string text) {
		HeroReview review = this.heroReviewSpawner.Spawn(Vector3.zero, this.heroReviewContainer);
		review.PostReview(heroType, score, text);

		if (this.bestReview == null) {
			this.bestReview = review;
			this.toggleShowReviewsButton.gameObject.SetActive(true);
        }
		else if (review.score > this.bestReview.score) {
            this.bestReview = review;
        }

        List<HeroReview> reviewList;
		if (!this.reviews.ContainsKey(heroType)) {
			reviewList = new List<HeroReview>();
			this.reviews[heroType] = reviewList;
		}
		else {
			reviewList = this.reviews[heroType];
		}

		reviewList.Add(review);
		while (reviewList.Count > this.keepReviewLimit) {
			HeroReview oldReview = reviewList[0];
			if ((oldReview != this.bestReview) && (this.keepReviewLimit > 2)) {
				oldReview = reviewList[1];
			}
			if (oldReview == review) {
				break;
			}
		 
			reviewList.Remove(oldReview);
			this.heroReviewSpawner.Despawn(oldReview);
		}
	}

	public void ToggleShowBoard() {
		this.isShowingBoard = !this.isShowingBoard;
        this.reviewBoardContainer.gameObject.SetActive(this.isShowingBoard);
    }
}
