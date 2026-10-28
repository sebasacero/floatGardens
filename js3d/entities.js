// Personajes low-poly y objetos del mundo: enemigos, peatones, cultivos, botín, zonas, proyectiles.
(function () {
  const C = W.CONFIG;
  const matCache = new Map();
  function mat(color, opts) {
    const key = color + JSON.stringify(opts || {});
    if (!matCache.has(key)) matCache.set(key, new THREE.MeshLambertMaterial(Object.assign({ color }, opts)));
    return matCache.get(key);
  }
  const boxGeo = new THREE.BoxGeometry(1, 1, 1);
  function box(w, h, d, color, x, y, z) {
    const m = new THREE.Mesh(boxGeo, mat(color));
    m.scale.set(w, h, d);
    m.position.set(x || 0, y || 0, z || 0);
    return m;
  }
  function pivot(x, y, z, child) {
    const g = new THREE.Group();
    g.position.set(x, y, z);
    if (child) g.add(child);
    return g;
  }

  // Sombra circular bajo los personajes
  const shadowTex = (() => {
    const c = document.createElement('canvas');
    c.width = c.height = 64;
    const ctx = c.getContext('2d');
    const g = ctx.createRadialGradient(32, 32, 2, 32, 32, 30);
    g.addColorStop(0, 'rgba(0,0,0,0.45)');
    g.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, 64, 64);
    return new THREE.CanvasTexture(c);
  })();
  const shadowMat = new THREE.MeshBasicMaterial({ map: shadowTex, transparent: true, depthWrite: false });
  const shadowGeo = new THREE.PlaneGeometry(1.4, 1.4);

  // AeroPress X-1
  function makeAeroPress() {
    const g = new THREE.Group();
    const chamber = new THREE.Mesh(new THREE.CylinderGeometry(0.09, 0.09, 0.42, 10), mat('#bcd6e6', { transparent: true, opacity: 0.8 }));
    chamber.rotation.x = Math.PI / 2;
    const coffee = new THREE.Mesh(new THREE.CylinderGeometry(0.075, 0.075, 0.26, 10), mat('#4a2c1d'));
    coffee.rotation.x = Math.PI / 2;
    coffee.position.z = 0.06;
    const plunger = new THREE.Mesh(new THREE.CylinderGeometry(0.08, 0.08, 0.22, 10), mat('#333'));
    plunger.rotation.x = Math.PI / 2;
    plunger.position.z = -0.28;
    const cap = new THREE.Mesh(new THREE.CylinderGeometry(0.11, 0.11, 0.04, 10), mat('#2fbf71'));
    cap.rotation.x = Math.PI / 2;
    cap.position.z = 0.23;
    g.add(chamber, coffee, plunger, cap);
    g.userData.muzzle = new THREE.Object3D();
    g.userData.muzzle.position.z = 0.28;
    g.add(g.userData.muzzle);
    return g;
  }

  // Humanoide genérico. Cada parte rígida (cuerpo, piernas, brazos) es una sola malla con colores por vértice:
  // 5 llamadas de dibujo por personaje en vez de ~15. Las geometrías se comparten entre personajes iguales.
  const partCache = new Map();
  function parts(list, opts) {
    const key = JSON.stringify(list) + JSON.stringify(opts || {});
    if (!partCache.has(key)) {
      partCache.set(key, W.Geom.merge(list.map(([w, h, d, color, x, y, z, rz]) => ({
        geo: boxGeo, matrix: W.Geom.matrix(x, y, z, 0, 0, rz, w, h, d), color,
      }))));
    }
    return new THREE.Mesh(partCache.get(key), W.Geom.vcMat(opts));
  }
  function makeHuman(o) {
    const root = new THREE.Group();
    const skin = o.skin || '#d9a273';
    const leg = [[0.22, 0.85, 0.26, o.pants, 0, -0.42, 0]];
    const legL = pivot(-0.13, 0.85, 0, parts(leg));
    const legR = pivot(0.13, 0.85, 0, parts(leg));
    const arm = (extra) => [[0.17, 0.66, 0.2, o.shirt, 0, -0.3, 0], [0.15, 0.14, 0.17, skin, 0, -0.66, 0]].concat(extra || []);
    const armL = pivot(-0.4, 1.52, 0, parts(arm(o.holdL)));
    const armR = pivot(0.4, 1.52, 0, parts(arm(o.holdR)));
    const b = [
      [0.62, 0.72, 0.34, o.shirt, 0, 1.22, 0], // torso
      [0.36, 0.38, 0.36, skin, 0, 1.79, 0], // cabeza
      [0.06, 0.06, 0.02, '#111', -0.08, 1.82, 0.185], [0.06, 0.06, 0.02, '#111', 0.08, 1.82, 0.185], // ojos
    ];
    if (o.hair) b.push([0.38, 0.1, 0.38, o.hair, 0, 2.0, 0]);
    if (o.apron) b.push([0.5, 0.75, 0.04, o.apron, 0, 1.1, 0.19]);
    if (o.tie) b.push([0.08, 0.4, 0.03, o.tie, 0, 1.32, 0.18]);
    if (o.mustache) b.push([0.2, 0.05, 0.03, '#2a1a10', 0, 1.7, 0.185]);
    if (o.cap) b.push([0.4, 0.14, 0.4, o.cap, 0, 2.03, 0], [0.36, 0.04, 0.22, o.cap, 0, 1.97, 0.26]);
    if (o.hat === 'fedora') b.push([0.6, 0.04, 0.6, '#222', 0, 1.99, 0], [0.38, 0.2, 0.38, '#222', 0, 2.1, 0], [0.39, 0.05, 0.39, '#c0392b', 0, 2.03, 0]);
    if (o.hat === 'beret') b.push([0.44, 0.1, 0.44, '#8f1f1a', 0.03, 2.02, 0]);
    if (o.hat === 'tophat') b.push([0.62, 0.04, 0.62, '#111', 0, 1.99, 0], [0.38, 0.5, 0.38, '#111', 0, 2.25, 0], [0.39, 0.07, 0.39, '#c0392b', 0, 2.05, 0]);
    if (o.sash) b.push([0.08, 0.85, 0.36, o.sash, 0, 1.22, 0, 0.6]);
    if (o.badge) b.push([0.1, 0.1, 0.03, '#e6c34a', 0.17, 1.4, 0.18]);
    const body = parts(b);
    root.add(body);
    body.add(legL, legR, armL, armR);
    const shadow = new THREE.Mesh(shadowGeo, shadowMat);
    shadow.rotation.x = -Math.PI / 2;
    shadow.position.y = 0.02;
    root.add(shadow);
    if (o.scale) body.scale.setScalar(o.scale);
    if (o.scale) shadow.scale.setScalar(o.scale);

    return {
      root, body, legL, legR, armL, armR,
      animate(phase, amount, aiming) {
        const s = Math.sin(phase) * 0.7 * amount;
        legL.rotation.x = s;
        legR.rotation.x = -s;
        if (aiming) {
          armR.rotation.x = -1.45;
          armL.rotation.x = -1.3;
          armL.rotation.z = -0.45;
        } else {
          armL.rotation.x = -s;
          armR.rotation.x = s;
          armL.rotation.z = 0;
        }
      },
    };
  }

  // Barras de vida flotantes (sprites)
  const barBgMat = new THREE.SpriteMaterial({ color: '#111', depthTest: false, transparent: true, opacity: 0.7 });
  function makeBar(width, color) {
    const g = new THREE.Group();
    const bg = new THREE.Sprite(barBgMat);
    bg.scale.set(width + 0.06, 0.14, 1);
    const fill = new THREE.Sprite(new THREE.SpriteMaterial({ color, depthTest: false }));
    fill.center.set(0, 0.5);
    fill.position.x = -width / 2;
    fill.scale.set(width, 0.09, 1);
    bg.renderOrder = 10; fill.renderOrder = 11;
    g.add(bg, fill);
    g.userData = { fill, width };
    return g;
  }
  function setBar(bar, frac) {
    bar.userData.fill.scale.x = Math.max(0.001, bar.userData.width * frac);
  }

  const iconCache = new Map();
  function iconSprite(text, size) {
    if (!iconCache.has(text)) {
      const c = document.createElement('canvas');
      c.width = c.height = 64;
      const ctx = c.getContext('2d');
      ctx.fillStyle = 'rgba(255,255,255,0.95)';
      ctx.beginPath(); ctx.arc(32, 32, 28, 0, Math.PI * 2); ctx.fill();
      ctx.font = '36px system-ui, "Apple Color Emoji", "Segoe UI Emoji", sans-serif';
      ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
      ctx.fillStyle = '#4a2c1d';
      ctx.fillText(text, 32, 35);
      iconCache.set(text, new THREE.SpriteMaterial({ map: new THREE.CanvasTexture(c), depthTest: false }));
    }
    const s = new THREE.Sprite(iconCache.get(text));
    s.scale.setScalar(size || 0.6);
    s.renderOrder = 12;
    return s;
  }

  // ---------- Jugador ----------
  function makeBarista() {
    const h = makeHuman({ shirt: '#f5f1e8', pants: '#2c2a27', apron: '#0f3d2e', cap: '#2fbf71', hair: '#2a1a10', skin: '#c98f62' });
    const gun = makeAeroPress();
    gun.position.set(0, -0.6, 0.18);
    gun.rotation.x = Math.PI / 2;
    h.armR.add(gun);
    h.gun = gun;
    return h;
  }

  // ---------- Enemigos ----------
  class Enemy {
    constructor(kind, x, z, opts) {
      const t = W.ENEMY_TYPES[kind];
      this.kind = kind;
      this.t = t;
      this.name = t.name;
      this.maxHp = t.hp * (opts && opts.hpMul || 1);
      this.hp = this.maxHp;
      this.pos = { x, z };
      this.y = 0;
      this.speed = t.speed * (0.9 + Math.random() * 0.2);
      this.cool = 1 + Math.random();
      this.phase = Math.random() * 6;
      this.state = 'patrol';
      this.home = { x, z };
      this.goal = null;
      this.zoneId = opts && opts.zoneId;
      this.alerted = !!(opts && opts.alerted);
      this.ko = 0;
      this.strafe = Math.random() < 0.5 ? 1 : -1;
      const look = { shirt: t.color, pants: '#26282d', hat: t.hat, tie: '#c0392b', badge: true, mustache: Math.random() < 0.5, skin: Math.random() < 0.5 ? '#d9a273' : '#a8714a' };
      // el guardia lleva una pila de sellos; el inspector, su carpeta
      if (kind === 'guardia') look.holdR = [[0.25, 0.12, 0.25, '#c0392b', 0, -0.72, 0.1]];
      else look.holdL = [[0.28, 0.36, 0.04, '#9a6b3f', 0, -0.62, 0.15]];
      this.h = makeHuman(look);
      this.bar = makeBar(0.9, '#ff4b3a');
      this.bar.position.y = 2.45;
      this.h.root.add(this.bar);
      this.mesh = this.h.root;
      this.mesh.position.set(x, 0, z);
      this.radius = 0.45;
      this.headY = 1.79;
    }

    get dead() { return this.hp <= 0; }

    update(dt, G) {
      const p = G.player;
      if (this.dead) {
        this.ko += dt;
        this.h.body.rotation.x = Math.min(Math.PI / 2, this.ko * 4);
        this.mesh.position.y = W.World.baseY(this.pos.x, this.pos.z) - Math.max(0, this.ko - 1.2) * 1.5;
        return this.ko > 2;
      }
      const dx = p.pos.x - this.pos.x, dz = p.pos.z - this.pos.z;
      const dist = Math.hypot(dx, dz);
      if (!this.alerted && dist < 32 && !p.dead) {
        this.alerted = true;
        G.say(this, W.ENEMY_LINES[Math.floor(Math.random() * W.ENEMY_LINES.length)]);
      }
      if (this.alerted && (dist > 90 || p.dead)) this.alerted = false;

      let mx = 0, mz = 0, face = null;
      if (this.alerted) {
        face = Math.atan2(dx, dz);
        if (this.kind === 'inspector') {
          if (dist > 1.3) { mx = dx / dist; mz = dz / dist; }
          this.cool -= dt;
          if (dist < 1.6 && this.cool <= 0) {
            this.cool = 1.2;
            G.damagePlayer(this.t.melee, this);
          }
        } else {
          const want = this.t.range * 0.6;
          if (dist > want + 4) { mx = dx / dist; mz = dz / dist; }
          else if (dist < want - 6) { mx = -dx / dist; mz = -dz / dist; }
          else { mx = (-dz / dist) * this.strafe * 0.6; mz = (dx / dist) * this.strafe * 0.6; }
          if (Math.random() < dt * 0.3) this.strafe *= -1;
          this.cool -= dt;
          if (this.cool <= 0 && dist < this.t.range && G.lineOfSight(this.pos, 1.5, p.pos, 1.3)) {
            this.cool = this.t.cooldown * (0.8 + Math.random() * 0.4);
            G.throwStamp(this, p, this.t.ranged);
          }
        }
      } else {
        if (!this.goal || Math.hypot(this.goal.x - this.pos.x, this.goal.z - this.pos.z) < 1) {
          this.goal = { x: this.home.x + (Math.random() - 0.5) * 30, z: this.home.z + (Math.random() - 0.5) * 30 };
        }
        const gx = this.goal.x - this.pos.x, gz = this.goal.z - this.pos.z;
        const gd = Math.hypot(gx, gz) || 1;
        mx = (gx / gd) * 0.45; mz = (gz / gd) * 0.45;
      }
      steer(this, mx, mz, this.speed, dt);
      const moving = Math.hypot(mx, mz);
      if (moving > 0.05) {
        this.phase += dt * 10 * moving;
        if (face === null) face = Math.atan2(mx, mz);
      }
      if (face !== null) this.mesh.rotation.y = lerpAngle(this.mesh.rotation.y, face, Math.min(1, dt * 10));
      this.h.animate(this.phase, Math.min(1, moving * 1.5), this.kind === 'guardia' && this.alerted);
      this.mesh.position.set(this.pos.x, W.World.baseY(this.pos.x, this.pos.z), this.pos.z);
      setBar(this.bar, this.hp / this.maxHp);
      this.bar.visible = this.hp < this.maxHp || this.alerted;
      return false;
    }

    hit(dmg) {
      this.hp -= dmg;
      this.alerted = true;
      if (this.dead) this.bar.visible = false;
    }
  }

  class Boss extends Enemy {
    constructor(x, z) {
      super('guardia', x, z, { alerted: true });
      this.name = W.BOSS.name;
      this.boss = true;
      this.maxHp = this.hp = W.BOSS.hp;
      this.speed = 2.6;
      this.h = makeHuman({ shirt: '#2f3136', pants: '#18191c', hat: 'tophat', sash: '#c0392b', badge: true, mustache: true, scale: 2.6, skin: '#d9a273' });
      this.mesh = this.h.root;
      this.mesh.position.set(x, 0, z);
      this.bar = makeBar(0.01, '#ff4b3a');
      this.radius = 1.2;
      this.headY = 1.79 * 2.6;
      this.volley = 2.5;
      this.summon = 8;
      this.slam = 0;
    }
    update(dt, G) {
      const p = G.player;
      if (this.dead) {
        this.ko += dt;
        this.h.body.rotation.x = Math.min(Math.PI / 2, this.ko * 2);
        return this.ko > 4;
      }
      const dx = p.pos.x - this.pos.x, dz = p.pos.z - this.pos.z;
      const dist = Math.hypot(dx, dz) || 1;
      const face = Math.atan2(dx, dz);
      this.mesh.rotation.y = lerpAngle(this.mesh.rotation.y, face, Math.min(1, dt * 4));
      const rage = this.hp < this.maxHp * 0.5 ? 1.35 : 1;
      if (dist > 8) {
        steer(this, dx / dist, dz / dist, this.speed * rage, dt);
        this.phase += dt * 5;
      }
      this.mesh.position.set(this.pos.x, W.World.baseY(this.pos.x, this.pos.z), this.pos.z);
      this.h.animate(this.phase, dist > 8 ? 0.8 : 0, true);
      // ráfaga de sellos en abanico
      this.volley -= dt * rage;
      if (this.volley <= 0 && !p.dead) {
        this.volley = 2.6;
        const n = rage > 1 ? 7 : 5;
        for (let i = 0; i < n; i++) G.throwStamp(this, p, 12, (i - (n - 1) / 2) * 0.12, 4.2);
        G.say(this, ['¡Sellado y decomisado!', '¡Formulario rechazado!', '¡Nadie compite conmigo!'][Math.floor(Math.random() * 3)]);
      }
      // llama refuerzos
      this.summon -= dt;
      if (this.summon <= 0) {
        this.summon = rage > 1 ? 11 : 15;
        G.summonAround(this.pos, 3);
      }
      // pisotón si estás cerca
      this.slam -= dt;
      if (dist < 4.5 && this.slam <= 0) {
        this.slam = 2;
        G.damagePlayer(25, this, { knock: 14 });
        G.shake(0.5);
      }
      return false;
    }
  }

  // Avanza esquivando obstáculos: si se atasca, se desvía de lado un momento
  function steer(ent, mx, mz, speed, dt) {
    if (ent.avoid > 0) {
      ent.avoid -= dt;
      const px = -mz * ent.avoidDir, pz = mx * ent.avoidDir;
      mx = mx * 0.3 + px; mz = mz * 0.3 + pz;
    }
    const x0 = ent.pos.x, z0 = ent.pos.z;
    const want = Math.hypot(mx, mz) * speed * dt;
    ent.pos.x += mx * speed * dt;
    ent.pos.z += mz * speed * dt;
    W.World.collide(ent.pos, ent.radius, 0);
    const moved = Math.hypot(ent.pos.x - x0, ent.pos.z - z0);
    if (want > 0.01 && moved < want * 0.3 && !(ent.avoid > 0)) {
      ent.avoid = 0.8 + Math.random() * 0.6;
      ent.avoidDir = Math.random() < 0.5 ? 1 : -1;
    }
  }

  function lerpAngle(a, b, t) {
    let d = ((b - a + Math.PI) % (Math.PI * 2)) - Math.PI;
    if (d < -Math.PI) d += Math.PI * 2;
    return a + d * t;
  }

  // ---------- Peatones / clientes ----------
  const PED_STYLES = [
    { shirt: '#5b8def', pants: '#2d3a5a', hair: '#2b1b12' },
    { shirt: '#e8e8e8', pants: '#3b3b46', hair: '#4a3020', tie: '#2c5aa0' },
    { shirt: '#f4c542', pants: '#6b8e4e', hair: '#1a1a1a' },
    { shirt: '#7a5c99', pants: '#222', hair: '#111' },
    { shirt: '#e85d75', pants: '#333', hair: '#5a3a20' },
    { shirt: '#3f9e8a', pants: '#2d2d2d', hair: '#2b1b12' },
    { shirt: '#8a3b2a', pants: '#3a3020', hair: '#1a1a1a' }, // ruana
  ];
  const umbrellaGeos = new Map();
  const umbrellaCone = new THREE.ConeGeometry(0.75, 0.3, 8);
  function umbrellaGeo(color) {
    if (!umbrellaGeos.has(color)) {
      umbrellaGeos.set(color, W.Geom.merge([
        { geo: umbrellaCone, matrix: W.Geom.matrix(0, 2.35, 0), color },
        { geo: boxGeo, matrix: W.Geom.matrix(0, 1.9, 0, 0, 0, 0, 0.03, 0.9, 0.03), color: '#333' },
      ]));
    }
    return umbrellaGeos.get(color);
  }
  class Pedestrian {
    constructor(spot) {
      const st = PED_STYLES[Math.floor(Math.random() * PED_STYLES.length)];
      this.h = makeHuman(Object.assign({ skin: ['#d9a273', '#a8714a', '#e8c19a', '#8a5a3a'][Math.floor(Math.random() * 4)] }, st));
      this.mesh = this.h.root;
      this.pos = { x: spot.x, z: spot.z };
      this.cell = { row: spot.row, col: spot.col };
      this.phase = Math.random() * 6;
      this.speed = 1.3 + Math.random() * 0.6;
      this.wants = false;
      this.wantTimer = 5 + Math.random() * 20;
      this.icon = iconSprite('☕', 0.55);
      this.icon.position.y = 2.4;
      this.icon.visible = false;
      this.mesh.add(this.icon);
      // sombrilla para la lluvia bogotana (una sola malla por color)
      const umb = new THREE.Mesh(umbrellaGeo(['#111', '#2c5aa0', '#c0392b', '#2fbf71', '#f2a541'][Math.floor(Math.random() * 5)]), W.Geom.vcMat());
      umb.position.set(0.25, 0, 0.1);
      umb.visible = false;
      this.umbrella = umb;
      this.mesh.add(umb);
      this.pickGoal();
      this.mesh.position.set(this.pos.x, 0, this.pos.z);
    }
    pickGoal() {
      const { x, z } = W.World.cellCenter(this.cell.row, this.cell.col);
      const s = C.BLOCK / 2 - 0.9;
      const corners = [[x - s, z - s], [x + s, z - s], [x + s, z + s], [x - s, z + s]];
      // a veces cruza la calle hacia la manzana vecina
      if (Math.random() < 0.2) {
        const dirs = [[0, 1], [0, -1], [1, 0], [-1, 0]];
        const [dr, dc] = dirs[Math.floor(Math.random() * 4)];
        const nr = this.cell.row + dr, nc = this.cell.col + dc;
        if (nr >= 0 && nc >= 0 && nr < C.GRID && nc < C.GRID && W.DISTRICT_MAP[nr][nc] !== 'P') this.cell = { row: nr, col: nc };
      }
      const c2 = W.World.cellCenter(this.cell.row, this.cell.col);
      const k = corners[Math.floor(Math.random() * 4)];
      this.goal = { x: k[0] - x + c2.x, z: k[1] - z + c2.z };
    }
    update(dt, G) {
      const inMarket = G.isMarket(this.pos.x, this.pos.z);
      this.icon.visible = this.wants && inMarket;
      this.umbrella.visible = G.raining;
      if (this.stop > 0) { this.stop -= dt; this.h.animate(0, 0); return; }
      const gx = this.goal.x - this.pos.x, gz = this.goal.z - this.pos.z;
      const d = Math.hypot(gx, gz);
      if (d < 0.5) this.pickGoal();
      else {
        this.pos.x += (gx / d) * this.speed * dt;
        this.pos.z += (gz / d) * this.speed * dt;
        this.mesh.rotation.y = lerpAngle(this.mesh.rotation.y, Math.atan2(gx, gz), Math.min(1, dt * 6));
        this.phase += dt * 7;
      }
      this.wantTimer -= dt;
      if (this.wantTimer <= 0) {
        this.wants = !this.wants;
        this.wantTimer = this.wants ? 25 + Math.random() * 20 : 10 + Math.random() * 25;
      }
      this.icon.position.y = 2.4 + Math.sin(G.time * 3 + this.phase) * 0.08;
      this.h.animate(this.phase, 0.6);
      this.mesh.position.set(this.pos.x, W.World.baseY(this.pos.x, this.pos.z), this.pos.z);
    }
  }

  // ---------- Cultivo hidropónico flotante ----------
  const planterGeo = (() => {
    const M = W.Geom.matrix;
    const leafGeo = new THREE.IcosahedronGeometry(0.45, 0);
    const cg = new THREE.SphereGeometry(0.09, 6, 5);
    const plant = [{ geo: boxGeo, matrix: M(0, 0.5, 0, 0, 0, 0, 0.08, 1, 0.08), color: '#2a7a41' }];
    for (let i = 0; i < 4; i++) plant.push({ geo: leafGeo, matrix: M(Math.cos(i * 1.6) * 0.3, 0.6 + i * 0.15, Math.sin(i * 1.6) * 0.3), color: i % 2 ? '#3fa35b' : '#2a7a41' });
    const cherries = [];
    for (let i = 0; i < 9; i++) cherries.push({ geo: cg, matrix: M(Math.cos(i * 2.1) * 0.45, 0.55 + (i % 3) * 0.22, Math.sin(i * 2.1) * 0.45), color: '#d6423a' });
    return {
      base: W.Geom.merge([
        { geo: new THREE.CylinderGeometry(1.1, 1.1, 0.25, 14), matrix: M(0, 0.12, 0), color: '#f2f2ee' },
        { geo: new THREE.CylinderGeometry(0.35, 0.3, 0.3, 10), matrix: M(0, 0.3, 0), color: '#2d2d2d' },
      ]),
      plant: W.Geom.merge(plant),
      cherries: W.Geom.merge(cherries),
    };
  })();
  class Planter {
    constructor(spot) {
      this.pos = { x: spot.x, z: spot.z };
      this.mesh = new THREE.Group();
      // base, planta y cerezas: tres mallas fusionadas (antes eran 16 piezas sueltas)
      this.mesh.add(new THREE.Mesh(planterGeo.base, W.Geom.vcMat()));
      this.plant = new THREE.Group();
      this.plant.position.y = 0.4;
      this.plant.add(new THREE.Mesh(planterGeo.plant, W.Geom.vcMat({ flatShading: true })));
      this.cherries = new THREE.Mesh(planterGeo.cherries, W.Geom.vcMat({ emissive: '#3a0805' }));
      this.plant.add(this.cherries);
      this.mesh.add(this.plant);
      this.mesh.position.set(spot.x, spot.y || (spot.water ? 0.3 : 0.3), spot.z);
      this.growth = Math.random() * 0.6;
      this.bob = Math.random() * 6;
      this.water = spot.water;
      this.baseY = this.mesh.position.y;
    }
    get ripe() { return this.growth >= 1; }
    update(dt, G, rate) {
      if (this.growth < 1) this.growth = Math.min(1, this.growth + (dt / C.REGROW_TIME) * rate);
      const s = 0.35 + Math.min(1, this.growth) * 0.65;
      this.plant.scale.setScalar(s);
      this.cherries.visible = this.ripe;
      if (this.water) this.mesh.position.y = this.baseY + Math.sin(G.time * 1.5 + this.bob) * 0.05;
    }
    harvest() { this.growth = 0; }
  }

  // ---------- Botín (cajas de café y cerezas sueltas) ----------
  class Loot {
    constructor(x, z, amount, kind) {
      this.pos = { x, z };
      this.amount = amount;
      this.kind = kind || 'crate';
      this.mesh = new THREE.Group();
      if (this.kind === 'crate') {
        this.mesh.add(box(0.8, 0.6, 0.8, '#8b5a2b', 0, 0.3, 0));
        this.mesh.add(box(0.82, 0.12, 0.82, '#2fbf71', 0, 0.45, 0));
      } else {
        const orb = new THREE.Mesh(new THREE.SphereGeometry(0.22, 8, 6), mat('#e0413a', { emissive: '#5a0a05' }));
        orb.position.y = 0.4;
        this.mesh.add(orb);
      }
      const ring = new THREE.Mesh(new THREE.RingGeometry(0.7, 0.9, 20), new THREE.MeshBasicMaterial({ color: this.kind === 'crate' ? '#ffd34d' : '#ff6b5b', transparent: true, opacity: 0.7, side: THREE.DoubleSide }));
      ring.rotation.x = -Math.PI / 2;
      ring.position.y = 0.05;
      this.ring = ring;
      this.mesh.add(ring);
      this.mesh.position.set(x, 0.2, z);
      this.t = Math.random() * 6;
    }
    update(dt) {
      this.t += dt;
      this.mesh.rotation.y += dt;
      this.mesh.children[0].position.y = (this.kind === 'crate' ? 0.3 : 0.4) + Math.sin(this.t * 3) * 0.1;
      this.ring.material.opacity = 0.4 + Math.sin(this.t * 4) * 0.3;
    }
  }

  // ---------- Zonas de captura ----------
  const ZONE_COLORS = { fed: '#e0413a', fg: '#2fbf71', target: '#ffd34d' };
  class Zone {
    constructor(def) {
      this.def = def;
      this.id = def.id;
      this.name = def.name;
      this.pos = { x: def.center.x, z: def.center.z };
      this.progress = 0;
      this.owner = 'fed';
      this.mesh = new THREE.Group();
      this.canopyMat = new THREE.MeshLambertMaterial({ color: ZONE_COLORS.fed });
      const canopy = new THREE.Mesh(boxGeo, this.canopyMat);
      canopy.scale.set(3.8, 0.25, 2.4);
      canopy.position.y = 2.6;
      // mostrador, postes y asta de la bandera en una sola malla
      const stall = [[3, 1.1, 1.4, '#5a3a24', 0, 0.55, 0], [0.12, 7, 0.12, '#ddd', 2.6, 3.5, 0]];
      for (const [px, pz] of [[-1.8, -1.1], [1.8, -1.1], [-1.8, 1.1], [1.8, 1.1]]) stall.push([0.1, 2.5, 0.1, '#ccc', px, 1.3, pz]);
      this.mesh.add(parts(stall), canopy);
      this.flagMat = new THREE.MeshLambertMaterial({ color: ZONE_COLORS.fed, side: THREE.DoubleSide });
      this.flag = new THREE.Mesh(new THREE.PlaneGeometry(1.8, 1.1), this.flagMat);
      this.flag.position.set(3.5, 6.3, 0);
      this.mesh.add(this.flag);
      // anillo en el piso y columna de luz visible desde lejos
      this.ringMat = new THREE.MeshBasicMaterial({ color: ZONE_COLORS.fed, transparent: true, opacity: 0.5, side: THREE.DoubleSide });
      const ring = new THREE.Mesh(new THREE.RingGeometry(C.CAPTURE_RADIUS - 0.5, C.CAPTURE_RADIUS, 48), this.ringMat);
      ring.rotation.x = -Math.PI / 2;
      ring.position.y = 0.35;
      this.mesh.add(ring);
      this.beamMat = new THREE.MeshBasicMaterial({ color: ZONE_COLORS.fed, transparent: true, opacity: 0.18, depthWrite: false });
      const beam = new THREE.Mesh(new THREE.CylinderGeometry(1.2, 1.2, 120, 12, 1, true), this.beamMat);
      beam.position.y = 60;
      this.mesh.add(beam);
      this.mesh.position.set(this.pos.x, 0.3, this.pos.z);
    }
    setOwner(owner, isTarget) {
      this.owner = owner;
      const color = owner === 'fg' ? ZONE_COLORS.fg : isTarget ? ZONE_COLORS.target : ZONE_COLORS.fed;
      this.canopyMat.color.set(owner === 'fg' ? ZONE_COLORS.fg : ZONE_COLORS.fed);
      this.flagMat.color.set(owner === 'fg' ? ZONE_COLORS.fg : ZONE_COLORS.fed);
      this.ringMat.color.set(color);
      this.beamMat.color.set(color);
      this.beamMat.opacity = isTarget ? 0.3 : 0.15;
    }
    update(dt, t) {
      this.flag.rotation.y = Math.sin(t * 2 + this.pos.x) * 0.25;
      this.ringMat.opacity = 0.35 + Math.sin(t * 3) * 0.15;
    }
  }

  // ---------- Proyectiles ----------
  const tracerGeo = new THREE.CapsuleGeometry ? new THREE.CapsuleGeometry(0.06, 0.5, 2, 6) : new THREE.CylinderGeometry(0.06, 0.06, 0.6, 6);
  const tracerMat = new THREE.MeshBasicMaterial({ color: '#7a4422' });
  function makeTracer() {
    const m = new THREE.Mesh(tracerGeo, tracerMat);
    return m;
  }
  const stampMat = new THREE.MeshLambertMaterial({ color: '#c0392b', emissive: '#4a0b06' });
  const stampGeo = new THREE.CylinderGeometry(0.22, 0.22, 0.12, 12);
  function makeStamp() {
    return new THREE.Mesh(stampGeo, stampMat);
  }
  // Partículas: todas en una sola malla instanciada (una llamada de dibujo para hasta `max` cubitos)
  const partGeo = new THREE.BoxGeometry(0.12, 0.12, 0.12);
  class Particles {
    constructor(max) {
      this.mesh = new THREE.InstancedMesh(partGeo, new THREE.MeshLambertMaterial(), max);
      this.mesh.instanceMatrix.setUsage(THREE.DynamicDrawUsage);
      this.mesh.frustumCulled = false;
      const white = new THREE.Color('#ffffff');
      for (let i = 0; i < max; i++) this.mesh.setColorAt(i, white);
      this.mesh.count = 0;
      this.max = max;
      this.colors = new Map();
      this.m = new THREE.Matrix4();
    }
    color(hex) {
      if (!this.colors.has(hex)) this.colors.set(hex, new THREE.Color(hex));
      return this.colors.get(hex);
    }
    // list: [{ x, y, z, color }]
    sync(list) {
      const n = Math.min(list.length, this.max);
      for (let i = 0; i < n; i++) {
        const p = list[i];
        this.mesh.setMatrixAt(i, this.m.makeTranslation(p.x, p.y, p.z));
        this.mesh.setColorAt(i, this.color(p.color));
      }
      this.mesh.count = n;
      this.mesh.instanceMatrix.needsUpdate = true;
      this.mesh.instanceColor.needsUpdate = true;
    }
  }

  W.Ent = { makeHuman, makeBarista, Enemy, Boss, Pedestrian, Planter, Loot, Zone, makeTracer, makeStamp, Particles, iconSprite, lerpAngle, makeBar, setBar };
})();
