using Aarthificial.Typewriter.Editor.Descriptors;
using jmayberry.TypewriterHelper.Editor.Descriptors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEntryDescriptor(typeof(MyActionEntry))]
public class MyActionEntryDescriptor : BaseActionEntryDescriptor<MyActionType> { }