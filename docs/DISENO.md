# Float Gardens: Bogotá — Documento de diseño

> Un barista, una AeroPress y un PC en una ciudad donde el café solo crece adentro.
> Mundo abierto en tercera persona, campaña por capítulos, Colombia como mapa y el mundo como meta.
> Ficción y parodia: Coffee Land, Don Monopolio y sus inspectores no existen. Los datos sobre café, su historia
> y su cultivo son reales y están en el Códice del juego.

## 1. Concepto

**Premisa.** Bogotá está a 2.600 metros y a 14 grados: demasiado alta y fría para que un cafeto madure al aire libre.
En la ciudad que más tinto toma del país no hay dónde sembrar café... salvo adentro. Casas, bodegas y azoteas con
luz LED y raíces en el agua. Eso es Float Gardens: un proyecto colaborativo de café hidropónico.

**Protagonista.** Un barista joven (el jugador lo nombra) con dos herramientas: una **AeroPress** y un **PC**.
Con la AeroPress prepara (y, en combate, dispara chorros de café a presión). Con el PC mide, anota y afina
recetas: proporción, molienda, temperatura, tiempo, método. Su meta es ser un **referente mundial del café**
desde Bogotá: llegar al Campeonato Mundial de AeroPress con café cultivado en la ciudad.

**Antagonista.** **Coffee Land S.A.**, la corporación que decidió que el café bueno es el que lleva su sello.
Inspectores con fedora, guardias que lanzan sellos de caucho, formularios 27-B, multas por innovar. Su CEO,
**Don Monopolio**, no ha probado un café que no haya firmado antes. Es la parodia del gremio convertido en
candado; el juego distingue con cuidado entre la organización real de los caficultores (que aparece en el Códice
como historia) y esta corporación de ficción.

**Tono.** Agro-punk colombiano: la estética de *Cyberpunk 2077* (noche de neón, hologramas, drones, megacorporaciones)
pero el conflicto es por la comida y la tierra. Los neones anuncian AGROCORP, SEMILLAS Ω, HIDRO·PONIA y DRON·FUMIGA.
Los invernaderos de las azoteas brillan rosado por los LED de cultivo. En vez de implantes, **injertos** (mejoras
biológicas). Humor bogotano, cariño por el oficio, cero cinismo con el café.

**Pilares.**
1. *El café es real.* Cada misión enseña algo cierto. Si el jugador termina el juego sabiendo qué es el TDS,
   quién fue Baba Budan y por qué Castillo salvó la cosecha, ganamos.
2. *Bogotá de noche.* La ciudad es el espectáculo: lluvia, asfalto mojado, neón, bruma de los cerros.
3. *Farmear es compartir.* Los Puntos Float se canjean por café gratis en los puntos oficiales de Float Gardens.
   El juego y el proyecto real se alimentan.
4. *Una taza se juzga a ciegas.* El laboratorio premia la receta, no el cuento.

## 2. Historia

### Sinopsis
Capítulo a capítulo, el barista recupera los barrios de Bogotá que Coffee Land tiene sitiados, mientras seis
mentores le enseñan el café desde el origen hasta la taza. Al final enfrenta a Don Monopolio en la Plaza de Bolívar,
no con un disparo final sino con una taza: Don Monopolio la prueba y, por primera vez, no encuentra qué sellar.
Bogotá queda cultivando su propio café y se abren las expediciones por Colombia; después, el mundo.

