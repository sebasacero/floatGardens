# Terrazas de Kennedy (prototipo de plataformas 2.5D)

Un nivel jugable de plataformas con cámara lateral, inspirado en el estilo de *Donkey Kong Country*: corres y saltas
por las terrazas de las casas de ladrillo de Kennedy, recoges cerezas de café, espantas palomas y drones de Arcadia,
te disparas en los tanques de agua por encima de la Avenida de las Américas y llegas a la torre hidropónica de
Float Gardens antes de que te alcance la tormenta.

- **Jugar:** abre `terrazas.html` (o `https://sebasacero.github.io/floatGardens/terrazas.html` cuando esté en `main`).
- Funciona con teclado, pantalla táctil (mejor en horizontal) y control de Xbox/PlayStation (Gamepad API).
- No necesita compilación: Three.js r149 de `vendor/` y cinco scripts en esta carpeta.

## Controles

| Acción | Teclado | Control | Celular |
| --- | --- | --- | --- |
| Moverse | ← → · A D | Stick izquierdo / cruceta | ◀ ▶ |
| Saltar (mantén para subir más) | Espacio · W · ↑ | A | Saltar |
| Rodar (rueda + salta = salto largo) | Shift · X · J | X / B | Rodar |
| Pausa | Esc · P | Start | ❚❚ |

## El nivel (≈ 520 m, 1 minuto corriendo sin parar)

| Tramo | Qué pasa |
| --- | --- |
| A. Tu terraza | Arranca en tu terraza con el cultivo de café. Saltos cortos entre casas, primeras palomas, un colchón viejo que sirve de trampolín para subir a una casa de cinco pisos. |
| B. Av. de las Américas | Te metes a un tanque de agua y saltas para dispararte; otro tanque encima de una valla de Float Gardens te lanza al otro lado. Abajo pasan los TransMilenio. |
| C. Obra negra | Dos andamios colgantes que suben y bajan te llevan a la placa de un edificio en obra, con varillas y drones. |
| D. Lote con tejas | Tejas de Eternit sobre parales: tiemblan cuando las pisas y se caen a los 0,5 s. |
| E. La tormenta | La tormenta morada de Arcadia te persigue por las terrazas (como el pulpo del video). Si te alcanza, vuelves al último aviso. |
| F. La torre | El anexo de la torre hidropónica con camas de cultivo; tocar el aviso grande termina el nivel. |

Coleccionables: 141 cerezas (cada 100 = una vida) y las letras **F-L-O-A-T** (las cinco = una vida). Los avisos de
Float Gardens apagados son puntos de control: se prenden cuando pasas. Tienes 3 tintos de vida y 4 vidas.

## Cómo está hecho

| Archivo | Qué tiene |
| --- | --- |
| `nivel.js` | El nivel como una tabla: casas (`C`, ancho, altura de la placa) y huecos (`H`, ancho, tipo) con lo que lleva cada uno. Para cambiar el nivel solo se edita esta tabla. |
| `mundo.js` | El barrio procedural: la hilera jugable, nueve hileras de cuadras detrás que respetan las mismas calles, ciudad lejana instanciada, calles con carros, taxis, SITP y TransMilenio, postes con cables, árboles, cerros con Monserrate y Guadalupe, nubes y cometas. Todo en pocos *draw calls* (geometría combinada por material). |
| `personajes.js` | Modelos con primitivas: el barista (gorra verde, delantal, AeroPress en la espalda), paloma, dron, cerezas instanciadas, letras, tanque-cañón, colchón, andamio, teja, avisos y la pared de tormenta (shader de ruido). |
| `juego.js` | Física, cámara, enemigos, mecánicas, HUD, entrada y sonido sintetizado. |
| `base.js` | Azar con semilla, constructor de geometría y las texturas en canvas (fachadas, ladrillo, placa, avisos). |

### Sensación de control (la tabla `FISICA` en `juego.js`)

