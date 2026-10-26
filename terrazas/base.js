// Float Gardens: Terrazas de Kennedy — utilidades base.
// Azar con semilla, un constructor de geometría combinada (un solo draw call por material)
// y las texturas pintadas en canvas: fachadas de ladrillo, placa de concreto, avisos de Float Gardens...
(function () {
  const FG = (window.FGT = window.FGT || {});

  THREE.ColorManagement.legacyMode = false;

  FG.rng = function (seed) {
    return function () {
      seed |= 0; seed = (seed + 0x6D2B79F5) | 0;
      let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
      t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
      return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
  };
  FG.C = (h) => new THREE.Color(h);
  FG.M4 = function (x, y, z, ry = 0, sx = 1, sy = 1, sz = 1, rx = 0, rz = 0) {
    return new THREE.Matrix4().compose(
      new THREE.Vector3(x, y, z),
      new THREE.Quaternion().setFromEuler(new THREE.Euler(rx, ry, rz, 'YXZ')),
      new THREE.Vector3(sx, sy, sz));
  };

  // Geometría combinada con colores por vértice. Cada material del mundo estático tiene un Geo.
  class Geo {
    constructor() { this.p = []; this.n = []; this.u = []; this.c = []; }
    v(x, y, z, nx, ny, nz, u, v, c) {
      this.p.push(x, y, z); this.n.push(nx, ny, nz); this.u.push(u, v); this.c.push(c.r, c.g, c.b);
    }
    quad(a, b, c, d, col, u0 = 0, v0 = 0, u1 = 1, v1 = 1) {
      const e1 = [b[0] - a[0], b[1] - a[1], b[2] - a[2]], e2 = [d[0] - a[0], d[1] - a[1], d[2] - a[2]];
      let nx = e1[1] * e2[2] - e1[2] * e2[1], ny = e1[2] * e2[0] - e1[0] * e2[2], nz = e1[0] * e2[1] - e1[1] * e2[0];
      const l = Math.hypot(nx, ny, nz) || 1; nx /= l; ny /= l; nz /= l;
      const P = [a, b, c, d], U = [[u0, v0], [u1, v0], [u1, v1], [u0, v1]];
      for (const i of [0, 1, 2, 0, 2, 3]) this.v(P[i][0], P[i][1], P[i][2], nx, ny, nz, U[i][0], U[i][1], col);
    }
    // Caja con UV en metros (para texturas que se repiten) y opción de omitir caras.
    box(x0, x1, y0, y1, z0, z1, col, top = col, skip = {}) {
      if (!skip.front) this.quad([x0, y0, z1], [x1, y0, z1], [x1, y1, z1], [x0, y1, z1], col, x0 / 2, y0 / 2, x1 / 2, y1 / 2);
      if (!skip.back) this.quad([x1, y0, z0], [x0, y0, z0], [x0, y1, z0], [x1, y1, z0], col, -x1 / 2, y0 / 2, -x0 / 2, y1 / 2);
      if (!skip.right) this.quad([x1, y0, z1], [x1, y0, z0], [x1, y1, z0], [x1, y1, z1], col, -z1 / 2, y0 / 2, -z0 / 2, y1 / 2);
      if (!skip.left) this.quad([x0, y0, z0], [x0, y0, z1], [x0, y1, z1], [x0, y1, z0], col, z0 / 2, y0 / 2, z1 / 2, y1 / 2);
      if (!skip.top) this.quad([x0, y1, z1], [x1, y1, z1], [x1, y1, z0], [x0, y1, z0], top, x0 / 2, -z1 / 2, x1 / 2, -z0 / 2);
      if (skip.bottom === false) this.quad([x0, y0, z0], [x1, y0, z0], [x1, y0, z1], [x0, y0, z1], col);
    }
    // Copia una geometría de three (transformada) dentro del lote.
    geom(g, m, col) {
      const gg = g.index ? g.toNonIndexed() : g, P = gg.attributes.position, N = gg.attributes.normal, UV = gg.attributes.uv;
      const nm = new THREE.Matrix3().getNormalMatrix(m), a = new THREE.Vector3(), n = new THREE.Vector3();
      for (let i = 0; i < P.count; i++) {
        a.fromBufferAttribute(P, i).applyMatrix4(m);
        n.fromBufferAttribute(N, i).applyMatrix3(nm).normalize();
        this.v(a.x, a.y, a.z, n.x, n.y, n.z, UV ? UV.getX(i) : 0, UV ? UV.getY(i) : 0, col);
      }
    }
    build() {
      const g = new THREE.BufferGeometry();
      g.setAttribute('position', new THREE.Float32BufferAttribute(this.p, 3));
      g.setAttribute('normal', new THREE.Float32BufferAttribute(this.n, 3));
      g.setAttribute('uv', new THREE.Float32BufferAttribute(this.u, 2));
      g.setAttribute('color', new THREE.Float32BufferAttribute(this.c, 3));
      g.computeBoundingSphere();
      return g;
    }
  }
  FG.Geo = Geo;

  // ---------- texturas ----------
  function cnv(w, h) { const c = document.createElement('canvas'); c.width = w; c.height = h; return [c, c.getContext('2d')]; }
  function tex(c, repeat) {
    const t = new THREE.CanvasTexture(c);
    t.encoding = THREE.sRGBEncoding;
    t.anisotropy = 8;
    if (repeat) t.wrapS = t.wrapT = THREE.RepeatWrapping;
    return t;
  }
  function rrect(x, cx, cy, w, h, r) {
    x.beginPath(); x.moveTo(cx + r, cy); x.arcTo(cx + w, cy, cx + w, cy + h, r); x.arcTo(cx + w, cy + h, cx, cy + h, r);
    x.arcTo(cx, cy + h, cx, cy, r); x.arcTo(cx, cy, cx + w, cy, r); x.closePath();
  }
  function bricks(x, X, Y, W, H, base) {
    x.fillStyle = '#d9c6ae'; x.fillRect(X, Y, W, H);
    const bh = 11, bw = 26;
    for (let r = 0; r * bh < H; r++) for (let k = -1; k * bw < W; k++) {
      const off = (r % 2) * bw / 2, l = 24 + Math.random() * 14;
      x.fillStyle = `hsl(${base + Math.random() * 8 - 4},${52 + Math.random() * 12}%,${l + 18}%)`;
      x.fillRect(X + k * bw + off + 1, Y + r * bh + 1, bw - 2, bh - 2);
    }
  }
  function windowAt(x, X, Y, W, H, frame = '#f4f1ea', grille = false) {
    x.fillStyle = frame; x.fillRect(X - 6, Y - 6, W + 12, H + 14);
    const g = x.createLinearGradient(X, Y, X + W, Y + H);
    g.addColorStop(0, '#2b4a6e'); g.addColorStop(0.55, '#4f86b0'); g.addColorStop(1, '#20344f');
    x.fillStyle = g; x.fillRect(X, Y, W, H);
    x.fillStyle = 'rgba(255,255,255,.35)';
    x.beginPath(); x.moveTo(X + W * 0.15, Y + H); x.lineTo(X + W * 0.45, Y); x.lineTo(X + W * 0.6, Y); x.lineTo(X + W * 0.3, Y + H); x.fill();
    x.fillStyle = frame; x.fillRect(X + W / 2 - 3, Y, 6, H); x.fillRect(X, Y + H * 0.42, W, 5);
    if (grille) {
      x.strokeStyle = '#3d3f46'; x.lineWidth = 3;
      for (let i = 1; i < 6; i++) { x.beginPath(); x.moveTo(X + i * W / 6, Y - 4); x.lineTo(X + i * W / 6, Y + H + 4); x.stroke(); }
    }
    x.fillStyle = 'rgba(0,0,0,.25)'; x.fillRect(X - 8, Y + H + 8, W + 16, 6);
  }

  // Atlas de fachadas 4x2 (como en la maqueta 3D de la terraza): fila de arriba = pisos con ventanas,
  // fila de abajo = primer piso con tienda, puerta o miscelánea. u = columna/4, v = .25 (local) o .5 (pisos).
  FG.facadeAtlas = function () {
    const [c, x] = cnv(1024, 512);
    const SHOPS = [['TIENDA', '#2f9e57'], null, ['MISCELÁNEA', '#d9452b'], null];
    for (let col = 0; col < 4; col++) {
      const X = col * 256;
      for (let row = 0; row < 2; row++) {
        const Y = row * 256;
        if (col === 1) {
          x.fillStyle = '#f3efe6'; x.fillRect(X, Y, 256, 256);
          x.fillStyle = 'rgba(0,0,0,.06)';
          for (let i = 0; i < 40; i++) x.fillRect(X + Math.random() * 256, Y + Math.random() * 256, 20, 3);
        } else bricks(x, X, Y, 256, 256, col === 2 ? 14 : 18);
        x.fillStyle = 'rgba(0,0,0,.18)'; x.fillRect(X, Y + 246, 256, 10);
        if (row === 0) {
          if (col === 0) windowAt(x, X + 58, Y + 60, 140, 118);
          if (col === 1) windowAt(x, X + 50, Y + 58, 156, 120, '#ffffff', true);
          if (col === 2) {
            windowAt(x, X + 70, Y + 50, 116, 110);
            x.fillStyle = '#e9e4da'; x.fillRect(X + 40, Y + 176, 176, 14);
            x.strokeStyle = '#2f3138'; x.lineWidth = 4;
            for (let i = 0; i < 12; i++) { x.beginPath(); x.moveTo(X + 46 + i * 15, Y + 176); x.lineTo(X + 46 + i * 15, Y + 130); x.stroke(); }
            x.fillRect(X + 40, Y + 126, 176, 6);
          }
          if (col === 3) windowAt(x, X + 96, Y + 70, 64, 60);
        } else {
          const shop = SHOPS[col];
          if (shop) {
            x.fillStyle = '#9aa1aa'; x.fillRect(X + 24, Y + 60, 208, 190);
            x.fillStyle = '#7c838c'; for (let i = 0; i < 24; i++) x.fillRect(X + 24, Y + 64 + i * 8, 208, 3);
            x.fillStyle = shop[1]; x.fillRect(X + 14, Y + 26, 228, 30);
            x.fillStyle = '#fff'; x.font = 'bold 22px sans-serif'; x.textAlign = 'center'; x.fillText(shop[0], X + 128, Y + 49);
          }
          if (col === 1) {
            x.fillStyle = '#6b4a32'; x.fillRect(X + 28, Y + 70, 70, 182);
            x.fillStyle = '#d8b07a'; x.fillRect(X + 86, Y + 160, 6, 8);
            windowAt(x, X + 124, Y + 84, 104, 92, '#ffffff', true);
          }
          if (col === 3) windowAt(x, X + 96, Y + 80, 64, 56);
        }
      }
    }
    return tex(c);
  };

  // Ladrillo que se repite (culatas, muros, antepechos): 1 repetición = 2 m.
  FG.brickTex = function () {
    const [c, x] = cnv(256, 256);
    bricks(x, 0, 0, 256, 256, 17);
    for (let i = 0; i < 30; i++) {
      x.fillStyle = `rgba(40,20,10,${Math.random() * 0.08})`;
      x.fillRect(Math.random() * 256, Math.random() * 256, 4 + Math.random() * 30, 2 + Math.random() * 40);
    }
    return tex(c, true);
  };

  // Placa de concreto de las terrazas, con manchas de agua y juntas.
  FG.concreteTex = function () {
    const [c, x] = cnv(256, 256);
    x.fillStyle = '#b7b2a8'; x.fillRect(0, 0, 256, 256);
    for (let i = 0; i < 1600; i++) {
      const v = 150 + Math.random() * 60;
      x.fillStyle = `rgba(${v},${v - 4},${v - 10},.35)`; x.fillRect(Math.random() * 256, Math.random() * 256, 2, 2);
    }
    for (let i = 0; i < 7; i++) {
      const g = x.createRadialGradient(0, 0, 0, 0, 0, 40);
      g.addColorStop(0, 'rgba(70,70,60,.16)'); g.addColorStop(1, 'rgba(70,70,60,0)');
      x.save(); x.translate(Math.random() * 256, Math.random() * 256); x.scale(1.6, 0.8); x.fillStyle = g; x.fillRect(-40, -40, 80, 80); x.restore();
    }
    x.strokeStyle = 'rgba(80,76,70,.35)'; x.lineWidth = 2; x.strokeRect(1, 1, 254, 254);
    return tex(c, true);
  };

  FG.asphaltTex = function () {
    const [c, x] = cnv(256, 256);
    x.fillStyle = '#4b4d52'; x.fillRect(0, 0, 256, 256);
    for (let i = 0; i < 2500; i++) {
      const v = 55 + Math.random() * 40;
      x.fillStyle = `rgb(${v},${v},${v + 3})`; x.fillRect(Math.random() * 256, Math.random() * 256, 2, 2);
    }
    return tex(c, true);
  };

  function leafLogo(x, cx, cy, s, fill) {
    x.save(); x.translate(cx, cy); x.rotate(-0.5); x.fillStyle = fill;
    x.beginPath(); x.ellipse(0, 0, s * 0.42, s * 0.22, 0, 0, 7); x.fill();
    x.strokeStyle = 'rgba(0,0,0,.28)'; x.lineWidth = s * 0.05;
    x.beginPath(); x.moveTo(-s * 0.38, 0); x.lineTo(s * 0.38, 0); x.stroke(); x.restore();
  }
  FG.leafLogo = leafLogo;

  // Aviso de Float Gardens. on=false: apagado (punto de control sin tocar).
  FG.signTex = function (sub, on = true) {
    const w = 512, h = 208, [c, x] = cnv(w, h);
    rrect(x, 6, 6, w - 12, h - 12, 28); x.fillStyle = '#ffffff'; x.fill();
    rrect(x, 16, 16, w - 32, h - 32, 22);
    const g = x.createLinearGradient(0, 0, 0, h);
    if (on) { g.addColorStop(0, '#22b552'); g.addColorStop(1, '#0d7034'); } else { g.addColorStop(0, '#53615a'); g.addColorStop(1, '#2f3a34'); }
    x.fillStyle = g; x.fill();
    leafLogo(x, h * 0.42, h * 0.46, h * 0.55, on ? '#eaffd8' : '#a9b5ad');
    x.fillStyle = on ? '#fff' : '#c9d2cc'; x.textAlign = 'left'; x.textBaseline = 'middle';
    x.font = `${Math.round(h * 0.3)}px "Lilita One", "Arial Black", sans-serif`;
    x.shadowColor = 'rgba(0,0,0,.35)'; x.shadowOffsetY = 4; x.fillText('FLOAT GARDENS', h * 0.78, h * 0.43, w - h * 0.95);
    x.shadowOffsetY = 0; x.font = `${Math.round(h * 0.12)}px "Baloo 2", sans-serif`;
    x.fillStyle = on ? '#eaffd8' : '#a9b5ad'; x.fillText(sub, h * 0.8, h * 0.72, w - h);
    return tex(c);
  };

  // Fachada verde de la torre hidropónica (pisos de cultivo detrás del vidrio).
  FG.towerTex = function () {
    const [c, x] = cnv(512, 512);
    for (let f = 0; f < 4; f++) {
      const Y = f * 128;
      x.fillStyle = '#d9dfe5'; x.fillRect(0, Y + 106, 512, 22);
      const g = x.createLinearGradient(0, Y, 0, Y + 106);
      g.addColorStop(0, '#c9f2ff'); g.addColorStop(1, '#5aa9c9'); x.fillStyle = g; x.fillRect(0, Y, 512, 106);
      for (let r = 0; r < 3; r++) {
        x.fillStyle = '#1f5a2c'; x.fillRect(0, Y + 24 + r * 30, 512, 6);
        for (let k = 0; k < 26; k++) {
          x.fillStyle = `hsl(${100 + Math.random() * 35},65%,${34 + Math.random() * 18}%)`;
          x.beginPath(); x.arc(k * 20 + 8 + Math.random() * 4, Y + 20 + r * 30, 8 + Math.random() * 3, 0, 7); x.fill();
        }
      }
      x.fillStyle = 'rgba(255,255,255,.22)'; x.fillRect(0, Y, 512, 8);
      for (let b = 0; b <= 4; b++) { x.fillStyle = '#2a3140'; x.fillRect(b * 128 - 4, Y, 8, 106); }
    }
    return tex(c, true);
  };

  // Colchón viejo de terraza (sirve de trampolín).
  FG.mattressTex = function () {
    const [c, x] = cnv(256, 128);
    x.fillStyle = '#f1e9da'; x.fillRect(0, 0, 256, 128);
    for (let i = 0; i < 256; i += 32) { x.fillStyle = '#7fb3e0'; x.fillRect(i, 0, 14, 128); }
    x.fillStyle = 'rgba(120,90,40,.25)'; x.beginPath(); x.ellipse(170, 70, 40, 22, 0.3, 0, 7); x.fill();
    for (let i = 0; i < 8; i++) for (let j = 0; j < 4; j++) { x.fillStyle = 'rgba(0,0,0,.18)'; x.beginPath(); x.arc(16 + i * 32, 16 + j * 32, 3, 0, 7); x.fill(); }
    return tex(c);
  };

  // Letra coleccionable F-L-O-A-T en un grano de café.
  FG.letterTex = function (ch) {
    const [c, x] = cnv(128, 128);
    x.fillStyle = '#ffd34d'; x.beginPath(); x.arc(64, 64, 60, 0, 7); x.fill();
    x.fillStyle = '#e09b16'; x.beginPath(); x.arc(64, 64, 52, 0, 7); x.fill();
    x.fillStyle = '#fff'; x.font = '84px "Lilita One", "Arial Black", sans-serif'; x.textAlign = 'center'; x.textBaseline = 'middle';
    x.shadowColor = 'rgba(80,40,0,.6)'; x.shadowOffsetY = 5; x.fillText(ch, 64, 70);
    return tex(c);
  };

  FG.glowTex = function () {
    const [c, x] = cnv(128, 128);
    const g = x.createRadialGradient(64, 64, 0, 64, 64, 64);
    g.addColorStop(0, 'rgba(255,255,255,1)'); g.addColorStop(0.35, 'rgba(255,255,255,.45)'); g.addColorStop(1, 'rgba(255,255,255,0)');
    x.fillStyle = g; x.fillRect(0, 0, 128, 128);
    return tex(c);
  };

  FG.cloudTex = function () {
    const [c, x] = cnv(256, 128);
    for (let i = 0; i < 14; i++) {
      const px = 40 + Math.random() * 176, py = 50 + Math.random() * 40, r = 18 + Math.random() * 30;
      const g = x.createRadialGradient(px, py, 0, px, py, r);
      g.addColorStop(0, 'rgba(255,255,255,.95)'); g.addColorStop(0.6, 'rgba(255,255,255,.6)'); g.addColorStop(1, 'rgba(255,255,255,0)');
      x.fillStyle = g; x.fillRect(px - r, py - r, r * 2, r * 2);
    }
    return tex(c);
  };

  // Ropa colgada: una camiseta, una sábana o un jean (para los tendederos).
  FG.clothesColors = ['#e94b5a', '#ffd23f', '#3bb3ff', '#ffffff', '#7a5cff', '#41c46a', '#ff8c3a', '#2f4f9a'];
})();
