# Vista cenital ("devlog") en Unity

Equivalente en Unity de la vista cenital del juego web (`js3d/topdown.js`, `css/cenital.css`): cámara casi vertical
de lente larga (FOV 24, 58 m), mira asistida tipo twin-stick, retícula en el piso, sombra blanda bajo el barista y dos
temas: **noche** (piso carbón y andenes oscuros; la maqueta blanca queda a media luz) y **día** (maqueta blanca).
En pantallas táctiles se activa sola; en PC se elige en el lobby o en pausa. El mundo, los enemigos y el balance son
los mismos: solo cambian cámara, mira, controles y HUD.

> Estado: `Actors/TopDownCamera.cs` está escrito contra la API de Unity 6 pero **no se ha compilado ni probado en el
> editor** (en este entorno no hay Unity). Los tres enganches de abajo son pequeños y están pendientes de hacer en
> `GameManager.cs` y `PlayerController.cs`; si prefieres, los hago yo en el siguiente paso.

## Qué hay en `TopDownCamera.cs`

- `TopDownMode.Enabled` / `TopDownMode.Night`: preferencias guardadas en `PlayerPrefs` (`fg.cenital`, `fg.cenital.tema`).
- `TopDownCamera.Follow(dt)`: encuadre de juego; se adelanta 2,5 m hacia donde apunta (4,5 m con objetivo asistido).
- `TopDownCamera.Lobby(spawn, t)`: encuadre del lobby visto desde arriba.
- `TopDownCamera.Aim(dt, aimPixels, aimActive, enemies)`: devuelve el yaw del jugador. Mando: vector del dedo desde
  donde tocó (o el mouse proyectado al piso). Si no hay mando, mira hacia donde camina; al disparar se pega al enemigo
  más cercano a 42 m (cono de 36°, o de 11° si hay mando). Dibuja la línea de mira en el piso (lima = objetivo fijado).
- `TopDownCamera.ShotRay()`: el disparo sale del pecho, horizontal, o al torso del objetivo asistido.
- `TopDownCamera.ApplyTheme(night, sun, paving)`: cielo, niebla, ambiente, intensidad del sol, color del piso/andenes y
  de la retícula.

## Enganches pendientes (3)

1. **GameManager.Start**: además de `ThirdPersonCamera`, agrega `topRig = cam.gameObject.AddComponent<TopDownCamera>();
   topRig.cam = cam; topRig.target = player;`. En el bucle, donde hoy se llama `camRig.Follow(dt)` / `camRig.Lobby(...)`,
   usa `topRig` si `TopDownMode.Enabled`. Al cambiar de modo llama `topRig.SetVisible(on)` y, si está en cenital,
   `topRig.ApplyTheme(TopDownMode.Night, sunLight, new[]{ city.groundRenderer, city.sidewalkRenderer })` y
   `FGInput.CursorLocked = false` (la cenital no captura el mouse).
2. **PlayerController.Tick**: si `TopDownMode.Enabled`, en vez de `yaw += look.x` haz
   `yaw = g.topRig.Aim(dt, FGInput.touchLookPixelsFromStart, FGInput.touchLookActive, g.enemies); pitch = 0;` y para
   el movimiento usa `Vector3.forward`/`Vector3.right` del mundo (el joystick va con la pantalla: arriba = norte).
   `TouchControls.OnDrag` ya acumula `touchLookPixels`; falta guardar el vector desde el toque inicial
   (`touchLookPixelsFromStart`) y un booleano `touchLookActive` en `FGInput`.
3. **PlayerController.Fire**: si `TopDownMode.Enabled`, usa `var ray = g.topRig.ShotRay();` como origen y dirección
   del disparo (con el mismo `spread`), y una distancia mínima de 0,7 m en vez de la distancia de la cámara.

## HUD y controles

El HUD cenital del web (`#td` en `index.html`) es: telemetría monoespaciada arriba a la izquierda (`en vivo · cosecha`
/ `defensa`, velocidad, rumbo, vida, cerezas, café, enemigos), cápsula "Float Gardens · barrio" arriba a la derecha,
barra de vida fina abajo a la izquierda y radar de puntos abajo a la derecha (azul = enemigos y Arcadia, verde = lo
tuyo, lima = botín). Botones táctiles cuadrados con etiqueta: **disparar** (azul), **cosechar** (verde), impulso,
recargar. Se puede rehacer con `UIKit` siguiendo los tokens de `guia-diseno/assets/FGTokens.cs`.

Lo que se ve en el web (capturas reales) está en la carpeta del proyecto `movil-cenital/juego-web/`.