| Parámetro | Valor | Para qué |
| --- | --- | --- |
| `correr` / `rodar` | 9,5 / 15 m/s | Velocidad normal y de la rodada |
| `salto` | 15,5 m/s | Velocidad inicial del salto |
| `gravSostenido` / `gravSubida` / `gravCaida` | 30 / 62 / 55 m/s² | Salto variable: si sueltas el botón sube menos; la caída es más rápida que la subida |
| `coyote` / `buffer` | 0,10 / 0,13 s | Puedes saltar un instante después de salir del borde, y el salto se recuerda si lo presionas justo antes de tocar el piso |
| Alcance | ≈ 4 m de alto, ≈ 8 m de largo, ≈ 12 m rodando | Ningún hueco obligatorio pasa de 6,5 m ni ninguna subida de 3,6 m sin ayuda |

La física corre a 120 pasos por segundo fijos, separada del dibujo, así que se siente igual en un celular lento.
El movimiento es en 2D (x, y) sobre un plano en z = 0; el mundo alrededor es 3D completo.

### Cámara

Lateral con perspectiva, como en el video: sigue al jugador con un adelanto hacia donde mira, no rebota con cada
salto (sigue la altura de la última terraza pisada salvo que subas o caigas mucho), se aleja un poco en los tanques
y durante la tormenta para que se vea la persecución, y se aleja más en pantallas verticales.

## Cómo pasarlo a Unity

El código Unity más nuevo está en la rama de la PR #4. Este prototipo no lo toca; la idea es que la misma tabla de
nivel y los mismos números de física se lleven tal cual. Equivalencias propuestas:

| Web (este prototipo) | Unity 6 + URP |
| --- | --- |
| `nivel.js` (tabla del nivel) | `TerrazasLevel` (ScriptableObject) con la misma lista de casas y huecos; un `LevelBuilder` en el editor genera la escena con `MeshBuilder`/`FGTextures`, como `City.cs`. |
| `FISICA` + `moveAndCollide` | `PlatformerController`: controlador cinemático propio con cajas (AABB) contra `BoxCollider2D` o `Physics.BoxCast` restringido a z = 0. Mejor que `CharacterController` o un `Rigidbody` para tener coyote time, buffer y salto variable exactos. Correr en `FixedUpdate` a 120 Hz (`Time.fixedDeltaTime = 1/120`). |
| `updateCamera` | `SideScrollCamera` (o Cinemachine 3 con *Position Composer*: dead zone vertical, *lookahead* y *damping*). |
| Tanques-cañón | `TankCannon`: al entrar desactiva el `PlatformerController`, anima la trayectoria (parábola) y lo reactiva al llegar o encadena con el siguiente tanque. |
| Andamios / tejas / colchón | `MovingPlatform` (con *carry* del jugador), `CrumblePlatform`, `BouncePad`. Plataformas de un solo sentido con `PlatformEffector2D` o la misma regla del prototipo. |
| Palomas, drones | Reusar `Drone.cs`; `Pigeon` como patrulla simple. Pisotón = rebote; tocarlos de lado = daño. |
| Tormenta | `StormWall`: un plano con Shader Graph de ruido fbm que avanza; *rubber band* a 13 m del jugador. |
| HUD, menús, táctil | `HUD.cs`, `Menus.cs`, `TouchControls.cs` y `FGInput.cs` ya existen; agregar botones Rodar/Saltar y la fila de letras F-L-O-A-T. |
| Estilo visual | El mismo `FGStyleProfile` por plataforma (perfil Fortnite para celular): toon/flat shading, sombras suaves, cielo y bruma de tarde bogotana. |

Pasos sugeridos: 1) `PlatformerController` + `SideScrollCamera` con cajas grises y la tabla del nivel;
2) ajustar la sensación con los mismos números; 3) mecánicas (tanques, andamios, tejas, tormenta); 4) arte.
