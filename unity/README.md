# 🌱☕ Float Gardens: Bogotá — versión Unity

Versión en **Unity 6 + URP** del shooter de mundo abierto en Bogotá. Tiene la misma lógica que la versión web
(zonas, enemigos, economía, Puntos Float, jefe final), con look de cine: sol bajo con sombras largas y suaves,
oclusión ambiental (SSAO), reflejos en tiempo real (el agua, el vidrio y el asfalto mojado reflejan la ciudad), bruma de
montaña, etalonaje con tonos fríos en las sombras y cálidos en las luces, bloom y destellos de lente, grano de película,
aberración cromática, profundidad de campo en la distancia, desenfoque de movimiento de cámara, tonemapping ACES,
lluvia con partículas, física real y sonido.

Todo se genera por código: la ciudad, los personajes, el HUD y los menús. No hay prefabs ni escenas hechas a mano.
Así el proyecto es liviano en Git y fácil de modificar, y los modelos 3D se pueden ir cambiando por mejores poco a poco.

> **Estado:** primera versión. El código compila contra las librerías de Unity, pero todavía no se ha probado en el
> editor. Si al abrirlo aparece algún error en la consola, copia el mensaje y lo corregimos.

## 1. Abrir el proyecto (5 minutos)

1. Instala **Unity Hub** y **Unity 6 LTS** (6000.0.x). Si vas a exportar a celular, agrega los módulos *Android Build Support*
   y/o *iOS Build Support*.
2. En Unity Hub: **New project → Universal 3D** (la plantilla de URP). Ponle de nombre `FloatGardensBogota`.
3. Trae el juego al proyecto de una de estas dos formas (no las dos a la vez, porque se duplican las clases):
   - **Desde Unity (Package Manager):** **Window → Package Manager → botón + → Install package from git URL…** y pega
     `https://github.com/sebasacero/floatGardens.git?path=/unity/Assets/FloatGardens`.
     Necesita Git instalado en el computador. Para actualizar, vuelve a pegar el link o usa **Update** en el Package Manager.
   - **Copiando la carpeta:** copia `unity/Assets/FloatGardens` de este repositorio dentro de la carpeta `Assets/` del
     proyecto. Puedes arrastrarla desde el Finder o el Explorador hasta **Assets** en el panel Project de Unity.
4. Espera a que compile.
5. En la barra de menús: **Float Gardens → Configurar todo y crear escena**.
6. Dale **Play** ▶.

El menú hace lo siguiente:
- Si el proyecto no tiene URP activo (por ejemplo, si se creó con la plantilla 3D clásica y el juego se ve rosado), lo
  activa con `Assets/FloatGardens/Render/FG_URP.asset`. Además ajusta el URP para calidad de cine: HDR, MSAA x4, sombras
  suaves de 4096 px a 150 m, reflejos combinados y oclusión ambiental (SSAO).
- Crea los materiales plantilla en `Assets/FloatGardens/Resources/FloatGardens/`, para que el build incluya los shaders.
- Ajusta el nombre del producto y la orientación horizontal.
- Crea la escena `Assets/FloatGardens/Scenes/Bogota.unity` y la agrega al build.

## 2. La historia

Un barista joven con una **AeroPress** y un **PC** quiere ser un referente mundial del café desde una ciudad donde el
café solo crece adentro. **Arcadia Bean Corp.**, la corporación que patentó el café colombiano en la Neo-Bogotá de 2087, no está de acuerdo. Cuatro capítulos en
Bogotá con seis mentores (la Abuela Rosa, el Profe Arango, Kaldi, el Padre Francisco, Catalina y Alanita), un
**Códice del Café** con 50 entradas reales (historia, Colombia, cultivo, proceso, preparación, cata), un **mapa de
expediciones por Colombia** con diez regiones y sus variedades, y el **Laboratorio AeroPress** en el PC del HQ,
donde se afinan recetas con proporción, molienda, temperatura, tiempo y método hasta llegar a una taza de 80+.
El diseño completo está en [`docs/DISENO.md`](../docs/DISENO.md).

## 3. Controles

