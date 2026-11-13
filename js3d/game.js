// Bucle principal del mundo abierto: jugador, cámara, combate, zonas, economía y clima.
(function () {
  const C = W.CONFIG;
  const Ent = W.Ent;
  const rand = (a, b) => a + Math.random() * (b - a);

  const G = {
    state: 'loading',
    time: 0,
    enemies: [],
    peds: [],
    loot: [],
    planters: [],
    zones: [],
    tracers: [],
    stamps: [],
    particles: [],
    bubbles: [],
    boss: null,
    raining: false,
    showMap: false,
    shakeAmt: 0,

    init() {
      const canvas = document.getElementById('world');
      const mobile = W.Input.isTouch || /Mobi|Android/i.test(navigator.userAgent);
      this.renderer = new THREE.WebGLRenderer({ canvas, antialias: !mobile, powerPreference: 'high-performance' });
      this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, mobile ? 1.25 : 1.75));
      // sombras reales y exposición de cine (ACES) en las dos cámaras
      this.renderer.shadowMap.enabled = true;
      this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
      this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
      this.renderer.toneMappingExposure = 1.15;
      this.mobile = mobile;
      this.scene = new THREE.Scene();
      this.skyColor = new THREE.Color(C.MAQUETA ? '#e3e9e8' : '#a9bccb');
      this.scene.background = this.skyColor.clone();
      this.scene.fog = new THREE.Fog(this.skyColor.clone(), 40, 430);
      this.camera = new THREE.PerspectiveCamera(70, 1, 0.1, 900);
      this.hemi = C.MAQUETA ? new THREE.HemisphereLight('#eef4ff', '#8f9a88', 0.75) : new THREE.HemisphereLight('#e8f0f7', '#4a4f45', 0.95);
      this.hemiBase = { color: this.hemi.color.clone(), ground: this.hemi.groundColor.clone(), intensity: this.hemi.intensity };
      this.scene.add(this.hemi);
      const sun = new THREE.DirectionalLight('#fff4e0', C.MAQUETA ? 1.0 : 0.55);
      sun.position.set(-120, 200, 80);
      this.scene.add(sun);
      this.scene.add(sun.target);
      sun.shadow.mapSize.set(mobile ? 1024 : 2048, mobile ? 1024 : 2048);
      const sc = sun.shadow.camera;
      sc.left = -34; sc.right = 34; sc.top = 34; sc.bottom = -34; sc.near = 20; sc.far = 260;
      sun.shadow.bias = -0.0006; sun.shadow.normalBias = 0.03; sun.shadow.radius = 4;
      sun.castShadow = true;
      this.sun = sun;
      this.sunBase = sun.intensity;

      W.Tex.init();
      W.World.build(this.scene);
      // World.build congela las matrices de todo lo que ya estaba en la escena; el sol sí se mueve en la cenital
      sun.matrixAutoUpdate = sun.target.matrixAutoUpdate = true;
      this.save = FG.Save.load();
      this.save.world = Object.assign({ captured: [], upgrades: {}, kos: 0, sales: 0, bossDefeated: false }, this.save.world || {});
      this.save.world.rows = Object.assign({}, W.ROWS_DEFAULT, this.save.world.rows || {});

      this.buildRain();
      this.partPool = new Ent.Particles(161);
      this.scene.add(this.partPool.mesh);
      this.createPlayer();
      this.createZones();
      this.createPlanters();
      for (let i = 0; i < 45; i++) this.spawnPed();
      for (let i = 0; i < 18; i++) this.spawnCrate();

      W.Input.init(canvas);
      W.TopDown.load(W.Input.isTouch);
      W.TopDown.build(this);
      canvas.addEventListener('click', () => { if (this.state === 'playing' && !W.Input.locked && !W.TopDown.active) W.Input.lock(); });
      W.Input.onLockChange = (locked) => { if (!locked && this.state === 'playing' && !W.Input.isTouch && !W.TopDown.active) this.pause(); };
      W.HUD.init();
      W.Menus.init(this);
      window.addEventListener('resize', () => this.resize());
      this.resize();
      this.weatherTimer = 40;
      this.spawnTimer = 2;
      this.state = 'lobby';
      if (W.Input.isTouch) document.body.classList.add('touch');
      const loading = document.getElementById('loading');
      if (loading) loading.remove();
      W.Menus.show('lobby');
      this.last = performance.now();
      requestAnimationFrame((t) => this.loop(t));
    },

    resize() {
      const w = window.innerWidth, h = window.innerHeight;
      this.renderer.setSize(w, h, false);
      this.camera.aspect = w / h;
      this.camera.updateProjectionMatrix();
    },

    // ---------- estado del jugador ----------
    stats() {
      const u = this.save.world.upgrades;
      const l = (k) => u[k] || 0;
      return {
        damage: C.DAMAGE * (1 + l('dano') * 0.2),
        mag: C.MAG_SIZE + l('cargador') * 4,
        hp: C.MAX_HP + l('vida') * 25,
        speed: 1 + l('tenis') * 0.08,
        yield: C.HARVEST_YIELD + l('cultivo'),
        grow: 1 + l('cultivo') * 0.25,
      };
    },

    createPlayer() {
      const h = Ent.makeBarista();
      this.scene.add(h.root);
      const s = this.stats();
      const spawn = W.World.hqSpot.spawn;
      this.player = {
        h, mesh: h.root, pos: { x: spawn.x, z: spawn.z }, y: 0, vy: 0, yaw: 0, pitch: -0.05, // mira al norte: su terraza y la torre
        hp: s.hp, maxHp: s.hp, ammo: s.mag, magSize: s.mag, reloading: 0, reloadTime: C.RELOAD_TIME,
        fireCool: 0, spread: 0.02, recoil: 0, aiming: false, dead: false, lastHurt: -99, lastShot: -99, phase: 0, moving: 0,
        energy: C.ENERGY_MAX, dash: 0, dashDir: { x: 0, z: -1 },
      };
      this.cherries = this.save.world.cherries != null ? this.save.world.cherries : C.START_CHERRIES;
    },

    refreshStats() {
      const s = this.stats();
      const p = this.player;
      const ratio = p.hp / p.maxHp;
      p.maxHp = s.hp;
      p.hp = Math.min(s.hp, Math.max(p.hp, s.hp * ratio));
      p.magSize = s.mag;
    },

    createZones() {
      for (const def of W.ZONES) {
        const z = new Ent.Zone(def);
        this.scene.add(z.mesh);
        this.zones.push(z);
        if (this.save.world.captured.includes(def.id)) { z.owner = 'fg'; z.progress = 1; }
      }
      this.updateZoneColors();
    },

    createPlanters() {
      // cada fila de la terraza (A, B, C) lleva la variedad que el jugador sembró en el vivero
      for (const spot of W.World.planterSpots.hq) {
        const row = (spot.label || '')[0] || '';
        this.addPlanter(Object.assign({}, spot, { row, group: 'hq', variety: this.save.world.rows[row] }));
      }
      for (const z of this.zones) if (z.owner === 'fg') this.addZonePlanters(z);
    },
    addPlanter(spot) {
      const pl = new Ent.Planter(spot);
      this.scene.add(pl.mesh);
      this.planters.push(pl);
    },
    addZonePlanters(z) {
      for (const s of W.World.planterSpots[z.id] || []) this.addPlanter(Object.assign({}, s, { group: z.id, variety: W.ZONE_VARIETY[z.id] }));
    },
    // Vivero: resiembra una fila de la terraza con otra variedad (la fila arranca de cero)
    replantRow(row, id) {
      this.save.world.rows[row] = id;
      for (const pl of this.planters) if (pl.group === 'hq' && pl.row === row) pl.replant(id);
      this.persist();
    },
    // Cerezas que da una mata hoy: mejora de cultivo + variedad; la roya avanzada se come la mitad
    plantYield(pl) {
      const n = this.stats().yield + (pl.variety.yield || 0);
      return pl.roya > 0.5 ? Math.max(1, Math.floor(n / 2)) : n;
    },

    spawnPed() {
      const spot = W.World.sidewalkPoint((code) => code !== 'P' && code !== 'F');
      const p = new Ent.Pedestrian(spot);
      this.scene.add(p.mesh);
      this.peds.push(p);
    },

    spawnCrate(x, z, amount, kind) {
      if (x === undefined) {
        const s = W.World.sidewalkPoint();
        x = s.x; z = s.z;
      }
      const l = new Ent.Loot(x, z, amount || Math.floor(rand(3, 7)), kind);
      this.scene.add(l.mesh);
      this.loot.push(l);
    },

    // ---------- territorio ----------
    ownedDistricts() {
      const set = new Set([W.HQ.district]);
      for (const z of this.zones) if (z.owner === 'fg') set.add(z.def.district);
      return set;
    },
    isMarket(x, z) { return (this.owned || this.ownedDistricts()).has(W.World.districtCodeAt(x, z)); },
    allCaptured() { return this.zones.every((z) => z.owner === 'fg'); },
    capturedCount() { return this.zones.filter((z) => z.owner === 'fg').length; },

    updateZoneColors() {
      const p = this.player;
      let best = null, bd = Infinity;
      for (const z of this.zones) {
        if (z.owner === 'fg') continue;
        const d = Math.hypot(z.pos.x - p.pos.x, z.pos.z - p.pos.z);
        if (d < bd) { bd = d; best = z; }
      }
      this.targetZone = best;
      for (const z of this.zones) z.setOwner(z.owner, z === best);
    },

    objective() {
      if (!this.allCaptured()) {
        const z = this.targetZone;
        return { title: `Libera: ${z ? z.name : ''}`, sub: `Zonas ${this.capturedCount()}/${this.zones.length}`, target: z && z.pos };
      }
      if (!this.save.world.bossDefeated) {
        return { title: `Derrota a ${W.BOSS.name}`, sub: 'Plaza de Bolívar', target: W.World.bossArena };
      }
      return { title: 'Bogotá es de la red', sub: 'Sigue cosechando Puntos Float', target: null };
    },

    // ---------- bucle ----------
    loop(now) {
      const dt = Math.min(0.05, (now - this.last) / 1000);
      this.last = now;
      this.time += dt;
      const I = W.Input;

      if (this.state === 'playing') {
        if (I.pressed('pause')) this.pause();
        else this.update(dt);
      } else if (this.state === 'paused' && I.pressed('pause') && W.Input.isTouch) {
        this.resume();
      } else if (this.state === 'lobby' && W.TopDown.active) {
        W.TopDown.lobbyCamera(this);
      } else if (this.state === 'lobby') {
        // cámara de lobby: el barista posa en su terraza, con las filas de café y la torre colgante detrás
        const sp = W.World.hqSpot.spawn;
        const a = Math.sin(this.time * 0.2) * 0.3;
        this.player.mesh.position.set(sp.x, 0.25, sp.z);
        this.player.mesh.rotation.y = 0.45;
        this.player.h.animate(this.time * 2, 0.05, false);
        this.camera.position.set(sp.x + 2.6 + Math.sin(a) * 5, 2.5, sp.z + 1.5 + Math.cos(a) * 5.5);
        this.camera.lookAt(sp.x - 2.6, 3.4, sp.z - 8);
        if (this.camera.fov !== 58) { this.camera.fov = 58; this.camera.updateProjectionMatrix(); }
      } else if (this.state === 'dead') {
        this.player.h.body.rotation.x = Math.max(-Math.PI / 2, this.player.h.body.rotation.x - dt * 4);
      }

      {
        const lobby = this.state === 'lobby', sp = W.World.hqSpot.spawn, pp = this.player;
        W.TopDown.placeSun(this, lobby ? sp.x : pp.pos.x, lobby ? 0 : pp.y, lobby ? sp.z : pp.pos.z);
      }
      W.World.update(dt, this.time, this.state === 'playing' ? this.player : null);
      this.owned = this.ownedDistricts();
      for (const p of this.peds) p.update(dt, this);
      for (const z of this.zones) z.update(dt, this.time);
      for (const l of this.loot) l.update(dt);
      this.updateRain(dt);
      this.updateBubbles(dt);
      this.renderer.render(this.scene, this.camera);
      if (this.state === 'playing' || this.state === 'dead') {
        W.HUD.update(this, dt);
        if (W.TopDown.active) W.TopDown.hud(this, dt);
      }
      I.endFrame();
      requestAnimationFrame((t) => this.loop(t));
    },

    update(dt) {
      const I = W.Input;
      const p = this.player;
      this.updatePlayer(dt);
      if (W.TopDown.active) W.TopDown.camera(this, dt); else this.updateCamera(dt);
      this.updateWeapon(dt);
      this.updateInteraction();
      this.updateSpawns(dt);
      W.Plagas.update(dt, this);
      for (const e of this.enemies) {
        if (e.update(dt, this)) e.remove = true;
      }
      for (const e of this.enemies) if (e.remove) this.scene.remove(e.mesh);
      this.enemies = this.enemies.filter((e) => !e.remove);
      this.updateZones(dt);
      this.updateProjectiles(dt);
      this.updateParticles(dt);
      const rate = this.stats().grow;
      for (const pl of this.planters) pl.update(dt, this, rate);
      // botín
      for (const l of this.loot) {
        const ld = Math.hypot(l.pos.x - p.pos.x, l.pos.z - p.pos.z);
        if (ld < C.MAGNET_RADIUS && ld > 0.01 && !p.dead) {
          // imán: el botín cercano vuela hacia el barista, más rápido cuanto más cerca
          const sp = Math.min(ld, (5 + (C.MAGNET_RADIUS - ld) * 4) * dt);
          l.pos.x += ((p.pos.x - l.pos.x) / ld) * sp; l.pos.z += ((p.pos.z - l.pos.z) / ld) * sp;
          l.mesh.position.x = l.pos.x; l.mesh.position.z = l.pos.z;
        }
        if (ld < 1.3) {
          l.taken = true;
          this.scene.remove(l.mesh);
          this.cherries += l.amount;
          this.addPoints(l.amount * C.POINTS_PER_CHERRY);
          W.HUD.gain(p.pos.x, p.y + 2.1, p.pos.z, `+${l.amount}`);
          if (l.kind === 'crate') W.HUD.toast(`Costal de café · +${l.amount} cerezas`);
          W.Audio.pickup();
          if (l.kind === 'crate') setTimeout(() => this.spawnCrate(), 8000);
        }
      }
      this.loot = this.loot.filter((l) => !l.taken);
      // distrito actual
      const d = W.World.districtAt(p.pos.x, p.pos.z);
      if (!this.currentDistrict || d.code !== this.currentDistrict.code) {
        if (this.currentDistrict) {
          const ours = this.ownedDistricts().has(d.code);
          W.HUD.banner(d.name, ours ? 'Territorio de la red Float Gardens' : 'Territorio de Arcadia', ours ? 'ours' : 'theirs');
        }
        this.currentDistrict = d;
      }
      if (I.pressed('map')) { this.showMap = !this.showMap; W.HUD.setBigMap(this.showMap); }
      // regeneración de vida estilo shooter
      if (!p.dead && this.time - p.lastHurt > C.REGEN_DELAY && p.hp < p.maxHp) p.hp = Math.min(p.maxHp, p.hp + C.REGEN_RATE * dt);
      W.HUD.lowHealth(p.hp / p.maxHp);
      this.zoneColorTimer = (this.zoneColorTimer || 0) - dt;
      if (this.zoneColorTimer <= 0) { this.zoneColorTimer = 1; this.updateZoneColors(); }
      this.weather(dt);
      this.autosaveTimer = (this.autosaveTimer || 0) - dt;
      if (this.autosaveTimer <= 0) { this.autosaveTimer = 10; this.persist(); }
    },

    updatePlayer(dt) {
      const I = W.Input;
      const p = this.player;
      const td = W.TopDown.active;
      const look = I.look();
      const aimMul = p.aiming ? 0.55 : 1;
      if (td) {
        // cenital: el yaw es la mira (joystick derecho, mouse o asistida); la cámara no gira
        p.yaw = W.TopDown.aim(this, dt);
        p.pitch = 0;
      } else {
        p.yaw -= look.x * aimMul;
        p.pitch = Math.max(-1.1, Math.min(0.75, p.pitch - look.y * aimMul));
      }
      p.aiming = I.held('aim');

      const mv = I.move();
      const cy = td ? 0 : p.yaw; // en cenital el joystick va con la pantalla (arriba = norte)
      const fx = -Math.sin(cy), fz = -Math.cos(cy);
      const rx = Math.cos(cy), rz = -Math.sin(cy);
      let dx = fx * mv.y + rx * mv.x, dz = fz * mv.y + rz * mv.x;
      const len = Math.hypot(dx, dz);
      // en tercera persona solo se corre hacia adelante; en cenital, hacia cualquier lado
      const sprint = I.held('sprint') && (td ? len > 0.3 : mv.y > 0.3) && !p.aiming && p.reloading <= 0;
      p.sprinting = sprint && len > 0.1;
      let speed = (sprint ? C.SPRINT_SPEED : C.WALK_SPEED) * this.stats().speed;
      if (p.aiming) speed *= 0.6;
      if (len > 0) {
        dx /= Math.max(1, len); dz /= Math.max(1, len);
        p.pos.x += dx * speed * dt;
        p.pos.z += dz * speed * dt;
      }
      if (p.knock) {
        p.pos.x += p.knock.x * dt; p.pos.z += p.knock.z * dt;
        p.knock.x *= 0.88; p.knock.z *= 0.88;
        if (Math.hypot(p.knock.x, p.knock.z) < 0.3) p.knock = null;
      }
      // impulso: la mochila de prensas da un salto corto y rasante. Gasta energía y no recibe daño mientras dura.
      // En cenital reemplaza al salto (botón "impulso" o Espacio); en tercera persona va en Q.
      if (p.dash <= 0) p.energy = Math.min(C.ENERGY_MAX, p.energy + C.ENERGY_REGEN * dt);
      if ((I.pressed('dash') || (td && I.pressed('jump'))) && p.dash <= 0) {
        if (p.energy >= C.DASH_COST) {
          p.energy -= C.DASH_COST;
          p.dash = C.DASH_TIME;
          p.dashDir = len > 0.15 ? { x: dx, z: dz } : { x: -Math.sin(p.yaw), z: -Math.cos(p.yaw) };
          W.Audio.dash();
          this.shake(0.06);
        } else W.Audio.empty();
      }
      if (p.dash > 0) {
        p.dash -= dt;
        // en pasos cortos para no atravesar muros delgados
        for (let i = 0; i < 3; i++) {
          p.pos.x += p.dashDir.x * C.DASH_SPEED * dt / 3; p.pos.z += p.dashDir.z * C.DASH_SPEED * dt / 3;
          W.World.collide(p.pos, C.PLAYER_RADIUS, p.y);
        }
        this.burst({ x: p.pos.x, y: p.y + 0.9, z: p.pos.z }, '#ffffff', 2, 1.2);
      }
      // salto y gravedad
      const ground = W.World.groundAt ? W.World.groundAt(p.pos.x, p.pos.z, p.y) : 0;
      if (!td && I.pressed('jump') && p.y <= ground + 0.01) { p.vy = C.JUMP_SPEED; W.Audio.jump(); }
      p.vy -= C.GRAVITY * dt;
      p.y += p.vy * dt;
      if (p.y < ground) { p.y = ground; p.vy = 0; }
      p.air = p.y > ground + 0.05; // en el aire de verdad (antes cualquier andén contaba como salto y abría la mira)
      W.World.collide(p.pos, C.PLAYER_RADIUS, p.y);

      p.moving = Math.min(1, len);
      p.phase += dt * (sprint ? 13 : 9) * p.moving;
      const combat = p.aiming || this.time - p.lastShot < 0.8 || I.held('fire') || (td && W.TopDown.target);
      let face = p.yaw + Math.PI;
      if (!combat && len > 0.1) face = Math.atan2(dx, dz);
      p.mesh.rotation.y = Ent.lerpAngle(p.mesh.rotation.y, face, Math.min(1, dt * 14));
      p.mesh.position.set(p.pos.x, p.y, p.pos.z);
      p.h.animate(p.phase, p.moving * (sprint ? 1.2 : 0.8), combat);
      // durante el impulso el cuerpo se inclina hacia adelante
      p.h.body.rotation.x += ((p.dash > 0 ? 0.5 : 0) - p.h.body.rotation.x) * Math.min(1, dt * 18);
    },

    updateCamera(dt) {
      const p = this.player;
      const cam = this.camera;
      const targetFov = p.aiming ? 48 : p.sprinting ? 76 : 70;
      if (Math.abs(targetFov - cam.fov) > 0.01) {
        cam.fov += (targetFov - cam.fov) * Math.min(1, dt * 10);
        cam.updateProjectionMatrix();
      }
      const dist = p.aiming ? 2.3 : 3.6;
      const side = p.aiming ? 0.95 : 1.05;
      const cp = Math.cos(p.pitch + p.recoil);
      // vectores reutilizados: sin basura para el recolector en cada cuadro
      const v = this.camVecs || (this.camVecs = { right: new THREE.Vector3(), pivot: new THREE.Vector3(), back: new THREE.Vector3(), look: new THREE.Vector3() });
      const dir = new THREE.Vector3(-Math.sin(p.yaw) * cp, Math.sin(p.pitch + p.recoil), -Math.cos(p.yaw) * cp);
      const right = v.right.set(Math.cos(p.yaw), 0, -Math.sin(p.yaw));
      const pivot = v.pivot.set(p.pos.x, p.y + (p.aiming ? 1.8 : 1.95), p.pos.z).addScaledVector(right, side);
      const back = v.back.copy(dir).multiplyScalar(-1);
      let d = dist;
      const hit = W.World.raycast(pivot, back, dist + 0.3);
      if (hit < dist + 0.3) d = Math.max(0.4, hit - 0.3);
      this.camDist = this.camDist === undefined ? d : this.camDist + (d - this.camDist) * Math.min(1, dt * (d < this.camDist ? 30 : 6));
      cam.position.copy(pivot).addScaledVector(back, this.camDist);
      cam.position.y = Math.max(0.3, cam.position.y);
      if (this.shakeAmt > 0) {
        cam.position.x += (Math.random() - 0.5) * this.shakeAmt;
        cam.position.y += (Math.random() - 0.5) * this.shakeAmt;
        this.shakeAmt = Math.max(0, this.shakeAmt - dt * 2);
      }
      cam.lookAt(v.look.copy(cam.position).add(dir));
      this.aimDir = dir;
      p.recoil = Math.max(0, p.recoil - dt * 0.15);
    },

    // ---------- arma ----------
    updateWeapon(dt) {
      const I = W.Input;
      const p = this.player;
      p.fireCool -= dt;
      const base = p.aiming ? 0.006 : 0.018;
      const target = base + p.moving * (p.aiming ? 0.01 : 0.03) + (p.air ? 0.05 : 0);
      p.spread += (target - p.spread) * Math.min(1, dt * 8);
      if (p.reloading > 0) {
        p.reloading -= dt;
        if (p.reloading <= 0) {
          p.ammo = p.magSize;
          this.cherries -= C.CHERRIES_PER_RELOAD;
        }
        return;
      }
      if (I.pressed('reload') && p.ammo < p.magSize) this.reload();
      if (I.held('fire') && p.fireCool <= 0 && !p.sprinting) {
        if (p.ammo <= 0) {
          if (I.pressed('fire')) { W.Audio.empty(); this.reload(); }
          return;
        }
        this.fire();
      }
    },

    reload() {
      const p = this.player;
      if (p.reloading > 0) return;
      if (this.cherries < C.CHERRIES_PER_RELOAD) {
        W.HUD.toast('Sin cerezas · cosecha en tu terraza o busca costales');
        return;
      }
      p.reloading = p.reloadTime;
      W.Audio.reload();
    },

    fire() {
      const p = this.player;
      p.fireCool = C.FIRE_INTERVAL;
      p.ammo--;
      p.lastShot = this.time;
      p.recoil = Math.min(0.035, p.recoil + 0.006);
      p.spread += 0.008;
      W.Audio.shot();
      const ray = W.TopDown.active ? W.TopDown.shotRay(this) : { origin: this.camera.position.clone(), dir: this.aimDir.clone(), minT: this.camDist || 1 };
      const cam = ray.origin;
      const dir = ray.dir;
      dir.x += (Math.random() - 0.5) * p.spread * 2;
      dir.y += (Math.random() - 0.5) * p.spread * 2;
      dir.z += (Math.random() - 0.5) * p.spread * 2;
      dir.normalize();
      // primero lo que corta el rayo desde la cámara (paredes o enemigos)
      let tHit = W.World.raycast(cam, dir, C.RANGE);
      let target = null, head = false;
      const minT = ray.minT;
      for (const e of this.enemies) {
        if (e.dead) continue;
        if (e.pest) {
          // plaga: una sola esfera a la altura a la que vuela o camina
          const rp = this.raySphere(cam, dir, e.pos.x, e.mesh.position.y + e.alt, e.pos.z, e.hitR);
          if (rp > minT && rp < tHit) { tHit = rp; target = e; head = false; }
          continue;
        }
        if (e.h.drone) {
          // dron: una esfera para el cuerpo y, dentro, el núcleo (el ojo): pasar por el núcleo es golpe crítico
          const cy = e.mesh.position.y + 1.3;
          const rb = this.raySphere(cam, dir, e.pos.x, cy, e.pos.z, 0.68);
          if (rb > minT && rb < tHit) { tHit = rb; target = e; head = this.raySphere(cam, dir, e.pos.x, cy, e.pos.z, 0.27) < Infinity; }
          continue;
        }
        const sc = e.boss ? 2.6 : 1;
        const res = this.raySphere(cam, dir, e.pos.x, e.headY + (e.boss ? 0 : 0), e.pos.z, 0.24 * sc);
        if (res > minT && res < tHit) { tHit = res; target = e; head = true; }
        const rb = this.raySphere(cam, dir, e.pos.x, 1.15 * sc, e.pos.z, 0.5 * sc);
        if (rb > minT && rb < tHit) { tHit = rb; target = e; head = false; }
        const rl = this.raySphere(cam, dir, e.pos.x, 0.5 * sc, e.pos.z, 0.4 * sc);
        if (rl > minT && rl < tHit) { tHit = rl; target = e; head = false; }
      }
      const hitPoint = cam.clone().addScaledVector(dir, tHit);
      const muzzle = new THREE.Vector3();
      p.h.gun.userData.muzzle.getWorldPosition(muzzle);
      // ¿la pared tapa el disparo desde la boca del arma?
      const md = hitPoint.clone().sub(muzzle);
      const mlen = md.length();
      md.normalize();
      const block = W.World.raycast(muzzle, md, mlen);
      if (block < mlen - 0.3) { target = null; hitPoint.copy(muzzle).addScaledVector(md, block); }
      const tr = Ent.makeTracer();
      tr.position.copy(muzzle);
      tr.lookAt(hitPoint);
      tr.rotateX(Math.PI / 2);
      this.scene.add(tr);
      this.tracers.push({ mesh: tr, from: muzzle, to: hitPoint, dist: muzzle.distanceTo(hitPoint), t: 0, target, head });
      this.burst(muzzle, '#8b5a2b', 3, 2);
      W.TopDown.muzzleFlash(muzzle);
      if (W.TopDown.active) this.shake(0.035);
    },

    raySphere(o, d, cx, cy, cz, r) {
      const ox = o.x - cx, oy = o.y - cy, oz = o.z - cz;
      const b = ox * d.x + oy * d.y + oz * d.z;
      const c = ox * ox + oy * oy + oz * oz - r * r;
      const h = b * b - c;
      if (h < 0) return Infinity;
      const t = -b - Math.sqrt(h);
      return t > 0 ? t : Infinity;
    },

    updateProjectiles(dt) {
      for (const tr of this.tracers) {
        tr.t += C.TRACER_SPEED * dt;
        const k = Math.min(1, tr.t / tr.dist);
        tr.mesh.position.lerpVectors(tr.from, tr.to, k);
        if (k >= 1) {
          tr.done = true;
          this.scene.remove(tr.mesh);
          this.burst(tr.to, '#6b3b1f', 6, 3);
          if (tr.target && !tr.target.dead) this.hitEnemy(tr.target, tr.head, tr.to);
        }
      }
      this.tracers = this.tracers.filter((t) => !t.done);

      const p = this.player;
      for (const s of this.stamps) {
        s.mesh.position.x += s.vx * dt;
        s.mesh.position.y += s.vy * dt;
        s.mesh.position.z += s.vz * dt;
        s.mesh.rotation.x += dt * 10;
        s.life -= dt;
        const m = s.mesh.position;
        if (!p.dead && Math.hypot(m.x - p.pos.x, m.z - p.pos.z) < 0.6 && m.y > p.y && m.y < p.y + 2) {
          s.life = 0;
          this.damagePlayer(s.dmg, s.from);
          W.Audio.stamp();
        } else if (W.World.solidAt(m.x, m.y, m.z)) {
          s.life = 0;
          this.burst(m, '#2a22f5', 4, 2);
        }
        if (s.life <= 0) this.scene.remove(s.mesh);
      }
      this.stamps = this.stamps.filter((s) => s.life > 0);
    },

    hitEnemy(e, head, at) {
      const dmg = this.stats().damage * (head ? C.HEADSHOT_MUL : 1) * rand(0.9, 1.1);
      e.hit(dmg);
      W.HUD.damageNumber(at.x, at.y + 0.3, at.z, dmg, head);
      W.HUD.hitMarker(head, e.dead);
      W.Audio.hit(head);
      if (e.dead) this.onKO(e, head);
    },

    onKO(e, head) {
      W.Audio.ko();
      if (e.pest) {
        // plaga apagada: polvo del color de la plaga y, si era una broca cargada, suelta la cosecha
        this.save.world.plagas = (this.save.world.plagas || 0) + 1;
        this.addPoints(e.t.points);
        this.save.money += e.t.money;
        W.HUD.killFeed(`<b class="me">Barista</b> controló <b class="foe plaga">${e.name}</b>`);
        const at = { x: e.pos.x, y: e.mesh.position.y + e.alt, z: e.pos.z };
        this.burst(at, e.t.color, 12, 4);
        this.burst(at, '#6b3b1f', 5, 2.5);
        if (e.stolen) this.spawnCrate(e.pos.x, e.pos.z, e.stolen, 'orb');
        return;
      }
      this.save.world.kos++;
      this.addPoints(e.boss ? C.POINTS_PER_BOSS : C.POINTS_PER_KO);
      this.save.money += e.boss ? 50000 : C.KO_MONEY;
      W.HUD.killFeed(`<b class="me">Barista</b> apagó <b class="foe">${e.name}</b>${head ? ' <span class="hs">crítico</span>' : ''}`);
      this.say(e, W.KO_LINES[Math.floor(Math.random() * W.KO_LINES.length)]);
      // chispas azules y blancas: el dron se apaga
      this.burst({ x: e.pos.x, y: 1.3, z: e.pos.z }, '#2a22f5', 12, 5);
      this.burst({ x: e.pos.x, y: 1.3, z: e.pos.z }, '#ffffff', 6, 3.5);
      this.shake(e.boss ? 0.5 : 0.1);
      if (!e.boss && Math.random() < 0.45) this.spawnCrate(e.pos.x, e.pos.z, 2, 'orb');
      if (e.boss) this.onBossDefeated();
    },

    damagePlayer(dmg, from, opts) {
      const p = this.player;
      if (p.dead || p.dash > 0) return;
      p.hp -= dmg;
      p.lastHurt = this.time;
      W.HUD.hurt(dmg);
      W.Audio.hurt();
      this.shake(0.15);
      if (opts && opts.knock && from) {
        const dx = p.pos.x - from.pos.x, dz = p.pos.z - from.pos.z;
        const d = Math.hypot(dx, dz) || 1;
        p.knock = { x: (dx / d) * opts.knock, z: (dz / d) * opts.knock };
      }
      if (p.hp <= 0) this.die();
    },

    die() {
      const p = this.player;
      p.dead = true;
      p.hp = 0;
      this.state = 'dead';
      const lost = Math.floor(this.cherries / 2);
      this.cherries -= lost;
      W.Input.unlock();
      W.Menus.show('dead', { lost });
    },

    respawn() {
      const p = this.player;
      const s = W.World.hqSpot.spawn;
      p.pos.x = s.x; p.pos.z = s.z; p.y = 0; p.vy = 0; p.yaw = 0;
      p.dead = false;
      p.energy = C.ENERGY_MAX; p.dash = 0;
      p.hp = p.maxHp;
      p.ammo = p.magSize;
      p.reloading = 0;
      p.h.body.rotation.x = 0;
      this.cherries = Math.max(this.cherries, 4);
      for (const e of this.enemies) e.alerted = false;
      this.stamps.forEach((st) => this.scene.remove(st.mesh));
      this.stamps = [];
      this.play();
    },

    throwStamp(from, target, dmg, angleOff, speedMul) {
      const sx = from.pos.x, sz = from.pos.z;
      const sy = from.boss ? 3.6 : 1.5;
      const tx = target.pos.x, tz = target.pos.z, ty = target.y + 1.2;
      let dx = tx - sx, dy = ty - sy, dz = tz - sz;
      const d = Math.hypot(dx, dy, dz) || 1;
      dx /= d; dy /= d; dz /= d;
      if (!from.boss) angleOff = (Math.random() - 0.5) * 0.12;
      if (angleOff) {
        const c = Math.cos(angleOff), s = Math.sin(angleOff);
        [dx, dz] = [dx * c - dz * s, dx * s + dz * c];
      }
      const sp = 24 * (speedMul ? speedMul / 4 : 1);
      const m = Ent.makeStamp();
      m.position.set(sx, sy, sz);
      this.scene.add(m);
      this.stamps.push({ mesh: m, vx: dx * sp + (Math.random() - 0.5), vy: dy * sp, vz: dz * sp + (Math.random() - 0.5), life: 3, dmg, from });
    },

    lineOfSight(a, ay, b, by) {
      const o = new THREE.Vector3(a.x, ay, a.z);
      const d = new THREE.Vector3(b.x - a.x, by - ay, b.z - a.z);
      const len = d.length();
      d.normalize();
      return W.World.raycast(o, d, len) >= len - 0.5;
    },

    say(ent, text) {
      const el = document.createElement('div');
      el.className = 'bubble';
      el.textContent = text;
      document.getElementById('numbers').appendChild(el);
      this.bubbles.push({ el, ent, t: 0 });
    },
    updateBubbles(dt) {
      const v = this.bubbleVec || (this.bubbleVec = new THREE.Vector3());
      const W2 = window.innerWidth / 2, H2 = window.innerHeight / 2;
      for (const b of this.bubbles) {
        b.t += dt;
        const h = b.ent.boss ? 6.4 : 2.7;
        v.set(b.ent.pos.x, h, b.ent.pos.z).project(this.camera);
        const dist = Math.hypot(b.ent.pos.x - this.player.pos.x, b.ent.pos.z - this.player.pos.z);
        b.el.style.display = v.z < 1 && dist < 45 && this.state === 'playing' ? 'block' : 'none';
        b.el.style.transform = `translate(${v.x * W2 + W2}px, ${-v.y * H2 + H2}px) translate(-50%, -100%)`;
        if (b.t > 2.2) b.el.remove();
      }
      this.bubbles = this.bubbles.filter((b) => b.t <= 2.2);
    },

    // ---------- interacción ----------
    updateInteraction() {
      const I = W.Input;
      const p = this.player;
      this.prompt = null;
      const near = (o, r) => Math.hypot(o.x - p.pos.x, o.z - p.pos.z) < r;
      // cultivo
      for (const pl of this.planters) {
        if (pl.roya > 0 && near(pl.pos, pl.reach)) {
          // la roya se trata a mano (caldo bordelés); mientras antes, menos cosecha se pierde
          this.prompt = `<kbd>E</kbd> Tratar la roya${pl.label ? ` · fila ${pl.label}` : ''}`;
          if (I.pressed('action')) {
            pl.cure();
            pl.pop = 1;
            this.addPoints(2);
            W.HUD.toast(`Roya tratada${pl.label ? ` en la fila ${pl.label}` : ''} · +2 puntos`);
            W.HUD.gain(pl.pos.x, pl.mesh.position.y + 1.6, pl.pos.z, 'sana');
            W.Audio.pickup();
            this.burst({ x: pl.pos.x, y: pl.mesh.position.y + 1, z: pl.pos.z }, '#8ecfd0', 12, 3);
          }
          return;
        }
        if (pl.ripe && near(pl.pos, pl.reach)) {
          this.prompt = `<kbd>E</kbd> Cosechar ${pl.label ? `fila ${pl.label}` : 'café hidropónico'} · ${pl.variety.name}`;
          if (I.pressed('action')) {
            const n = this.plantYield(pl);
            const bonus = pl.variety.bonus || 0;
            pl.harvest();
            this.cherries += n;
            this.addPoints(n * C.POINTS_PER_CHERRY);
            this.save.money += bonus;
            W.HUD.toast(`${pl.variety.name} · +${n} cerezas · +${n} puntos${bonus ? ` · +$${bonus.toLocaleString('es-CO')}` : ''}`);
            W.HUD.gain(pl.pos.x, pl.mesh.position.y + 1.6, pl.pos.z, `+${n}`);
            W.Audio.harvest();
            this.burst({ x: pl.pos.x, y: pl.mesh.position.y + 0.9, z: pl.pos.z }, '#b3121a', 12, 3.5);
            this.burst({ x: pl.pos.x, y: pl.mesh.position.y + 0.9, z: pl.pos.z }, '#8cc84a', 6, 2.5);
          }
          return;
        }
      }
      // tienda en el HQ
      const shop = W.World.hqSpot.shop;
      if (near(shop, 3.2)) {
        this.prompt = '<kbd>E</kbd> Tienda de mejoras';
        if (I.pressed('action')) this.openShop();
        return;
      }
      // clientes
      for (const c of this.peds) {
        if (c.icon.visible && near(c.pos, 2.6)) {
          if (this.cherries < C.CHERRIES_PER_SALE) { this.prompt = `Necesitas ${C.CHERRIES_PER_SALE} cerezas para preparar un café`; return; }
          this.prompt = `<kbd>E</kbd> Vender café · ${'$' + C.SALE_PRICE.toLocaleString('es-CO')}`;
          if (I.pressed('action')) {
            this.cherries -= C.CHERRIES_PER_SALE;
            this.save.money += C.SALE_PRICE;
            this.save.world.sales++;
            this.addPoints(C.POINTS_PER_SALE);
            c.wants = false;
            c.icon.visible = false;
            c.wantTimer = 30 + Math.random() * 30;
            c.stop = 1.5;
            W.HUD.toast(`Venta · +$${C.SALE_PRICE.toLocaleString('es-CO')} · +${C.POINTS_PER_SALE} puntos`);
            W.Audio.cash();
          }
          return;
        }
      }
      if (this.player.ammo === 0 && this.cherries < C.CHERRIES_PER_RELOAD) this.prompt = 'Sin café · cosecha en tu terraza o busca costales';
    },

    // ---------- enemigos y zonas ----------
    updateSpawns(dt) {
      const p = this.player;
      const owned = this.ownedDistricts();
      this.spawnTimer -= dt;
      const cap = 3 + this.capturedCount();
      const patrols = this.enemies.filter((e) => !e.zoneId && !e.boss && !e.pest && !e.dead).length;
      if (this.spawnTimer <= 0 && patrols < cap) {
        this.spawnTimer = 2.5;
        for (let tries = 0; tries < 12; tries++) {
          const s = W.World.sidewalkPoint((code) => !owned.has(code) && code !== 'P');
          const d = Math.hypot(s.x - p.pos.x, s.z - p.pos.z);
          if (d < 45 || d > 120) continue;
          this.spawnEnemy(Math.random() < 0.3 + this.capturedCount() * 0.07 ? 'guardia' : 'inspector', s.x, s.z);
          break;
        }
      }
      // despawn de patrullas lejanas
      for (const e of this.enemies) {
        if (!e.zoneId && !e.boss && !e.pest && !e.alerted && Math.hypot(e.pos.x - p.pos.x, e.pos.z - p.pos.z) > 150) e.remove = true;
      }
      // jefe final
      if (this.allCaptured() && !this.save.world.bossDefeated && !this.boss) {
        const a = W.World.bossArena;
        if (Math.hypot(a.x - p.pos.x, a.z - p.pos.z) < 70) {
          this.boss = new Ent.Boss(a.x + 8, a.z - 10);
          this.scene.add(this.boss.mesh);
          this.enemies.push(this.boss);
          W.HUD.banner(W.BOSS.name, 'El director de Arcadia sale de su torre', 'theirs');
          this.say(this.boss, 'Nadie compite con Arcadia');
        }
      }
    },

    spawnEnemy(kind, x, z, opts) {
      const e = new Ent.Enemy(kind, x, z, Object.assign({ hpMul: 1 + this.capturedCount() * 0.1 }, opts));
      this.scene.add(e.mesh);
      this.enemies.push(e);
      return e;
    },

    summonAround(pos, n) {
      for (let i = 0; i < n; i++) {
        const a = Math.random() * Math.PI * 2;
        this.spawnEnemy('inspector', pos.x + Math.cos(a) * 6, pos.z + Math.sin(a) * 6, { alerted: true, zoneId: 'boss' });
      }
    },

    updateZones(dt) {
      const p = this.player;
      this.capturing = null;
      for (const z of this.zones) {
        if (z.owner === 'fg') continue;
        const inside = !p.dead && Math.hypot(z.pos.x - p.pos.x, z.pos.z - p.pos.z) < C.CAPTURE_RADIUS;
        if (!inside) { z.progress = Math.max(0, z.progress - dt * 0.01); continue; }
        if (!z.waves) z.waves = 0;
        const n = this.capturedCount();
        if (z.waves === 0 || (z.waves === 1 && z.progress > 0.4) || (z.waves === 2 && z.progress > 0.75)) {
          const count = 2 + Math.floor(n / (z.waves === 0 ? 2 : 3));
          const cell = W.World.cellCenter(z.def.row, z.def.col);
          const half = W.World.CELL / 2;
          for (let i = 0; i < count; i++) {
            // los refuerzos llegan por las calles que rodean la zona
            const side = Math.floor(Math.random() * 4), t = rand(-22, 22);
            const pt = [[cell.x + t, cell.z - half], [cell.x + t, cell.z + half], [cell.x - half, cell.z + t], [cell.x + half, cell.z + t]][side];
            const kind = i % 3 === 2 || Math.random() < 0.2 + n * 0.05 ? 'guardia' : 'inspector';
            this.spawnEnemy(kind, pt[0], pt[1], { alerted: true, zoneId: z.id });
          }
          if (z.waves === 0) W.HUD.banner(z.name, 'Defiende la zona hasta liberarla', 'theirs');
          z.waves++;
        }
        const contested = this.enemies.some((e) => !e.dead && !e.pest && Math.hypot(e.pos.x - z.pos.x, e.pos.z - z.pos.z) < C.CAPTURE_RADIUS + 8);
        if (!contested) z.progress = Math.min(1, z.progress + dt / C.CAPTURE_TIME);
        this.capturing = { zone: z, contested };
        if (z.progress >= 1) this.captureZone(z);
      }
    },

    captureZone(z) {
      z.owner = 'fg';
      z.progress = 1;
      this.save.world.captured.push(z.id);
      this.addPoints(C.POINTS_PER_ZONE);
      this.save.money += C.ZONE_MONEY;
      this.addZonePlanters(z);
      W.Audio.capture();
      const n = this.capturedCount();
      W.HUD.banner('Zona liberada', `${z.name} ahora es de la red · ${n}/${this.zones.length}`, 'ours');
      W.HUD.killFeed(`<b class="me">Float Gardens</b> liberó <b>${z.name}</b>`);
      for (const e of this.enemies) if (e.zoneId === z.id && !e.dead) { e.hp = 0; e.ko = 0; this.say(e, 'Retirada'); }
      this.updateZoneColors();
      this.persist();
      if (this.allCaptured()) {
        setTimeout(() => W.HUD.banner('La Torre Arcadia quedó expuesta', `Ve a la Plaza de Bolívar y derrota a ${W.BOSS.name}`, 'theirs'), 3200);
      }
    },

    onBossDefeated() {
      this.save.world.bossDefeated = true;
      for (const e of this.enemies) if (!e.dead) { e.hp = 0; }
      this.persist();
      W.Audio.capture();
      W.HUD.banner('Bogotá es de la red', `${W.BOSS.name} colgó el sombrero`, 'ours');
      setTimeout(() => { if (this.state === 'playing') { this.state = 'paused'; W.Input.unlock(); W.Menus.show('victory'); } }, 3500);
    },

    // ---------- efectos ----------
    burst(at, color, n, speed) {
      for (let i = 0; i < n; i++) {
        if (this.particles.length > 160) break;
        this.particles.push({ x: at.x, y: at.y, z: at.z, color, vx: rand(-1, 1) * speed, vy: rand(0.5, 1.5) * speed, vz: rand(-1, 1) * speed, life: rand(0.3, 0.7) });
      }
      this.partPool.sync(this.particles);
    },
    updateParticles(dt) {
      for (const p of this.particles) {
        p.vy -= 18 * dt;
        p.x += p.vx * dt;
        p.y += p.vy * dt;
        p.z += p.vz * dt;
        p.life -= dt;
        if (p.life <= 0 || p.y < 0) p.life = 0;
      }
      this.particles = this.particles.filter((p) => p.life > 0);
      this.partPool.sync(this.particles);
    },
    shake(a) { this.shakeAmt = Math.max(this.shakeAmt, a); },

    // Lluvia bogotana
    // Las gotas no se recalculan en la CPU: hay dos capas idénticas (0-40 m y 40-80 m) y se desplaza
    // la malla completa hacia abajo, repitiendo cada 40 m. Las gotas bajo el suelo quedan ocultas.
    buildRain() {
      const n = 2200;
      const pos = new Float32Array(n * 12);
      for (let i = 0; i < n; i++) {
        const x = rand(-40, 40), y = rand(0, 40), z = rand(-40, 40);
        pos.set([x, y, z, x, y - 0.6, z, x, y + 40, z, x, y + 39.4, z], i * 12);
      }
      const g = new THREE.BufferGeometry();
      g.setAttribute('position', new THREE.BufferAttribute(pos, 3));
      this.rain = new THREE.LineSegments(g, new THREE.LineBasicMaterial({ color: '#cfdbe6', transparent: true, opacity: 0.45 }));
      this.rain.frustumCulled = false;
      this.rain.visible = false;
      this.scene.add(this.rain);
    },
    updateRain(dt) {
      if (!this.rain.visible) return;
      this.rainFall = ((this.rainFall || 0) + 28 * dt) % 40;
      this.rain.position.set(this.camera.position.x, -this.rainFall, this.camera.position.z);
    },
    weather(dt) {
      this.weatherTimer -= dt;
      if (this.weatherTimer <= 0) {
        this.raining = !this.raining;
        this.weatherTimer = this.raining ? rand(35, 60) : rand(50, 90);
        this.rain.visible = this.raining;
        W.TopDown.sky(this);
        W.HUD.toast(this.raining ? 'Empezó a llover en Bogotá' : 'Escampó');
      }
    },

    // ---------- economía y guardado ----------
    addPoints(n) {
      this.save.puntos += n;
      this.save.puntosTotal += n;
    },
    persist() {
      this.save.world.cherries = this.cherries;
      FG.Save.write(this.save);
    },

    // ---------- estados ----------
    play() {
      this.state = 'playing';
      W.Menus.hide();
      W.HUD.show(true);
      document.body.classList.add('playing');
      if (W.TopDown.leaveLobby) W.TopDown.leaveLobby(this);
      if (!W.TopDown.active) W.Input.lock();
      W.Input.endFrame();
      W.Audio.ensure();
      if (!this.currentDistrict) {
        const d = W.World.districtAt(this.player.pos.x, this.player.pos.z);
        this.currentDistrict = d;
        W.HUD.banner(d.name, `${W.HQ.name} · Neo-Bogotá`, 'ours');
      }
      this.updateZoneColors();
    },
    pause() {
      if (this.state !== 'playing') return;
      this.state = 'paused';
      this.persist();
      W.Input.unlock();
      this.showMap = false;
      W.HUD.setBigMap(false);
      W.Menus.show('pause');
    },
    resume() { this.play(); },
    openShop() {
      this.state = 'paused';
      W.Input.unlock();
      W.Menus.show('shop', { from: 'game' });
    },
    toLobby() {
      this.persist();
      this.state = 'lobby';
      W.HUD.show(false);
      document.body.classList.remove('playing');
      W.Menus.show('lobby');
    },
  };

  W.Game = G;
  window.addEventListener('load', () => G.init());
})();
