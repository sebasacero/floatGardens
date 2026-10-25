// Generación de la ciudad: Bogotá estilizada en manzanas, con cerros al oriente.
(function () {
  const C = W.CONFIG;
  const CELL = C.BLOCK + C.STREET;
  const HALF = (C.GRID * CELL) / 2;
  const LOT = C.BLOCK / 2 - 2; // los edificios dejan 2 m de andén
  const BOUNDS = { minX: -HALF - 10, maxX: HALF + 10, minZ: -HALF - 10, maxZ: HALF + 10 };

  // Aleatorio con semilla: el mapa sale igual en cada partida
  let seed = 20260;
  function rnd() {
    seed |= 0; seed = (seed + 0x6D2B79F5) | 0;
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  }
  const rr = (a, b) => a + rnd() * (b - a);
  const pickR = (arr) => arr[Math.floor(rnd() * arr.length)];

  const lineX = (k) => -HALF + k * CELL;
  function cellCenter(row, col) {
    return { x: -HALF + col * CELL + CELL / 2, z: -HALF + row * CELL + CELL / 2 };
  }
  function cellOf(x, z) {
    const col = Math.max(0, Math.min(C.GRID - 1, Math.floor((x + HALF) / CELL)));
    const row = Math.max(0, Math.min(C.GRID - 1, Math.floor((z + HALF) / CELL)));
    return { row, col };
  }
  function districtCode(row, col) { return W.DISTRICT_MAP[row][col]; }

  // ---------- Geometría combinada (pocas llamadas de dibujo) ----------
  class Geo {
    constructor() { this.pos = []; this.nor = []; this.uv = []; this.col = []; }
    tri(a, b, c, n, ua, ub, uc, color) {
      for (const [p, u] of [[a, ua], [b, ub], [c, uc]]) {
        this.pos.push(p[0], p[1], p[2]);
        this.nor.push(n[0], n[1], n[2]);
        this.uv.push(u[0], u[1]);
        this.col.push(color.r, color.g, color.b);
      }
    }
    quad(bl, br, tr, tl, n, u0, v0, u1, v1, color) {
      this.tri(bl, br, tr, n, [u0, v0], [u1, v0], [u1, v1], color);
      this.tri(bl, tr, tl, n, [u0, v0], [u1, v1], [u0, v1], color);
    }
    // Paredes de una caja (base en y0). uw/vh = metros por repetición de textura.
    walls(x, z, y0, w, h, d, color, uw, vh) {
      const y1 = y0 + h;
      const faces = [
        [[1, 0, 0], [0, 0, -1], d], [[-1, 0, 0], [0, 0, 1], d],
        [[0, 0, 1], [1, 0, 0], w], [[0, 0, -1], [-1, 0, 0], w],
      ];
      for (const [n, r, width] of faces) {
        const ext = n[0] !== 0 ? w / 2 : d / 2;
        const cx = x + n[0] * ext, cz = z + n[2] * ext;
        const hw = width / 2;
        const bl = [cx - r[0] * hw, y0, cz - r[2] * hw];
        const br = [cx + r[0] * hw, y0, cz + r[2] * hw];
        const tr = [br[0], y1, br[2]];
        const tl = [bl[0], y1, bl[2]];
        this.quad(bl, br, tr, tl, n, 0, y0 / vh, width / uw, y1 / vh, color);
      }
    }
    top(x, z, y, w, d, color, scale) {
      const s = scale || 8;
      const x0 = x - w / 2, x1 = x + w / 2, z0 = z - d / 2, z1 = z + d / 2;
      this.quad([x0, y, z1], [x1, y, z1], [x1, y, z0], [x0, y, z0], [0, 1, 0], x0 / s, -z1 / s, x1 / s, -z0 / s, color);
    }
    box(x, z, y0, w, h, d, color, uw, vh, topGeo, topColor) {
      this.walls(x, z, y0, w, h, d, color, uw || 4, vh || 3);
      (topGeo || this).top(x, z, y0 + h, w, d, topColor || color);
    }
    mesh(material) {
      const g = new THREE.BufferGeometry();
      g.setAttribute('position', new THREE.Float32BufferAttribute(this.pos, 3));
      g.setAttribute('normal', new THREE.Float32BufferAttribute(this.nor, 3));
      g.setAttribute('uv', new THREE.Float32BufferAttribute(this.uv, 2));
      g.setAttribute('color', new THREE.Float32BufferAttribute(this.col, 3));
      g.computeBoundingSphere();
      return new THREE.Mesh(g, material);
    }
  }

  const col = (hex) => new THREE.Color(hex);
  const WHITE = new THREE.Color('#ffffff');
  const tint = (hex, amt) => {
    const c = new THREE.Color(hex);
    const k = 1 + (rnd() - 0.5) * (amt || 0.15);
    return c.multiplyScalar(k);
  };

  // ---------- Colisiones ----------
  const boxes = [];
  const hash = new Map();
  const HCELL = 27;
  function hkey(i, j) { return i * 10007 + j; }
  function addCollider(minX, maxX, minZ, maxZ, h, tag) {
    const b = { minX, maxX, minZ, maxZ, h, tag };
    boxes.push(b);
    for (let i = Math.floor(minX / HCELL); i <= Math.floor(maxX / HCELL); i++) {
      for (let j = Math.floor(minZ / HCELL); j <= Math.floor(maxZ / HCELL); j++) {
        const k = hkey(i, j);
        if (!hash.has(k)) hash.set(k, []);
        hash.get(k).push(b);
      }
    }
    return b;
  }
  function nearBoxes(x, z, r) {
    const out = new Set();
    for (let i = Math.floor((x - r) / HCELL); i <= Math.floor((x + r) / HCELL); i++) {
      for (let j = Math.floor((z - r) / HCELL); j <= Math.floor((z + r) / HCELL); j++) {
        const list = hash.get(hkey(i, j));
        if (list) for (const b of list) out.add(b);
      }
    }
    return out;
  }

  const World = {
    CELL, HALF, BOUNDS, cellCenter, cellOf, lineX,
    boxes, lake: null, buses: [], planterSpots: {}, labels: [],

    build(scene) {
      this.scene = scene;
      const T = W.Tex;
      const mats = {
        brick: new THREE.MeshLambertMaterial({ map: T.brick, vertexColors: true }),
        stucco: new THREE.MeshLambertMaterial({ map: T.stucco, vertexColors: true }),
        glass: new THREE.MeshLambertMaterial({ map: T.glass, vertexColors: true }),
        concrete: new THREE.MeshLambertMaterial({ map: T.concrete, vertexColors: true }),
        warehouse: new THREE.MeshLambertMaterial({ map: T.warehouse, vertexColors: true }),
        roof: new THREE.MeshLambertMaterial({ map: T.roof, vertexColors: true }),
        sidewalk: new THREE.MeshLambertMaterial({ map: T.sidewalk, vertexColors: true }),
        grass: new THREE.MeshLambertMaterial({ map: T.grass, vertexColors: true }),
        cobble: new THREE.MeshLambertMaterial({ map: T.cobble, vertexColors: true }),
        plain: new THREE.MeshLambertMaterial({ vertexColors: true }),
      };
      this.mats = mats;
      const geo = {};
      for (const k in mats) geo[k] = new Geo();
      this.geo = geo;
      this.muralGeo = T.murals.map(() => new Geo());

      // Suelo de asfalto
      const ground = new THREE.Mesh(
        new THREE.PlaneGeometry(HALF * 2 + 400, HALF * 2 + 400),
        new THREE.MeshLambertMaterial({ map: T.asphalt }),
      );
      T.asphalt.repeat.set(90, 90);
      ground.rotation.x = -Math.PI / 2;
      scene.add(ground);

      this.zoneCells = new Map(W.ZONES.map((z) => [`${z.row},${z.col}`, z]));
      for (let row = 0; row < C.GRID; row++) {
        for (let c = 0; c < C.GRID; c++) this.buildCell(row, c);
      }
      this.buildStreets();
      this.buildCaracas();
      this.buildMountains();
      this.buildTrees();

      for (const k in geo) if (geo[k].pos.length) scene.add(geo[k].mesh(mats[k]));
      this.muralGeo.forEach((g, i) => { if (g.pos.length) scene.add(g.mesh(new THREE.MeshLambertMaterial({ map: T.murals[i] }))); });
      // la ciudad no se mueve: fijamos sus matrices para no recalcularlas en cada cuadro
      const moving = new Set([...this.buses.map((b) => b.mesh), this.cabin]);
      for (const o of scene.children) if (!moving.has(o)) W.Geom.freeze(o);
      this.buildMinimap();
    },

    // ---------- Manzanas ----------
    buildCell(row, c) {
      const g = this.geo;
      const code = districtCode(row, c);
      const style = W.DISTRICTS[code].style;
      const { x, z } = cellCenter(row, c);
      const B = C.BLOCK;
      const isHQ = row === W.HQ.row && c === W.HQ.col;
      const zone = this.zoneCells.get(`${row},${c}`);

      if (style === 'park') {
        this.parkSlab(row, c, x, z);
        if (zone) this.zonePlaza(zone, x, z, true);
        return;
      }
      if (style === 'plaza') {
        g.cobble.box(x, z, 0, B, 0.25, B, col('#d9d2c4'), 8, 8);
        this.plazaBolivar(x, z);
        return;
      }
      if (style === 'sede') {
        g.sidewalk.box(x, z, 0, B, 0.25, B, col('#ffffff'), 8, 8);
        this.sede(x, z);
        return;
      }
      const slabMat = style === 'colonial' ? g.cobble : g.sidewalk;
      slabMat.box(x, z, 0, B, 0.25, B, col('#ffffff'), 8, 8);
      if (isHQ) { this.hq(x, z); return; }
      if (zone) { this.zonePlaza(zone, x, z, false); return; }
      if (row === 7 && c === 10) { this.torre(x, z); return; }
      this.buildings(style, x, z);
    },

    building(mat, x, z, w, h, d, color, roofColor) {
      const g = this.geo;
      g[mat].walls(x, z, 0.25, w, h, d, color, 4, 3);
      g.roof.top(x, z, h + 0.25, w, d, roofColor || col('#ffffff'));
      addCollider(x - w / 2, x + w / 2, z - d / 2, z + d / 2, h + 0.25, 'building');
      // Toldos de colores en algunos locales del primer piso
      if (h < 40 && rnd() < 0.35) {
        const side = Math.floor(rnd() * 4);
        const color = col(pickR(['#d6423a', '#2fbf71', '#f2a541', '#47b5e0', '#e85d75']));
        const aw = Math.min(w, d) * 0.6;
        const off = [[w / 2 + 0.6, 0, 1.2, aw], [-w / 2 - 0.6, 0, 1.2, aw], [0, d / 2 + 0.6, aw, 1.2], [0, -d / 2 - 0.6, aw, 1.2]][side];
        g.plain.box(x + off[0], z + off[1], 3, off[2], 0.25, off[3], color);
      }
      // Murales
      if (h >= 6 && rnd() < 0.08 && (mat === 'brick' || mat === 'warehouse' || mat === 'concrete')) {
        const ti = Math.floor(rnd() * W.Tex.murals.length);
        const mw = Math.min(10, Math.max(w, d) * 0.7);
        const side = Math.floor(rnd() * 4);
        const y = 1.2 + mw / 4;
        // todos los murales con la misma textura van en una sola malla: [normal, derecha, centro]
        const [n, r, cx, cz] = [
          [[1, 0, 0], [0, 0, -1], x + w / 2 + 0.05, z], [[-1, 0, 0], [0, 0, 1], x - w / 2 - 0.05, z],
          [[0, 0, 1], [1, 0, 0], x, z + d / 2 + 0.05], [[0, 0, -1], [-1, 0, 0], x, z - d / 2 - 0.05],
        ][side];
        const hw = mw / 2, y0 = y - mw / 4, y1 = y + mw / 4;
        const bl = [cx - r[0] * hw, y0, cz - r[2] * hw], br = [cx + r[0] * hw, y0, cz + r[2] * hw];
        this.muralGeo[ti].quad(bl, br, [br[0], y1, br[2]], [bl[0], y1, bl[2]], n, 0, 0, 1, 1, WHITE);
      }
    },

    buildings(style, x, z) {
      const floors = (a, b) => Math.round(rr(a, b) / 3) * 3;
      const L = LOT;
      const grid = (n, fn) => {
        const s = (L * 2) / n;
        for (let i = 0; i < n; i++) {
          for (let j = 0; j < n; j++) fn(x - L + s * (i + 0.5), z - L + s * (j + 0.5), s - 0.02);
        }
      };
      switch (style) {
        case 'glass':
          if (rnd() < 0.5) this.building('glass', x, z, 30, floors(33, 75), 30, tint('#ffffff'), col('#9aa3ab'));
          else {
            this.building('glass', x - 9, z, 16, floors(24, 60), 34, tint('#e6f0ff'));
            this.building('glass', x + 9, z, 16, floors(21, 54), 34, tint('#ffffff'));
          }
          break;
        case 'tower':
          this.building('concrete', x, z, 36, 9, 36, tint('#ffffff'));
          this.building('glass', x, z, 24, floors(45, 105), 24, tint('#dfe9f5'));
          break;
        case 'brick':
          grid(2, (bx, bz, s) => this.building(rnd() < 0.85 ? 'brick' : 'concrete', bx, bz, s, floors(12, 27), s, tint('#ffffff', 0.25)));
          break;
        case 'house':
          grid(3, (bx, bz, s) => this.building('brick', bx, bz, s, floors(6, 10), s, tint('#ffffff', 0.3), col('#7a4a3a')));
          break;
        case 'colonial': {
          const colors = ['#ffffff', '#f6e27a', '#9fd3f0', '#f3b27a', '#d6f0c2', '#f7a6a6', '#ffd9a0'];
          grid(3, (bx, bz, s) => {
            const h = floors(5, 9);
            this.building('stucco', bx, bz, s, h, s, col(pickR(colors)), col('#b5533c'));
            this.geo.plain.box(bx, bz, h + 0.25, s + 0.8, 0.5, s + 0.8, col('#a8492f'));
          });
          break;
        }
        case 'warehouse':
          if (rnd() < 0.5) this.building('warehouse', x, z, 36, floors(8, 13), 36, tint('#ffffff', 0.3));
          else {
            this.building('warehouse', x, z - 9, 36, floors(8, 12), 17, tint('#e0e8ff', 0.3));
            this.building('warehouse', x, z + 9, 36, floors(6, 10), 17, tint('#fff0e0', 0.3));
          }
          break;
        default:
          grid(2, (bx, bz, s) => this.building(rnd() < 0.6 ? 'brick' : 'concrete', bx, bz, s, floors(9, 22), s, tint('#ffffff', 0.25)));
      }
    },

    // Parque Simón Bolívar: pasto que cubre también las calles internas, lago con balsas flotantes
    parkSlab(row, c, x, z) {
      const isP = (r, k) => r >= 0 && k >= 0 && r < C.GRID && k < C.GRID && districtCode(r, k) === 'P';
      const half = C.BLOCK / 2, full = CELL / 2;
      const x0 = x - (isP(row, c - 1) ? full : half), x1 = x + (isP(row, c + 1) ? full : half);
      const z0 = z - (isP(row - 1, c) ? full : half), z1 = z + (isP(row + 1, c) ? full : half);
      this.geo.grass.box((x0 + x1) / 2, (z0 + z1) / 2, 0, x1 - x0, 0.25, z1 - z0, tint('#ffffff', 0.1), 8, 8);
      if (isP(row, c + 1) && isP(row + 1, c) && isP(row + 1, c + 1)) {
        this.geo.grass.box(x + full, z + full, 0, C.STREET, 0.25, C.STREET, col('#ffffff'), 8, 8);
      }
      this.parkCells = this.parkCells || [];
      this.parkCells.push({ x, z });
      if (row === 1 && c === 2 && !this.lake) {
        const lx = x + CELL / 2, lz = z + 6;
        this.lake = { x: lx, z: lz, rx: 38, rz: 24 };
        const lakeMesh = new THREE.Mesh(new THREE.CircleGeometry(1, 40), new THREE.MeshLambertMaterial({ map: W.Tex.water }));
        W.Tex.water.repeat.set(6, 6);
        lakeMesh.scale.set(this.lake.rx, this.lake.rz, 1);
        lakeMesh.rotation.x = -Math.PI / 2;
        lakeMesh.position.set(lx, 0.3, lz);
        this.scene.add(lakeMesh);
        this.lakeMesh = lakeMesh;
      }
    },

    zonePlaza(zone, x, z, park) {
      const g = this.geo;
      if (!park) g.cobble.box(x, z, 0.25, C.BLOCK - 4, 0.05, C.BLOCK - 4, col('#ffffff'), 8, 8);
      // bancas
      for (const [bx, bz, w, d] of [[x - 14, z, 1, 5], [x + 14, z, 1, 5], [x, z - 14, 5, 1], [x, z + 14, 5, 1]]) {
        g.plain.box(bx, bz, 0.3, w, 0.5, d, col('#6b4a30'));
      }
      zone.center = { x, z };
      if (zone.id === 'simon' && this.lake) {
        zone.center = { x: this.lake.x - 8, z: this.lake.z + this.lake.rz + 10 };
        this.planterSpots[zone.id] = [-0.75, -0.55, -0.35, -0.15].map((a) => {
          const ang = Math.PI / 2 + a;
          return { x: this.lake.x + Math.cos(ang) * this.lake.rx * 0.9 - 6, z: this.lake.z + Math.sin(ang) * this.lake.rz * 0.9, water: true };
        });
      } else {
        this.planterSpots[zone.id] = [0, 1, 2, 3].map((i) => {
          const a = (i / 4) * Math.PI * 2 + Math.PI / 4;
          return { x: x + Math.cos(a) * 8, z: z + Math.sin(a) * 8 };
        });
      }
      this.treeSpots = this.treeSpots || [];
      for (const [tx, tz] of [[-16, -16], [16, -16], [-16, 16], [16, 16]]) this.treeSpots.push({ x: x + tx, z: z + tz });
    },

    hq(x, z) {
      const g = this.geo;
      g.cobble.box(x, z, 0.25, C.BLOCK - 4, 0.05, C.BLOCK - 4, col('#ffffff'), 8, 8);
      // edificio de ladrillo con jardín en la azotea
      this.building('brick', x, z - 12, 34, 15, 14, col('#ffffff'), col('#5a8f4a'));
      for (let i = -3; i <= 3; i++) g.plain.box(x + i * 4.5, z - 12, 15.25, 3, 1.4, 10, col(i % 2 ? '#3fa35b' : '#2a7a41'));
      const signMat = new THREE.MeshBasicMaterial({ map: W.Tex.sign('FLOAT GARDENS', '#0f3d2e', '#7dffb0'), side: THREE.DoubleSide });
      const sign = new THREE.Mesh(new THREE.PlaneGeometry(20, 4), signMat);
      sign.position.set(x, 11, z - 4.9);
      this.scene.add(sign);
      // piscina hidropónica
      g.plain.box(x - 4, z + 7, 0.3, 22, 0.6, 11, col('#9fb7a8'));
      const pool = new THREE.Mesh(new THREE.PlaneGeometry(20.5, 9.5), new THREE.MeshLambertMaterial({ map: W.Tex.water }));
      pool.rotation.x = -Math.PI / 2;
      pool.position.set(x - 4, 0.92, z + 7);
      this.scene.add(pool);
      this.planterSpots.hq = [];
      for (let i = 0; i < 6; i++) this.planterSpots.hq.push({ x: x - 12 + (i % 3) * 8, z: z + 4.5 + Math.floor(i / 3) * 5, water: true, y: 0.95 });
      // kiosco de la tienda
      g.plain.box(x + 13, z + 7, 0.3, 6, 1.2, 3, col('#5a3a24'));
      g.plain.box(x + 13, z + 7, 3.4, 7, 0.3, 4, col('#2fbf71'));
      for (const [px, pz] of [[-3.2, -1.8], [3.2, -1.8], [-3.2, 1.8], [3.2, 1.8]]) g.plain.box(x + 13 + px, z + 7 + pz, 0.3, 0.2, 3.1, 0.2, col('#ddd'));
      addCollider(x + 10, x + 16, z + 5.5, z + 8.5, 1.5, 'kiosk');
      this.hqSpot = { x, z, shop: { x: x + 13, z: z + 10 }, spawn: { x: x - 2, z: z + 16 } };
      this.treeSpots = this.treeSpots || [];
      this.treeSpots.push({ x: x - 17, z: z + 17 });
    },

    torre(x, z) {
      this.building('concrete', x, z, 36, 9, 36, col('#ffffff'));
      this.building('glass', x, z, 22, 140, 22, col('#d0dcea'));
      this.crownMat = new THREE.MeshBasicMaterial({ color: '#ff3366' });
      const crown = new THREE.Mesh(new THREE.BoxGeometry(23, 10, 23), this.crownMat);
      crown.position.set(x, 136, z);
      this.scene.add(crown);
      this.landmarks = this.landmarks || [];
      this.landmarks.push({ name: 'Torre del Centro', x, z });
    },

    plazaBolivar(x, z) {
      const g = this.geo;
      g.plain.box(x, z, 0.25, 4, 2.5, 4, col('#cfc8bb'));
      g.plain.box(x, z, 2.75, 1.4, 3, 1.4, col('#5d6b5a'));
      addCollider(x - 2, x + 2, z - 2, z + 2, 2.75, 'statue');
      this.bossArena = { x, z };
    },

    sede(x, z) {
      const g = this.geo;
      const gray = col('#c9c4ba');
      g.concrete.walls(x, z - 4, 0.25, 36, 18, 26, gray, 4, 3);
      g.roof.top(x, z - 4, 18.25, 36, 26, col('#ffffff'));
      addCollider(x - 18, x + 18, z - 17, z + 9, 18.25, 'sede');
      // columnas y frontón
      // columnas y frontón: una sola malla
      const colGeo = new THREE.CylinderGeometry(0.9, 1, 14, 10);
      const portico = [];
      for (let i = 0; i < 8; i++) {
        const cx = x - 14 + i * 4;
        portico.push({ geo: colGeo, matrix: W.Geom.matrix(cx, 7.25, z + 12), color: '#e8e3d8' });
        addCollider(cx - 1, cx + 1, z + 11, z + 13, 14, 'column');
      }
      g.plain.box(x, z + 11, 14.25, 34, 2, 5, col('#ddd6c8'));
      const shape = new THREE.Shape();
      shape.moveTo(-17, 0); shape.lineTo(17, 0); shape.lineTo(0, 5); shape.lineTo(-17, 0);
      portico.push({ geo: new THREE.ExtrudeGeometry(shape, { depth: 5, bevelEnabled: false }), matrix: W.Geom.matrix(x, 16.25, z + 8.5), color: '#e8e3d8' });
      this.scene.add(new THREE.Mesh(W.Geom.merge(portico), W.Geom.vcMat()));
      const frieze = new THREE.Mesh(new THREE.PlaneGeometry(26, 1.8),
        new THREE.MeshBasicMaterial({ map: W.Tex.sign('LA FEDERACIÓN', '#8f1f1a', '#fff3dc', 512, 64) }));
      frieze.position.set(x, 15.25, z + 13.55);
      this.scene.add(frieze);
      const banMat = new THREE.MeshLambertMaterial({ map: W.Tex.sign('★', '#c0392b', '#ffd34d', 128, 256, '900 120px system-ui'), side: THREE.DoubleSide });
      const banGeo = new THREE.PlaneGeometry(3, 8);
      this.scene.add(new THREE.Mesh(W.Geom.merge([-16, 16].map((bx) => ({ geo: banGeo, matrix: W.Geom.matrix(x + bx, 9, z + 9.1) }))), banMat));
      this.sedeSpot = { x, z: z + 16 };
    },

    // ---------- Calles ----------
    buildStreets() {
      const g = this.geo.plain;
      const white = col('#e8e8e0');
      const yellow = col('#e8c547');
      for (let k = 0; k <= C.GRID; k++) {
        const p = lineX(k);
        for (let s = 0; s < C.GRID; s++) {
          const a = -HALF + s * CELL + C.STREET / 2 + 1;
          const b = a + C.BLOCK - 2;
          for (let t = a; t < b; t += 6) {
            // carreras (norte-sur)
            if (k !== 6) g.top(p, t + 1.5, 0.03, 0.25, 3, k % 3 === 0 ? yellow : white);
            // calles (oriente-occidente)
            g.top(t + 1.5, p, 0.03, 3, 0.25, k % 3 === 0 ? yellow : white);
          }
        }
      }
      // Cebras alrededor de la sede de Float Gardens
      const hq = cellCenter(W.HQ.row, W.HQ.col);
      for (let i = -5; i <= 5; i++) {
        g.top(hq.x + i * 1.2, hq.z + CELL / 2 - C.STREET / 2 + 1.5, 0.04, 0.6, 2.5, white);
        g.top(hq.x + i * 1.2, hq.z - CELL / 2 + C.STREET / 2 - 1.5, 0.04, 0.6, 2.5, white);
      }
    },

    // Av. Caracas con carril exclusivo, estaciones y buses rojos articulados
    buildCaracas() {
      const g = this.geo.plain;
      const x = lineX(6);
      g.top(x, 0, 0.025, 9, HALF * 2, col('#8a3b34'));
      g.top(x - 4.6, 0, 0.04, 0.3, HALF * 2, col('#f0f0f0'));
      g.top(x + 4.6, 0, 0.04, 0.3, HALF * 2, col('#f0f0f0'));
      this.stations = [];
      const glassBox = new THREE.BoxGeometry(2.2, 2.8, 25);
      const glassParts = [];
      for (const row of [1, 4, 7, 10]) {
        const z = cellCenter(row, 6).z;
        g.box(x, z, 0, 2.4, 0.4, 26, col('#9a9a9a'));
        g.box(x, z, 3.2, 2.8, 0.3, 26, col('#c0392b'));
        glassParts.push({ geo: glassBox, matrix: W.Geom.matrix(x, 1.8, z) });
        addCollider(x - 1.2, x + 1.2, z - 13, z + 13, 3.5, 'station');
        this.stations.push(z);
      }
      // vidrios de las 4 estaciones en una malla
      this.scene.add(new THREE.Mesh(W.Geom.merge(glassParts), new THREE.MeshLambertMaterial({ color: '#9fd0e8', transparent: true, opacity: 0.45 })));
      // bus articulado: carrocería, ventanas y fuelle en una sola geometría compartida por los 4 buses
      const M = W.Geom.matrix;
      const busParts = [{ geo: new THREE.BoxGeometry(2.3, 2.6, 1), matrix: M(0, 1.8, 0), color: '#333' }];
      for (const off of [-5.2, 5.2]) {
        busParts.push({ geo: new THREE.BoxGeometry(2.5, 3, 9.6), matrix: M(0, 1.9, off), color: '#c62828' });
        busParts.push({ geo: new THREE.BoxGeometry(2.56, 1, 8.6), matrix: M(0, 2.3, off), color: '#1d2b36' });
      }
      const busGeo = W.Geom.merge(busParts);
      for (let i = 0; i < 4; i++) {
        const bus = new THREE.Mesh(busGeo, W.Geom.vcMat());
        const dir = i % 2 ? 1 : -1;
        const lane = x + (dir > 0 ? 2.6 : -2.6);
        bus.position.set(lane, 0, -HALF + ((i * 0.27) % 1) * HALF * 2);
        bus.rotation.y = dir > 0 ? 0 : Math.PI;
        this.scene.add(bus);
        this.buses.push({ mesh: bus, dir, x: lane, z: bus.position.z, stop: 0, lastStation: null });
      }
    },

    // Cerros orientales con Monserrate, su santuario blanco y el teleférico
    buildMountains() {
      const mat = new THREE.MeshLambertMaterial({ color: '#3d6b3a', flatShading: true });
      const mat2 = new THREE.MeshLambertMaterial({ color: '#2f5a33', flatShading: true });
      const peaks = [
        [430, -380, 130, 150], [470, -260, 150, 190], [440, -120, 120, 160], [455, 20, 140, 185],
        [440, 150, 130, 210], [470, 300, 150, 180], [430, 430, 120, 140], [560, -200, 160, 230], [580, 150, 170, 240],
      ];
      // un cerro por material en lugar de una malla por pico
      const byMat = [[], []];
      peaks.forEach(([px, pz, r, h], i) => {
        byMat[i % 2].push({ geo: new THREE.ConeGeometry(r, h, 7), matrix: W.Geom.matrix(px, h / 2 - 2, pz, 0, i, 0) });
      });
      this.scene.add(new THREE.Mesh(W.Geom.merge(byMat[1]), mat), new THREE.Mesh(W.Geom.merge(byMat[0]), mat2));
      // Monserrate (pico de 210 m en z=150)
      const white = new THREE.MeshLambertMaterial({ color: '#f5f5f0' });
      const church = new THREE.Mesh(W.Geom.merge([
        { geo: new THREE.BoxGeometry(14, 8, 8), matrix: W.Geom.matrix(0, 4, 0) },
        { geo: new THREE.BoxGeometry(4, 16, 4), matrix: W.Geom.matrix(-6, 8, 0) },
      ]), white);
      church.position.set(440, 198, 150);
      this.scene.add(church);
      // teleférico
      const start = new THREE.Vector3(352, 2, 170);
      const end = new THREE.Vector3(432, 196, 152);
      const cable = new THREE.Line(new THREE.BufferGeometry().setFromPoints([start, end]), new THREE.LineBasicMaterial({ color: '#222' }));
      this.scene.add(cable);
      this.cabin = new THREE.Mesh(new THREE.BoxGeometry(3, 2.4, 3), new THREE.MeshLambertMaterial({ color: '#d6423a' }));
      this.cable = { start, end };
      this.scene.add(this.cabin);
      this.landmarks.push({ name: 'Monserrate', x: 440, z: 150 });
    },

    buildTrees() {
      const spots = (this.treeSpots || []).slice();
      for (const p of this.parkCells || []) {
        for (let i = 0; i < 14; i++) {
          const tx = p.x + rr(-26, 26), tz = p.z + rr(-26, 26);
          if (this.lake && this.inLake(tx, tz, 4)) continue;
          spots.push({ x: tx, z: tz });
        }
      }
      // árboles en los andenes de Teusaquillo y Chapinero
      for (let row = 0; row < C.GRID; row++) {
        for (let c = 0; c < C.GRID; c++) {
          const code = districtCode(row, c);
          if (code !== 'U' && code !== 'C' && code !== 'T') continue;
          const { x, z } = cellCenter(row, c);
          for (const [dx, dz] of [[-19, -10], [-19, 10], [19, -10], [19, 10], [-10, 19], [10, 19]]) {
            if (rnd() < 0.5) spots.push({ x: x + dx, z: z + dz, small: true });
          }
        }
      }
      const trunkGeo = new THREE.CylinderGeometry(0.25, 0.35, 3, 6);
      const leafGeo = new THREE.IcosahedronGeometry(2.2, 0);
      const trunks = new THREE.InstancedMesh(trunkGeo, new THREE.MeshLambertMaterial({ color: '#6b4a30' }), spots.length);
      const leaves = new THREE.InstancedMesh(leafGeo, new THREE.MeshLambertMaterial({ color: '#ffffff', flatShading: true }), spots.length);
      const m = new THREE.Matrix4();
      const leafColors = ['#3f8f3f', '#2f7a3a', '#4f9f45', '#5a8f3a'];
      spots.forEach((s, i) => {
        const k = s.small ? 0.7 : rr(0.9, 1.5);
        m.makeScale(k, k, k).setPosition(s.x, 1.5 * k + 0.25, s.z);
        trunks.setMatrixAt(i, m);
        m.makeScale(k, k * 1.15, k).setPosition(s.x, 4 * k + 0.25, s.z);
        leaves.setMatrixAt(i, m);
        leaves.setColorAt(i, new THREE.Color(pickR(leafColors)));
      });
      this.scene.add(trunks, leaves);
    },

    // ---------- Minimapa pre-renderizado ----------
    buildMinimap() {
      const S = 1; // px por metro
      const size = Math.ceil(HALF * 2 + 40);
      const cv = document.createElement('canvas');
      cv.width = cv.height = size;
      const ctx = cv.getContext('2d');
      const o = HALF + 20;
      ctx.fillStyle = '#2b3036';
      ctx.fillRect(0, 0, size, size);
      const colors = { glass: '#5d7f99', tower: '#6d8aa3', brick: '#8f5a46', house: '#8a6150', colonial: '#c9a46a', warehouse: '#7c858c', mixed: '#7e6f66', park: '#3f7a3a', plaza: '#a89f90', sede: '#a33a30' };
      for (let row = 0; row < C.GRID; row++) {
        for (let c = 0; c < C.GRID; c++) {
          const { x, z } = cellCenter(row, c);
          const style = W.DISTRICTS[districtCode(row, c)].style;
          ctx.fillStyle = colors[style];
          const s = style === 'park' ? CELL : C.BLOCK;
          ctx.fillRect((x - s / 2 + o) * S, (z - s / 2 + o) * S, s * S, s * S);
        }
      }
      if (this.lake) {
        ctx.fillStyle = '#3f8fb3';
        ctx.beginPath();
        ctx.ellipse((this.lake.x + o) * S, (this.lake.z + o) * S, this.lake.rx * S, this.lake.rz * S, 0, 0, Math.PI * 2);
        ctx.fill();
      }
      ctx.fillStyle = '#b8443a';
      ctx.fillRect((lineX(6) - 2 + o) * S, 0, 4 * S, size);
      this.minimap = { canvas: cv, offset: o, scale: S };
    },

    // ---------- Consultas ----------
    districtCodeAt(x, z) {
      const { row, col: c } = cellOf(x, z);
      return districtCode(row, c);
    },
    districtAt(x, z) {
      const { row, col: c } = cellOf(x, z);
      const code = districtCode(row, c);
      return { code, ...W.DISTRICTS[code], row, col: c };
    },

    streetName(x, z) {
      const k = Math.max(0, Math.min(C.GRID, Math.round((x + HALF) / CELL)));
      const j = Math.max(0, Math.min(C.GRID, Math.round((z + HALF) / CELL)));
      return `${W.CALLES[j]} con ${W.CARRERAS[k]}`;
    },

    inLake(x, z, pad) {
      const L = this.lake;
      if (!L) return false;
      const dx = (x - L.x) / (L.rx + (pad || 0)), dz = (z - L.z) / (L.rz + (pad || 0));
      return dx * dx + dz * dz < 1;
    },

    // Resuelve colisiones de un círculo (x, z) contra edificios, lago y límites. Modifica p.
    collide(p, r, y) {
      const py = y || 0;
      for (const b of nearBoxes(p.x, p.z, r + 1)) {
        if (py >= b.h - 0.05) continue;
        const cx = Math.max(b.minX, Math.min(p.x, b.maxX));
        const cz = Math.max(b.minZ, Math.min(p.z, b.maxZ));
        let dx = p.x - cx, dz = p.z - cz;
        const d2 = dx * dx + dz * dz;
        if (d2 >= r * r) continue;
        if (d2 > 1e-8) {
          const d = Math.sqrt(d2);
          p.x = cx + (dx / d) * r;
          p.z = cz + (dz / d) * r;
        } else {
          // dentro de la caja: salir por el lado más cercano
          const opts = [[b.minX - r - p.x, 0], [b.maxX + r - p.x, 0], [0, b.minZ - r - p.z], [0, b.maxZ + r - p.z]];
          opts.sort((a, c2) => Math.abs(a[0] + a[1]) - Math.abs(c2[0] + c2[1]));
          p.x += opts[0][0]; p.z += opts[0][1];
        }
      }
      const L = this.lake;
      if (L && this.inLake(p.x, p.z, r)) {
        const ang = Math.atan2((p.z - L.z) / L.rz, (p.x - L.x) / L.rx);
        p.x = L.x + Math.cos(ang) * (L.rx + r);
        p.z = L.z + Math.sin(ang) * (L.rz + r);
      }
      p.x = Math.max(BOUNDS.minX, Math.min(BOUNDS.maxX, p.x));
      p.z = Math.max(BOUNDS.minZ, Math.min(BOUNDS.maxZ, p.z));
    },

    // Altura del piso: andenes (0.25 m) y cajas bajas sobre las que se puede subir
    baseY(x, z) {
      const { row, col: c } = cellOf(x, z);
      if (districtCode(row, c) === 'P') return 0.25;
      const cc = cellCenter(row, c);
      return Math.abs(x - cc.x) <= C.BLOCK / 2 && Math.abs(z - cc.z) <= C.BLOCK / 2 ? 0.25 : 0;
    },
    groundAt(x, z, y) {
      let g = this.baseY(x, z);
      for (const b of nearBoxes(x, z, 0.1)) {
        if (x > b.minX && x < b.maxX && z > b.minZ && z < b.maxZ && b.h <= y + 0.35 && b.h > g) g = b.h;
      }
      return g;
    },

    // ¿Hay una pared en este punto?
    solidAt(x, y, z) {
      for (const b of nearBoxes(x, z, 0.1)) {
        if (y < b.h && x > b.minX && x < b.maxX && z > b.minZ && z < b.maxZ) return true;
      }
      return y < 0;
    },

    // Distancia a la primera caja que corta el rayo (o maxDist)
    raycast(o, d, maxDist) {
      let best = maxDist;
      const ex = o.x + d.x * maxDist, ez = o.z + d.z * maxDist;
      const minX = Math.min(o.x, ex) - 1, maxX = Math.max(o.x, ex) + 1;
      const minZ = Math.min(o.z, ez) - 1, maxZ = Math.max(o.z, ez) + 1;
      for (const b of boxes) {
        if (b.maxX < minX || b.minX > maxX || b.maxZ < minZ || b.minZ > maxZ) continue;
        let t0 = 0, t1 = best;
        const axes = [[o.x, d.x, b.minX, b.maxX], [o.y, d.y, -1, b.h], [o.z, d.z, b.minZ, b.maxZ]];
        let hit = true;
        for (const [oo, dd, lo, hi] of axes) {
          if (Math.abs(dd) < 1e-9) { if (oo < lo || oo > hi) { hit = false; break; } continue; }
          let ta = (lo - oo) / dd, tb = (hi - oo) / dd;
          if (ta > tb) [ta, tb] = [tb, ta];
          t0 = Math.max(t0, ta); t1 = Math.min(t1, tb);
          if (t0 > t1) { hit = false; break; }
        }
        if (hit && t0 < best) best = t0;
      }
      if (d.y < 0) { const tg = -o.y / d.y; if (tg < best) best = tg; }
      return best;
    },

    // Punto aleatorio en un andén (para peatones, botín y enemigos)
    sidewalkPoint(filter) {
      for (let tries = 0; tries < 40; tries++) {
        const row = Math.floor(Math.random() * C.GRID), c = Math.floor(Math.random() * C.GRID);
        const code = districtCode(row, c);
        if (filter && !filter(code, row, c)) continue;
        const { x, z } = cellCenter(row, c);
        const s = C.BLOCK / 2 - 0.9;
        const side = Math.floor(Math.random() * 4);
        const t = (Math.random() * 2 - 1) * s;
        const p = [[x + t, z - s], [x + t, z + s], [x - s, z + t], [x + s, z + t]][side];
        const pos = { x: p[0], z: p[1] };
        if (code === 'P' && this.inLake(pos.x, pos.z, 3)) continue;
        if (!this.solidAt(pos.x, 0.5, pos.z)) return { ...pos, row, col: c, code };
      }
      return { x: 0, z: 0, row: 6, col: 6, code: 'I' };
    },

    update(dt, t, player) {
      // buses
      for (const bus of this.buses) {
        if (bus.stop > 0) { bus.stop -= dt; continue; }
        bus.z += bus.dir * 11 * dt;
        if (bus.z > HALF + 20) bus.z = -HALF - 20;
        if (bus.z < -HALF - 20) bus.z = HALF + 20;
        for (const sz of this.stations) {
          if (Math.abs(bus.z - sz) < 0.4 && bus.lastStation !== sz) { bus.stop = 3; bus.lastStation = sz; }
        }
        bus.mesh.position.z = bus.z;
        // empuja al jugador fuera del bus
        if (player && Math.abs(player.pos.x - bus.x) < 1.25 + C.PLAYER_RADIUS && Math.abs(player.pos.z - bus.z) < 10.5) {
          player.pos.x = bus.x + Math.sign(player.pos.x - bus.x || 1) * (1.3 + C.PLAYER_RADIUS);
        }
      }
      if (this.crownMat) this.crownMat.color.setHSL((t * 0.05) % 1, 0.8, 0.55);
      if (this.cabin) {
        const k = (Math.sin(t * 0.08) + 1) / 2;
        this.cabin.position.lerpVectors(this.cable.start, this.cable.end, k);
        this.cabin.position.y -= 1.8;
      }
      if (this.lakeMesh) W.Tex.water.offset.set(t * 0.01, t * 0.006);
    },
  };

  W.World = World;
})();
