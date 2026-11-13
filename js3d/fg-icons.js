// Kit de interfaz v2 (guía de diseño, assets/icons.js): íconos de línea de la web para el HUD y el lobby.
// Íconos de línea 1.4 px en rejilla de 24, mismo trazo que la web. Uso: <i data-i="campana"></i>
const FG_ICONS={
  hoja:'<path d="M12 3c4.5 2.5 6.5 6 6 10.5-.3 3.4-2.9 6.2-6 7.5-3.1-1.3-5.7-4.1-6-7.5C5.5 9 7.5 5.5 12 3z" fill="#2c302e" stroke="none"/><path d="M12 6v14" stroke="#fff" stroke-width="1.6"/>',
  hojaBlanca:'<path d="M12 3c4.5 2.5 6.5 6 6 10.5-.3 3.4-2.9 6.2-6 7.5-3.1-1.3-5.7-4.1-6-7.5C5.5 9 7.5 5.5 12 3z" fill="#fff" stroke="none"/><path d="M12 5v15" stroke="#7fb349" stroke-width="1.4"/>',
  gota:'<path d="M12 3s6 6.5 6 11a6 6 0 0 1-12 0c0-4.5 6-11 6-11z"/>',
  calendario:'<rect x="4" y="5" width="16" height="15"/><path d="M4 10h16M9 3v4M15 3v4"/>',
  clima:'<path d="M6 4v16M12 4v16M18 4v16"/><circle cx="6" cy="9" r="1.6"/><circle cx="12" cy="15" r="1.6"/><circle cx="18" cy="8" r="1.6"/>',
  sensor:'<rect x="5" y="5" width="14" height="14" rx="2"/><path d="M9 9h6v6H9z"/>',
  tablero:'<rect x="4" y="4" width="7" height="7"/><rect x="13" y="4" width="7" height="7"/><rect x="4" y="13" width="7" height="7"/><rect x="13" y="13" width="7" height="7"/>',
  eco:'<path d="M5 19c0-8 6-13 14-14-1 8-6 14-14 14zM5 19l7-7"/>',
  campana:'<path d="M6 16V11a6 6 0 0 1 12 0v5l2 2H4zM10 20a2 2 0 0 0 4 0"/>',
  ajustes:'<circle cx="12" cy="12" r="3"/><path d="M12 2v3M12 19v3M2 12h3M19 12h3M4.9 4.9l2.1 2.1M17 17l2.1 2.1M4.9 19.1L7 17M17 7l2.1-2.1"/>',
  cuenta:'<circle cx="12" cy="8" r="4"/><path d="M4 21c1.5-4 4.5-6 8-6s6.5 2 8 6"/>',
  buscar:'<circle cx="11" cy="11" r="6"/><path d="M20 20l-4.5-4.5"/>',
  pin:'<path d="M12 21s-6-5.5-6-11a6 6 0 0 1 12 0c0 5.5-6 11-6 11z"/><circle cx="12" cy="10" r="2"/>',
  mapa:'<path d="M3 6l6-2 6 2 6-2v14l-6 2-6-2-6 2z"/><path d="M9 4v14M15 6v14"/>',
  mochila:'<rect x="5" y="7" width="14" height="13" rx="2"/><path d="M9 7V5a3 3 0 0 1 6 0v2M5 13h14M10 13v2h4v-2"/>',
  red:'<circle cx="12" cy="5" r="2"/><circle cx="5" cy="18" r="2"/><circle cx="19" cy="18" r="2"/><path d="M11 7l-5 9M13 7l5 9M7 18h10"/>',
  objetivo:'<circle cx="12" cy="12" r="7"/><circle cx="12" cy="12" r="2.5"/><path d="M12 2v3M12 19v3M2 12h3M19 12h3"/>',
  senal:'<path d="M5 12a10 10 0 0 1 14 0M8 15a6 6 0 0 1 8 0"/><circle cx="12" cy="18.5" r="1.2"/><path d="M2 9a14 14 0 0 1 20 0"/>',
  saltar:'<path d="M12 20V5M6 11l6-6 6 6"/>',
  tienda:'<path d="M4 9h16l-1 11H5zM8 9V7a4 4 0 0 1 8 0v2"/>',
  laboratorio:'<path d="M9 3h6M10 3v6l-5 9a2 2 0 0 0 2 3h10a2 2 0 0 0 2-3l-5-9V3M7.5 15h9"/>',
  jugar:'<path d="M8 5l11 7-11 7z"/>',
  amigos:'<circle cx="9" cy="8" r="3.5"/><circle cx="17" cy="9" r="2.5"/><path d="M2.5 20c1-3.5 3.5-5.5 6.5-5.5s5.5 2 6.5 5.5M15 14.6c2.8 0 5 1.8 6 4.9"/>',
  cereza:'<circle cx="8.5" cy="15.5" r="4"/><circle cx="16" cy="14" r="4"/><path d="M8.5 11.5C9 7 12 4 16 3M16 10V3"/>',
  hackear:'<rect x="7" y="7" width="10" height="10"/><path d="M10 3v4M14 3v4M10 17v4M14 17v4M3 10h4M3 14h4M17 10h4M17 14h4"/>',
  ojo:'<path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12z"/><circle cx="12" cy="12" r="2.5"/>',
  rociador:'<path d="M4 20l6-6M8 12l4-4 4 4-4 4zM15 6l2-2M17 10l3-1M13 4l1-3"/>',
  flecha:'<path d="M2 8l6-6M3 2h5v5"/>'
};
function fgIcons(root=document){root.querySelectorAll('i[data-i]').forEach(el=>{const n=el.dataset.i;const vb=n==='flecha'?'0 0 10 10':'0 0 24 24';
  el.outerHTML=`<svg class="ico" viewBox="${vb}" aria-hidden="true">${FG_ICONS[n]||''}</svg>`})}
