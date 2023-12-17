using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CardDeck;
using jmayberry.CustomAttributes;

public class RoomCardManager : CardManager<CardType, CardSideEffect> {
    [Required] public GameObject drawBackup;
    [Required] public GameObject discardBackup;

    [Readonly] public RoomCard selectedCard;

    public static RoomCardManager instanceApplied { get; private set; }
    public override void Awake() {
        base.Awake();

        if (instanceApplied != null) {
            Debug.LogError("Found more than one RoomCardManager in the scene.");
        }

        instanceApplied = this;
    }

    public void Start() {
        // Clean up piles
        this.pileDraw.ClearPile();
        this.pileHand.ClearPile();
        this.pileDiscard.ClearPile();
        this.pileDestroy.ClearPile();

        // Temp: Start game right away
        this.OnNewGame();
    }
}