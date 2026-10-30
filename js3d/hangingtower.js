// Torre de terrazas colgantes (guía de diseño v2, references/torre-colgante-2089.md):
// la forma de los Jardines Colgantes de Babilonia hecha en la maqueta blanca de la web.
// Terrazas escalonadas con arcadas, café en los pretiles, enredaderas que cuelgan y guamos.
(function () {
  const M = (x, y, z, ry, sx, sy, sz) => W.Geom.matrix(x, y, z, 0, ry || 0, 0, sx, sy, sz);
  const PLASTER = '#f4f5f3', CORE = '#e3e7e4', PLANTER = '#f2f4f2', WATER = '#8e9c9d';
  const LEAVES = ['#1f5f27', '#2c7a2f', '#357f30', '#24682a'];
  const BOX = new THREE.BoxGeometry(1, 1, 1);
  const BLOB = new THREE.IcosahedronGeometry(1, 0);

  // Muro con arcos de medio punto: un arco cada ~4,6 m, vano = 62 % del paso
  const wallCache = new Map();
  function archWall(w, h, n) {
    const key = `${w.toFixed(2)}|${h}|${n}`;
    if (wallCache.has(key)) return wallCache.get(key);
    const s = new THREE.Shape();
    s.moveTo(-w / 2, 0); s.lineTo(w / 2, 0); s.lineTo(w / 2, h); s.lineTo(-w / 2, h); s.lineTo(-w / 2, 0);
    const pitch = w / n, r = pitch * 0.31, ys = h - 0.55 - r;
    for (let k = 0; k < n; k++) {
      const cx = -w / 2 + pitch * (k + 0.5);
      const p = new THREE.Path();
      p.moveTo(cx - r, 0.35); p.lineTo(cx + r, 0.35); p.lineTo(cx + r, ys);
      p.absarc(cx, ys, r, 0, Math.PI, false); p.lineTo(cx - r, 0.35);
      s.holes.push(p);
    }
    const g = new THREE.ExtrudeGeometry(s, { depth: 0.7, bevelEnabled: false, curveSegments: 8 });
    wallCache.set(key, g);
    return g;
  }

  // opts: terraces, floorH, base, setback, plinth, sealed, rnd, addCollider(minX,maxX,minZ,maxZ,h,tag)
  function build(scene, cx, cz, opts) {
    const rnd = opts.rnd || Math.random;
    const rr = (a, b) => a + rnd() * (b - a);
    const pick = (a) => a[Math.floor(rnd() * a.length)];
    const parts = [], blobs = [];
    const box = (x, y, z, sx, sy, sz, color, ry) => parts.push({ geo: BOX, matrix: M(x, y, z, ry, sx, sy, sz), color });
    const blob = (x, y, z, sx, sy, sz, color) => blobs.push([M(x, y, z, rnd() * 6.28, sx, sy, sz), new THREE.Color(color)]);
    const faces = (fn) => { for (let f = 0; f < 4; f++) { const a = f * Math.PI / 2; fn(a, Math.cos(a), Math.sin(a)); } };
    const at = (ca, sa, lx, lz) => [cx + lx * ca + lz * sa, cz - lx * sa + lz * ca];
    const TH = opts.floorH || 6, S0 = opts.base || 30, DS = opts.setback || 1.6;
    let y = opts.y0 || 0;

    if (opts.plinth) {
      box(cx, y + opts.plinth / 2, cz, S0 - 2, opts.plinth, S0 - 2, CORE);
      const g = archWall(S0 + 0.6, opts.plinth, Math.round(S0 / 5));
      faces((a) => parts.push({ geo: g, matrix: new THREE.Matrix4().makeTranslation(cx, y, cz).multiply(new THREE.Matrix4().makeRotationY(a)).multiply(new THREE.Matrix4().makeTranslation(0, 0, (S0 + 0.6) / 2 - 0.5)), color: PLASTER }));
      if (opts.addCollider) opts.addCollider(cx - S0 / 2, cx + S0 / 2, cz - S0 / 2, cz + S0 / 2, y + opts.plinth, 'torre');
      y += opts.plinth;
    }
    let last = S0;
    for (let i = 0; i < opts.terraces; i++) {
      const s = S0 - i * DS * 2;
      if (s < 6) break;
      last = s;
      const inner = s - 3.2, n = Math.max(3, Math.round(s / 4.6)), g = archWall(s, TH, n);
      box(cx, y + TH / 2, cz, inner, TH, inner, CORE);
      faces((a, ca, sa) => {
        parts.push({ geo: g, matrix: new THREE.Matrix4().makeTranslation(cx, y, cz).multiply(new THREE.Matrix4().makeRotationY(a)).multiply(new THREE.Matrix4().makeTranslation(0, 0, s / 2 - 0.7)), color: PLASTER });
        // café adentro de la arcada, bajo una barra de luz blanca de cultivo
        if (!opts.sealed) for (let t = -s / 2 + 1.6; t < s / 2 - 1.6; t += 1.4) { const [px, pz] = at(ca, sa, t, inner / 2 + 0.6); blob(px, y + 0.7, pz, 0.45, 0.55, 0.45, pick(LEAVES)); }
        const [lx, lz] = at(ca, sa, 0, inner / 2 + 0.9);
        box(lx, y + TH - 0.9, lz, s - 2, 0.08, 0.12, '#ffffff', a);
      });
      if (opts.addCollider) opts.addCollider(cx - s / 2, cx + s / 2, cz - s / 2, cz + s / 2, y + TH + 0.44, 'torre');
      y += TH;
      box(cx, y + 0.22, cz, s + 0.8, 0.44, s + 0.8, PLASTER);
      const ns = S0 - (i + 1) * DS * 2;
      faces((a, ca, sa) => {
        const [px, pz] = at(ca, sa, 0, s / 2 - 0.1);
        box(px, y + 0.85, pz, s + 0.8, 0.85, 1.1, PLANTER, a);
        const [wx, wz] = at(ca, sa, 0, s / 2 - 0.9);
        box(wx, y + 0.5, wz, s - 1, 0.06, 0.35, WATER, a);
        if (opts.sealed) return; // Arcadia seca las terrazas que sella
        // café en la jardinera, con cerezas
        for (let t = -s / 2 + 0.7; t < s / 2 - 0.4; t += 0.75) {
          const [qx, qz] = at(ca, sa, t, s / 2 - 0.1);
          blob(qx, y + 1.65, qz, rr(0.38, 0.5), rr(0.4, 0.55), rr(0.38, 0.5), pick(LEAVES));
          if (rnd() < 0.3) blob(qx + rr(-0.2, 0.2), y + 1.5, qz + rr(-0.2, 0.2), 0.08, 0.09, 0.08, '#b3121a');
        }
        // enredaderas que cuelgan del pretil: 70 % cortas, 30 % largas
        for (let t = -s / 2 + 0.9; t < s / 2 - 0.5; t += rr(1.3, 2.8)) {
          const [vx, vz] = at(ca, sa, t, s / 2 + 0.42);
          const len = rnd() < 0.3 ? rr(2.6, TH * 0.7) : rr(0.8, 2);
          const k = Math.round(len / 0.32);
          for (let j = 0; j < k; j++) { const f = j / k; blob(vx + rr(-0.15, 0.15), y + 0.9 - j * 0.32, vz + rr(-0.1, 0.15), 0.3 * (1 - f * 0.5), 0.26, 0.28 * (1 - f * 0.5), f > 0.75 ? '#79b83c' : pick(LEAVES)); }
        }
        // guamos de sombra en la terraza
        if (ns >= 6) {
          const mid = (s / 2 + ns / 2) / 2 + 0.2;
          for (const t of [-s / 2 + 2.6, 0, s / 2 - 2.6]) {
            if (rnd() > 0.7) continue;
            const [tx, tz] = at(ca, sa, t + rr(-0.5, 0.5), mid);
            box(tx, y + 1.6, tz, 0.2, 2.4, 0.2, '#6b4a2e');
            for (let j = 0; j < 7; j++) blob(tx + rr(-1, 1), y + rr(2.6, 3.6), tz + rr(-1, 1), rr(0.6, 0.9), rr(0.45, 0.65), rr(0.6, 0.9), pick(['#2c7a2f', '#357f30', '#3f8a33', '#4c9a3a']));
          }
        }
      });
    }
    const top = y + 0.44;
    const mesh = new THREE.Mesh(W.Geom.merge(parts), W.Geom.vcMat());
    scene.add(mesh);
    if (blobs.length) {
      const im = new THREE.InstancedMesh(BLOB, new THREE.MeshLambertMaterial({ flatShading: true }), blobs.length);
      blobs.forEach(([m, c], i) => { im.setMatrixAt(i, m); im.setColorAt(i, c); });
      scene.add(im);
    }
    if (opts.sealed) {
      // sello de Arcadia: caja de vidrio azul al 9 % con aristas azules y líneas cada 0,7 m
      const g = new THREE.BoxGeometry(last + 1.4, 7, last + 1.4);
      const seal = new THREE.Mesh(g, new THREE.MeshBasicMaterial({ color: '#2a22f5', transparent: true, opacity: 0.09, depthWrite: false, side: THREE.DoubleSide }));
      seal.position.set(cx, top + 3.5, cz);
      scene.add(seal);
      const edges = new THREE.LineSegments(new THREE.EdgesGeometry(g), new THREE.LineBasicMaterial({ color: '#2a22f5' }));
      edges.position.copy(seal.position);
      scene.add(edges);
      const ring = new THREE.EdgesGeometry(new THREE.BoxGeometry(last + 1.4, 0.001, last + 1.4));
      const ringMat = new THREE.LineBasicMaterial({ color: '#2a22f5', transparent: true, opacity: 0.35 });
      for (let yy = 0.7; yy < 7; yy += 0.7) { const r = new THREE.LineSegments(ring, ringMat); r.position.set(cx, top + yy, cz); scene.add(r); }
    } else {
      // corona: invernadero de la red (vidrio claro) y baldosa blanca con la hoja
      const gh = new THREE.Mesh(new THREE.BoxGeometry(Math.min(9, last - 1), 3, Math.min(6, last - 1.5)), new THREE.MeshLambertMaterial({ color: '#ffffff', transparent: true, opacity: 0.25 }));
      gh.position.set(cx, top + 1.5, cz - 0.5);
      scene.add(gh);
      const tile = new THREE.Mesh(new THREE.PlaneGeometry(3.2, 3.2), new THREE.MeshBasicMaterial({ map: leafTile(), side: THREE.DoubleSide }));
      tile.position.set(cx, top + 3.4, cz + last / 2 - 0.9);
      scene.add(tile);
    }
    return { top, size: last };
  }

  // baldosa blanca con la hoja tinta (el logo de la web)
  let tileTex = null;
  function leafTile() {
    if (tileTex) return tileTex;
    const c = document.createElement('canvas'); c.width = c.height = 128;
    const x = c.getContext('2d');
    x.fillStyle = '#ffffff'; x.fillRect(0, 0, 128, 128);
    x.save(); x.translate(64, 64); x.scale(4.2, 4.2); x.translate(-12, -12);
    x.fillStyle = '#2c302e';
    x.beginPath(); x.moveTo(12, 3); x.bezierCurveTo(16.5, 5.5, 18.5, 9, 18, 13.5); x.bezierCurveTo(17.7, 16.9, 15.1, 19.7, 12, 21);
    x.bezierCurveTo(8.9, 19.7, 6.3, 16.9, 6, 13.5); x.bezierCurveTo(5.5, 9, 7.5, 5.5, 12, 3); x.fill();
    x.strokeStyle = '#ffffff'; x.lineWidth = 0.9; x.beginPath(); x.moveTo(12, 6); x.lineTo(12, 19); x.stroke();
    x.restore();
    tileTex = new THREE.CanvasTexture(c);
    return tileTex;
  }

  // Acueducto de arcos con canal de agua arriba (une torres, como el de Jerwan)
  function aqueduct(scene, x1, z1, x2, z2, h) {
    const len = Math.hypot(x2 - x1, z2 - z1), ang = Math.atan2(x2 - x1, z2 - z1);
    const g = archWall(len, h, Math.max(1, Math.round(len / 7)));
    const m = new THREE.Matrix4().makeTranslation((x1 + x2) / 2, 0, (z1 + z2) / 2).multiply(new THREE.Matrix4().makeRotationY(ang - Math.PI / 2)).multiply(new THREE.Matrix4().makeTranslation(0, 0, -0.35));
    const parts = [{ geo: g, matrix: m, color: '#f3f4f2' }, { geo: BOX, matrix: M((x1 + x2) / 2, h + 0.05, (z1 + z2) / 2, ang, 1.2, 0.1, len), color: WATER }];
    scene.add(new THREE.Mesh(W.Geom.merge(parts), W.Geom.vcMat()));
  }

  W.HangingTower = { build, aqueduct };
})();
