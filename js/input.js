// Teclado + controles táctiles (joystick virtual y dos botones).
(function () {
  const BINDINGS = {
    up: ['ArrowUp', 'KeyW'],
    down: ['ArrowDown', 'KeyS'],
    left: ['ArrowLeft', 'KeyA'],
    right: ['ArrowRight', 'KeyD'],
    action: ['KeyE', 'Enter'],
    shoot: ['Space', 'KeyJ', 'KeyK'],
    pause: ['KeyP', 'Escape'],
  };
  const CAPTURED = new Set(['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Space']);

  const Input = {
    keys: new Set(),
    pressedSet: new Set(),
    touch: { x: 0, y: 0, action: false, shoot: false },

    init() {
      window.addEventListener('keydown', (e) => {
        if (CAPTURED.has(e.code) && !isTypingTarget(e.target)) e.preventDefault();
        if (!e.repeat) {
          for (const name in BINDINGS) if (BINDINGS[name].includes(e.code)) this.pressedSet.add(name);
        }
        this.keys.add(e.code);
      });
      window.addEventListener('keyup', (e) => this.keys.delete(e.code));
      window.addEventListener('blur', () => this.keys.clear());
      this.initTouch();
    },

    held(name) {
      if (name === 'action' && this.touch.action) return true;
      if (name === 'shoot' && this.touch.shoot) return true;
      return BINDINGS[name].some((k) => this.keys.has(k));
    },

    pressed(name) {
      return this.pressedSet.has(name);
    },

    axis() {
      let x = (this.held('right') ? 1 : 0) - (this.held('left') ? 1 : 0);
      let y = (this.held('down') ? 1 : 0) - (this.held('up') ? 1 : 0);
      x += this.touch.x;
      y += this.touch.y;
      const len = Math.hypot(x, y);
      if (len > 1) { x /= len; y /= len; }
      return { x, y };
    },

    endFrame() {
      this.pressedSet.clear();
    },

    initTouch() {
      const stick = document.getElementById('stick');
      const knob = document.getElementById('knob');
      const btnAction = document.getElementById('btnAction');
      const btnShoot = document.getElementById('btnShoot');
      if (!stick) return;

      let stickId = null;
      const moveStick = (t) => {
        const r = stick.getBoundingClientRect();
        const cx = r.left + r.width / 2;
        const cy = r.top + r.height / 2;
        let dx = (t.clientX - cx) / (r.width / 2);
        let dy = (t.clientY - cy) / (r.height / 2);
        const len = Math.hypot(dx, dy);
        if (len > 1) { dx /= len; dy /= len; }
        this.touch.x = Math.abs(dx) < 0.15 ? 0 : dx;
        this.touch.y = Math.abs(dy) < 0.15 ? 0 : dy;
        knob.style.transform = `translate(${dx * 35}px, ${dy * 35}px)`;
      };
      const resetStick = () => {
        stickId = null;
        this.touch.x = 0;
        this.touch.y = 0;
        knob.style.transform = '';
      };

      stick.addEventListener('touchstart', (e) => {
        e.preventDefault();
        const t = e.changedTouches[0];
        stickId = t.identifier;
        moveStick(t);
      }, { passive: false });
      stick.addEventListener('touchmove', (e) => {
        e.preventDefault();
        for (const t of e.changedTouches) if (t.identifier === stickId) moveStick(t);
      }, { passive: false });
      stick.addEventListener('touchend', (e) => {
        for (const t of e.changedTouches) if (t.identifier === stickId) resetStick();
      });
      stick.addEventListener('touchcancel', resetStick);

      const bindButton = (el, name) => {
        el.addEventListener('touchstart', (e) => {
          e.preventDefault();
          this.touch[name] = true;
          this.pressedSet.add(name);
        }, { passive: false });
        const up = (e) => { e.preventDefault(); this.touch[name] = false; };
        el.addEventListener('touchend', up);
        el.addEventListener('touchcancel', up);
      };
      bindButton(btnAction, 'action');
      bindButton(btnShoot, 'shoot');
    },
  };

  function isTypingTarget(el) {
    return el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA');
  }

  FG.Input = Input;
})();
