// El nivel: una cuadra larga de Kennedy vista de lado. Cada entrada es una casa (C) o un hueco (H).
// Las alturas son la placa de la terraza en metros. Las cosas del nivel (cerezas, palomas, tanques...)
// se ubican con x relativo al inicio de la casa o del hueco.
//
// Capacidad del jugador (ver FISICA en juego.js): salto sostenido ≈ 4 m de alto, ≈ 8 m de largo corriendo,
// ≈ 12 m rodando y saltando. Ningún hueco obligatorio pasa de 6.5 m ni ninguna subida de 3.6 m sin ayuda.
(function () {
  const FG = window.FGT;

  // tipos de hueco: 'calle' (sigue hacia el fondo), 'avenida' (con TransMilenio), 'lote' (lote vacío, la cuadra sigue)
  const L = [
    // ---- A. Tu terraza: aprende a correr y saltar ----
    ['C', 20, 8.1, { inicio: true, cp: 5, cultivo: true, cerezas: [['linea', 9, 1, 8]], pista: [[2, 'Corre con ← → (o A / D) · salta con Espacio']] }],
    ['C', 8, 9.4, { cerezas: [['linea', 2, 1, 4]] }],
    ['C', 9, 8.6, {}],
    ['H', 4.5, 'calle', { cerezas: [['arco', 0, 9.6, 4.5]] }],
    ['C', 12, 8.6, { palomas: [[4, 10]], pista: [[0, 'Cae encima de las palomas para espantarlas']] }],
    ['H', 5, 'calle', { cerezas: [['arco', 0, 9.8, 5]] }],
    ['C', 7, 10.2, {}],
    ['C', 10, 11.4, { cerezas: [['linea', 2, 1, 5]] }],
    ['H', 5.5, 'calle', { cerezas: [['arco', 0, 11.6, 5.5]] }],
    ['C', 9, 9.0, {}],
    ['C', 11, 9.6, { colchon: [8.6], pista: [[3, 'Rebota en el colchón para subir']] }],
    ['C', 9, 14.4, { letras: [[5, 2.2, 'F']], cerezas: [['linea', 1.5, 1, 4]] }],
    ['C', 8, 12.0, {}],
    ['H', 6, 'calle', { cerezas: [['arco', 0, 12.4, 6]], pista: [[-6, 'Shift (o X) para rodar: rueda y salta para llegar más lejos']] }],
    ['C', 14, 10.8, { cp: 8, palomas: [[1.5, 6], [8.5, 13]] }],

    // ---- B. Avenida de las Américas: tanques-cañón sobre el TransMilenio ----
    ['C', 8, 10.8, { tanque: [[5.6, 'sig', 0.7]], pista: [[0, 'Métete al tanque y salta para dispararte']], cerezas: [['linea', 0.8, 1, 3]] }],
    ['H', 34, 'avenida', { tanqueValla: [17, 15.2], letras: [[9, 15.6, 'L']], cerezas: [['linea', 22, 15.8, 7, 0.9]] }],
    ['C', 12, 9.5, { cp: 8 }],

    // ---- C. Obra negra: andamios que suben y bajan ----
    ['C', 8, 9.5, { drones: [[4, 13.5, 3, 0.6]] }],
    ['H', 5, 'calle', { cerezas: [['arco', 0, 9.8, 5]] }],
    ['C', 10, 8.2, { cerezas: [['linea', 2, 1, 5]] }],
    ['H', 7, 'lote', { andamios: [[0.6, 2.6, 7.6, 14.2, 4.2, 0], [3.9, 2.6, 13.4, 20.6, 4.2, 0.5]], pista: [[-4, 'Súbete a los andamios']] }],
    ['C', 14, 21.6, { obra: true, letras: [[11.5, 2.6, 'O']], drones: [[7, 25, 4.5, 0.5]], cerezas: [['linea', 1, 1, 6]] }],
    ['H', 4, 'calle', {}],
    ['C', 9, 17.0, {}],
    ['C', 9, 13.5, { cerezas: [['linea', 1, 1, 6]] }],
    ['H', 6, 'calle', { cerezas: [['arco', 0, 13.8, 6]] }],
    ['C', 10, 11.0, { cp: 5 }],

    // ---- D. Lote con tejas de Eternit que se caen ----
    ['C', 7, 11.0, { pista: [[0, 'Las tejas de Eternit no aguantan: ¡no te quedes quieto!']] }],
    ['H', 15, 'lote', { tejas: [[1.3, 10.6, 2.8], [6.1, 11.2, 2.8], [10.9, 10.6, 2.8]], cerezas: [['linea', 1.6, 11.6, 3, 0.8], ['linea', 6.4, 12.2, 3, 0.8], ['linea', 11.2, 11.6, 3, 0.8]] }],
    ['C', 10, 11.5, { escalon: [[8, 1.5]] }],
    ['C', 12, 13.5, { palomas: [[1, 6], [7, 11.5]] }],
    ['H', 7, 'calle', { letras: [[3.5, 17.6, 'A']], cerezas: [['arco', 0, 13.8, 7]] }],
    ['C', 9, 11.8, { cp: 5 }],

    // ---- E. ¡La tormenta de Arcadia! Corre hasta la torre ----
    ['C', 10, 11.8, { tormenta: 0, pista: [[0, '¡Se viene la tormenta de Arcadia! ¡Corre a la torre!']] }],
    ['C', 8, 12.9, { cerezas: [['linea', 1, 1, 4]] }],
    ['H', 5, 'calle', { cerezas: [['arco', 0, 13.2, 5]] }],
    ['C', 9, 10.6, { drones: [[4.5, 14.2, 3, 0.9]] }],
    ['H', 5.5, 'calle', { cerezas: [['arco', 0, 10.9, 5.5]] }],
    ['C', 8, 12.0, {}],
    ['C', 7, 13.3, {}],
    ['H', 4.5, 'calle', {}],
    ['C', 10, 11.8, { tanque: [[8, 'sig', 0.75]] }],
    ['H', 22, 'avenida', { letras: [[11, 17.0, 'T']], cerezas: [['linea', 4, 15.2, 6, 1.2]] }],
    ['C', 10, 10.4, {}],
    ['H', 5, 'calle', { cerezas: [['arco', 0, 10.7, 5]] }],
    ['C', 9, 11.6, {}],
    ['C', 10, 10.6, { palomas: [[1.5, 8.5]] }],
    ['H', 6, 'calle', { cerezas: [['arco', 0, 10.9, 6]] }],
    ['C', 12, 10.6, { escalon: [[10.3, 1.6]], finTormenta: 12 }],

    // ---- F. La torre de Float Gardens ----
    ['H', 4.5, 'calle', {}],
    ['C', 28, 14.0, { meta: 18, torre: true, cerezas: [['linea', 2, 1, 8]] }],
    ['C', 10, 10.8, { final: true }],
  ];

  FG.FLOOR = 2.7;
  FG.ZF = 2.6;   // fachada de la hilera jugable (hacia la cámara)
  FG.ZB = -6;    // fondo de las casas jugables

  // Recorre la lista y devuelve todo con coordenadas absolutas.
  FG.buildLevel = function () {
    const out = { casas: [], huecos: [], solidos: [], ents: [], pistas: [], spawn: null, cps: [], fin: 0 };
    let x = 0;
    for (const e of L) {
      const opt = (e[0] === 'C' ? e[3] : e[3]) || {};
      if (e[0] === 'C') {
        const [, w, h] = e;
        const casa = { x0: x, x1: x + w, h, opt };
        out.casas.push(casa);
        out.solidos.push({ x0: x, x1: x + w, y0: -50, y1: h, tipo: 'casa' });
        if (opt.inicio) out.spawn = { x: x + 2.5, y: h };
        if (opt.cp != null) out.ents.push({ t: 'cp', x: x + opt.cp, y: h, i: out.cps.length }), out.cps.push(x + opt.cp);
        if (opt.meta != null) out.ents.push({ t: 'meta', x: x + opt.meta, y: h });
        if (opt.tormenta != null) out.tormenta = { x0: x + opt.tormenta };
        if (opt.finTormenta != null) out.tormenta.x1 = x + opt.finTormenta;
        for (const [rx, hh] of opt.escalon || []) {
          out.solidos.push({ x0: x + rx - 1.2, x1: x + rx + 1.2, y0: h, y1: h + hh, tipo: 'escalon' });
          out.ents.push({ t: 'escalon', x: x + rx, y: h, h: hh });
        }
        for (const rx of opt.colchon || []) out.ents.push({ t: 'colchon', x: x + rx, y: h });
        for (const [rx, tgt, ang] of opt.tanque || []) out.ents.push({ t: 'tanque', x: x + rx, y: h + 1.1, tgt, ang });
        for (const [a, b] of opt.palomas || []) out.ents.push({ t: 'paloma', x0: x + a, x1: x + b, y: h });
        addCommon(opt, x, h);
      } else {
        const [, w, tipo] = e;
        out.huecos.push({ x0: x, x1: x + w, tipo, opt });
        for (const [rx, y] of opt.tanqueValla ? [opt.tanqueValla] : []) out.ents.push({ t: 'tanque', x: x + rx, y, tgt: 'sig', ang: 0.35, valla: true });
        for (const [rx, w2, y0, y1, per, ph] of opt.andamios || []) out.ents.push({ t: 'andamio', x0: x + rx, x1: x + rx + w2, y0, y1, per, ph });
        for (const [rx, y, w2] of opt.tejas || []) out.ents.push({ t: 'teja', x0: x + rx, x1: x + rx + w2, y });
        addCommon(opt, x, 0);
      }
      x += e[1];
    }
    out.fin = x;

    function addCommon(opt, x, h) {
      for (const [rx, txt] of opt.pista || []) out.pistas.push({ x: x + rx, txt });
      for (const [rx, ry, ch] of opt.letras || []) out.ents.push({ t: 'letra', x: x + rx, y: h + ry, ch });
      for (const [rx, ry, wx, hy] of opt.drones || []) out.ents.push({ t: 'dron', x: x + rx, y: ry, ax: wx, sp: hy });
      for (const c of opt.cerezas || []) {
        if (c[0] === 'linea') { // ['linea', rx, dy (sobre la placa) o y absoluto en huecos, n, paso]
          const [, rx, dy, n, paso = 1] = c;
          for (let i = 0; i < n; i++) out.ents.push({ t: 'cereza', x: x + rx + i * paso, y: (h ? h : 0) + dy });
        } else { // ['arco', rx, y base, ancho del hueco]: un arco sobre el hueco como trayectoria del salto
          const [, rx, y0, w] = c, n = Math.max(4, Math.round(w / 1.1));
          for (let i = 0; i <= n; i++) {
            const t = i / n;
            out.ents.push({ t: 'cereza', x: x + rx + t * w, y: y0 + 1 + Math.sin(t * Math.PI) * 2.4 });
          }
        }
      }
    }

    // Los tanques disparan hacia el siguiente tanque o hacia la siguiente casa.
    const tanques = out.ents.filter((e) => e.t === 'tanque').sort((a, b) => a.x - b.x);
    for (const tq of tanques) {
      const next = tanques.find((o) => o.x > tq.x && o.x - tq.x < 30);
      if (next && !tq.valla) { tq.tx = next.x; tq.ty = next.y; tq.chain = true; continue; }
      const casa = out.casas.find((c) => c.x0 > tq.x + 2);
      tq.tx = casa.x0 + 3.2; tq.ty = casa.h + 0.1;
    }
    return out;
  };
})();
