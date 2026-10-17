// Float Gardens: La Conquista del Café — configuración del juego.
// Todos los valores de balance están aquí para poder ajustarlos sin tocar la lógica.
window.FG = window.FG || {};

FG.CONFIG = {
  WIDTH: 960,
  HEIGHT: 600,
  HUD_HEIGHT: 56,

  LEVEL_TIME: 180,          // segundos por ciudad
  START_CONQUEST: 10,       // % de conquista inicial
  BOSS_AT: 70,              // % en el que aparece el jefe
  CAP_WITHOUT_BOSS: 90,     // tope de conquista mientras el jefe siga en pie

  STAGE_TIME: 6,            // segundos por etapa de crecimiento (brote -> flor -> cereza)
  WITHER_TIME: 7,           // segundos que una planta queda decomisada
  BASE_YIELD: 3,            // cerezas por cosecha
  START_CHERRIES: 4,
  CHERRIES_PER_BREW: 2,
  MAX_CUPS: 6,
  BASE_BREW_TIME: 1.6,

  NUTRIENT_DRAIN: 0.55,     // % por segundo (+ un extra por planta)
  NUTRIENT_DRAIN_PER_PLANT: 0.07,

  BASE_SPEED: 190,
  BASE_SHOT_COOLDOWN: 0.45,
  SHOT_SPEED: 430,

  MAX_QUEUE: 5,

  // Conquista
  SERVE_CONQUEST: 7,
  FAST_SERVE_BONUS: 2,
  LOST_CUSTOMER: -4,
  CONFISCATED_PLANT: -3,
  INSPECTOR_REPELLED: 2,
  BOSS_DEFEATED: 15,

  // Puntos Float (el programa de lealtad: se canjean por café gratis)
  POINTS_PER_CHERRY: 1,
  POINTS_PER_SERVE: 10,
  POINTS_PER_INSPECTOR: 5,
  POINTS_PER_BOSS: 100,
  POINTS_PER_CITY: 200,     // solo la primera vez que se conquista cada ciudad
  REDEEM_COST: 1500,        // Puntos Float para un café gratis (~3-5 partidas)

  BASE_PRICE: 6000,         // COP por taza
};

// Ruta de conquista: de Chapinero al mundo... y de vuelta a la sede de la Federación.
FG.CITIES = [
  {
    id: 'bogota', name: 'Bogotá', district: 'Chapinero',
    accent: '#2fbf71', road: '#3a3f47', floor: '#c99a6a',
    priceMul: 1.0, customerEvery: 6.0, patience: 28,
    enemyEvery: 11, enemyFirst: 14, maxEnemies: 2, enemySpeed: 48, enemyHp: 3,
    boss: 'El Gran Inspector', bossHp: 22,
    intro: 'Chapinero: aquí nació la idea. ¡Abre tu primer Float Gardens!',
  },
  {
    id: 'medellin', name: 'Medellín', district: 'El Poblado',
    accent: '#f2a541', road: '#3d3a40', floor: '#c48f5e',
    priceMul: 1.1, customerEvery: 5.4, patience: 26,
    enemyEvery: 9.5, enemyFirst: 11, maxEnemies: 3, enemySpeed: 54, enemyHp: 3,
    boss: 'La Comisaria del Tinto', bossHp: 28,
    intro: 'La eterna primavera: perfecta para cultivar café flotante.',
  },
  {
    id: 'cartagena', name: 'Cartagena', district: 'Getsemaní',
    accent: '#e85d75', road: '#46403b', floor: '#d1a374',
    priceMul: 1.25, customerEvery: 5.0, patience: 24,
    enemyEvery: 8.5, enemyFirst: 10, maxEnemies: 3, enemySpeed: 58, enemyHp: 4,
    boss: 'El Almirante del Grano', bossHp: 32,
    intro: 'Turistas por montones. ¡Que prueben café hidropónico!',
  },
  {
    id: 'nyc', name: 'Nueva York', district: 'Brooklyn',
    accent: '#47b5e0', road: '#2f3338', floor: '#b8875a',
    priceMul: 1.6, customerEvery: 4.5, patience: 22,
    enemyEvery: 7.5, enemyFirst: 9, maxEnemies: 4, enemySpeed: 62, enemyHp: 4,
    boss: 'El Agregado Comercial', bossHp: 38,
    intro: 'Primera sede internacional. La Federación tiene oficina aquí...',
  },
  {
    id: 'paris', name: 'París', district: 'Le Marais',
    accent: '#9b7fe6', road: '#353440', floor: '#c2946a',
    priceMul: 1.8, customerEvery: 4.2, patience: 20,
    enemyEvery: 7, enemyFirst: 8, maxEnemies: 4, enemySpeed: 66, enemyHp: 5,
    boss: 'Madame Certificación', bossHp: 44,
    intro: 'Los parisinos aman la AeroPress. ¡Conquista el café de la esquina!',
  },
  {
    id: 'tokio', name: 'Tokio', district: 'Shibuya',
    accent: '#ff6f61', road: '#2b2e36', floor: '#bf9165',
    priceMul: 2.0, customerEvery: 3.8, patience: 19,
    enemyEvery: 6.2, enemyFirst: 7, maxEnemies: 5, enemySpeed: 70, enemyHp: 5,
    boss: 'El Sensei del Monopolio', bossHp: 50,
    intro: 'Precisión japonesa. Cada segundo de extracción cuenta.',
  },
  {
    id: 'sede', name: 'Bogotá', district: 'La Sede de la Federación',
    accent: '#d6423a', road: '#26282d', floor: '#a97c55',
    priceMul: 2.2, customerEvery: 3.5, patience: 18,
    enemyEvery: 5.5, enemyFirst: 6, maxEnemies: 6, enemySpeed: 74, enemyHp: 6,
    boss: 'Don Monopolio', bossHp: 70,
    intro: 'La batalla final: de vuelta a casa, frente a la sede de la Federación.',
  },
];

