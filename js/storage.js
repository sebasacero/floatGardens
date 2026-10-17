// Guardado local del progreso y de los cupones de café gratis.
(function () {
  const KEY = 'floatgardens_save_v1';
  const ALPHABET = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';

  function defaults() {
    return {
      money: 0,
      puntos: 0,          // saldo canjeable
      puntosTotal: 0,     // histórico
      conquered: [],      // índices de ciudades conquistadas
      upgrades: {},
      vouchers: [],
      seenIntro: false,
      stats: { served: 0, harvested: 0, repelled: 0 },
    };
  }

  FG.Save = {
    load() {
      try {
        const raw = localStorage.getItem(KEY);
        if (!raw) return defaults();
        const data = JSON.parse(raw);
        const base = defaults();
        return Object.assign(base, data, { stats: Object.assign(base.stats, data.stats || {}) });
      } catch (e) {
        return defaults();
      }
    },
    write(data) {
      try { localStorage.setItem(KEY, JSON.stringify(data)); } catch (e) { /* modo privado */ }
    },
    reset() {
      try { localStorage.removeItem(KEY); } catch (e) { /* noop */ }
      return defaults();
    },
  };

  function randomChars(n) {
    const out = [];
    const buf = new Uint32Array(n);
    if (window.crypto && crypto.getRandomValues) crypto.getRandomValues(buf);
    else for (let i = 0; i < n; i++) buf[i] = Math.floor(Math.random() * 1e9);
    for (let i = 0; i < n; i++) out.push(ALPHABET[buf[i] % ALPHABET.length]);
    return out.join('');
  }

  function checksum(body) {
    let h = 7;
    for (const ch of body) h = (h * 31 + ch.charCodeAt(0)) % 1024;
    return ALPHABET[h % 32] + ALPHABET[(h >> 5) % 32];
  }

  // Los cupones se generan en el navegador: sirven como demostración y para
  // validar el formato en el punto de venta. Para canjes reales se necesita un
  // backend que registre y marque cada código como usado (ver README).
  FG.Vouchers = {
    create() {
      const body = randomChars(6);
      return `FG-${body.slice(0, 3)}-${body.slice(3)}-${checksum(body)}`;
    },
    isValidFormat(code) {
      const m = /^FG-([A-Z2-9]{3})-([A-Z2-9]{3})-([A-Z2-9]{2})$/.exec(String(code).trim().toUpperCase());
      return !!m && checksum(m[1] + m[2]) === m[3];
    },
  };
})();