| Acción | Computador | Celular |
| --- | --- | --- |
| Moverse / correr | `WASD` / `Shift` | Joystick (al fondo = correr) |
| Mirar | Mouse (clic en el juego para capturarlo) | Deslizar en la mitad derecha |
| Disparar / apuntar | Clic / clic derecho | DISPARO (arrastra para apuntar) / APUNTAR |
| Saltar / recargar | `Espacio` / `R` | SALTO / RECARGA |
| Cosechar, vender, hablar, tienda, laboratorio | `E` | **E** |
| Mapa / pausa | `M` / `Esc` | MAPA / II |

Funciona con el **Input System** nuevo (el que trae Unity 6 por defecto) y también con el sistema clásico.

## 4. Exportar a Android o iOS

1. **File → Build Profiles** (o *Build Settings*) → elige **Android** o **iOS** → *Switch Platform*.
2. En **Player Settings** cambia el *Package Name* (por ejemplo `com.floatgardens.bogota`) y pon el ícono.
3. Para celulares de gama media, en el asset de URP de la carpeta `Settings` usa el perfil *Mobile* o *Performant*:
   sombras a 30–50 m y render scale de 0,8–0,9.
4. **Build**. En Android sale un `.apk` o `.aab` para la Play Store. En iOS sale un proyecto de Xcode.

## 5. Gráficos y personajes

**Lo que ya hace el código:**
- Texturas PBR generadas al arrancar, a 128 px por metro en fachadas. Cada una trae color, relieve (normal map) y
  máscara de metal/brillo: el ladrillo y la pega reciben la luz rasante, y el vidrio de las ventanas brilla y refleja
  la ciudad mientras la pared queda mate. Se pintan en paralelo en hilos de fondo (menos de un segundo).
- Edificios con cornisas, antepechos, franjas de entrepiso, balcones alineados con las ventanas, tanques de agua negros,
  aires acondicionados, antenas y cuartos de máquinas en las torres.
- Calles con postes de luz, postes de concreto con cables colgando (bien bogotano) y carros parqueados con pintura
  brillante, muchos de ellos taxis amarillos.

**Bogotá agro-punk de noche:** el día dura 14 minutos. Al caer la noche se encienden las ventanas (cada fachada trae
un mapa de luz), los neones de las agrocorporaciones (AGROCORP, SEMILLAS Ω, HIDRO·PONIA, DRON·FUMIGA...), las tiras de
neón de las torres, los hologramas que giran sobre las azoteas y los LEDs rosados de los invernaderos hidropónicos.
Los postes y los neones iluminan de verdad: un grupo de 24 luces se va moviendo a los más cercanos a la cámara
(URP en Forward+). Drones fumigadores patrullan los barrios con reflectores. La luna sale por el lado contrario al sol.

