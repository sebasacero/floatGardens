// Vista cenital ("devlog"): cámara casi vertical de lente larga, mira asistida tipo twin-stick,
// piso con retícula y dos temas: noche (piso carbón, el estilo del video) y día (maqueta blanca).
// Se activa sola en pantallas táctiles; en PC se elige en el lobby o en pausa. Todo el mundo 3D,
// los enemigos y el balance son los mismos: solo cambian cámara, mira, controles y HUD.
(function () {
  const KEY = 'fg.cenital', KEY_THEME = 'fg.cenital.tema';
  const read = (k, def) => { try { const v = localStorage.getItem(k); return v === null ? def : v; } catch (e) { return def; } };
  const write = (k, v) => { try { localStorage.setItem(k, v); } catch (e) { /* modo privado */ } };

  const THEMES = {
    noche: {
      sky: '#23272c', fogNear: 90, fogFar: 520,
      ground: '#262b30', paving: '#3a4046', grid: 'rgba(255,255,255,0.09)',
      hemi: { sky: '#aab4c8', ground: '#15181b', intensity: 0.42 }, sun: 0.72,
    },
    dia: {
      sky: '#e3e9e8', fogNear: 110, fogFar: 650,
      ground: '#ffffff', paving: '#ffffff', grid: 'rgba(44,48,46,0.14)',
      hemi: { sky: '#eef4ff', ground: '#8f9a88', intensity: 0.75 }, sun: 1.0,
    },
  };

  const TD = {
    enabled: false,
    theme: 'noche',
    // cámara
    FOV: 24, DIST: 54, TILT: 9 * Math.PI / 180,
    AUTO_AIM_RANGE: 42, AUTO_AIM_CONE: Math.PI / 5, SNAP_CONE: 0.2,
    GRID: 8,

    load(isTouch) {
      const saved = read(KEY, null);
      this.enabled = saved === null ? !!isTouch : saved === '1';
      this.theme = THEMES[read(KEY_THEME, 'noche')] ? read(KEY_THEME, 'noche') : 'noche';
    },
    get active() { return this.enabled; },

    set(G, on) {
      this.enabled = !!on;
      write(KEY, on ? '1' : '0');
      this.apply(G);
    },
    setTheme(G, t) {
      if (!THEMES[t]) return;
      this.theme = t;
      write(KEY_THEME, t);
      this.apply(G);
    },
    toggleTheme(G) { this.setTheme(G, this.theme === 'noche' ? 'dia' : 'noche'); },

    // ---------- escena ----------
    build(G) {
      const scene = G.scene;
      // retícula del piso: sigue al jugador a saltos de GRID para que las líneas no "naden"
      const cv = document.createElement('canvas');
      cv.width = cv.height = 128;
      this.gridCtx = cv.getContext('2d');
      this.gridTex = new THREE.CanvasTexture(cv);
      this.gridTex.wrapS = this.gridTex.wrapT = THREE.RepeatWrapping;
      const reps = 400 / this.GRID;
      this.gridTex.repeat.set(reps, reps);
      this.grid = new THREE.Mesh(new THREE.PlaneGeometry(400, 400), new THREE.MeshBasicMaterial({ map: this.gridTex, transparent: true, depthWrite: false }));
      this.grid.rotation.x = -Math.PI / 2;
      this.grid.position.y = 0.02;
      this.grid.renderOrder = 1;
      this.grid.visible = false;
      scene.add(this.grid);
      // sombra blanda bajo el barista (la escena no usa sombras reales)
      this.blob = new THREE.Mesh(new THREE.CircleGeometry(0.75, 24), new THREE.MeshBasicMaterial({ color: '#000', transparent: true, opacity: 0.28, depthWrite: false }));
      this.blob.rotation.x = -Math.PI / 2;
      this.blob.position.y = 0.03;
      this.blob.renderOrder = 2;
      this.blob.visible = false;
      scene.add(this.blob);
      // marcador de mira: una línea fina en el piso hacia donde apunta (lima = "lo que apuntas")
      this.aimLine = new THREE.Mesh(new THREE.PlaneGeometry(0.1, 1), new THREE.MeshBasicMaterial({ color: '#d6e48a', transparent: true, opacity: 0.9, depthWrite: false }));
      this.aimLine.rotation.x = -Math.PI / 2;
      this.aimLine.position.y = 0.04;
      this.aimLine.renderOrder = 3;
      this.aimLine.visible = false;
      scene.add(this.aimLine);
      this.camVec = new THREE.Vector3();
      this.lookVec = new THREE.Vector3();
      this.follow = null;
      this.apply(G);
    },

    drawGrid(color) {
      const g = this.gridCtx, S = 128;
      g.clearRect(0, 0, S, S);
      g.fillStyle = color;
      g.fillRect(0, 0, S, 1.5);
      g.fillRect(0, 0, 1.5, S);
      this.gridTex.needsUpdate = true;
    },

    apply(G) {
      const on = this.enabled;
      const T = THEMES[this.theme];
      const body = document.body;
      body.classList.toggle('cenital', on);
      body.classList.toggle('cenital-noche', on && this.theme === 'noche');
      body.classList.toggle('cenital-dia', on && this.theme === 'dia');
      W.Input.topdown = on;
      if (this.grid) this.grid.visible = on;
      if (this.blob) this.blob.visible = false; // con sombras reales no hace falta
      if (G.sun) G.sun.castShadow = on;
      this.reskin(G, on);
      if (this.aimLine) this.aimLine.visible = false;
      if (on) this.drawGrid(T.grid);
      // luces, cielo, niebla y piso
      const sky = on ? T.sky : G.skyColor.getStyle();
      G.scene.background.set(sky);
      G.scene.fog.color.set(sky);
      G.scene.fog.near = on ? T.fogNear : 40;
      G.scene.fog.far = on ? T.fogFar : 430;
      if (G.hemi) {
        if (on) { G.hemi.color.set(T.hemi.sky); G.hemi.groundColor.set(T.hemi.ground); G.hemi.intensity = T.hemi.intensity; }
        else { G.hemi.color.copy(G.hemiBase.color); G.hemi.groundColor.copy(G.hemiBase.ground); G.hemi.intensity = G.hemiBase.intensity; }
      }
      if (G.sun) G.sun.intensity = on ? T.sun : G.sunBase;
      if (W.World.ground) W.World.ground.material.color.set(on ? T.ground : '#ffffff');
      // andenes, adoquín y pasto: de noche son "la mesa oscura" bajo la maqueta blanca
      const mats = W.World.mats || {};
      for (const k of ['sidewalk', 'cobble', 'grass']) if (mats[k]) mats[k].color.set(on ? T.paving : '#ffffff');
      if (W.World.pool) W.World.pool.material.color.set(on && this.theme === 'noche' ? '#2f363b' : '#8e9c9d');
      if (G.camera) {
        G.camera.fov = on ? this.FOV : 70;
        G.camera.updateProjectionMatrix();
      }
      this.follow = null;
      this.labelButtons(on);
      if (on) W.Input.unlock();
    },

    // Arte de la cenital: barista de armadura blanca con mochila de prensas, drones de Arcadia y costales durazno.
    // Al volver a tercera persona vuelve el barista de delantal y los inspectores humanos.
    reskin(G, on) {
      const p = G.player;
      if (p && (!!p.h.cenital !== on)) {
        const h = on ? W.Ent.makeBaristaBlanco() : W.Ent.makeBarista();
        h.cenital = on;
        h.root.traverse((o) => { if (o.isMesh) o.castShadow = true; });
        h.root.position.copy(p.mesh.position);
        h.root.rotation.y = p.mesh.rotation.y;
        G.scene.remove(p.mesh);
        G.scene.add(h.root);
        p.h = h;
        p.mesh = h.root;
      }
      for (const e of G.enemies || []) if (!e.dead && e.reskin && (!!e.h.drone !== on)) e.reskin();
      if (G.loot) {
        G.loot = G.loot.map((l) => {
          G.scene.remove(l.mesh);
          const n = new W.Ent.Loot(l.pos.x, l.pos.z, l.amount, l.kind);
          G.scene.add(n.mesh);
          return n;
        });
      }
    },

    // Los botones táctiles cambian de texto: en cenital son cuadrados con etiqueta, como en el video.
    labelButtons(on) {
      const L = { tFire: ['💥', 'disparar'], tJump: ['⤒', 'impulso'], tReload: ['↻', 'recargar'], tAction: ['E', 'cosechar'], tAim: ['🎯', 'fijar'] };
      for (const id in L) {
        const el = document.getElementById(id);
        if (!el) continue;
        el.textContent = on ? L[id][1] : L[id][0];
      }
    },

    // ---------- mira ----------
    // Devuelve el yaw del jugador para este cuadro: joystick derecho / mouse, o mira asistida.
    aim(G, dt) {
      const I = W.Input;
      const p = G.player;
      let yaw = null, manual = false;
      const a = I.aimVec;
      if (a.active && Math.hypot(a.x, a.y) > 18) {
        // pantalla: derecha = +x mundo, arriba = -z mundo (la cámara mira al norte)
        yaw = Math.atan2(-a.x, a.y);
        manual = true;
      } else if (!I.isTouch && I.mouse.px != null) {
        const pt = this.groundPoint(G, I.mouse.px, I.mouse.py, p.y + 1.2);
        if (pt) { yaw = Math.atan2(-(pt.x - p.pos.x), -(pt.z - p.pos.z)); manual = true; }
      }
      // objetivo asistido: el enemigo más cercano dentro del cono
      const want = I.held('fire') || this.time_since(G, p.lastShot) < 1.2 || manual;
      let target = null;
      if (want) {
        let bd = Infinity;
        const cone = manual ? this.SNAP_CONE : this.AUTO_AIM_CONE;
        const ref = yaw != null ? yaw : p.yaw;
        for (const e of G.enemies) {
          if (e.dead) continue;
          const dx = e.pos.x - p.pos.x, dz = e.pos.z - p.pos.z;
          let d = Math.hypot(dx, dz);
          if (d > this.AUTO_AIM_RANGE) continue;
          const ey = Math.atan2(-dx, -dz);
          let diff = Math.atan2(Math.sin(ey - ref), Math.cos(ey - ref));
          if (manual && Math.abs(diff) > cone) continue;
          if (!manual && Math.abs(diff) > cone && yaw == null && G.time - p.lastShot > 0.4) {
            // sin mando: cualquier enemigo cerca sirve, pero prefiere los del frente
            d += Math.abs(diff) * 6;
          }
          if (d < bd && G.lineOfSight(p.pos, p.y + 1.3, e.pos, 1.2)) { bd = d; target = e; }
        }
      }
      if (target) {
        const ty = Math.atan2(-(target.pos.x - p.pos.x), -(target.pos.z - p.pos.z));
        yaw = manual ? ty : Ent_lerpAngle(p.yaw, ty, Math.min(1, dt * 16));
      }
      this.target = target;
      if (yaw == null) {
        // sin mando ni objetivo: mira hacia donde camina
        const mv = I.move();
        if (Math.hypot(mv.x, mv.y) > 0.2) yaw = Math.atan2(-mv.x, mv.y);
        else yaw = p.yaw;
      }
      // marcador en el piso
      const show = manual || !!target || I.held('fire');
      this.aimLine.visible = show;
      if (show) {
        const len = target ? Math.hypot(target.pos.x - p.pos.x, target.pos.z - p.pos.z) : 6;
        this.aimLine.scale.y = len;
        this.aimLine.position.set(p.pos.x - Math.sin(yaw) * len / 2, p.y + 0.04, p.pos.z - Math.cos(yaw) * len / 2);
        this.aimLine.rotation.z = yaw;
        this.aimLine.material.color.set(target ? '#d6e48a' : '#ffffff');
        this.aimLine.material.opacity = target ? 0.9 : 0.35;
      }
      return yaw;
    },
    time_since(G, t) { return G.time - t; },

    // Punto del plano y=h bajo un píxel de pantalla.
    groundPoint(G, px, py, h) {
      const cam = G.camera;
      const v = this.lookVec.set((px / window.innerWidth) * 2 - 1, -(py / window.innerHeight) * 2 + 1, 0.5).unproject(cam);
      v.sub(cam.position).normalize();
      if (Math.abs(v.y) < 1e-4) return null;
      const t = (h - cam.position.y) / v.y;
      if (t < 0) return null;
      return { x: cam.position.x + v.x * t, z: cam.position.z + v.z * t };
    },

    // Rayo del disparo: sale del pecho del barista, horizontal, hacia donde mira.
    shotRay(G) {
      const p = G.player;
      const origin = new THREE.Vector3(p.pos.x, p.y + 1.3, p.pos.z);
      const dir = new THREE.Vector3(-Math.sin(p.yaw), 0, -Math.cos(p.yaw));
      if (this.target) {
        // apunta al torso del objetivo asistido, también en altura
        dir.set(this.target.pos.x - origin.x, this.target.y + 1.15 - origin.y, this.target.pos.z - origin.z).normalize();
      }
      return { origin, dir, minT: 0.7 };
    },

    // ---------- cámara ----------
    camera(G, dt) {
      const p = G.player;
      const cam = G.camera;
      // la cámara se adelanta un poco hacia donde apunta y hacia donde camina
      const lead = this.target ? 4.5 : 2.5;
      const tx = p.pos.x - Math.sin(p.yaw) * lead, tz = p.pos.z - Math.cos(p.yaw) * lead;
      if (!this.follow) this.follow = { x: tx, z: tz };
      const k = Math.min(1, dt * 4.5);
      this.follow.x += (tx - this.follow.x) * k;
      this.follow.z += (tz - this.follow.z) * k;
      const d = this.DIST, s = Math.sin(this.TILT), c = Math.cos(this.TILT);
      cam.position.set(this.follow.x, p.y + c * d, this.follow.z + s * d);
      if (G.shakeAmt > 0) {
        cam.position.x += (Math.random() - 0.5) * G.shakeAmt * 2;
        cam.position.z += (Math.random() - 0.5) * G.shakeAmt * 2;
        G.shakeAmt = Math.max(0, G.shakeAmt - dt * 2);
      }
      cam.lookAt(this.camVec.set(this.follow.x, p.y, this.follow.z));
      const targetFov = p.aiming || this.target ? 22 : p.sprinting ? 26 : this.FOV;
      if (Math.abs(targetFov - cam.fov) > 0.01) { cam.fov += (targetFov - cam.fov) * Math.min(1, dt * 6); cam.updateProjectionMatrix(); }
      this.placeDecor(G);
      // el sol acompaña al jugador para que su sombra siempre caiga dentro del mapa de sombras
      if (G.sun) { G.sun.position.set(p.pos.x - 60, p.y + 100, p.pos.z - 40); G.sun.target.position.set(p.pos.x, p.y, p.pos.z); }
      G.aimDir = this.shotRay(G).dir;
    },

    placeDecor(G) {
      const p = G.player;
      const gs = this.GRID;
      this.grid.position.set(Math.round(p.pos.x / gs) * gs, 0.02, Math.round(p.pos.z / gs) * gs);
      const ground = W.World.groundAt ? W.World.groundAt(p.pos.x, p.pos.z, p.y) : 0;
      this.blob.position.set(p.pos.x, ground + 0.03, p.pos.z);
      const h = Math.max(0, p.y - ground);
      const sc = Math.max(0.45, 1 - h * 0.12);
      this.blob.scale.set(sc, sc, 1);
      this.blob.material.opacity = 0.28 * sc;
    },

    // Lobby: el barista posa sobre el pad, visto desde arriba, con un giro lento.
    lobbyCamera(G) {
      const sp = W.World.hqSpot.spawn;
      const p = G.player;
      p.mesh.position.set(sp.x, 0.25, sp.z);
      p.mesh.rotation.y = 0.5 + Math.sin(G.time * 0.3) * 0.25;
      p.h.animate(G.time * 2, 0.05, true);
      const cam = G.camera;
      const d = 44, tilt = 14 * Math.PI / 180;
      const a = Math.sin(G.time * 0.18) * 0.5;
      cam.position.set(sp.x + Math.sin(a) * Math.sin(tilt) * d, 0.25 + Math.cos(tilt) * d, sp.z + Math.cos(a) * Math.sin(tilt) * d);
      cam.lookAt(this.camVec.set(sp.x, 0.6, sp.z - 1.5));
      if (cam.fov !== this.FOV) { cam.fov = this.FOV; cam.updateProjectionMatrix(); }
      this.grid.position.set(Math.round(sp.x / this.GRID) * this.GRID, 0.02, Math.round(sp.z / this.GRID) * this.GRID);
      if (G.sun) { G.sun.position.set(sp.x - 60, 100, sp.z - 40); G.sun.target.position.set(sp.x, 0, sp.z); }
    },

    // ---------- HUD ----------
    hud(G, dt) {
      const p = G.player;
      const H = W.HUD;
      const speed = this.lastPos ? Math.hypot(p.pos.x - this.lastPos.x, p.pos.z - this.lastPos.z) / Math.max(dt, 1e-3) : 0;
      this.lastPos = { x: p.pos.x, z: p.pos.z };
      this.speedS = (this.speedS || 0) + (speed - (this.speedS || 0)) * Math.min(1, dt * 8);
      const heading = Math.round((((-p.yaw * 180) / Math.PI) % 360 + 540) % 360 - 180);
      const threat = G.enemies.some((e) => !e.dead && e.alerted && Math.hypot(e.pos.x - p.pos.x, e.pos.z - p.pos.z) < 45);
      H.set('tdMode', 'text', threat ? 'en vivo · defensa' : 'en vivo · cosecha');
      H.set('tdMode', 'class', threat ? 'h threat' : 'h');
      H.set('tdSpeed', 'text', this.speedS.toFixed(1).replace('.', ','));
      H.set('tdHeading', 'text', `${heading}°`);
      H.set('tdHp', 'text', `${Math.ceil(Math.max(0, p.hp))}`);
      H.set('tdCherries', 'text', `${G.cherries}`);
      H.set('tdAmmo', 'text', p.reloading > 0 ? 'prensando…' : `${p.ammo} / ${p.magSize}`);
      H.set('tdFoes', 'text', `${G.enemies.filter((e) => !e.dead).length}`);
      const d = G.currentDistrict;
      if (d) H.set('tdPlace', 'text', d.name);
      H.set('tdHpBig', 'text', `${Math.ceil(Math.max(0, p.hp))}`);
      H.set('tdHpFill', 'width', `${Math.max(0, p.hp / p.maxHp) * 100}%`);
      H.set('tdHpFill', 'class', p.hp / p.maxHp < 0.3 ? 'low' : '');
      this.drawRadar(G);
    },

    drawRadar(G) {
      const cv = document.getElementById('tdRadar');
      if (!cv) return;
      const ctx = cv.getContext('2d');
      const Wd = cv.width, Hd = cv.height;
      const p = G.player;
      const night = this.theme === 'noche';
      ctx.clearRect(0, 0, Wd, Hd);
      // puntos del panel, como en el video
      ctx.fillStyle = night ? '#5d6663' : '#b5bbb8';
      for (let i = 0; i < 7; i++) for (let j = 0; j < 4; j++) { ctx.beginPath(); ctx.arc(5 + i * 20, 6 + j * 22, 1.3, 0, Math.PI * 2); ctx.fill(); }
      ctx.fillStyle = night ? '#3b4240' : '#d7dcd9';
      ctx.fillRect(5, Hd - 10, Wd - 10, 1);
      // escala: 130 px ≈ 90 m
      const k = (Wd - 10) / 90;
      const cx = Wd / 2, cz = Hd / 2 - 4;
      const dot = (x, z, color, s) => {
        const dx = (x - p.pos.x) * k, dz = (z - p.pos.z) * k;
        if (Math.abs(dx) > Wd / 2 - 4 || Math.abs(dz) > Hd / 2 - 6) return;
        ctx.fillStyle = color;
        ctx.fillRect(cx + dx - s / 2, cz + dz - s / 2, s, s);
      };
      let n = 0;
      const hq = W.World.hqSpot;
      dot(hq.x, hq.z, '#4f8f2c', 7);
      for (const pl of G.planters) if (pl.ripe) dot(pl.pos.x, pl.pos.z, '#8fbf45', 4);
      for (const l of G.loot) dot(l.pos.x, l.pos.z, '#d6e48a', 4);
      for (const z of G.zones) dot(z.pos.x, z.pos.z, z.owner === 'fg' ? '#4f8f2c' : '#2a22f5', 6);
      for (const e of G.enemies) if (!e.dead) { n++; dot(e.pos.x, e.pos.z, '#2a22f5', e.boss ? 9 : 6); }
      ctx.fillStyle = night ? '#ffffff' : '#2c302e';
      ctx.fillRect(cx - 3, cz - 3, 6, 6);
      const label = document.getElementById('tdRadarN');
      if (label) { const t = `${n} enem.`; if (label.textContent !== t) label.textContent = t; }
    },
  };

  function Ent_lerpAngle(a, b, t) { return W.Ent.lerpAngle(a, b, t); }

  W.TopDown = TD;
})();
