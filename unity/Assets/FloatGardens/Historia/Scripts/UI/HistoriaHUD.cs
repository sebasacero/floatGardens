// PLACEHOLDER. Firmas obligatorias.
using UnityEngine;
namespace FloatGardens.Historia
{
    public class HistoriaHUD : MonoBehaviour
    {
        public void Build(StoryGame g) { }
        public void Show(bool on) { }
        public void OnPlaceChanged() { }
        public void Tick(float dt) { }
        public void Refresh() { }
        public void Toast(string msg) { }
        public void Objective(string title, string desc) { }
        public void ChapterCard(Chapter c) { }
        public void PlaceCard(string title, string subtitle) { }
    }
}
