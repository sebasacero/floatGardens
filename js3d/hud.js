// HUD estilo shooter móvil: minimapa giratorio, brújula, kill feed, vida, munición, hitmarkers.
(function () {
  const $ = (id) => document.getElementById(id);
  const money = (n) => '$' + Math.round(n).toLocaleString('es-CO');
  const TWO_PI = Math.PI * 2;

  // El HUD se actualiza en cada cuadro: solo tocamos el DOM cuando un valor cambia de verdad,
  // así el navegador no recalcula estilos ni diseño 60 veces por segundo.
  const els = {};
  const el = (id) => els[id] || (els[id] = $(id));
  const last = new Map();
  function set(id, prop, value) {
    const k = id + ':' + prop;
    if (last.get(k) === value) return;
    last.set(k, value);
    const e = el(id);
    if (prop === 'text') e.textContent = value;
    else if (prop === 'html') e.innerHTML = value;
    else if (prop === 'class') e.className = value;
    else if (prop.startsWith('--')) e.style.setProperty(prop, value);
    else e.style[prop] = value;
  }
  function toggle(id, cls, on) {
    const k = id + '.' + cls;
    on = !!on;
    if (last.get(k) === on) return;
    last.set(k, on);
    el(id).classList.toggle(cls, on);
  }

  const HUD = {
    init() {
      this.el = $('hud');
      this.mm = $('minimap').getContext('2d');
      this.compass = $('compass').getContext('2d');
      this.big = $('bigmap');
      this.bigCtx = this.big.getContext('2d');
      this.numbers = [];
      this.feedItems = [];
      this.lastBanner = 0;
    },

    show(on) { this.el.classList.toggle('hidden', !on); },

    update(G, dt) {
      const p = G.player;
      this.drawMinimap(G);
      this.drawCompass(G);

      // vida
      const frac = Math.max(0, p.hp / p.maxHp);
      set('hpFill', 'width', `${frac * 100}%`);
      set('hpFill', 'class', frac < 0.3 ? 'low' : '');
      set('hpText', 'text', `${Math.ceil(Math.max(0, p.hp))}`);
      set('cherries', 'text', G.cherries);
      // arma
      set('ammo', 'text', p.ammo);
      set('ammoMax', 'text', `/ ${p.magSize}`);
      set('ammo', 'class', p.ammo <= 3 ? 'low' : '');
      toggle('reloadBar', 'hidden', !(p.reloading > 0));
      if (p.reloading > 0) set('reloadFill', 'width', `${(1 - p.reloading / p.reloadTime) * 100}%`);
      set('wMoney', 'text', money(G.save.money));
      set('wPoints', 'text', `★ ${G.save.puntos}`);

      // ubicación
      const d = G.currentDistrict;
      if (d) {
        set('locDistrict', 'text', d.name);
        set('locDistrict', 'class', G.isMarket(p.pos.x, p.pos.z) ? 'ours' : 'theirs');
        set('locStreet', 'text', W.World.streetName(p.pos.x, p.pos.z));
      }

      // objetivo
      const obj = G.objective();
      set('objTitle', 'text', obj.title);
      set('objSub', 'text', obj.target ? `${Math.round(Math.hypot(obj.target.x - p.pos.x, obj.target.z - p.pos.z))} m · ${obj.sub}` : obj.sub);

      // captura
      const cz = G.capturing;
      toggle('capture', 'hidden', !cz);
      if (cz) {
        set('capName', 'text', cz.contested ? `${cz.zone.name} · ¡ELIMINA A LOS ENEMIGOS!` : `CAPTURANDO ${cz.zone.name.toUpperCase()}`);
        set('capFill', 'width', `${cz.zone.progress * 100}%`);
        toggle('capture', 'contested', cz.contested);
      }

      // jefe
      const b = G.boss;
      toggle('bossbar', 'hidden', !(b && !b.dead));
      if (b && !b.dead) set('bossFill', 'width', `${(b.hp / b.maxHp) * 100}%`);

      // indicación de interacción
      toggle('prompt', 'hidden', !G.prompt);
      if (G.prompt) set('prompt', 'html', G.prompt);

      // mira
      set('crosshair', '--gap', `${6 + p.spread * 60}px`);
      toggle('crosshair', 'aim', p.aiming);

      // números de daño
      const cam = G.camera;
      const v = new THREE.Vector3();
      const W2 = window.innerWidth / 2, H2 = window.innerHeight / 2;
      for (const n of this.numbers) {
        n.t += dt;
        v.set(n.x, n.y + n.t * 1.2, n.z).project(cam);
        const vis = v.z < 1;
        n.el.style.display = vis ? 'block' : 'none';
        n.el.style.transform = `translate(${v.x * W2 + W2}px, ${-v.y * H2 + H2}px) translate(-50%, -50%) scale(${1 + Math.max(0, 0.25 - n.t) * 2})`;
        n.el.style.opacity = Math.max(0, 1 - n.t / 0.9);
      }
      this.numbers = this.numbers.filter((n) => { if (n.t > 0.9) { n.el.remove(); return false; } return true; });

      // kill feed
      const now = performance.now();
      this.feedItems = this.feedItems.filter((f) => { if (now - f.t > 5000) { f.el.remove(); return false; } return true; });

      if (G.showMap) this.drawBigMap(G);
    },

    drawMinimap(G) {
      const ctx = this.mm;
      const S = ctx.canvas.width;
      const R = S / 2;
      const mm = W.World.minimap;
      const p = G.player;
      const zoom = 0.8; // px por metro
      ctx.save();
      ctx.clearRect(0, 0, S, S);
      ctx.beginPath();
      ctx.rect(0, 0, S, S);
      ctx.clip();
      ctx.fillStyle = '#20242a';
      ctx.fillRect(0, 0, S, S);
      ctx.translate(R, R);
      ctx.rotate(p.yaw);
      ctx.scale(zoom, zoom);
      ctx.drawImage(mm.canvas, -(p.pos.x + mm.offset), -(p.pos.z + mm.offset));
      const dot = (x, z, color, r) => {
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(x - p.pos.x, z - p.pos.z, r, 0, TWO_PI);
        ctx.fill();
      };
      // zonas
      for (const z of G.zones) {
        ctx.strokeStyle = z.owner === 'fg' ? '#2fbf71' : z === G.targetZone ? '#ffd34d' : '#e0413a';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.arc(z.pos.x - p.pos.x, z.pos.z - p.pos.z, W.CONFIG.CAPTURE_RADIUS, 0, TWO_PI);
        ctx.stroke();
      }
      const hq = W.World.hqSpot;
      dot(hq.x, hq.z, '#2fbf71', 6);
      for (const l of G.loot) dot(l.pos.x, l.pos.z, '#ffd34d', 2.5);
      for (const c of G.peds) if (c.icon.visible) dot(c.pos.x, c.pos.z, '#fff3dc', 2.5);
      for (const e of G.enemies) if (!e.dead && (e.alerted || Math.hypot(e.pos.x - p.pos.x, e.pos.z - p.pos.z) < 45)) dot(e.pos.x, e.pos.z, '#ff4b3a', e.boss ? 6 : 3);
      ctx.restore();
      // jugador (siempre mirando hacia arriba)
      ctx.fillStyle = '#ffffff';
      ctx.beginPath();
      ctx.moveTo(R, R - 8); ctx.lineTo(R + 6, R + 6); ctx.lineTo(R, R + 3); ctx.lineTo(R - 6, R + 6);
      ctx.closePath();
      ctx.fill();
      // N del norte en el borde
      const a = -Math.PI / 2 + p.yaw;
      ctx.fillStyle = '#fff';
      ctx.font = 'bold 12px system-ui, sans-serif';
      ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
      ctx.fillText('N', R + Math.cos(a) * (R - 10), R + Math.sin(a) * (R - 10));
    },

    drawCompass(G) {
      const ctx = this.compass;
      const Wd = ctx.canvas.width, H = ctx.canvas.height;
      const p = G.player;
      const heading = ((-p.yaw * 180) / Math.PI % 360 + 360) % 360;
      const pxDeg = Wd / 120;
      ctx.clearRect(0, 0, Wd, H);
      const labels = { 0: 'N', 45: 'NE', 90: 'E', 135: 'SE', 180: 'S', 225: 'SO', 270: 'O', 315: 'NO' };
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      for (let b = 0; b < 360; b += 15) {
        let diff = ((b - heading + 540) % 360) - 180;
        if (Math.abs(diff) > 62) continue;
        const x = Wd / 2 + diff * pxDeg;
        const alpha = 1 - Math.abs(diff) / 70;
        ctx.fillStyle = `rgba(44,48,46,${alpha})`;
        if (labels[b] !== undefined) {
          ctx.font = `${b % 90 === 0 ? 600 : 500} ${b % 90 === 0 ? 14 : 11}px Manrope, system-ui, sans-serif`;
          ctx.fillStyle = b === 0 ? `rgba(79,143,44,${alpha})` : `rgba(44,48,46,${alpha})`;
          ctx.fillText(labels[b], x, H / 2 - 2);
        } else {
          ctx.fillRect(x - 1, H / 2 - 4, 2, 8);
        }
      }
      const marker = (tx, tz, color, label) => {
        const b = ((Math.atan2(tx - p.pos.x, -(tz - p.pos.z)) * 180) / Math.PI + 360) % 360;
        let diff = ((b - heading + 540) % 360) - 180;
        diff = Math.max(-58, Math.min(58, diff));
        const x = Wd / 2 + diff * pxDeg;
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.moveTo(x, H - 2); ctx.lineTo(x - 6, H - 10); ctx.lineTo(x + 6, H - 10);
        ctx.fill();
        if (label) { ctx.font = '500 10px Manrope, system-ui'; ctx.fillText(label, x, 7); }
      };
      const obj = G.objective();
      if (obj.target) marker(obj.target.x, obj.target.z, '#4f8f2c');
      marker(440, 150, 'rgba(44,48,46,0.45)', 'Monserrate');
      ctx.fillStyle = '#2a22f5';
      ctx.fillRect(Wd / 2 - 1, 0, 2, 6);
    },

    drawBigMap(G) {
      const cv = this.big;
      const ctx = this.bigCtx;
      const S = Math.min(window.innerWidth, window.innerHeight) * 0.86;
      if (cv.width !== Math.round(S)) { cv.width = cv.height = Math.round(S); }
      const mm = W.World.minimap;
      const k = S / mm.canvas.width;
      ctx.clearRect(0, 0, S, S);
      ctx.drawImage(mm.canvas, 0, 0, S, S);
      const tx = (x) => (x + mm.offset) * k, tz = (z) => (z + mm.offset) * k;
      ctx.textAlign = 'center';
      ctx.font = `bold ${Math.max(10, S / 55)}px system-ui, sans-serif`;
      for (const z of G.zones) {
        const color = z.owner === 'fg' ? '#2fbf71' : z === G.targetZone ? '#ffd34d' : '#e0413a';
        ctx.fillStyle = color;
        ctx.beginPath(); ctx.arc(tx(z.pos.x), tz(z.pos.z), 8, 0, TWO_PI); ctx.fill();
        ctx.fillStyle = '#fff';
        ctx.fillText(z.name, tx(z.pos.x), tz(z.pos.z) - 14);
      }
      const hq = W.World.hqSpot;
      ctx.fillStyle = '#2fbf71';
      ctx.fillRect(tx(hq.x) - 7, tz(hq.z) - 7, 14, 14);
      ctx.fillStyle = '#fff';
      ctx.fillText('Float Gardens HQ', tx(hq.x), tz(hq.z) - 14);
      const bs = W.World.bossArena;
      ctx.fillStyle = G.allCaptured() ? '#ffd34d' : '#a33a30';
      ctx.fillText(G.allCaptured() ? '☠ Sede de la Federación' : 'Sede de la Federación 🔒', tx(bs.x), tz(bs.z) - 30);
      ctx.fillStyle = 'rgba(255,255,255,0.7)';
      ctx.fillText('⛰ Cerros orientales · Monserrate ➜', S - 120, S / 2);
      // jugador
      const p = G.player;
      ctx.save();
      ctx.translate(tx(p.pos.x), tz(p.pos.z));
      ctx.rotate(-p.yaw);
      ctx.fillStyle = '#ffd34d';
      ctx.beginPath(); ctx.moveTo(0, -10); ctx.lineTo(7, 8); ctx.lineTo(0, 4); ctx.lineTo(-7, 8); ctx.closePath(); ctx.fill();
      ctx.restore();
    },

    killFeed(html) {
      const el = document.createElement('div');
      el.className = 'feed-item';
      el.innerHTML = html;
      $('killfeed').prepend(el);
      this.feedItems.push({ el, t: performance.now() });
      while (this.feedItems.length > 5) this.feedItems.shift().el.remove();
    },

    hitMarker(head, ko) {
      const h = $('hitmarker');
      h.className = '';
      void h.offsetWidth;
      h.className = ko ? 'show ko' : head ? 'show head' : 'show';
    },

    damageNumber(x, y, z, amount, crit) {
      const el = document.createElement('div');
      el.className = crit ? 'dmg crit' : 'dmg';
      el.textContent = Math.round(amount);
      $('numbers').appendChild(el);
      this.numbers.push({ el, x: x + (Math.random() - 0.5) * 0.4, y, z, t: 0 });
    },

    banner(title, sub, kind) {
      const b = $('banner');
      b.className = `banner ${kind || ''}`;
      b.innerHTML = `<div class="b-title">${title}</div>${sub ? `<div class="b-sub">${sub}</div>` : ''}`;
      void b.offsetWidth;
      b.classList.add('show');
      clearTimeout(this.bannerTimer);
      this.bannerTimer = setTimeout(() => b.classList.remove('show'), 2800);
    },

    toast(text) {
      const t = $('toast');
      t.textContent = text;
      t.classList.remove('show');
      void t.offsetWidth;
      t.classList.add('show');
    },

    hurt(amount) {
      const v = $('vignette');
      v.style.opacity = Math.min(0.85, 0.25 + amount / 40);
      clearTimeout(this.hurtTimer);
      this.hurtTimer = setTimeout(() => { v.style.opacity = 0; }, 180);
    },

    lowHealth(frac) {
      set('lowhp', 'opacity', `${frac < 0.35 ? (0.35 - frac) * 2 : 0}`);
    },

    setBigMap(on) { this.big.parentElement.classList.toggle('hidden', !on); },
  };

  W.HUD = HUD;
})();
