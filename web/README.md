# Web floatGardens · Plataforma de control

Sitio estático (HTML/CSS/JS, sin build) con estética de "Greenhouse Control Platform":
foto del invernadero de café, tarjetas de vidrio con métricas grandes, tarjeta verde de
predicción de cosecha, panel de alerta azul y la tabla de la red de terrazas de Kennedy.

- `index.html`: la página completa. Ábrela directo en el navegador.
- `assets/invernadero.jpg`, `assets/cafe.png`: renders 3D hechos con Three.js.
- `tools/render-invernadero.html`: la escena que genera esos renders
  (`?w=2400&h=1350&cx=-0.9&tx=1.1&cy=1.5` para el invernadero, `?mode=cherry&w=600&h=420` para el racimo).
  Necesita servir `node_modules/three@0.160.0` desde la misma raíz.

Los datos que muestra son de ejemplo del juego, no lecturas reales.
