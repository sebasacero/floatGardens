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

  // AeroPress X-1
  // El rociador: una lanza de riego (tanque de café a presión, caño fino y boquilla verde), no un fusil.
  function makeAeroPress() {
    const g = new THREE.Group();
    const tank = new THREE.Mesh(new THREE.CylinderGeometry(0.09, 0.09, 0.34, 10), mat('#f4f5f3'));
    tank.rotation.x = Math.PI / 2;
    tank.position.z = -0.1;
    const coffee = new THREE.Mesh(new THREE.CylinderGeometry(0.075, 0.075, 0.2, 10), mat('#4a2c1d'));
    coffee.rotation.x = Math.PI / 2;
    coffee.position.z = -0.04;
    const lance = new THREE.Mesh(new THREE.CylinderGeometry(0.03, 0.03, 0.9, 8), mat('#2c302e'));
    lance.rotation.x = Math.PI / 2;
    lance.position.z = 0.5;
    const nozzle = new THREE.Mesh(new THREE.ConeGeometry(0.06, 0.12, 10), mat('#4f8f2c'));
    nozzle.rotation.x = Math.PI / 2;
    nozzle.position.z = 0.98;
    g.add(tank, coffee, lance, nozzle);
    g.userData.muzzle = new THREE.Object3D();
    g.userData.muzzle.position.z = 1.02;
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
    if (o.hat === 'tophat') b.push([0.62, 0.04, 0.62, '#111', 0, 1.99, 0], [0.38, 0.5, 0.38, '#111', 0, 2.25, 0], [0.39, 0.07, 0.39, '#2a22f5', 0, 2.05, 0]);
    if (o.sash) b.push([0.08, 0.85, 0.36, o.sash, 0, 1.22, 0, 0.6]);
    if (o.badge) b.push([0.1, 0.1, 0.03, '#e6c34a', 0.17, 1.4, 0.18]);
    if (o.helmet) {
      // casco blanco con visor oscuro y la hoja verde de la red encima (vista cenital)
      b.push([0.42, 0.44, 0.42, o.helmet, 0, 1.8, 0], [0.3, 0.14, 0.06, '#1a1f22', 0, 1.82, 0.2], [0.14, 0.03, 0.22, '#8fbf45', 0, 2.03, -0.02]);
      b.push([0.7, 0.14, 0.4, o.helmet, 0, 1.6, 0]); // hombreras
    }
    if (o.jetpack) {
      // mochila: dos prensas de café (cámara, tapa y émbolo)
      for (const sx of [-0.15, 0.15]) b.push([0.22, 0.56, 0.22, '#3a2618', sx, 1.22, -0.3], [0.26, 0.06, 0.26, '#1f2224', sx, 1.52, -0.3], [0.05, 0.2, 0.05, '#1f2224', sx, 1.64, -0.3], [0.26, 0.05, 0.26, '#1f2224', sx, 0.93, -0.3]);
    }
    const body = parts(b);
    root.add(body);
    body.add(legL, legR, armL, armR);
    if (o.scale) body.scale.setScalar(o.scale);
    root.traverse((m) => { if (m.isMesh) m.castShadow = true; });

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

  // Marcador sobre un peatón que quiere café: baldosa blanca cuadrada con una taza de trazo fino (como los marcadores de la web)
  const iconCache = new Map();
  function iconSprite(text, size) {
    if (!iconCache.has(text)) {
      const c = document.createElement('canvas');
      c.width = c.height = 64;
      const ctx = c.getContext('2d');
      ctx.fillStyle = 'rgba(255,255,255,0.96)';
      ctx.fillRect(6, 6, 52, 52);
      ctx.strokeStyle = '#2c302e'; ctx.lineWidth = 3.2; ctx.lineJoin = 'round';
      ctx.beginPath(); ctx.moveTo(19, 26); ctx.lineTo(41, 26); ctx.lineTo(39, 44); ctx.lineTo(21, 44); ctx.closePath(); ctx.stroke();
      ctx.beginPath(); ctx.arc(43, 33, 5, -Math.PI / 2, Math.PI / 2); ctx.stroke();
      ctx.strokeStyle = '#4f8f2c';
      ctx.beginPath(); ctx.moveTo(26, 20); ctx.lineTo(26, 15); ctx.moveTo(33, 20); ctx.lineTo(33, 13); ctx.stroke();
      iconCache.set(text, new THREE.SpriteMaterial({ map: new THREE.CanvasTexture(c), depthTest: false }));
    }
    const s = new THREE.Sprite(iconCache.get(text));
    s.scale.setScalar(size || 0.6);
    s.renderOrder = 12;
    return s;
  }

  // ---------- Jugador ----------
  // Barista de la red: armadura blanca (maqueta) con mochila de prensas de café. Es el mismo en todas las cámaras.
  function makeBarista() { return makeBaristaBlanco(); }
  function makeBaristaBlanco() {
    const h = makeHuman({ shirt: '#f2f3f1', pants: '#c9ccca', skin: '#f2f3f1', helmet: '#f2f3f1', jetpack: true });
    const gun = makeAeroPress();
    gun.position.set(0, -0.6, 0.18);
    gun.rotation.x = Math.PI / 2;
    h.armR.add(gun);
    h.gun = gun;
    return h;
  }

  // Dron de Arcadia (vista cenital): caja azul con cuatro rotores; flota a la altura del torso
  // para que las esferas de impacto del enemigo sigan funcionando igual.
  function makeDrone(kind) {
    const root = new THREE.Group();
    const blue = '#2a22f5';
    const b = [
      [0.95, 0.32, 0.95, blue, 0, 1.3, 0], [0.4, 0.12, 0.4, '#e9ecea', 0, 1.52, 0],
      [0.14, 0.14, 0.06, '#ffffff', 0, 1.22, 0.49], // ojo
    ];
    for (const [dx, dz] of [[1, 1], [1, -1], [-1, 1], [-1, -1]]) b.push([0.55, 0.06, 0.08, '#2c302e', dx * 0.38, 1.35, dz * 0.38, 0, dx * dz * Math.PI / 4]);
    if (kind === 'guardia') b.push([0.3, 0.1, 0.5, '#1b14c0', 0, 1.1, 0.2], [0.08, 0.3, 0.08, '#2c302e', 0, 1.0, 0.45]); // escáner y sello
    else b.push([0.5, 0.08, 0.3, '#1b14c0', 0, 1.1, 0]);
    // la matriz usa [w,h,d,color,x,y,z,rz]; para la rotación en y de los brazos reusamos el campo rz como ry
    const body = new THREE.Mesh(W.Geom.merge(b.map(([w, h, d, color, x, y, z, ry]) => ({ geo: boxGeo, matrix: W.Geom.matrix(x, y, z, 0, ry || 0, 0, w, h, d), color }))), W.Geom.vcMat({ emissive: '#1b14c0', emissiveIntensity: 0.3 }));
    root.add(body);
    // cono de luz azul del escáner sobre el piso
    const cone = new THREE.Mesh(new THREE.CircleGeometry(1.4, 28), new THREE.MeshBasicMaterial({ color: '#4a44ff', transparent: true, opacity: 0.14, depthWrite: false }));
    cone.rotation.x = -Math.PI / 2;
    cone.position.y = 0.04;
    root.add(cone);
    const rotors = [];
    for (const [dx, dz] of [[1, 1], [1, -1], [-1, 1], [-1, -1]]) {
      const r = box(0.56, 0.02, 0.1, '#aab0ff', dx * 0.62, 1.4, dz * 0.62);
      body.add(r);
      rotors.push(r);
    }
    const noop = new THREE.Group();
    return {
      root, body, cone, legL: noop, legR: noop, armL: noop, armR: noop, drone: true,
      animate(phase, amount, aiming) {
        body.position.y = Math.sin(phase * 0.35) * 0.08;
        for (let i = 0; i < 4; i++) rotors[i].rotation.y = phase * 3 + i;
        body.rotation.z = aiming ? 0 : Math.sin(phase * 0.5) * 0.04;
      },
    };
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
      this.serial = `AR-${2080 + Math.floor(Math.random() * 19)}`;
      this.buildMesh();
      this.mesh.position.set(x, 0, z);
      this.radius = 0.45;
      this.headY = 1.79;
    }

    // Aspecto: dron de Arcadia (escáner o sellador).
    buildMesh() {
      const t = this.t, kind = this.kind;
      // guía v2: la amenaza es Arcadia y es azul, en cualquier cámara
      this.h = makeDrone(kind);
      this.name = `${t.name} ${this.serial}`;
      this.bar = makeBar(0.9, '#2a22f5');
      this.bar.position.y = 2.45;
      this.h.root.add(this.bar);
      this.mesh = this.h.root;
      this.h.body.castShadow = true;
    }
    get dead() { return this.hp <= 0; }

    update(dt, G) {
      const p = G.player;
      if (this.dead) {
        // el dron pierde sustentación: gira, cae y se apaga
        this.ko += dt;
        const b = this.h.body;
        b.rotation.z += dt * (9 - Math.min(8, this.ko * 6));
        b.rotation.x = Math.min(0.9, this.ko * 2.2);
        b.position.y = Math.max(-0.95, b.position.y - dt * (1 + this.ko * 9));
        if (this.h.cone) this.h.cone.visible = false;
        this.mesh.position.y = W.World.baseY(this.pos.x, this.pos.z) - Math.max(0, this.ko - 1.3) * 1.5;
        return this.ko > 2;
      }
      // golpe: el cuerpo se hincha un instante
      if (this.flash > 0) { this.flash = Math.max(0, this.flash - dt); this.h.body.scale.setScalar(1 + this.flash * 2.4); }
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
      this.flash = 0.12;
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
      this.h = makeHuman({ shirt: '#2f3136', pants: '#18191c', hat: 'tophat', sash: '#2a22f5', badge: true, mustache: true, scale: 2.6, skin: '#d9a273' });
      this.mesh = this.h.root;
      this.mesh.position.set(x, 0, z);
      this.bar = makeBar(0.01, '#2a22f5');
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
        G.say(this, ['Sellado y decomisado', 'Licencia rechazada', 'Nadie compite con Arcadia'][Math.floor(Math.random() * 3)]);
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
      this.icon = iconSprite('taza', 0.6);
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
    // mata de café frondosa (guía v2): hojas en cuatro verdes, brotes lima y cerezas rojas/pintonas/amarillas
    // pocas caras: con sombreado plano se ve igual y cada mata pesa la tercera parte
    const leafGeo = new THREE.SphereGeometry(0.5, 6, 3);
    leafGeo.scale(1, 0.32, 0.55);
    const cg = new THREE.SphereGeometry(0.075, 5, 3);
    const cgLo = new THREE.SphereGeometry(0.085, 4, 2);
    const LEAF = ['#1f5f27', '#2c7a2f', '#357f30', '#24682a'];
    let seed = 11;
    const rnd = () => (seed = (seed * 16807) % 2147483647) / 2147483647;
    const plant = [{ geo: new THREE.CylinderGeometry(0.05, 0.07, 0.9, 6), matrix: M(0, 0.45, 0), color: '#6b4a2e' }];
    for (let i = 0; i < 24; i++) {
      const a = rnd() * 6.28, h = 0.35 + rnd() * 0.85, r = 0.25 + rnd() * 0.5 * (1.15 - h * 0.4);
      plant.push({ geo: leafGeo, matrix: M(Math.cos(a) * r, h, Math.sin(a) * r, rnd() * 0.6 - 0.3, -a, rnd() * 0.5, 0.7 + rnd() * 0.5), color: LEAF[i % 4] });
    }
    for (let i = 0; i < 4; i++) { const a = rnd() * 6.28; plant.push({ geo: leafGeo, matrix: M(Math.cos(a) * 0.15, 1.25 + rnd() * 0.12, Math.sin(a) * 0.15, 0, -a, 0, 0.45), color: '#8cc84a' }); }
    const cherries = [];
    const CH = ['#b3121a', '#b3121a', '#e0471c', '#e8a31f'];
    for (let i = 0; i < 16; i++) {
      const a = rnd() * 6.28, h = 0.4 + rnd() * 0.6, r = 0.18 + rnd() * 0.25, c = CH[i % 4];
      for (let k = 0; k < 3; k++) cherries.push({ geo: cg, matrix: M(Math.cos(a) * r + k * 0.05, h + k * 0.03, Math.sin(a) * r), color: c, ci: i % 4 });
    }
    // pústulas de roya: polvo naranja sobre las hojas (se muestran cuando la mata está infectada)
    const spotGeo = new THREE.SphereGeometry(0.15, 5, 3);
    const spots = [];
    for (let i = 0; i < 26; i++) {
      const a = rnd() * 6.28, h = 0.4 + rnd() * 0.8, r = 0.3 + rnd() * 0.42 * (1.15 - h * 0.4);
      spots.push({ geo: spotGeo, matrix: M(Math.cos(a) * r, h + 0.1, Math.sin(a) * r, 0, 0, 0, 0.7 + rnd() * 0.8), color: i % 3 ? '#d9601a' : '#e8791c' });
    }
    // variante liviana para las jardineras de las torres (muchas matas por torre): 9 hojas y 5 racimos
    // (se ven de lejos: sin tronco, 7 hojas, un brote y 4 racimos)
    const lo = [];
    for (let i = 0; i < 7; i++) lo.push(plant[1 + i * 3]);
    lo.push(plant[25]);
    const loCh = [];
    for (let i = 0; i < 4; i++) loCh.push({ geo: cgLo, matrix: cherries[i * 9].matrix, color: cherries[i * 9].color });
    // tramo de una fila de cultivo: 2 x 3 matas que se cosechan juntas (las filas A, B y C de la terraza)
    const full = W.Geom.merge(plant), fullCh = W.Geom.merge(cherries);
    const seg = [], segCh = [], segM = [];
    for (let i = 0; i < 3; i++) for (let j = 0; j < 2; j++) {
      const m = M((j - 0.5) * 1.5 + (rnd() - 0.5) * 0.2, 0, (i - 1) * 1.5 + (rnd() - 0.5) * 0.2, 0, rnd() * 6.28, 0, 0.95 + rnd() * 0.2);
      seg.push({ geo: full, matrix: m }); segCh.push({ geo: fullCh, matrix: m }); segM.push(m);
    }
    const fullSpots = W.Geom.merge(spots);
    // cerezas con la paleta de cada variedad (rojas, amarillas, vinotinto…); se arman una vez por variedad
    const chCache = {};
    const cherriesFor = (key, pal, bed) => {
      const k = key + (bed ? ':b' : ':p');
      if (!chCache[k]) {
        const one = W.Geom.merge(cherries.map((d) => ({ geo: cg, matrix: d.matrix, color: pal[d.ci] })));
        chCache[k] = bed ? W.Geom.mergeColored(segM.map((m) => ({ geo: one, matrix: m }))) : one;
      }
      return chCache[k];
    };
    return {
      cherriesFor,
      spots: fullSpots,
      segSpots: W.Geom.mergeColored(segM.map((m) => ({ geo: fullSpots, matrix: m }))),
      seg: W.Geom.mergeColored(seg),
      segCherries: W.Geom.mergeColored(segCh),
      plantLo: W.Geom.merge(lo),
      cherriesLo: W.Geom.merge(loCh),
      base: W.Geom.merge([
        { geo: new THREE.CylinderGeometry(1.1, 1.1, 0.25, 14), matrix: M(0, 0.12, 0), color: '#f2f2ee' },
        { geo: new THREE.CylinderGeometry(0.35, 0.3, 0.3, 10), matrix: M(0, 0.3, 0), color: '#2d2d2d' },
      ]),
      plant: full,
      cherries: fullCh,
    };
  })();
  const royaMat = new THREE.MeshLambertMaterial({ vertexColors: true, emissive: '#5a2000', emissiveIntensity: 0.5, flatShading: true });
  const LEAF_SICK = new THREE.Color('#ffc861');
  class Planter {
    constructor(spot) {
      this.pos = { x: spot.x, z: spot.z };
      this.mesh = new THREE.Group();
      this.bed = !!spot.bed;
      this.label = spot.label || '';
      this.row = spot.row || '';
      this.group = spot.group || 'hq';
      this.reach = this.bed ? 3.5 : spot.water ? 4 : 3.3;
      // base, planta y cerezas: tres mallas fusionadas (antes eran 16 piezas sueltas)
      if (!this.bed) this.mesh.add(new THREE.Mesh(planterGeo.base, W.Geom.vcMat()));
      this.plant = new THREE.Group();
      this.plant.position.y = this.bed ? 0 : 0.4;
      // el follaje tiene material propio: se tiñe según la variedad y amarillea con la roya
      this.leafMat = new THREE.MeshLambertMaterial({ vertexColors: true, flatShading: true });
      this.plant.add(new THREE.Mesh(this.bed ? planterGeo.seg : planterGeo.plant, this.leafMat));
      this.cherries = new THREE.Mesh(planterGeo.cherries, W.Geom.vcMat({ emissive: '#3a0805' }));
      this.plant.add(this.cherries);
      this.spots = new THREE.Mesh(this.bed ? planterGeo.segSpots : planterGeo.spots, royaMat);
      this.spots.visible = false;
      this.plant.add(this.spots);
      this.mesh.add(this.plant);
      this.mesh.traverse((o) => { if (o.isMesh) o.castShadow = true; });
      this.spots.castShadow = false;
      this.mesh.position.set(spot.x, spot.y || (spot.water ? 0.3 : 0.3), spot.z);
      this.growth = Math.random() * 0.6;
      this.bob = Math.random() * 6;
      this.water = spot.water;
      this.baseY = this.mesh.position.y;
      this.roya = 0;   // 0 sana · 1 roya total (se cae la hoja)
      this.leaf = 1;   // follaje: lo recortan las hormigas arrieras y la roya
      this.spore = 0;
      this.setVariety(spot.variety || 'caturra');
    }
    setVariety(id) {
      this.varietyId = W.VARIEDADES[id] ? id : 'caturra';
      this.variety = W.VARIEDADES[this.varietyId];
      this.cherries.geometry = planterGeo.cherriesFor(this.varietyId, this.variety.cherry, this.bed);
      this.leafBase = new THREE.Color(this.variety.leaf || '#ffffff');
      this.leafMat.color.copy(this.leafBase);
    }
    replant(id) { this.setVariety(id); this.growth = 0; this.roya = 0; this.leaf = 1; this.pop = 1; }
    get ripe() { return this.growth >= 1; }
    get sick() { return this.roya > 0; }
    // La roya llega a la mata: las variedades resistentes (Castillo, Tabi) casi siempre la rechazan.
    infect() {
      if (this.roya > 0) return 'ya';
      if (Math.random() < this.variety.roya) return 'resiste';
      this.roya = 0.04;
      return 'infectada';
    }
    cure() { this.roya = 0; this.spore = 0; }
    update(dt, G, rate) {
      const v = this.variety, P = W.PLAGAS;
      if (this.roya > 0) {
        this.roya = Math.min(1, this.roya + dt / P.roya.time);
        if (this.roya >= 1) {
          // roya total: se cae la hoja y la cosecha que estaba lista se pierde
          this.leaf = Math.max(0.35, this.leaf - dt * 0.05);
          this.growth = Math.max(0.3, this.growth - dt * 0.04);
        }
      } else if (this.leaf < 1) this.leaf = Math.min(1, this.leaf + dt * 0.02);
      const r = rate * v.grow * (0.3 + 0.7 * this.leaf) * (1 - 0.85 * this.roya);
      if (this.growth < 1) this.growth = Math.min(1, this.growth + (dt / C.REGROW_TIME) * r);
      const g = Math.min(1, this.growth);
      // recién cosechada rebota un momento (pop) y luego vuelve a crecer
      this.pop = Math.max(0, (this.pop || 0) - dt * 3);
      const k = (1 + Math.sin(this.pop * Math.PI) * 0.12) * (0.6 + 0.4 * this.leaf);
      const hy = v.height || 1;
      if (this.bed) this.plant.scale.set((0.66 + g * 0.34) * k, (0.45 + g * 0.55) * k * hy, (0.8 + g * 0.2) * k);
      else this.plant.scale.set((0.35 + g * 0.65) * k, (0.35 + g * 0.65) * k * hy, (0.35 + g * 0.65) * k);
      this.cherries.visible = this.ripe;
      this.spots.visible = this.roya > 0.1;
      // la hoja con roya amarillea: el tinte le quita verde y el brillo propio le suma amarillo
      this.leafMat.color.copy(this.leafBase).lerp(LEAF_SICK, this.roya * 0.8);
      this.leafMat.emissive.setRGB(0.4 * this.roya, 0.25 * this.roya, 0);
      if (this.water && !this.bed) this.mesh.position.y = this.baseY + Math.sin(G.time * 1.5 + this.bob) * 0.05;
    }
    harvest() { this.growth = 0; this.pop = 1; }
  }

  // ---------- Botín (cajas de café y cerezas sueltas) ----------
  class Loot {
    constructor(x, z, amount, kind) {
      this.pos = { x, z };
      this.amount = amount;
      this.kind = kind || 'crate';
      this.mesh = new THREE.Group();
      if (this.kind === 'crate') {
        // costal de café: fique claro con la franja verde de la red
        this.mesh.add(box(1.15, 0.62, 0.8, '#efe8e1', 0, 0.31, 0));
        this.mesh.add(box(1.17, 0.05, 0.82, '#8f8576', 0, 0.5, 0));
        this.mesh.add(box(0.12, 0.03, 0.82, '#4f8f2c', 0.32, 0.625, 0));
      } else {
        const orb = new THREE.Mesh(new THREE.SphereGeometry(0.22, 8, 6), mat('#e0413a', { emissive: '#5a0a05' }));
        orb.position.y = 0.4;
        this.mesh.add(orb);
      }
      const ring = new THREE.Mesh(new THREE.RingGeometry(0.7, 0.9, 20), new THREE.MeshBasicMaterial({ color: '#d6e48a', transparent: true, opacity: 0.7, side: THREE.DoubleSide }));
      this.mesh.children.forEach((c) => { c.castShadow = true; });
      ring.rotation.x = -Math.PI / 2;
      ring.position.y = 0.05;
      this.ring = ring;
      this.mesh.add(ring);
      this.mesh.position.set(x, 0.2, z);
      this.t = Math.random() * 6;
    }
    update(dt) {
      this.t += dt;
      this.mesh.rotation.y += dt * 0.25;
      this.mesh.children[0].position.y = (this.kind === 'crate' ? 0.3 : 0.4) + Math.sin(this.t * 3) * 0.1;
      this.ring.material.opacity = 0.4 + Math.sin(this.t * 4) * 0.3;
    }
  }

  // ---------- Zonas de captura ----------
  // guía v2: Arcadia en azul, lo tuyo en verde hoja, el objetivo en lima
  const ZONE_COLORS = W.CONFIG.MAQUETA ? { fed: '#2a22f5', fg: '#4f8f2c', target: '#d6e48a' } : { fed: '#e0413a', fg: '#2fbf71', target: '#ffd34d' };
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
  const tracerGeoBig = tracerGeo.clone().scale(1.5, 2.2, 1.5);
  function makeTracer() {
    return new THREE.Mesh(tracerGeoBig, tracerMat);
  }
  const stampMat = new THREE.MeshLambertMaterial({ color: '#2a22f5', emissive: '#2a22f5', emissiveIntensity: 0.8 });
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

  W.Ent = { steer, tracerMat, makeHuman, makeBarista, makeBaristaBlanco, makeDrone, planterGeo, Enemy, Boss, Pedestrian, Planter, Loot, Zone, makeTracer, makeStamp, Particles, iconSprite, lerpAngle, makeBar, setBar };
})();
