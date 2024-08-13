using UnityEngine;

using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;
using UnityEngine.Events;
using System;

namespace jmayberry.TypewriterHelper.Samples.ChatPortrait {
	public class DebugManager : MonoBehaviour {

		[EntryFilter(Variant = EntryVariant.Event)] public EntryReference testConversation;
		public UnityEvent actionToRun;

		public void OnStartTestConversation() {
			DialogManager.instance.TryStartSequence(testConversation);
		}

		public void OnStopConversation() {
			DialogManager.instance.StopSequence();
		}
	}
}