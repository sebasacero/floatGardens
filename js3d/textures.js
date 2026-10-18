// Texturas generadas con canvas: ladrillo bogotano, fachadas coloniales, vidrio, asfalto, grafiti...
(function () {
  function canvas(w, h) {
    const c = document.createElement('canvas');
    c.width = w; c.height = h;
    return [c, c.getContext('2d')];
  }

  function toTexture(c, repeat) {
    const t = new THREE.CanvasTexture(c);
    t.wrapS = t.wrapT = THREE.RepeatWrapping;
    t.anisotropy = 4;
    if (repeat) t.repeat.set(repeat, repeat);
    return t;
  }

  function noise(ctx, w, h, amount, alpha) {
    for (let i = 0; i < amount; i++) {
      const v = Math.random() < 0.5 ? 0 : 255;
      ctx.fillStyle = `rgba(${v},${v},${v},${alpha * Math.random()})`;
      ctx.fillRect(Math.random() * w, Math.random() * h, 2, 2);
    }
  }

  function windowRect(ctx, x, y, w, h, frame, glass) {
    ctx.fillStyle = frame;
    ctx.fillRect(x - 3, y - 3, w + 6, h + 6);
    const g = ctx.createLinearGradient(x, y, x + w, y + h);
    g.addColorStop(0, glass[0]);
    g.addColorStop(1, glass[1]);
    ctx.fillStyle = g;
    ctx.fillRect(x, y, w, h);
    ctx.fillStyle = frame;
    ctx.fillRect(x + w / 2 - 1.5, y, 3, h);
    ctx.fillRect(x, y + h * 0.4, w, 3);
  }

  // Un "módulo" de fachada = 4 m de ancho x 3 m de alto (un piso, una ventana)
  function brick() {
    const [c, ctx] = canvas(128, 96);
    ctx.fillStyle = '#9a4a2f';
    ctx.fillRect(0, 0, 128, 96);
    for (let y = 0; y < 96; y += 8) {
      const off = (y / 8) % 2 ? 8 : 0;
      for (let x = -off; x < 128; x += 16) {
        const s = 0.85 + Math.random() * 0.3;
        ctx.fillStyle = `rgb(${150 * s | 0},${70 * s | 0},${45 * s | 0})`;
        ctx.fillRect(x + 1, y + 1, 14, 6);
      }
    }
    windowRect(ctx, 34, 22, 60, 50, '#e8e2d6', ['#2d3d4f', '#6b8299']);
    return toTexture(c);
  }

  function stucco() {
    const [c, ctx] = canvas(128, 96);
    ctx.fillStyle = '#f4f0e8';
    ctx.fillRect(0, 0, 128, 96);
    noise(ctx, 128, 96, 600, 0.08);
    // zócalo y balcón de madera
    ctx.fillStyle = '#d9d2c4';
    ctx.fillRect(0, 88, 128, 8);
    windowRect(ctx, 40, 18, 48, 54, '#5a3a22', ['#3b3226', '#6b5a44']);
    ctx.fillStyle = '#4a2e1a';
    ctx.fillRect(30, 70, 68, 6);
    for (let x = 32; x < 98; x += 8) ctx.fillRect(x, 58, 3, 12);
    return toTexture(c);
  }

  function glass() {
    const [c, ctx] = canvas(128, 96);
    const g = ctx.createLinearGradient(0, 0, 128, 96);
    g.addColorStop(0, '#5b8fb8');
    g.addColorStop(0.5, '#a8cbe6');
    g.addColorStop(1, '#3e6d94');
    ctx.fillStyle = g;
    ctx.fillRect(0, 0, 128, 96);
    ctx.fillStyle = '#c9d6e0';
    ctx.fillRect(0, 0, 128, 4);
    ctx.fillRect(0, 0, 3, 96);
    ctx.fillRect(64, 0, 2, 96);
    ctx.fillStyle = 'rgba(255,255,255,0.18)';
    ctx.beginPath();
    ctx.moveTo(10, 96); ctx.lineTo(60, 0); ctx.lineTo(80, 0); ctx.lineTo(30, 96);
    ctx.fill();
    return toTexture(c);
  }

  function concrete() {
    const [c, ctx] = canvas(128, 96);
    ctx.fillStyle = '#b9b5ad';
    ctx.fillRect(0, 0, 128, 96);
    noise(ctx, 128, 96, 800, 0.12);
    ctx.fillStyle = '#a19c93';
    ctx.fillRect(0, 0, 128, 6);
    windowRect(ctx, 22, 22, 84, 46, '#7d7a74', ['#34404c', '#71879a']);
    return toTexture(c);
  }

  function warehouse() {
    const [c, ctx] = canvas(128, 128);
    ctx.fillStyle = '#8e969c';
    ctx.fillRect(0, 0, 128, 128);
    for (let x = 0; x < 128; x += 8) {
      ctx.fillStyle = x % 16 ? '#9aa2a8' : '#7c848a';
      ctx.fillRect(x, 0, 4, 128);
    }
    noise(ctx, 128, 128, 400, 0.1);
    return toTexture(c);
  }

  function roof() {
    const [c, ctx] = canvas(64, 64);
    ctx.fillStyle = '#8a8680';
    ctx.fillRect(0, 0, 64, 64);
    noise(ctx, 64, 64, 300, 0.15);
    return toTexture(c);
  }

  function asphalt() {
    const [c, ctx] = canvas(256, 256);
    ctx.fillStyle = '#41444a';
    ctx.fillRect(0, 0, 256, 256);
    noise(ctx, 256, 256, 6000, 0.12);
    return toTexture(c);
  }

  function sidewalk() {
    const [c, ctx] = canvas(128, 128);
    ctx.fillStyle = '#b4b0a8';
    ctx.fillRect(0, 0, 128, 128);
    ctx.strokeStyle = '#97938b';
    ctx.lineWidth = 2;
    for (let i = 0; i <= 128; i += 32) {
      ctx.beginPath(); ctx.moveTo(i, 0); ctx.lineTo(i, 128); ctx.stroke();
      ctx.beginPath(); ctx.moveTo(0, i); ctx.lineTo(128, i); ctx.stroke();
    }
    noise(ctx, 128, 128, 500, 0.08);
    return toTexture(c);
  }

  function cobble() {
    const [c, ctx] = canvas(128, 128);
    ctx.fillStyle = '#6f675e';
    ctx.fillRect(0, 0, 128, 128);
    for (let y = 0; y < 128; y += 12) {
      for (let x = (y / 12) % 2 ? -6 : 0; x < 128; x += 12) {
        const s = 0.8 + Math.random() * 0.4;
        ctx.fillStyle = `rgb(${150 * s | 0},${140 * s | 0},${128 * s | 0})`;
        ctx.beginPath();
        ctx.ellipse(x + 6, y + 6, 5, 5, 0, 0, Math.PI * 2);
        ctx.fill();
      }
    }
    return toTexture(c);
  }

  function grass() {
    const [c, ctx] = canvas(128, 128);
    ctx.fillStyle = '#4f8f3f';
    ctx.fillRect(0, 0, 128, 128);
    for (let i = 0; i < 1500; i++) {
      const s = 0.7 + Math.random() * 0.6;
      ctx.fillStyle = `rgb(${70 * s | 0},${140 * s | 0},${60 * s | 0})`;
      ctx.fillRect(Math.random() * 128, Math.random() * 128, 2, 3);
    }
    return toTexture(c);
  }

  function water() {
    const [c, ctx] = canvas(128, 128);
    ctx.fillStyle = '#3f8fb3';
    ctx.fillRect(0, 0, 128, 128);
    ctx.strokeStyle = 'rgba(255,255,255,0.25)';
    ctx.lineWidth = 2;
    for (let i = 0; i < 18; i++) {
      const x = Math.random() * 128, y = Math.random() * 128;
      ctx.beginPath(); ctx.moveTo(x, y); ctx.quadraticCurveTo(x + 6, y - 3, x + 12, y); ctx.stroke();
    }
    return toTexture(c);
  }

  // Letreros con texto (neón de Float Gardens, banderas de la Federación...)
  function sign(text, bg, fg, w, h, font) {
    const [c, ctx] = canvas(w || 512, h || 128);
    ctx.fillStyle = bg;
    ctx.fillRect(0, 0, c.width, c.height);
    ctx.fillStyle = fg;
    ctx.font = font || `900 ${c.height * 0.5}px system-ui, sans-serif`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.shadowColor = fg;
    ctx.shadowBlur = 12;
    ctx.fillText(text, c.width / 2, c.height / 2 + 4);
    const t = new THREE.CanvasTexture(c);
    t.anisotropy = 4;
    return t;
  }

  // Bogotá es famosa por su arte urbano: murales aleatorios para las paredes
  const MURAL_TEXT = ['CAFÉ LIBRE', 'FLOAT', 'BOGOTÁ', 'TINTO NO', '☕ 2600 m', 'HIDRO'];
  const MURAL_COLORS = ['#ff4f6d', '#ffd23f', '#2fbf71', '#47b5e0', '#9b5de5', '#ff8c42', '#00bbf9'];
  function mural(seed) {
    const [c, ctx] = canvas(256, 128);
    const pick = (i) => MURAL_COLORS[(seed * 7 + i * 3) % MURAL_COLORS.length];
    ctx.fillStyle = pick(0);
    ctx.fillRect(0, 0, 256, 128);
    for (let i = 0; i < 6; i++) {
      ctx.fillStyle = pick(i + 1);
      ctx.beginPath();
      ctx.arc(Math.random() * 256, Math.random() * 128, 20 + Math.random() * 50, 0, Math.PI * 2);
      ctx.fill();
    }
    // taza de café o colibrí estilizado
    ctx.fillStyle = '#fff';
    ctx.fillRect(170, 50, 46, 40);
    ctx.fillStyle = '#4a2c1d';
    ctx.fillRect(174, 54, 38, 10);
    ctx.strokeStyle = '#fff';
    ctx.lineWidth = 6;
    ctx.beginPath(); ctx.arc(220, 70, 10, -Math.PI / 2, Math.PI / 2); ctx.stroke();
    ctx.font = '900 38px system-ui, sans-serif';
    ctx.lineWidth = 8;
    ctx.strokeStyle = '#111';
    ctx.fillStyle = '#fff';
    const text = MURAL_TEXT[seed % MURAL_TEXT.length];
    ctx.save();
    ctx.translate(20, 80);
    ctx.rotate(-0.08);
    ctx.strokeText(text, 0, 0);
    ctx.fillText(text, 0, 0);
    ctx.restore();
    return new THREE.CanvasTexture(c);
  }

  function label(text, color, bg) {
    const [c, ctx] = canvas(256, 64);
    ctx.font = '800 30px system-ui, sans-serif';
    const w = Math.min(250, ctx.measureText(text).width + 24);
    ctx.fillStyle = bg || 'rgba(0,0,0,0.6)';
    ctx.beginPath();
    ctx.roundRect ? ctx.roundRect(128 - w / 2, 8, w, 48, 12) : ctx.rect(128 - w / 2, 8, w, 48);
    ctx.fill();
    ctx.fillStyle = color || '#fff';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(text, 128, 33);
    return new THREE.CanvasTexture(c);
  }

  W.Tex = {
    init() {
      this.brick = brick();
      this.stucco = stucco();
      this.glass = glass();
      this.concrete = concrete();
      this.warehouse = warehouse();
      this.roof = roof();
      this.asphalt = asphalt();
      this.sidewalk = sidewalk();
      this.cobble = cobble();
      this.grass = grass();
      this.water = water();
      this.murals = [0, 1, 2, 3, 4, 5].map(mural);
    },
    sign,
    label,
  };
})();
