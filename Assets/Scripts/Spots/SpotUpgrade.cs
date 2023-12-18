using jmayberry.CustomAttributes;

public class SpotUpgrade : Spot {

    public override void OnMouseDown() {
        RoomManager.instance.SwapSpot(this);
    }
}
