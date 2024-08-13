using UnityEditor;

using Aarthificial.Typewriter.Editor.Descriptors;
using jmayberry.TypewriterHelper.Editor.Descriptors;

[CustomEntryDescriptor(typeof(MyDialogEntry))]
public class MyDialogEntryDescriptor : BaseDialogEntryDescriptor { }

[CustomPropertyDrawer(typeof(MyDialogEntry))]
public class MyDialogEntryPropertyDrawer : BaseDialogEntryDescriptor { }