### Personajes
| Personaje | Quién es | Qué enseña | Dónde |
| --- | --- | --- | --- |
| **Abuela Rosa** | Chapolera de Chinchiná, cuarenta cosechas | Cosecha, la cereza, el tinto, la idea de Float Gardens | HQ, Chapinero |
| **El Profe Arango** | Agrónomo de Cenicafé | Arábica vs. robusta, altura, roya y Castillo | Parque de Teusaquillo |
| **Kaldi** | Dice ser el pastor etíope de la leyenda | Origen: Kaffa, los sufíes, Moca, Baba Budan, De Clieu, Europa | Lago del Simón Bolívar |
| **Padre Francisco** | Cura de Salazar de las Palmas | Jesuitas, la penitencia de sembrar café, 1835, 1927 | Chorro de Quevedo |
| **Catalina** | Catadora Q, jueza de campeonatos | La escala de 100 puntos, catación, rueda de sabores, agua | Zona T |
| **Alanita** | Ingeniera, campeona de AeroPress | La AeroPress, el mundial de Oslo 2008, Melitta, espresso, Chemex, tueste | Centro Internacional |
| **Don Monopolio** | CEO de Coffee Land | El antagonista; al final, el primer converso | Torre Coffee Land, Plaza de Bolívar |

### Capítulos (implementados en `Scripts/Game/Story.cs`)
1. **Una AeroPress y un PC** (Chapinero): hablar con Rosa, cosechar 8 cerezas, vender 3 cafés, preparar una taza
   de 74+ en el laboratorio.
2. **Los que saben** (Teusaquillo, Simón Bolívar, La Candelaria): cada mentor pide recuperar su barrio a cambio de
   la siguiente lección. Tres zonas capturadas, tres bloques de historia del café.
3. **Taza de Excelencia** (Zona T, Centro Internacional): Catalina exige una taza de 84+; Alanita entrena para el
   mundial si se liberan la Zona T y el Centro.
4. **Coffee Land** (Puente Aranda, Plaza de Bolívar): el centro logístico de la corporación y el enfrentamiento final.

### Capítulos siguientes (diseñados, no implementados)
5. **Expediciones** — Viajar a diez regiones cafeteras de Colombia (ya están en el mapa, con sus variedades y Códice).
   Cada una es un nivel propio: la finca de ladera del Eje Cafetero con Yipao, la Sierra Nevada con los arhuacos,
   los cañones de Nariño, el Huila de la Taza de Excelencia. Misiones de cosecha, proceso (lavado, honey, natural),
   secado y subasta.
6. **El mundo** — Oslo (el primer mundial de AeroPress), Melbourne (la cultura del flat white), Tokio (el kissaten y
   el filtrado lento), Addis Abeba (la ceremonia del café, el origen). Cada ciudad, un campeonato.
7. **El sello propio** — Final: el barista funda la certificación abierta de Float Gardens, con datos del PC
   compartidos y cupones de café gratis para quien farmea. Coffee Land abre un invernadero.

## 3. Mundo

### Bogotá (mundo abierto, 12 × 12 manzanas)
Zona T, Chapinero (HQ), Teusaquillo, Parque Simón Bolívar con lago, Centro Internacional con la Torre, La Candelaria
y el Chorro de Quevedo, Plaza de Bolívar, Puente Aranda industrial, Av. Caracas con buses articulados, cerros
orientales con Monserrate y teleférico. Ciclo de día y noche de 14 minutos, lluvia, asfalto mojado, neones,
hologramas, invernaderos en las azoteas, drones fumigadores de la corporación.

### Colombia (expediciones)
Diez regiones con posición real en el mapa, altitud, perfil de taza, variedades que regalan y entradas del Códice:
Bogotá, Tequendama y Sumapaz, Santander, Eje Cafetero, Huila, Nariño, Cauca, Tolima, Suroeste antioqueño y
Sierra Nevada. Se desbloquean por capítulo.

### El mundo (futuro)
Oslo, Melbourne, Tokio, Addis Abeba.

## 4. Sistemas

