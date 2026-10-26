// Float Gardens: Terrazas de Kennedy — plataformas 2.5D.
// Física de plataforma (tiempo coyote, salto sostenido, rodar), cámara lateral, enemigos, tanques-cañón,
// andamios, tejas que se caen, la tormenta que persigue, HUD, controles (teclado, táctil y control) y sonido.
(function () {
  const FG = window.FGT;
  const $ = (id) => document.getElementById(id);
  // las texturas de los avisos usan las fuentes de la página: espera a que carguen (máximo 2.5 s)
  const fonts = document.fonts ? Promise.all([document.fonts.load('40px "Lilita One"'), document.fonts.load('20px "Baloo 2"')]) : Promise.resolve();
  Promise.race([fonts, new Promise((r) => setTimeout(r, 2500))]).catch(() => {}).then(main);

  function main() {

    // ---------- física (metros, segundos) ----------
    const FISICA = {
      correr: 9.5, rodar: 15, accelSuelo: 75, accelAire: 42, frenado: 60,
      salto: 15.5, gravSostenido: 30, gravSubida: 62, gravCaida: 55, caidaMax: 28,
      coyote: 0.1, buffer: 0.13, colchon: 19.5, pisoton: 12,
      ancho: 0.8, alto: 1.6, altoRodando: 1.0, tiempoRodar: 0.38,
    };
    const STEP = 1 / 120;

    // ---------- render ----------
    const canvas = $('c');
    const renderer = new THREE.WebGLRenderer({ canvas, antialias: true, powerPreference: 'high-performance' });
    renderer.outputEncoding = THREE.sRGBEncoding;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.0;
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    const mobile = matchMedia('(pointer: coarse)').matches;
    renderer.setPixelRatio(Math.min(devicePixelRatio, mobile ? 1.5 : 2));

    const scene = new THREE.Scene();
    const HORIZON = new THREE.Color('#f3d9b8');
    scene.fog = new THREE.Fog(HORIZON, 220, 7000);
    const camera = new THREE.PerspectiveCamera(38, 1, 0.3, 9000);

    // cielo de tarde bogotana: azul arriba, durazno en el horizonte
    const sunDir = new THREE.Vector3(-0.45, 0.55, 0.7).normalize();
    const sky = new THREE.Mesh(new THREE.SphereGeometry(6000, 32, 16), new THREE.ShaderMaterial({
      side: THREE.BackSide, fog: false, depthWrite: false,
      uniforms: { top: { value: new THREE.Color('#2f6fd6') }, mid: { value: new THREE.Color('#8fc0f0') }, hor: { value: HORIZON.clone() }, sunDir: { value: sunDir }, sunC: { value: new THREE.Color('#ffe2b0') } },
      vertexShader: 'varying vec3 vP;void main(){vP=normalize(position);gl_Position=projectionMatrix*modelViewMatrix*vec4(position,1.);}',
      fragmentShader: `uniform vec3 top,mid,hor,sunDir,sunC;varying vec3 vP;void main(){float h=vP.y;vec3 c=mix(hor,mid,smoothstep(0.,.16,h));c=mix(c,top,smoothstep(.16,.65,h));
        float s=max(dot(vP,normalize(sunDir)),0.);c+=sunC*(pow(s,60.)*.9+pow(s,6.)*.18);gl_FragColor=vec4(c,1.);}`,
    }));
    scene.add(sky);

    const hemi = new THREE.HemisphereLight('#cfe4ff', '#a08a6a', 0.75); scene.add(hemi);
    const sun = new THREE.DirectionalLight('#fff0d6', 2.1);
    sun.castShadow = true;
    sun.shadow.mapSize.set(mobile ? 1024 : 2048, mobile ? 1024 : 2048);
    Object.assign(sun.shadow.camera, { left: -34, right: 34, top: 26, bottom: -26, near: 1, far: 220 });
    sun.shadow.bias = -0.0004; sun.shadow.normalBias = 0.04;
    scene.add(sun, sun.target);

    // ---------- nivel y mundo ----------
    const lvl = FG.buildLevel();
    const world = FG.buildWorld(scene, lvl);

    // ---------- jugador ----------
    const P = {
      x: 0, y: 0, vx: 0, vy: 0, face: 1, ground: null, onGround: false, coyote: 0, buffer: 0, roll: 0, rollCd: 0, longJump: false,
      hp: 3, inv: 0, state: 'normal', phase: 0, squash: 1, lastGroundY: 0, tank: null, flight: null, deadT: 0,
    };
    const playerMesh = FG.makePlayer(); scene.add(playerMesh);
    const blob = new THREE.Mesh(new THREE.CircleGeometry(0.5, 16), new THREE.MeshBasicMaterial({ color: '#000', transparent: true, opacity: 0.25, depthWrite: false }));
    blob.rotation.x = -Math.PI / 2; scene.add(blob);

    // ---------- entidades ----------
    const solids = lvl.solidos.slice();
    const ents = { cerezas: [], letras: [], palomas: [], drones: [], tanques: [], colchones: [], andamios: [], tejas: [], cps: [], meta: null };
    for (const e of lvl.ents) {
      if (e.t === 'cereza') ents.cerezas.push({ x: e.x, y: e.y, got: false });
      else if (e.t === 'letra') { const m = FG.makeLetter(e.ch); m.position.set(e.x, e.y, 0); scene.add(m); ents.letras.push({ ...e, m, got: false }); }
      else if (e.t === 'paloma') { const m = FG.makePigeon(); scene.add(m); ents.palomas.push({ ...e, m, x: (e.x0 + e.x1) / 2, dir: 1, alive: true, dieT: 0, vy: 0, py: e.y }); }
      else if (e.t === 'dron') { const m = FG.makeDrone(); scene.add(m); ents.drones.push({ ...e, m, alive: true, dieT: 0, t0: Math.random() * 6, cx: e.x, cy: e.y }); }
      else if (e.t === 'tanque') {
        const m = FG.makeTank(e.valla); m.position.set(e.x, e.y, 0); scene.add(m);
        ents.tanques.push({ ...e, m, shake: 0, cool: 0 });
      } else if (e.t === 'colchon') {
        const m = FG.makeMattress(); m.position.set(e.x, e.y + 0.3, -0.2); scene.add(m);
        const s = { x0: e.x - 1, x1: e.x + 1, y0: e.y, y1: e.y + 0.72, tipo: 'colchon', oneway: true, m, squash: 0 };
        solids.push(s); ents.colchones.push(s);
      } else if (e.t === 'andamio') {
        const w = e.x1 - e.x0, m = FG.makeScaffold(w, 30); scene.add(m);
        const s = { x0: e.x0, x1: e.x1, y0: e.y0 - 0.25, y1: e.y0, tipo: 'andamio', oneway: true, m, a: e, dy: 0 };
        solids.push(s); ents.andamios.push(s);
      } else if (e.t === 'teja') {
        const w = e.x1 - e.x0, m = FG.makeSheet(w), posts = FG.makeSheetPosts(w, e.y);
        m.position.set((e.x0 + e.x1) / 2, e.y, -0.3); posts.position.copy(m.position); scene.add(m, posts);
        const s = { x0: e.x0, x1: e.x1, y0: e.y - 0.2, y1: e.y, tipo: 'teja', oneway: true, m, y: e.y, t: -1, fall: 0, vy: 0, gone: false };
        solids.push(s); ents.tejas.push(s);
      } else if (e.t === 'cp') {
        const m = FG.makeCheckpoint(); m.position.set(e.x, e.y, -1.6); scene.add(m);
        ents.cps.push({ ...e, m, on: false });
      } else if (e.t === 'meta') { const m = FG.makeGoal(); m.position.set(e.x, e.y, -0.6); scene.add(m); ents.meta = { ...e, m }; }
      else if (e.t === 'escalon') {
        // tanque elevado sobre una base de ladrillo: sirve de escalón
        const g = new THREE.Group();
        const base = new THREE.Mesh(new THREE.BoxGeometry(2.4, e.h, 1.6), FG.mats.mBrick); base.position.y = e.h / 2; base.castShadow = base.receiveShadow = true; g.add(base);
        const tq = new THREE.Mesh(new THREE.CylinderGeometry(0.6, 0.55, 1.0, 14), new THREE.MeshStandardMaterial({ color: '#25272c' })); tq.position.set(0, e.h + 0.5, -0.9); tq.castShadow = true; g.add(tq);
        g.position.set(e.x, e.y, 0); scene.add(g);
      }
    }
    const cherryMesh = FG.makeCherries(ents.cerezas.length); scene.add(cherryMesh);
    const tmpM = new THREE.Matrix4(), tmpQ = new THREE.Quaternion(), tmpV = new THREE.Vector3(), tmpS = new THREE.Vector3(), UP = new THREE.Vector3(0, 1, 0);
    const storm = FG.makeStorm(); scene.add(storm); storm.visible = false;

    // ---------- partículas ----------
    const NP = 600;
    const pGeo = new THREE.BufferGeometry();
    const pPos = new Float32Array(NP * 3), pCol = new Float32Array(NP * 3), pData = [];
    for (let i = 0; i < NP; i++) { pPos[i * 3 + 1] = -999; pData.push({ life: 0, vx: 0, vy: 0, vz: 0, g: 0 }); }
    pGeo.setAttribute('position', new THREE.BufferAttribute(pPos, 3));
    pGeo.setAttribute('color', new THREE.BufferAttribute(pCol, 3));
    const points = new THREE.Points(pGeo, new THREE.PointsMaterial({ size: 0.22, vertexColors: true, map: FG.glowTex(), transparent: true, depthWrite: false }));
    points.frustumCulled = false; scene.add(points);
    let pNext = 0;
    function burst(x, y, z, n, color, spd = 4, g = 10, life = 0.7) {
      const c = new THREE.Color(color);
      for (let k = 0; k < n; k++) {
        const i = pNext = (pNext + 1) % NP, d = pData[i], a = Math.random() * Math.PI * 2, s = spd * (0.4 + Math.random() * 0.6);
        d.life = life * (0.6 + Math.random() * 0.6); d.vx = Math.cos(a) * s; d.vy = Math.abs(Math.sin(a)) * s * 0.9 + 1; d.vz = (Math.random() - 0.5) * s; d.g = g;
        pPos[i * 3] = x; pPos[i * 3 + 1] = y; pPos[i * 3 + 2] = z;
        pCol[i * 3] = c.r; pCol[i * 3 + 1] = c.g; pCol[i * 3 + 2] = c.b;
      }
    }
    function updateParticles(dt) {
      for (let i = 0; i < NP; i++) {
        const d = pData[i]; if (d.life <= 0) continue;
        d.life -= dt; d.vy -= d.g * dt;
        pPos[i * 3] += d.vx * dt; pPos[i * 3 + 1] += d.vy * dt; pPos[i * 3 + 2] += d.vz * dt;
        if (d.life <= 0) pPos[i * 3 + 1] = -999;
      }
      pGeo.attributes.position.needsUpdate = true; pGeo.attributes.color.needsUpdate = true;
    }

    // ---------- sonido sintetizado ----------
    const S = {
      ctx: null, on: true,
      init() { if (this.ctx) return; try { this.ctx = new (window.AudioContext || window.webkitAudioContext)(); this.g = this.ctx.createGain(); this.g.gain.value = 0.3; this.g.connect(this.ctx.destination); } catch (e) { this.on = false; } },
      tone(f0, f1, dur, type = 'square', vol = 0.3, delay = 0) {
        if (!this.ctx || !this.on) return;
        const t = this.ctx.currentTime + delay, o = this.ctx.createOscillator(), g = this.ctx.createGain();
        o.type = type; o.frequency.setValueAtTime(f0, t); o.frequency.exponentialRampToValueAtTime(f1, t + dur);
        g.gain.setValueAtTime(vol, t); g.gain.exponentialRampToValueAtTime(0.001, t + dur);
        o.connect(g).connect(this.g); o.start(t); o.stop(t + dur + 0.02);
      },
      noise(dur, vol = 0.4, freq = 800) {
        if (!this.ctx || !this.on) return;
        const t = this.ctx.currentTime, n = this.ctx.sampleRate * dur, b = this.ctx.createBuffer(1, n, this.ctx.sampleRate), d = b.getChannelData(0);
        for (let i = 0; i < n; i++) d[i] = (Math.random() * 2 - 1) * (1 - i / n);
        const s = this.ctx.createBufferSource(), f = this.ctx.createBiquadFilter(), g = this.ctx.createGain();
        s.buffer = b; f.type = 'lowpass'; f.frequency.value = freq; g.gain.value = vol;
        s.connect(f).connect(g).connect(this.g); s.start(t);
      },
      jump() { this.tone(260, 520, 0.14, 'square', 0.12); },
      cherry(k) { this.tone(880 * Math.pow(1.03, k % 12), 1320 * Math.pow(1.03, k % 12), 0.09, 'sine', 0.22); },
      stomp() { this.tone(300, 80, 0.15, 'triangle', 0.4); this.noise(0.08, 0.2, 1500); },
      hurt() { this.tone(400, 120, 0.3, 'sawtooth', 0.18); },
      boom() { this.noise(0.45, 0.7, 500); this.tone(120, 40, 0.4, 'sine', 0.5); },
      bounce() { this.tone(180, 720, 0.22, 'sine', 0.3); },
      letter() { [523, 659, 784, 1046].forEach((f, i) => this.tone(f, f, 0.12, 'square', 0.12, i * 0.07)); },
      cp() { [392, 523, 659, 784].forEach((f, i) => this.tone(f, f * 1.01, 0.16, 'triangle', 0.25, i * 0.09)); },
      win() { [523, 659, 784, 659, 784, 1046].forEach((f, i) => this.tone(f, f, 0.22, 'square', 0.14, i * 0.13)); },
      crack() { this.noise(0.25, 0.4, 2500); },
    };

    // ---------- entrada: teclado, táctil y control ----------
    const keys = {};
    const I = { left: false, right: false, jump: false, roll: false, jumpPressed: false, rollPressed: false };
    const touch = { left: false, right: false, jump: false, roll: false };
    let padPrev = { jump: false, roll: false, start: false };
    addEventListener('keydown', (e) => {
      if (['Space', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.code)) e.preventDefault();
      if (e.repeat) return;
      keys[e.code] = true;
      if (['Space', 'KeyW', 'ArrowUp', 'KeyK', 'KeyZ'].includes(e.code)) I.jumpPressed = true;
      if (['ShiftLeft', 'ShiftRight', 'KeyJ', 'KeyX'].includes(e.code)) I.rollPressed = true;
      if (e.code === 'Escape' || e.code === 'KeyP') togglePause();
      if (e.code === 'Enter' && game.state === 'menu') start();
      S.init();
    });
    addEventListener('keyup', (e) => { keys[e.code] = false; });
    addEventListener('blur', () => { for (const k in keys) keys[k] = false; });
    for (const [id, k] of [['tLeft', 'left'], ['tRight', 'right'], ['tJump', 'jump'], ['tRoll', 'roll']]) {
      const b = $(id);
      const down = (e) => { e.preventDefault(); S.init(); if (!touch[k]) { if (k === 'jump') I.jumpPressed = true; if (k === 'roll') I.rollPressed = true; } touch[k] = true; b.classList.add('on'); };
      const up = (e) => { e.preventDefault(); touch[k] = false; b.classList.remove('on'); };
      b.addEventListener('pointerdown', down); b.addEventListener('pointerup', up); b.addEventListener('pointercancel', up); b.addEventListener('pointerleave', up);
    }
    function readInput() {
      let pad = null;
      for (const g of (navigator.getGamepads ? navigator.getGamepads() : [])) if (g) { pad = g; break; }
      const pj = pad && (pad.buttons[0]?.pressed || pad.buttons[3]?.pressed);
      const pr = pad && (pad.buttons[2]?.pressed || pad.buttons[1]?.pressed || pad.buttons[7]?.pressed);
      const ps = pad && pad.buttons[9]?.pressed;
      const ax = pad ? pad.axes[0] : 0;
      if (window.__bot) { Object.assign(I, window.__bot(P, I)); padPrev = { jump: false, roll: false, start: false }; return; }
      I.left = keys.KeyA || keys.ArrowLeft || touch.left || ax < -0.35 || (pad && pad.buttons[14]?.pressed);
      I.right = keys.KeyD || keys.ArrowRight || touch.right || ax > 0.35 || (pad && pad.buttons[15]?.pressed);
      I.jump = keys.Space || keys.KeyW || keys.ArrowUp || keys.KeyK || keys.KeyZ || touch.jump || pj;
      I.roll = keys.ShiftLeft || keys.ShiftRight || keys.KeyJ || keys.KeyX || touch.roll || pr;
      if (pj && !padPrev.jump) I.jumpPressed = true;
      if (pr && !padPrev.roll) I.rollPressed = true;
      if (ps && !padPrev.start) { if (game.state === 'menu' || game.state === 'win' || game.state === 'over') start(); else togglePause(); }
      if (pj && !padPrev.jump && (game.state === 'menu' || game.state === 'win' || game.state === 'over')) start();
      padPrev = { jump: pj, roll: pr, start: ps };
    }

    // ---------- estado de la partida ----------
    const game = { state: 'menu', t: 0, cherries: 0, letters: 0, lives: 4, deaths: 0, cp: null, stormX: -999, stormOn: false, stormDone: false, hintsSeen: new Set(), shake: 0, stormFlash: 0 };
    const TOTAL_CHERRIES = ents.cerezas.length;

    function resetLevelEntities() {
      for (const p of ents.palomas) { p.alive = true; p.dieT = 0; p.m.visible = true; p.x = (p.x0 + p.x1) / 2; p.py = p.y; p.m.rotation.set(0, 0, 0); }
      for (const d of ents.drones) { d.alive = true; d.dieT = 0; d.m.visible = true; d.m.rotation.set(0, 0, 0); }
      for (const s of ents.tejas) resetTeja(s);
    }
    function resetTeja(s) { s.t = -1; s.gone = false; s.vy = 0; s.y1 = s.y; s.y0 = s.y - 0.2; s.m.position.y = s.y; s.m.visible = true; s.m.userData.sheet.rotation.set(0, 0, 0); }

    function spawnAt(x, y) {
      Object.assign(P, { x, y, vx: 0, vy: 0, face: 1, ground: null, onGround: false, roll: 0, longJump: false, hp: 3, inv: 0, state: 'normal', lastGroundY: y, tank: null, flight: null });
      playerMesh.visible = true;
      cam.x = x + 2.5; cam.y = y + 3.2;
    }

    function start() {
      S.init();
      for (const c of ents.cerezas) c.got = false;
      for (const l of ents.letras) { l.got = false; l.m.visible = true; }
      for (const c of ents.cps) { c.on = false; }
      storm.visible = false;
      Object.assign(game, { state: 'play', t: 0, cherries: 0, letters: 0, lives: 4, deaths: 0, stormOn: false, stormDone: false, hintsSeen: new Set() });
      game.cp = { x: lvl.spawn.x, y: lvl.spawn.y };
      ents.cps.forEach((c, i) => { if (i === 0) { c.on = true; FG.lightCheckpoint(c.m); } });
      resetLevelEntities();
      spawnAt(lvl.spawn.x, lvl.spawn.y);
      $('menu').hidden = true; $('end').hidden = true; $('pause').hidden = true; $('hud').hidden = false;
      updateHud();
    }
    function togglePause() {
      if (game.state === 'play') { game.state = 'pause'; $('pause').hidden = false; }
      else if (game.state === 'pause') { game.state = 'play'; $('pause').hidden = true; }
    }

    // ---------- colisiones ----------
    const overlapX = (s, x, w) => x + w / 2 > s.x0 && x - w / 2 < s.x1;
    function moveAndCollide(dt) {
      const w = FISICA.ancho, h = P.roll > 0 ? FISICA.altoRodando : FISICA.alto;
      // horizontal
      P.x += P.vx * dt;
      for (const s of solids) {
        if (s.oneway || s.gone) continue;
        if (overlapX(s, P.x, w) && P.y + h > s.y0 && P.y < s.y1 - 0.001) {
          if (P.vx > 0) P.x = s.x0 - w / 2 - 0.0005; else if (P.vx < 0) P.x = s.x1 + w / 2 + 0.0005;
          else P.x = P.x < (s.x0 + s.x1) / 2 ? s.x0 - w / 2 : s.x1 + w / 2;
          P.vx = 0; if (P.roll > 0) P.roll = 0;
        }
      }
      P.x = Math.max(0.6, Math.min(lvl.fin - 0.6, P.x));
      // vertical
      const prevY = P.y;
      P.y += P.vy * dt;
      const wasGround = P.onGround;
      P.onGround = false;
      for (const s of solids) {
        if (s.gone || !overlapX(s, P.x, w)) continue;
        if (s.oneway) {
          if (P.vy <= 0 && prevY >= s.y1 - 0.05 && P.y <= s.y1) land(s);
          else if (s.tipo === 'colchon' && P.vy <= 0 && P.y >= s.y0 - 0.05 && P.y < s.y1) land(s); // entrar caminando también rebota
        } else if (P.y + h > s.y0 && P.y < s.y1) {
          if (P.vy <= 0 && prevY >= s.y1 - 0.3) land(s);
          else if (P.vy > 0) { P.y = s.y0 - h; P.vy = 0; }
        }
      }
      if (P.onGround && !wasGround) onLand();
    }
    function land(s) {
      P.y = s.y1; P.vy = 0; P.onGround = true; P.ground = s;
      if (s.tipo === 'colchon') {
        P.vy = FISICA.colchon * (I.jump ? 1.08 : 1); P.forceHold = true; P.onGround = false; P.longJump = false; s.squash = 1; S.bounce(); P.squash = 0.6;
        burst(P.x, P.y, 0, 8, '#ffffff', 3, 8, 0.4);
      }
      if (s.tipo === 'teja' && s.t < 0) { s.t = 0; S.crack(); }
    }
    function onLand() {
      const impact = Math.min(1, Math.abs(P.lastVy) / 25);
      P.squash = 1 - impact * 0.35;
      if (impact > 0.3) burst(P.x, P.y + 0.05, 0.2, 6 + impact * 8, '#d9cbb3', 2.5, 6, 0.45);
      P.longJump = false;
    }

    // ---------- muerte y daño ----------
    function hurt(fromX) {
      if (P.inv > 0 || P.state !== 'normal') return;
      P.hp--; S.hurt(); game.shake = 0.3;
      if (P.hp <= 0) return die('¡Ay! Se te acabaron los tintos');
      P.inv = 1.6; P.vx = (P.x < fromX ? -1 : 1) * 7; P.vy = 8; P.onGround = false; P.roll = 0;
      updateHud();
    }
    function die(msg) {
      if (P.state === 'dead') return;
      P.state = 'dead'; P.deadT = 1.5; P.vy = 10; P.vx = 0; game.deaths++;
      showToast(msg, 1.4); S.hurt();
    }
    function respawn() {
      game.lives--;
      if (game.lives < 0) return gameOver();
      resetLevelEntities();
      if (game.stormOn && !game.stormDone) { game.stormOn = false; storm.visible = false; }
      spawnAt(game.cp.x, game.cp.y);
      updateHud();
    }
    function gameOver() {
      game.state = 'over'; $('hud').hidden = true; storm.visible = false;
      $('endTitle').textContent = 'Se acabó el turno';
      $('endText').textContent = 'La tormenta de Arcadia ganó esta vez. Vuelve a intentarlo desde tu terraza.';
      $('endStats').innerHTML = stats();
      $('end').hidden = false;
    }
    function win() {
      game.state = 'win'; S.win(); $('hud').hidden = true;
      for (let i = 0; i < 8; i++) setTimeout(() => burst(P.x + (Math.random() - 0.5) * 6, P.y + 4, 0, 30, ['#ffcf3d', '#35d06a', '#e0262f', '#3aa8ff'][i % 4], 7, 6, 1.6), i * 160);
      $('endTitle').textContent = '¡Llegaste a la torre!';
      $('endText').textContent = 'Las cerezas que recogiste ya están en la red de cultivo de Float Gardens.';
      $('endStats').innerHTML = stats();
      setTimeout(() => { if (game.state === 'win') $('end').hidden = false; }, 1400);
    }
    function stats() {
      const t = game.t, mm = Math.floor(t / 60), ss = Math.floor(t % 60).toString().padStart(2, '0');
      return `<div><b>${game.cherries}</b><span>de ${TOTAL_CHERRIES} cerezas</span></div><div><b>${game.letters}/5</b><span>letras FLOAT</span></div><div><b>${mm}:${ss}</b><span>tiempo</span></div><div><b>${game.deaths}</b><span>caídas</span></div>`;
    }

    // ---------- paso de simulación ----------
    function step(dt) {
      game.t += dt;
      const T = game.t;

      // plataformas móviles
      for (const s of ents.andamios) {
        const a = s.a, k = 0.5 - 0.5 * Math.cos(((T / a.per) + a.ph) * Math.PI * 2);
        const y = a.y0 + (a.y1 - a.y0) * k;
        s.dy = y - s.y1; s.y1 = y; s.y0 = y - 0.25;
      }
      for (const s of ents.tejas) {
        if (s.t >= 0 && !s.gone) {
          s.t += dt;
          if (s.t > 0.5) { s.vy -= 30 * dt; s.y1 += s.vy * dt; s.y0 = s.y1 - 0.2; if (s.y1 < s.y - 1.2) s.gone = true; }
        } else if (s.gone) { s.t += dt; s.y1 += (s.vy -= 30 * dt) * dt; if (s.t > 5) resetTeja(s); }
      }

      if (P.state === 'dead') {
        P.deadT -= dt; P.vy -= 40 * dt; P.y += P.vy * dt;
        if (P.deadT <= 0) respawn();
        return;
      }

      if (I.jumpPressed) P.buffer = FISICA.buffer;
      P.buffer -= dt;

      // dentro de un tanque-cañón
      if (P.state === 'tank') {
        const tq = P.tank; P.x = tq.x; P.y = tq.y - 0.8; tq.shake = Math.min(1, tq.shake + dt * 3);
        tq.hold += dt;
        if (P.buffer > 0 || tq.hold > 2.6) fire(tq);
        return;
      }
      if (P.state === 'flight') {
        const f = P.flight; f.t += dt / f.T;
        const k = Math.min(1, f.t);
        const nx = f.x0 + (f.x1 - f.x0) * k, ny = f.y0 + (f.y1 - f.y0) * k + Math.sin(k * Math.PI) * f.arc;
        P.vx = (nx - P.x) / dt; P.vy = (ny - P.y) / dt; P.x = nx; P.y = ny; P.phase += dt * 20;
        if (Math.random() < 0.6) burst(P.x - Math.sign(P.vx) * 0.3, P.y + 0.6, 0, 1, '#ffffff', 0.5, 0, 0.4);
        if (k >= 1) {
          if (f.into) enterTank(f.into);
          else { P.state = 'normal'; P.vx = Math.sign(f.x1 - f.x0) * 5; P.vy = -1; }
        }
        collectAll();
        return;
      }

      // carry de andamio
      if (P.onGround && P.ground && P.ground.tipo === 'andamio') P.y += P.ground.dy;
      if (P.onGround && P.ground && P.ground.tipo === 'teja' && P.ground.t > 0.5) P.onGround = false;

      const ax = (I.right ? 1 : 0) - (I.left ? 1 : 0);
      if (ax) P.face = ax;
      P.coyote = P.onGround ? FISICA.coyote : P.coyote - dt;
      P.rollCd -= dt;

      // rodar
      if (I.rollPressed && P.onGround && P.rollCd <= 0 && P.roll <= 0) {
        P.roll = FISICA.tiempoRodar; P.rollCd = 0.55; P.vx = P.face * FISICA.rodar; S.tone(200, 120, 0.12, 'triangle', 0.2);
        burst(P.x, P.y + 0.1, 0.2, 6, '#d9cbb3', 2, 6, 0.35);
      }
      if (P.roll > 0) { P.roll -= dt; P.vx = P.face * FISICA.rodar; if (!P.onGround && P.coyote <= 0) P.roll = 0; }
      else {
        const cap = P.longJump ? FISICA.rodar : FISICA.correr;
        const target = ax * cap, acc = P.onGround ? (ax ? FISICA.accelSuelo : FISICA.frenado) : FISICA.accelAire;
        if (P.longJump && ax === Math.sign(P.vx) && Math.abs(P.vx) > FISICA.correr) { /* conserva el impulso de la rodada */ }
        else P.vx += Math.sign(target - P.vx) * Math.min(Math.abs(target - P.vx), acc * dt);
      }

      // saltar
      if (P.buffer > 0 && (P.onGround || P.coyote > 0)) {
        P.vy = FISICA.salto; P.onGround = false; P.coyote = 0; P.buffer = 0;
        if (P.roll > 0) { P.longJump = true; P.roll = 0; P.vx = P.face * FISICA.rodar; }
        P.squash = 1.25; S.jump();
        burst(P.x, P.y + 0.05, 0.2, 5, '#e8dcc6', 2, 6, 0.3);
      }
      if (P.vy <= 0) P.forceHold = false;
      const g = P.vy > 0 ? (I.jump || P.forceHold ? FISICA.gravSostenido : FISICA.gravSubida) : FISICA.gravCaida;
      P.vy = Math.max(P.vy - g * dt, -FISICA.caidaMax);
      P.lastVy = P.vy;
      moveAndCollide(dt);
      if (P.onGround) P.lastGroundY = P.y;
      if (P.inv > 0) P.inv -= dt;

      // caída a la calle
      if (P.y < Math.min(P.lastGroundY - 7, 4)) die('¡A la calle!');

      // enemigos
      const pw = FISICA.ancho, ph = P.roll > 0 ? FISICA.altoRodando : FISICA.alto;
      const hit = (ex, ey, ew, eh) => Math.abs(P.x - ex) < (pw + ew) / 2 && P.y < ey + eh && P.y + ph > ey;
      for (const p of ents.palomas) {
        if (!p.alive) continue;
        if (hit(p.x, p.py, 0.9, 0.8)) {
          if ((P.vy < 0 && P.y > p.py + 0.35) || P.roll > 0 || P.longJump) killPigeon(p);
          else hurt(p.x);
        }
      }
      for (const d of ents.drones) {
        if (!d.alive) continue;
        if (hit(d.cx, d.cy - 0.35, 1.5, 0.8)) {
          if ((P.vy < 0 && P.y > d.cy - 0.1) || P.longJump) killDrone(d);
          else hurt(d.cx);
        }
      }
      // tanques
      for (const tq of ents.tanques) {
        tq.cool -= dt;
        if (tq.cool <= 0 && Math.abs(P.x - tq.x) < 1.2 && Math.abs(P.y + 0.8 - tq.y) < 1.4) enterTank(tq);
      }
      // puntos de control
      for (const c of ents.cps) if (!c.on && P.x > c.x - 0.5 && Math.abs(P.y - c.y) < 4) {
        c.on = true; game.cp = { x: c.x, y: c.y }; FG.lightCheckpoint(c.m); S.cp(); showToast('¡Punto de control!', 1.6);
        burst(c.x, c.y + 2.6, -1, 30, '#53ff8f', 5, 4, 0.9);
      }
      // tormenta
      if (lvl.tormenta) {
        const tm = lvl.tormenta;
        if (!game.stormOn && !game.stormDone && P.x > tm.x0 + 1) { game.stormOn = true; game.stormX = P.x - 15; storm.visible = true; storm.userData.uni.uA.value = 1; }
        if (game.stormOn) {
          game.stormX += 6.2 * dt;
          game.stormX = Math.max(game.stormX, P.x - 13);
          if (P.x > tm.x1) { game.stormOn = false; game.stormDone = true; showToast('¡Le ganaste a la tormenta!', 2); }
          else if (P.x - 0.2 < game.stormX) die('La tormenta de Arcadia te alcanzó');
        }
      }
      // meta
      if (ents.meta && P.x > ents.meta.x - 0.4 && P.y >= ents.meta.y - 0.5) win();
      // pistas
      for (const h of lvl.pistas) if (!game.hintsSeen.has(h) && P.x > h.x) { game.hintsSeen.add(h); showToast(h.txt, 4.5, true); }
      collectAll();
    }

    function collectAll() {
      const cx = P.x, cy = P.y + 0.8;
      for (const c of ents.cerezas) {
        if (c.got) continue;
        if (Math.abs(c.x - cx) < 0.8 && Math.abs(c.y - cy) < 1.1) {
          c.got = true; game.cherries++; S.cherry(game.cherries);
          burst(c.x, c.y, 0.2, 8, '#ff4757', 3, 4, 0.45);
          if (game.cherries % 100 === 0) { game.lives++; showToast('¡+1 vida! 100 cerezas', 1.8); }
          updateHud();
        }
      }
      for (const l of ents.letras) {
        if (l.got) continue;
        if (Math.abs(l.x - cx) < 1 && Math.abs(l.y - cy) < 1.2) {
          l.got = true; l.m.visible = false; game.letters++; S.letter();
          burst(l.x, l.y, 0.2, 24, '#ffd34d', 5, 3, 0.8);
          if (game.letters === 5) { game.lives++; showToast('¡FLOAT completo! +1 vida', 2.2); }
          updateHud(l.ch);
        }
      }
    }
    function killPigeon(p) {
      p.alive = false; p.dieT = 1; p.vy = 6; S.stomp();
      if (P.roll <= 0 && !P.longJump) { P.vy = I.jump ? FISICA.pisoton * 1.25 : FISICA.pisoton; P.onGround = false; }
      burst(p.x, p.py + 0.5, 0.2, 14, '#c9d0da', 3, 3, 0.9);
      game.shake = 0.12;
    }
    function killDrone(d) {
      d.alive = false; d.dieT = 1.2; S.stomp(); S.noise(0.3, 0.4, 900);
      P.vy = I.jump ? FISICA.pisoton * 1.3 : FISICA.pisoton; P.onGround = false;
      burst(d.cx, d.cy, 0.2, 18, '#a86bff', 5, 6, 0.8); burst(d.cx, d.cy, 0.2, 10, '#ffb13d', 4, 6, 0.5);
      game.shake = 0.18;
    }
    function enterTank(tq) {
      P.state = 'tank'; P.tank = tq; tq.hold = 0; tq.shake = 0; P.vx = P.vy = 0; P.onGround = false; P.ground = null; P.coyote = 0; P.roll = 0; P.longJump = false; P.buffer = 0;
      playerMesh.visible = false; S.tone(200, 300, 0.1, 'square', 0.2);
      P.x = tq.x; P.y = tq.y - 0.8;
    }
    function fire(tq) {
      const dx = tq.tx - tq.x, dy = tq.ty - tq.y, dist = Math.hypot(dx, dy);
      const into = tq.chain ? ents.tanques.find((o) => Math.abs(o.x - tq.tx) < 0.1) : null;
      P.state = 'flight'; P.buffer = 0;
      P.flight = { t: 0, T: Math.max(0.55, dist / 24), x0: tq.x, y0: tq.y - 0.8, x1: tq.tx, y1: into ? into.y - 0.8 : tq.ty, arc: Math.min(6, dist * 0.16), into };
      playerMesh.visible = true; tq.cool = 1; tq.shake = 0; P.face = Math.sign(dx) || 1;
      S.boom(); game.shake = 0.35;
      burst(tq.x, tq.y + 0.8, 0.2, 26, '#e8e2d6', 6, 2, 0.8);
    }

    // ---------- cámara lateral ----------
    const cam = { x: 0, y: 0, look: 0, z: 19 };
    function updateCamera(dt) {
      const k = (r) => 1 - Math.exp(-r * dt);
      const flying = P.state === 'flight' || P.state === 'tank';
      cam.look += ((flying ? Math.sign(P.vx || 1) * 4 : game.stormOn ? 0.5 : P.face * 2.6) - cam.look) * k(2.2);
      let ty = P.lastGroundY + 3.0;
      if (P.y > P.lastGroundY + 3.5 || P.y < P.lastGroundY - 0.5 || flying) ty = P.y + 2.2;
      if (P.state === 'dead') ty = cam.y;
      cam.x += (P.x + cam.look - cam.x) * k(flying ? 8 : 6);
      cam.y += (ty - cam.y) * k(flying ? 6 : 3.2);
      const portrait = camera.aspect < 1;
      camera.fov = portrait ? 52 : 38;
      const halfW = Math.tan(THREE.MathUtils.degToRad(camera.fov / 2)) * camera.aspect;
      const want = Math.min(42, Math.max(16.5, 9.5 / halfW)) + (flying ? 4 : 0) + (game.stormOn ? 6 : 0);
      cam.z += (want - cam.z) * k(2);
      let sx = 0, sy = 0;
      if (game.shake > 0) { game.shake -= dt; sx = (Math.random() - 0.5) * game.shake * 1.4; sy = (Math.random() - 0.5) * game.shake * 1.4; }
      camera.position.set(cam.x + sx, cam.y + 1.4 + sy + (portrait ? 2 : 0), cam.z);
      camera.lookAt(cam.x + sx * 0.5, cam.y - 0.6 + (portrait ? 1 : 0), 0);
      camera.updateProjectionMatrix();
      sun.target.position.set(cam.x, cam.y - 4, -6);
      sun.position.copy(sun.target.position).addScaledVector(sunDir, 90);
    }

    // ---------- dibujar ----------
    const animSt = { mode: 'idle', phase: 0, speed: 0 };
    function draw(dt) {
      const T = game.t;
      // jugador
      playerMesh.position.set(P.x, P.y, 0);
      const turn = P.face > 0 ? Math.PI / 2 - 0.45 : -Math.PI / 2 + 0.45;
      playerMesh.rotation.y += (turn - playerMesh.rotation.y) * Math.min(1, dt * 14);
      const body = playerMesh.userData.body;
      P.squash += (1 - P.squash) * Math.min(1, dt * 10);
      const spd = Math.abs(P.vx);
      if (P.state === 'flight' || P.roll > 0 || (P.longJump && !P.onGround)) {
        animSt.mode = 'jump';
        body.rotation.z -= dt * 18 * (P.face > 0 ? 1 : 1);
        body.scale.setScalar(0.85);
        body.position.y = 0.55;
      } else {
        body.rotation.z = 0; body.position.y = 0.85 * P.squash;
        body.scale.set(1 / Math.sqrt(P.squash), P.squash, 1 / Math.sqrt(P.squash));
        animSt.mode = !P.onGround ? (P.vy > 0 ? 'jump' : 'fall') : spd > 0.5 ? 'run' : 'idle';
      }
      animSt.speed = Math.min(1, spd / FISICA.correr);
      animSt.phase += dt * (animSt.mode === 'run' ? 4 + spd * 1.3 : 6);
      if (P.state === 'dead') { animSt.mode = 'fall'; body.rotation.z += dt * 6; }
      FG.posePlayer(playerMesh, animSt, dt);
      playerMesh.visible = P.state !== 'tank' && (P.inv <= 0 || Math.floor(T * 16) % 2 === 0);
      // polvo al correr
      if (P.onGround && spd > 6 && Math.random() < dt * 14) burst(P.x - P.face * 0.3, P.y + 0.05, 0.3, 1, '#d6c8ae', 1.2, 3, 0.35);
      // sombra blob
      let gy = -99;
      for (const s of solids) if (!s.gone && overlapX(s, P.x, 0.1) && s.y1 <= P.y + 0.05 && s.y1 > gy) gy = s.y1;
      blob.visible = gy > -50 && P.state !== 'tank';
      blob.position.set(P.x, gy + 0.03, 0); blob.scale.setScalar(Math.max(0.3, 1 - (P.y - gy) / 8));

      // cerezas
      let i = 0;
      for (const c of ents.cerezas) {
        tmpQ.setFromAxisAngle(UP, T * 2.4 + c.x * 0.4);
        tmpS.setScalar(c.got ? 0 : 0.9);
        tmpV.set(c.x, c.y + Math.sin(T * 3 + c.x) * 0.08, 0);
        cherryMesh.setMatrixAt(i++, tmpM.compose(tmpV, tmpQ, tmpS));
      }
      cherryMesh.instanceMatrix.needsUpdate = true;
      for (const l of ents.letras) { l.m.rotation.y = T * 2; l.m.position.y = l.y + Math.sin(T * 2.5) * 0.15; }

      // palomas
      for (const p of ents.palomas) {
        if (p.alive) {
          p.x += p.dir * 1.3 * dt;
          if (p.x > p.x1) p.dir = -1; if (p.x < p.x0) p.dir = 1;
          p.m.position.set(p.x, p.py + Math.abs(Math.sin(T * 9 + p.x0)) * 0.05, 0);
          p.m.rotation.y = p.dir > 0 ? -0.3 : Math.PI + 0.3;
          p.m.userData.head.position.x = 0.38 + Math.sin(T * 9 + p.x0) * 0.06;
        } else if (p.dieT > 0) {
          p.dieT -= dt; p.vy += 8 * dt; p.py += p.vy * dt; p.m.position.set(p.x, p.py, 0); p.m.rotation.z += dt * 5;
          if (p.dieT <= 0) p.m.visible = false;
        }
      }
      // drones
      for (const d of ents.drones) {
        if (d.alive) {
          d.cx = d.x + Math.sin((T + d.t0) * d.sp) * d.ax; d.cy = d.y + Math.sin((T + d.t0) * 1.7) * 0.6;
          d.m.position.set(d.cx, d.cy, 0); d.m.rotation.z = Math.cos((T + d.t0) * d.sp) * 0.25;
          for (const r of d.m.userData.rotors) r.rotation.y += dt * 40;
        } else if (d.dieT > 0) {
          d.dieT -= dt; d.cy -= dt * 9; d.m.position.y = d.cy; d.m.rotation.x += dt * 8;
          if (d.dieT <= 0) d.m.visible = false;
        }
      }
      // tanques: apuntan a su destino, tiemblan cuando estás adentro
      for (const tq of ents.tanques) {
        const dx = tq.tx - tq.x, dy = tq.ty - tq.y + Math.min(6, Math.hypot(dx, tq.ty - tq.y) * 0.16) * Math.PI * 0.55;
        const ang = Math.atan2(dy, dx) - Math.PI / 2;
        const s = P.state === 'tank' && P.tank === tq ? Math.sin(T * 40) * 0.06 * tq.shake : 0;
        tq.m.userData.barrel.rotation.z = ang + s;
        tq.m.userData.barrel.scale.setScalar(1 + (P.state === 'tank' && P.tank === tq ? 0.06 * Math.sin(T * 12) : 0));
      }
      for (const s of ents.colchones) { s.squash = Math.max(0, s.squash - dt * 4); s.m.userData.m.scale.y = 1 - Math.sin(s.squash * Math.PI) * 0.35; }
      for (const s of ents.andamios) {
        s.m.position.set((s.x0 + s.x1) / 2, s.y1, -0.2);
        for (const r of s.m.userData.ropes) { const len = 30 - s.y1; r.scale.y = len; r.position.y = len / 2; }
      }
      for (const s of ents.tejas) {
        s.m.position.y = s.y1;
        const sh = s.t >= 0 && s.t < 0.5 && !s.gone ? Math.sin(T * 60) * 0.03 : 0;
        s.m.userData.sheet.rotation.z = sh + (s.t > 0.5 ? (s.t - 0.5) * 0.6 : 0);
        s.m.visible = !(s.gone && s.t > 1.5);
      }
      if (ents.meta) ents.meta.m.userData.ribbon.visible = game.state !== 'win';
      // tormenta
      if (storm.visible) {
        const u = storm.userData.uni; u.uT.value = T; u.uEdge.value = game.stormX;
        if (!game.stormOn) { u.uA.value = Math.max(0, u.uA.value - dt * 0.6); if (u.uA.value <= 0) storm.visible = false; }
        if (game.stormOn) game.stormX += 0;
        storm.position.x = game.stormX - 50;
        for (const m of storm.children) if (m.isMesh) m.position.x = 0;
        game.stormFlash -= dt;
        if (game.stormFlash < -Math.random() * 3) { game.stormFlash = 0.12; if (game.stormOn) S.noise(0.6, 0.25, 220); }
        storm.userData.flash.intensity = game.stormFlash > 0 ? 3 : 0;
        storm.userData.flash.position.set(46, cam.y + 4, 6);
      }
      world.update(T);
      updateParticles(dt);
    }

    // ---------- HUD ----------
    let toastT = 0;
    const TOUCH_HINTS = {
      'Corre con ← → (o A / D) · salta con Espacio': 'Corre con ◀ ▶ · toca Saltar (mantén para subir más)',
      'Shift (o X) para rodar: rueda y salta para llegar más lejos': 'Toca Rodar y luego Saltar para llegar más lejos',
    };
    function showToast(txt, dur, hint) { const t = $('toast'); if (hint && mobile && TOUCH_HINTS[txt]) txt = TOUCH_HINTS[txt]; t.textContent = txt; t.className = hint ? 'toast hint show' : 'toast show'; toastT = dur; }
    function updateHud(newLetter) {
      $('hCherries').textContent = game.cherries;
      $('hLives').textContent = Math.max(0, game.lives);
      $('hHp').innerHTML = [0, 1, 2].map((i) => `<i class="${i < P.hp ? 'on' : ''}"></i>`).join('');
      for (const l of ents.letras) $('L' + l.ch).classList.toggle('on', l.got);
      if (newLetter) { const el = $('L' + newLetter); el.classList.remove('pop'); void el.offsetWidth; el.classList.add('pop'); }
    }
    function hudTick(dt) {
      if (toastT > 0) { toastT -= dt; if (toastT <= 0) $('toast').classList.remove('show'); }
      const t = game.t, mm = Math.floor(t / 60), ss = Math.floor(t % 60).toString().padStart(2, '0');
      $('hTime').textContent = `${mm}:${ss}`;
      const prog = Math.min(1, Math.max(0, P.x / (ents.meta ? ents.meta.x : lvl.fin)));
      $('hProg').style.setProperty('--p', prog);
      const sd = game.stormOn ? Math.max(0, P.x - game.stormX) : -1;
      $('hStorm').hidden = !game.stormOn;
      if (game.stormOn) $('hStorm').textContent = `Tormenta a ${sd.toFixed(0)} m`;
    }

    // ---------- bucle ----------
    function resize() {
      const w = innerWidth, h = innerHeight;
      renderer.setSize(w, h, false); camera.aspect = w / h; camera.updateProjectionMatrix();
    }
    addEventListener('resize', resize); resize();

    let last = performance.now(), acc = 0;
    spawnAt(lvl.spawn.x, lvl.spawn.y);
    game.cp = { x: lvl.spawn.x, y: lvl.spawn.y };
    ents.cps[0].on = true; FG.lightCheckpoint(ents.cps[0].m);
    $('loading').hidden = true;

    function frame(now) {
      requestAnimationFrame(frame);
      if (window.__manual) return;
      const dt = Math.min(0.05, (now - last) / 1000); last = now;
      readInput();
      if (game.state === 'play') {
        acc += dt;
        let n = 0;
        while (acc >= STEP && n < 12) { step(STEP); I.jumpPressed = false; I.rollPressed = false; acc -= STEP; n++; }
        hudTick(dt);
      } else if (game.state === 'menu') {
        game.t += dt; animSt.mode = 'idle';
        // en el menú la cámara pasea despacio por las terrazas
        I.jumpPressed = I.rollPressed = false;
      } else { I.jumpPressed = I.rollPressed = false; if (game.state === 'win') game.t += 0; }
      if (game.state === 'menu') {
        const k = (now / 1000) * 0.08;
        camera.fov = camera.aspect < 1 ? 55 : 40;
        camera.position.set(14 + Math.sin(k) * 6, 15.5, 24); camera.lookAt(18 + Math.sin(k) * 6, 9, -6); camera.updateProjectionMatrix();
        sun.target.position.set(16, 6, -6); sun.position.copy(sun.target.position).addScaledVector(sunDir, 90);
        draw(dt);
      } else {
        if (game.state === 'play' || game.state === 'win') updateCamera(dt);
        draw(game.state === 'pause' ? 0 : dt);
      }
      renderer.render(scene, camera);
    }
    requestAnimationFrame(frame);

    $('btnPlay').addEventListener('click', start);
    $('btnAgain').addEventListener('click', start);
    $('btnResume').addEventListener('click', togglePause);
    $('tPause').addEventListener('click', togglePause);
    $('btnSound').addEventListener('click', () => { S.on = !S.on; $('btnSound').textContent = S.on ? 'Sonido: sí' : 'Sonido: no'; });

    // ganchos para pruebas automáticas
    window.__fg = { P, game, lvl, ents, start, FISICA, I, cam,
      renderFrame: (dt) => { updateCamera(dt); draw(dt); hudTick(dt); renderer.render(scene, camera); }, step: (n) => { for (let i = 0; i < n; i++) step(STEP); } };
  }
})();
