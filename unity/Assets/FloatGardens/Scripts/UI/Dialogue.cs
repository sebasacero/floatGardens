// Caja de diálogo al estilo de los RPG: nombre del que habla, texto que se escribe letra a letra y
// E / clic / toque para seguir. Cuando se acaban las líneas avisa a quien la abrió.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FloatGardens
{
    public class Dialogue : MonoBehaviour
    {
        Canvas canvas;
        Text who, body, hint;
        Image portrait;
        Line[] lines;
        int index;
        float shown;
        Action then;
        public bool Active { get; private set; }
        const float CharsPerSecond = 55f;

        public void Build()
        {
            canvas = UIKit.Canvas("Dialogo", 25);
            canvas.transform.SetParent(transform, false);
            var root = (RectTransform)canvas.transform;
            // Franjas de cine arriba y abajo
            UIKit.Rect(root, "FranjaArriba", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 54)).gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);
            var box = UIKit.Rect(root, "Caja", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 170));
            box.gameObject.AddComponent<Image>().color = UIKit.Glass;
            UIKit.Image(box, "Linea", new Vector2(0.5f, 1), Vector2.zero, new Vector2(4000, 3), UIKit.Yellow);
            portrait = UIKit.Image(box, "Retrato", new Vector2(0, 1), new Vector2(40, -22), new Vector2(14, 14), UIKit.Mint, UIKit.Circle);
            who = UIKit.Label(box, "", 24, UIKit.Yellow, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(64, -16), new Vector2(700, 30));
            body = UIKit.Label(box, "", 21, Color.white, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(64, -52), new Vector2(1100, 100), FontStyle.Normal);
            hint = UIKit.Label(box, "", 14, new Color(1, 1, 1, 0.7f), TextAnchor.MiddleRight, new Vector2(1, 0), new Vector2(-30, 14), new Vector2(400, 20), FontStyle.Normal);
            var tap = UIKit.Stretch(box, "Toque").gameObject.AddComponent<Image>();
            tap.color = new Color(0, 0, 0, 0);
            tap.raycastTarget = true;
            tap.gameObject.AddComponent<Button>().onClick.AddListener(Next);
            canvas.gameObject.SetActive(false);
        }

        public void Play(Line[] lines, Action then)
        {
            this.lines = lines;
            this.then = then;
            index = 0;
            Active = true;
            canvas.gameObject.SetActive(true);
            ShowLine();
        }

        void ShowLine()
        {
            var l = lines[index];
            who.text = l.who;
            who.color = l.who == "Barista" ? UIKit.Mint : l.who == "Don Monopolio" ? UIKit.Red : UIKit.Yellow;
            portrait.color = who.color;
            shown = 0;
            body.text = "";
            hint.text = (index + 1) + " / " + lines.Length + "   ·   " + (FGInput.IsTouchDevice ? "toca para seguir" : "E o clic para seguir");
        }

        void Next()
        {
            if (!Active) return;
            if (shown < lines[index].text.Length) { shown = lines[index].text.Length; body.text = lines[index].text; return; }
            index++;
            if (index < lines.Length) { ShowLine(); return; }
            Active = false;
            canvas.gameObject.SetActive(false);
            var cb = then;
            then = null;
            cb?.Invoke();
        }

        void Update()
        {
            if (!Active) return;
            string full = lines[index].text;
            if (shown < full.Length)
            {
                shown = Mathf.Min(full.Length, shown + Time.deltaTime * CharsPerSecond);
                body.text = full.Substring(0, (int)shown);
            }
            if (FGInput.Pressed(FGAction.Use) || FGInput.Pressed(FGAction.Fire) || FGInput.Pressed(FGAction.Jump)) Next();
        }
    }
}
