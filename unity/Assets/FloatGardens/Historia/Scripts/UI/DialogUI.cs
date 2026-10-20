// PLACEHOLDER. Firmas obligatorias.
using System;
using UnityEngine;
namespace FloatGardens.Historia
{
    public class DialogUI : MonoBehaviour
    {
        public bool Active { get; private set; }
        public void Build(StoryGame g) { }
        public void Play(DialogLine[] lines, Action onDone) { onDone?.Invoke(); }
    }
}
