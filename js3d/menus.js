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
  // íconos de trazo fino para botones y mejoras (la web no usa emojis)
  const ic = (p) => `<svg class="ic" viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="1.8">${p}</svg>`;
  const IC = {
    leaf: ic('<path d="M5 19c0-8 5-13 14-14-1 9-6 14-14 14z"/><path d="M5 19 13 11"/>'),
    shop: ic('<path d="M5 8h14l-1 13H6z"/><path d="M9 8a3 3 0 0 1 6 0"/>'),
    ticket: ic('<path d="M3 8h18v3a2 2 0 0 0 0 4v3H3v-3a2 2 0 0 0 0-4z"/><path d="M14 8v10" stroke-dasharray="2 2"/>'),
    book: ic('<path d="M4 5h7a2 2 0 0 1 2 2v13a2 2 0 0 0-2-2H4zM20 5h-5a2 2 0 0 0-2 2"/><path d="M20 5v13h-5a2 2 0 0 0-2 2"/>'),
    pad: ic('<rect x="3" y="8" width="18" height="10"/><path d="M8 11v4M6 13h4M16 12h.01M18 14h.01"/>'),
    cam: ic('<path d="M4 8h3l2-2h6l2 2h3v11H4z"/><circle cx="12" cy="13" r="3.2"/>'),
    sun: ic('<circle cx="12" cy="12" r="4"/><path d="M12 2v3M12 19v3M2 12h3M19 12h3M5 5l2 2M17 17l2 2M5 19l2-2M17 7l2-2"/>'),
    moon: ic('<path d="M20 14.5A8 8 0 1 1 9.500 4a6.500 6.500 0 0 0 10.500 10.500z"/>'),
    sound: ic('<path d="M4 10v4h3l5 4V6L7 10z"/><path d="M16 9a4 4 0 0 1 0 6M18.500 6.500a8 8 0 0 1 0 11"/>'),
    mute: ic('<path d="M4 10v4h3l5 4V6L7 10z"/><path d="M16 10l5 5M21 10l-5 5"/>'),
    play: ic('<path d="M7 5l12 7-12 7z"/>'),
    dano: ic('<circle cx="12" cy="12" r="7"/><path d="M12 2v5M12 17v5M2 12h5M17 12h5"/>'),
    cargador: ic('<rect x="8" y="3" width="8" height="18"/><path d="M8 8h8M8 13h8"/>'),
    vida: ic('<path d="M12 20s-7-4.500-7-10a4 4 0 0 1 7-2.500A4 4 0 0 1 19 10c0 5.500-7 10-7 10z"/>'),
    tenis: ic('<path d="M3 17h18v-2l-6-2-3-5-3 1v4H3z"/>'),
    cultivo: ic('<path d="M12 21v-9M12 12c0-4-3-6-7-6 0 4 3 6 7 6zM12 14c0-3 2.500-5 6-5 0 3-2.500 5-6 5z"/>'),
  };
  const LEAF_TILE = '<svg viewBox="0 0 24 24" width="18" height="18"><path d="M12 3c4.500 2.500 6.500 6 6 10.500-.3 3.400-2.900 6.200-6 7.500-3.100-1.300-5.700-4.100-6-7.500C5.500 9 7.500 5.500 12 3z" fill="currentColor"/><path d="M12 6v13" stroke="#fff" stroke-width=".9"/></svg>';

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
      if (window.fgIcons) fgIcons(this.el);
    },

    topbar(s) {
      return `<div class="topbar">
        <div class="brand"><span class="leaf">${LEAF_TILE}</span> floatGardens <small>Kennedy · Neo-Bogotá</small></div>
        <div class="chips">
          <span class="chip money">${money(s.money)}</span>
          <span class="chip pts">${s.puntos.toLocaleString('es-CO')} <small>puntos</small></span>
        </div>
      </div>`;
    },

    // Cámara (cenital / tercera persona) y tema de la cenital (noche / día)
    viewButtons(cls) {
      const c = cls || 'btn-side';
      const td = W.TopDown.active;
      // el tema (día / noche) vale para las dos cámaras
      return `<button class="${c}" data-a="camera">${IC.cam}Cámara · ${td ? 'cenital' : 'tercera persona'}</button>` +
        `<button class="${c}" data-a="theme">${W.TopDown.theme === 'noche' ? `${IC.moon}Luz · noche` : `${IC.sun}Luz · día`}</button>`;
    },

    pointsMeter(s) {
      const cost = FG.CONFIG.REDEEM_COST;
      const frac = Math.min(1, s.puntos / cost);
      const n = (v) => v.toLocaleString('es-CO');
      return `<div class="pmeter"><div class="pmeter-label">Puntos Float · ${n(s.puntos)} / ${n(cost)}</div>
        <div class="pmeter-bar"><i style="width:${frac * 100}%"></i></div>
        <small>${s.puntos >= cost ? 'Tienes un café gratis para canjear' : `Te faltan ${n(cost - s.puntos)} para un café gratis`}</small></div>`;
    },

    screens: {
      // Lobby del celular (rediseño v3): la web en vertical. Cápsula y marca, la ventana a la maqueta con el riel,
      // el selector de red y la tarjeta verde de tu terraza; título, métricas, Jugar y la tabla de zonas de la red.
      lobbyTd(s) {
        const G = this.G;
        const n = G.capturedCount();
        const started = n > 0 || s.world.kos > 0;
        const d = W.World.districtAt(W.World.hqSpot.spawn.x, W.World.hqSpot.spawn.z);
        const hq = G.planters.filter((pl) => pl.group === 'hq');
        const avg = hq.length ? Math.round(hq.reduce((a, pl) => a + Math.min(1, pl.growth), 0) / hq.length * 100) : 0;
        const ready = hq.filter((pl) => pl.ripe).length, sick = hq.filter((pl) => pl.roya > 0).length;
        const eta = sick ? `Roya en ${sick} ${sick === 1 ? 'mata' : 'matas'}` : ready ? `${ready} ${ready === 1 ? 'mata lista' : 'matas listas'} para cosechar` : avg > 70 ? 'Casi lista · sigue creciendo' : 'Creciendo';
        const sealed = G.zones.filter((z) => z.owner !== 'fg');
        const toast = s.world.bossDefeated ? 'Bogotá es de la red · sigue cosechando' : sealed.length ? `Arcadia tiene ${sealed.length === 1 ? sealed[0].name : `${sealed.length} zonas selladas`}` : `Ve a la Plaza de Bolívar · ${W.BOSS.name}`;
        const rows = G.zones.map((z) => `<tr><td>${z.name}</td><td class="num">${z.owner === 'fg' ? '100' : Math.round(z.progress * 100)} %</td><td><span class="chip ${z.owner === 'fg' ? '' : 'crit'}">${z.owner === 'fg' ? 'Libre' : 'Sellada'}</span></td></tr>`).join('');
        const dot = s.puntos >= FG.CONFIG.REDEEM_COST ? 'on' : '';
        return `<div class="lob">
          <div class="eyebrow"><span class="pill">Float Gardens · Temporada 1</span><span class="brand"><i data-i="hoja"></i><span>float<b>Gardens</b></span></span></div>
          <section class="frame">
            <div class="td-top">
              <nav class="td-rail"><span class="logo"><i data-i="hoja"></i></span><button class="on" data-a="play" aria-label="jugar"><i data-i="mapa"></i></button><button data-a="shop" aria-label="mejoras"><i data-i="mochila"></i></button><button data-a="rewards" aria-label="puntos"><i data-i="red"></i></button></nav>
              <div class="td-icons"><button data-a="rewards" aria-label="avisos"><i data-i="campana"></i><span class="dot ${dot}"></span></button><button data-a="ajustes" aria-label="ajustes"><i data-i="cuenta"></i></button></div>
            </div>
            <div class="selector"><span>Red <b>${d.name}</b></span><i><i data-i="pin"></i></i></div>
            <article class="td-harvest"><div class="in"><h3>Tu terraza · filas A B C</h3><div class="sub">${W.HQ.name}</div><div class="big num">${avg}<span class="u">%</span></div><div class="eta">${eta}</div></div>
              <div class="actions"><button data-a="play">Jugar</button><button data-a="howto" aria-label="cómo jugar">?</button><button data-a="shop">Vivero</button></div></article>
            <div class="toast ${sealed.length && !s.world.bossDefeated ? 'foe' : ''}">${toast}</div>
          </section>
          <h1>Defiende tu<br><b>terraza</b></h1>
          <div class="kpis">
            <div><span class="num">${G.cherries}<span class="u">cerezas</span></span><span class="lbl">en tu canasta</span></div>
            <div><span class="num">${n}<span class="u">/ ${G.zones.length}</span></span><span class="lbl">zonas libres</span></div>
            <div><span class="num">${s.puntos.toLocaleString('es-CO')}</span><span class="lbl">Puntos Float</span></div>
          </div>
          <div class="cta"><button class="btn" data-a="play"><span>${started ? 'Continuar' : 'Jugar'}</span><span class="mono">${d.name} · ${W.TopDown.theme === 'noche' ? 'noche' : 'día'}</span></button><button class="btn alt" data-a="shop">Mejoras</button></div>
          <section class="net"><div class="net-head"><span>Zonas de la red</span><div class="filters"><button class="on">Todas</button><button data-a="rewards">Puntos Float</button></div></div>
            <table>${rows}</table></section>
          <div class="foot"><span>Juego de ficción · no afiliado a ninguna organización real</span><button data-a="ajustes">Ajustes</button><button data-a="reset">Reiniciar mundo</button></div>
        </div>`;
      },

      ajustes() {
        return `<div class="panel narrow">
          <h2>Ajustes</h2>
          <div class="col">
            ${this.viewButtons('btn-g')}
            <button class="btn-g" data-a="sound">${W.Audio.muted ? `${IC.mute}Sonido · no` : `${IC.sound}Sonido · sí`}</button>
            <button class="btn-g" data-a="howto">${IC.book}Cómo jugar</button>
            <a class="btn-g" href="clasico.html">${IC.pad}Modo clásico 2D</a>
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
        const view = W.TopDown.active ? 'vista cenital' : 'tercera persona';
        return `${this.topbar(s)}
          <div class="lobby">
            <div class="lobby-left">
              <div class="mode-tag">temporada 01 · ${s.world.bossDefeated ? 'red libre' : 'cosecha'}</div>
              <h1 class="title">Defiende tu<br><span>terraza</span></h1>
              <p class="pitch">Cosecha café hidropónico en la torre colgante de Kennedy, véndelo en tus barrios y libera Neo-Bogotá de los drones de Arcadia.</p>
              <div class="dock">
                <div><label>cerezas</label><p class="cherry">${G.cherries}</p></div>
                <div><label>zonas libres</label><p>${n}<small>/ ${G.zones.length}</small></p></div>
                <div><label>drones apagados</label><p>${s.world.kos.toLocaleString('es-CO')}</p></div>
                <div><label>cafés vendidos</label><p>${s.world.sales.toLocaleString('es-CO')}</p></div>
              </div>
              ${this.pointsMeter(s)}
            </div>
            <div class="lobby-right">
              <button class="btn-play" data-a="play"><b>${started ? 'Continuar' : 'Jugar'}</b><small>Kennedy · ${view} · ${W.TopDown.theme === 'noche' ? 'noche' : 'día'}</small></button>
              <div class="side-buttons">
                <button class="btn-side" data-a="shop">${IC.shop}Mejoras</button>
                <button class="btn-side" data-a="rewards">${IC.ticket}Puntos Float</button>
                <button class="btn-side" data-a="howto">${IC.book}Cómo jugar</button>
                <a class="btn-side" href="clasico.html">${IC.pad}Modo clásico 2D</a>
                ${this.viewButtons()}
                <button class="btn-side" data-a="sound">${W.Audio.muted ? `${IC.mute}Sonido · no` : `${IC.sound}Sonido · sí`}</button>
              </div>
            </div>
          </div>
          <div class="lobby-foot">Juego de ficción · no afiliado a ninguna organización real · <a href="promo/">banners</a> · <button class="link" data-a="reset">reiniciar mundo</button></div>`;
      },

      howto() {
        const touch = W.Input.isTouch;
        return `<div class="panel">
          <h2>Cómo jugar</h2>
          <div class="howto">
            <div><b>Cosecha</b>Las filas A, B y C de tu terraza en Kennedy dan cerezas cuando la etiqueta dice "lista". También hay costales de café por la ciudad. Cada cereza suma Puntos Float.</div>
            <div><b>Dispara</b>Tu prensa dispara chorros de café a presión. Recargar gasta 1 cereza. Un tiro al ojo del dron hace más daño.</div>
            <div><b>Impulso</b>La mochila de prensas te lanza en la dirección en que te mueves. Gasta energía y, mientras dura, los sellos no te tocan.</div>
            <div><b>Willys</b>El campero de la red está parqueado frente a tu terraza. Súbete con Acción y manéjalo con el joystick: por la calle va casi tres veces más rápido que a pie. Bájate con Acción cuando llegues a la zona.</div>
            <div><b>Vende</b>Los peatones con una taza quieren café, pero solo te compran en barrios de la red. 2 cerezas = $8.000.</div>
            <div><b>Libera</b>Entra a las zonas azules, apaga las oleadas de drones y quédate dentro hasta liberarlas. Cada zona abre un barrio para vender y cultivar.</div>
            <div><b>Plagas</b>La roya, la broca y las hormigas arrieras no van por ti: van por tus matas. Apágalas antes de que lleguen. Si una fila amarillea, acércate y trata la roya.</div>
            <div><b>Variedades</b>En el vivero (Mejoras) siembras Caturra, Castillo, Borbón amarillo, Tabi o Geisha. Castillo y Tabi resisten la roya; Geisha paga más por cosecha.</div>
            <div><b>Arcadia</b>Con las 6 zonas libres, ve a la Plaza de Bolívar y derrota a Don Monopolio. Si caes, vuelves a tu terraza y pierdes la mitad de las cerezas.</div>
          </div>
          <div class="keys">${W.TopDown.active
            ? (touch ? 'Vista cenital · joystick: moverte (al fondo = correr) · arrastra a la derecha o sobre <b>disparar</b>: apuntar (la mira se pega al dron más cercano) · impulso · cosechar · recargar'
              : 'Vista cenital · <kbd>WASD</kbd> moverte · <kbd>Mouse</kbd> apuntar · <kbd>Clic</kbd> disparar · <kbd>Shift</kbd> correr · <kbd>Espacio</kbd> impulso · <kbd>R</kbd> recargar · <kbd>E</kbd> acción · <kbd>M</kbd> mapa · <kbd>Esc</kbd> pausa')
            : touch
            ? 'Joystick: moverte (al fondo = correr) · desliza a la derecha: mirar · disparar (arrastra para apuntar) · apuntar · saltar · recargar · acción'
            : '<kbd>WASD</kbd> moverte · <kbd>Mouse</kbd> mirar · <kbd>Clic</kbd> disparar · <kbd>Clic der.</kbd> apuntar · <kbd>Shift</kbd> correr · <kbd>Espacio</kbd> saltar · <kbd>Q</kbd> impulso · <kbd>R</kbd> recargar · <kbd>E</kbd> acción · <kbd>M</kbd> mapa · <kbd>Esc</kbd> pausa'}</div>
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
            <div class="uicon">${IC[u.id] || IC.leaf}</div>
            <div class="ubody"><b>${u.name}</b><p>${u.desc}</p>
              <div class="pips">${Array.from({ length: u.max }, (_, i) => `<i class="${i < lvl ? 'on' : ''}"></i>`).join('')}</div></div>
            <button class="btn-buy" data-a="buy" data-id="${u.id}" ${maxed || s.money < cost ? 'disabled' : ''}>${maxed ? 'máx.' : money(cost)}</button>
          </div>`;
        }).join('');
        // vivero: qué variedad lleva cada fila de la terraza
        const rows = s.world.rows || W.ROWS_DEFAULT;
        const trait = (v) => [
          v.roya >= 0.7 ? '<i class="ok">resiste la roya</i>' : '<i class="bad">susceptible a la roya</i>',
          v.grow >= 1.2 ? '<i>crece rápido</i>' : v.grow <= 0.85 ? '<i>crece lento</i>' : '',
          v.yield ? `<i>+${v.yield} cereza</i>` : '',
          v.bonus ? `<i>+${money(v.bonus)} por cosecha</i>` : '',
        ].join('');
        const vcards = Object.keys(W.VARIEDADES).map((id) => {
          const v = W.VARIEDADES[id];
          const btns = ['A', 'B', 'C'].map((r) => `<button class="btn-row ${rows[r] === id ? 'on' : ''}" data-a="plant" data-id="${id}" data-row="${r}" ${rows[r] === id || s.money < v.cost ? 'disabled' : ''} title="Sembrar ${v.name} en la fila ${r}">${r}</button>`).join('');
          return `<div class="vcard">
            <div class="vdot" style="background:${v.cherry[0]}"></div>
            <div class="ubody"><b>${v.name}</b><p>${v.desc}</p><div class="traits">${trait(v)}</div></div>
            <div class="vbuy"><small>${money(v.cost)} por fila</small><div class="vrows">${btns}</div></div>
          </div>`;
        }).join('');
        return `<div class="panel wide">
          <h2>Mejoras y vivero</h2>
          <p class="sub">Tienes <b>${money(s.money)}</b> · gana plata vendiendo café, liberando zonas y apagando drones</p>
          <div class="ucards">${cards}</div>
          <h3 class="vh">Vivero · variedades de café</h3>
          <p class="sub">Elige qué sembrar en las filas A, B y C de tu terraza. La fila resembrada arranca de cero. Las resistentes aguantan la roya; las delicadas pagan mejor.</p>
          <div class="vcards">${vcards}</div>
          <div class="row"><button class="btn-y" data-a="${d.from === 'game' ? 'resume' : 'back'}">${d.from === 'game' ? 'Volver al juego' : 'Volver'}</button></div>
        </div>`;
      },

      rewards(s, d) {
        const cost = FG.CONFIG.REDEEM_COST;
        const vouchers = s.vouchers.slice().reverse().map((v) => `<li><code>${v.code}</code><span>${new Date(v.date).toLocaleDateString('es-CO')}</span></li>`).join('');
        const points = FG.OFFICIAL_POINTS.map((p) => `<li><b>${p.name}</b> — ${p.city}<br><span>${p.address}</span></li>`).join('');
        return `<div class="panel">
          <h2>Puntos Float</h2>
          ${this.pointsMeter(s)}
          ${d.code ? `<div class="voucher"><div>Tu café gratis</div><code>${d.code}</code><small>Muéstralo en un punto oficial de Float Gardens</small></div>` : ''}
          <div class="row"><button class="btn-y" data-a="redeem" ${s.puntos < cost ? 'disabled' : ''}>Canjear café gratis · ${cost.toLocaleString('es-CO')} puntos</button></div>
          <div class="cols">
            <div><h3>Mis cupones</h3>${vouchers ? `<ul class="list">${vouchers}</ul>` : '<p class="sub">Aún no tienes cupones. A cosechar.</p>'}</div>
            <div><h3>Puntos oficiales</h3><ul class="list">${points}</ul></div>
          </div>
          <p class="sub small">Los Puntos Float se comparten con el modo clásico. Ganas puntos cosechando (+1 por cereza), vendiendo (+10), apagando drones (+5), liberando zonas (+100) y venciendo a Don Monopolio (+300).</p>
          <div class="row"><button class="btn-g" data-a="back">Volver</button></div>
        </div>`;
      },

      pause() {
        return `<div class="panel narrow">
          <h2>Pausa</h2>
          <div class="col">
            <button class="btn-y" data-a="resume">${IC.play}Seguir jugando</button>
            <button class="btn-g" data-a="shop-game">${IC.shop}Mejoras</button>
            <button class="btn-g" data-a="rewards">${IC.ticket}Puntos Float</button>
            <button class="btn-g" data-a="howto">${IC.book}Cómo jugar</button>
            ${this.viewButtons('btn-g')}
            <button class="btn-g" data-a="lobby">Volver al lobby</button>
          </div>
          <p class="sub small">${W.Input.isTouch || W.TopDown.active ? '' : 'Haz clic en "Seguir jugando" para volver a capturar el mouse.'}</p>
        </div>`;
      },

      dead(s, d) {
        return `<div class="panel narrow dead">
          <h2 class="big-red">Arcadia te selló</h2>
          <p class="sub">Los drones cerraron tu ruta. Perdiste ${d.lost} cerezas.</p>
          <div class="col"><button class="btn-y" data-a="respawn">Volver a mi terraza</button></div>
        </div>`;
      },

      victory(s) {
        return `<div class="panel">
          <h2 class="big-green">Bogotá es de la red</h2>
          <p class="sub">Don Monopolio colgó el sombrero y Arcadia quitó sus sellos. Todo empezó con una prensa de café y una terraza compartida en Kennedy.</p>
          <div class="stats"><div><b>${s.world.kos}</b>drones apagados</div><div><b>${s.world.sales}</b>cafés vendidos</div><div><b>${s.puntosTotal.toLocaleString('es-CO')}</b>Puntos Float totales</div></div>
          ${this.pointsMeter(s)}
          <div class="row"><button class="btn-y" data-a="resume">Seguir cosechando</button><button class="btn-g" data-a="rewards">Canjear</button></div>
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
        case 'plant': {
          const v = W.VARIEDADES[d.id];
          if (v && s.money >= v.cost && (s.world.rows || {})[d.row] !== d.id) {
            s.money -= v.cost;
            G.replantRow(d.row, d.id);
            W.Audio.cash();
          }
          this.show('shop', this.data);
          break;
        }
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
          this.show(prev === 'ajustes' || prev === 'pause' ? prev : 'lobby');
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
