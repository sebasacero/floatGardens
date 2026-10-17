// Pantallas HTML sobre el canvas: menú, mapa, tienda, Puntos Float, etc.
(function () {
  const C = FG.CONFIG;
  const money = (n) => FG.Render.formatMoney(n);

  const UI = {
    init(game) {
      this.game = game;
      this.el = document.getElementById('overlay');
      this.el.addEventListener('click', (e) => {
        const b = e.target.closest('[data-action]');
        if (b && !b.disabled) game.handleAction(b.dataset.action, b.dataset);
      });
    },

    hide() {
      this.el.classList.add('hidden');
      this.el.innerHTML = '';
    },

    show(name, data) {
      const html = this.screens[name].call(this, this.game.save, data || {});
      this.el.innerHTML = `<div class="panel panel-${name}">${html}</div>`;
      this.el.classList.remove('hidden');
      const first = this.el.querySelector('.btn.primary');
      if (first) first.focus({ preventScroll: true });
    },

    logo() {
      return `<div class="logo">
        <div class="logo-leaf">🌱☕</div>
        <h1>FLOAT GARDENS</h1>
        <div class="logo-sub">La Conquista del Café</div>
      </div>`;
    },

    pointsBox(save) {
      const frac = Math.min(1, save.puntos / C.REDEEM_COST);
      return `<div class="points-box">
        <div class="points-label">★ ${save.puntos} Puntos Float</div>
        <div class="meter"><div style="width:${frac * 100}%"></div></div>
        <div class="points-hint">${save.puntos >= C.REDEEM_COST
          ? '¡Tienes un café gratis listo para canjear!'
          : `Te faltan ${C.REDEEM_COST - save.puntos} para un café gratis`}</div>
      </div>`;
    },

    screens: {
      menu(save) {
        const started = save.conquered.length > 0 || save.puntosTotal > 0;
        return `${this.logo()}
          <p class="tagline">Farmea café hidropónico, gana <b>Puntos Float</b> y tómate un café <b>gratis</b> en los puntos oficiales de Float Gardens.</p>
          ${this.pointsBox(save)}
          <div class="buttons">
            <button class="btn primary" data-action="play">${started ? '▶ Continuar la conquista' : '▶ Jugar'}</button>
            <button class="btn" data-action="howto">📖 Cómo jugar</button>
            <button class="btn" data-action="rewards">🎟️ Canjear Puntos Float</button>
          </div>
          <button class="link" data-action="reset">Reiniciar progreso</button>`;
      },

      intro() {
        return `<h2>Bogotá, ${new Date().getFullYear()}</h2>
          <div class="story">
            <p>Un barista con su <b>AeroPress</b> tuvo una idea loca: cultivar <b>café hidropónico</b> en balsas flotantes, en plena ciudad, como un <b>proyecto colaborativo</b> con su comunidad.</p>
            <p>Así nació <b>Float Gardens</b>.</p>
            <p>Pero <b class="fed">la Federación</b> no quiere competencia: sus inspectores vienen a decomisar plantas, espantar clientes y llenarlo todo de sellos.</p>
            <p>Tu misión: cultivar, prensar, vender... y <b>conquistar el mundo del café</b>, ciudad por ciudad.</p>
          </div>
          <div class="buttons"><button class="btn primary" data-action="intro-done">¡Vamos! ☕</button></div>`;
      },

      howto() {
        return `<h2>Cómo jugar</h2>
          <div class="howto-grid">
            <div><span class="big">🌱</span><b>1. Farmea</b><p>Acércate a las plantas con cerezas rojas y presiona <kbd>E</kbd> para cosechar. Cada cereza = Puntos Float.</p></div>
            <div><span class="big">💧</span><b>2. Nutre</b><p>Recarga el tanque de nutrientes o las plantas dejan de crecer.</p></div>
            <div><span class="big">☕</span><b>3. Prensa</b><p>En la mesa de la AeroPress <b>mantén</b> <kbd>E</kbd>: 2 cerezas = 1 taza.</p></div>
            <div><span class="big">🧾</span><b>4. Vende</b><p>Sirve en la barra antes de que se acabe la paciencia del cliente. Rápido = propina.</p></div>
            <div><span class="big">💥</span><b>5. Defiéndete</b><p>Dispara chorros de café a presión con <kbd>Espacio</kbd> para espantar a los inspectores de la Federación.</p></div>
            <div><span class="big">🌎</span><b>6. Conquista</b><p>Llena la barra de conquista, derrota al jefe y sigue a la próxima ciudad.</p></div>
          </div>
          <p class="controls">Moverse: <kbd>WASD</kbd>/<kbd>flechas</kbd> · Acción: <kbd>E</kbd>/<kbd>Enter</kbd> · Disparar: <kbd>Espacio</kbd> · Pausa: <kbd>P</kbd>/<kbd>Esc</kbd><br>En celular: joystick a la izquierda y botones a la derecha.</p>
          <p class="disclaimer">Juego de ficción y parodia. No está afiliado ni respaldado por ninguna organización cafetera real.</p>
          <div class="buttons"><button class="btn primary" data-action="back">Volver</button></div>`;
      },

      map(save) {
        const nodes = FG.CITIES.map((c, i) => {
          const done = save.conquered.includes(i);
          const open = i === 0 || save.conquered.includes(i - 1);
          const cls = done ? 'done' : open ? 'open' : 'locked';
          return `<button class="city ${cls}" data-action="start-city" data-index="${i}" ${open ? '' : 'disabled'} style="--accent:${c.accent}">
            <span class="city-icon">${done ? '🚩' : open ? '☕' : '🔒'}</span>
            <span class="city-name">${c.name}</span>
            <span class="city-district">${c.district}</span>
            <span class="city-tag">${done ? 'Conquistada · rejugar' : open ? 'Jugar' : 'Bloqueada'}</span>
          </button>`;
        }).join('<span class="route">➜</span>');
        return `<h2>Mapa de conquista</h2>
          <p class="sub">${save.conquered.length}/${FG.CITIES.length} ciudades · ${money(save.money)} · ★ ${save.puntos} Puntos Float</p>
          <div class="map">${nodes}</div>
          <p class="sub small">Puedes rejugar ciudades conquistadas para seguir farmeando Puntos Float.</p>
          <div class="buttons">
            <button class="btn" data-action="shop">🛠️ Mejoras</button>
            <button class="btn" data-action="rewards">🎟️ Puntos Float</button>
            <button class="btn" data-action="menu">Menú</button>
          </div>`;
      },

      shop(save) {
        const cards = FG.UPGRADES.map((u) => {
          const lvl = save.upgrades[u.id] || 0;
          const maxed = lvl >= u.max;
          const cost = u.cost * (lvl + 1);
          const pips = Array.from({ length: u.max }, (_, i) => `<i class="${i < lvl ? 'on' : ''}"></i>`).join('');
          return `<div class="card">
            <div class="card-icon">${u.icon}</div>
            <div class="card-body">
              <b>${u.name}</b>
              <p>${u.desc}</p>
              <div class="pips">${pips}</div>
            </div>
            <button class="btn small ${maxed ? '' : 'primary'}" data-action="buy" data-id="${u.id}" ${maxed || save.money < cost ? 'disabled' : ''}>
              ${maxed ? 'Máx.' : money(cost)}
            </button>
          </div>`;
        }).join('');
        return `<h2>Mejoras de Float Gardens</h2>
          <p class="sub">Tienes <b>${money(save.money)}</b></p>
          <div class="cards">${cards}</div>
          <div class="buttons"><button class="btn primary" data-action="map">Volver al mapa</button></div>`;
      },

      rewards(save, data) {
        const vouchers = save.vouchers.slice().reverse().map((v) => `<li><code>${v.code}</code><span>${new Date(v.date).toLocaleDateString('es-CO')}</span></li>`).join('');
        const points = FG.OFFICIAL_POINTS.map((p) => `<li><b>${p.name}</b> — ${p.city}<br><span>${p.address}</span></li>`).join('');
        return `<h2>🎟️ Puntos Float</h2>
          ${this.pointsBox(save)}
          ${data.newCode ? `<div class="voucher-new">
              <div>¡Tu café gratis!</div>
              <code>${data.newCode}</code>
              <small>Muestra este código en un punto oficial de Float Gardens.</small>
            </div>` : ''}
          <div class="buttons">
            <button class="btn primary" data-action="redeem" ${save.puntos < C.REDEEM_COST ? 'disabled' : ''}>Canjear café gratis (${C.REDEEM_COST} ★)</button>
          </div>
          <div class="cols">
            <div><h3>Mis cupones</h3>${vouchers ? `<ul class="vouchers">${vouchers}</ul>` : '<p class="sub small">Aún no tienes cupones. ¡A farmear!</p>'}</div>
            <div><h3>Puntos oficiales</h3><ul class="official">${points}</ul></div>
          </div>
          <p class="sub small">Ganas Puntos Float cosechando cerezas (+${C.POINTS_PER_CHERRY}), sirviendo café (+${C.POINTS_PER_SERVE}), espantando inspectores (+${C.POINTS_PER_INSPECTOR}), venciendo jefes (+${C.POINTS_PER_BOSS}) y conquistando ciudades por primera vez (+${C.POINTS_PER_CITY}).</p>
          <div class="buttons"><button class="btn" data-action="back">Volver</button></div>`;
      },

      pause() {
        return `<h2>Pausa</h2>
          <div class="buttons vertical">
            <button class="btn primary" data-action="resume">▶ Seguir</button>
            <button class="btn" data-action="retry">↻ Reiniciar ciudad</button>
            <button class="btn" data-action="quit-level">Salir al mapa</button>
          </div>`;
      },

      levelEnd(save, d) {
        const e = d.earned;
        return `<h2>${d.won ? `🚩 ¡${d.city.name} conquistada!` : `☕ La Federación resistió en ${d.city.name}`}</h2>
          <p class="sub">${d.won ? 'Float Gardens abre sus puertas en ' + d.city.district + '.' : 'Se acabó el tiempo. Mejora tu negocio e inténtalo de nuevo.'}</p>
          <div class="stats">
            <div><span>${e.harvested}</span>cerezas farmeadas</div>
            <div><span>${e.served}</span>cafés vendidos</div>
            <div><span>${e.lost}</span>clientes perdidos</div>
            <div><span>${e.repelled}</span>inspectores espantados</div>
            <div><span>${money(e.money)}</span>ventas</div>
            <div><span>★ ${e.puntos}</span>Puntos Float</div>
          </div>
          ${this.pointsBox(save)}
          <div class="buttons">
            ${d.won ? '<button class="btn primary" data-action="map">Continuar ➜</button>' : '<button class="btn primary" data-action="retry">↻ Reintentar</button>'}
            <button class="btn" data-action="shop">🛠️ Mejoras</button>
            ${save.puntos >= C.REDEEM_COST ? '<button class="btn" data-action="rewards">🎟️ Canjear café</button>' : ''}
          </div>`;
      },

      victory(save) {
        return `${this.logo()}
          <h2>¡Conquistaste el mundo del café!</h2>
          <div class="story">
            <p>De Chapinero a Shibuya, el café hidropónico flotante de Float Gardens llegó a cada esquina. Don Monopolio colgó el sombrero y la Federación... abrió su propio invernadero flotante.</p>
            <p>Todo empezó con una AeroPress y una idea compartida. <b>Gracias por cultivar con nosotros.</b></p>
          </div>
          ${this.pointsBox(save)}
          <div class="buttons">
            <button class="btn primary" data-action="rewards">🎟️ Canjear Puntos Float</button>
            <button class="btn" data-action="map">Seguir farmeando</button>
          </div>`;
      },

      confirmReset() {
        return `<h2>¿Reiniciar progreso?</h2>
          <p class="sub">Se borran ciudades, dinero, mejoras y Puntos Float de este navegador. Los cupones ya generados también desaparecen de la lista.</p>
          <div class="buttons">
            <button class="btn danger" data-action="reset-confirm">Sí, borrar todo</button>
            <button class="btn primary" data-action="menu">Cancelar</button>
          </div>`;
      },
    },
  };

  FG.UI = UI;
})();
