// Plagas y enfermedades del café. No persiguen al barista: van por las matas.
//  · Nube de roya (Hemileia vastatrix): baja del cielo, se posa en una mata y la infecta. Las variedades
//    resistentes (Castillo, Tabi) casi siempre la rechazan. Una mata con roya avanzada suelta más esporas.
//  · Broca (Hypothenemus hampei): busca la cereza madura, la perfora y se va con la cosecha.
//  · Hormiga arriera (Atta): llega en fila por el piso, corta hojas y se las lleva; muerde si te atraviesas.
// Están agrandadas para poder apuntarles; el color es de ser vivo (naranja, café, rojizo), nunca azul Arcadia.
(function () {
  const C = W.CONFIG;
  const Ent = W.Ent;
  const M = W.Geom.matrix;
  const MAX = 7;
  const rand = (a, b) => a + Math.random() * (b - a);

  // ---------- mallas ----------
  const geo = (() => {
    const ico = new THREE.IcosahedronGeometry(1, 0);
    const sph = new THREE.SphereGeometry(1, 7, 5);
    const box = new THREE.BoxGeometry(1, 1, 1);
    let seed = 7;
    const rnd = () => (seed = (seed * 16807) % 2147483647) / 2147483647;
    // nube de esporas: racimo de poliedros naranja
    const roya = [];
    for (let i = 0; i < 18; i++) {
      const a = rnd() * 6.28, b = rnd() * 3.14 - 1.57, r = 0.15 + rnd() * 0.42, s = 0.1 + rnd() * 0.2;
      roya.push({ geo: ico, matrix: M(Math.cos(a) * Math.cos(b) * r, Math.sin(b) * r * 0.8, Math.sin(a) * Math.cos(b) * r, rnd() * 3, rnd() * 3, 0, s), color: ['#d9601a', '#e8791c', '#c4500f'][i % 3] });
    }
    roya.push({ geo: ico, matrix: M(0, 0, 0, 0.4, 0.2, 0, 0.3), color: '#a8420c' });
    // broca: cucarrón oscuro de cuerpo ovalado (mira hacia +z)
    const broca = [
      { geo: sph, matrix: M(0, 0, -0.08, 0, 0, 0, 0.27, 0.24, 0.4), color: '#2a1a12' },
      { geo: sph, matrix: M(0, 0.03, 0.27, 0, 0, 0, 0.22, 0.2, 0.2), color: '#3b2417' },
      { geo: sph, matrix: M(0, -0.03, 0.44, 0, 0, 0, 0.13, 0.12, 0.13), color: '#1c110b' },
      { geo: box, matrix: M(0, 0.16, -0.08, 0, 0, 0, 0.02, 0.2, 0.7), color: '#150c07' },
      { geo: box, matrix: M(0.07, 0.02, 0.6, 0.3, 0.5, 0, 0.03, 0.03, 0.2), color: '#1c110b' },
      { geo: box, matrix: M(-0.07, 0.02, 0.6, 0.3, -0.5, 0, 0.03, 0.03, 0.2), color: '#1c110b' },
    ];
    for (const sx of [-1, 1]) for (const z of [-0.2, 0.05, 0.28]) broca.push({ geo: box, matrix: M(sx * 0.3, -0.16, z, 0, 0, sx * 0.7, 0.26, 0.035, 0.035), color: '#1c110b' });
    // hormiga arriera: cabeza grande, tórax, abdomen y seis patas
    const ant = [
      { geo: sph, matrix: M(0, 0.42, 0.46, 0, 0, 0, 0.24, 0.2, 0.22), color: '#9a3a1a' },
      { geo: sph, matrix: M(0, 0.38, 0.08, 0, 0, 0, 0.15, 0.14, 0.24), color: '#7a2c14' },
      { geo: sph, matrix: M(0, 0.4, -0.38, 0, 0, 0, 0.24, 0.21, 0.3), color: '#6a2410' },
      { geo: box, matrix: M(0.1, 0.36, 0.7, 0, 0.5, 0, 0.05, 0.05, 0.2), color: '#3a1408' },
      { geo: box, matrix: M(-0.1, 0.36, 0.7, 0, -0.5, 0, 0.05, 0.05, 0.2), color: '#3a1408' },
      { geo: box, matrix: M(0.14, 0.62, 0.6, -0.7, 0.3, 0, 0.025, 0.025, 0.3), color: '#3a1408' },
      { geo: box, matrix: M(-0.14, 0.62, 0.6, -0.7, -0.3, 0, 0.025, 0.025, 0.3), color: '#3a1408' },
    ];
    for (const sx of [-1, 1]) for (const z of [-0.12, 0.08, 0.28]) {
      ant.push({ geo: box, matrix: M(sx * 0.26, 0.3, z, 0, 0, sx * -0.6, 0.34, 0.04, 0.04), color: '#3a1408' });
      ant.push({ geo: box, matrix: M(sx * 0.44, 0.12, z, 0, 0, sx * 1.2, 0.26, 0.035, 0.035), color: '#3a1408' });
    }
    const leaf = new THREE.SphereGeometry(0.5, 6, 3);
    leaf.scale(0.9, 0.06, 0.6);
    return { roya: W.Geom.merge(roya), broca: W.Geom.merge(broca), ant: W.Geom.merge(ant), leaf, box, speck: ico };
  })();
  const mats = {
    roya: new THREE.MeshLambertMaterial({ vertexColors: true, flatShading: true, emissive: '#5a2000', emissiveIntensity: 0.45 }),
    body: new THREE.MeshLambertMaterial({ vertexColors: true, flatShading: true }),
    wing: new THREE.MeshLambertMaterial({ color: '#efe6cf', transparent: true, opacity: 0.75, side: THREE.DoubleSide }),
    leaf: new THREE.MeshLambertMaterial({ color: '#4f9a2e', flatShading: true }),
    halo: new THREE.MeshBasicMaterial({ color: '#e8791c', transparent: true, opacity: 0.16, depthWrite: false }),
  };
  const haloGeo = new THREE.CircleGeometry(1.1, 24);

  class Pest {
    constructor(kind, x, z, target, opts) {
      const t = W.PLAGAS[kind];
      this.pest = true;
      this.kind = kind;
      this.t = t;
      this.name = t.name;
      this.serial = t.short;
      this.maxHp = t.hp;
      this.hp = t.hp;
      this.pos = { x, z };
      this.home = { x, z };
      this.target = target;
      this.group = target ? target.group : 'hq';
      this.speed = t.speed * rand(0.9, 1.1);
      this.radius = 0.4;
      this.alerted = false;
      this.state = 'approach';
      this.timer = 0;
      this.ko = 0;
      this.cool = 0;
      this.delay = (opts && opts.delay) || 0;
      this.phase = Math.random() * 6;
      this.stolen = 0;
      this.mesh = new THREE.Group();
      this.body = new THREE.Group();
      this.mesh.add(this.body);
      if (kind === 'roya') {
        this.alt = (opts && opts.alt) || 9; this.hitR = 0.72;
        this.body.add(new THREE.Mesh(geo.roya, mats.roya));
        // esporas sueltas que giran alrededor
        this.specks = [0, 1, 2].map((i) => {
          const s = new THREE.Mesh(geo.speck, mats.roya);
          s.scale.setScalar(0.07 + i * 0.02);
          this.body.add(s);
          return s;
        });
        this.halo = new THREE.Mesh(haloGeo, mats.halo);
        this.halo.rotation.x = -Math.PI / 2;
        this.mesh.add(this.halo);
      } else if (kind === 'broca') {
        this.alt = (opts && opts.alt) || 8; this.hitR = 0.58;
        this.body.add(new THREE.Mesh(geo.broca, mats.body));
        this.wings = [-1, 1].map((sx) => {
          const piv = new THREE.Group();
          const w = new THREE.Mesh(geo.box, mats.wing);
          w.scale.set(0.62, 0.015, 0.3);
          w.position.x = sx * 0.33;
          piv.position.set(sx * 0.08, 0.2, 0);
          piv.add(w);
          this.body.add(piv);
          return piv;
        });
        this.body.scale.setScalar(1.15);
      } else {
        this.alt = 0.38; this.hitR = 0.62; this.ground = true;
        this.body.add(new THREE.Mesh(geo.ant, mats.body));
        this.leafMesh = new THREE.Mesh(geo.leaf, mats.leaf);
        this.leafMesh.position.set(0, 0.95, 0.25);
        this.leafMesh.rotation.z = 1.2;
        this.leafMesh.visible = false;
        this.body.add(this.leafMesh);
        this.body.scale.setScalar(1.2);
      }
      this.body.traverse((o) => { if (o.isMesh) o.castShadow = true; });
      this.bar = Ent.makeBar(0.7, '#e8791c');
      this.bar.visible = false;
      this.mesh.add(this.bar);
      this.h = { body: this.body };
      this.place();
    }
    get dead() { return this.hp <= 0; }
    hit(dmg) {
      this.hp -= dmg;
      this.flash = 0.12;
      if (this.dead) this.bar.visible = false;
    }
    place() {
      const by = W.World.baseY(this.pos.x, this.pos.z);
      this.mesh.position.set(this.pos.x, by, this.pos.z);
      this.body.position.y = this.ground ? 0 : this.alt;
      this.bar.position.y = this.alt + 0.95;
      if (this.halo) this.halo.position.y = 0.05;
    }
    // vuela derecho hacia un punto (las plagas voladoras pasan por encima de las camas)
    fly(tx, tz, alt, dt, sp) {
      const dx = tx - this.pos.x, dz = tz - this.pos.z;
      const d = Math.hypot(dx, dz);
      const wob = Math.sin(this.phase * 0.7) * (this.kind === 'broca' ? 0.55 : 0.2);
      if (d > 0.05) {
        const step = Math.min(d, (sp || this.speed) * dt);
        const nx = dx / d, nz = dz / d;
        this.pos.x += nx * step + -nz * wob * step * (d > 2 ? 1 : 0);
        this.pos.z += nz * step + nx * wob * step * (d > 2 ? 1 : 0);
        this.mesh.rotation.y = Ent.lerpAngle(this.mesh.rotation.y, Math.atan2(dx, dz), Math.min(1, dt * 8));
      }
      this.alt += (alt - this.alt) * Math.min(1, dt * 1.6);
      return d;
    }
    pick(G, test, score) {
      let best = null, bs = -Infinity;
      for (const pl of G.planters) {
        if (pl.group !== this.group || !test(pl)) continue;
        const s = score ? score(pl) : -Math.hypot(pl.pos.x - this.pos.x, pl.pos.z - this.pos.z);
        if (s > bs) { bs = s; best = pl; }
      }
      return best;
    }

    update(dt, G) {
      if (this.gone) return true;
      this.phase += dt * 10;
      if (this.dead) {
        this.ko += dt;
        const k = Math.max(0, 1 - this.ko * 1.6);
        this.body.scale.setScalar(Math.max(0.01, k) * (this.kind === 'roya' ? 1 : 1.15));
        if (!this.ground) this.body.position.y = Math.max(0.2, this.body.position.y - dt * 4);
        else this.body.rotation.z = Math.min(Math.PI, this.ko * 6);
        if (this.halo) this.halo.visible = false;
        return this.ko > 0.7;
      }
      if (this.delay > 0) { this.delay -= dt; this.place(); return false; }
      if (this.kind === 'roya') this.updateRoya(dt, G);
      else if (this.kind === 'broca') this.updateBroca(dt, G);
      else this.updateAnt(dt, G);
      this.place();
      if (this.flash > 0) { this.flash = Math.max(0, this.flash - dt); this.body.scale.multiplyScalar(1 + this.flash * 2); }
      Ent.setBar(this.bar, this.hp / this.maxHp);
      this.bar.visible = this.hp < this.maxHp;
      return !!this.gone;
    }

    updateRoya(dt, G) {
      const tg = this.target;
      const top = (pl) => pl.mesh.position.y + 1.1;
      this.body.rotation.y += dt * 0.9;
      this.body.rotation.x = Math.sin(this.phase * 0.12) * 0.25;
      this.specks.forEach((s, i) => { const a = this.phase * (0.18 + i * 0.05) + i * 2.1; s.position.set(Math.cos(a) * 0.75, Math.sin(a * 1.3) * 0.3, Math.sin(a) * 0.75); });
      let sc = 1 + Math.sin(this.phase * 0.3) * 0.06;
      if (this.state === 'approach') {
        if (!tg || tg.roya > 0) {
          this.target = this.pick(G, (pl) => pl.roya === 0);
          if (!this.target) this.state = 'leave';
          return;
        }
        const d = this.fly(tg.pos.x, tg.pos.z, d0(this, tg) < 7 ? top(tg) + 0.5 : 2.2, dt);
        if (d < 0.5 && this.alt < top(tg) + 1.3) { this.state = 'settle'; this.timer = this.t.settle; }
      } else if (this.state === 'settle') {
        // se posa sobre la mata: es el último momento para apagarla
        this.timer -= dt;
        this.fly(tg.pos.x, tg.pos.z, top(tg) - 0.1, dt);
        sc *= 0.55 + 0.45 * Math.max(0, this.timer / this.t.settle);
        if (this.timer <= 0) {
          const res = tg.infect();
          const where = tg.label ? `la fila ${tg.label}` : 'el cultivo';
          if (res === 'infectada') {
            W.HUD.toast(`Roya en ${where} · acércate y trátala`);
            W.HUD.killFeed(`<b class="foe plaga">Roya</b> infectó <b>${tg.label || 'un cultivo'}</b> · ${tg.variety.name}`);
            W.Audio.hurt();
          } else if (res === 'resiste') {
            W.HUD.toast(`${tg.variety.name} resistió la roya en ${where}`);
            W.HUD.gain(tg.pos.x, tg.mesh.position.y + 1.8, tg.pos.z, 'resiste');
          }
          G.burst({ x: this.pos.x, y: this.mesh.position.y + this.alt, z: this.pos.z }, '#e8791c', 10, 2.5);
          this.gone = true;
        }
      } else {
        this.alt += dt * 3;
        sc *= Math.max(0.05, 1 - (this.alt - 3) / 8);
        if (this.alt > 11) this.gone = true;
      }
      this.body.scale.setScalar(sc);
      this.halo.scale.setScalar(0.5 + sc * 0.5);
    }

    updateBroca(dt, G) {
      const tg = this.target;
      this.wings[0].rotation.z = Math.sin(this.phase * 4) * 0.7 + 0.3;
      this.wings[1].rotation.z = -Math.sin(this.phase * 4) * 0.7 - 0.3;
      this.body.rotation.x = 0;
      if (this.state === 'approach') {
        this.timer += dt;
        if (!tg || !tg.ripe) {
          // espera la cereza madura dando vueltas sobre el cultivo
          const ripe = this.pick(G, (pl) => pl.ripe);
          if (ripe) { this.target = ripe; return; }
          const any = this.pick(G, () => true, (pl) => pl.growth);
          if (!any || this.timer > 26) { this.state = 'flee'; this.timer = 0; return; }
          const a = this.phase * 0.09;
          this.fly(any.pos.x + Math.cos(a) * 3.2, any.pos.z + Math.sin(a) * 3.2, 2.6, dt, this.speed * 0.6);
          return;
        }
        const top = tg.mesh.position.y + 0.95;
        const d = this.fly(tg.pos.x, tg.pos.z, d0(this, tg) < 6 ? top + 0.3 : 2.4, dt);
        if (d < 0.45 && this.alt < top + 1) { this.state = 'bore'; this.timer = this.t.bore; }
      } else if (this.state === 'bore') {
        this.timer -= dt;
        this.alt += (tg.mesh.position.y + 0.95 - this.alt) * Math.min(1, dt * 4);
        this.wings[0].rotation.z = 0.15; this.wings[1].rotation.z = -0.15;
        this.body.rotation.x = 0.5 + Math.sin(this.phase * 2.2) * 0.12; // cabeza abajo, perforando
        if (!tg.ripe) { this.state = 'approach'; this.timer = 0; this.target = null; return; }
        if (this.timer <= 0) {
          this.stolen = G.plantYield(tg);
          tg.growth = 0.2; tg.pop = 1;
          W.HUD.toast(`La broca perforó ${tg.label ? `la fila ${tg.label}` : 'un cultivo'} · se lleva ${this.stolen} cerezas`);
          W.HUD.killFeed(`<b class="foe plaga">Broca</b> perforó <b>${tg.label || 'un cultivo'}</b>`);
          G.burst({ x: tg.pos.x, y: tg.mesh.position.y + 0.9, z: tg.pos.z }, '#b3121a', 8, 3);
          const dx = this.pos.x - G.player.pos.x, dz = this.pos.z - G.player.pos.z, d = Math.hypot(dx, dz) || 1;
          this.fleeTo = { x: this.pos.x + (dx / d) * 60, z: this.pos.z + (dz / d) * 60 };
          this.state = 'flee'; this.timer = 0;
        }
      } else {
        this.timer += dt;
        const f = this.fleeTo || { x: this.home.x, z: this.home.z };
        this.fly(f.x, f.z, 2.2 + this.timer * 1.4, dt, this.speed * (this.stolen ? 0.8 : 1));
        if (this.timer > 7) this.gone = true;
      }
    }

    updateAnt(dt, G) {
      const p = G.player;
      const tg = this.target;
      let mx = 0, mz = 0;
      this.cool -= dt;
      if (!p.dead && this.cool <= 0 && Math.hypot(p.pos.x - this.pos.x, p.pos.z - this.pos.z) < 1.5 && Math.abs(p.y - W.World.baseY(this.pos.x, this.pos.z)) < 1) {
        this.cool = 1.3;
        G.damagePlayer(this.t.bite, this);
      }
      if (this.state === 'approach') {
        if (!tg) { this.state = 'return'; return; }
        const dx = tg.pos.x - this.pos.x, dz = tg.pos.z - this.pos.z, d = Math.hypot(dx, dz) || 1;
        this.walked = (this.walked || 0) + dt;
        if (d < (tg.bed ? 3.0 : 1.8) || (d < 5 && this.avoid > 0) || this.walked > 22) { this.state = 'cut'; this.timer = this.t.cut; }
        else { mx = dx / d; mz = dz / d; }
      } else if (this.state === 'cut') {
        this.timer -= dt;
        this.body.rotation.x = Math.sin(this.phase * 2) * 0.12;
        if (this.timer <= 0) {
          this.body.rotation.x = 0;
          tg.leaf = Math.max(0.3, tg.leaf - 0.2);
          tg.pop = 0.6;
          this.leafMesh.visible = true;
          G.burst({ x: tg.pos.x, y: tg.mesh.position.y + 0.9, z: tg.pos.z }, '#4f9a2e', 5, 2);
          if (!tg.antWarned || G.time - tg.antWarned > 12) { tg.antWarned = G.time; W.HUD.toast(`Las arrieras están pelando ${tg.label ? `la fila ${tg.label}` : 'un cultivo'}`); }
          this.state = 'return';
        }
      } else {
        const dx = this.home.x - this.pos.x, dz = this.home.z - this.pos.z, d = Math.hypot(dx, dz) || 1;
        this.back = (this.back || 0) + dt;
        if (d < 1.6 || this.back > 20) { this.gone = true; return; }
        mx = dx / d; mz = dz / d;
      }
      Ent.steer(this, mx, mz, this.speed * (this.leafMesh.visible ? 0.8 : 1), dt);
      if (Math.hypot(mx, mz) > 0.05) {
        this.mesh.rotation.y = Ent.lerpAngle(this.mesh.rotation.y, Math.atan2(mx, mz), Math.min(1, dt * 8));
        this.body.position.x = Math.sin(this.phase * 1.4) * 0.03;
        this.body.rotation.z = Math.sin(this.phase * 1.4) * 0.07;
      }
    }
  }
  function d0(a, pl) { return Math.hypot(pl.pos.x - a.pos.x, pl.pos.z - a.pos.z); }

  W.Plagas = {
    Pest,
    timer: 22,
    reset() { this.timer = 22; },
    alive(G) { let n = 0; for (const e of G.enemies) if (e.pest && !e.dead) n++; return n; },

    spawn(G, kind, x, z, target, opts) {
      const e = new Pest(kind, x, z, target, opts);
      G.scene.add(e.mesh);
      G.enemies.push(e);
      return e;
    },

    update(dt, G) {
      const p = G.player;
      if (p.dead || G.boss) return;
      const alive = this.alive(G);
      // una mata con roya avanzada suelta esporas hacia la vecina sana
      for (const pl of G.planters) {
        if (pl.roya < 0.6) continue;
        if (Math.hypot(pl.pos.x - p.pos.x, pl.pos.z - p.pos.z) > 70) continue;
        pl.spore += dt;
        if (pl.spore > 15 && alive < MAX) {
          pl.spore = 0;
          let best = null, bd = 14;
          for (const o of G.planters) {
            if (o === pl || o.group !== pl.group || o.roya > 0) continue;
            const d = Math.hypot(o.pos.x - pl.pos.x, o.pos.z - pl.pos.z);
            if (d < bd) { bd = d; best = o; }
          }
          if (best) {
            this.spawn(G, 'roya', pl.pos.x, pl.pos.z, best, { alt: pl.mesh.position.y + 1.4 });
            G.burst({ x: pl.pos.x, y: pl.mesh.position.y + 1.3, z: pl.pos.z }, '#e8791c', 8, 2);
          }
        }
      }
      this.timer -= dt;
      if (this.timer > 0) return;
      // el brote cae sobre el cultivo más cercano al barista (si está lejos de todos, no pasa nada a sus espaldas)
      let near = null, nd = 55;
      for (const pl of G.planters) {
        const d = Math.hypot(pl.pos.x - p.pos.x, pl.pos.z - p.pos.z);
        if (d < nd) { nd = d; near = pl; }
      }
      if (!near || alive > MAX - 3) { this.timer = 6; return; }
      const group = G.planters.filter((pl) => pl.group === near.group);
      // la roya prefiere la lluvia; la broca, el tiempo seco y la cereza madura
      const w = { roya: G.raining ? 3 : 1.3, broca: (G.raining ? 0.6 : 1.5) * (group.some((pl) => pl.growth > 0.55) ? 1 : 0.2), arriera: 1 };
      if (!group.some((pl) => pl.roya === 0)) w.roya = 0;
      let r = Math.random() * (w.roya + w.broca + w.arriera);
      const kind = (r -= w.roya) < 0 ? 'roya' : (r -= w.broca) < 0 ? 'broca' : 'arriera';
      this.outbreak(G, kind, group);
      this.timer = rand(36, 54) - Math.min(12, G.capturedCount() * 2);
    },

    // Suelta un brote sobre un grupo de matas (las filas de la terraza o el cultivo de una zona liberada)
    outbreak(G, kind, group) {
      if (!group || !group.length) return [];
      const c = { x: 0, z: 0 };
      for (const pl of group) { c.x += pl.pos.x / group.length; c.z += pl.pos.z / group.length; }
      const n = G.capturedCount();
      const out = [];
      if (kind === 'roya') {
        const healthy = group.filter((pl) => pl.roya === 0).sort(() => Math.random() - 0.5);
        const count = Math.min(healthy.length, 2 + Math.floor(n / 3));
        for (let i = 0; i < count; i++) {
          const a = Math.random() * 6.28;
          out.push(this.spawn(G, 'roya', c.x + Math.cos(a) * 15, c.z + Math.sin(a) * 15, healthy[i], { delay: i * 1.2 }));
        }
        W.HUD.banner('Brote de roya', 'Apaga las nubes de esporas antes de que se posen en las matas', 'plaga');
      } else if (kind === 'broca') {
        const count = 2 + (n > 2 ? 1 : 0);
        const ripe = group.slice().sort((a, b) => b.growth - a.growth);
        for (let i = 0; i < count; i++) {
          const a = Math.random() * 6.28;
          out.push(this.spawn(G, 'broca', c.x + Math.cos(a) * 17, c.z + Math.sin(a) * 17, ripe[i % ripe.length], { delay: i * 0.8 }));
        }
        W.HUD.banner('Llegó la broca', 'Va por la cereza madura: cosecha o apágala antes de que perfore', 'plaga');
      } else {
        const tg = group[Math.floor(Math.random() * group.length)];
        // el hormiguero queda en el borde de la plaza: busca un punto de piso libre
        let sx = c.x, sz = c.z + 17;
        for (let i = 0; i < 14; i++) {
          const a = Math.random() * 6.28, x = tg.pos.x + Math.cos(a) * 15, z = tg.pos.z + Math.sin(a) * 15;
          const t = { x, z };
          W.World.collide(t, 0.6, 0);
          if (Math.hypot(t.x - x, t.z - z) < 0.05 && W.World.baseY(x, z) < 0.6) { sx = x; sz = z; break; }
        }
        const count = 3 + (n > 3 ? 1 : 0);
        for (let i = 0; i < count; i++) out.push(this.spawn(G, 'arriera', sx, sz, tg, { delay: i * 1.1 }));
        W.HUD.banner('Hormigas arrieras', `Van en fila a pelar ${tg.label ? `la fila ${tg.label}` : 'el cultivo'}`, 'plaga');
      }
      W.Audio.stamp();
      return out;
    },
  };
})();
