using jmayberry.TypewriterHelper.Entries;

public enum MySpeakerType {
	Unknown,
	System,
	Slime,
	Skeleton,
}

public enum MyEmotionType {
	Normal,
	Happy,
	Angry,
	Sad,
}

public class MyDialogEntry : BaseDialogEntry<MyEmotionType> { }