| Sistema | Qué hace | Archivo |
| --- | --- | --- |
| **Cultivo** | Balsas hidropónicas en el HQ y en las zonas capturadas; cosecha de cerezas; mejora "Hidroponía avanzada" | `Actors/Props.cs` (Planter) |
| **Venta** | Peatones con taza compran AeroPress solo en barrios de Float Gardens | `Game/GameManager.cs` |
| **Combate** | AeroPress hitscan con retroceso, headshots, recarga con cerezas; inspectores cuerpo a cuerpo, guardias a distancia, jefe | `Actors/PlayerController.cs`, `Actors/Enemy.cs` |
| **Territorio** | Seis zonas de captura con oleadas; cada zona abre un barrio como mercado | `GameManager.Zones` |
| **Campaña** | Cuatro capítulos, dieciséis misiones (hablar, cosechar, vender, capturar, preparar, jefe), diálogos con máquina de escribir | `Game/Story.cs`, `UI/Dialogue.cs`, `Actors/StoryNPC.cs` |
| **Laboratorio AeroPress** | Cinco variables + método; modelo de extracción (EY, TDS) y puntaje de cata sobre 100 con notas y consejo; récord por variedad | `Core/Brewing.cs`, `UI/MenusStory.cs` |
| **Códice del Café** | 50 entradas en 8 categorías, desbloqueadas por misiones, regiones y recetas | `Core/Codex.cs` |
| **Expediciones** | Mapa de Colombia dibujado por código, regiones, variedades | `Core/Codex.cs`, `UI/MenusStory.cs` |
| **Progresión** | Mejoras (daño, cargador, vida, velocidad, cultivo); futuro: injertos | `Core/FGConfig.cs` |
| **Puntos Float** | +1 por cereza, +10 venta, +5 KO, +100 zona, +300 jefe, hasta +60 por taza; 1.500 = cupón de café gratis con dígito de control | `Core/FGSave.cs` |

### El laboratorio, en detalle
El modelo es una simplificación honesta de la extracción real. Parte de 20 % de extracción con una receta de
referencia y la mueve con cada variable: moler fino, calentar y alargar el tiempo extraen más; grueso, frío y corto,
menos. El TDS es lo extraído repartido en el agua. El puntaje castiga salirse del rango dorado (18-22 % de extracción,
1,15-1,35 % de TDS), alejarse de la temperatura y la molienda que prefiere cada variedad (la Geisha pide 87 °C y
molienda fina; la Castillo, 93 °C), y una proporción fuera de 1:14-1:17. La receta inicial de principiante
(18 g, 250 ml, molienda gruesa, agua hirviendo, 45 s) da unos 71 puntos; la óptima, unos 90. Entre las dos
está el juego.

## 5. Referencias cafeteras del juego (todas en el Códice)

**Origen y leyenda:** Kaldi y las cabras de Kaffa · qahwa → kahve → café · los sufíes de Yemen · el puerto de Moca ·
las siete semillas de Baba Budan (c. 1670) · Gabriel de Clieu y la plántula de Martinica (1723) · el "bautizo" de
Clemente VIII · las penny universities de Londres y Lloyd's · la Cantata del café de Bach · los 60 granos de Beethoven ·
Balzac · el Motín del Té y el café en Estados Unidos.

**Métodos:** Melitta Bentz y el filtro de papel (1908) · Moriondo (1884), Bezzera (1901) y la palanca de Gaggia (1947) ·
la Chemex de Schlumbohm (1941) · la tercera ola (Trish Rothgeb) · Alan Adler y la AeroPress (2005) · el primer mundial
de AeroPress en Oslo (2008) y su formato de ocho minutos y jueces a ciegas.

**Colombia:** los jesuitas y el siglo XVIII · la penitencia del padre Francisco Romero · la primera exportación por
Cúcuta (1835) · el tinto, el perico y el pintado · 1927 y la organización de los caficultores · Cenicafé en Chinchiná
(1938) · las variedades Colombia, Tabi, Castillo y Cenicafé 1 · la roya de 2008-2011 · el Paisaje Cultural Cafetero
(UNESCO 2011) · chapoleras y yipaos · Huila y la Taza de Excelencia · Nariño a 2.300 m y la inversión térmica · la
Sierra Nevada y los pueblos arhuaco, kogui y wiwa · dos cosechas: principal y mitaca · suave lavado, excelso, saco
de 70 kg · el contrato C de Nueva York y la prima colombiana.

