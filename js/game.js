// Punto de entrada: estados del juego, bucle principal y acciones de la interfaz.
(function () {
  const C = FG.CONFIG;

  const Game = {
    state: 'menu',
    level: null,
    returnTo: 'menu',

    init() {
      this.canvas = document.getElementById('game');
      this.ctx = this.canvas.getContext('2d');
      this.touchEl = document.getElementById('touch');
      this.isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
      this.save = FG.Save.load();
      this.attractBg = FG.Render.buildBackground(FG.CITIES[0]);
      FG.Input.init();
      FG.UI.init(this);
      this.goto('menu');
      this.last = performance.now();
      requestAnimationFrame((t) => this.loop(t));
    },

    loop(now) {
      const dt = Math.min(0.05, (now - this.last) / 1000);
      this.last = now;
      const t = now / 1000;

      if (this.state === 'playing') {
        if (FG.Input.pressed('pause')) this.pause();
        else this.level.update(dt);
      } else if (this.state === 'paused' && FG.Input.pressed('pause')) {
        this.resume();
      }

      if (this.level) {
        this.level.render(this.ctx, t);
      } else {
        this.ctx.drawImage(this.attractBg, 0, 0);
        FG.Render.drawWater(this.ctx, t);
      }
      FG.Input.endFrame();
      requestAnimationFrame((n) => this.loop(n));
    },

    persist() { FG.Save.write(this.save); },

    goto(screen, data) {
      if (screen !== 'pause' && screen !== 'levelEnd') this.level = null;
      this.state = screen;
      this.touchEl.classList.add('hidden');
      FG.UI.show(screen, data);
    },

    startCity(index) {
      this.cityIndex = index;
      this.level = new FG.Level(this, index);
      this.state = 'playing';
      FG.Input.endFrame(); // descarta teclas presionadas en el menú
      FG.UI.hide();
      if (this.isTouch) this.touchEl.classList.remove('hidden');
    },

    pause() {
      this.state = 'paused';
      this.touchEl.classList.add('hidden');
      FG.UI.show('pause');
    },

    resume() {
      this.state = 'playing';
      FG.Input.endFrame();
      FG.UI.hide();
      if (this.isTouch) this.touchEl.classList.remove('hidden');
    },

    onLevelEnd(won, level) {
      const s = this.save;
      if (won && !s.conquered.includes(level.cityIndex)) {
        s.conquered.push(level.cityIndex);
        level.addPoints(C.POINTS_PER_CITY);
      }
      this.persist();
      this.state = 'levelEnd';
      this.touchEl.classList.add('hidden');
      const allDone = FG.CITIES.every((_, i) => s.conquered.includes(i));
      if (won && level.cityIndex === FG.CITIES.length - 1 && allDone) {
        this.goto('victory');
        return;
      }
      FG.UI.show('levelEnd', { won, city: level.city, earned: level.earned });
    },

    handleAction(action, data) {
      const s = this.save;
      switch (action) {
        case 'play':
          if (!s.seenIntro) this.goto('intro');
          else this.goto('map');
          break;
        case 'intro-done':
          s.seenIntro = true;
          this.persist();
          this.goto('map');
          break;
        case 'menu':
        case 'map':
        case 'shop':
          this.goto(action);
          break;
        case 'howto':
        case 'rewards':
          this.returnTo = this.state === 'menu' ? 'menu' : 'map';
          this.goto(action);
          break;
        case 'back':
          this.goto(this.returnTo);
          break;
        case 'start-city':
          this.startCity(Number(data.index));
          break;
        case 'resume':
          this.resume();
          break;
        case 'retry':
          this.startCity(this.cityIndex);
          break;
        case 'quit-level':
          this.persist();
          this.goto('map');
          break;
        case 'buy': {
          const u = FG.UPGRADES.find((x) => x.id === data.id);
          const lvl = s.upgrades[u.id] || 0;
          const cost = u.cost * (lvl + 1);
          if (lvl < u.max && s.money >= cost) {
            s.money -= cost;
            s.upgrades[u.id] = lvl + 1;
            this.persist();
          }
          this.goto('shop');
          break;
        }
        case 'redeem':
          if (s.puntos >= C.REDEEM_COST) {
            s.puntos -= C.REDEEM_COST;
            const code = FG.Vouchers.create();
            s.vouchers.push({ code, date: Date.now() });
            this.persist();
            this.goto('rewards', { newCode: code });
          }
          break;
        case 'reset':
          this.goto('confirmReset');
          break;
        case 'reset-confirm':
          this.save = FG.Save.reset();
          this.goto('menu');
          break;
      }
    },
  };

  window.addEventListener('load', () => Game.init());
  FG.Game = Game;
})();
