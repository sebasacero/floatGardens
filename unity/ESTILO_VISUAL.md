# Estilo visual por plataforma

Float Gardens se ve distinto en cada plataforma, con la misma escena y el mismo código de juego:

| Plataforma | Estilo | Qué cambia |
| --- | --- | --- |
| **Móvil** (Android, iOS) | Neón tipo juguete | Colores muy saturados, materiales de plástico brillante, mucho bloom, niebla violeta, sin grano ni viñeta. Es el más liviano. |
| **Web** (WebGL) | Cómic / grafiti 2.5D | Colores planos y mates, sombras duras como tinta, contraste alto, sin tonemapping, grano tipo papel, texto con borde negro grueso y una cámara de lente larga y más alejada que aplana la perspectiva. |
| **Consola** (PlayStation, Xbox, Switch) | Pintura | Luz cálida, sombras suaves, tonos partidos ámbar y verde azulado, materiales mates, grano fino, un toque de aberración cromática y antialiasing SMAA. Es el más artístico. |

La versión de computador (Windows, Mac, Linux) usa el estilo de consola. Se cambia en `FG_StyleSet` (*Desktop Uses*).

## Cómo funciona

- `Scripts/Style/FGStyleProfile.cs` es un **ScriptableObject** con todo el look de una plataforma: paleta de la
  interfaz, tema de texto (tamaño, sombra o borde), materiales (saturación, tinte, brillo, neón), luz y cielo,
  niebla, posprocesado URP (bloom, tonemapping, color, viñeta, grano, aberración, tonos partidos, antialiasing) y
  cámara (campo de visión y distancia).
- `Scripts/Style/FGStyleSet.cs` dice qué perfil usa cada plataforma.
- `Scripts/Style/FGStyle.cs` elige el perfil al arrancar. Usa los símbolos de compilación de Unity
  (`UNITY_ANDROID`, `UNITY_IOS`, `UNITY_WEBGL`, `UNITY_PS5`...), que cambian con la plataforma activa. Por eso la misma
  escena cambia sola de estilo cuando cambias de plataforma en **File → Build Profiles**, tanto en el editor como en el build.
- `Atmosphere`, `FGMaterials`, `UIKit` y `ThirdPersonCamera` leen el perfil. Los colores de siempre
  (`UIKit.Yellow`, `UIKit.Green`...) ahora salen de la paleta, así que el HUD y los menús no tuvieron que cambiar.

Si los assets no existen, se usan los valores por defecto escritos en `FGStyleProfile.CreateDefault`, así que el juego
nunca se queda sin estilo.

## Uso en el editor

1. **Float Gardens → Configurar todo y crear escena** (o **Estilo visual → Crear perfiles por plataforma**) crea
   `Resources/FloatGardens/FG_Style_Movil`, `FG_Style_Web`, `FG_Style_Consola` y `FG_StyleSet`.
2. Selecciona un perfil y ajusta los valores en el inspector. El menú no pisa perfiles que ya existen.
3. Para ver otro estilo sin cambiar de plataforma: **Float Gardens → Estilo visual → Vista previa: ...** y dale Play.
   Vuelve a **Vista previa: según plataforma activa** antes de compilar (la vista previa solo afecta al editor).

## Límites y siguiente paso

Con el URP estándar se logra la dirección de arte (paleta, luz, posprocesado, cámara), pero no un cómic o una pintura
completos. Para eso faltan shaders propios, que se pueden agregar después como campos del perfil:

- **Web:** contornos de tinta (Renderer Feature de bordes por profundidad y normales), sombreado *toon* por bandas y
  tramado de puntos (halftone).
- **Consola:** filtro de pinceladas (Kuwahara), texturas de lienzo y luz volumétrica.
- **Móvil:** materiales *matcap* de juguete y modelos low-poly redondeados.

También conviene un URP Asset por plataforma (sombras, render scale, MSAA) en **Quality Settings**, para que móvil rinda
y consola use la máxima calidad.