FG.UPGRADES = [
  { id: 'torres',      icon: '🌱', name: 'Torre hidropónica',      max: 3, cost: 15000,
    desc: '+2 plantas de café flotante en el invernadero.' },
  { id: 'nutrientes',  icon: '💧', name: 'Solución nutritiva premium', max: 3, cost: 12000,
    desc: 'Las plantas crecen un 20% más rápido por nivel. Nivel 2+: +1 cereza por cosecha.' },
  { id: 'aeropress',   icon: '☕', name: 'AeroPress pro',          max: 3, cost: 18000,
    desc: 'Preparas más rápido. Desde el nivel 2, cada prensada da 2 tazas.' },
  { id: 'presion',     icon: '💥', name: 'Presión turbo',          max: 3, cost: 14000,
    desc: 'Disparas chorros de café más seguido. Nivel 3: doble daño.' },
  { id: 'tenis',       icon: '👟', name: 'Tenis de barista',       max: 3, cost: 10000,
    desc: 'Te mueves un 12% más rápido por nivel.' },
  { id: 'colaborador', icon: '🤝', name: 'Cultivador colaborativo', max: 2, cost: 25000,
    desc: 'Un miembro de la comunidad cosecha por ti automáticamente.' },
  { id: 'marketing',   icon: '📣', name: 'Banners de promoción',   max: 3, cost: 16000,
    desc: 'Llegan un 15% más clientes por nivel.' },
];

// Puntos oficiales donde se canjean los Puntos Float. Edita esta lista con las
// direcciones reales antes de publicar el juego.
FG.OFFICIAL_POINTS = [
  { name: 'Float Gardens — Punto oficial', city: 'Bogotá', address: 'Dirección por confirmar' },
];

// Posiciones de las balsas flotantes (en orden de desbloqueo).
FG.PLANT_SLOTS = [
  { x: 85, y: 135 }, { x: 185, y: 135 }, { x: 285, y: 135 },
  { x: 85, y: 245 }, { x: 185, y: 245 }, { x: 285, y: 245 },
  { x: 385, y: 135 }, { x: 385, y: 245 },
  { x: 85, y: 355 }, { x: 185, y: 355 },
  { x: 285, y: 355 }, { x: 385, y: 355 },
];

FG.CHANNEL_ROWS = [135, 245, 355];

FG.STATIONS = {
  tank:     { x: 488, y: 250, r: 55, label: 'Recargar nutrientes' },
  aeropress:{ x: 640, y: 150, r: 55, label: 'Preparar café' },
  counter:  { x: 600, y: 222, w: 300, h: 30, label: 'Servir' },
  door:     { x: 975, y: 305 },
};

FG.CUSTOMER_TYPES = [
  { name: 'Estudiante',       shirt: '#5b8def', pants: '#2d3a5a', hair: '#2b1b12', payMul: 0.9, patienceMul: 1.1 },
  { name: 'Oficinista',       shirt: '#e8e8e8', pants: '#3b3b46', hair: '#4a3020', payMul: 1.0, patienceMul: 0.9, tie: '#c0392b' },
  { name: 'Turista',          shirt: '#f4c542', pants: '#6b8e4e', hair: '#d9b36c', payMul: 1.3, patienceMul: 1.0, hat: '#f0e3c0' },
  { name: 'Hipster cafetero', shirt: '#7a5c99', pants: '#222', hair: '#1a1a1a', payMul: 1.5, patienceMul: 0.8, beard: true },
];

FG.INSPECTOR_LINES = [
  '¡Café sin sello, café decomisado!',
  '¿Hidro-qué? ¡Eso no es tradición!',
  '¡Multa por innovar!',
  '¡Ese café no tiene permiso!',
  '¡Aquí manda la Federación!',
  'Mejor tómese un tinto federado.',
  '¡Formulario 27-B, por triplicado!',
];
