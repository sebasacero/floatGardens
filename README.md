# 🌱☕ Float Gardens: La Conquista del Café

Videojuego de navegador sobre **Float Gardens**, un proyecto colaborativo de **café hidropónico** en Bogotá.
Eres un barista con una **AeroPress** que cultiva café en balsas flotantes, lo prepara, lo vende y hace crecer
la marca ciudad por ciudad hasta **conquistar el mundo del café**, mientras los inspectores de **la Federación**
intentan detenerte.

Mientras juegas **farmeas** (cosechas) café y ganas **Puntos Float**, que se canjean por **café gratis** en los
puntos oficiales de Float Gardens.

> Juego de ficción y parodia. No está afiliado ni respaldado por ninguna organización cafetera real.

## Cómo jugarlo

No necesita instalación ni dependencias: es HTML + CSS + JavaScript puro.

- **Local:** abre `index.html` en el navegador (doble clic). O sirve la carpeta: `python3 -m http.server` y entra a `http://localhost:8000`.
- **En línea:** https://sebasacero.github.io/floatGardens/ (GitHub Pages: *Settings → Pages → Deploy from a branch → `main` / `(root)`*).

### Controles

| Acción | Teclado | Celular |
| --- | --- | --- |
| Moverse | `WASD` / flechas | Joystick izquierdo |
| Cosechar / recargar / servir | `E` o `Enter` | Botón **E** |
| Prensar la AeroPress | Mantener `E` | Mantener **E** |
| Disparar chorro de café | `Espacio` | Botón 💥 |
| Pausa | `P` o `Esc` | — |

### El ciclo del juego

1. **Farmea**: las plantas de café flotan en canales de agua. Cuando tienen cerezas rojas, cosecha con `E`.
2. **Nutre**: el tanque de nutrientes se vacía; si llega a 0, las plantas dejan de crecer.
3. **Prensa**: en la mesa de la AeroPress, 2 cerezas = 1 taza.
4. **Vende**: sirve en la barra antes de que el cliente pierda la paciencia (si eres rápido, deja propina).
5. **Defiéndete**: los inspectores de la Federación decomisan plantas, espantan clientes y te quitan inventario.
   Dispárales chorros de café a presión.
6. **Conquista**: llena la barra de conquista. Al 70% llega el jefe de la ciudad; derrótalo y llega al 100%.

Ruta de conquista: **Bogotá (Chapinero) → Medellín → Cartagena → Nueva York → París → Tokio → la Sede de la Federación**.

Con el dinero de las ventas compras mejoras: más torres hidropónicas, nutrientes premium, AeroPress pro,
presión turbo, tenis de barista, **cultivadores colaborativos** (cosechan por ti) y **banners de promoción**
(llegan más clientes). Las ciudades conquistadas se pueden rejugar para seguir farmeando puntos.

## Puntos Float y café gratis

| Acción | Puntos |
| --- | --- |
| Cada cereza cosechada | +1 |
| Cada café vendido | +10 |
| Inspector espantado | +5 |
| Jefe derrotado | +100 |
| Ciudad conquistada (primera vez) | +200 |

Con **1.500 Puntos Float** (unas 3 a 5 partidas) el jugador genera un cupón (`FG-XXX-XXX-XX`) para mostrar en un punto oficial.

Para ajustar el programa edita `js/config.js`:

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
index.html            página del juego
css/style.css         estilos de las pantallas y controles táctiles
js/config.js          balance, ciudades, mejoras, puntos oficiales
js/storage.js         guardado local y cupones
js/input.js           teclado y controles táctiles
js/render.js          dibujo en canvas (sin imágenes externas)
js/level.js           lógica de cada ciudad: cultivo, clientes, Federación, jefe
js/ui.js              menú, mapa, tienda, Puntos Float
js/game.js            bucle principal y estados
promo/                banners y textos de promoción
```

## Origen

acero-press: documentando el camino hacia 50.000 tazas de café en AeroPress: receta, origen, foto o video de
cada preparación, y la sincronización automática con Instagram.
