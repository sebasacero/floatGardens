// El barrio: la hilera jugable de casas, las cuadras de atrás, calles con carros, la avenida con
// TransMilenio, los cerros con Monserrate y Guadalupe, cielo, nubes y cometas. Todo procedural.
(function () {
  const FG = window.FGT;
  const { Geo, C, M4 } = FG;

  FG.buildWorld = function (scene, lvl) {
    const R = FG.rng(2087), rr = (a, b) => a + (b - a) * R(), pick = (a) => a[Math.floor(R() * a.length)];
    const FLOOR = FG.FLOOR, ZF = FG.ZF, ZB = FG.ZB;
    const PAINT = ['#f6d77a', '#9fd3f2', '#f2a6b5', '#b6e3a5', '#f6b77a', '#e9e1ff', '#ffffff', '#ffe9a8'];
    const anim = [];
    const trees = [];
    function tree(x, y, z, s) { trees.push([x, y, z, s]); }

    // ---------- materiales ----------
    const mFacade = new THREE.MeshStandardMaterial({ map: FG.facadeAtlas(), vertexColors: true, roughness: 0.9 });
    const mBrick = new THREE.MeshStandardMaterial({ map: FG.brickTex(), vertexColors: true, roughness: 0.95 });
    const mConc = new THREE.MeshStandardMaterial({ map: FG.concreteTex(), vertexColors: true, roughness: 0.95 });
    const mSolid = new THREE.MeshStandardMaterial({ vertexColors: true, roughness: 0.8 });
    const mGlow = new THREE.MeshBasicMaterial({ vertexColors: true, toneMapped: false });
    const signTexOn = FG.signTex('Red de cultivo · Kennedy');
    const mSign = new THREE.MeshStandardMaterial({ map: signTexOn, emissive: C('#ffffff'), emissiveMap: signTexOn, emissiveIntensity: 0.35, roughness: 0.6 });
    const asph = FG.asphaltTex(); asph.repeat.set(1, 1);
    const mAsph = new THREE.MeshStandardMaterial({ map: asph, vertexColors: true, roughness: 0.95 });
    const towerT = FG.towerTex();
    const mTower = new THREE.MeshStandardMaterial({ map: towerT, vertexColors: true, roughness: 0.3, metalness: 0.15, emissive: C('#1d4a2a'), emissiveIntensity: 0.25 });
    FG.mats = { mSolid, mBrick, mConc, mGlow, mSign };

    const GF = new Geo(), GB = new Geo(), GC = new Geo(), GS = new Geo(), GL = new Geo(), GSign = new Geo(), GA = new Geo(), GT = new Geo();

    // ---------- una casa ----------
    // facing: 'front' = fachada con ventanas hacia la cámara (+z); 'back' = culata de ladrillo.
    function house(x0, x1, z0, z1, h, facing, opts = {}) {
      const painted = opts.painted != null ? opts.painted : R() < 0.4;
      const col = painted ? C(opts.paint || pick(PAINT)) : C('#ffffff').multiplyScalar(rr(0.86, 1.02));
      const roofCol = C('#ffffff').multiplyScalar(rr(0.85, 1.05));
      const sideCol = painted && R() < 0.3 ? col : C('#ffffff').multiplyScalar(rr(0.82, 1));
      const w = x1 - x0, floors = Math.max(1, Math.floor(h / FLOOR));
      GB.box(x0, x1, 0, h, z0, z1, sideCol, sideCol, { front: facing === 'front', top: true });
      GC.quad([x0, h, z1], [x1, h, z1], [x1, h, z0], [x0, h, z0], roofCol, x0 / 3, -z1 / 3, x1 / 3, -z0 / 3);
      if (facing === 'front') {
        const nb = Math.max(1, Math.round(w / 3.3)), bw = w / nb, e = 0.004, cell = painted ? 1 : R() < 0.3 ? 2 : 0;
        for (let f = 0; f < floors; f++) {
          for (let b = 0; b < nb; b++) {
            let c = cell, row = f === 0 ? 0 : 1;
            if (f === 0) c = [0, 1, 2, 1, 3][Math.floor(R() * 5)];
            else if (R() < 0.15) c = 3;
            const tint = f === 0 && c !== 1 ? C('#ffffff') : col;
            GF.quad([x0 + b * bw, f * FLOOR, z1], [x0 + (b + 1) * bw, f * FLOOR, z1], [x0 + (b + 1) * bw, (f + 1) * FLOOR, z1], [x0 + b * bw, (f + 1) * FLOOR, z1],
              tint, c / 4 + e, row * 0.5 + e, (c + 1) / 4 - e, row * 0.5 + 0.5 - e);
          }
        }
        // franja superior (antepecho) en ladrillo o pintada
        if (h - floors * FLOOR > 0.05) GB.quad([x0, floors * FLOOR, z1], [x1, floors * FLOOR, z1], [x1, h, z1], [x0, h, z1], painted ? col : sideCol, x0 / 2, floors * FLOOR / 2, x1 / 2, h / 2);
        // cornisa de concreto
        GS.box(x0 - 0.05, x1 + 0.05, h - 0.18, h, z1, z1 + 0.18, C('#d8d3c8'));
        if (R() < 0.5) for (let f = 1; f < floors; f++) GS.box(x0, x1, f * FLOOR - 0.08, f * FLOOR + 0.06, z1, z1 + 0.12, C('#cfc8bb'));
      }
    }

    // ---------- objetos de terraza ----------
    function tank(x, y, z, col) {
      GS.geom(new THREE.CylinderGeometry(0.62, 0.55, 1.1, 14), M4(x, y + 0.55, z), C(col));
      GS.geom(new THREE.CylinderGeometry(0.66, 0.66, 0.12, 14), M4(x, y + 1.14, z), C(col).multiplyScalar(1.3));
      GS.geom(new THREE.CylinderGeometry(0.22, 0.22, 0.12, 10), M4(x, y + 1.24, z), C(col).multiplyScalar(1.5));
    }
    function rebar(x, y, z) {
      GS.box(x - 0.18, x + 0.18, y, y + 0.35, z - 0.18, z + 0.18, C('#9c968c'));
      for (let i = 0; i < 4; i++) {
        const ox = i % 2 ? 0.08 : -0.08, oz = i < 2 ? 0.08 : -0.08;
        GS.box(x + ox - 0.02, x + ox + 0.02, y + 0.35, y + rr(0.9, 1.7), z + oz - 0.02, z + oz + 0.02, C('#7a3d22'));
      }
    }
    function clothesLine(x0, x1, y, z) {
      GS.box(x0 - 0.04, x0 + 0.04, y, y + 1.9, z - 0.04, z + 0.04, C('#6b6f78'));
      GS.box(x1 - 0.04, x1 + 0.04, y, y + 1.9, z - 0.04, z + 0.04, C('#6b6f78'));
      GS.box(x0, x1, y + 1.84, y + 1.87, z - 0.01, z + 0.01, C('#e8e8e8'));
      for (let px = x0 + 0.3; px < x1 - 0.5;) {
        const w = rr(0.4, 0.9), hh = rr(0.5, 1.1), c = C(pick(FG.clothesColors));
        GL.quad([px, y + 1.85 - hh, z], [px + w, y + 1.85 - hh, z], [px + w, y + 1.85, z], [px, y + 1.85, z], c);
        px += w + rr(0.08, 0.3);
      }
    }
    function antenna(x, y, z) {
      GS.box(x - 0.03, x + 0.03, y, y + 2.6, z - 0.03, z + 0.03, C('#7d828c'));
      for (let i = 0; i < 4; i++) GS.box(x - 0.6 + i * 0.1, x + 0.6 - i * 0.1, y + 1.8 + i * 0.2, y + 1.83 + i * 0.2, z - 0.02, z + 0.02, C('#7d828c'));
    }
    function dish(x, y, z) {
      GS.geom(new THREE.SphereGeometry(0.35, 10, 6, 0, Math.PI * 2, 0, 1.0), M4(x, y + 0.9, z, 0.4, 1, 1, 1, 1.2), C('#e6e6e6'));
      GS.box(x - 0.03, x + 0.03, y, y + 0.9, z - 0.03, z + 0.03, C('#8a8f98'));
    }
    function plants(x, y, z, n) {
      for (let i = 0; i < n; i++) {
        const px = x + i * 0.55;
        GS.geom(new THREE.CylinderGeometry(0.2, 0.15, 0.32, 8), M4(px, y + 0.16, z), C(pick(['#b5532e', '#2f6fd1', '#e8e1d0'])));
        GS.geom(new THREE.IcosahedronGeometry(0.3, 0), M4(px, y + 0.5, z, rr(0, 3), 1, rr(0.8, 1.3), 1), C(pick(['#3f9a3a', '#5bb04a', '#2e7d32'])));
      }
    }
    function fgSign(x, y, z, s = 1) {
      for (const o of [-1.6 * s, 1.6 * s]) GS.box(x + o - 0.08, x + o + 0.08, y, y + 2.2 * s, z - 0.08, z + 0.08, C('#3a3f48'));
      const w = 4.1 * s, hh = 1.67 * s, y0 = y + 2.0 * s;
      GSign.quad([x - w / 2, y0, z + 0.1], [x + w / 2, y0, z + 0.1], [x + w / 2, y0 + hh, z + 0.1], [x - w / 2, y0 + hh, z + 0.1], C('#ffffff'));
      GS.box(x - w / 2, x + w / 2, y0, y0 + hh, z - 0.05, z + 0.08, C('#e8e8e8'), C('#e8e8e8'), { front: true });
    }
    function shed(x0, x1, y, z0, z1) {
      GB.box(x0, x1, y, y + 2.3, z0, z1, C('#ffffff').multiplyScalar(rr(0.85, 1)), C('#fff'), { top: true });
      GS.box(x0 - 0.15, x1 + 0.15, y + 2.3, y + 2.42, z0 - 0.15, z1 + 0.15, C('#9aa0a6'));
      GS.box(x0 + 0.4, x0 + 1.2, y, y + 2.0, z1, z1 + 0.04, C('#5a6f8a'));
    }
    function roofProps(x0, x1, z0, z1, h, opt = {}) {
      const w = x1 - x0;
      const pc = R() < 0.6 ? C('#c0643c') : C(pick(PAINT)), t = 0.22, ph = rr(0.6, 1.1);
      // antepecho al fondo y a los lados (no estorba en el carril del jugador)
      GB.box(x0, x1, h, h + ph, z0, z0 + t, pc);
      if (!opt.lane) {
        GB.box(x0, x1, h, h + ph, z1 - t, z1, pc);
        GB.box(x0, x0 + t, h, h + ph, z0, z1, pc); GB.box(x1 - t, x1, h, h + ph, z0, z1, pc);
      } else GS.box(x0, x1, h, h + 0.25, z1 - 0.2, z1, C('#d8d3c8'));
      const zb = opt.lane ? z0 + 0.5 : z0 + 0.6, zf = opt.lane ? -2.2 : z1 - 0.8; // franja de utilería
      if (R() < 0.6) tank(x0 + rr(0.9, w - 0.9), h, rr(zb + 0.4, zb + 1.6), R() < 0.75 ? '#25272c' : '#2c6fd1');
      if (R() < 0.35 && w > 6) { const a = x0 + rr(0.5, w - 4.5); clothesLine(a, a + rr(2.5, 4), h, rr(zb + 1.5, zf)); }
      if (R() < 0.25 && w > 5) shed(x1 - 3.2, x1 - 0.4, h, zb, zb + 2.6);
      if (R() < 0.3) for (const px of [x0 + 0.3, x1 - 0.3]) rebar(px, h, zb + 0.1);
      if (R() < 0.3) antenna(x0 + rr(0.5, w - 0.5), h, zb + 0.3);
      if (R() < 0.25) dish(x0 + rr(0.5, w - 0.5), h, zb + 0.4);
      if (R() < 0.35) plants(x0 + rr(0.3, Math.max(0.4, w - 3)), h, zf - rr(0, 1), 2 + Math.floor(R() * 4));
      if (opt.fg) fgSign((x0 + x1) / 2 + rr(-w * 0.15, w * 0.15), h, zb + 0.6, opt.lane ? 0.9 : 1.15);
    }

    // ---------- hilera jugable ----------
    for (const c of lvl.casas) {
      const o = c.opt;
      if (o.obra) { obra(c.x0, c.x1, c.h); continue; }
      house(c.x0, c.x1, ZB, ZF, c.h, 'front', o.torre ? { painted: true, paint: '#e9f5ea' } : {});
      if (o.cultivo) startTerrace(c);
      else if (o.torre) towerAnnex(c);
      else roofProps(c.x0, c.x1, ZB, ZF, c.h, { lane: true, fg: R() < 0.25 && o.cp == null });
    }

    function startTerrace(c) {
      const h = c.h;
      GB.box(c.x0, c.x1, h, h + 0.9, ZB, ZB + 0.22, C('#c0643c'));
      // camas de cultivo de café con matas y cerezas
      for (let i = 0; i < 6; i++) {
        const px = c.x0 + 7 + i * 1.9;
        GS.box(px - 0.6, px + 0.6, h, h + 0.45, ZB + 0.6, ZB + 2.6, C('#8a5a32'), C('#4b3020'));
        for (let z = ZB + 0.9; z < ZB + 2.5; z += 0.6) {
          const s = rr(0.7, 1);
          GS.geom(new THREE.IcosahedronGeometry(0.42 * s, 1), M4(px + rr(-0.15, 0.15), h + 0.8, z, rr(0, 3), 1, 1.3, 1), C(pick(['#2f8a3a', '#3d9b45', '#277a33'])));
          for (let k = 0; k < 4; k++) GS.geom(new THREE.SphereGeometry(0.06, 6, 4), M4(px + rr(-0.35, 0.35), h + rr(0.6, 1.1), z + 0.3), C('#d7262e'));
        }
      }
      tank(c.x0 + 3, h, ZB + 1.2, '#25272c');
      clothesLine(c.x0 + 1.2, c.x0 + 5.5, h, -2.8);
      plants(c.x0 + 15.5, h, -2.4, 6);
      for (const px of [c.x0 + 0.3, c.x1 - 0.3]) rebar(px, h, ZB + 0.3);
    }

    // Edificio en obra negra: ladrillo crudo, huecos sin ventana, placas a la vista y varillas.
    function obra(x0, x1, h) {
      const floors = Math.floor(h / FLOOR);
      GB.box(x0, x1, 0, h, ZB, ZF, C('#e6d6c8'), C('#fff'), { top: true });
      GC.quad([x0, h, ZF], [x1, h, ZF], [x1, h, ZB], [x0, h, ZB], C('#cfcac0'), x0 / 3, -ZF / 3, x1 / 3, -ZB / 3);
      for (let f = 0; f < floors; f++) {
        GS.box(x0 - 0.06, x1 + 0.06, f * FLOOR - 0.12, f * FLOOR + 0.12, ZF, ZF + 0.1, C('#a9a49a'));
        if (f === 0) continue;
        for (let b = 0; b < 3; b++) {
          const bx = x0 + 1.4 + b * ((x1 - x0 - 2.8) / 2) - 1;
          GS.box(bx, bx + 2, f * FLOOR + 0.7, f * FLOOR + 2.1, ZF, ZF + 0.02, C('#1b1b1f'));
        }
      }
      for (let px = x0 + 0.4; px < x1; px += 4.3) { rebar(px, h, ZB + 0.4); rebar(px, h, -2.4); }
      GS.box(x0 + 2, x0 + 3.6, h, h + 0.8, ZB + 0.8, ZB + 1.8, C('#c0643c'));
      GS.box(x1 - 4, x1 - 2.4, h, h + 0.5, ZB + 0.8, ZB + 2, C('#9aa0a6'));
      // polisombra verde colgando en la fachada
      GS.box(x1 - 4.5, x1 - 0.3, h * 0.45, h - 0.5, ZF + 0.12, ZF + 0.14, C('#2f6b3a'));
    }

    // La torre de Float Gardens: anexo (techo jugable) + torre hidropónica atrás.
    function towerAnnex(c) {
      const h = c.h, tx0 = c.x0 + 4, tx1 = c.x1 - 4, tz0 = -34, tz1 = -8, th = 52;
      GB.box(c.x0, c.x1, h, h + 0.4, ZB, ZB + 0.25, C('#e9f5ea'));
      const fl = th / 3.6;
      // torre: fachada de vidrio con cultivos (textura repetida por pisos)
      GT.quad([tx0, 0, tz1], [tx1, 0, tz1], [tx1, th, tz1], [tx0, th, tz1], C('#ffffff'), 0, 0, (tx1 - tx0) / 16, fl / 4);
      GT.quad([tx1, 0, tz1], [tx1, 0, tz0], [tx1, th, tz0], [tx1, th, tz1], C('#e6e6e6'), 0, 0, (tz1 - tz0) / 16, fl / 4);
      GT.quad([tx0, 0, tz0], [tx0, 0, tz1], [tx0, th, tz1], [tx0, th, tz0], C('#e6e6e6'), 0, 0, (tz1 - tz0) / 16, fl / 4);
      GS.box(tx0 - 0.4, tx1 + 0.4, th, th + 0.8, tz0 - 0.4, tz1 + 0.4, C('#eef2f5'));
      for (const px of [tx0, tx1]) GS.box(px - 0.5, px + 0.5, 0, th, tz1 - 0.5, tz1 + 0.5, C('#f3f6f8'));
      // invernadero en la azotea de la torre
      const gx = (tx0 + tx1) / 2, gz = (tz0 + tz1) / 2;
      const glass = new THREE.Mesh(new THREE.CylinderGeometry(7, 7, tx1 - tx0 - 2, 20, 1, true, 0, Math.PI),
        new THREE.MeshStandardMaterial({ color: '#cfefff', transparent: true, opacity: 0.45, roughness: 0.1, metalness: 0.2, side: THREE.DoubleSide }));
      glass.rotation.z = Math.PI / 2; glass.position.set(gx, th + 0.8, gz); scene.add(glass);
      for (let i = 0; i < 8; i++) GS.geom(new THREE.IcosahedronGeometry(1.4, 1), M4(tx0 + 2 + i * ((tx1 - tx0 - 4) / 7), th + 1.6, gz + rr(-2, 2)), C(pick(['#3fa34d', '#57b65a', '#2e8b3e'])));
      // corona con el aviso
      const sw = 22, sh = 9, sy = th + 6;
      for (const o of [-8, 8]) GS.box(gx + o - 0.3, gx + o + 0.3, th + 0.8, sy, tz1 - 1, tz1 - 0.4, C('#3a3f48'));
      GSign.quad([gx - sw / 2, sy, tz1 - 0.2], [gx + sw / 2, sy, tz1 - 0.2], [gx + sw / 2, sy + sh, tz1 - 0.2], [gx - sw / 2, sy + sh, tz1 - 0.2], C('#ffffff'));
      // camas hidropónicas en el anexo
      for (let i = 0; i < 4; i++) {
        const px = c.x0 + 2 + i * 3.2;
        GS.box(px, px + 2.4, h, h + 0.9, ZB + 0.6, ZB + 2.6, C('#f0f0f0'));
        for (let k = 0; k < 5; k++) GS.geom(new THREE.IcosahedronGeometry(0.3, 1), M4(px + 0.3 + k * 0.45, h + 1.05, ZB + 1.6), C('#5cc25a'));
      }
    }

    // ---------- cuadras de atrás ----------
    // Las calles de la hilera jugable siguen hacia el fondo; los lotes no.
    const cortes = lvl.huecos.filter((g) => g.tipo !== 'lote');
    const X0 = -260, X1 = lvl.fin + 260;
    const blocks = [];
    { let a = X0;
      // calles extra antes y después del nivel
      const extra = [];
      for (let x = -40; x > X0; x -= 44) extra.push({ x0: x - 7, x1: x, tipo: 'calle' });
      for (let x = lvl.fin + 30; x < X1; x += 44) extra.push({ x0: x, x1: x + 7, tipo: 'calle' });
      const all = cortes.concat(extra).sort((p, q) => p.x0 - q.x0);
      for (const g of all) { if (g.x0 > a) blocks.push([a, g.x0]); a = g.x1; }
      blocks.push([a, X1]);
      FG.calles = all;
    }
    // decorado en la hilera jugable fuera del nivel
    for (const [b0, b1] of blocks) {
      for (const [s0, s1] of [[b0, Math.min(b1, 0)], [Math.max(b0, lvl.fin), b1]]) {
        let x = s0;
        while (x < s1 - 3) {
          const w = Math.min(rr(7, 11), s1 - x); const h = Math.floor(rr(2, 5)) * FLOOR + rr(0, 0.9);
          house(x, x + w, ZB, ZF, h, 'front'); roofProps(x, x + w, ZB, ZF, h, { fg: R() < 0.2 }); x += w;
        }
      }
    }
    const ROWS = [[-15, -6, 'back'], [-32, -23, 'front'], [-41, -32, 'back'], [-58, -49, 'front'], [-67, -58, 'back'], [-84, -75, 'front'], [-93, -84, 'back'], [-110, -101, 'front'], [-119, -110, 'back']];
    for (const [z0, z1, facing] of ROWS) {
      const lejos = z0 < -70;
      for (const [b0, b1] of blocks) {
        let x = b0;
        while (x < b1 - 2) {
          let w = rr(6, 11); if (b1 - x - w < 4) w = b1 - x;
          const floors = Math.floor(rr(2, z0 > -20 ? 6 : 5.5));
          const h = floors * FLOOR + rr(0, 0.9);
          house(x, x + w, z0, z1, h, facing);
          if (!lejos || R() < 0.5) roofProps(x, x + w, z0, z1, h, { fg: R() < 0.12 });
          x += w;
        }
      }
    }
    // ciudad lejana: cajas de ladrillo con techos claros hasta el pie de los cerros
    {
      const n = 2600, box = new THREE.BoxGeometry(1, 1, 1); box.translate(0, 0.5, 0);
      const im = new THREE.InstancedMesh(box, new THREE.MeshStandardMaterial({ roughness: 1 }), n);
      const m = new THREE.Matrix4(), col = new THREE.Color();
      let k = 0;
      for (let i = 0; i < n * 3 && k < n; i++) {
        const z = -128 - Math.pow(R(), 1.3) * 1300, x = rr(-1400 + z * 0.6, lvl.fin + 1400 - z * 0.6);
        if (Math.abs(((x % 46) + 46) % 46) < 6) continue; // deja calles
        const big = z < -450 && R() < 0.015, w = big ? rr(14, 22) : rr(7, 12), h = big ? rr(24, 42) : Math.floor(rr(2, 5)) * FLOOR;
        m.compose(new THREE.Vector3(x, 0, z), new THREE.Quaternion(), new THREE.Vector3(w, h, rr(8, 12)));
        im.setMatrixAt(k, m);
        im.setColorAt(k, big ? col.set(pick(['#e8e4dc', '#c9d4dd', '#f0e7d6'])) : R() < 0.3 ? col.set(pick(PAINT)) : col.setHSL(0.04 + R() * 0.02, 0.5, 0.42 + R() * 0.12));
        k++;
      }
      im.count = k; im.receiveShadow = false; scene.add(im);
    }

    // ---------- suelo, andenes y calles ----------
    GA.quad([X0 - 1600, 0, 60], [X1 + 1600, 0, 60], [X1 + 1600, 0, -1500], [X0 - 1600, 0, -1500], C('#ffffff'), (X0 - 1600) / 8, -60 / 8, (X1 + 1600) / 8, 1500 / 8);
    const blockZ = [[-15, ZF], [-41, -23], [-67, -49], [-93, -75], [-119, -101]];
    for (const [b0, b1] of blocks) for (const [z0, z1] of blockZ) GS.box(b0 - 0.9, b1 + 0.9, 0, 0.15, z0 - 1.6, z1 + 1.6, C('#c9c4bb'), C('#bdb8ae'));
    for (const g of lvl.huecos) {
      if (g.tipo === 'lote') {
        GS.box(g.x0, g.x1, 0, 0.2, -6, ZF, C('#7d5f3e'), C('#8a6a45'));
        for (let i = 0; i < 14; i++) GS.geom(new THREE.IcosahedronGeometry(rr(0.2, 0.45), 0), M4(rr(g.x0 + 0.3, g.x1 - 0.3), 0.25, rr(-5.5, 2)), C(pick(['#6c8f3a', '#86a64a', '#5a7a30'])));
        GB.box(g.x0, g.x1, 0, 2.2, ZF - 0.2, ZF, C('#ffffff'));
      }
    }
    // líneas de las calles y cebras
    for (const g of FG.calles) {
      const cx = (g.x0 + g.x1) / 2;
      if (g.tipo === 'avenida') { avenue(g); continue; }
      for (let z = -150; z < 40; z += 4) GS.box(cx - 0.06, cx + 0.06, 0.01, 0.03, z, z + 2, C('#f2f2f2'));
    }
    for (const [z0] of blockZ) for (let x = X0; x < X1; x += 4) GS.box(x, x + 2, 0.01, 0.03, z0 - 4.06, z0 - 3.94, C('#f2f2f2'));

    // Avenida: TransMilenio por el centro, separadores con árboles, estación y valla con el tanque.
    function avenue(g) {
      const cx = (g.x0 + g.x1) / 2, w = g.x1 - g.x0, tm = w > 30;
      GS.box(cx - 1.2, cx + 1.2, 0, 0.3, -1500, 60, C('#9fb08a'), C('#7fa05a'));
      for (let z = -200; z < 40; z += 9) tree(cx, 0.3, z, 1.2);
      if (tm) {
        GA.box(cx - 8, cx - 1.2, 0, 0.02, -1500, 60, C('#c96a5a'), C('#c96a5a'), { front: true, back: true, left: true, right: true });
        GA.box(cx + 1.2, cx + 8, 0, 0.02, -1500, 60, C('#c96a5a'), C('#c96a5a'), { front: true, back: true, left: true, right: true });
        // estación (vagón de vidrio)
        GS.box(cx - 2.2, cx + 2.2, 0, 1.0, -70, -30, C('#c9c4bb'));
        GS.box(cx - 2.2, cx + 2.2, 3.6, 3.9, -70, -30, C('#dfe3e6'));
        for (let z = -70; z <= -30; z += 5) GS.box(cx - 2.2, cx - 2.0, 1, 3.6, z, z + 0.2, C('#8b9198'));
        // valla publicitaria de Float Gardens que sostiene el tanque del medio
        const tq = lvl.ents.find((e) => e.t === 'tanque' && e.valla && e.x > g.x0 && e.x < g.x1);
        if (tq) {
          const top = tq.y - 1.15;
          GS.geom(new THREE.CylinderGeometry(0.35, 0.45, top - 3.4, 10), M4(tq.x, (top - 3.4) / 2, -1.2), C('#5e6570'));
          GS.box(tq.x - 4.6, tq.x + 4.6, top - 3.6, top - 3.4, -1.8, 0.6, C('#5e6570'));
          GSign.quad([tq.x - 4.4, top - 3.4, -0.3], [tq.x + 4.4, top - 3.4, -0.3], [tq.x + 4.4, top, -0.3], [tq.x - 4.4, top, -0.3], C('#ffffff'));
          GS.box(tq.x - 4.5, tq.x + 4.5, top - 3.45, top + 0.05, -0.6, -0.32, C('#e8e8e8'));
          GS.box(tq.x - 1.3, tq.x + 1.3, top - 0.1, top, -1.4, 0.9, C('#5e6570'));
        }
      }
      // ciclorruta
      GS.box(g.x0 + 0.4, g.x0 + 2.0, 0.01, 0.04, -1500, 60, C('#c34b3d'));
    }

    // ---------- árboles ----------
    for (const [b0, b1] of blocks) for (const [z0] of blockZ) for (let x = b0 + rr(2, 8); x < b1 - 1; x += rr(10, 24)) tree(x, 0.15, z0 - 1.2, rr(0.8, 1.2));
    {
      const n = trees.length;
      const crown = new THREE.InstancedMesh(new THREE.IcosahedronGeometry(1, 1), new THREE.MeshStandardMaterial({ roughness: 0.9, flatShading: true }), n * 3);
      const trunk = new THREE.InstancedMesh(new THREE.CylinderGeometry(0.12, 0.18, 1, 6), new THREE.MeshStandardMaterial({ color: '#5b4330', roughness: 1 }), n);
      const m = new THREE.Matrix4(), col = new THREE.Color();
      trees.forEach(([x, y, z, s], i) => {
        const th = 3.2 * s;
        trunk.setMatrixAt(i, M4(x, y + th / 2, z, 0, s, th, s));
        for (let k = 0; k < 3; k++) {
          crown.setMatrixAt(i * 3 + k, M4(x + rr(-0.7, 0.7) * s, y + th + rr(0, 1.2) * s, z + rr(-0.6, 0.6) * s, rr(0, 3), 1.5 * s, 1.3 * s, 1.5 * s));
          crown.setColorAt(i * 3 + k, col.set(pick(['#4f8f3a', '#5ea648', '#3e7d33', '#6fb04f'])));
        }
      });
      crown.castShadow = trunk.castShadow = true;
      scene.add(crown, trunk);
    }

    // ---------- postes y cables de luz ----------
    const cableGeo = new THREE.CylinderGeometry(0.025, 0.025, 1, 3, 1, true);
    for (const [z0] of blockZ) {
      const zc = z0 - 0.4;
      let prev = null;
      for (let x = X0 + 6; x < X1; x += 27) {
        if (FG.calles.some((g) => x > g.x0 - 1 && x < g.x1 + 1)) continue;
        GS.geom(new THREE.CylinderGeometry(0.11, 0.15, 9, 6), M4(x, 4.5, zc), C('#b9b4aa'));
        GS.box(x - 0.9, x + 0.9, 8.3, 8.42, zc - 0.06, zc + 0.06, C('#8a857c'));
        if (prev != null && x - prev < 60) {
          for (const [oy, ox] of [[8.42, -0.8], [8.42, 0.8], [7.6, 0]]) {
            const seg = 8, sag = 0.9;
            for (let i = 0; i < seg; i++) {
              const t0 = i / seg, t1 = (i + 1) / seg;
              const ax = prev + (x - prev) * t0, bx = prev + (x - prev) * t1;
              const ay = oy - Math.sin(t0 * Math.PI) * sag, by = oy - Math.sin(t1 * Math.PI) * sag;
              const len = Math.hypot(bx - ax, by - ay), ang = Math.atan2(by - ay, bx - ax);
              GS.geom(cableGeo, M4((ax + bx) / 2, (ay + by) / 2, zc + ox * 0.3, 0, 1, len, 1, 0, ang - Math.PI / 2), C('#1c1c1c'));
            }
          }
        }
        prev = x;
      }
    }

    // ---------- cerros orientales con Monserrate y Guadalupe ----------
    {
      const W = 7000, D = 900, nx = 280, nz = 36;
      const g = new THREE.PlaneGeometry(W, D, nx, nz); g.rotateX(-Math.PI / 2);
      const P = g.attributes.position, cols = [];
      const zc = -2300, xc = lvl.fin * 0.5;
      const ridge = (x) => {
        const gg = (c, w, h) => h * Math.exp(-(((x - c) / w) ** 2));
        return 380 + gg(xc - 260, 260, 260) + gg(xc + 380, 300, 250) + gg(xc - 1100, 500, 160) + gg(xc + 1300, 600, 220) + Math.sin(x * 0.004) * 50 + Math.sin(x * 0.011 + 1) * 24;
      };
      FG.ridge = ridge; FG.ridgeZ = zc; FG.ridgeXc = xc;
      for (let i = 0; i < P.count; i++) {
        const x = P.getX(i) + xc, z = P.getZ(i);
        const t = THREE.MathUtils.clamp((z + D / 2) / D, 0, 1); // 0 fondo, 1 frente
        const h = ridge(x) * Math.sin(Math.min(1, (1 - t) * 1.6) * Math.PI / 2) * (1 - t * 0.9) + Math.sin(x * 0.03 + z * 0.02) * 6;
        P.setXYZ(i, x, Math.max(0, h), z + zc);
        const c = C('#2f5a3a').lerp(C('#5f7f4f'), Math.random() * 0.35).lerp(C('#6f86a0'), THREE.MathUtils.clamp((h - 150) / 500, 0, 0.35));
        cols.push(c.r, c.g, c.b);
      }
      g.setAttribute('color', new THREE.Float32BufferAttribute(cols, 3));
      g.computeVertexNormals();
      const hills = new THREE.Mesh(g, new THREE.MeshStandardMaterial({ vertexColors: true, roughness: 1, flatShading: true }));
      scene.add(hills);
      // Monserrate: santuario blanco con techo rojo; Guadalupe: la virgen blanca
      const mx = xc - 260, mz = zc + 140, my = ridge(mx) * 0.92;
      const sant = new THREE.Group();
      const white = new THREE.MeshStandardMaterial({ color: '#f7f4ee' }), red = new THREE.MeshStandardMaterial({ color: '#c2452d' });
      const b1 = new THREE.Mesh(new THREE.BoxGeometry(30, 16, 18), white); b1.position.y = 8; sant.add(b1);
      const t1 = new THREE.Mesh(new THREE.BoxGeometry(8, 34, 8), white); t1.position.set(-12, 17, 0); sant.add(t1);
      const t2 = t1.clone(); t2.position.x = 12; sant.add(t2);
      const r1 = new THREE.Mesh(new THREE.ConeGeometry(16, 8, 4), red); r1.position.y = 20; r1.rotation.y = Math.PI / 4; sant.add(r1);
      sant.position.set(mx, my, mz); scene.add(sant);
      const gx = xc + 380, gy = ridge(gx) * 0.93;
      const virgen = new THREE.Group();
      const base = new THREE.Mesh(new THREE.CylinderGeometry(6, 8, 10, 8), white); base.position.y = 5; virgen.add(base);
      const body = new THREE.Mesh(new THREE.ConeGeometry(5, 26, 10), white); body.position.y = 23; virgen.add(body);
      const head = new THREE.Mesh(new THREE.SphereGeometry(2.6, 10, 8), white); head.position.y = 37; virgen.add(head);
      virgen.position.set(gx, gy, mz); scene.add(virgen);
      // cable del teleférico
      const cab = new THREE.Mesh(new THREE.CylinderGeometry(0.8, 0.8, 600, 4), new THREE.MeshBasicMaterial({ color: '#333' }));
      cab.position.set(mx - 30, my - 150, mz + 200); cab.rotation.x = Math.PI / 2 - 0.55; scene.add(cab);
    }

    // ---------- nubes y cometas ----------
    const cloudT = FG.cloudTex();
    for (let i = 0; i < 26; i++) {
      const s = new THREE.Sprite(new THREE.SpriteMaterial({ map: cloudT, transparent: true, opacity: rr(0.7, 0.95), depthWrite: false, fog: false }));
      const z = rr(-2600, -1500); s.position.set(rr(-1800, lvl.fin + 1800), rr(450, 1100), z); s.scale.set(rr(500, 1000), rr(220, 380), 1);
      scene.add(s); anim.push((t) => { s.position.x += 0.02; });
    }
    for (let i = 0; i < 9; i++) {
      const kg = new THREE.BufferGeometry();
      kg.setAttribute('position', new THREE.Float32BufferAttribute([0, 1.6, 0, -1, 0, 0, 0, -2, 0, 0, 1.6, 0, 0, -2, 0, 1, 0, 0], 3));
      const cs = [pick(FG.clothesColors), pick(FG.clothesColors)].map(C), col = [];
      for (let k = 0; k < 6; k++) { const c = cs[k < 3 ? 0 : 1]; col.push(c.r, c.g, c.b); }
      kg.setAttribute('color', new THREE.Float32BufferAttribute(col, 3)); kg.computeVertexNormals();
      const kite = new THREE.Mesh(kg, new THREE.MeshBasicMaterial({ vertexColors: true, side: THREE.DoubleSide }));
      const bx = rr(-50, lvl.fin + 50), by = rr(32, 60), bz = rr(-160, -60), ph = rr(0, 6), s = rr(1.4, 2.2);
      kite.scale.setScalar(s);
      const tail = new THREE.Mesh(new THREE.BoxGeometry(0.06, 4, 0.06), new THREE.MeshBasicMaterial({ color: '#fff' })); tail.position.y = -4; kite.add(tail);
      scene.add(kite);
      anim.push((t) => { kite.position.set(bx + Math.sin(t * 0.4 + ph) * 3, by + Math.sin(t * 0.9 + ph) * 1.5, bz); kite.rotation.z = Math.sin(t * 1.3 + ph) * 0.3; });
    }

    // ---------- carros, busetas y TransMilenio ----------
    function vehicle(kind) {
      const g = new Geo();
      const col = kind === 'tm' ? '#d6302f' : kind === 'taxi' ? '#ffd21f' : kind === 'sitp' ? '#2f78c4' : pick(['#e8e8e8', '#9aa2ad', '#c0392b', '#2c3e50', '#16a085', '#7f8c8d']);
      const L = kind === 'tm' ? 18 : kind === 'sitp' ? 10 : 4.2, W = kind === 'tm' || kind === 'sitp' ? 2.5 : 1.8, H = kind === 'tm' || kind === 'sitp' ? 3.1 : 1.0;
      g.box(-W / 2, W / 2, 0.35, 0.35 + H, -L / 2, L / 2, C(col));
      if (kind === 'tm' || kind === 'sitp') {
        g.box(-W / 2 - 0.01, W / 2 + 0.01, 1.4, 2.6, -L / 2 + 0.6, L / 2 - 0.6, C('#26323d'), C(col), { top: true });
        if (kind === 'tm') g.box(-W / 2 - 0.02, W / 2 + 0.02, 0.35, 3.45, -0.4, 0.4, C('#2a2a2a'));
      } else g.box(-W / 2 + 0.12, W / 2 - 0.12, 1.35, 2.0, -1.1, 1.0, C('#26323d'), C(col));
      for (const [wx, wz] of [[-1, -1], [1, -1], [-1, 1], [1, 1]]) g.geom(new THREE.CylinderGeometry(0.36, 0.36, 0.3, 10), M4(wx * (W / 2 - 0.05), 0.36, wz * (L / 2 - 0.9), 0, 1, 1, 1, 0, Math.PI / 2), C('#1a1a1a'));
      const mesh = new THREE.Mesh(g.build(), mSolid); mesh.castShadow = true;
      return mesh;
    }
    for (const g of FG.calles) {
      const cx = (g.x0 + g.x1) / 2, w = g.x1 - g.x0;
      const lanes = g.tipo === 'avenida' ? (w > 30 ? [[-4.6, 'tm', 1], [4.6, 'tm', -1], [-11, 'car', 1], [11, 'car', -1]] : [[-5, 'car', 1], [5, 'sitp', -1]]) : w >= 5 ? [[0, 'car', R() < 0.5 ? 1 : -1]] : [];
      for (const [ox, kind0, dir] of lanes) {
        const n = kind0 === 'tm' ? 2 : 2;
        for (let i = 0; i < n; i++) {
          const kind = kind0 === 'car' ? (R() < 0.45 ? 'taxi' : 'car') : kind0;
          const v = vehicle(kind); scene.add(v);
          const sp = kind === 'tm' ? rr(11, 14) : rr(7, 12), off = rr(0, 300);
          v.rotation.y = dir > 0 ? 0 : Math.PI;
          anim.push((t) => { const z = ((off + t * sp) % 300); v.position.set(cx + ox, 0, dir > 0 ? -260 + z : 40 - z); });
        }
      }
    }
    for (const [z0] of blockZ.slice(0, 3)) {
      for (let i = 0; i < 6; i++) {
        const v = vehicle(R() < 0.3 ? 'taxi' : R() < 0.15 ? 'sitp' : 'car'); scene.add(v);
        const dir = i % 2 ? 1 : -1, sp = rr(6, 11), off = rr(0, X1 - X0), zz = z0 - 4 + dir * 1.6;
        v.rotation.y = dir > 0 ? Math.PI / 2 : -Math.PI / 2;
        anim.push((t) => { const d = (off + t * sp) % (X1 - X0); v.position.set(dir > 0 ? X0 + d : X1 - d, 0, zz); });
      }
    }

    // ---------- mallas finales ----------
    const add = (geo, mat, cast = true) => {
      const m = new THREE.Mesh(geo.build(), mat); m.castShadow = cast; m.receiveShadow = true; scene.add(m); return m;
    };
    add(GF, mFacade); add(GB, mBrick); add(GC, mConc, false); add(GS, mSolid); add(GT, mTower);
    add(GL, new THREE.MeshStandardMaterial({ vertexColors: true, side: THREE.DoubleSide, roughness: 0.9 }));
    add(GSign, mSign, false);
    add(GA, mAsph, false);

    return { update(t) { for (const f of anim) f(t); } };
  };
})();
