// Lógica de una ciudad: cultivo, preparación, clientes y la Federación.
(function () {
  const C = FG.CONFIG;
  const R = FG.Render;
  const S = FG.STATIONS;

  const rand = (a, b) => a + Math.random() * (b - a);
  const pick = (arr) => arr[Math.floor(Math.random() * arr.length)];
  const dist = (a, b) => Math.hypot(a.x - b.x, a.y - b.y);
  const clamp = (v, a, b) => Math.max(a, Math.min(b, v));

  function computeStats(up) {
    const lvl = (id) => up[id] || 0;
    return {
      plantCount: 6 + lvl('torres') * 2,
      growthMul: 1 + lvl('nutrientes') * 0.2,
      brewTime: C.BASE_BREW_TIME / (1 + lvl('aeropress') * 0.25),
      cupsPerBrew: lvl('aeropress') >= 2 ? 2 : 1,
      shotCooldown: C.BASE_SHOT_COOLDOWN * Math.pow(0.8, lvl('presion')),
      damage: lvl('presion') >= 3 ? 2 : 1,
      speed: C.BASE_SPEED * (1 + lvl('tenis') * 0.12),
      helpers: lvl('colaborador'),
      spawnMul: 1 + lvl('marketing') * 0.15,
    };
  }

  class Plant {
    constructor(x, y) {
      this.x = x; this.y = y;
      this.stage = 1; this.growth = 0; this.withered = 0;
      this.sway = Math.random() * 6;
    }
    get ripe() { return this.stage === 3 && this.withered <= 0; }
    update(dt, rate) {
      if (this.withered > 0) {
        this.withered -= dt;
        if (this.withered <= 0) { this.stage = 1; this.growth = 0; }
        return;
      }
      if (this.stage < 3 && rate > 0) {
        this.growth += dt * rate;
        if (this.growth >= C.STAGE_TIME) { this.growth = 0; this.stage++; }
      }
    }
    harvest() { this.stage = 1; this.growth = 0; }
    confiscate() { this.withered = C.WITHER_TIME; this.stage = 0; this.growth = 0; }
  }

  class Level {
    constructor(game, cityIndex) {
      this.game = game;
      this.cityIndex = cityIndex;
      this.city = FG.CITIES[cityIndex];
      this.stats = computeStats(game.save.upgrades);
      this.bg = R.buildBackground(this.city);

      this.time = C.LEVEL_TIME;
      this.conquest = C.START_CONQUEST;
      this.cherries = C.START_CHERRIES;
      this.cups = 0;
      this.nutrients = 100;

      this.player = {
        x: 700, y: 195, facing: 'down', moving: false, walk: 0,
        stun: 0, invuln: 0, aimX: 0, aimY: 1,
      };
      this.plants = FG.PLANT_SLOTS.slice(0, this.stats.plantCount).map((s, i) => {
        const p = new Plant(s.x, s.y);
        p.stage = i < 2 ? 3 : 1 + (i % 2);
        p.growth = Math.random() * C.STAGE_TIME;
        return p;
      });
      this.customers = [];
      this.inspectors = [];
      this.projectiles = [];
      this.stamps = [];
      this.texts = [];
      this.particles = [];
      this.boss = null;
      this.bossDefeated = false;

      this.customerTimer = 2;
      this.enemyTimer = this.city.enemyFirst;
      this.helperTimers = Array.from({ length: this.stats.helpers }, (_, i) => 4 + i * 3);
      this.brew = 0;
      this.shotTimer = 0;
      this.prompt = null;
      this.banner = { text: this.city.intro, t: 4, color: null };
      this.finished = false;
      this.earned = { money: 0, puntos: 0, served: 0, lost: 0, harvested: 0, repelled: 0, bosses: 0, confiscated: 0 };
    }

    // ---------- utilidades ----------
    addPoints(n, x, y) {
      const s = this.game.save;
      s.puntos += n;
      s.puntosTotal += n;
      this.earned.puntos += n;
      if (x !== undefined) this.text(`+${n} ★`, x, y, '#9fe8bf');
    }
    addConquest(n) {
      const cap = this.bossDefeated ? 100 : C.CAP_WITHOUT_BOSS;
      if (n > 0) this.conquest = Math.max(this.conquest, Math.min(this.conquest + n, cap));
      else this.conquest = Math.max(0, this.conquest + n);
    }
    text(text, x, y, color, size) {
      this.texts.push({ text, x, y, color: color || '#fff', life: 1.2, size });
    }
    burst(x, y, color, n) {
      for (let i = 0; i < n; i++) {
        const a = Math.random() * Math.PI * 2;
        const sp = rand(40, 140);
        this.particles.push({ x, y, vx: Math.cos(a) * sp, vy: Math.sin(a) * sp - 40, life: rand(0.4, 0.8), color, r: rand(2, 4) });
      }
    }
    showBanner(text, t, color) { this.banner = { text, t: t || 2.5, color }; }

    // ---------- bucle ----------
    update(dt) {
      if (this.finished) return;
      this.time -= dt;
      if (this.banner) { this.banner.t -= dt; if (this.banner.t <= 0) this.banner = null; }

      this.updatePlayer(dt);
      this.updateGarden(dt);
      this.updateCustomers(dt);
      this.updateEnemies(dt);
      this.updateProjectiles(dt);
      this.updateEffects(dt);

      if (!this.boss && !this.bossDefeated && this.conquest >= C.BOSS_AT) this.spawnBoss();
      if (this.conquest >= 100 && this.bossDefeated) this.finish(true);
      else if (this.time <= 0) this.finish(false);
    }

    finish(won) {
      this.finished = true;
      this.game.onLevelEnd(won, this);
    }

    updatePlayer(dt) {
      const p = this.player;
      const inp = FG.Input;
      p.stun = Math.max(0, p.stun - dt);
      p.invuln = Math.max(0, p.invuln - dt);
      this.shotTimer -= dt;

      const ax = p.stun > 0 ? { x: 0, y: 0 } : inp.axis();
      p.moving = ax.x !== 0 || ax.y !== 0;
      if (p.moving) {
        p.x = clamp(p.x + ax.x * this.stats.speed * dt, 16, C.WIDTH - 16);
        p.y = clamp(p.y + ax.y * this.stats.speed * dt, C.HUD_HEIGHT + 50, C.HEIGHT - 6);
        p.walk += dt * 14;
        const len = Math.hypot(ax.x, ax.y);
        p.aimX = ax.x / len; p.aimY = ax.y / len;
        if (Math.abs(ax.x) > Math.abs(ax.y)) p.facing = ax.x > 0 ? 'right' : 'left';
        else p.facing = ax.y > 0 ? 'down' : 'up';
      }

      // Disparo de presión con la AeroPress
      if (p.stun <= 0 && inp.held('shoot') && this.shotTimer <= 0) {
        this.shotTimer = this.stats.shotCooldown;
        this.projectiles.push({
          x: p.x + p.aimX * 16, y: p.y - 22 + p.aimY * 10,
          vx: p.aimX * C.SHOT_SPEED, vy: p.aimY * C.SHOT_SPEED, life: 0.9, dmg: this.stats.damage,
        });
      }

      this.handleInteraction(dt);
    }

    handleInteraction(dt) {
      const p = this.player;
      const inp = FG.Input;
      const feet = { x: p.x, y: p.y - 10 };
      this.prompt = null;
      this.highlight = null;
      if (p.stun > 0) return;

      // 1) Plantas maduras: ¡a farmear!
      let best = null, bestD = 52;
      for (const pl of this.plants) {
        const d = dist(feet, pl);
        if (d < bestD && pl.ripe) { best = pl; bestD = d; }
      }
      if (best) {
        this.highlight = best;
        this.prompt = '[E] Cosechar';
        if (inp.pressed('action')) this.harvest(best, false);
        return;
      }

      // 2) Tanque de nutrientes
      if (dist(feet, S.tank) < S.tank.r) {
        this.prompt = this.nutrients >= 99 ? 'Tanque lleno' : '[E] Recargar nutrientes';
        if (inp.pressed('action') && this.nutrients < 99) {
          this.nutrients = 100;
          this.text('¡Nutrientes al 100%!', S.tank.x, S.tank.y - 60, '#9fe8bf');
          this.burst(S.tank.x, S.tank.y - 20, '#3fbf8f', 10);
        }
        return;
      }

      // 3) AeroPress: mantener E para preparar
      if (dist(feet, S.aeropress) < S.aeropress.r) {
        if (this.cups >= C.MAX_CUPS) this.prompt = 'Manos llenas: ¡a servir!';
        else if (this.cherries < C.CHERRIES_PER_BREW) this.prompt = `Necesitas ${C.CHERRIES_PER_BREW} cerezas`;
        else {
          this.prompt = '[Mantén E] Prensar AeroPress';
          if (inp.held('action')) {
            this.brew += dt;
            if (this.brew >= this.stats.brewTime) {
              this.brew = 0;
              this.cherries -= C.CHERRIES_PER_BREW;
              const made = Math.min(this.stats.cupsPerBrew, C.MAX_CUPS - this.cups);
              this.cups += made;
              this.text(`+${made} ☕`, S.aeropress.x, S.aeropress.y - 50, '#fff3dc', 16);
              this.burst(S.aeropress.x, S.aeropress.y - 30, '#6b3b1f', 8);
            }
          } else {
            this.brew = Math.max(0, this.brew - dt * 2);
          }
        }
        return;
      }
      this.brew = 0;

      // 4) Barra: servir al primer cliente de la fila
      const k = S.counter;
      const nearCounter = p.x > k.x - 20 && p.x < k.x + k.w + 20 && p.y > k.y - 50 && p.y < k.y + 90;
      if (nearCounter) {
        const waiting = this.queue().find((c) => c.state === 'waiting');
        if (!waiting) this.prompt = 'Nadie en la fila';
        else if (this.cups <= 0) this.prompt = 'Sin café: prepara en la AeroPress';
        else {
          this.prompt = '[E] Servir café';
          if (inp.pressed('action')) this.serve(waiting);
        }
      }
    }

    harvest(plant, byHelper) {
      const n = C.BASE_YIELD + (this.stats.growthMul > 1.3 ? 1 : 0);
      plant.harvest();
      this.cherries += n;
      this.earned.harvested += n;
      this.game.save.stats.harvested += n;
      this.burst(plant.x, plant.y - 20, R.COLORS.cherry, 10);
      this.text(`+${n} 🍒`, plant.x, plant.y - 40, '#ffb3ad');
      this.addPoints(n * C.POINTS_PER_CHERRY, byHelper ? undefined : plant.x, byHelper ? undefined : plant.y - 58);
    }

    serve(c) {
      this.cups--;
      const fast = c.patience / c.maxPatience > 0.6;
      const price = Math.round(C.BASE_PRICE * this.city.priceMul * c.type.payMul / 100) * 100;
      const tip = fast ? Math.round(price * 0.2 / 100) * 100 : 0;
      this.game.save.money += price + tip;
      this.earned.money += price + tip;
      this.earned.served++;
      this.game.save.stats.served++;
      this.addConquest(C.SERVE_CONQUEST + (fast ? C.FAST_SERVE_BONUS : 0));
      this.addPoints(C.POINTS_PER_SERVE);
      this.text(`+${R.formatMoney(price + tip)}${tip ? ' (propina)' : ''}`, c.x, c.y - 90, '#ffe08a');
      c.state = 'leaving';
      c.mood = 'happy';
      this.reflowQueue();
    }

    // ---------- jardín ----------
    updateGarden(dt) {
      const drain = C.NUTRIENT_DRAIN + this.plants.length * C.NUTRIENT_DRAIN_PER_PLANT;
      const wasEmpty = this.nutrients <= 0;
      this.nutrients = Math.max(0, this.nutrients - drain * dt);
      if (this.nutrients <= 0 && !wasEmpty) this.showBanner('¡Sin nutrientes! Las plantas dejaron de crecer', 2.5, 'rgba(160,90,20,0.92)');
      const rate = this.nutrients > 0 ? this.stats.growthMul : 0;
      for (const pl of this.plants) pl.update(dt, rate);

      // Cultivadores colaborativos
      for (let i = 0; i < this.helperTimers.length; i++) {
        this.helperTimers[i] -= dt;
        if (this.helperTimers[i] <= 0) {
          this.helperTimers[i] = 7;
          const ripe = this.plants.filter((p) => p.ripe);
          if (ripe.length) {
            const pl = pick(ripe);
            this.harvest(pl, true);
            this.text('🤝 Cosecha colaborativa', pl.x, pl.y - 62, '#bff0cf', 12);
          }
        }
      }
    }

    // ---------- clientes ----------
    queue() { return this.customers.filter((c) => c.state !== 'leaving'); }

    slotPos(i) { return { x: 645 + i * 60, y: 312 }; }

    reflowQueue() {
      this.queue().forEach((c, i) => {
        c.slot = i;
        const s = this.slotPos(i);
        c.tx = s.x; c.ty = s.y;
        if (c.state === 'waiting' && (Math.abs(c.x - s.x) > 2)) c.state = 'walking';
      });
    }

    updateCustomers(dt) {
      this.customerTimer -= dt;
      if (this.customerTimer <= 0) {
        this.customerTimer = this.city.customerEvery / this.stats.spawnMul * rand(0.8, 1.2);
        if (this.queue().length < C.MAX_QUEUE) {
          const type = pick(FG.CUSTOMER_TYPES);
          const patience = this.city.patience * type.patienceMul;
          const c = { x: S.door.x, y: S.door.y + 10, type, state: 'walking', patience, maxPatience: patience, walk: 0, mood: null, slot: 0, tx: 0, ty: 0 };
          this.customers.push(c);
          this.reflowQueue();
        }
      }

      for (const c of this.customers) {
        if (c.state === 'walking' || c.state === 'leaving') {
          const tx = c.state === 'leaving' ? S.door.x + 30 : c.tx;
          const ty = c.state === 'leaving' ? S.door.y + 10 : c.ty;
          const dx = tx - c.x, dy = ty - c.y;
          const d = Math.hypot(dx, dy);
          const sp = 95 * dt;
          c.walk += dt * 12;
          if (d <= sp) {
            c.x = tx; c.y = ty;
            if (c.state === 'walking') c.state = 'waiting';
            else c.gone = true;
          } else {
            c.x += (dx / d) * sp; c.y += (dy / d) * sp;
          }
        }
        if (c.state === 'waiting' || c.state === 'walking') {
          c.patience -= dt;
          if (c.patience <= 0) this.loseCustomer(c, 'Se cansó de esperar');
        }
      }
      const before = this.customers.length;
      this.customers = this.customers.filter((c) => !c.gone);
      if (this.customers.length !== before) this.reflowQueue();
    }

    loseCustomer(c, why) {
      if (c.state === 'leaving') return;
      c.state = 'leaving';
      c.mood = 'angry';
      this.earned.lost++;
      this.addConquest(C.LOST_CUSTOMER);
      this.text(why, c.x, c.y - 90, '#ff9a8f', 12);
      this.reflowQueue();
    }

    // ---------- la Federación ----------
    updateEnemies(dt) {
      this.enemyTimer -= dt;
      if (this.enemyTimer <= 0) {
        this.enemyTimer = this.city.enemyEvery * rand(0.8, 1.2);
        if (this.inspectors.filter((e) => !e.fleeing).length < this.city.maxEnemies) this.spawnInspector();
      }

      for (const e of this.inspectors) this.updateInspector(e, dt);
      this.inspectors = this.inspectors.filter((e) => !e.gone);
      if (this.boss) this.updateBoss(this.boss, dt);
    }

    spawnInspector() {
      const left = Math.random() < 0.5;
      const hp = this.city.enemyHp;
      this.inspectors.push({
        x: left ? -20 : C.WIDTH + 20, y: rand(500, 580), hp, maxHp: hp,
        speed: this.city.enemySpeed * rand(0.9, 1.15), target: null, facing: left ? 'right' : 'left',
        walk: 0, hitFlash: 0, say: 2.2, line: pick(FG.INSPECTOR_LINES), fleeing: false, mustache: Math.random() < 0.5,
        kx: 0, ky: 0,
      });
    }

    pickTarget() {
      const plants = this.plants.filter((p) => p.withered <= 0 && p.stage >= 1);
      const waiting = this.queue().filter((c) => c.state === 'waiting');
      if (waiting.length && (Math.random() < 0.4 || !plants.length)) return { kind: 'customer', ref: pick(waiting) };
      if (!plants.length) return null;
      const ripe = plants.filter((p) => p.stage >= 2);
      return { kind: 'plant', ref: pick(ripe.length && Math.random() < 0.7 ? ripe : plants) };
    }

    targetValid(t) {
      if (!t) return false;
      if (t.kind === 'plant') return t.ref.withered <= 0;
      return t.ref.state === 'waiting' && this.customers.includes(t.ref);
    }

    moveToward(e, tx, ty, speed, dt) {
      const dx = tx - e.x, dy = ty - e.y;
      const d = Math.hypot(dx, dy);
      if (d < 1) return 0;
      const step = Math.min(d, speed * dt);
      e.x += (dx / d) * step + e.kx * dt;
      e.y += (dy / d) * step + e.ky * dt;
      e.kx *= 0.85; e.ky *= 0.85;
      e.walk += dt * 10;
      if (Math.abs(dx) > Math.abs(dy)) e.facing = dx > 0 ? 'right' : 'left';
      else e.facing = dy > 0 ? 'down' : 'up';
      return d;
    }

    updateInspector(e, dt) {
      e.hitFlash = Math.max(0, e.hitFlash - dt);
      e.say = Math.max(0, e.say - dt);
      if (e.fleeing) {
        const tx = e.x < C.WIDTH / 2 ? -60 : C.WIDTH + 60;
        this.moveToward(e, tx, 560, 160, dt);
        if (e.x < -40 || e.x > C.WIDTH + 40) e.gone = true;
        return;
      }
      if (!this.targetValid(e.target)) e.target = this.pickTarget();
      if (!e.target) { this.moveToward(e, C.WIDTH / 2, 520, e.speed, dt); return; }

      const t = e.target.ref;
      const d = this.moveToward(e, t.x, t.y + (e.target.kind === 'plant' ? 8 : 0), e.speed, dt);
      if (d < 18) {
        if (e.target.kind === 'plant') {
          t.confiscate();
          this.earned.confiscated++;
          this.addConquest(C.CONFISCATED_PLANT);
          this.text('¡Decomisada!', t.x, t.y - 40, '#ff9a8f');
        } else {
          this.loseCustomer(t, '«Mejor un tinto federado»');
        }
        e.say = 1.8;
        e.line = pick(FG.INSPECTOR_LINES);
        e.target = null;
      }

      // Contacto con el barista: decomiso de inventario
      const p = this.player;
      if (p.invuln <= 0 && Math.hypot(p.x - e.x, p.y - e.y) < 24) {
        p.invuln = 1.5;
        if (this.cups > 0) { this.cups--; this.text('-1 ☕ decomisado', p.x, p.y - 80, '#ff9a8f'); }
        else if (this.cherries > 0) { const n = Math.min(2, this.cherries); this.cherries -= n; this.text(`-${n} 🍒 decomisadas`, p.x, p.y - 80, '#ff9a8f'); }
        e.say = 1.5;
        e.line = '¡Esto queda decomisado!';
      }
    }

    spawnBoss() {
      const hp = this.city.bossHp;
      this.boss = {
        x: C.WIDTH / 2, y: C.HEIGHT + 40, hp, maxHp: hp, speed: 42, walk: 0, facing: 'up',
        hitFlash: 0, say: 3, line: '¡Nadie compite con la Federación!', attack: 2.5, kx: 0, ky: 0,
        target: null, retarget: 0,
      };
      this.showBanner(`¡Llegó ${this.city.boss}! Defiende tu café`, 3, 'rgba(160,30,25,0.92)');
    }

    updateBoss(b, dt) {
      b.hitFlash = Math.max(0, b.hitFlash - dt);
      b.say = Math.max(0, b.say - dt);
      b.retarget -= dt;
      if (b.retarget <= 0 || !this.targetValid(b.target)) {
        b.retarget = 4;
        b.target = Math.random() < 0.5 ? { kind: 'player', ref: this.player } : this.pickTarget();
      }
      const ref = b.target ? b.target.ref : this.player;
      this.moveToward(b, ref.x, Math.max(ref.y, 150), b.speed, dt);

      // Pisa plantas a su paso
      for (const pl of this.plants) {
        if (pl.withered <= 0 && pl.stage >= 1 && dist(b, pl) < 26) {
          pl.confiscate();
          this.earned.confiscated++;
          this.addConquest(C.CONFISCATED_PLANT);
          this.text('¡Decomisada!', pl.x, pl.y - 40, '#ff9a8f');
        }
      }
      // Lanza sellos al barista
      b.attack -= dt;
      if (b.attack <= 0) {
        b.attack = Math.max(1.2, 2.6 - this.cityIndex * 0.2);
        const p = this.player;
        const dx = p.x - b.x, dy = (p.y - 25) - (b.y - 50);
        const d = Math.hypot(dx, dy) || 1;
        const sp = 220 + this.cityIndex * 15;
        this.stamps.push({ x: b.x, y: b.y - 50, vx: (dx / d) * sp, vy: (dy / d) * sp, life: 3 });
        if (Math.random() < 0.35) { b.say = 1.6; b.line = pick(['¡Sellado y decomisado!', '¡Formulario rechazado!', '¡Su café no está en el registro!']); }
      }
      // Contacto
      const p = this.player;
      if (p.invuln <= 0 && Math.hypot(p.x - b.x, p.y - b.y) < 36) {
        p.invuln = 1.5;
        p.stun = 0.8;
        if (this.cups > 0) this.cups--;
        this.text('¡Aplastado por la burocracia!', p.x, p.y - 80, '#ff9a8f', 12);
      }
    }

    // ---------- proyectiles ----------
    updateProjectiles(dt) {
      for (const pr of this.projectiles) {
        pr.x += pr.vx * dt; pr.y += pr.vy * dt; pr.life -= dt;
        for (const e of this.inspectors) {
          if (e.fleeing || pr.life <= 0) continue;
          if (Math.hypot(pr.x - e.x, pr.y - (e.y - 25)) < 22) this.hitEnemy(e, pr, false);
        }
        const b = this.boss;
        if (b && pr.life > 0 && Math.hypot(pr.x - b.x, pr.y - (b.y - 40)) < 38) this.hitEnemy(b, pr, true);
      }
      this.projectiles = this.projectiles.filter((pr) => pr.life > 0 && pr.x > -20 && pr.x < C.WIDTH + 20 && pr.y > 40 && pr.y < C.HEIGHT + 20);

      const p = this.player;
      for (const s of this.stamps) {
        s.x += s.vx * dt; s.y += s.vy * dt; s.life -= dt;
        if (p.invuln <= 0 && Math.hypot(s.x - p.x, s.y - (p.y - 25)) < 20) {
          s.life = 0;
          p.stun = 1.1;
          p.invuln = 1.6;
          this.text('¡SELLADO!', p.x, p.y - 80, '#ff6b5b', 16);
        }
      }
      this.stamps = this.stamps.filter((s) => s.life > 0 && s.x > -20 && s.x < C.WIDTH + 20 && s.y > 40 && s.y < C.HEIGHT + 20);
    }

    hitEnemy(e, pr, isBoss) {
      pr.life = 0;
      e.hp -= pr.dmg;
      e.hitFlash = 0.15;
      const d = Math.hypot(pr.vx, pr.vy) || 1;
      const kb = isBoss ? 60 : 380;
      e.kx += (pr.vx / d) * kb; e.ky += (pr.vy / d) * kb;
      this.burst(pr.x, pr.y, '#6b3b1f', 5);
      if (e.hp > 0) return;
      if (isBoss) {
        this.boss = null;
        this.bossDefeated = true;
        this.earned.bosses++;
        this.addPoints(C.POINTS_PER_BOSS, e.x, e.y - 100);
        this.addConquest(C.BOSS_DEFEATED);
        this.burst(e.x, e.y - 40, '#ffd34d', 30);
        this.stamps = [];
        this.showBanner(`¡${this.city.boss} huyó! Remata la conquista`, 3, 'rgba(20,110,60,0.92)');
      } else {
        e.fleeing = true;
        e.say = 1.5;
        e.line = pick(['¡Esto no se queda así!', '¡Me quemé con el café!', '¡Llamaré a mi supervisor!']);
        this.earned.repelled++;
        this.game.save.stats.repelled++;
        this.addPoints(C.POINTS_PER_INSPECTOR, e.x, e.y - 70);
        this.addConquest(C.INSPECTOR_REPELLED);
      }
    }

    updateEffects(dt) {
      for (const t of this.texts) { t.y -= 28 * dt; t.life -= dt; }
      this.texts = this.texts.filter((t) => t.life > 0);
      for (const p of this.particles) { p.x += p.vx * dt; p.y += p.vy * dt; p.vy += 260 * dt; p.life -= dt; }
      this.particles = this.particles.filter((p) => p.life > 0);
    }

    // ---------- dibujo ----------
    render(ctx, t) {
      ctx.drawImage(this.bg, 0, 0);
      R.drawWater(ctx, t);
      R.drawTank(ctx, this.nutrients, t, this.prompt && this.prompt.includes('nutrientes'));
      for (const slot of FG.PLANT_SLOTS.slice(this.plants.length)) R.drawEmptySlot(ctx, slot);
      for (const pl of this.plants) R.drawPlant(ctx, pl, t, pl === this.highlight);

      // Entidades ordenadas por profundidad
      const ents = [];
      ents.push({ y: this.player.y, draw: () => R.drawPlayer(ctx, this.player, t, this) });
      for (const c of this.customers) ents.push({ y: c.y, draw: () => R.drawCustomer(ctx, c, t) });
      for (const e of this.inspectors) ents.push({ y: e.y, draw: () => R.drawInspector(ctx, e, t) });
      if (this.boss) ents.push({ y: this.boss.y, draw: () => R.drawBoss(ctx, this.boss, t, this.city.boss) });
      ents.sort((a, b) => a.y - b.y);
      for (const e of ents) e.draw();

      for (const pr of this.projectiles) R.drawProjectile(ctx, pr);
      for (const s of this.stamps) R.drawStamp(ctx, s, t);
      for (const p of this.particles) { R.circle(ctx, p.x, p.y, p.r, p.color); }
      for (const ft of this.texts) R.drawFloatText(ctx, ft);

      const p = this.player;
      if (this.brew > 0) R.drawProgress(ctx, S.aeropress.x, S.aeropress.y - 72, this.brew / this.stats.brewTime);
      if (this.prompt) R.drawPrompt(ctx, p.x, p.y - (this.cups > 0 ? 88 : 72), this.prompt);

      R.drawHUD(ctx, this);
      if (this.banner) R.drawBanner(ctx, this.banner.text, this.banner.t, this.banner.color);
    }
  }

  FG.Level = Level;
  FG.computeStats = computeStats;
})();