function fgBars(el,peak,n=42){for(let i=0;i<n;i++){const d=i/6;const near=Math.abs(d-peak);const h=near<.5?92-near*30:30+Math.sin(i*1.7)*6;const b=document.createElement('i');b.style.height=h+'%';if(near<.5)b.className='hi';el.appendChild(b)}}
function fgStripes(el,full,n=12){for(let i=0;i<n;i++){const s=document.createElement('i');if(i>=n-full)s.className='f';el.appendChild(s)}}
function fgTicks(el,n=60,opt={}){for(let i=0;i<n;i++){const t=i/n;const day=Math.sin(Math.PI*Math.min(1,Math.max(0,(t-.05)/.9)));const h=12+day*70+((i*37)%11)-5;const b=document.createElement('i');b.style.height=Math.max(4,h)+'%';
  if(opt.g&&i>opt.g[0]&&i<opt.g[1]&&i%3!==0)b.className='g';if(opt.r&&opt.r.includes(i))b.className='r';el.appendChild(b)}}
function fgTele(el,w=[6,7,19,27],n=34){for(let i=0;i<n;i++){const b=document.createElement('i');if(w.includes(i))b.className='w';el.appendChild(b)}}
// plano técnico del dron de Arcadia, en líneas blancas como el plano de la bomba en la web
const FG_DRONE_BP=`<svg class="bp" viewBox="0 0 240 130" fill="none" stroke="#fff" stroke-width=".9" aria-hidden="true">
 <g opacity=".35" stroke-width=".4"><path d="M0 15h240M0 45h240M0 75h240M0 105h240M30 0v130M80 0v130M130 0v130M180 0v130M230 0v130"/></g>
 <rect x="95" y="45" width="50" height="40"/><rect x="108" y="55" width="24" height="20"/><circle cx="120" cy="65" r="4"/>
 <path d="M95 48L62 25M145 48l33-23M95 82l-33 23M145 82l33 23"/>
 <ellipse cx="55" cy="20" rx="26" ry="8"/><ellipse cx="185" cy="20" rx="26" ry="8"/><ellipse cx="55" cy="110" rx="26" ry="8"/><ellipse cx="185" cy="110" rx="26" ry="8"/>
 <circle cx="55" cy="20" r="2.5" fill="#fff"/><circle cx="185" cy="20" r="2.5" fill="#fff"/><circle cx="55" cy="110" r="2.5" fill="#fff"/><circle cx="185" cy="110" r="2.5" fill="#fff"/>
 <path d="M29 4h52M29 1v6M81 1v6"/><text x="38" y="0" dy="-1" fill="#fff" stroke="none" font-size="7" font-family="JetBrains Mono,monospace">Ø 420 mm</text>
 <path d="M150 65h40" stroke-dasharray="2 2"/><text x="192" y="68" fill="#fff" stroke="none" font-size="7" font-family="JetBrains Mono,monospace">sensor</text></svg>`;
