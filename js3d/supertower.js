// Súper torres de Kennedy 2089 (guía de diseño v2, references/torre-colgante-2089.md, "Súper torres y acentos").
// Rascacielos de 40 a 110 pisos en la maqueta blanca: retícula de losas y montantes sobre vidrio claro,
// dos retrocesos, jardín en el cielo cada 18 pisos y corona con máquinas, tanque y módulo de la red.
// El color entra solo como ACENTO, con los colores de la cereza de café: pintón, amarilla, madura.
// El azul sigue siendo solo de Arcadia.
(function () {
  const M = (x, y, z, ry, sx, sy, sz) => W.Geom.matrix(x, y, z, 0, ry || 0, 0, sx, sy, sz);
  const BOX = new THREE.BoxGeometry(1, 1, 1);
  const CYL = new THREE.CylinderGeometry(1, 1, 1, 10);
  const FH = 3.4; // alto de piso
  const TONES = ['#f3f3f0', '#eeebe6', '#f1efe9', '#e9ece8', '#efe8e1', '#f5f4f1', '#ebe7e0', '#ece2cf', '#e9e0cf'];
  const ACC = { pinton: '#e0471c', amarilla: '#e8a31f', madura: '#b3121a' };
  const GLASS = '#b3bdbe', METAL = '#3a403d', LEAVES = ['#2c7a2f', '#357f30', '#24682a'];
  const darker = (hex) => '#' + new THREE.Color(hex).lerp(new THREE.Color(METAL), 0.45).getHexString();

  // Devuelve piezas { geo, matrix, color } para fusionar con W.Geom.merge (una sola malla para todas las torres).
  // opts: w, d, floors, y0, accent (color de toda la piel), crown (color del último tramo),
  //       stripe (cara 0-3) + stripeCol (una franja de montantes), slabEvery, rnd, addCollider
  function parts(cx, cz, opts) {
    const rnd = opts.rnd || Math.random;
    const rr = (a, b) => a + rnd() * (b - a);
    const pick = (a) => a[Math.floor(rnd() * a.length)];
    const out = [];
    const box = (x, y, z, sx, sy, sz, color, ry) => out.push({ geo: BOX, matrix: M(x, y, z, ry, sx, sy, sz), color });
    const at = (a, lx, lz) => [cx + lx * Math.cos(a) + lz * Math.sin(a), cz - lx * Math.sin(a) + lz * Math.cos(a)];
    const fl = opts.floors, y0 = opts.y0 || 0;
    const tone = opts.accent || pick(TONES);
    // tres tramos: base 55-70 %, cuerpo hasta 86-94 %, corona
    const f1 = Math.round(fl * rr(0.55, 0.7)), f2 = Math.round(fl * rr(0.86, 0.94));
    const w1 = opts.w, d1 = opts.d, w2 = w1 - rr(3, 5), d2 = d1 - rr(3, 5), w3 = w2 - rr(3, 5), d3 = d2 - rr(3, 5);
    const segs = [[0, f1, w1, d1], [f1, f2, w2, d2], [f2, fl, w3, d3]];
    let top = segs[0];
    segs.forEach(([a, b, sw, sd], si) => {
      if (b <= a || sw < 6) return;
      top = segs[si];
      const crown = opts.crown && si === 2;
      const skin = crown ? opts.crown : tone;
      const glass = opts.accent || crown ? darker(skin) : GLASS;
      const h = (b - a) * FH, yb = y0 + a * FH;
      box(cx, yb + h / 2, cz, sw - 1.4, h, sd - 1.4, glass);
      const every = opts.slabEvery || 1; // el juego dibuja una losa cada 2 pisos para ahorrar vértices
      for (let f = a; f <= b; f++) {
        if ((f - a) % every === 0 || f === b) box(cx, y0 + f * FH + 0.25, cz, sw + 0.3, 0.5, sd + 0.3, skin);
        if (f > a && f < b - 2 && f % 18 === 0) { // jardín en el cielo: lo vivo tiene color
          box(cx, y0 + f * FH + FH / 2, cz, sw - 3, FH, sd - 3, '#e3e7e4');
          for (let q = 0; q < 4; q++) {
            const ang = q * Math.PI / 2, L = q % 2 ? sd : sw, D = q % 2 ? sw : sd;
            const [px, pz] = at(ang, 0, D / 2 - 0.6);
            box(px, y0 + f * FH + 0.9, pz, L - 0.6, 1.1, 0.9, pick(LEAVES), ang);
          }
        }
      }
      for (let q = 0; q < 4; q++) { // montantes: la retícula de ventanas
        const ang = q * Math.PI / 2, L = q % 2 ? sd : sw, D = q % 2 ? sw : sd;
        const n = Math.max(3, Math.round(L / 3.1));
        for (let k = 0; k <= n; k++) {
          const edge = k === 0 || k === n;
          const [px, pz] = at(ang, -L / 2 + (L * k) / n, D / 2 - 0.1);
          box(px, yb + h / 2, pz, edge ? 1 : 0.6, h, 0.6, !edge && opts.stripe === q ? opts.stripeCol : skin, ang);
        }
      }
      if (opts.addCollider) opts.addCollider(cx - sw / 2, cx + sw / 2, cz - sd / 2, cz + sd / 2, yb + h, 'super');
    });
    // corona
    const yt = y0 + top[1] * FH, tw = top[2], td = top[3];
    box(cx, yt + 0.5, cz, tw + 0.6, 1, td + 0.6, opts.crown || tone);
    box(cx + tw * 0.2, yt + 2.2, cz - td * 0.15, tw * 0.35, 3.4, td * 0.35, '#eef0ed');
    out.push({ geo: CYL, matrix: M(cx - tw * 0.25, yt + 2.4, cz + td * 0.2, 0, 1.6, 3.8, 1.6), color: '#e8ebe9' });
    if (rnd() < 0.5) out.push({ geo: CYL, matrix: M(cx + tw * 0.3, yt + 8, cz + td * 0.3, 0, 0.12, 14, 0.12), color: METAL });
    // módulo de invernadero de la red (vidrio blanco, se arma aparte) y su jardinera verde
    box(cx - tw * 0.1, yt + 1.4, cz, Math.min(6, tw - 4), 0.7, Math.min(9, td - 3), '#f4f6f5');
    box(cx - tw * 0.1, yt + 1.9, cz, Math.min(5, tw - 5), 0.4, Math.min(8, td - 4), '#357f30');
    return { parts: out, top: yt };
  }

  // Regla de acento por manzana: como máximo una torre de color (35 % de las manzanas).
  // Las demás: 12 % con corona de color, 10 % con una franja vertical; el resto blancas.
  function blockStyles(n, rnd) {
    const accK = rnd() < 0.35 ? Math.floor(rnd() * n) : -1;
    const accC = rnd() < 0.6 ? ACC.pinton : ACC.amarilla;
    const out = [];
    for (let k = 0; k < n; k++) {
      const u = rnd();
      if (k === accK) out.push({ accent: accC });
      else if (u < 0.12) out.push({ crown: [ACC.pinton, ACC.amarilla, ACC.madura][Math.floor(rnd() * 3)] });
      else if (u < 0.22) out.push({ stripe: Math.floor(rnd() * 4), stripeCol: rnd() < 0.5 ? ACC.pinton : ACC.amarilla });
      else out.push({});
    }
    return out;
  }

  W.SuperTower = { parts, blockStyles, ACC, FLOOR: FH };
})();
