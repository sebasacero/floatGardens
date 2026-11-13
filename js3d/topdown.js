// Vista cenital (rediseño v3, guía de diseño v2): cámara casi vertical de lente larga sobre la terraza,
// mira asistida tipo twin-stick y el HUD del kit de la web (riel, objetivo, Vida y Agua, marcadores de 1,5 px,
// tarjeta verde, panel azul solo para Arcadia, franja de métricas). Se activa sola en pantallas táctiles;
// en PC se elige en el lobby o en pausa. Este módulo también lleva la LUZ del juego para las dos cámaras:
// día (maqueta blanca al sol) y noche (la misma maqueta con menos luz y regletas cálidas sobre las filas).
// El mundo 3D, los enemigos y el balance son los mismos: solo cambian cámara, mira, controles y HUD.
(function () {
  const KEY = 'fg.cenital', KEY_THEME = 'fg.cenital.tema';
  const read = (k, def) => { try { const v = localStorage.getItem(k); return v === null ? def : v; } catch (e) { return def; } };
  const write = (k, v) => { try { localStorage.setItem(k, v); } catch (e) { /* modo privado */ } };

  const THEMES = {
    // noche: la misma maqueta blanca con menos luz (cielo oscuro, sol de luna frío, exposición baja)
    noche: {
      sky: '#161b20', zenith: '#0d1013', fogNear: 90, fogFar: 520,
      ground: '#ffffff', paving: '#ffffff', grid: 'rgba(255,255,255,0.0)',
      hemi: { sky: '#a9b6cc', ground: '#1e2429', intensity: 0.32 }, sun: 0.3, sunColor: '#d6e0f5', exposure: 0.72, grow: 18,
    },
    dia: {
      sky: '#e3e9e8', zenith: '#c5d3dc', fogNear: 110, fogFar: 650,
      ground: '#ffffff', paving: '#ffffff', grid: 'rgba(44,48,46,0.0)',
      hemi: { sky: '#eef4ff', ground: '#b7c2a8', intensity: 1.0 }, sun: 1.6, sunColor: '#fff4e4', exposure: 1.12, grow: 0,
    },
  };

  const TD = {
    enabled: false,
    theme: 'noche',
    // cámara
    FOV: 22, DIST: 58, TILT: 9 * Math.PI / 180,
    AUTO_AIM_RANGE: 42, AUTO_AIM_CONE: Math.PI / 5, SNAP_CONE: 0.2,
    GRID: 8,

    load(isTouch) {
      const saved = read(KEY, null);
      this.enabled = saved === null ? !!isTouch : saved === '1';
      // sin preferencia guardada: noche en el celular (el estilo del video), día en PC
      const def = isTouch ? 'noche' : 'dia';
      this.theme = THEMES[read(KEY_THEME, def)] ? read(KEY_THEME, def) : def;
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
      // cielo: cúpula con degradado del horizonte (color de la niebla) al cenit; sigue a la cámara
      const sc = document.createElement('canvas');
      sc.width = 4; sc.height = 256;
      this.skyCtx = sc.getContext('2d');
      this.skyTex = new THREE.CanvasTexture(sc);
      this.skyDome = new THREE.Mesh(new THREE.SphereGeometry(820, 24, 12), new THREE.MeshBasicMaterial({ map: this.skyTex, side: THREE.BackSide, fog: false, depthWrite: false }));
      this.skyDome.renderOrder = -10;
      this.skyDome.frustumCulled = false;
      scene.add(this.skyDome);
      // destello del disparo
      this.flash = new THREE.PointLight('#ffd9a8', 0, 10, 2);
      scene.add(this.flash);
      // regletas de cultivo sobre las filas A, B y C: cálidas, solo encienden de noche
      this.growLights = [];
      const hq = W.World.hqSpot;
      if (hq) for (const dx of [-11, -3.5, 4]) { const L = new THREE.PointLight('#fff1d6', 0, 11, 1.6); L.position.set(hq.x + dx, 3.2, hq.z + 8.4); scene.add(L); this.growLights.push(L); }
      // íconos del kit y botones del riel (mapa, mejoras, puntos, pausa) y del panel azul (marcar drones)
      if (window.fgIcons) fgIcons(document.getElementById('td'));
      document.getElementById('td').addEventListener('click', (e) => {
        const b = e.target.closest('[data-td]');
        if (b) this.railAction(G, b.dataset.td);
      });
      this.apply(G);
    },

    // Botones del riel y del panel azul en el juego
    railAction(G, a) {
      if (G.state !== 'playing') return;
      if (a === 'map') { G.showMap = !G.showMap; W.HUD.setBigMap(G.showMap); }
      else if (a === 'pause') G.pause();
      else if (a === 'shop') { G.pause(); W.Menus.returnTo = 'pause'; W.Menus.show('shop', { from: 'game' }); }
      else if (a === 'rewards') { G.pause(); W.Menus.returnTo = 'pause'; W.Menus.show('rewards'); }
      else if (a === 'mark') { this.markUntil = G.time + 8; }
    },

    drawSky(T, rain) {
      const g = this.skyCtx;
      const grad = g.createLinearGradient(0, 0, 0, 256);
      const top = rain ? '#77848f' : T.zenith, hor = rain ? '#8e9ba6' : T.sky;
      grad.addColorStop(0, top); grad.addColorStop(0.42, hor); grad.addColorStop(1, hor);
      g.fillStyle = grad; g.fillRect(0, 0, 4, 256);
      this.skyTex.needsUpdate = true;
    },
    // Cielo y niebla según tema y clima (la lluvia solo cambia el cielo de día)
    sky(G) {
      const T = THEMES[this.theme];
      const rain = G.raining && this.theme === 'dia';
      const c = rain ? '#8e9ba6' : T.sky;
      G.scene.background.set(c);
      G.scene.fog.color.set(c);
      G.scene.fog.near = this.enabled ? T.fogNear : 55;
      G.scene.fog.far = this.enabled ? T.fogFar : rain ? 320 : 480;
      this.drawSky(T, rain);
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
      const night = this.theme === 'noche';
      const body = document.body;
      body.classList.toggle('cenital', on);
      body.classList.toggle('cenital-noche', on && night);
      body.classList.toggle('cenital-dia', on && !night);
      body.classList.toggle('fg-noche', night);
      W.Input.topdown = on;
      if (this.grid) this.grid.visible = on;
      if (this.aimLine) this.aimLine.visible = false;
      if (on) this.drawGrid(T.grid);
      // luces, cielo, niebla y piso: iguales en las dos cámaras
      this.sky(G);
      if (G.hemi) { G.hemi.color.set(T.hemi.sky); G.hemi.groundColor.set(T.hemi.ground); G.hemi.intensity = T.hemi.intensity; }
      if (G.sun) {
        G.sun.intensity = T.sun; G.sun.color.set(T.sunColor);
        // la cenital ve ~30 m alrededor; la tercera persona necesita sombras más lejos
        const r = on ? 34 : 62, sc = G.sun.shadow.camera;
        if (sc.right !== r) { sc.left = -r; sc.right = r; sc.top = r; sc.bottom = -r; sc.updateProjectionMatrix(); }
      }
      if (!on) this.clearTags();
      if (W.World.ground) W.World.ground.material.color.set(T.ground);
      // la maqueta no cambia de color de noche: solo hay menos luz
      const mats = W.World.mats || {};
      for (const k of ['sidewalk', 'cobble', 'grass', 'tile']) if (mats[k]) mats[k].color.set(T.paving);
      if (G.renderer) G.renderer.toneMappingExposure = T.exposure;
      if (this.growLights) for (const L of this.growLights) L.intensity = T.grow;
      // el chorro del rociador: lima de día (lo que apuntas), blanco de noche
      if (W.Ent.tracerMat) W.Ent.tracerMat.color.set(night ? '#ffffff' : '#d6e48a');
      if (!on && G.camera) this.leaveLobby(G);
      if (G.camera) {
        G.camera.fov = on ? this.FOV : 70;
        G.camera.updateProjectionMatrix();
      }
      this.follow = null;
      this.labelButtons(on);
      if (on) W.Input.unlock();
    },

    // El sol acompaña al punto de interés para que las sombras siempre caigan dentro del mapa de sombras.
    // En cenital entra desde atrás (noroccidente); en tercera persona, de lado, para que las fachadas que ves tengan luz.
    placeSun(G, x, y, z) {
      if (!G.sun) return;
      if (this.enabled) G.sun.position.set(x - 60, y + 100, z - 40); else G.sun.position.set(x - 62, y + 96, z + 44);
      G.sun.target.position.set(x, y, z);
      if (this.skyDome) this.skyDome.position.copy(G.camera.position);
      if (this.flash && this.flash.intensity > 0) this.flash.intensity = Math.max(0, this.flash.intensity - 0.35);
    },
    muzzleFlash(at) { this.flash.position.copy(at); this.flash.intensity = 2.2; },

    // Los botones táctiles cambian de texto: en cenital son el bloque de cuatro acciones del kit.
    labelButtons(on) {
      const L = { tFire: ['disparar', 'Rociar'], tJump: ['saltar', 'Impulso'], tReload: ['recargar', 'Llenar'], tAction: ['acción', 'Cosechar'], tAim: ['apuntar', 'fijar'] };
      for (const id in L) {
        const el = document.getElementById(id);
        if (!el) continue;
        const t = on ? L[id][1] : L[id][0];
        if (el.firstChild && el.firstChild.nodeType === 3) el.firstChild.nodeValue = t; else el.textContent = t;
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
        dir.set(this.target.pos.x - origin.x, this.target.mesh.position.y + (this.target.pest ? this.target.alt : this.target.boss ? 3 : 1.3) - origin.y, this.target.pos.z - origin.z).normalize();
      }
      return { origin, dir, minT: 0.7 };
    },

    // ---------- cámara ----------
    camera(G, dt) {
      const p = G.player;
      const cam = G.camera;
      // la cámara se adelanta un poco hacia donde apunta y hacia donde camina
      if (cam.view && cam.view.enabled) this.leaveLobby(G);
      const lead = this.target ? 4.5 : 2.5;
      const tx = p.pos.x - Math.sin(p.yaw) * lead, tz = p.pos.z - Math.cos(p.yaw) * lead;
      if (!this.follow) this.follow = { x: tx, z: tz };
      const k = Math.min(1, dt * 4.5);
      this.follow.x += (tx - this.follow.x) * k;
      this.follow.z += (tz - this.follow.z) * k;
      // celular vertical: la cámara sube (se ve más terraza a los lados) y encuadra al barista en el tercio alto,
      // porque los controles ocupan la parte de abajo
      const portrait = cam.aspect < 0.8;
      const d = portrait ? this.DIST * 1.32 : this.DIST, s = Math.sin(this.TILT), c = Math.cos(this.TILT);
      const bias = portrait ? 4.5 : 0;
      cam.position.set(this.follow.x, p.y + c * d, this.follow.z + bias + s * d);
      if (G.shakeAmt > 0) {
        cam.position.x += (Math.random() - 0.5) * G.shakeAmt * 2;
        cam.position.z += (Math.random() - 0.5) * G.shakeAmt * 2;
        G.shakeAmt = Math.max(0, G.shakeAmt - dt * 2);
      }
      cam.lookAt(this.camVec.set(this.follow.x, p.y, this.follow.z + bias));
      const targetFov = p.aiming || this.target ? 22 : p.sprinting ? 26 : this.FOV;
      if (Math.abs(targetFov - cam.fov) > 0.01) { cam.fov += (targetFov - cam.fov) * Math.min(1, dt * 6); cam.updateProjectionMatrix(); }
      this.placeDecor(G);
      G.aimDir = this.shotRay(G).dir;
    },

    placeDecor(G) {
      const p = G.player;
      const gs = this.GRID;
      this.grid.position.set(Math.round(p.pos.x / gs) * gs, 0.02, Math.round(p.pos.z / gs) * gs);
    },

    // Lobby: la torre colgante de la sede vista desde arriba y de lado, como el render de la web, con un giro lento.
    // En el celular la maqueta se ve por la "ventana" del lobby (arriba), así que la cámara se descentra hacia allá.
    lobbyCamera(G) {
      const hq = W.World.hqSpot;
      const sp = hq.spawn;
      const p = G.player;
      p.mesh.position.set(sp.x, 0.25, sp.z);
      p.mesh.rotation.y = 0.5 + Math.sin(G.time * 0.3) * 0.25;
      p.h.animate(G.time * 2, 0.05, true);
      const cam = G.camera;
      const d = 150, tilt = 38 * Math.PI / 180;
      const a = -0.62 + Math.sin(G.time * 0.12) * 0.18;
      const tx = hq.x + 2, ty = 14, tz = hq.z - 3;
      cam.position.set(tx + Math.sin(a) * Math.sin(tilt) * d, ty + Math.cos(tilt) * d, tz + Math.cos(a) * Math.sin(tilt) * d);
      cam.lookAt(this.camVec.set(tx, ty, tz));
      cam.translateX(8);
      if (cam.fov !== 26) { cam.fov = 26; cam.updateProjectionMatrix(); }
      const frame = document.querySelector('.menu-lobbyTd .frame');
      if (frame) {
        // la ventana del lobby es "la pantalla": la cámara encuadra la torre en ella y el lienzo pinta lo que sobra alrededor
        const r = frame.getBoundingClientRect();
        const Wd = window.innerWidth, Hd = window.innerHeight;
        const fw = Math.round(r.width), fh = Math.round(r.height), ox = -Math.round(r.left), oy = -Math.round(r.top);
        if (fw > 0 && fh > 0 && (!cam.view || !cam.view.enabled || cam.view.offsetX !== ox || cam.view.offsetY !== oy || cam.view.fullWidth !== fw || cam.view.width !== Wd)) {
          cam.aspect = fw / fh;
          cam.setViewOffset(fw, fh, ox, oy, Wd, Hd);
        }
      } else if (cam.view && cam.view.enabled) this.leaveLobby(G);
      this.grid.position.set(Math.round(sp.x / this.GRID) * this.GRID, 0.02, Math.round(sp.z / this.GRID) * this.GRID);
    },
    leaveLobby(G) {
      const cam = G.camera;
      if (!cam || !cam.view || !cam.view.enabled) return;
      cam.clearViewOffset();
      cam.aspect = window.innerWidth / window.innerHeight;
      cam.updateProjectionMatrix();
    },

    // ---------- HUD (kit de la web) ----------
    hud(G, dt) {
      const p = G.player;
      const H = W.HUD;
      const hp = Math.ceil(Math.max(0, p.hp));
      H.set('tdHp', 'text', `${hp}`);
      H.set('tdHpMax', 'text', `/${p.maxHp}`);
      H.set('tdHpFill', 'width', `${Math.max(0, p.hp / p.maxHp) * 100}%`);
      H.set('tdHpFill', 'class', p.hp / p.maxHp < 0.3 ? 'low' : '');
      // Agua: el tanque del rociador (los chorros de café a presión)
      H.set('tdAmmo', 'text', p.reloading > 0 ? '…' : `${p.ammo}`);
      H.set('tdAmmoMax', 'text', `/${p.magSize}`);
      H.set('tdAmmoFill', 'width', `${p.reloading > 0 ? (1 - p.reloading / p.reloadTime) * 100 : (p.ammo / p.magSize) * 100}%`);
      H.set('tdAmmoFill', 'class', p.ammo <= 3 && !(p.reloading > 0) ? 'low' : '');
      H.set('tEnFill', 'width', `${p.energy}%`);
      H.set('tEnFill', 'class', p.energy < W.CONFIG.DASH_COST ? 'empty' : '');
      H.set('tdPts', 'text', G.save.puntos.toLocaleString('es-CO'));
      H.set('tdCherries', 'text', `${G.cherries}`);
      H.set('tdZones', 'text', `${G.capturedCount()}`);
      H.set('tdZonesMax', 'text', `/${G.zones.length}`);
      // objetivo: una línea, con la distancia en mono
      const obj = G.objective();
      H.set('tdObj', 'text', obj.title);
      H.set('tdObjN', 'text', obj.target ? `${Math.round(Math.hypot(obj.target.x - p.pos.x, obj.target.z - p.pos.z))} m` : `${G.capturedCount()}/${G.zones.length}`);
      H.set('tdDot', 'class', G.save.puntos >= FG.CONFIG.REDEEM_COST ? 'dot on' : 'dot');
      this.updateAlert(G);
      this.updateSignal(G, dt);
      this.updateTags(G);
    },

    // Panel azul: solo cuando Arcadia actúa (liberando una zona, jefe o drones en alerta cerca)
    updateAlert(G) {
      const H = W.HUD;
      const p = G.player;
      const cz = G.capturing, b = G.boss && !G.boss.dead ? G.boss : null;
      const near = G.enemies.filter((e) => !e.dead && !e.pest && e.alerted && Math.hypot(e.pos.x - p.pos.x, e.pos.z - p.pos.z) < 45);
      const pests = G.enemies.filter((e) => !e.dead && e.pest && Math.hypot(e.pos.x - p.pos.x, e.pos.z - p.pos.z) < 40);
      let on = true;
      if (cz) {
        H.set('tdAlertCode', 'text', cz.zone.id || 'zona');
        H.set('tdAlertTitle', 'text', cz.contested ? `Arcadia defiende ${cz.zone.name}` : `Liberando ${cz.zone.name}`);
        H.set('tdAlertL1', 'text', 'Liberada');
        H.set('tdAlertV1', 'text', `${Math.round(cz.zone.progress * 100)} %`);
        H.set('tdAlertV2', 'text', `${near.length}`);
      } else if (b) {
        H.set('tdAlertCode', 'text', 'jefe');
        H.set('tdAlertTitle', 'text', W.BOSS.name);
        H.set('tdAlertL1', 'text', 'Vida');
        H.set('tdAlertV1', 'text', `${Math.round((b.hp / b.maxHp) * 100)} %`);
        H.set('tdAlertV2', 'text', `${near.length}`);
      } else if (pests.length) {
        H.set('tdAlertCode', 'text', 'plaga');
        H.set('tdAlertTitle', 'text', `${pests[0].name || 'Plaga'} va por tus matas`);
        H.set('tdAlertL1', 'text', 'Con roya');
        H.set('tdAlertV1', 'text', `${G.planters.filter((x) => x.roya > 0).length}`);
        H.set('tdAlertV2', 'text', `${near.length}`);
      } else if (near.length) {
        H.set('tdAlertCode', 'text', near[0].serial || 'dron');
        H.set('tdAlertTitle', 'text', near.length > 1 ? 'Drones de Arcadia encima' : 'Un dron de Arcadia te vio');
        H.set('tdAlertL1', 'text', 'Vida');
        H.set('tdAlertV1', 'text', `${Math.ceil(Math.max(0, p.hp))}`);
        H.set('tdAlertV2', 'text', `${near.length}`);
      } else on = false;
      H.toggle('tdAlert', 'on', on);
      H.toggle('tdAlertAck', 'on', this.markUntil > G.time);
    },

    // Señal de Arcadia: barras de la franja; las de la derecha se vuelven azules según cuántos drones hay cerca
    updateSignal(G, dt) {
      const el = document.getElementById('tdSig');
      if (!el) return;
      if (!el.children.length) for (let i = 0; i < 22; i++) el.appendChild(document.createElement('i'));
      const p = G.player;
      let n = 0, hot = 0;
      for (const e of G.enemies) if (!e.dead) { const d = Math.hypot(e.pos.x - p.pos.x, e.pos.z - p.pos.z); if (d < 90) { n++; if (d < 40) hot++; } }
      const k = Math.min(22, n * 3 + hot * 4);
      this.sigT = (this.sigT || 0) + dt;
      if (this.sigT < 0.12) return;
      this.sigT = 0;
      for (let i = 0; i < 22; i++) {
        const c = el.children[i];
        const b = i >= 22 - k;
        const h = b ? 45 + ((i * 37 + Math.floor(G.time * 3)) % 50) : 22 + ((i * 13) % 14);
        c.style.height = `${h}%`;
        if (b !== c.classList.contains('b')) c.classList.toggle('b', b);
      }
    },

    // Marcadores sobre el mundo: cuadro fino sobre cada mata, dron, plaga y costal cercano (lima = al alcance,
    // azul = Arcadia); la fila al alcance abre la tarjeta verde.
    tags: new Map(),
    clearTags() { for (const el of this.tags.values()) el.remove(); this.tags.clear(); W.HUD.toggle('tdCard', 'on', false); },
    updateTags(G) {
      const root = document.getElementById('tdTags');
      if (!root) return;
      const p = G.player;
      const cam = G.camera;
      const v = this.tagVec || (this.tagVec = new THREE.Vector3());
      const v2 = this.tagVec2 || (this.tagVec2 = new THREE.Vector3());
      const W2 = window.innerWidth / 2, H2 = window.innerHeight / 2;
      // distribución según la pantalla: en horizontal (pantalla baja) las tarjetas de arriba ocupan menos alto
      const land = W2 > H2 && H2 * 2 <= 520;
      this.land = land;
      const top = (land ? 112 : 150) + (parseFloat(getComputedStyle(document.body).getPropertyValue('--sat')) || 0);
      // dónde queda el barista en pantalla, para que la tarjeta verde no lo tape
      v.set(p.pos.x, p.y + 1, p.pos.z).project(cam);
      this.meAt = { x: v.x * W2 + W2, y: -v.y * H2 + H2 };
      const seen = new Set();
      // proyecta el centro y el radio: el cuadro crece y encoge con la lente
      const put = (key, x, y, z, r, label, cls) => {
        let el = this.tags.get(key);
        if (!el) { el = document.createElement('div'); el.className = 'td-mark'; el.appendChild(document.createElement('span')); root.appendChild(el); this.tags.set(key, el); }
        if (el.dataset.h !== label) { el.firstChild.textContent = label; el.dataset.h = label; el.firstChild.style.display = label ? '' : 'none'; }
        if (el.dataset.c !== cls) { el.className = `td-mark ${cls}`; el.dataset.c = cls; }
        v.set(x, y, z).project(cam);
        v2.set(x + r, y, z).project(cam);
        const sx = v.x * W2 + W2, sy = -v.y * H2 + H2;
        const px = Math.max(18, Math.abs(v2.x - v.x) * W2 * 2);
        const hide = v.z > 1 || (sy < top && (sx < 200 || sx > W2 * 2 - 180)) || (land && sy < 52) ? 'none' : '';
        if (el.style.display !== hide) el.style.display = hide;
        el.style.width = el.style.height = `${px.toFixed(0)}px`;
        el.style.transform = `translate(${(sx - px / 2).toFixed(1)}px, ${(sy - px / 2).toFixed(1)}px)`;
        seen.add(key);
        return { sx, sy, px };
      };
      const near = (o, r) => Math.abs(o.x - p.pos.x) < r && Math.abs(o.z - p.pos.z) < r;
      // la fila al alcance (la que abre la tarjeta verde): la misma que usa la acción "Cosechar"
      let card = null, cardAt = null;
      for (const pl of G.planters) {
        if (!near(pl.pos, 26)) continue;
        const pct = Math.round(Math.min(1, pl.growth) * 100);
        const d = Math.hypot(pl.pos.x - p.pos.x, pl.pos.z - p.pos.z);
        const reach = d < pl.reach;
        const name = pl.label ? `Fila ${pl.label}` : pl.variety.name;
        let label, cls = '';
        if (pl.roya > 0) { label = `${name} · roya ${Math.round(pl.roya * 100)} %`; cls = 'sick'; }
        else if (pl.ripe) { label = `${name} · lista`; }
        else label = `${name} · ${pct} %`;
        if (reach && !card) { cls += ' on'; card = pl; }
        const at = put(pl, pl.pos.x, pl.mesh.position.y + 0.6, pl.pos.z, pl.bed ? 1.6 : 0.9, reach ? '' : label, cls);
        if (card === pl) cardAt = at;
      }
      const mk = this.markUntil > G.time;
      for (const e of G.enemies) {
        if (e.dead || !near(e.pos, mk ? 90 : 40)) continue;
        const d = Math.round(Math.hypot(e.pos.x - p.pos.x, e.pos.z - p.pos.z));
        if (e.pest) put(e, e.pos.x, e.mesh.position.y + e.alt, e.pos.z, e.hitR || 0.6, `${e.serial} · ${d} m`, 'foe up');
        else if (e.boss) put(e, e.pos.x, e.mesh.position.y + 2.6, e.pos.z, 1.8, `${W.BOSS.name} · ${Math.ceil(e.hp)}`, 'foe up');
        else put(e, e.pos.x, e.mesh.position.y + 1.3, e.pos.z, 0.7, `${e.serial} · ${d} m`, e === this.target ? 'foe' : 'foe up');
      }
      for (const l of G.loot) {
        if (l.kind !== 'crate' || !near(l.pos, 26)) continue;
        put(l, l.pos.x, 0.55, l.pos.z, 0.7, `Costal · ${l.amount} cerezas`, 'up');
      }
      for (const [key, el] of this.tags) if (!seen.has(key)) { el.remove(); this.tags.delete(key); }
      this.updateCard(G, card, cardAt);
    },

    // Tarjeta verde de la fila al alcance: variedad, madurez y qué hacer (cosechar, tratar la roya, esperar)
    updateCard(G, pl, at) {
      const H = W.HUD;
      H.toggle('tdCard', 'on', !!pl);
      if (!pl) return;
      const pct = Math.round(Math.min(1, pl.growth) * 100);
      H.set('tdCardH', 'text', `${pl.label ? `Fila ${pl.label} · ` : ''}${pl.variety.name}`);
      H.set('tdCardSub', 'text', `${W.HQ.name} · ${pl.ripe ? `${G.plantYield ? G.plantYield(pl) : ''} cerezas` : 'tu terraza'}`);
      H.set('tdCardBig', 'text', pl.roya > 0 ? `${Math.round(pl.roya * 100)}` : `${pct}`);
      H.set('tdCardEta', 'text', pl.roya > 0 ? 'Con roya · Cosechar la trata' : pl.ripe ? 'Lista para cosechar' : pct > 70 ? 'Casi lista · sigue creciendo' : 'Creciendo');
      const bars = document.getElementById('tdCardBars');
      if (bars && !bars.children.length) for (let i = 0; i < 24; i++) bars.appendChild(document.createElement('i'));
      if (bars) { const k = Math.round(pct / 100 * 24); for (let i = 0; i < 24; i++) { const c = bars.children[i]; const hi = i < k; const h = hi ? 40 + ((i * 29) % 55) : 22 + ((i * 11) % 12); if (c.style.height !== `${h}%`) c.style.height = `${h}%`; if (hi !== c.classList.contains('hi')) c.classList.toggle('hi', hi); } }
      const el = document.getElementById('tdCard');
      const Wd = window.innerWidth, Hd = window.innerHeight;
      const cs = getComputedStyle(document.body);
      const gl = Math.max(16, parseFloat(cs.getPropertyValue('--gl')) || 16), gr = Math.max(16, parseFloat(cs.getPropertyValue('--gr')) || 16);
      const sat = parseFloat(cs.getPropertyValue('--sat')) || 0, sab = parseFloat(cs.getPropertyValue('--sab')) || 0;
      // horizontal: acoplada bajo Vida y Agua, encima del joystick (no cabe junto a la mata sin tapar el juego)
      if (this.land) {
        if (!el.classList.contains('dock')) el.classList.add('dock');
        H.set('tdCard', 'transform', `translate(${gl.toFixed(0)}px, ${(116 + sat).toFixed(0)}px)`);
        return;
      }
      if (el.classList.contains('dock')) el.classList.remove('dock');
      // vertical: junto al marcador, dentro de la zona libre (bajo las tarjetas, sobre los controles),
      // sin tapar al barista ni al panel azul
      const cw = el.offsetWidth || 140, ch = el.offsetHeight || 150;
      const stick = document.getElementById('stick');
      const ctrlTop = stick && stick.offsetHeight ? stick.getBoundingClientRect().top - 27 : Hd - 229 - sab; // tope del bloque de acciones
      const y0 = 214 + sat, y1 = Math.max(y0, ctrlTop - 10 - ch);
      const x0 = gl, x1 = Wd - gr - cw;
      const me = this.meAt || { x: Wd / 2, y: Hd / 2 };
      const al = document.getElementById('tdAlert');
      const blocks = [{ l: me.x - 30, r: me.x + 30, t: me.y - 44, b: me.y + 30 }];
      if (al && al.classList.contains('on')) blocks.push({ l: Wd - gr - al.offsetWidth - 6, r: Wd, t: al.offsetTop, b: al.offsetTop + al.offsetHeight + 6 });
      const hits = (x, y) => blocks.some((k) => x < k.r && x + cw > k.l && y < k.b && y + ch > k.t);
      const clampX = (x) => Math.max(x0, Math.min(x1, x)), clampY = (y) => Math.max(y0, Math.min(y1, y));
      const xr = clampX(at.sx + at.px / 2 + 14), xl = clampX(at.sx - at.px / 2 - 14 - cw);
      const ys = [clampY(at.sy - 24), clampY(me.y + 36), clampY(me.y - 50 - ch), y1, y0];
      let x = xr, y = ys[0], ok = false;
      for (const cy of ys) { for (const cx of (at.sx + at.px / 2 + 14 + cw > Wd - gr ? [xl, xr] : [xr, xl])) { if (!hits(cx, cy)) { x = cx; y = cy; ok = true; break; } } if (ok) break; }
      if (!ok) { x = x0; y = y1; }
      H.set('tdCard', 'transform', `translate(${x.toFixed(0)}px, ${y.toFixed(0)}px)`);
    },
  };

  function Ent_lerpAngle(a, b, t) { return W.Ent.lerpAngle(a, b, t); }

  W.TopDown = TD;
})();