**Personajes de verdad con Mixamo (unos minutos y un clic):**
1. En [Mixamo](https://www.mixamo.com) (gratis) descarga un personaje en **FBX for Unity, With Skin**.
2. Descarga también sus animaciones en **FBX for Unity, Without Skin** (marca *In Place* en las de caminar y correr):
   *Idle*, *Walking*, *Running*, *Rifle Aiming Idle* y *Death*.
3. Arrastra los archivos a cualquier carpeta de `Assets` en Unity.
4. Selecciónalos todos en el panel Project y usa el menú **Float Gardens → Personajes → Crear Barista con lo seleccionado**
   (o Enemigo, Jefe, Peatón).
5. Dale **Play**: el juego carga solo el prefab desde `Assets/FloatGardens/Resources/FloatGardens/Modelos`.

El menú pone el rig en Humanoid, arma el Animator Controller con los parámetros que usa el juego (`Speed`, `Aiming`,
`Dead`) y reconoce cada animación por su nombre. También puedes arrastrar prefabs propios a los campos del componente
`FGBootstrap` del objeto **FloatGardens**.

Para los edificios, la ciudad está en `Scripts/World/City.cs`: cada barrio tiene su función (`Buildings`). Puedes
reemplazar las cajas por modelos de paquetes gratuitos como [Kenney City Kit](https://kenney.nl/assets) o
[Quaternius](https://quaternius.com), o usar ProBuilder para modelar edificios icónicos como la Torre del Centro,
la Sede o el santuario de Monserrate.

Otras ideas de calidad:
- AI Navigation (NavMesh) para que los enemigos esquiven mejor.
- Cinemachine para la cámara.
- TextMeshPro para textos más nítidos.
- Audio real grabado en Bogotá.

## 6. Hoja de ruta hacia un AAA (agro-punk colombiano)

La referencia es Cyberpunk 2077, pero en una Colombia donde las agrocorporaciones controlan semillas, agua y tierra,
y Float Gardens es la resistencia hidropónica. Lo que da el código y lo que necesita arte:

| Etapa | Qué | Cómo |
| --- | --- | --- |
| 1. El look (hecho) | Noche de neón, día/noche, luces reales, invernaderos, drones, texturas PBR, posprocesado de cine | Código |
| 2. Moverse | Moto y carro manejables, mapa más grande (Corabastos como mercado ciber, cerros con invernaderos verticales), metro elevado | Código |
| 3. RPG | Diálogos con NPCs, misiones, árbol de habilidades, "injertos" (mejoras biológicas en vez de implantes), inventario, reputación por barrio | Código |
| 4. Arte | Personajes de Mixamo, paquetes de ciudad (Synty, Kenney), música, voces, cinemáticas | Assets + artistas |

## 7. Estructura

```
Assets/FloatGardens/
  Scripts/FGBootstrap.cs        punto de entrada (modelos opcionales)
  Scripts/Core/FGConfig.cs      mapa de barrios, calles, zonas, enemigos, mejoras y balance
  Scripts/Core/FGSave.cs        guardado (PlayerPrefs) y cupones FG-XXX-XXX-XX
  Scripts/Core/FGInput.cs       teclado/mouse (Input System nuevo o clásico) + táctil
  Scripts/Core/SFX.cs           efectos de sonido sintetizados
  Scripts/World/City.cs         generación de Bogotá, colisiones, minimapa
  Scripts/World/Bus.cs          buses articulados de la Av. Caracas
  Scripts/World/Drone.cs        drones fumigadores con reflector y hologramas publicitarios
  Scripts/World/FGTextures.cs   texturas procedurales (ladrillo, colonial, vidrio...)
  Scripts/World/FGMaterials.cs  materiales URP (Lit, emisivos, transparentes)
  Scripts/World/MeshBuilder.cs  mallas combinadas para pocas llamadas de dibujo
  Scripts/Actors/               jugador, cámara, enemigos y jefe, peatones, personajes de la historia, cultivos, botín, zonas, efectos
  Scripts/Game/GameManager.cs   estados, zonas, oleadas, economía, jefe final
  Scripts/Game/Story.cs         campaña: capítulos, misiones, diálogos y personajes
  Scripts/Core/Codex.cs         Códice del Café, variedades y regiones de Colombia
  Scripts/Core/Brewing.cs       modelo de extracción y puntaje de cata del AeroPress
  Scripts/Game/Atmosphere.cs    ciclo día/noche, sol y luna, cielo, niebla, luces de calle, posprocesado y lluvia
  Scripts/UI/                   HUD, menús, diálogo, Códice, mapa, laboratorio y controles táctiles (uGUI por código)
  Editor/FGSetup.cs             menú "Float Gardens" del editor
  Editor/FGCharacters.cs        menú "Float Gardens > Personajes" (Mixamo a personaje del juego)
  package.json, *.asmdef, *.meta  permiten instalarlo como paquete desde una URL de Git
```

## 8. Puntos Float y café gratis

Igual que en la web: se ganan cosechando, vendiendo, derrotando enemigos, capturando zonas y venciendo al jefe.
Con 1.500 puntos se genera un cupón `FG-XXX-XXX-XX` con dígito de control (mismo formato que la web). El progreso
se guarda en el dispositivo y no se comparte con la versión web. Para regalar café de verdad hace falta un backend
que registre cada cupón y lo marque como usado.

> Juego de ficción y parodia. No está afiliado ni respaldado por ninguna organización cafetera real.
