# 🌱☕ Float Gardens: La Conquista del Café

Videojuego de navegador sobre **Float Gardens**, un proyecto colaborativo de **café hidropónico** en Bogotá.
Es un **shooter de mundo abierto en tercera persona** (al estilo de CoD Mobile o Free Fire) en una Bogotá
estilizada: eres un barista con una **AeroPress** que dispara chorros de café a presión, cultiva café en balsas
flotantes, lo vende en sus barrios y le arrebata la ciudad, zona por zona, a **la Federación**.

Mientras juegas **farmeas** (cosechas) café y ganas **Puntos Float**, que se canjean por **café gratis** en los
puntos oficiales de Float Gardens.

> Juego de ficción y parodia. No está afiliado ni respaldado por ninguna organización cafetera real.

## Cómo jugarlo

No necesita instalación ni compilación: HTML + CSS + JavaScript, con [Three.js](https://threejs.org) r149
incluido en `vendor/` (licencia MIT).

- **En línea:** https://sebasacero.github.io/floatGardens/ (GitHub Pages: *Settings → Pages → Deploy from a branch → `main` / `(root)`*).
- **Local:** sirve la carpeta con `python3 -m http.server` y entra a `http://localhost:8000` (también abre con doble clic en `index.html`).
- **Modo clásico 2D:** `clasico.html` (la primera versión del juego, desde arriba).

### Controles

| Acción | Computador | Celular |
| --- | --- | --- |
| Moverse / correr | `WASD` / `Shift` | Joystick (al fondo = correr) |
| Mirar | Mouse (clic en el juego para capturarlo) | Deslizar en la mitad derecha |
| Disparar / apuntar | Clic / clic derecho | 💥 (arrastra para apuntar) / 🎯 |
| Saltar / recargar | `Espacio` / `R` | ⤒ / ↻ |
| Cosechar, vender, tienda | `E` | **E** |
| Mapa / pausa | `M` / `Esc` | 🗺 / ❚❚ |

### El mapa: Bogotá

Una cuadrícula de manzanas con calles y carreras reales (Calle 72, Carrera 7, Av. Caracas, Av. Jiménez...) y un
GPS en pantalla que dice en qué esquina estás. Barrios con estilo propio:

- **Chapinero**: edificios de ladrillo. Aquí está el **HQ de Float Gardens** con su piscina hidropónica y la tienda.
- **La Candelaria**: casas coloniales de colores con techos de teja y calles empedradas.
- **Centro Internacional**: torres de vidrio y la Torre del Centro con su corona de luces.
- **Zona T**, **Teusaquillo**, **Puente Aranda** (bodegas con murales), el **Parque Simón Bolívar** con su lago.
- **Av. Caracas** con carril rojo, estaciones y buses articulados en movimiento.
- Los **cerros orientales** con **Monserrate**, su santuario y el teleférico.
- La **Plaza de Bolívar** frente a la **Sede de la Federación**.
- Llueve de vez en cuando (y los peatones sacan sombrilla).

### El ciclo del juego

1. **Farmea**: cosecha las plantas flotantes del HQ y abre las cajas de café 📦 repartidas por la ciudad.
2. **Dispara**: cada recarga de la AeroPress gasta 1 cereza. Los disparos a la cabeza hacen más daño.
3. **Vende**: los peatones con ☕ te compran café (2 cerezas = $8.000), pero solo en barrios de Float Gardens.
4. **Conquista**: entra a una zona roja, sobrevive a las oleadas de inspectores y guardias, y quédate dentro
   hasta capturarla. Cada zona capturada abre un barrio nuevo para vender y agrega cultivos.
5. **Mejora**: con la plata compra daño, cargador, vida, velocidad y cultivo en la tienda del HQ.
6. **Jefe final**: con las 6 zonas, ve a la Plaza de Bolívar y derrota a **Don Monopolio**.

La vida se regenera si te cubres unos segundos. Si caes, vuelves al HQ y pierdes la mitad de tus cerezas.

## Puntos Float y café gratis

| Acción | Puntos |
| --- | --- |
| Cada cereza cosechada o recogida | +1 |
| Cada café vendido | +10 |
| Enemigo derrotado (mundo abierto) / espantado (clásico) | +5 |
| Zona capturada (mundo abierto) | +100 |
| Jefe derrotado | +100 clásico · +300 Don Monopolio |
| Ciudad conquistada por primera vez (clásico) | +200 |

Los Puntos Float, el dinero y los cupones se comparten entre el mundo abierto y el modo clásico.
Con **1.500 Puntos Float** (unas 3 a 5 partidas) el jugador genera un cupón (`FG-XXX-XXX-XX`) para mostrar en un punto oficial.

Para ajustar el programa edita `js/config.js` (y `js3d/config.js` para el mundo abierto):

- `REDEEM_COST` y los `POINTS_PER_*` definen cuánto cuesta un café.
- `FG.OFFICIAL_POINTS` es la lista de puntos oficiales que aparece en el juego. **Agrega aquí las direcciones reales.**

**Importante:** hoy el progreso y los cupones se guardan en el navegador (`localStorage`). El código trae un
dígito de control (`FG.Vouchers.isValidFormat`) para detectar códigos mal escritos, pero cualquiera con
conocimientos técnicos podría fabricar uno válido. Antes de regalar café real conviene añadir un pequeño backend
(por ejemplo Supabase o Firebase) que registre cada cupón al generarlo, lo marque como usado al canjearlo y limite
cuántos cafés puede canjear una persona por semana.

## Banners de promoción

En [`promo/`](promo/) están los banners en SVG (se pueden abrir en Figma, Illustrator, Inkscape o exportar a PNG
desde el navegador) y textos listos para publicar:

| Archivo | Uso |
| --- | --- |
| `banner-redes-1200x628.svg` | Facebook, X, LinkedIn, vista previa de enlaces |
| `banner-instagram-1080x1080.svg` | Post de Instagram |
| `banner-historia-1080x1920.svg` | Historias y Reels |
| `banner-web-728x90.svg` | Banner web |

Abre `promo/index.html` para verlos todos y copiar los textos. `banner-redes-1200x628.svg` es la pieza maestra:
si cambias sus símbolos (barista, planta, AeroPress, inspector), corre `python3 promo/generar_banners.py`
para regenerar los otros tres formatos.

## Estructura

```
index.html            mundo abierto 3D (página principal)
css/world.css         HUD, menús y controles táctiles del mundo abierto
js3d/config.js        mapa de barrios, calles, zonas, enemigos, mejoras y balance
js3d/textures.js      texturas generadas por código (ladrillo, colonial, vidrio, grafiti...)
js3d/world.js         generación de la ciudad, colisiones, buses, cerros, minimapa
js3d/entities.js      personajes low-poly, enemigos, jefe, peatones, cultivos, botín, zonas
js3d/game.js          bucle principal: jugador, cámara, disparo, zonas, economía, clima
js3d/hud.js           minimapa, brújula, kill feed, vida, munición, hitmarkers
js3d/menus.js         lobby, tienda, Puntos Float, pausa, derrota y victoria
js3d/input.js         teclado + mouse (pointer lock) y controles táctiles
js3d/audio.js         efectos de sonido sintetizados
vendor/three.min.js   motor 3D Three.js r149 (MIT)
clasico.html          modo clásico 2D (usa css/style.css y js/)
js/storage.js         guardado local y cupones (compartido por ambos modos)
promo/                banners y textos de promoción
```

## Origen

acero-press: documentando el camino hacia 50.000 tazas de café en AeroPress: receta, origen, foto o video de
cada preparación, y la sincronización automática con Instagram.
