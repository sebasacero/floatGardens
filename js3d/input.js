// Controles estilo shooter: teclado + mouse (pointer lock) y táctil (joystick + deslizar para apuntar).
(function () {
  const KEYS = {
    up: ['KeyW', 'ArrowUp'], down: ['KeyS', 'ArrowDown'], left: ['KeyA', 'ArrowLeft'], right: ['KeyD', 'ArrowRight'],
    sprint: ['ShiftLeft', 'ShiftRight'], jump: ['Space'], dash: ['KeyQ'], action: ['KeyE'], reload: ['KeyR'],
    map: ['KeyM', 'Tab'], pause: ['Escape', 'KeyP'], fire: [], aim: [],
  };

  const Input = {
    keys: new Set(),
    pressedSet: new Set(),
    mouse: { dx: 0, dy: 0, fire: false, aim: false },
    touch: { x: 0, y: 0, fire: false, aim: false, sprint: false },
    sensitivity: 0.0022,
    touchSensitivity: 0.006,
    isTouch: false,
    topdown: false,
    // vista cenital: vector de mira del dedo (desde donde tocó) o posición del mouse
    aimVec: { x: 0, y: 0, active: false },

    init(canvas) {
      this.canvas = canvas;
      this.isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
      window.addEventListener('keydown', (e) => {
        if (['Space', 'Tab', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.code)) e.preventDefault();
        if (!e.repeat) for (const k in KEYS) if (KEYS[k].includes(e.code)) this.pressedSet.add(k);
        this.keys.add(e.code);
      });
      window.addEventListener('keyup', (e) => this.keys.delete(e.code));
      window.addEventListener('blur', () => { this.keys.clear(); this.mouse.fire = false; this.mouse.aim = false; });

      canvas.addEventListener('mousedown', (e) => {
        if (!this.locked && !this.topdown) return;
        if (e.button === 0) { this.mouse.fire = true; this.pressedSet.add('fire'); }
        if (e.button === 2) this.mouse.aim = true;
      });
      window.addEventListener('mouseup', (e) => {
        if (e.button === 0) this.mouse.fire = false;
        if (e.button === 2) this.mouse.aim = false;
      });
      canvas.addEventListener('contextmenu', (e) => e.preventDefault());
      document.addEventListener('mousemove', (e) => {
        this.mouse.px = e.clientX; this.mouse.py = e.clientY;
        if (!this.locked) return;
        this.mouse.dx += e.movementX || 0;
        this.mouse.dy += e.movementY || 0;
      });
      document.addEventListener('pointerlockchange', () => {
        this.locked = document.pointerLockElement === canvas;
        if (!this.locked) { this.mouse.fire = false; this.mouse.aim = false; }
        if (this.onLockChange) this.onLockChange(this.locked);
      });
      this.initTouch();
    },

    lock() {
      if (this.isTouch || !this.canvas.requestPointerLock) return;
      try {
        const r = this.canvas.requestPointerLock();
        if (r && r.catch) r.catch(() => {});
      } catch (e) { /* algunos navegadores lo bloquean */ }
    },
    unlock() { if (document.pointerLockElement) document.exitPointerLock(); },

    held(name) {
      if (name === 'fire') return this.mouse.fire || this.touch.fire;
      if (name === 'aim') return this.mouse.aim || this.touch.aim;
      if (name === 'sprint' && this.touch.sprint) return true;
      return KEYS[name].some((k) => this.keys.has(k));
    },
    pressed(name) { return this.pressedSet.has(name); },

    move() {
      let x = (this.held('right') ? 1 : 0) - (this.held('left') ? 1 : 0) + this.touch.x;
      let y = (this.held('up') ? 1 : 0) - (this.held('down') ? 1 : 0) - this.touch.y;
      const l = Math.hypot(x, y);
      if (l > 1) { x /= l; y /= l; }
      return { x, y };
    },

    look() {
      const out = { x: this.mouse.dx * this.sensitivity, y: this.mouse.dy * this.sensitivity };
      out.x += this.lookTouch.dx * this.touchSensitivity;
      out.y += this.lookTouch.dy * this.touchSensitivity;
      this.mouse.dx = this.mouse.dy = 0;
      this.lookTouch.dx = this.lookTouch.dy = 0;
      return out;
    },

    endFrame() { this.pressedSet.clear(); },

    lookTouch: { id: null, x: 0, y: 0, dx: 0, dy: 0 },

    initTouch() {
      const root = document.getElementById('touch');
      if (!root) return;
      const stick = document.getElementById('stick');
      const knob = document.getElementById('knob');
      const look = document.getElementById('lookArea');
      let stickId = null;
      const moveStick = (t) => {
        const r = stick.getBoundingClientRect();
        let dx = (t.clientX - r.left - r.width / 2) / (r.width / 2);
        let dy = (t.clientY - r.top - r.height / 2) / (r.height / 2);
        const l = Math.hypot(dx, dy);
        if (l > 1) { dx /= l; dy /= l; }
        this.touch.x = Math.abs(dx) < 0.12 ? 0 : dx;
        this.touch.y = Math.abs(dy) < 0.12 ? 0 : dy;
        this.touch.sprint = l > 0.95;
        knob.style.transform = `translate(${dx * 38}px, ${dy * 38}px)`;
      };
      const resetStick = () => {
        stickId = null;
        this.touch.x = this.touch.y = 0;
        this.touch.sprint = false;
        knob.style.transform = '';
      };
      stick.addEventListener('touchstart', (e) => { e.preventDefault(); const t = e.changedTouches[0]; stickId = t.identifier; moveStick(t); }, { passive: false });
      stick.addEventListener('touchmove', (e) => { e.preventDefault(); for (const t of e.changedTouches) if (t.identifier === stickId) moveStick(t); }, { passive: false });
      stick.addEventListener('touchend', (e) => { for (const t of e.changedTouches) if (t.identifier === stickId) resetStick(); });
      stick.addEventListener('touchcancel', resetStick);

      // Deslizar en la mitad derecha para mirar
      const L = this.lookTouch;
      look.addEventListener('touchstart', (e) => {
        e.preventDefault();
        const t = e.changedTouches[0];
        L.id = t.identifier; L.x = t.clientX; L.y = t.clientY;
        L.sx = t.clientX; L.sy = t.clientY;
        this.aimVec.active = true; this.aimVec.x = 0; this.aimVec.y = 0;
      }, { passive: false });
      look.addEventListener('touchmove', (e) => {
        e.preventDefault();
        for (const t of e.changedTouches) {
          if (t.identifier !== L.id) continue;
          L.dx += t.clientX - L.x; L.dy += t.clientY - L.y;
          L.x = t.clientX; L.y = t.clientY;
          this.aimVec.x = t.clientX - L.sx; this.aimVec.y = t.clientY - L.sy;
        }
      }, { passive: false });
      look.addEventListener('touchend', (e) => { for (const t of e.changedTouches) if (t.identifier === L.id) { L.id = null; this.aimVec.active = false; } });
      look.addEventListener('touchcancel', () => { L.id = null; this.aimVec.active = false; });

      // Botones. El de disparo también sirve para apuntar arrastrando.
      const bind = (id, name, holdKey) => {
        const el = document.getElementById(id);
        if (!el) return;
        let tid = null, lx = 0, ly = 0;
        el.addEventListener('touchstart', (e) => {
          e.preventDefault();
          const t = e.changedTouches[0];
          tid = t.identifier; lx = t.clientX; ly = t.clientY;
          if (name === 'fire') { this.aimVec.active = true; this.aimVec.x = 0; this.aimVec.y = 0; this.fireStart = { x: lx, y: ly }; }
          if (holdKey) this.touch[holdKey] = true;
          this.pressedSet.add(name);
          el.classList.add('on');
        }, { passive: false });
        el.addEventListener('touchmove', (e) => {
          e.preventDefault();
          if (name !== 'fire') return;
          for (const t of e.changedTouches) {
            if (t.identifier !== tid) continue;
            L.dx += t.clientX - lx; L.dy += t.clientY - ly;
            lx = t.clientX; ly = t.clientY;
            if (this.fireStart) { this.aimVec.x = t.clientX - this.fireStart.x; this.aimVec.y = t.clientY - this.fireStart.y; }
          }
        }, { passive: false });
        const up = (e) => { e.preventDefault(); if (holdKey) this.touch[holdKey] = false; el.classList.remove('on'); if (name === 'fire') { this.aimVec.active = false; this.fireStart = null; } };
        el.addEventListener('touchend', up);
        el.addEventListener('touchcancel', up);
      };
      bind('tFire', 'fire', 'fire');
      bind('tAim', 'aim');
      bind('tJump', 'jump');
      bind('tReload', 'reload');
      bind('tAction', 'action');
      bind('tMap', 'map');
      bind('tPause', 'pause');
      document.getElementById('tAim') && document.getElementById('tAim').addEventListener('touchstart', () => { this.touch.aim = !this.touch.aim; });
    },
  };

  W.Input = Input;
})();
