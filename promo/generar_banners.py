"""Genera los banners de Instagram, historias y web a partir de los símbolos
de banner-redes-1200x628.svg. Uso: python3 promo/generar_banners.py"""
import os, re
root = os.path.dirname(os.path.abspath(__file__)) + '/'
src = open(root + 'banner-redes-1200x628.svg').read()
defs = re.search(r'<defs>.*?</defs>', src, re.S).group(0)
FONT = 'system-ui, -apple-system, Segoe UI, Roboto, sans-serif'

def use(sym, x, y, w, h, extra=''):
    return f'<use href="#{sym}" xlink:href="#{sym}" x="{x}" y="{y}" width="{w}" height="{h}"{extra}/>'

def svg(w, h, title, body):
    return f'''<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="{w}" height="{h}" viewBox="0 0 {w} {h}">
  <title>{title}</title>
  {defs}
  <rect width="{w}" height="{h}" fill="url(#bg)"/>
{body}
</svg>
'''

def pill(cx, cy, w, h, fill, text, size, color, rot=0):
    return f'''  <g transform="translate({cx} {cy}) rotate({rot})">
    <rect x="{-w/2}" y="{-h/2}" width="{w}" height="{h}" rx="{h/2}" fill="{fill}"/>
    <text x="0" y="{size*0.35:.1f}" text-anchor="middle" font-family="{FONT}" font-weight="900" font-size="{size}" fill="{color}">{text}</text>
  </g>'''

def water(w, top, h):
    seg = w / 6
    d = f'M0 {h} V{top} '
    for i in range(6):
        x1 = seg * i + seg / 2; x2 = seg * (i + 1)
        d += f'Q{x1:.0f} {top - 22} {x2:.0f} {top} '
    d += f'V{h} Z'
    waves = ''.join(f'<path d="M{seg*i+40:.0f} {top+50+(i%3)*45} q14 -9 28 0" stroke="#fff" stroke-opacity="0.35" stroke-width="5" fill="none" stroke-linecap="round"/>' for i in range(6))
    return f'  <path d="{d}" fill="url(#water)"/>\n  {waves}'

def steps(x, y, gap, size):
    items = [('🌱', 'Farmea', 'cosecha cerezas'), ('★', 'Suma', 'Puntos Float'), ('☕', 'Canjea', 'café gratis')]
    out = []
    for i, (ic, a, b) in enumerate(items):
        cx = x + i * gap
        out.append(f'''  <g font-family="{FONT}" text-anchor="middle">
    <circle cx="{cx}" cy="{y}" r="{size}" fill="#fff3dc" fill-opacity="0.1" stroke="#2fbf71" stroke-width="4"/>
    <text x="{cx}" y="{y + size*0.35:.0f}" font-size="{size}" fill="#ffe08a">{ic}</text>
    <text x="{cx}" y="{y + size + 42}" font-size="{size*0.62:.0f}" font-weight="900" fill="#fff3dc">{a}</text>
    <text x="{cx}" y="{y + size + 76}" font-size="{size*0.44:.0f}" fill="#9fe8bf">{b}</text>
  </g>''')
        if i < 2:
            out.append(f'  <text x="{cx + gap/2}" y="{y + 14}" text-anchor="middle" font-family="{FONT}" font-size="{size*0.8:.0f}" font-weight="900" fill="#2fbf71">➜</text>')
    return '\n'.join(out)

# ---------- Instagram 1080x1080 ----------
ig = f'''  <g font-family="{FONT}" text-anchor="middle">
    <text x="540" y="92" font-size="34" font-weight="800" fill="#2fbf71" letter-spacing="8">FLOAT GARDENS</text>
    <rect x="370" y="112" width="340" height="44" rx="22" fill="#d6423a"/>
    <text x="540" y="143" font-size="23" font-weight="800" fill="#fff3dc" letter-spacing="1">LA CONQUISTA DEL CAFÉ</text>
    <text x="540" y="262" font-size="112" font-weight="900" fill="#fff3dc">¡FARMEA CAFÉ!</text>
    <text x="540" y="322" font-size="36" font-weight="600" fill="#fff3dc">Juega, gana <tspan fill="#9fe8bf" font-weight="800">Puntos Float</tspan> y tómate un</text>
    <text x="540" y="384" font-size="54" font-weight="900" fill="#ffe08a">CAFÉ GRATIS ☕</text>
  </g>
{water(1080, 820, 1080)}
  {use('plant', 70, 700, 230, 210)}
  {use('plant', 790, 720, 230, 210)}
  {use('barista', 360, 450, 300, 400)}
  {use('aeropress', 577, 478, 78, 136)}
  <path d="M640 505 Q760 455 860 490" stroke="#6b3b1f" stroke-width="12" fill="none" stroke-linecap="round" stroke-dasharray="2 26"/>
  {use('splash', 850, 455, 80, 80)}
  <g transform="translate(940 640) rotate(12)">{use('inspector', -60, -190, 120, 195)}</g>
{pill(205, 520, 140, 56, '#9fe8bf', '+10 ★', 30, '#0f3d2e', -8)}
  {use('cherry', 150, 600, 70, 70)}
{pill(540, 960, 400, 84, '#2fbf71', '▶ JUEGA GRATIS', 38, '#082419')}
  <text x="540" y="1045" text-anchor="middle" font-family="{FONT}" font-size="22" fill="#fff3dc" opacity="0.85">Canjea en los puntos oficiales de Float Gardens · Bogotá</text>'''
