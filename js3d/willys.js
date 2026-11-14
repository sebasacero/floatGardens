// El Willys de la red: el campero cafetero (yipao) para viajar rápido entre zonas.
// Se parquea en la calle frente a la terraza de Kennedy. Acción (E / botón "Subir") para subir y bajar;
// se maneja con el mismo joystick o WASD. En la calle corre casi tres veces más que el barista a pie.
// Maqueta blanca como el resto del mundo; el único color es el rojo cereza de las sillas y los costales de fique.
(function () {
  const C = W.CONFIG;
  const MAX_STREET = 26;   // m/s en la calle (a pie: 6 caminando, 9,5 corriendo)
  const MAX_OFF = 12;      // sobre andén, parque o terraza
  const ACCEL = 15, BRAKE = 22, TURN = 3.4;
  const RADIUS = 1.25, REACH = 3.4;

  const matCache = new Map();
  const mat = (color) => { if (!matCache.has(color)) matCache.set(color, new THREE.MeshLambertMaterial({ color })); return matCache.get(color); };
  const boxGeo = new THREE.BoxGeometry(1, 1, 1);
  function box(w, h, d, color, x, y, z) {
    const m = new THREE.Mesh(boxGeo, mat(color));
    m.scale.set(w, h, d);
    m.position.set(x || 0, y || 0, z || 0);
    return m;
  }

  // Campero de los años cincuenta en cajas: capó plano, parrilla de ranuras, farolas redondas, guardabarros
  // rectos, parabrisas abatible, repuesto atrás y la carga de costales. El frente mira a +z.
  function makeWillys() {
    const g = new THREE.Group();
    const white = '#f4f5f3', grey = '#c9ccca', dark = '#2c302e', cherry = '#b3121a';
    // chasis y tina
    g.add(box(1.5, 0.16, 3.3, grey, 0, 0.5, 0));
    g.add(box(1.5, 0.5, 1.9, white, 0, 0.82, -0.55));      // tina (cabina y platón)
    g.add(box(1.5, 0.12, 1.9, grey, 0, 1.1, -0.55));       // borde de la tina
    g.add(box(1.1, 0.42, 1.15, white, 0, 0.84, 0.95));     // capó
    g.add(box(1.12, 0.04, 1.17, cherry, 0, 1.07, 0.95));   // capó rojo cereza: se distingue desde arriba sobre la calle clara
    // parrilla de siete ranuras y farolas
    g.add(box(1.1, 0.46, 0.08, white, 0, 0.82, 1.56));
    for (let i = -3; i <= 3; i++) g.add(box(0.06, 0.3, 0.03, dark, i * 0.1, 0.84, 1.61));
    for (const s of [-1, 1]) {
      const lamp = new THREE.Mesh(new THREE.CylinderGeometry(0.11, 0.11, 0.06, 12), mat('#fff6d8'));
      lamp.rotation.x = Math.PI / 2;
      lamp.position.set(s * 0.43, 0.9, 1.6);
      g.add(lamp);
      // guardabarros rectos sobre las llantas de adelante
      g.add(box(0.36, 0.06, 0.95, white, s * 0.73, 0.86, 1.0));
      g.add(box(0.36, 0.28, 0.06, white, s * 0.73, 0.74, 1.46));
    }
    g.add(box(1.6, 0.1, 0.1, grey, 0, 0.5, 1.7));          // bómper
    // parabrisas abatible: marco fino y vidrio claro
    g.add(box(1.4, 0.06, 0.05, dark, 0, 1.72, 0.36));
    for (const s of [-1, 1]) g.add(box(0.05, 0.6, 0.05, dark, s * 0.68, 1.44, 0.36));
    const glass = new THREE.Mesh(boxGeo, new THREE.MeshLambertMaterial({ color: '#dfe9ea', transparent: true, opacity: 0.35 }));
    glass.scale.set(1.3, 0.54, 0.02); glass.position.set(0, 1.44, 0.36);
    g.add(glass);
    // timón y sillas rojo cereza
    const wheel = new THREE.Mesh(new THREE.TorusGeometry(0.17, 0.025, 6, 14), mat(dark));
    wheel.position.set(-0.36, 1.26, 0.12); wheel.rotation.x = -0.5;
    g.add(wheel);
    for (const s of [-1, 1]) {
      g.add(box(0.5, 0.12, 0.5, cherry, s * 0.36, 0.96, -0.3));
      g.add(box(0.5, 0.5, 0.1, cherry, s * 0.36, 1.24, -0.58));
    }
    // la carga del yipao: costales de fique con la franja verde de la red y un racimo de plátano
    for (const [x, y, z, r] of [[-0.36, 1.2, -1.02, 0], [0.36, 1.2, -1.05, 0.1], [0, 1.56, -1.04, 1.5]]) {
      const sack = box(0.62, 0.36, 0.78, '#efe8e1', x, y, z);
      sack.rotation.y = r;
      g.add(sack);
      const band = box(0.64, 0.04, 0.14, '#4f8f2c', x, y + 0.19, z);
      band.rotation.y = r;
      g.add(band);
    }
    g.add(box(0.26, 0.5, 0.26, '#7fae3c', 0.52, 1.5, -1.38));
    // llantas y repuesto
    const tyreGeo = new THREE.CylinderGeometry(0.38, 0.38, 0.26, 14);
    const hubGeo = new THREE.CylinderGeometry(0.16, 0.16, 0.28, 10);
    const wheels = [];
    for (const [x, z] of [[-0.74, 1.05], [0.74, 1.05], [-0.74, -1.0], [0.74, -1.0]]) {
      const w = new THREE.Group();
      const tyre = new THREE.Mesh(tyreGeo, mat(dark)); tyre.rotation.z = Math.PI / 2;
      const hub = new THREE.Mesh(hubGeo, mat(white)); hub.rotation.z = Math.PI / 2;
      const nut = box(0.3, 0.07, 0.07, cherry, 0, 0.1, 0);   // marca para que se vea girar
      w.add(tyre, hub, nut);
      w.position.set(x, 0.38, z);
      g.add(w);
      wheels.push(w);
    }
    const spare = new THREE.Mesh(tyreGeo, mat(dark));
    spare.rotation.x = Math.PI / 2;
    spare.position.set(0, 0.95, -1.62);
    g.add(spare);
    g.traverse((m) => { if (m.isMesh) m.castShadow = true; });
    g.userData.wheels = wheels;
    g.scale.setScalar(1.12);
    return g;
  }

  const Willys = {
    driving: false,
    pos: { x: 0, z: 0 }, heading: Math.PI / 2, speed: 0, y: 0,
    MAX_STREET,

    park() {
      const hq = W.World.hqSpot;
      // en la calle del sur de la terraza, a unos pasos de donde aparece el barista
      return { x: hq.x + 6, z: hq.z + C.BLOCK / 2 + 3.2, heading: Math.PI / 2 };
    },
    build(G) {
      this.mesh = makeWillys();
      G.scene.add(this.mesh);
      this.reset(G);
    },
    reset(G) {
      const pk = this.park();
      if (this.driving) this.leave(G, true);
      this.pos.x = pk.x; this.pos.z = pk.z; this.heading = pk.heading; this.speed = 0;
      this.place();
    },
    place() {
      const m = this.mesh;
      this.y = W.World.groundAt ? W.World.groundAt(this.pos.x, this.pos.z, 0.4) : 0;
      m.position.set(this.pos.x, this.y, this.pos.z);
      m.rotation.y = this.heading;
    },
    distTo(p) { return Math.hypot(this.pos.x - p.pos.x, this.pos.z - p.pos.z); },
    canBoard(G) { return !this.driving && !G.player.dead && this.distTo(G.player) < REACH; },

    // Acción: subir (a pie y cerca) o bajar (manejando). Devuelve true si la acción ya quedó atendida.
    interact(G) {
      const I = W.Input;
      if (this.driving) {
        G.prompt = '<kbd>E</kbd> Bajar del Willys';
        if (I.pressed('action')) this.leave(G);
        return true;
      }
      if (this.canBoard(G)) {
        G.prompt = '<kbd>E</kbd> Subir al Willys';
        if (I.pressed('action')) this.board(G);
        return true;
      }
      return false;
    },
    board(G) {
      const p = G.player;
      this.driving = true;
      this.speed = 0;
      p.dash = 0; p.sprinting = false; p.aiming = false; p.knock = null;
      p.h.body.rotation.x = 0;
      W.Audio.tone(110, 0.35, 0.25, 'sawtooth', 170);
      W.HUD.toast(W.Input.isTouch ? 'Willys · joystick para manejar · Bajar para salir' : 'Willys · WASD para manejar · E para bajar');
    },
    leave(G, silent) {
      const p = G.player;
      this.driving = false;
      this.speed = 0;
      // el barista baja por el lado del conductor; si hay pared, por el otro
      const side = (s) => ({ x: this.pos.x + Math.cos(this.heading) * 1.7 * s, z: this.pos.z - Math.sin(this.heading) * 1.7 * s });
      let out = side(1);
      if (W.World.solidAt(out.x, 0.6, out.z)) out = side(-1);
      p.pos.x = out.x; p.pos.z = out.z;
      W.World.collide(p.pos, C.PLAYER_RADIUS, p.y);
      p.vy = 0;
      if (!silent) W.Audio.tone(150, 0.2, 0.18, 'sawtooth', 90);
    },

    // Manejo: el joystick (o WASD) marca hacia dónde ir; el campero gira hacia allá y acelera.
    drive(G, dt) {
      const I = W.Input;
      const p = G.player;
      const td = W.TopDown.active;
      const look = I.look();
      if (td) { p.yaw = W.TopDown.aim(G, dt); p.pitch = 0; }
      else { p.yaw -= look.x; p.pitch = Math.max(-1.1, Math.min(0.75, p.pitch - look.y)); }
      p.aiming = false; p.sprinting = false; p.dash = 0;
      p.energy = Math.min(C.ENERGY_MAX, p.energy + C.ENERGY_REGEN * dt);

      const mv = I.move();
      const cy = td ? 0 : p.yaw;
      const fx = -Math.sin(cy), fz = -Math.cos(cy), rx = Math.cos(cy), rz = -Math.sin(cy);
      const dx = fx * mv.y + rx * mv.x, dz = fz * mv.y + rz * mv.x;
      const len = Math.min(1, Math.hypot(dx, dz));
      const onStreet = W.World.baseY(this.pos.x, this.pos.z) < 0.1;
      const max = onStreet ? MAX_STREET : MAX_OFF;
      let target = 0;
      if (len > 0.12) {
        const want = Math.atan2(dx, dz);
        let diff = want - this.heading;
        diff = Math.atan2(Math.sin(diff), Math.cos(diff));
        // gira más cerrado cuando va despacio; con el timón muy cruzado, afloja
        const rate = TURN * (1.5 - 0.6 * Math.min(1, this.speed / MAX_STREET));
        this.heading += Math.max(-rate * dt, Math.min(rate * dt, diff));
        target = max * len * (Math.abs(diff) > 1.6 ? 0.25 : Math.abs(diff) > 0.8 ? 0.6 : 1);
      }
      if (this.speed < target) this.speed = Math.min(target, this.speed + ACCEL * dt);
      else this.speed = Math.max(target, this.speed - BRAKE * dt);

      // avanza en pasos cortos para no atravesar muros delgados; si choca, pierde velocidad
      const steps = 4;
      const sx = Math.sin(this.heading), sz = Math.cos(this.heading);
      let blocked = 0;
      for (let i = 0; i < steps; i++) {
        const nx = this.pos.x + sx * this.speed * dt / steps, nz = this.pos.z + sz * this.speed * dt / steps;
        this.pos.x = nx; this.pos.z = nz;
        W.World.collide(this.pos, RADIUS, 0.3);
        blocked += Math.hypot(this.pos.x - nx, this.pos.z - nz);
      }
      if (blocked > 0.02 && this.speed > 4) {
        if (this.speed > 14) { G.shake(0.12); W.Audio.burst(180, 0.18, 0.4, 'lowpass'); }
        this.speed *= 0.45;
      }
      this.place();
      // ruedas que giran y un cabeceo leve al acelerar
      const spin = this.speed * dt / 0.38;
      for (const w of this.mesh.userData.wheels) w.rotation.x += spin;
      this.mesh.rotation.x = -Math.min(0.05, (target - this.speed) * 0.004);
      // polvo detrás cuando va rápido
      if (this.speed > 12 && Math.random() < 0.5) G.burst({ x: this.pos.x - sx * 1.9, y: this.y + 0.3, z: this.pos.z - sz * 1.9 }, '#ffffff', 1, 1.4);
      // motor: un ronroneo corto, más agudo con la velocidad
      this.hum = (this.hum || 0) - dt;
      if (this.hum <= 0 && this.speed > 1) { this.hum = 0.16; W.Audio.tone(55 + this.speed * 3.2, 0.17, 0.05, 'sawtooth'); }

      // el barista va sentado al timón; si rocía, gira el torso hacia la mira
      const combat = G.time - p.lastShot < 0.8 || I.held('fire') || (td && W.TopDown.target);
      const lx = -0.4, lz = -0.3;
      p.pos.x = this.pos.x; p.pos.z = this.pos.z;
      p.y = this.y; p.vy = 0; p.air = false;
      p.moving = 0;
      p.mesh.position.set(this.pos.x + Math.cos(this.heading) * lx + sx * lz, this.y + 0.52, this.pos.z - Math.sin(this.heading) * lx + sz * lz);
      p.mesh.rotation.y = W.Ent.lerpAngle(p.mesh.rotation.y, combat ? p.yaw + Math.PI : this.heading, Math.min(1, dt * 14));
      p.h.animate(0, 0, combat);
      p.h.body.rotation.x = 0;
    },

    // A pie: si el barista vuelve a la terraza y dejó el campero lejos, alguien de la red lo trae de vuelta.
    update(G, dt) {
      if (this.driving) return;
      this.mesh.rotation.x = 0;
      this.checkT = (this.checkT || 0) - dt;
      if (this.checkT > 0) return;
      this.checkT = 1;
      const hq = W.World.hqSpot, p = G.player, pk = this.park();
      if (Math.hypot(p.pos.x - hq.x, p.pos.z - hq.z) < 26 && Math.hypot(this.pos.x - pk.x, this.pos.z - pk.z) > 70) {
        this.reset(G);
        W.HUD.toast('La red trajo tu Willys a la terraza');
      }
    },
  };

  W.Willys = Willys;
})();
