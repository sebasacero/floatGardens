# 🌱☕ Float Gardens: Bogotá — versión Unity

Versión en **Unity 6 + URP** del shooter de mundo abierto en Bogotá. Tiene la misma lógica que la versión web
(zonas, enemigos, economía, Puntos Float, jefe final), con la calidad gráfica que da Unity: sombras en tiempo real,
materiales PBR (el agua y el asfalto mojado reflejan el cielo), neón con resplandor (bloom), cielo procedural,
tonemapping ACES, lluvia con partículas, física real y sonido.

Todo se genera por código: la ciudad, los personajes, el HUD y los menús. No hay prefabs ni escenas hechas a mano.
Así el proyecto es liviano en Git y fácil de modificar, y los modelos 3D se pueden ir cambiando por mejores poco a poco.

> **Estado:** primera versión. El código compila contra las librerías de Unity, pero todavía no se ha probado en el
> editor. Si al abrirlo aparece algún error en la consola, copia el mensaje y lo corregimos.

## 1. Abrir el proyecto (5 minutos)

1. Instala **Unity Hub** y **Unity 6 LTS** (6000.0.x). Si vas a exportar a celular, agrega los módulos *Android Build Support*
   y/o *iOS Build Support*.
2. En Unity Hub: **New project → Universal 3D** (la plantilla de URP). Ponle de nombre `FloatGardensBogota`.
3. Copia la carpeta `unity/Assets/FloatGardens` de este repositorio dentro de la carpeta `Assets/` del proyecto nuevo.
4. Vuelve a Unity y espera a que compile.
5. En la barra de menús: **Float Gardens → Configurar todo y crear escena**.
6. Dale **Play** ▶.

El menú hace lo siguiente:
- Crea los materiales plantilla en `Assets/FloatGardens/Resources/FloatGardens/`, para que el build incluya los shaders.
- Crea los perfiles visuales de móvil, web y consola en esa misma carpeta.
- Ajusta el nombre del producto y la orientación horizontal.
- Crea la escena `Assets/FloatGardens/Scenes/Bogota.unity` y la agrega al build.

## 2. Controles

| Acción | Computador | Celular |
| --- | --- | --- |
| Moverse / correr | `WASD` / `Shift` | Joystick (al fondo = correr) |
| Mirar | Mouse (clic en el juego para capturarlo) | Deslizar en la mitad derecha |
| Disparar / apuntar | Clic / clic derecho | DISPARO (arrastra para apuntar) / APUNTAR |
| Saltar / recargar | `Espacio` / `R` | SALTO / RECARGA |
| Cosechar, vender, tienda | `E` | **E** |
| Mapa / pausa | `M` / `Esc` | MAPA / II |

Funciona con el **Input System** nuevo (el que trae Unity 6 por defecto) y también con el sistema clásico.

## 3. Exportar a Android o iOS

1. **File → Build Profiles** (o *Build Settings*) → elige **Android** o **iOS** → *Switch Platform*.
2. En **Player Settings** cambia el *Package Name* (por ejemplo `com.floatgardens.bogota`) y pon el ícono.
3. Para celulares de gama media, en el asset de URP de la carpeta `Settings` usa el perfil *Mobile* o *Performant*:
   sombras a 30–50 m y render scale de 0,8–0,9.
4. **Build**. En Android sale un `.apk` o `.aab` para la Play Store. En iOS sale un proyecto de Xcode.

## 4. Mejorar los gráficos (siguiente paso)

El código ya está listo para cambiar los personajes de cajas por modelos animados:

1. Descarga un personaje de [Mixamo](https://www.mixamo.com) (gratis) en formato FBX para Unity, con animaciones
   *Idle*, *Run*, *Rifle Aiming Idle* y *Death*.
2. En Unity, en el FBX: **Rig → Animation Type: Humanoid**.
3. Crea un **Animator Controller** con tres parámetros:
   - `Speed` (float de 0 a 1.2): mezcla entre Idle y Run.
   - `Aiming` (bool): capa o estado de apuntar.
   - `Dead` (bool): animación de caída.
4. Crea un prefab con el modelo y su Animator.
5. Selecciona el objeto **FloatGardens** en la escena y arrastra los prefabs a los campos *Player Model*,
   *Enemy Model*, *Boss Model* y *Pedestrian Model* del componente `FGBootstrap`.

Para los edificios, la ciudad está en `Scripts/World/City.cs`: cada barrio tiene su función (`Buildings`). Puedes
reemplazar las cajas por modelos de paquetes gratuitos como [Kenney City Kit](https://kenney.nl/assets) o
[Quaternius](https://quaternius.com), o usar ProBuilder para modelar edificios icónicos como la Torre del Centro,
la Sede o el santuario de Monserrate.

Otras ideas de calidad:
- AI Navigation (NavMesh) para que los enemigos esquiven mejor.
- Cinemachine para la cámara.
- TextMeshPro para textos más nítidos.
- Audio real grabado en Bogotá.

## 5. Estilo visual por plataforma

El juego cambia de look según la plataforma de compilación: **móvil** MOBA estilo League of Legends (cámara isométrica), **web** cómic/grafiti 2.5D y
**consola** pintura. Los perfiles son ScriptableObjects editables en `Resources/FloatGardens/`. Detalles en
[ESTILO_VISUAL.md](ESTILO_VISUAL.md).

## 6. Estructura

```
Assets/FloatGardens/
  Scripts/FGBootstrap.cs        punto de entrada (modelos opcionales)
  Scripts/Core/FGConfig.cs      mapa de barrios, calles, zonas, enemigos, mejoras y balance
  Scripts/Core/FGSave.cs        guardado (PlayerPrefs) y cupones FG-XXX-XXX-XX
  Scripts/Core/FGInput.cs       teclado/mouse (Input System nuevo o clásico) + táctil
  Scripts/Core/SFX.cs           efectos de sonido sintetizados
  Scripts/World/City.cs         generación de Bogotá, colisiones, minimapa
  Scripts/World/Bus.cs          buses articulados de la Av. Caracas
  Scripts/World/FGTextures.cs   texturas procedurales (ladrillo, colonial, vidrio...)
  Scripts/World/FGMaterials.cs  materiales URP (Lit, emisivos, transparentes)
  Scripts/World/MeshBuilder.cs  mallas combinadas para pocas llamadas de dibujo
  Scripts/Actors/               jugador, cámara, enemigos y jefe, peatones, cultivos, botín, zonas, efectos
  Scripts/Game/GameManager.cs   estados, zonas, oleadas, economía, jefe final
  Scripts/Game/Atmosphere.cs    sol, cielo, niebla, posprocesado y lluvia
  Scripts/Style/                perfiles visuales por plataforma (móvil, web, consola)
  Scripts/UI/                   HUD, menús y controles táctiles (uGUI por código)
  Editor/FGSetup.cs             menú "Float Gardens" del editor
  Editor/FGStyleMenu.cs         perfiles visuales y vista previa por plataforma
```

## 7. Puntos Float y café gratis

Igual que en la web: se ganan cosechando, vendiendo, derrotando enemigos, capturando zonas y venciendo al jefe.
Con 1.500 puntos se genera un cupón `FG-XXX-XXX-XX` con dígito de control (mismo formato que la web). El progreso
se guarda en el dispositivo y no se comparte con la versión web. Para regalar café de verdad hace falta un backend
que registre cada cupón y lo marque como usado.

> Juego de ficción y parodia. No está afiliado ni respaldado por ninguna organización cafetera real.
