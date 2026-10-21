// Cuadro de diálogo: nombre, retrato con la inicial, texto que se escribe solo. Clic, toque, E, espacio o Enter avanzan.
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FloatGardens.Historia
{
    public class DialogUI : MonoBehaviour
    {
        public bool Active { get; private set; }
        StoryGame g;
        Canvas canvas;
        RectTransform root;
        Text speaker, body, initial, hint;
        Image portrait, plate;
        DialogLine[] lines;
        Action onDone;
        int index;
        string full = "";
        float shown;
        bool tapped;
        int openedFrame;

        const float CharsPerSecond = 50f;

        public void Build(StoryGame game)
        {
            g = game;
            canvas = UIKit.Canvas("Dialogo", 40);
            canvas.transform.SetParent(transform, false);
            root = (RectTransform)canvas.transform;

            // Bloqueador de pantalla completa: un toque en cualquier parte avanza
            var blocker = UIKit.Stretch(root, "Bloqueo").gameObject.AddComponent<Image>();
            blocker.color = new Color(0, 0, 0, 0.18f);
            blocker.raycastTarget = true;
            blocker.gameObject.AddComponent<DialogTap>().ui = this;

            var box = UIKit.Image(root, "Caja", new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(1080, 200), new Color(0.07f, 0.045f, 0.03f, 0.95f));
            UIKit.Image(box.transform, "Borde", new Vector2(0.5f, 1), Vector2.zero, new Vector2(1080, 3), HConfig.Gold);
            portrait = UIKit.Image(box.transform, "Retrato", new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(132, 132), HConfig.Coffee);
            initial = UIKit.Label(portrait.transform, "", 64, UIKit.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(132, 132));
            plate = UIKit.Image(box.transform, "Nombre", new Vector2(0, 1), new Vector2(176, 18), new Vector2(360, 40), HConfig.Coffee);
            speaker = UIKit.Label(plate.transform, "", 20, UIKit.Text, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(340, 36));
            body = UIKit.Label(box.transform, "", 21, UIKit.Text, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(178, -34), new Vector2(870, 140), FontStyle.Normal);
            body.lineSpacing = 1.12f;
            hint = UIKit.Label(box.transform, "▼", 18, HConfig.Gold, TextAnchor.MiddleRight, new Vector2(1, 0), new Vector2(-20, 12), new Vector2(60, 24));
            UIKit.Button(box.transform, "Saltar", new Vector2(1, 1), new Vector2(-14, 20), new Vector2(110, 36), new Color(1, 1, 1, 0.12f), UIKit.Text, SkipAll, 16);
            root.gameObject.SetActive(false);
        }

        public void Play(DialogLine[] newLines, Action done)
        {
            if (newLines == null || newLines.Length == 0) { done?.Invoke(); return; }
            lines = newLines;
            onDone = done;
            index = 0;
            openedFrame = Time.frameCount;
            Active = true;
            root.gameObject.SetActive(true);
            FGInput.CursorLocked = false;
            ShowLine();
        }

        void ShowLine()
        {
            var l = lines[index];
            string who = g != null ? g.Fill(l.speaker) : l.speaker;
            full = g != null ? g.Fill(l.text) : l.text;
            shown = 0f;
            body.text = "";
            bool narrator = string.IsNullOrEmpty(who) || who == "Narrador";
            portrait.gameObject.SetActive(!narrator);
            plate.gameObject.SetActive(!narrator);
            body.fontStyle = narrator ? FontStyle.Italic : FontStyle.Normal;
            body.rectTransform.anchoredPosition = narrator ? new Vector2(40, -34) : new Vector2(178, -34);
            body.rectTransform.sizeDelta = narrator ? new Vector2(1000, 140) : new Vector2(870, 140);
            if (!narrator)
            {
                speaker.text = who;
                Color c = SpeakerColor(who);
                plate.color = c;
                portrait.color = c * new Color(0.75f, 0.75f, 0.75f, 1f);
                initial.text = Initial(who);
            }
        }

        static string Initial(string who)
        {
            string w = who;
            foreach (var p in new[] { "Doña ", "Don ", "Dra. ", "Profesor ", "Profesora " })
                if (w.StartsWith(p)) { w = w.Substring(p.Length); break; }
            return w.Length > 0 ? w.Substring(0, 1).ToUpperInvariant() : "?";
        }

        static Color SpeakerColor(string who)
        {
            int h = 17;
            foreach (char ch in who) h = h * 31 + ch;
            float hue = Mathf.Abs(h % 360) / 360f;
            return Color.HSVToRGB(hue, 0.55f, 0.55f);
        }

        internal void Tap() { tapped = true; }

        void Advance()
        {
            SFX.Play("pickup", 0.35f);
            if (shown < full.Length) { shown = full.Length; body.text = full; return; }
            index++;
            if (index < lines.Length) { ShowLine(); return; }
            Finish();
        }

        void SkipAll()
        {
            if (!Active) return;
            Finish();
        }

        void Finish()
        {
            Active = false;
            root.gameObject.SetActive(false);
            var done = onDone;
            onDone = null;
            lines = null;
            done?.Invoke(); // puede abrir otro diálogo: por eso Active ya es false
        }

        void Update()
        {
            if (!Active) return;
            if (shown < full.Length)
            {
                shown = Mathf.Min(full.Length, shown + Time.unscaledDeltaTime * CharsPerSecond);
                body.text = full.Substring(0, Mathf.FloorToInt(shown));
            }
            hint.enabled = shown >= full.Length && Mathf.Repeat(Time.unscaledTime, 1f) < 0.6f;
            if (Time.frameCount == openedFrame) { tapped = false; return; } // la misma E que abrió el diálogo no lo avanza
            bool key = FGInput.Pressed(FGAction.Use) || FGInput.Pressed(FGAction.Jump) || EnterPressed();
            if (tapped || key) { tapped = false; Advance(); }
        }

        static bool EnterPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            return k != null && (k[Key.Enter].wasPressedThisFrame || k[Key.NumpadEnter].wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#else
            return false;
#endif
        }
    }

    /// <summary>Recibe los toques del bloqueador del diálogo.</summary>
    public class DialogTap : MonoBehaviour, IPointerClickHandler
    {
        public DialogUI ui;
        public void OnPointerClick(PointerEventData e) { if (ui != null) ui.Tap(); }
    }
}
