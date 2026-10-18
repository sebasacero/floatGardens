// Entrada unificada: Input System nuevo o clásico (según la configuración del proyecto) + controles táctiles.
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FloatGardens
{
    public enum FGAction { Fire, Aim, Jump, Reload, Use, Sprint, Map, Pause }

    public static class FGInput
    {
        // Valores que escriben los controles táctiles (TouchControls)
        public static Vector2 touchMove;
        public static Vector2 touchLookPixels;
        static readonly HashSet<FGAction> touchHeld = new HashSet<FGAction>();
        static readonly HashSet<FGAction> touchPressed = new HashSet<FGAction>();

        public static float mouseSensitivity = 1f;
        public static float touchSensitivity = 1f;

        public static bool IsTouchDevice
        {
            get
            {
                if (Application.isMobilePlatform) return true;
#if ENABLE_INPUT_SYSTEM
                // Con el Input System nuevo, la clase Input clásica lanza excepciones: no se usa aquí.
                return Touchscreen.current != null && Mouse.current == null;
#elif ENABLE_LEGACY_INPUT_MANAGER
                return Input.touchSupported && !Input.mousePresent;
#else
                return false;
#endif
            }
        }

        public static void TouchDown(FGAction a) { touchHeld.Add(a); touchPressed.Add(a); }
        public static void TouchUp(FGAction a) { touchHeld.Remove(a); }
        public static void ToggleTouch(FGAction a) { if (!touchHeld.Remove(a)) touchHeld.Add(a); }

        public static bool CursorLocked
        {
            get => Cursor.lockState == CursorLockMode.Locked;
            set
            {
                if (IsTouchDevice) return;
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !value;
            }
        }

        /// <summary>x = lateral (derecha +), y = adelante (+).</summary>
        public static Vector2 Move()
        {
            Vector2 v = touchMove;
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null)
            {
                if (k[Key.W].isPressed || k[Key.UpArrow].isPressed) v.y += 1;
                if (k[Key.S].isPressed || k[Key.DownArrow].isPressed) v.y -= 1;
                if (k[Key.D].isPressed || k[Key.RightArrow].isPressed) v.x += 1;
                if (k[Key.A].isPressed || k[Key.LeftArrow].isPressed) v.x -= 1;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v.y += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v.y -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) v.x += 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) v.x -= 1;
#endif
            return Vector2.ClampMagnitude(v, 1f);
        }

        /// <summary>Giro de cámara en grados (x = yaw, y = pitch).</summary>
        public static Vector2 Look()
        {
            Vector2 deg = touchLookPixels * (200f / Mathf.Max(400, Screen.width)) * touchSensitivity;
            if (CursorLocked)
            {
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null) deg += Mouse.current.delta.ReadValue() * 0.12f * mouseSensitivity;
#elif ENABLE_LEGACY_INPUT_MANAGER
                deg += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 2.2f * mouseSensitivity;
#endif
            }
            return deg;
        }

        public static bool Held(FGAction a)
        {
            if (touchHeld.Contains(a)) return true;
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            var m = Mouse.current;
            switch (a)
            {
                case FGAction.Fire: return m != null && CursorLocked && m.leftButton.isPressed;
                case FGAction.Aim: return m != null && CursorLocked && m.rightButton.isPressed;
                case FGAction.Sprint: return k != null && (k[Key.LeftShift].isPressed || k[Key.RightShift].isPressed);
                case FGAction.Jump: return k != null && k[Key.Space].isPressed;
                case FGAction.Use: return k != null && k[Key.E].isPressed;
                case FGAction.Reload: return k != null && k[Key.R].isPressed;
                case FGAction.Map: return k != null && (k[Key.M].isPressed || k[Key.Tab].isPressed);
                case FGAction.Pause: return k != null && (k[Key.Escape].isPressed || k[Key.P].isPressed);
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            switch (a)
            {
                case FGAction.Fire: return CursorLocked && Input.GetMouseButton(0);
                case FGAction.Aim: return CursorLocked && Input.GetMouseButton(1);
                case FGAction.Sprint: return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                case FGAction.Jump: return Input.GetKey(KeyCode.Space);
                case FGAction.Use: return Input.GetKey(KeyCode.E);
                case FGAction.Reload: return Input.GetKey(KeyCode.R);
                case FGAction.Map: return Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Tab);
                case FGAction.Pause: return Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.P);
            }
#endif
            return false;
        }

        public static bool Pressed(FGAction a)
        {
            if (touchPressed.Contains(a)) return true;
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            var m = Mouse.current;
            switch (a)
            {
                case FGAction.Fire: return m != null && CursorLocked && m.leftButton.wasPressedThisFrame;
                case FGAction.Aim: return m != null && CursorLocked && m.rightButton.wasPressedThisFrame;
                case FGAction.Sprint: return k != null && k[Key.LeftShift].wasPressedThisFrame;
                case FGAction.Jump: return k != null && k[Key.Space].wasPressedThisFrame;
                case FGAction.Use: return k != null && k[Key.E].wasPressedThisFrame;
                case FGAction.Reload: return k != null && k[Key.R].wasPressedThisFrame;
                case FGAction.Map: return k != null && (k[Key.M].wasPressedThisFrame || k[Key.Tab].wasPressedThisFrame);
                case FGAction.Pause: return k != null && (k[Key.Escape].wasPressedThisFrame || k[Key.P].wasPressedThisFrame);
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            switch (a)
            {
                case FGAction.Fire: return CursorLocked && Input.GetMouseButtonDown(0);
                case FGAction.Aim: return CursorLocked && Input.GetMouseButtonDown(1);
                case FGAction.Sprint: return Input.GetKeyDown(KeyCode.LeftShift);
                case FGAction.Jump: return Input.GetKeyDown(KeyCode.Space);
                case FGAction.Use: return Input.GetKeyDown(KeyCode.E);
                case FGAction.Reload: return Input.GetKeyDown(KeyCode.R);
                case FGAction.Map: return Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab);
                case FGAction.Pause: return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
            }
#endif
            return false;
        }

        /// <summary>Clic izquierdo sin importar el bloqueo del cursor (para recapturarlo).</summary>
        public static bool ClickedAnywhere()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        public static void EndFrame()
        {
            touchPressed.Clear();
            touchLookPixels = Vector2.zero;
        }

        public static void ResetTouch()
        {
            touchHeld.Clear();
            touchPressed.Clear();
            touchMove = Vector2.zero;
            touchLookPixels = Vector2.zero;
        }
    }
}
