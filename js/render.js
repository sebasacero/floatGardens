// Todo el dibujo del juego: sin imágenes externas, solo primitivas de canvas.
(function () {
  const C = FG.CONFIG;
  const COLORS = {
    green: '#2fbf71', darkGreen: '#0f3d2e', leaf: '#3fa35b', leafDark: '#2a7a41',
    water: '#47b5e0', waterDeep: '#2b8fc0', cherry: '#d6423a', cherryDark: '#8f1f1a',
    coffee: '#4a2c1d', cream: '#fff3dc', skin: '#e0ac7e', skinDark: '#a8714a',
    fedGrey: '#6d7078', fedDark: '#3a3c42', fedRed: '#c0392b',
  };

  function roundRect(ctx, x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.arcTo(x + w, y, x + w, y + h, r);
    ctx.arcTo(x + w, y + h, x, y + h, r);
    ctx.arcTo(x, y + h, x, y, r);
    ctx.arcTo(x, y, x + w, y, r);
    ctx.closePath();
  }

  function ellipse(ctx, x, y, rx, ry, fill) {
    ctx.beginPath();
    ctx.ellipse(x, y, rx, ry, 0, 0, Math.PI * 2);
    ctx.fillStyle = fill;
    ctx.fill();
  }

  function circle(ctx, x, y, r, fill) {
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    ctx.fillStyle = fill;
    ctx.fill();
  }

  function shadow(ctx, x, y, w) {
    ellipse(ctx, x, y, w, w * 0.35, 'rgba(0,0,0,0.22)');
  }

  // ---------- Fondo estático (se pinta una vez por nivel) ----------
  function buildBackground(city) {
    const cv = document.createElement('canvas');
    cv.width = C.WIDTH;
    cv.height = C.HEIGHT;
    const ctx = cv.getContext('2d');

    // Calle
    ctx.fillStyle = city.road;
    ctx.fillRect(0, 470, C.WIDTH, C.HEIGHT - 470);
    ctx.fillStyle = '#9aa0a6';
    ctx.fillRect(0, 452, C.WIDTH, 22);
    ctx.fillStyle = '#7d8389';
    for (let x = 0; x < C.WIDTH; x += 48) ctx.fillRect(x, 452, 2, 22);
    ctx.fillStyle = 'rgba(255,255,255,0.55)';
    for (let x = 10; x < C.WIDTH; x += 70) ctx.fillRect(x, 535, 38, 5);

    // Letrero de la ciudad en la acera
    roundRect(ctx, 20, 560, 250, 30, 8);
    ctx.fillStyle = 'rgba(0,0,0,0.45)';
    ctx.fill();
    ctx.fillStyle = city.accent;
    ctx.font = 'bold 15px system-ui, sans-serif';
    ctx.textBaseline = 'middle';
    ctx.fillText(`📍 ${city.name} — ${city.district}`, 32, 576);

    // Piso general
    ctx.fillStyle = '#e9efe4';
    ctx.fillRect(0, C.HUD_HEIGHT, C.WIDTH, 396);

    // Invernadero
    ctx.fillStyle = '#dcebd6';
    ctx.fillRect(20, 70, 430, 360);
    ctx.strokeStyle = 'rgba(80,140,100,0.35)';
    ctx.lineWidth = 1;
    for (let x = 20; x <= 450; x += 43) { ctx.beginPath(); ctx.moveTo(x, 70); ctx.lineTo(x, 430); ctx.stroke(); }
    for (let y = 70; y <= 430; y += 45) { ctx.beginPath(); ctx.moveTo(20, y); ctx.lineTo(450, y); ctx.stroke(); }
    ctx.strokeStyle = '#7aa88a';
    ctx.lineWidth = 4;
    ctx.strokeRect(20, 70, 430, 360);
    ctx.fillStyle = COLORS.darkGreen;
    roundRect(ctx, 150, 62, 170, 22, 6);
    ctx.fill();
    ctx.fillStyle = COLORS.cream;
    ctx.font = 'bold 12px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText('INVERNADERO FLOTANTE', 235, 73);
    ctx.textAlign = 'left';

    // Canales de agua (cultivo en balsas flotantes)
    for (const y of FG.CHANNEL_ROWS) {
      roundRect(ctx, 40, y - 26, 390, 52, 14);
      ctx.fillStyle = '#9fb7a8';
      ctx.fill();
      roundRect(ctx, 44, y - 22, 382, 44, 11);
      ctx.fillStyle = COLORS.water;
      ctx.fill();
    }
    // Tubería al tanque
    ctx.strokeStyle = '#8c9aa3';
    ctx.lineWidth = 6;
    ctx.beginPath();
    ctx.moveTo(430, 135); ctx.lineTo(462, 135); ctx.lineTo(462, 355); ctx.lineTo(430, 355);
    ctx.moveTo(430, 245); ctx.lineTo(470, 245);
    ctx.stroke();

    // Café: piso de madera
    ctx.fillStyle = city.floor;
    ctx.fillRect(540, 70, 410, 380);
    ctx.strokeStyle = 'rgba(0,0,0,0.12)';
    ctx.lineWidth = 1;
    for (let y = 70; y < 450; y += 18) { ctx.beginPath(); ctx.moveTo(540, y); ctx.lineTo(950, y); ctx.stroke(); }
    for (let y = 70, row = 0; y < 450; y += 18, row++) {
      for (let x = 540 + (row % 2) * 40; x < 950; x += 80) { ctx.beginPath(); ctx.moveTo(x, y); ctx.lineTo(x, y + 18); ctx.stroke(); }
    }
    // Pared superior con tablero de menú
    ctx.fillStyle = '#2d2a26';
    ctx.fillRect(540, 62, 410, 34);
    ctx.fillStyle = city.accent;
    ctx.fillRect(540, 94, 410, 4);
    ctx.fillStyle = COLORS.cream;
    ctx.font = 'bold 16px Georgia, serif';
    ctx.textAlign = 'center';
    ctx.fillText('FLOAT GARDENS · CAFÉ HIDROPÓNICO', 745, 80);
    ctx.textAlign = 'left';
    // Puerta
    ctx.fillStyle = '#5b3b26';
    ctx.fillRect(944, 262, 16, 90);
    ctx.fillStyle = city.accent;
    ctx.fillRect(944, 262, 4, 90);

    // Mesa de la AeroPress
    drawTable(ctx, FG.STATIONS.aeropress.x, FG.STATIONS.aeropress.y);

    // Barra
    const k = FG.STATIONS.counter;
    ctx.fillStyle = 'rgba(0,0,0,0.25)';
    ctx.fillRect(k.x + 4, k.y + 8, k.w, k.h);
    roundRect(ctx, k.x, k.y, k.w, k.h, 6);
    ctx.fillStyle = '#5a3a24';
    ctx.fill();
    roundRect(ctx, k.x, k.y, k.w, 10, 5);
    ctx.fillStyle = '#7d5434';
    ctx.fill();
    // Caja registradora y planta decorativa sobre la barra
    roundRect(ctx, 860, 210, 30, 18, 3);
    ctx.fillStyle = '#333';
    ctx.fill();
    ctx.fillStyle = city.accent;
    ctx.fillRect(864, 213, 22, 6);
    drawMiniPlant(ctx, 620, 222);

    // Mesas de clientes
    for (const [tx, ty] of [[600, 400], [700, 410], [800, 400]]) {
      shadow(ctx, tx, ty + 14, 26);
      circle(ctx, tx, ty, 22, '#e7d3b5');
      circle(ctx, tx, ty, 17, '#f3e6cf');
    }

    // Edificios al otro lado de la calle (siluetas)
    ctx.fillStyle = 'rgba(0,0,0,0.18)';
    for (let x = 320; x < C.WIDTH; x += 90) {
      const h = 22 + ((x * 37) % 26);
      ctx.fillRect(x, 600 - h, 70, h);
    }

    return cv;
  }

  function drawTable(ctx, x, y) {
    shadow(ctx, x, y + 26, 46);
    roundRect(ctx, x - 50, y - 18, 100, 40, 6);
    ctx.fillStyle = '#6b4a30';
    ctx.fill();
    roundRect(ctx, x - 50, y - 18, 100, 10, 5);
    ctx.fillStyle = '#8b6242';
    ctx.fill();
    // Molino
    roundRect(ctx, x - 40, y - 30, 16, 26, 3);
    ctx.fillStyle = '#c9c9c9';
    ctx.fill();
    ctx.fillStyle = '#555';
    ctx.fillRect(x - 36, y - 36, 8, 6);
    // Hervidor
    roundRect(ctx, x + 22, y - 26, 20, 22, 6);
    ctx.fillStyle = '#e0e0e0';
    ctx.fill();
    ctx.strokeStyle = '#999';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(x + 22, y - 18); ctx.quadraticCurveTo(x + 10, y - 22, x + 12, y - 30);
    ctx.stroke();
    // AeroPress grande
    drawAeroPress(ctx, x - 4, y - 8, 1.3);
  }

  function drawAeroPress(ctx, x, y, s) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(s, s);
    // taza base
    roundRect(ctx, -9, -2, 18, 10, 3);
    ctx.fillStyle = '#f5f5f5';
    ctx.fill();
    // cámara
    roundRect(ctx, -6, -24, 12, 23, 2);
    ctx.fillStyle = 'rgba(200,220,235,0.9)';
    ctx.fill();
    ctx.fillStyle = COLORS.coffee;
    ctx.fillRect(-5, -14, 10, 12);
    // émbolo
    roundRect(ctx, -5, -32, 10, 10, 2);
    ctx.fillStyle = '#444';
    ctx.fill();
    ctx.fillStyle = '#222';
    ctx.fillRect(-8, -34, 16, 3);
    ctx.restore();
  }

  function drawMiniPlant(ctx, x, y) {
    roundRect(ctx, x - 7, y - 12, 14, 12, 2);
    ctx.fillStyle = '#b86b3a';
    ctx.fill();
    for (let i = -1; i <= 1; i++) {
      ellipse(ctx, x + i * 6, y - 18 - Math.abs(i) * -3, 4, 8, COLORS.leaf);
    }
  }

  // ---------- Agua animada ----------
  function drawWater(ctx, t) {
    ctx.strokeStyle = 'rgba(255,255,255,0.35)';
    ctx.lineWidth = 2;
    for (const y of FG.CHANNEL_ROWS) {
      for (let i = 0; i < 6; i++) {
        const x = 60 + ((i * 67 + t * 18) % 350);
        const yy = y - 12 + ((i * 13) % 24);
        ctx.beginPath();
        ctx.moveTo(x, yy);
        ctx.quadraticCurveTo(x + 6, yy - 3, x + 12, yy);
        ctx.stroke();
      }
    }
  }

  // ---------- Plantas de café ----------
  function drawPlant(ctx, p, t, highlight) {
    const bob = Math.sin(t * 1.6 + p.sway) * 1.5;
    const x = p.x;
    const y = p.y + bob;

    // balsa flotante
    ellipse(ctx, x, y + 6, 30, 11, 'rgba(0,0,0,0.15)');
    ellipse(ctx, x, y + 3, 30, 11, '#f2f2ee');
    ellipse(ctx, x, y + 1, 12, 5, '#2d2d2d');

    if (highlight) {
      ctx.strokeStyle = 'rgba(255,255,255,0.9)';
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.ellipse(x, y + 3, 34, 14, 0, 0, Math.PI * 2);
      ctx.stroke();
    }

    if (p.withered > 0) {
      ctx.strokeStyle = '#7b5a3a';
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.moveTo(x, y); ctx.quadraticCurveTo(x - 4, y - 14, x - 14, y - 10);
      ctx.moveTo(x, y); ctx.quadraticCurveTo(x + 5, y - 16, x + 13, y - 8);
      ctx.stroke();
      // sello de decomiso
      ctx.save();
      ctx.translate(x, y - 16);
      ctx.rotate(-0.25);
      ctx.strokeStyle = COLORS.fedRed;
      ctx.lineWidth = 2;
      ctx.strokeRect(-26, -8, 52, 16);
      ctx.fillStyle = COLORS.fedRed;
      ctx.font = 'bold 9px system-ui, sans-serif';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText('DECOMISADO', 0, 1);
      ctx.restore();
      return;
    }

    const sway = Math.sin(t * 2 + p.sway) * 2;
    const size = [6, 12, 18, 21][p.stage];
    // tallo
    ctx.strokeStyle = COLORS.leafDark;
    ctx.lineWidth = 3;
    ctx.beginPath();
    ctx.moveTo(x, y);
    ctx.lineTo(x + sway * 0.5, y - size * 1.4);
    ctx.stroke();
    // hojas
    const leaves = p.stage === 1 ? 3 : 6;
    for (let i = 0; i < leaves; i++) {
      const a = (i / leaves) * Math.PI * 2 + p.sway;
      const lx = x + Math.cos(a) * size * 0.75 + sway;
      const ly = y - size * 1.1 + Math.sin(a) * size * 0.45;
      ctx.save();
      ctx.translate(lx, ly);
      ctx.rotate(a);
      ellipse(ctx, 0, 0, size * 0.55, size * 0.28, i % 2 ? COLORS.leaf : COLORS.leafDark);
      ctx.restore();
    }
    ellipse(ctx, x + sway, y - size * 1.25, size * 0.55, size * 0.4, COLORS.leaf);

    if (p.stage === 2) {
      for (let i = 0; i < 5; i++) {
        const a = i * 1.3 + p.sway;
        circle(ctx, x + sway + Math.cos(a) * size * 0.6, y - size * 1.2 + Math.sin(a) * size * 0.35, 3, '#fffaf0');
      }
    }
    if (p.stage === 3) {
      for (let i = 0; i < 7; i++) {
        const a = i * 0.9 + p.sway;
        const cx = x + sway + Math.cos(a) * size * 0.65;
        const cy = y - size * 1.15 + Math.sin(a) * size * 0.4;
        circle(ctx, cx, cy, 4.2, COLORS.cherry);
        circle(ctx, cx - 1.3, cy - 1.3, 1.3, 'rgba(255,255,255,0.7)');
      }
      // destello: "¡lista para farmear!"
      const s = (Math.sin(t * 5 + p.sway) + 1) / 2;
      ctx.fillStyle = `rgba(255,240,150,${0.4 + s * 0.6})`;
      star(ctx, x + 18, y - size * 1.9, 4 + s * 2);
    } else {
      // progreso de crecimiento
      const prog = p.growth / C.STAGE_TIME;
      ctx.strokeStyle = 'rgba(255,255,255,0.8)';
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.arc(x + 22, y - 4, 5, -Math.PI / 2, -Math.PI / 2 + prog * Math.PI * 2);
      ctx.stroke();
    }
  }

  // Balsa vacía: se desbloquea con la mejora "Torre hidropónica"
  function drawEmptySlot(ctx, s) {
    ellipse(ctx, s.x, s.y + 3, 30, 11, 'rgba(255,255,255,0.35)');
    ctx.fillStyle = 'rgba(15,61,46,0.55)';
    ctx.font = 'bold 18px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('+', s.x, s.y + 3);
    ctx.textAlign = 'left';
    ctx.textBaseline = 'alphabetic';
  }

  function star(ctx, x, y, r) {
    ctx.beginPath();
    for (let i = 0; i < 8; i++) {
      const rr = i % 2 ? r * 0.4 : r;
      const a = (i / 8) * Math.PI * 2 - Math.PI / 2;
      ctx.lineTo(x + Math.cos(a) * rr, y + Math.sin(a) * rr);
    }
    ctx.closePath();
    ctx.fill();
  }

  function drawTank(ctx, level, t, highlight) {
    const { x, y } = FG.STATIONS.tank;
    shadow(ctx, x, y + 46, 26);
    roundRect(ctx, x - 22, y - 44, 44, 88, 10);
    ctx.fillStyle = '#d9e3e8';
    ctx.fill();
    ctx.strokeStyle = highlight ? '#fff' : '#8c9aa3';
    ctx.lineWidth = highlight ? 3 : 2;
    ctx.stroke();
    const h = 76 * (level / 100);
    const color = level < 25 ? '#e0a03a' : '#3fbf8f';
    roundRect(ctx, x - 16, y + 38 - h, 32, h, 6);
    ctx.fillStyle = color;
    ctx.fill();
    // burbujas
    ctx.fillStyle = 'rgba(255,255,255,0.6)';
    for (let i = 0; i < 3; i++) {
      const by = y + 34 - ((t * 25 + i * 26) % Math.max(h, 1));
      if (h > 6) circle(ctx, x - 6 + i * 6, by, 2, 'rgba(255,255,255,0.6)');
    }
    ctx.fillStyle = COLORS.darkGreen;
    ctx.font = 'bold 9px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText('NUTRI', x, y - 50);
    ctx.textAlign = 'left';
    if (level <= 0) {
      ctx.fillStyle = COLORS.fedRed;
      ctx.font = 'bold 12px system-ui, sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText('¡VACÍO!', x, y + 62 + Math.sin(t * 8) * 2);
      ctx.textAlign = 'left';
    }
  }

  // ---------- Personajes ----------
  function drawPerson(ctx, x, y, o) {
    const s = o.scale || 1;
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(s, s);
    const step = o.moving ? Math.sin(o.walk) * 3 : 0;

    shadow(ctx, 0, 0, 13);
    // piernas
    ctx.fillStyle = o.pants;
    ctx.fillRect(-7, -14 + Math.max(0, step), 6, 14 - Math.max(0, step));
    ctx.fillRect(1, -14 + Math.max(0, -step), 6, 14 - Math.max(0, -step));
    // cuerpo
    roundRect(ctx, -11, -33, 22, 21, 6);
    ctx.fillStyle = o.shirt;
    ctx.fill();
    if (o.apron) {
      roundRect(ctx, -8, -28, 16, 18, 3);
      ctx.fillStyle = o.apron;
      ctx.fill();
      // hoja del logo
      ellipse(ctx, 0, -20, 3.5, 2, '#bff0cf');
    }
    if (o.tie) {
      ctx.fillStyle = o.tie;
      ctx.beginPath();
      ctx.moveTo(-2, -32); ctx.lineTo(2, -32); ctx.lineTo(3, -20); ctx.lineTo(0, -17); ctx.lineTo(-3, -20);
      ctx.fill();
    }
    if (o.sash) {
      ctx.strokeStyle = o.sash;
      ctx.lineWidth = 4;
      ctx.beginPath();
      ctx.moveTo(-10, -32); ctx.lineTo(10, -14);
      ctx.stroke();
    }
    if (o.badge) {
      circle(ctx, 6, -27, 3.5, o.badge);
    }
    // brazos
    ctx.fillStyle = o.shirt;
    ctx.fillRect(-15, -31 - step * 0.5, 5, 14);
    ctx.fillRect(10, -31 + step * 0.5, 5, 14);
    circle(ctx, -12.5, -16 - step * 0.5, 3, o.skin || COLORS.skin);
    circle(ctx, 12.5, -16 + step * 0.5, 3, o.skin || COLORS.skin);
    // cabeza
    circle(ctx, 0, -41, 10, o.skin || COLORS.skin);
    // pelo
    if (o.hair) {
      ctx.fillStyle = o.hair;
      ctx.beginPath();
      ctx.arc(0, -43, 10, Math.PI, 0);
      ctx.fill();
    }
    if (o.beard) {
      ctx.fillStyle = o.hair;
      ctx.beginPath();
      ctx.arc(0, -38, 8, 0.1, Math.PI - 0.1);
      ctx.fill();
    }
    // cara
    if (o.facing !== 'up') {
      const ex = o.facing === 'left' ? -3 : o.facing === 'right' ? 3 : 0;
      circle(ctx, -3.5 + ex, -41, 1.5, '#222');
      circle(ctx, 3.5 + ex, -41, 1.5, '#222');
      if (o.angry) {
        ctx.strokeStyle = '#222';
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.moveTo(-6 + ex, -46); ctx.lineTo(-2 + ex, -44);
        ctx.moveTo(6 + ex, -46); ctx.lineTo(2 + ex, -44);
        ctx.stroke();
      }
      if (o.mustache) {
        ctx.fillStyle = '#2a1a10';
        ellipse(ctx, -3 + ex, -36.5, 4, 1.8, '#2a1a10');
        ellipse(ctx, 3 + ex, -36.5, 4, 1.8, '#2a1a10');
      }
    }
    // sombreros
    if (o.cap) {
      ctx.fillStyle = o.cap;
      ctx.beginPath();
      ctx.arc(0, -46, 9.5, Math.PI, 0);
      ctx.fill();
      ctx.fillRect(o.facing === 'left' ? -15 : 3, -47, 12, 3);
    }
    if (o.fedora) {
      ellipse(ctx, 0, -48, 15, 4, o.fedora);
      roundRect(ctx, -9, -58, 18, 11, 4);
      ctx.fillStyle = o.fedora;
      ctx.fill();
      ctx.fillStyle = COLORS.fedRed;
      ctx.fillRect(-9, -50, 18, 2.5);
    }
    if (o.tophat) {
      ellipse(ctx, 0, -49, 15, 4, '#111');
      ctx.fillStyle = '#111';
      ctx.fillRect(-9, -70, 18, 21);
      ctx.fillStyle = COLORS.fedRed;
      ctx.fillRect(-9, -54, 18, 3);
    }
    if (o.sunhat) {
      ellipse(ctx, 0, -47, 16, 5, o.sunhat);
      ctx.fillStyle = o.sunhat;
      ctx.beginPath();
      ctx.arc(0, -48, 8, Math.PI, 0);
      ctx.fill();
    }
    ctx.restore();
  }

  function drawPlayer(ctx, pl, t, level) {
    const blink = pl.invuln > 0 && Math.floor(t * 12) % 2 === 0;
    if (blink) ctx.globalAlpha = 0.45;
    drawPerson(ctx, pl.x, pl.y, {
      shirt: '#f5f1e8', pants: '#33302c', apron: COLORS.darkGreen, cap: COLORS.green,
      hair: '#2a1a10', facing: pl.facing, moving: pl.moving, walk: pl.walk,
    });
    // AeroPress en la mano
    const hx = pl.facing === 'left' ? pl.x - 15 : pl.x + 15;
    drawAeroPress(ctx, hx, pl.y - 12, 0.55);
    ctx.globalAlpha = 1;

    if (pl.stun > 0) {
      ctx.fillStyle = '#ffd34d';
      for (let i = 0; i < 3; i++) {
        const a = t * 6 + (i * Math.PI * 2) / 3;
        star(ctx, pl.x + Math.cos(a) * 14, pl.y - 60 + Math.sin(a) * 4, 4);
      }
    }
    // Tazas que lleva el barista
    if (level.cups > 0) {
      for (let i = 0; i < level.cups; i++) drawCupIcon(ctx, pl.x - (level.cups - 1) * 5 + i * 10, pl.y - 66, 0.6);
    }
  }

  function drawCustomer(ctx, c, t) {
    const ty = c.type;
    drawPerson(ctx, c.x, c.y, {
      shirt: ty.shirt, pants: ty.pants, hair: ty.hair, tie: ty.tie, beard: ty.beard, sunhat: ty.hat,
      facing: c.state === 'leaving' ? 'right' : (c.state === 'walking' ? 'left' : 'up'),
      moving: c.state !== 'waiting', walk: c.walk, angry: c.mood === 'angry',
    });
    if (c.state === 'waiting') {
      const p = Math.max(0, c.patience / c.maxPatience);
      const bx = c.x, by = c.y - 72;
      circle(ctx, bx, by, 13, 'rgba(255,255,255,0.95)');
      ctx.strokeStyle = p > 0.5 ? COLORS.green : p > 0.25 ? '#f2a541' : COLORS.cherry;
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.arc(bx, by, 13, -Math.PI / 2, -Math.PI / 2 + p * Math.PI * 2);
      ctx.stroke();
      drawCupIcon(ctx, bx, by + 1, 0.55);
    }
    if (c.mood === 'happy' && c.state === 'leaving') {
      ctx.fillStyle = '#e85d75';
      heart(ctx, c.x, c.y - 66, 6);
    }
    if (c.mood === 'angry' && c.state === 'leaving') {
      ctx.fillStyle = COLORS.cherry;
      ctx.font = 'bold 16px system-ui, sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText('💢', c.x, c.y - 62);
      ctx.textAlign = 'left';
    }
  }

  function heart(ctx, x, y, s) {
    ctx.beginPath();
    ctx.moveTo(x, y + s);
    ctx.bezierCurveTo(x - s * 2, y - s * 0.5, x - s * 0.6, y - s * 1.8, x, y - s * 0.6);
    ctx.bezierCurveTo(x + s * 0.6, y - s * 1.8, x + s * 2, y - s * 0.5, x, y + s);
    ctx.fill();
  }

  function drawInspector(ctx, e, t) {
    if (e.hitFlash > 0) ctx.globalAlpha = 0.6;
    drawPerson(ctx, e.x, e.y, {
      shirt: COLORS.fedGrey, pants: COLORS.fedDark, fedora: '#2b2b30', tie: COLORS.fedRed,
      badge: '#e6c34a', facing: e.facing, moving: true, walk: e.walk, angry: true, mustache: e.mustache,
    });
    ctx.globalAlpha = 1;
    // portapapeles
    const cx = e.facing === 'left' ? e.x - 17 : e.x + 13;
    ctx.fillStyle = '#9a6b3f';
    ctx.fillRect(cx, e.y - 30, 9, 12);
    ctx.fillStyle = '#fff';
    ctx.fillRect(cx + 1.5, e.y - 28, 6, 8);
    // vida
    hpBar(ctx, e.x, e.y - 64, 26, e.hp / e.maxHp);
    if (e.say > 0) bubble(ctx, e.x, e.y - 74, e.line);
  }

  function drawBoss(ctx, b, t, name) {
    if (b.hitFlash > 0) ctx.globalAlpha = 0.6;
    drawPerson(ctx, b.x, b.y, {
      scale: 1.6, shirt: '#2f3136', pants: '#18191c', tophat: true, sash: COLORS.fedRed,
      badge: '#e6c34a', facing: b.facing, moving: true, walk: b.walk, angry: true, mustache: true,
    });
    ctx.globalAlpha = 1;
    ctx.fillStyle = '#fff';
    ctx.font = 'bold 13px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.lineWidth = 3;
    ctx.strokeStyle = 'rgba(0,0,0,0.7)';
    ctx.strokeText(name, b.x, b.y - 128);
    ctx.fillText(name, b.x, b.y - 128);
    ctx.textAlign = 'left';
    hpBar(ctx, b.x, b.y - 118, 70, b.hp / b.maxHp);
    if (b.say > 0) bubble(ctx, b.x, b.y - 140, b.line);
  }

  function hpBar(ctx, x, y, w, frac) {
    ctx.fillStyle = 'rgba(0,0,0,0.5)';
    ctx.fillRect(x - w / 2 - 1, y - 1, w + 2, 6);
    ctx.fillStyle = frac > 0.5 ? '#e74c3c' : '#ff8c42';
    ctx.fillRect(x - w / 2, y, w * Math.max(0, frac), 4);
  }

  function bubble(ctx, x, y, text) {
    ctx.font = 'bold 11px system-ui, sans-serif';
    const w = ctx.measureText(text).width + 14;
    const bx = Math.min(Math.max(x - w / 2, 4), C.WIDTH - w - 4);
    roundRect(ctx, bx, y - 18, w, 22, 8);
    ctx.fillStyle = 'rgba(255,255,255,0.96)';
    ctx.fill();
    ctx.strokeStyle = COLORS.fedRed;
    ctx.lineWidth = 1.5;
    ctx.stroke();
    ctx.fillStyle = '#222';
    ctx.textBaseline = 'middle';
    ctx.fillText(text, bx + 7, y - 7);
    ctx.textBaseline = 'alphabetic';
  }

  function drawProjectile(ctx, p) {
    ctx.save();
    ctx.translate(p.x, p.y);
    ctx.rotate(Math.atan2(p.vy, p.vx));
    ellipse(ctx, -10, 0, 10, 2.5, 'rgba(120,70,40,0.35)');
    ellipse(ctx, 0, 0, 7, 4.5, '#6b3b1f');
    ellipse(ctx, 1.5, -1.5, 2.5, 1.5, 'rgba(255,230,200,0.7)');
    ctx.restore();
  }

  function drawStamp(ctx, s, t) {
    ctx.save();
    ctx.translate(s.x, s.y);
    ctx.rotate(t * 8);
    circle(ctx, 0, 0, 10, COLORS.fedRed);
    circle(ctx, 0, 0, 7, '#f7d7d3');
    ctx.fillStyle = COLORS.fedRed;
    ctx.font = 'bold 8px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('ARC', 0, 0.5);
    ctx.restore();
  }

  // ---------- Iconos del HUD ----------
  function drawCherryIcon(ctx, x, y, s) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(s, s);
    ctx.strokeStyle = COLORS.leafDark;
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(-4, 2); ctx.quadraticCurveTo(-2, -8, 3, -10);
    ctx.moveTo(5, 3); ctx.quadraticCurveTo(4, -6, 3, -10);
    ctx.stroke();
    circle(ctx, -4, 4, 5, COLORS.cherry);
    circle(ctx, 5, 5, 5, COLORS.cherry);
    circle(ctx, -5.5, 2.5, 1.5, 'rgba(255,255,255,0.7)');
    ellipse(ctx, 7, -9, 5, 2.5, COLORS.leaf);
    ctx.restore();
  }

  function drawCupIcon(ctx, x, y, s) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(s, s);
    ctx.strokeStyle = '#fff';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(9, 1, 4, -Math.PI / 2, Math.PI / 2);
    ctx.stroke();
    roundRect(ctx, -8, -7, 16, 15, 4);
    ctx.fillStyle = '#fff';
    ctx.fill();
    ctx.fillStyle = COLORS.coffee;
    ctx.fillRect(-6, -5, 12, 3);
    ctx.fillStyle = COLORS.green;
    ctx.fillRect(-8, 0, 16, 3);
    ctx.restore();
  }

  function drawDropIcon(ctx, x, y, s, color) {
    ctx.save();
    ctx.translate(x, y);
    ctx.scale(s, s);
    ctx.beginPath();
    ctx.moveTo(0, -10);
    ctx.quadraticCurveTo(8, 0, 6, 5);
    ctx.arc(0, 4, 6, 0.2, Math.PI - 0.2);
    ctx.quadraticCurveTo(-8, 0, 0, -10);
    ctx.fillStyle = color || COLORS.water;
    ctx.fill();
    ctx.restore();
  }

  function formatMoney(n) {
    return '$' + Math.round(n).toLocaleString('es-CO');
  }

  function drawHUD(ctx, L) {
    const W = C.WIDTH;
    ctx.fillStyle = 'rgba(15,40,30,0.94)';
    ctx.fillRect(0, 0, W, C.HUD_HEIGHT);
    ctx.fillStyle = L.city.accent;
    ctx.fillRect(0, C.HUD_HEIGHT - 3, W, 3);

    ctx.textBaseline = 'middle';
    // Ciudad y tiempo
    ctx.fillStyle = COLORS.cream;
    ctx.font = 'bold 15px system-ui, sans-serif';
    ctx.fillText(L.city.name, 14, 18);
    ctx.font = '12px system-ui, sans-serif';
    ctx.fillStyle = 'rgba(255,243,220,0.75)';
    ctx.fillText(L.city.district, 14, 37);
    const secs = Math.max(0, Math.ceil(L.time));
    const mm = Math.floor(secs / 60);
    const ss = String(secs % 60).padStart(2, '0');
    ctx.font = 'bold 22px ui-monospace, monospace';
    ctx.fillStyle = L.time < 30 ? '#ff7b6b' : COLORS.cream;
    ctx.fillText(`${mm}:${ss}`, 170, 28);

    // Barra de conquista
    const bx = 260, bw = 300, by = 20, bh = 18;
    ctx.font = 'bold 10px system-ui, sans-serif';
    ctx.fillStyle = COLORS.green;
    ctx.fillText('FLOAT GARDENS', bx, 11);
    ctx.textAlign = 'right';
    ctx.fillStyle = '#ff8a7a';
    ctx.fillText('ARCADIA BEAN CORP.', bx + bw, 11);
    ctx.textAlign = 'left';
    roundRect(ctx, bx, by, bw, bh, 9);
    ctx.fillStyle = COLORS.fedRed;
    ctx.fill();
    ctx.save();
    roundRect(ctx, bx, by, bw, bh, 9);
    ctx.clip();
    ctx.fillStyle = COLORS.green;
    ctx.fillRect(bx, by, bw * (L.conquest / 100), bh);
    ctx.restore();
    // marca del jefe
    const mx = bx + bw * (C.BOSS_AT / 100);
    ctx.fillStyle = 'rgba(255,255,255,0.8)';
    ctx.fillRect(mx - 1, by - 2, 2, bh + 4);
    ctx.fillStyle = '#fff';
    ctx.font = 'bold 12px system-ui, sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText(`Conquista ${Math.floor(L.conquest)}%`, bx + bw / 2, by + bh / 2 + 1);
    ctx.font = '10px system-ui, sans-serif';
    ctx.fillStyle = 'rgba(255,243,220,0.75)';
    const hint = L.bossDefeated ? '¡Jefe derrotado! Llega al 100%'
      : L.boss ? `¡Derrota a ${L.city.boss}!` : `El jefe aparece al ${C.BOSS_AT}%`;
    ctx.fillText(hint, bx + bw / 2, by + bh + 10);
    ctx.textAlign = 'left';

    // Inventario
    let x = 585;
    drawCherryIcon(ctx, x, 28, 1);
    ctx.fillStyle = COLORS.cream;
    ctx.font = 'bold 15px system-ui, sans-serif';
    ctx.fillText(String(L.cherries), x + 13, 28);
    x += 50;
    drawCupIcon(ctx, x, 28, 0.9);
    ctx.fillStyle = COLORS.cream;
    ctx.fillText(`${L.cups}/${C.MAX_CUPS}`, x + 18, 28);
    x += 60;
    drawDropIcon(ctx, x, 28, 1, L.nutrients < 25 ? '#e0a03a' : '#3fbf8f');
    ctx.fillStyle = L.nutrients < 25 ? '#ffb74d' : COLORS.cream;
    ctx.fillText(`${Math.ceil(L.nutrients)}%`, x + 11, 28);
    x += 60;
    ctx.fillStyle = '#ffe08a';
    ctx.fillText(formatMoney(L.game.save.money), x, 18);
    ctx.fillStyle = '#9fe8bf';
    ctx.font = 'bold 13px system-ui, sans-serif';
    ctx.fillText(`★ ${L.game.save.puntos} Puntos Float`, x, 38);
    ctx.textBaseline = 'alphabetic';
  }

  function drawPrompt(ctx, x, y, text) {
    ctx.font = 'bold 12px system-ui, sans-serif';
    const w = ctx.measureText(text).width + 16;
    roundRect(ctx, x - w / 2, y - 12, w, 22, 11);
    ctx.fillStyle = 'rgba(15,61,46,0.92)';
    ctx.fill();
    ctx.fillStyle = COLORS.cream;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(text, x, y);
    ctx.textAlign = 'left';
    ctx.textBaseline = 'alphabetic';
  }

  function drawProgress(ctx, x, y, frac) {
    roundRect(ctx, x - 24, y, 48, 8, 4);
    ctx.fillStyle = 'rgba(0,0,0,0.5)';
    ctx.fill();
    roundRect(ctx, x - 23, y + 1, 46 * frac, 6, 3);
    ctx.fillStyle = COLORS.green;
    ctx.fill();
  }

  function drawBanner(ctx, text, alpha, color) {
    ctx.save();
    ctx.globalAlpha = Math.min(1, alpha);
    ctx.font = 'bold 22px system-ui, sans-serif';
    const w = Math.min(C.WIDTH - 40, ctx.measureText(text).width + 50);
    roundRect(ctx, C.WIDTH / 2 - w / 2, 78, w, 44, 22);
    ctx.fillStyle = color || 'rgba(15,61,46,0.92)';
    ctx.fill();
    ctx.fillStyle = COLORS.cream;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(text, C.WIDTH / 2, 101);
    ctx.restore();
  }

  function drawFloatText(ctx, ft) {
    ctx.save();
    ctx.globalAlpha = Math.max(0, Math.min(1, ft.life / 0.4));
    ctx.font = `bold ${ft.size || 14}px system-ui, sans-serif`;
    ctx.textAlign = 'center';
    ctx.lineWidth = 3;
    ctx.strokeStyle = 'rgba(0,0,0,0.55)';
    ctx.strokeText(ft.text, ft.x, ft.y);
    ctx.fillStyle = ft.color;
    ctx.fillText(ft.text, ft.x, ft.y);
    ctx.restore();
  }

  FG.Render = {
    COLORS, roundRect, circle, ellipse, buildBackground, drawWater, drawPlant, drawEmptySlot, drawTank,
    drawPlayer, drawCustomer, drawInspector, drawBoss, drawProjectile, drawStamp, drawHUD,
    drawPrompt, drawProgress, drawBanner, drawFloatText, drawCherryIcon, drawCupIcon, formatMoney,
  };
})();
