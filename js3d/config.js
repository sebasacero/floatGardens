// Float Gardens: Bogotá — mundo abierto en tercera persona.
// Configuración del mapa, las zonas, el arma y el balance.
window.FG = window.FG || {};
window.W = window.W || {};

W.CONFIG = {
  BLOCK: 40,              // lado de una manzana (m)
  STREET: 14,             // ancho de calle (m)
  GRID: 12,               // manzanas por lado
  MAQUETA: true,          // guía v2: ciudad en maqueta blanca (false = estilo anterior de ladrillo y color)
  SUPER_TORRES: true,     // Kennedy 2089: Zona T y Centro Internacional con súper torres y acentos de color de cereza

  PLAYER_RADIUS: 0.45,
  WALK_SPEED: 6,
  SPRINT_SPEED: 9.5,
  JUMP_SPEED: 7,
  GRAVITY: 20,
  MAX_HP: 100,
  REGEN_DELAY: 4,         // segundos sin recibir daño antes de regenerar
  REGEN_RATE: 18,         // vida por segundo

  // AeroPress X-1: dispara chorros de café a presión
  MAG_SIZE: 12,
  FIRE_INTERVAL: 0.14,
  DAMAGE: 25,
  HEADSHOT_MUL: 1.6,
  RANGE: 140,
  TRACER_SPEED: 160,
  RELOAD_TIME: 1.4,
  CHERRIES_PER_RELOAD: 1,

  START_CHERRIES: 15,
  CHERRIES_PER_SALE: 2,
  SALE_PRICE: 8000,
  HARVEST_YIELD: 4,
  REGROW_TIME: 25,

  // Impulso: un salto corto con la mochila de prensas; gasta energía, que vuelve sola
  DASH_SPEED: 26,
  DASH_TIME: 0.17,
  DASH_COST: 34,
  ENERGY_MAX: 100,
  ENERGY_REGEN: 24,
  MAGNET_RADIUS: 4.2,     // el botín cercano vuela hacia el barista

  CAPTURE_RADIUS: 16,
  CAPTURE_TIME: 22,       // segundos dentro de la zona sin enemigos cerca

  // Puntos Float (se canjean por café gratis; compartidos con el modo clásico)
  POINTS_PER_CHERRY: 1,
  POINTS_PER_SALE: 10,
  POINTS_PER_KO: 5,
  POINTS_PER_ZONE: 100,
  POINTS_PER_BOSS: 300,
  ZONE_MONEY: 20000,
  KO_MONEY: 500,
};

// Mapa de barrios: fila 0 = norte, columna 0 = occidente. Al oriente están los cerros.
// T Zona T · C Chapinero · U Teusaquillo · P Parque Simón Bolívar · I Centro Internacional
// L La Candelaria · A Puente Aranda · R residencial · K Kennedy (la red de terrazas) · F Torre Arcadia · B Plaza de Bolívar
W.DISTRICT_MAP = [
  'RRPPPRTTTTTT',
  'RPPPPRTTTTTT',
  'RPPPPRCCTTTT',
  'RPPPPUCCCCCC',
  'RRUUUUCCCCCC',
  'RRUUUUCCCCCC',
  'AAUUUUIIIIII',
  'AAAARRIIIIII',
  'AAAARRRLLFLL',
  'AAAAKKKLLBLL',
  'AAAKKKKLLLLL',
  'AARKKKKLLLLL',
];

W.DISTRICTS = {
  T: { name: 'Zona T', style: 'glass' },
  C: { name: 'Chapinero', style: 'brick' },
  U: { name: 'Teusaquillo', style: 'house' },
  P: { name: 'Parque Simón Bolívar', style: 'park' },
  I: { name: 'Centro Internacional', style: 'tower' },
  L: { name: 'La Candelaria', style: 'colonial' },
  A: { name: 'Puente Aranda', style: 'warehouse' },
  R: { name: 'Barrios del occidente', style: 'mixed' },
  K: { name: 'Kennedy', style: 'mixed' },
  F: { name: 'Torre Arcadia', style: 'sede' },
  B: { name: 'Plaza de Bolívar', style: 'plaza' },
};

// Carreras (norte-sur) de occidente a oriente y calles (oriente-occidente) de norte a sur.
W.CARRERAS = ['Av. Boyacá', 'Av. 68', 'Carrera 50', 'Av. NQS', 'Carrera 24', 'Carrera 17', 'Av. Caracas',
  'Carrera 13', 'Carrera 11', 'Carrera 9', 'Carrera 7', 'Carrera 5', 'Av. Circunvalar'];
W.CALLES = ['Calle 100', 'Calle 93', 'Calle 85', 'Calle 72', 'Calle 63', 'Calle 53', 'Calle 45', 'Calle 26',
  'Calle 19', 'Av. Jiménez', 'Calle 6', 'Calle 1', 'Av. Primera de Mayo'];

// Sede de Float Gardens (inicio, tienda y cultivo): la torre colgante de Kennedy, al suroccidente
W.HQ = { row: 10, col: 5, name: 'Torre Float · Kennedy', district: 'K' };

// Zonas que hay que arrebatarle a Arcadia
W.ZONES = [
  { id: 'teusaquillo', row: 5, col: 3, name: 'Parque Teusaquillo', district: 'U' },
  { id: 'simon', row: 2, col: 2, name: 'Lago del Simón Bolívar', district: 'P' },
  { id: 'zonat', row: 1, col: 8, name: 'Zona T', district: 'T' },
  { id: 'centro', row: 7, col: 8, name: 'Centro Internacional', district: 'I' },
  { id: 'candelaria', row: 10, col: 8, name: 'Chorro de Quevedo', district: 'L' },
  { id: 'aranda', row: 9, col: 2, name: 'Puente Aranda', district: 'A' },
];

// Kennedy 2089: manzanas de los barrios del occidente donde la red levantó torres colgantes [fila, columna]
W.HANGING_TOWERS = [[7, 4], [10, 3], [11, 6], [9, 5]];

// Batalla final
W.BOSS = { row: 9, col: 9, name: 'Don Monopolio', hp: 3000 };

W.ENEMY_TYPES = {
  inspector: { name: 'Dron escáner', hp: 60, speed: 4.4, melee: 10, color: '#6d7078', hat: 'fedora' },
  guardia: { name: 'Dron sellador', hp: 100, speed: 3.2, ranged: 9, range: 30, cooldown: 2.2, color: '#33363d', hat: 'beret' },
};

W.UPGRADES = [
  { id: 'dano', name: 'Presión turbo', desc: '+20 % de daño por nivel.', max: 3, cost: 20000 },
  { id: 'cargador', name: 'Cámara XL', desc: '+4 disparos por cargador.', max: 3, cost: 15000 },
  { id: 'vida', name: 'Armadura reforzada', desc: '+25 de vida máxima.', max: 3, cost: 18000 },
  { id: 'tenis', name: 'Tenis de barista', desc: '+8 % de velocidad.', max: 3, cost: 12000 },
  { id: 'cultivo', name: 'Hidroponía avanzada', desc: '+1 cereza por cosecha y crecen más rápido.', max: 3, cost: 16000 },
];

W.KO_LINES = ['Señal perdida', 'Batería crítica', 'Reportando a Arcadia…', 'Error 27-B'];
W.ENEMY_LINES = ['Café sin sello', 'Escaneando terraza', 'Cosecha sin licencia', 'Propiedad de Arcadia', 'Sello en curso'];
