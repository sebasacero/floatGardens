// PLACEHOLDER. Firmas obligatorias.
using UnityEngine;
namespace FloatGardens.Historia
{
    public class TitleMenu : MonoBehaviour
    {
        public bool Visible { get; private set; }
        public void Build(StoryGame g) { }
        public void ShowTitle() { }
        public void ShowPause() { }
        public void ShowEnding() { }
        public void Hide() { Visible = false; }
    }
}
