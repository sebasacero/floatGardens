// Menús estilo lobby de shooter móvil.
(function () {
  const money = (n) => '$' + Math.round(n).toLocaleString('es-CO');
  // íconos de las pestañas del lobby cenital (trazo fino, como la web)
  const svg = (p) => `<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.8">${p}</svg>`;
  const TAB = {
    home: svg('<path d="M4 11 12 4l8 7v9H4z"/>'),
    shop: svg('<path d="M5 8h14l-1 13H6z"/><path d="M9 8a3 3 0 0 1 6 0"/>'),
    net: svg('<circle cx="6" cy="6" r="2.5"/><circle cx="18" cy="8" r="2.5"/><circle cx="10" cy="18" r="2.5"/><path d="M8 7l8 1M7 8l2 8M16 10l-4 6"/>'),
    gear: svg('<circle cx="12" cy="12" r="3"/><path d="M12 2v3M12 19v3M2 12h3M19 12h3M4.9 4.9l2.1 2.1M17 17l2.1 2.1M4.9 19.1 7 17M17 7l2.1-2.1"/>'),
  };

  const Menus = {
    init(G) {
      this.G = G;
      this.el = document.getElementById('menu');
      this.el.addEventListener('click', (e) => {
        const b = e.target.closest('[data-a]');
        if (b && !b.disabled) this.action(b.dataset.a, b.dataset);
      });
    },
    hide() { this.el.classList.add('hidden'); this.el.innerHTML = ''; },
    show(name, data) {
      // en la vista cenital, el lobby del celular es el de la maqueta (título, tarjetas, Jugar, pestañas)
      if (name === 'lobby' && W.TopDown && W.TopDown.active && (W.Input.isTouch || window.innerWidth < 760)) name = 'lobbyTd';
      this.current = name;
      this.data = data || {};
      this.el.innerHTML = this.screens[name].call(this, this.G.save, this.data);
      this.el.className = `menu menu-${name}`;
    },

    topbar(s) {
      return `<div class="topbar">
        <div class="brand"><span class="leaf">🌱</span> FLOAT GARDENS</div>
        <div class="chips">
          <span class="chip money">${money(s.money)}</span>
          <span class="chip pts">★ ${s.puntos}</span>
        </div>
      </div>`;
    },

    // Cámara (cenital / tercera persona) y tema de la cenital (noche / día)
    viewButtons(cls) {
      const c = cls || 'btn-side';
      const td = W.TopDown.active;
      return `<button class="${c}" data-a="camera">📷 Cámara: ${td ? 'cenital' : 'tercera persona'}</button>` +
        (td ? `<button class="${c}" data-a="theme">${W.TopDown.theme === 'noche' ? '🌙 Tema: noche' : '☀️ Tema: día'}</button>` : '');
    },

    pointsMeter(s) {
      const cost = FG.CONFIG.REDEEM_COST;
      const frac = Math.min(1, s.puntos / cost);
      return `<div class="pmeter"><div class="pmeter-label">★ ${s.puntos} / ${cost} Puntos Float</div>
        <div class="pmeter-bar"><i style="width:${frac * 100}%"></i></div>
        <small>${s.puntos >= cost ? '¡Tienes un café gratis para canjear!' : `Te faltan ${cost - s.puntos} para un café gratis`}</small></div>`;
    },

    screens: {
      lobbyTd(s) {
        const G = this.G;
        const n = G.capturedCount();
        const started = n > 0 || s.world.kos > 0;
        const d = W.World.districtAt(W.World.hqSpot.spawn.x, W.World.hqSpot.spawn.z);
        return `<div class="tdl">
          <div class="tdl-top"><span class="tdl-k">temporada 01 · ${s.world.bossDefeated ? 'red libre' : 'cosecha'}</span><span class="td-cap"><svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 19c0-8 5-13 14-14-1 9-6 14-14 14z"/><path d="M5 19 13 11"/></svg>Float Gardens <span>Kennedy 2089</span></span></div>
          <div class="tdl-title"><div class="tdl-k">devlog #001 · ${d.name}</div><h1>Defiende tu<br><b>terraza</b></h1></div>
          <div class="tdl-cards">
            <div><label>cerezas</label><p>${G.cherries}<small>🍒</small></p></div>
            <div><label>zonas</label><p>${n}<small>/ ${G.zones.length}</small></p></div>
            <div><label>puntos</label><p>${s.puntos}<small>★</small></p></div>
          </div>
          <button class="tdl-play" data-a="play"><b>${started ? 'Continuar' : 'Jugar'}</b><span>${d.name} · ${W.TopDown.theme === 'noche' ? 'noche' : 'día'}</span></button>
          <div class="tdl-tabs">
            <button class="on" data-a="play-noop"><i>${TAB.home}</i>inicio</button>
            <button data-a="shop"><i>${TAB.shop}</i>mejoras</button>
            <button data-a="rewards"><i>${TAB.net}</i>puntos</button>
            <button data-a="ajustes"><i>${TAB.gear}</i>ajustes</button>
          </div>
        </div>`;
      },

      ajustes() {
        return `<div class="panel narrow">
          <h2>Ajustes</h2>
          <div class="col">
            ${this.viewButtons('btn-g')}
            <button class="btn-g" data-a="sound">${W.Audio.muted ? '🔇 Sonido: no' : '🔊 Sonido: sí'}</button>
            <button class="btn-g" data-a="howto">📖 Cómo jugar</button>
            <a class="btn-g" href="clasico.html">🕹️ Modo clásico 2D</a>
            <button class="btn-g" data-a="reset">Reiniciar mundo</button>
            <button class="btn-y" data-a="back">Volver</button>
          </div>
          <p class="sub small">Juego de ficción y parodia · no afiliado a ninguna organización real</p>
        </div>`;
      },

      lobby(s) {
        const G = this.G;
        const n = G.capturedCount();
        const started = n > 0 || s.world.kos > 0;
        return `${this.topbar(s)}
          <div class="lobby">
            <div class="lobby-left">
              <div class="mode-tag">MUNDO ABIERTO · BOGOTÁ</div>
              <h1 class="title">LA CONQUISTA<br><span>DEL CAFÉ</span></h1>
              <p class="pitch">Recorre Bogotá en tercera persona, farmea café hidropónico, véndelo en tus barrios y arrebátale la ciudad a la Federación con tu AeroPress.</p>
              <div class="progress-card">
                <div class="pc-row"><span>Zonas capturadas</span><b>${n}/${G.zones.length}</b></div>
                <div class="pc-bar"><i style="width:${(n / G.zones.length) * 100}%"></i></div>
                <div class="pc-row small"><span>Derrotados: ${s.world.kos}</span><span>Ventas: ${s.world.sales}</span><span>🍒 ${G.cherries}</span></div>
              </div>
              ${this.pointsMeter(s)}
            </div>
            <div class="lobby-right">
              <button class="btn-play" data-a="play">${started ? 'CONTINUAR' : 'JUGAR'}<small>Bogotá · ${W.TopDown.active ? `Vista cenital · ${W.TopDown.theme === 'noche' ? 'noche' : 'día'}` : 'Tercera persona'}</small></button>
              <div class="side-buttons">
                <button class="btn-side" data-a="shop">🛠️ Mejoras</button>
                <button class="btn-side" data-a="rewards">🎟️ Puntos Float</button>
                <button class="btn-side" data-a="howto">📖 Cómo jugar</button>
                <a class="btn-side" href="clasico.html">🕹️ Modo clásico 2D</a>
                ${this.viewButtons()}
                <button class="btn-side" data-a="sound">${W.Audio.muted ? '🔇 Sonido: no' : '🔊 Sonido: sí'}</button>
              </div>
            </div>
          </div>
          <div class="lobby-foot">Juego de ficción y parodia · no afiliado a ninguna organización real · <a href="promo/">banners</a> · <button class="link" data-a="reset">reiniciar mundo</button></div>`;
      },

      howto() {
        const touch = W.Input.isTouch;
        return `<div class="panel">
          <h2>Cómo jugar</h2>
          <div class="howto">
            <div><b>🌱 Farmea</b>Cosecha las plantas flotantes del HQ en Chapinero y abre cajas de café 📦 por la ciudad. Cada cereza suma Puntos Float.</div>
            <div><b>☕ Dispara</b>Tu AeroPress dispara chorros de café a presión. Recargar gasta 1 🍒. Apunta a la cabeza para más daño.</div>
            <div><b>🧾 Vende</b>Los peatones con ☕ quieren café, pero solo te compran en barrios de Float Gardens. 2 🍒 = $8.000.</div>
            <div><b>🚩 Conquista</b>Entra a las zonas rojas, derrota las oleadas y quédate dentro hasta capturarlas. Cada zona abre un barrio nuevo para vender y cultivar.</div>
            <div><b>☠ Jefe final</b>Con las 6 zonas, ve a la Plaza de Bolívar y derrota a Don Monopolio.</div>
            <div><b>❤️ Vida</b>Se regenera si te cubres unos segundos. Si caes, vuelves al HQ y pierdes la mitad de tus cerezas.</div>
          </div>
          <div class="keys">${W.TopDown.active
            ? (touch ? 'Vista cenital · Joystick: moverte · Arrastra a la derecha o sobre <b>disparar</b>: apuntar (la mira se pega al enemigo más cercano) · impulso: saltar · cosechar: acción · recargar · 🗺 mapa'
              : 'Vista cenital · <kbd>WASD</kbd> moverte · <kbd>Mouse</kbd> apuntar · <kbd>Clic</kbd> disparar · <kbd>Shift</kbd> correr · <kbd>Espacio</kbd> saltar · <kbd>R</kbd> recargar · <kbd>E</kbd> acción · <kbd>M</kbd> mapa · <kbd>Esc</kbd> pausa')
            : touch
            ? 'Joystick: moverte (al fondo = correr) · Desliza a la derecha: mirar · 💥 disparar (arrastra para apuntar) · 🎯 apuntar · ⤒ saltar · ↻ recargar · E acción · 🗺 mapa'
            : '<kbd>WASD</kbd> moverte · <kbd>Mouse</kbd> mirar · <kbd>Clic</kbd> disparar · <kbd>Clic der.</kbd> apuntar · <kbd>Shift</kbd> correr · <kbd>Espacio</kbd> saltar · <kbd>R</kbd> recargar · <kbd>E</kbd> acción · <kbd>M</kbd> mapa · <kbd>Esc</kbd> pausa'}</div>
          <div class="row"><button class="btn-y" data-a="back">Volver</button></div>
        </div>`;
      },

      shop(s, d) {
        const up = s.world.upgrades;
        const cards = W.UPGRADES.map((u) => {
          const lvl = up[u.id] || 0;
          const maxed = lvl >= u.max;
          const cost = u.cost * (lvl + 1);
          return `<div class="ucard">
            <div class="uicon">${u.icon}</div>
            <div class="ubody"><b>${u.name}</b><p>${u.desc}</p>
              <div class="pips">${Array.from({ length: u.max }, (_, i) => `<i class="${i < lvl ? 'on' : ''}"></i>`).join('')}</div></div>
            <button class="btn-buy" data-a="buy" data-id="${u.id}" ${maxed || s.money < cost ? 'disabled' : ''}>${maxed ? 'MÁX' : money(cost)}</button>
          </div>`;
        }).join('');
        return `<div class="panel wide">
          <h2>🛠️ Mejoras</h2>
          <p class="sub">Tienes <b>${money(s.money)}</b> · gana plata vendiendo café, capturando zonas y derrotando enemigos</p>
          <div class="ucards">${cards}</div>
          <div class="row"><button class="btn-y" data-a="${d.from === 'game' ? 'resume' : 'back'}">${d.from === 'game' ? 'Volver al juego' : 'Volver'}</button></div>
        </div>`;
      },

      rewards(s, d) {
        const cost = FG.CONFIG.REDEEM_COST;
        const vouchers = s.vouchers.slice().reverse().map((v) => `<li><code>${v.code}</code><span>${new Date(v.date).toLocaleDateString('es-CO')}</span></li>`).join('');
        const points = FG.OFFICIAL_POINTS.map((p) => `<li><b>${p.name}</b> — ${p.city}<br><span>${p.address}</span></li>`).join('');
        return `<div class="panel">
          <h2>🎟️ Puntos Float</h2>
          ${this.pointsMeter(s)}
          ${d.code ? `<div class="voucher"><div>¡Tu café gratis!</div><code>${d.code}</code><small>Muéstralo en un punto oficial de Float Gardens</small></div>` : ''}
          <div class="row"><button class="btn-y" data-a="redeem" ${s.puntos < cost ? 'disabled' : ''}>Canjear café gratis (${cost} ★)</button></div>
          <div class="cols">
            <div><h3>Mis cupones</h3>${vouchers ? `<ul class="list">${vouchers}</ul>` : '<p class="sub">Aún no tienes cupones. ¡A farmear!</p>'}</div>
            <div><h3>Puntos oficiales</h3><ul class="list">${points}</ul></div>
          </div>
          <p class="sub small">Los Puntos Float se comparten con el modo clásico. Ganas puntos cosechando (+1 por cereza), vendiendo (+10), derrotando enemigos (+5), capturando zonas (+100) y venciendo al jefe (+300).</p>
          <div class="row"><button class="btn-g" data-a="back">Volver</button></div>
        </div>`;
      },

      pause() {
        return `<div class="panel narrow">
          <h2>PAUSA</h2>
          <div class="col">
            <button class="btn-y" data-a="resume">▶ Seguir jugando</button>
            <button class="btn-g" data-a="shop-game">🛠️ Mejoras</button>
            <button class="btn-g" data-a="rewards">🎟️ Puntos Float</button>
            <button class="btn-g" data-a="howto">📖 Cómo jugar</button>
            ${this.viewButtons('btn-g')}
            <button class="btn-g" data-a="lobby">Volver al lobby</button>
          </div>
          <p class="sub small">${W.Input.isTouch || W.TopDown.active ? '' : 'Haz clic en "Seguir jugando" para volver a capturar el mouse.'}</p>
        </div>`;
      },

      dead(s, d) {
        return `<div class="panel narrow dead">
          <h2 class="big-red">¡TE SELLARON!</h2>
          <p class="sub">La Federación te llenó de formularios. Perdiste ${d.lost} 🍒.</p>
          <div class="col"><button class="btn-y" data-a="respawn">Volver al HQ</button></div>
        </div>`;
      },

      victory(s) {
        return `<div class="panel">
          <h2 class="big-green">¡BOGOTÁ ES DE FLOAT GARDENS!</h2>
          <p class="sub">Don Monopolio colgó el sombrero y la Federación... abrió su propio invernadero flotante. Todo empezó con una AeroPress y una idea compartida.</p>
          <div class="stats"><div><b>${s.world.kos}</b>derrotados</div><div><b>${s.world.sales}</b>cafés vendidos</div><div><b>★ ${s.puntosTotal}</b>Puntos Float totales</div></div>
          ${this.pointsMeter(s)}
          <div class="row"><button class="btn-y" data-a="resume">Seguir farmeando</button><button class="btn-g" data-a="rewards">🎟️ Canjear</button></div>
        </div>`;
      },

      reset() {
        return `<div class="panel narrow">
          <h2>¿Reiniciar el mundo?</h2>
          <p class="sub">Se pierden las zonas capturadas, mejoras y cerezas del mundo abierto. Tus Puntos Float, dinero y cupones se conservan.</p>
          <div class="row"><button class="btn-r" data-a="reset-ok">Sí, reiniciar</button><button class="btn-y" data-a="back">Cancelar</button></div>
        </div>`;
      },
    },

    action(a, d) {
      const G = this.G;
      const s = G.save;
      const prev = this.current;
      switch (a) {
        case 'play':
        case 'resume':
          G.play();
          break;
        case 'lobby':
          G.toLobby();
          break;
        case 'respawn':
          G.respawn();
          break;
        case 'shop':
          this.returnTo = prev;
          this.show('shop', { from: 'menu' });
          break;
        case 'shop-game':
          this.returnTo = prev;
          this.show('shop', { from: 'menu' });
          break;
        case 'rewards':
        case 'howto':
        case 'reset':
        case 'ajustes':
          this.returnTo = prev === 'rewards' || prev === 'howto' || prev === 'shop' || prev === 'reset' || prev === 'ajustes' ? this.returnTo : prev;
          if (prev === 'ajustes' && a !== 'ajustes') this.returnTo = 'ajustes';
          this.show(a);
          break;
        case 'back':
          this.show(this.returnTo === 'lobbyTd' ? 'lobby' : (this.returnTo || (G.state === 'lobby' ? 'lobby' : 'pause')));
          break;
        case 'buy': {
          const u = W.UPGRADES.find((x) => x.id === d.id);
          const lvl = s.world.upgrades[u.id] || 0;
          const cost = u.cost * (lvl + 1);
          if (lvl < u.max && s.money >= cost) {
            s.money -= cost;
            s.world.upgrades[u.id] = lvl + 1;
            G.refreshStats();
            G.persist();
            W.Audio.cash();
          }
          this.show('shop', this.data);
          break;
        }
        case 'redeem': {
          const cost = FG.CONFIG.REDEEM_COST;
          if (s.puntos >= cost) {
            s.puntos -= cost;
            const code = FG.Vouchers.create();
            s.vouchers.push({ code, date: Date.now() });
            G.persist();
            this.show('rewards', { code });
          }
          break;
        }
        case 'sound':
          W.Audio.muted = !W.Audio.muted;
          this.show(prev === 'ajustes' ? 'ajustes' : 'lobby');
          break;
        case 'camera':
          W.TopDown.set(G, !W.TopDown.active);
          this.show(prev === 'lobbyTd' ? 'lobby' : prev);
          break;
        case 'theme':
          W.TopDown.toggleTheme(G);
          this.show(prev === 'lobbyTd' ? 'lobby' : prev);
          break;
        case 'play-noop':
          break;
        case 'reset-ok':
          s.world = { captured: [], upgrades: {}, kos: 0, sales: 0, bossDefeated: false, cherries: W.CONFIG.START_CHERRIES };
          FG.Save.write(s);
          location.reload();
          break;
      }
    },
  };

  W.Menus = Menus;
})();