open(root + 'banner-instagram-1080x1080.svg', 'w').write(svg(1080, 1080, 'Float Gardens — ¡Farmea café! (Instagram)', ig))

# ---------- Historias 1080x1920 ----------
st = f'''  <g font-family="{FONT}" text-anchor="middle">
    <text x="540" y="170" font-size="44" font-weight="800" fill="#2fbf71" letter-spacing="10">FLOAT GARDENS</text>
    <rect x="320" y="200" width="440" height="58" rx="29" fill="#d6423a"/>
    <text x="540" y="240" font-size="30" font-weight="800" fill="#fff3dc" letter-spacing="1">LA CONQUISTA DEL CAFÉ</text>
    <text x="540" y="420" font-size="140" font-weight="900" fill="#fff3dc">¡FARMEA</text>
    <text x="540" y="560" font-size="140" font-weight="900" fill="#fff3dc">CAFÉ!</text>
    <text x="540" y="640" font-size="40" font-weight="600" fill="#fff3dc">Un barista, una AeroPress y un</text>
    <text x="540" y="694" font-size="40" font-weight="600" fill="#fff3dc">invernadero flotante contra la Federación.</text>
  </g>
{steps(230, 820, 310, 64)}
{water(1080, 1380, 1920)}
  {use('plant', 40, 1250, 260, 238)}
  {use('plant', 780, 1270, 260, 238)}
  {use('barista', 330, 1060, 380, 506)}
  {use('aeropress', 604, 1095, 99, 172)}
  <path d="M690 1135 Q800 1075 900 1110" stroke="#6b3b1f" stroke-width="12" fill="none" stroke-linecap="round" stroke-dasharray="2 26"/>
  {use('splash', 880, 1070, 90, 90)}
  <g transform="translate(960 1280) rotate(12)">{use('inspector', -65, -205, 130, 212)}</g>
{pill(200, 1130, 160, 64, '#9fe8bf', '+10 ★', 34, '#0f3d2e', -8)}
{pill(540, 1640, 520, 110, '#ffe08a', 'CAFÉ GRATIS ☕', 54, '#4a2c1d')}
{pill(540, 1780, 460, 96, '#2fbf71', '▶ JUEGA AHORA', 44, '#082419')}
  <text x="540" y="1880" text-anchor="middle" font-family="{FONT}" font-size="26" fill="#fff3dc" opacity="0.85">Canjea tus Puntos Float en los puntos oficiales</text>'''
open(root + 'banner-historia-1080x1920.svg', 'w').write(svg(1080, 1920, 'Float Gardens — ¡Farmea café! (Historias)', st))

# ---------- Web 728x90 ----------
web = f'''  <path d="M0 90 V74 Q60 66 120 74 T240 74 T360 74 T480 74 T600 74 T728 72 V90 Z" fill="url(#water)" opacity="0.8"/>
  {use('plant', 6, 14, 78, 72)}
  <g font-family="{FONT}">
    <text x="96" y="30" font-size="14" font-weight="800" fill="#2fbf71" letter-spacing="3">FLOAT GARDENS</text>
    <text x="96" y="62" font-size="30" font-weight="900" fill="#fff3dc">¡Farmea café!</text>
    <text x="318" y="40" font-size="17" font-weight="600" fill="#fff3dc">Gana <tspan fill="#9fe8bf" font-weight="800">Puntos Float</tspan></text>
    <text x="318" y="64" font-size="20" font-weight="900" fill="#ffe08a">y toma café GRATIS ☕</text>
  </g>
  {use('aeropress', 538, 18, 34, 60)}
{pill(642, 45, 150, 46, '#2fbf71', '▶ JUEGA', 22, '#082419')}'''
open(root + 'banner-web-728x90.svg', 'w').write(svg(728, 90, 'Float Gardens — ¡Farmea café! (web)', web))
print('ok')
