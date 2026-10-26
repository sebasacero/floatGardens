// Modelos del juego, hechos con primitivas: el barista, las palomas, los drones de Arcadia, las cerezas,
// las letras F-L-O-A-T, los tanques-cañón, el colchón, los andamios, las tejas, los avisos y la tormenta.
(function () {
  const FG = window.FGT;
  const { C } = FG;
  const std = (color, extra) => new THREE.MeshStandardMaterial(Object.assign({ color, roughness: 0.7 }, extra || {}));
  const mesh = (g, m, x = 0, y = 0, z = 0) => { const o = new THREE.Mesh(g, m); o.position.set(x, y, z); o.castShadow = true; return o; };

  // ---------- el barista de Float Gardens ----------
  FG.makePlayer = function () {
    const root = new THREE.Group();
    const body = new THREE.Group(); body.position.y = 0.85; root.add(body); // pivote para rodar y estirar
    const skin = std('#c98c5a'), shirt = std('#f4f1ea'), apron = std('#1f9d4c'), jean = std('#2f4f7a'), shoe = std('#2a2a2a'), cap = std('#23b357'), dark = std('#1b1b1b');
    const torso = mesh(new THREE.CapsuleGeometry(0.3, 0.38, 4, 10), shirt, 0, 0.22, 0); body.add(torso);
    const ap = mesh(new THREE.BoxGeometry(0.5, 0.6, 0.06), apron, 0, 0.12, 0.28); body.add(ap);
    const strap = mesh(new THREE.BoxGeometry(0.42, 0.06, 0.05), apron, 0, 0.42, 0.27); body.add(strap);
    // la AeroPress en la espalda
    const press = mesh(new THREE.CylinderGeometry(0.11, 0.11, 0.55, 10), std('#3a3f48'), 0, 0.3, -0.33); press.rotation.z = 0.25; body.add(press);
    const plunger = mesh(new THREE.CylinderGeometry(0.13, 0.13, 0.08, 10), std('#ff7a2b'), -0.07, 0.6, -0.33); body.add(plunger);
    const head = new THREE.Group(); head.position.y = 0.78; body.add(head);
    head.add(mesh(new THREE.SphereGeometry(0.3, 16, 12), skin));
    const capTop = mesh(new THREE.SphereGeometry(0.315, 16, 8, 0, Math.PI * 2, 0, Math.PI / 2), cap, 0, 0.04, 0); head.add(capTop);
    const brim = mesh(new THREE.BoxGeometry(0.42, 0.04, 0.26), cap, 0, 0.06, 0.32); head.add(brim);
    const logo = mesh(new THREE.SphereGeometry(0.07, 8, 6), std('#eaffd8'), 0, 0.2, 0.25); logo.scale.set(1.3, 0.7, 0.4); head.add(logo);
    for (const s of [-1, 1]) head.add(mesh(new THREE.SphereGeometry(0.045, 8, 6), dark, s * 0.11, 0.0, 0.27));
    const stache = mesh(new THREE.BoxGeometry(0.26, 0.06, 0.06), std('#3b2416'), 0, -0.1, 0.28); head.add(stache);
    const nose = mesh(new THREE.SphereGeometry(0.06, 8, 6), skin, 0, -0.04, 0.3); head.add(nose);
    const limb = (len, r, m, endM, endR) => {
      const g = new THREE.Group();
      const l = mesh(new THREE.CapsuleGeometry(r, len, 3, 8), m, 0, -len / 2 - r * 0.5, 0); g.add(l);
      const e = mesh(new THREE.SphereGeometry(endR, 8, 6), endM, 0, -len - r, endM === shoe ? 0.06 : 0); if (endM === shoe) e.scale.set(1, 0.7, 1.5); g.add(e);
      return g;
    };
    const legs = [-1, 1].map((s) => { const g = limb(0.42, 0.11, jean, shoe, 0.13); g.position.set(s * 0.14, -0.12, 0); body.add(g); return g; });
    const arms = [-1, 1].map((s) => { const g = limb(0.36, 0.09, shirt, skin, 0.1); g.position.set(s * 0.38, 0.46, 0); g.rotation.z = s * 0.15; body.add(g); return g; });
    root.userData = { body, head, legs, arms };
    root.traverse((o) => { if (o.isMesh) o.castShadow = true; });
    return root;
  };

  // Pose del barista según el estado (anim.t = fase de carrera).
  FG.posePlayer = function (m, st, dt) {
    const { body, head, legs, arms } = m.userData;
    const k = Math.min(1, dt * 14), lerp = (o, v) => o + (v - o) * k;
    let L = [0, 0], A = [0, 0], tilt = 0;
    if (st.mode === 'run') {
      const s = Math.sin(st.phase), amp = 0.35 + 0.65 * st.speed;
      L = [s * 0.9 * amp, -s * 0.9 * amp]; A = [-s * 0.8 * amp, s * 0.8 * amp]; tilt = 0.12 * st.speed;
    } else if (st.mode === 'jump') { L = [-0.9, 0.5]; A = [-2.6, -2.2]; tilt = 0.05; }
    else if (st.mode === 'fall') { L = [0.4, -0.3]; A = [-2.0 + Math.sin(st.phase * 2) * 0.4, -2.3 - Math.sin(st.phase * 2) * 0.4]; }
    else if (st.mode === 'idle') { A = [Math.sin(st.phase * 0.5) * 0.05, -Math.sin(st.phase * 0.5) * 0.05]; }
    legs[0].rotation.x = lerp(legs[0].rotation.x, L[0]); legs[1].rotation.x = lerp(legs[1].rotation.x, L[1]);
    arms[0].rotation.x = lerp(arms[0].rotation.x, A[0]); arms[1].rotation.x = lerp(arms[1].rotation.x, A[1]);
    body.rotation.x = lerp(body.rotation.x, tilt);
    head.position.y = 0.78 + (st.mode === 'idle' ? Math.sin(st.phase * 0.5) * 0.015 : 0);
  };

  // ---------- paloma ----------
  FG.makePigeon = function () {
    const g = new THREE.Group(), grey = std('#8b95a6'), dark = std('#5d6675');
    const b = mesh(new THREE.SphereGeometry(0.3, 12, 10), grey, 0, 0.36, 0); b.scale.set(1.5, 1, 1); g.add(b);
    const tail = mesh(new THREE.BoxGeometry(0.34, 0.06, 0.2), dark, -0.48, 0.42, 0); tail.rotation.z = 0.35; g.add(tail);
    const neck = mesh(new THREE.SphereGeometry(0.17, 10, 8), std('#5f7f74', { metalness: 0.5, roughness: 0.3 }), 0.3, 0.6, 0); g.add(neck);
    const head = new THREE.Group(); head.position.set(0.38, 0.76, 0); g.add(head);
    head.add(mesh(new THREE.SphereGeometry(0.13, 10, 8), grey));
    const beak = mesh(new THREE.ConeGeometry(0.04, 0.14, 6), std('#e0a36a'), 0.15, -0.01, 0); beak.rotation.z = -Math.PI / 2; head.add(beak);
    for (const s of [-1, 1]) { head.add(mesh(new THREE.SphereGeometry(0.03, 6, 4), std('#ff7a2b'), 0.07, 0.04, s * 0.09)); }
    for (const s of [-1, 1]) {
      const wing = mesh(new THREE.SphereGeometry(0.22, 8, 6), dark, -0.05, 0.42, s * 0.24); wing.scale.set(1.5, 0.5, 0.35); g.add(wing);
      g.add(mesh(new THREE.CylinderGeometry(0.025, 0.025, 0.14, 5), std('#d9737a'), 0.02, 0.08, s * 0.08));
    }
    g.userData.head = head;
    g.scale.setScalar(1.35);
    return g;
  };

  // ---------- dron de Arcadia ----------
  FG.makeDrone = function () {
    const g = new THREE.Group(), dm = std('#2a2342', { metalness: 0.5, roughness: 0.35 });
    const core = mesh(new THREE.CylinderGeometry(0.55, 0.45, 0.35, 12), dm); g.add(core);
    const dome = mesh(new THREE.SphereGeometry(0.4, 12, 8, 0, Math.PI * 2, 0, Math.PI / 2), std('#6b4bd8', { metalness: 0.3, roughness: 0.2 }), 0, 0.15, 0); g.add(dome);
    const eye = mesh(new THREE.SphereGeometry(0.14, 10, 8), new THREE.MeshBasicMaterial({ color: '#ff2d55' }), 0, 0, 0.47); g.add(eye);
    const rotors = [];
    for (const [x, z] of [[-0.75, -0.5], [0.75, -0.5], [-0.75, 0.5], [0.75, 0.5]]) {
      const arm = mesh(new THREE.BoxGeometry(0.5, 0.06, 0.08), dm, x * 0.6, 0.05, z * 0.6); arm.rotation.y = Math.atan2(-z, x); g.add(arm);
      const r = mesh(new THREE.CylinderGeometry(0.34, 0.34, 0.02, 14), new THREE.MeshBasicMaterial({ color: '#c8b8ff', transparent: true, opacity: 0.45 }), x, 0.16, z);
      g.add(r); rotors.push(r);
    }
    g.userData.rotors = rotors;
    return g;
  };

  // ---------- cerezas de café (instanciadas) ----------
  FG.makeCherries = function (n) {
    const geo = new FG.Geo();
    geo.geom(new THREE.SphereGeometry(0.2, 12, 10), new THREE.Matrix4().makeTranslation(-0.1, 0, 0), C('#e0262f'));
    geo.geom(new THREE.SphereGeometry(0.18, 12, 10), new THREE.Matrix4().makeTranslation(0.13, 0.05, 0.05), C('#c81e28'));
    geo.geom(new THREE.CylinderGeometry(0.02, 0.02, 0.3, 5), FG.M4(0.0, 0.28, 0, 0, 1, 1, 1, 0, 0.3), C('#3d7a2a'));
    geo.geom(new THREE.SphereGeometry(0.1, 8, 6), FG.M4(0.08, 0.42, 0, 0, 1.6, 0.4, 0.8), C('#4caf50'));
    const im = new THREE.InstancedMesh(geo.build(), new THREE.MeshStandardMaterial({ vertexColors: true, roughness: 0.25, emissive: C('#5a0000'), emissiveIntensity: 0.4 }), n);
    im.castShadow = true;
    return im;
  };

  FG.makeLetter = function (ch) {
    const t = FG.letterTex(ch);
    const side = std('#e09b16', { metalness: 0.6, roughness: 0.3 });
    const face = new THREE.MeshStandardMaterial({ map: t, metalness: 0.3, roughness: 0.4, emissive: C('#4a3000'), emissiveIntensity: 0.6 });
    const m = new THREE.Mesh(new THREE.CylinderGeometry(0.6, 0.6, 0.14, 24), [side, face, face]);
    m.rotation.x = Math.PI / 2; m.castShadow = true;
    const g = new THREE.Group(); g.add(m);
    return g;
  };

  // ---------- tanque de agua que hace de cañón ----------
  FG.makeTank = function (valla) {
    const g = new THREE.Group();
    const barrel = new THREE.Group(); g.add(barrel);
    const black = std('#25272c', { roughness: 0.5 });
    const body = mesh(new THREE.CylinderGeometry(0.95, 0.85, 2.0, 18), black); barrel.add(body);
    for (const y of [-0.5, 0.1, 0.6]) { const r = mesh(new THREE.TorusGeometry(0.93 - y * 0.05, 0.05, 6, 18), std('#33363d'), 0, y, 0); r.rotation.x = Math.PI / 2; barrel.add(r); }
    const rim = mesh(new THREE.TorusGeometry(0.88, 0.09, 8, 20), std('#3a3d44'), 0, 1.0, 0); rim.rotation.x = Math.PI / 2; barrel.add(rim);
    const mouth = mesh(new THREE.CircleGeometry(0.85, 20), new THREE.MeshBasicMaterial({ color: '#0b0b0d' }), 0, 0.99, 0); mouth.rotation.x = -Math.PI / 2; barrel.add(mouth);
    const sticker = new THREE.Mesh(new THREE.PlaneGeometry(1.2, 0.5), new THREE.MeshStandardMaterial({ map: FG.signTex('Cañón de agua'), transparent: true }));
    sticker.position.set(0, -0.1, 0.96); barrel.add(sticker);
    if (!valla) {
      for (const s of [-1, 1]) g.add(mesh(new THREE.BoxGeometry(0.14, 1.2, 0.14), std('#6b6f78'), s * 0.6, -0.6, 0.3), mesh(new THREE.BoxGeometry(0.14, 1.2, 0.14), std('#6b6f78'), s * 0.6, -0.6, -0.3));
    }
    g.userData.barrel = barrel;
    return g;
  };

  FG.makeMattress = function () {
    const t = FG.mattressTex();
    const side = std('#d9cdb8'), top = new THREE.MeshStandardMaterial({ map: t, roughness: 0.9 });
    const m = new THREE.Mesh(new THREE.BoxGeometry(2.0, 0.42, 1.5), [side, side, top, side, side, side]);
    m.position.y = 0.21; m.castShadow = m.receiveShadow = true;
    const g = new THREE.Group(); g.add(m);
    // cajas de cerveza que lo sostienen
    for (const s of [-0.6, 0.6]) g.add(mesh(new THREE.BoxGeometry(0.5, 0.3, 0.4), std('#d9a520'), s, -0.15, 0));
    m.position.y = 0.21; g.userData.m = m;
    return g;
  };

  FG.makeScaffold = function (w, ropeTop) {
    const g = new THREE.Group();
    const wood = std('#a2753f'), metal = std('#6b7079', { metalness: 0.4 });
    const deck = mesh(new THREE.BoxGeometry(w, 0.22, 1.6), wood, 0, -0.11, 0); deck.receiveShadow = true; g.add(deck);
    for (let i = 0; i < 4; i++) g.add(mesh(new THREE.BoxGeometry(w, 0.02, 0.04), std('#7b5428'), 0, 0.005, -0.6 + i * 0.4));
    g.add(mesh(new THREE.BoxGeometry(w, 0.06, 0.06), metal, 0, 0.95, -0.75));
    for (const s of [-1, 1]) g.add(mesh(new THREE.BoxGeometry(0.06, 1.0, 0.06), metal, s * (w / 2 - 0.05), 0.5, -0.75));
    const ropes = [];
    for (const s of [-1, 1]) {
      const r = mesh(new THREE.CylinderGeometry(0.025, 0.025, 1, 4), std('#d8cfb0'), s * (w / 2 - 0.1), 0, -0.3);
      g.add(r); ropes.push(r);
    }
    g.userData = { ropes, ropeTop };
    return g;
  };

  FG.makeSheet = function (w) {
    const g = new THREE.Group();
    const sheet = new THREE.Group(); g.add(sheet);
    const gm = std('#a7acaf', { roughness: 0.6, metalness: 0.2 });
    sheet.add(mesh(new THREE.BoxGeometry(w, 0.06, 2.0), gm, 0, -0.03, 0));
    for (let x = -w / 2 + 0.12; x < w / 2; x += 0.18) {
      const r = mesh(new THREE.CylinderGeometry(0.05, 0.05, 2.0, 5, 1, false, 0, Math.PI), gm, x, 0, 0); r.rotation.x = Math.PI / 2; sheet.add(r);
    }
    g.userData.sheet = sheet;
    return g;
  };
  FG.makeSheetPosts = function (w, y) {
    const g = new THREE.Group(), wood = std('#8a6438');
    for (const s of [-1, 1]) {
      const p = mesh(new THREE.BoxGeometry(0.16, y, 0.16), wood, s * (w / 2 - 0.3), -y / 2 - 0.06, 0.6); g.add(p);
      const p2 = p.clone(); p2.position.z = -0.6; g.add(p2);
    }
    return g;
  };

  // Aviso de punto de control: se prende cuando lo tocas.
  FG.makeCheckpoint = function () {
    const g = new THREE.Group();
    const off = FG.signTex('Punto de control', false), on = FG.signTex('¡Guardado!');
    const mat = new THREE.MeshStandardMaterial({ map: off, emissive: C('#ffffff'), emissiveMap: off, emissiveIntensity: 0.1 });
    const s = new THREE.Mesh(new THREE.PlaneGeometry(3.3, 1.34), mat); s.position.set(0, 2.6, -0.2); g.add(s);
    const back = mesh(new THREE.BoxGeometry(3.4, 1.42, 0.08), std('#e8e8e8'), 0, 2.6, -0.27); g.add(back);
    for (const x of [-1.3, 1.3]) g.add(mesh(new THREE.BoxGeometry(0.12, 2.0, 0.12), std('#3a3f48'), x, 1.0, -0.3));
    const glow = new THREE.Sprite(new THREE.SpriteMaterial({ map: FG.glowTex(), color: '#53ff8f', transparent: true, opacity: 0, depthWrite: false, blending: THREE.AdditiveBlending }));
    glow.position.set(0, 2.6, 0); glow.scale.set(6, 3.4, 1); g.add(glow);
    g.userData = { mat, on, glow };
    return g;
  };
  FG.lightCheckpoint = function (g) {
    const u = g.userData; u.mat.map = u.on; u.mat.emissiveMap = u.on; u.mat.emissiveIntensity = 0.55; u.mat.needsUpdate = true; u.glow.material.opacity = 0.7;
  };

  // Meta en el anexo de la torre: arco con cinta y aviso grande.
  FG.makeGoal = function () {
    const g = new THREE.Group();
    const t = FG.signTex('¡Llegaste a la torre!');
    const s = new THREE.Mesh(new THREE.PlaneGeometry(5.2, 2.1), new THREE.MeshStandardMaterial({ map: t, emissive: C('#ffffff'), emissiveMap: t, emissiveIntensity: 0.5 }));
    s.position.set(0, 4.3, 0.1); g.add(s);
    for (const x of [-2.4, 2.4]) g.add(mesh(new THREE.CylinderGeometry(0.14, 0.14, 4.4, 8), std('#eef2f5'), x, 2.2, 0));
    const ribbon = mesh(new THREE.BoxGeometry(4.8, 0.18, 0.04), std('#ffcf3d'), 0, 1.3, 0.05); g.add(ribbon);
    g.userData.ribbon = ribbon;
    return g;
  };

  // La tormenta morada de Arcadia: una pared de nubes que avanza. uEdge = x del borde.
  FG.makeStorm = function () {
    const uni = { uT: { value: 0 }, uEdge: { value: -999 }, uA: { value: 1 } };
    const mat = new THREE.ShaderMaterial({
      uniforms: uni, transparent: true, depthWrite: false, side: THREE.DoubleSide,
      vertexShader: 'varying vec3 vW;void main(){vec4 w=modelMatrix*vec4(position,1.);vW=w.xyz;gl_Position=projectionMatrix*viewMatrix*w;}',
      fragmentShader: `uniform float uT,uEdge,uA;varying vec3 vW;
        float h(vec2 p){return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453);}
        float n(vec2 p){vec2 i=floor(p),f=fract(p);f=f*f*(3.-2.*f);return mix(mix(h(i),h(i+vec2(1,0)),f.x),mix(h(i+vec2(0,1)),h(i+vec2(1,1)),f.x),f.y);}
        float fbm(vec2 p){float s=0.,a=.5;for(int i=0;i<5;i++){s+=a*n(p);p*=2.03;a*=.5;}return s;}
        void main(){
          vec2 p=vec2(vW.x*.09-uT*.35,vW.y*.07+uT*.12);
          float f=fbm(p+fbm(p*1.7+uT*.2));
          float d=uEdge-vW.x;
          float edge=smoothstep(-1.5,6.,d+ (f-.5)*7.);
          vec3 c=mix(vec3(.18,.05,.32),vec3(.72,.33,1.),f*f*1.4);
          c+=vec3(.9,.7,1.)*pow(max(0.,1.-abs(d+(f-.5)*7.)/2.5),3.)*.8;
          gl_FragColor=vec4(c,edge*uA*.93);
        }`,
    });
    const g = new THREE.Group();
    for (const [z, s] of [[3.2, 1], [-3, 1.1], [-14, 1.3]]) {
      const m = new THREE.Mesh(new THREE.PlaneGeometry(120 * s, 70), mat); m.position.set(0, 25, z); m.renderOrder = 5; g.add(m);
    }
    const flash = new THREE.PointLight('#c88bff', 0, 60); flash.position.set(-4, 18, 6); g.add(flash);
    g.userData = { uni, flash };
    return g;
  };
})();