**Cultivo y proceso:** arábica (44 cromosomas) y robusta (22) · altura y densidad del grano · anatomía de la cereza
(pulpa, mucílago, pergamino, película plateada, caracol) · sombra, suelo volcánico, 18-22 °C · hidroponía · proceso
lavado, natural, honey y anaeróbico · trilla y café verde · tueste: primer crack ~196 °C, segundo ~224 °C, Maillard.

**Preparación y cata:** las cinco variables · proporción 1:15-1:17 · TDS 1,15-1,35 % y extracción 18-22 % · agua
75-150 ppm · molienda de muelas vs. cuchillas · método estándar e invertido · protocolo de catación (8,25 g / 150 ml,
93 °C, cuatro minutos, sorber con ruido) · la escala de 100 puntos y los Q graders · la rueda de sabores · la Geisha de
La Esmeralda (2004).

**Variedades jugables:** Castillo, Caturra, Típica, Bourbon, Tabi, Pink Bourbon, Geisha, Cenicafé 1, Wush Wush, Maragogipe.

## 6. Dirección de arte y sonido

- **Luz:** sol de media tarde con sombras largas; de noche, luna fría, ambiente violeta, neones magenta/cian/lima,
  LEDs rosados de cultivo; 24 luces reales que siguen a la cámara. Posprocesado de cine: ACES, bloom, lens flare,
  etalonaje con sombras frías y luces cálidas, grano, viñeta, profundidad de campo lejana, motion blur de cámara.
- **Materiales:** texturas PBR procedurales a 128 px/m con relieve, brillo y ventanas que se encienden.
- **Personajes:** hoy, humanoides de primitivas con animación procedural. Siguiente paso: Mixamo (menú
  *Float Gardens → Personajes*), después personajes propios.
- **UI:** HUD estilo shooter móvil (minimapa que gira, brújula, kill feed), menús tipo lobby, caja de diálogo
  con franjas de cine, Códice como enciclopedia, mapa de Colombia.
- **Sonido (por hacer):** ambiente de Bogotá (buses, lluvia, campanas de La Candelaria), música con tiple y
  sintetizadores, voces en español bogotano, el sonido real de una AeroPress al presionar.

## 7. Qué hace falta para que sea AAA de verdad

Lo honesto: *Cyberpunk 2077* lo hicieron unas 500 personas en ocho años. Lo que sigue es el camino realista de un
estudio pequeño para que el juego **se vea y se sienta** AAA.

| Área | Hoy | Siguiente | AAA |
| --- | --- | --- | --- |
| Personajes | Primitivas | Mixamo (gratis, 1 día) | Personajes propios con captura de movimiento |
| Ciudad | Procedural con detalle | Paquetes (Synty, Kenney) para hitos: Monserrate, la Torre, el Chorro | Modelado a mano de 5-6 hitos icónicos |
| Animación | Procedural | Animator con Mixamo | Cinemáticas con Timeline + Cinemachine |
| Audio | Sintetizado | Grabaciones de Bogotá + música original | Voces profesionales, mezcla adaptativa |
| Mundo | Bogotá 12×12 | Expediciones por Colombia (niveles) | Streaming de mundo, tráfico, vehículos manejables |
| Narrativa | 4 capítulos, 16 misiones | Capítulos 5-7, misiones secundarias, reputación por barrio | Decisiones con consecuencias, finales múltiples |
| Técnica | URP, Forward+, SSAO | Perfilado en celular, LOD, oclusión | Ray tracing opcional, DLSS/FSR |

**Equipo mínimo para la versión "AAA indie":** 1 programador, 1 artista 3D, 1 artista técnico/VFX, 1 diseñador
narrativo, 1 compositor/sonidista, QA. 12-18 meses. Con el código actual como base.

## 8. Estado

Todo lo descrito en las secciones 2-5 está implementado en código y compila contra Unity 6 / URP 17.6, pero todavía
no se ha probado dentro del editor. El siguiente paso es jugarlo, calibrar y corregir con capturas y la consola.
