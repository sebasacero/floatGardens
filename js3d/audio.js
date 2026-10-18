// Efectos de sonido sintetizados (sin archivos de audio).
(function () {
  const Audio = {
    ctx: null,
    muted: false,
    ensure() {
      if (this.ctx || this.muted) return this.ctx;
      try {
        this.ctx = new (window.AudioContext || window.webkitAudioContext)();
        this.master = this.ctx.createGain();
        this.master.gain.value = 0.35;
        this.master.connect(this.ctx.destination);
        const len = this.ctx.sampleRate * 0.5;
        this.noise = this.ctx.createBuffer(1, len, this.ctx.sampleRate);
        const d = this.noise.getChannelData(0);
        for (let i = 0; i < len; i++) d[i] = Math.random() * 2 - 1;
      } catch (e) { this.ctx = null; }
      return this.ctx;
    },
    env(node, t, a, peak, dur) {
      node.gain.setValueAtTime(0.0001, t);
      node.gain.exponentialRampToValueAtTime(peak, t + a);
      node.gain.exponentialRampToValueAtTime(0.0001, t + dur);
    },
    burst(freq, dur, peak, type) {
      const c = this.ensure();
      if (!c || this.muted) return;
      const t = c.currentTime;
      const src = c.createBufferSource();
      src.buffer = this.noise;
      const f = c.createBiquadFilter();
      f.type = type || 'bandpass';
      f.frequency.setValueAtTime(freq, t);
      f.frequency.exponentialRampToValueAtTime(freq * 0.4, t + dur);
      const g = c.createGain();
      this.env(g, t, 0.005, peak, dur);
      src.connect(f).connect(g).connect(this.master);
      src.start(t);
      src.stop(t + dur + 0.05);
    },
    tone(freq, dur, peak, type, slide) {
      const c = this.ensure();
      if (!c || this.muted) return;
      const t = c.currentTime;
      const o = c.createOscillator();
      o.type = type || 'sine';
      o.frequency.setValueAtTime(freq, t);
      if (slide) o.frequency.exponentialRampToValueAtTime(slide, t + dur);
      const g = c.createGain();
      this.env(g, t, 0.005, peak, dur);
      o.connect(g).connect(this.master);
      o.start(t);
      o.stop(t + dur + 0.05);
    },
    shot() { this.burst(1800, 0.12, 0.5); this.tone(220, 0.08, 0.2, 'square', 90); },
    hit(head) { this.tone(head ? 1500 : 1100, 0.06, 0.25, 'triangle'); },
    ko() { this.tone(660, 0.1, 0.25, 'triangle'); setTimeout(() => this.tone(990, 0.15, 0.25, 'triangle'), 80); },
    reload() { this.burst(600, 0.08, 0.3); setTimeout(() => this.burst(900, 0.06, 0.3), 380); setTimeout(() => this.tone(500, 0.05, 0.2, 'square'), 900); },
    empty() { this.tone(300, 0.05, 0.2, 'square'); },
    hurt() { this.tone(140, 0.18, 0.4, 'sawtooth', 70); },
    pickup() { this.tone(880, 0.07, 0.2, 'sine', 1320); },
    cash() { this.tone(1046, 0.08, 0.2, 'triangle'); setTimeout(() => this.tone(1568, 0.12, 0.2, 'triangle'), 70); },
    capture() { [523, 659, 784, 1046].forEach((f, i) => setTimeout(() => this.tone(f, 0.18, 0.25, 'triangle'), i * 110)); },
    stamp() { this.burst(300, 0.15, 0.3, 'lowpass'); },
    jump() { this.tone(300, 0.08, 0.1, 'sine', 500); },
  };
  W.Audio = Audio;
})();
