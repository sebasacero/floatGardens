// La campaña: cuatro capítulos en Bogotá. Un barista con una AeroPress y un PC quiere ser un referente mundial del café
// en una ciudad donde solo se puede cultivar adentro, mientras Coffee Land decide quién puede vender café.
// Cada misión enseña algo real del café (el Códice) y abre regiones de Colombia (expediciones).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public enum MissionKind { Talk, Harvest, Sell, Capture, Brew, Boss }

    public class Line
    {
        public string who, text;
        public Line(string who, string text) { this.who = who; this.text = text; }
    }

    public class Mission
    {
        public string id, title;
        public MissionKind kind;
        public string npc = "", zone = "";
        public int count = 1;
        public float minScore;
        public Line[] intro = new Line[0], done = new Line[0];
        public string[] codex = new string[0];
        public int reward = 40;
    }

    public class Chapter
    {
        public string id, title, place;
        public Mission[] missions;
    }

    public class NPCDef
    {
        public string id, name, role, zone; // zone: "hq" o id de zona de FGConfig.Zones
        public Vector3 offset;
        public HumanoidStyle style;
        public string[] idle;
    }

    public class Story
    {
        readonly GameManager g;
        public readonly List<StoryNPC> npcs = new List<StoryNPC>();
        SaveData S => g.save;

        public Story(GameManager gm) { g = gm; }

        // ---------- Personajes ----------
        public static readonly NPCDef[] NPCs =
        {
            new NPCDef { id = "rosa", name = "Abuela Rosa", role = "chapolera de Chinchiná", zone = "hq", offset = new Vector3(6, 0.02f, -15),
                style = new HumanoidStyle { shirt = FGMaterials.Hex("#f2d8a6"), pants = FGMaterials.Hex("#5a3a2a"), hair = FGMaterials.Hex("#dddddd"), apron = FGMaterials.Hex("#c0392b") },
                idle = new[] { "Mijo, el café se recoge maduro, rojo como un corazón. Verde no sirve, pasado tampoco.", "En Chinchiná madrugábamos a las cuatro. Aquí usted madruga a prender los LED.", "¿Ya probó el tinto con panela? Así lo tomábamos en la finca." } },
            new NPCDef { id = "arango", name = "El Profe Arango", role = "agrónomo de Cenicafé", zone = "teusaquillo", offset = new Vector3(0, 0.02f, 11),
                style = new HumanoidStyle { shirt = FGMaterials.Hex("#ffffff"), pants = FGMaterials.Hex("#2d3a4a"), hair = FGMaterials.Hex("#3a2a1a"), tie = FGMaterials.Hex("#2fbf71") },
                idle = new[] { "Arábica: 44 cromosomas. Robusta: 22. Colombia solo siembra arábica, por algo será.", "La altura no es magia: es madurar despacio. Grano denso, taza compleja.", "Castillo le salvó la cosecha a este país. Respétela." } },
            new NPCDef { id = "kaldi", name = "Kaldi", role = "pastor de Kaffa (o eso dice)", zone = "simon", offset = new Vector3(-11, 0.02f, 0),
                style = new HumanoidStyle { shirt = FGMaterials.Hex("#e8dcc2"), pants = FGMaterials.Hex("#8a6a4a"), hair = FGMaterials.Hex("#1a1a1a"), sash = FGMaterials.Hex("#c0392b"), mustache = true },
                idle = new[] { "Mis cabras bailaron primero. Todo lo demás, los sufíes, Moca, París, vino después.", "Qahwa, kahve, caffè, café. La palabra viajó más que yo.", "Siete semillas en la ropa de un peregrino. Así se rompió el primer monopolio del café." } },
            new NPCDef { id = "francisco", name = "Padre Francisco", role = "cura de Salazar de las Palmas", zone = "candelaria", offset = new Vector3(0, 0.02f, 11),
                style = new HumanoidStyle { shirt = FGMaterials.Hex("#1a1a1a"), pants = FGMaterials.Hex("#1a1a1a"), skin = FGMaterials.Hex("#e0b58a"), hair = FGMaterials.Hex("#888888"), hat = "beret" },
                idle = new[] { "Tres matas de café por pecado venial. Por los otros, hablamos.", "Los jesuitas trajeron la semilla; la fe y el tinto hicieron el resto.", "En 1835 salieron 2.500 sacos por Cúcuta. Nadie imaginó lo que venía." } },
            new NPCDef { id = "catalina", name = "Catalina", role = "catadora Q, jueza de campeonatos", zone = "zonat", offset = new Vector3(0, 0.02f, 11),
                style = new HumanoidStyle { shirt = FGMaterials.Hex("#111111"), pants = FGMaterials.Hex("#222222"), hair = FGMaterials.Hex("#5a2a1a"), apron = FGMaterials.Hex("#f3f1ea") },
                idle = new[] { "Sorba con ruido. Sin pena. El café tiene que llegarle a la nariz.", "80 puntos es especialidad. 86 gana concursos. 90 casi no existe.", "Las tazas se juzgan a ciegas. El cuento del barista no suma puntos." } },
            new NPCDef { id = "alanita", name = "Alanita", role = "ingeniera, campeona de AeroPress", zone = "centro", offset = new Vector3(0, 0.02f, 11),
                style = new HumanoidStyle { shirt = FGMaterials.Hex("#22e8ff"), pants = FGMaterials.Hex("#2a2a35"), hair = FGMaterials.Hex("#ff2bd6"), cap = FGMaterials.Hex("#111111") },
                idle = new[] { "Un ingeniero de frisbees inventó la AeroPress porque quería una sola taza buena. Respeto.", "Ocho minutos, una taza, un jurado a ciegas. Así es el mundial.", "Invertida y con bloom. No me discuta." } },
        };

        // ---------- Capítulos ----------
        static Line L(string who, string text) => new Line(who, text);
        const string Yo = "Barista";

        public static readonly Chapter[] Chapters =
        {
            new Chapter { id = "c1", title = "Capítulo 1 · Una AeroPress y un PC", place = "Chapinero", missions = new[]
            {
                new Mission { id = "rosa1", title = "Habla con la Abuela Rosa", kind = MissionKind.Talk, npc = "rosa", codex = new[] { "floatgardens", "bogota" }, done = new[]
                {
                    L("Abuela Rosa", "Ahí está mi barista. ¿Sabe cuántas matas de café hay sembradas en Bogotá? Ninguna que madure. Dos mil seiscientos metros, catorce grados... el cafeto se muere de frío."),
                    L(Yo, "Por eso las raíces van en el agua, abuela. Adentro, con luz LED, en la bodega que nos prestaron. Y en las azoteas."),
                    L("Abuela Rosa", "Coffee Land dice que eso no es café, que el café de verdad lleva su sello. Yo recogí café cuarenta años en Chinchiná y nunca vi un sello madurar una cereza."),
                    L(Yo, "Voy a ser un referente del café, abuela. Desde aquí. Con esta AeroPress y ese computador."),
                    L("Abuela Rosa", "Entonces empiece por lo primero: cosechar. Las balsas de la piscina ya tienen cereza roja. Rojo como un corazón, ¿oyó? Verde no sirve."),
                } },
                new Mission { id = "cosecha1", title = "Cosecha 8 cerezas en las balsas del HQ", kind = MissionKind.Harvest, count = 8, reward = 50, codex = new[] { "cereza", "hidroponia", "sombra" }, done = new[]
                {
                    L("Abuela Rosa", "Mire esa cereza: piel, pulpa dulce, el mucílago pegajoso, el pergamino y adentro dos semillas. Ocho meses para llegar ahí."),
                    L(Yo, "En el agua con nutrientes crecen parejo. Y gastamos una décima parte del agua de una finca."),
                    L("Abuela Rosa", "Bonito. Ahora véndalas: el café que nadie toma es adorno."),
                } },
                new Mission { id = "venta1", title = "Vende 3 cafés a los vecinos de Chapinero", kind = MissionKind.Sell, count = 3, reward = 60, codex = new[] { "tinto", "aeropress" }, intro = new[]
                {
                    L("Abuela Rosa", "Los vecinos con taza en la mano quieren tinto. Pero ojo: solo le compran en barrios donde Coffee Land no manda. Por ahora, Chapinero."),
                    L(Yo, "Dos cerezas, una AeroPress, treinta segundos de presión. Vamos."),
                }, done = new[]
                {
                    L("Abuela Rosa", "¿Vio la cara de la señora? Eso no lo da un sello. Y esa AeroPress suya... ¿sabía que la inventó un señor que hacía frisbees?"),
                    L(Yo, "Alan Adler, 2005. Quería una sola taza buena, sin amargor. Yo quiero lo mismo, pero para toda la ciudad."),
                    L("Abuela Rosa", "Pues vaya prenda ese computador y aprenda a medir. El café bueno no es suerte: es receta."),
                } },
                new Mission { id = "lab1", title = "Prepara un café de 74 puntos o más en tu PC (Laboratorio AeroPress)", kind = MissionKind.Brew, minScore = 74, reward = 80, codex = new[] { "variables", "molienda", "extraccion" }, done = new[]
                {
                    L("Abuela Rosa", "Setenta y pico de puntos con una receta anotada. Mi mamá preparaba de memoria y nunca le salía igual dos veces."),
                    L(Yo, "Proporción, molienda, temperatura, tiempo y método. Cambio una a la vez y lo anoto todo. Así se llega a ochenta."),
                    L("Abuela Rosa", "Ochenta es especialidad, dicen los catadores. Para eso le falta hablar con los que saben. En el parque de Teusaquillo hay un profe de Cenicafé que no se calla."),
                } },
            } },
            new Chapter { id = "c2", title = "Capítulo 2 · Los que saben", place = "Teusaquillo, Simón Bolívar y La Candelaria", missions = new[]
            {
                new Mission { id = "arango1", title = "Busca al Profe Arango en el parque de Teusaquillo", kind = MissionKind.Talk, npc = "arango", codex = new[] { "arabica", "altura" }, done = new[]
                {
                    L("El Profe Arango", "¿Usted es el de las raíces en el agua? Siéntese. Primera lección: hay dos especies. Arábica, de altura, aromática. Robusta, de tierra caliente, amarga y con el doble de cafeína."),
                    L(Yo, "Colombia siembra solo arábica."),
                    L("El Profe Arango", "Y la siembra entre 1.200 y 2.000 metros porque arriba la cereza madura despacio y el grano sale denso, lleno de azúcares. Usted en Bogotá tiene la altura pero no la temperatura. Por eso sus LED."),
                    L("El Profe Arango", "Ahora, los inspectores de Coffee Land me cerraron el vivero del parque. Si quiere la segunda lección, recupere este parque."),
                } },
                new Mission { id = "cap_teusaquillo", title = "Captura el Parque Teusaquillo", kind = MissionKind.Capture, zone = "teusaquillo", reward = 120, codex = new[] { "cenicafe", "roya", "paisaje", "chapoleras" }, done = new[]
                {
                    L("El Profe Arango", "Segunda lección, entonces. En 2008 la roya, un hongo, nos tumbó la cosecha. Cenicafé, en Chinchiná, llevaba décadas cruzando variedades: Colombia en el 82, Tabi en 2002, Castillo en 2005."),
                    L(Yo, "Y el país renovó millones de cafetos con Castillo."),
                    L("El Profe Arango", "Resistente y con buena taza. Lléveselo al laboratorio. Y si de verdad quiere entender de dónde viene todo esto, hay un señor raro en el lago del Simón Bolívar que dice ser un pastor etíope."),
                } },
                new Mission { id = "kaldi1", title = "Encuentra a Kaldi junto al lago del Simón Bolívar", kind = MissionKind.Talk, npc = "kaldi", codex = new[] { "kaldi", "yemen", "babaBudan", "deClieu" }, done = new[]
                {
                    L("Kaldi", "Mis cabras comieron las cerezas rojas de un arbusto de Kaffa y bailaron toda la noche. Yo las probé. Desde entonces nadie en el mundo duerme tranquilo."),
                    L(Yo, "Etiopía. Después Yemen."),
                    L("Kaldi", "Los sufíes lo tostaron y lo bebieron para rezar despiertos. Por el puerto de Moca salió durante doscientos años, y Yemen no dejaba salir ni una semilla fértil. Hasta que Baba Budan escondió siete en su ropa."),
                    L("Kaldi", "Luego un marino francés, De Clieu, compartió su agua con una matica en medio del Atlántico. De esa planta descienden casi todos los cafetos de América. Incluido el suyo."),
                    L("Kaldi", "Coffee Land cree que inventó el café. Nadie lo inventó: se compartió. Recupere este parque y le cuento lo que pasó en Europa."),
                } },
                new Mission { id = "cap_simon", title = "Captura el lago del Simón Bolívar", kind = MissionKind.Capture, zone = "simon", reward = 120, codex = new[] { "papa", "londres", "bach", "boston" }, done = new[]
                {
                    L("Kaldi", "En Europa quisieron prohibirlo por ser bebida de infieles. El papa lo probó y lo bautizó. En Londres, por un penique, tomabas café con científicos: las universidades de un penique."),
                    L("Kaldi", "Bach le escribió una cantata. Beethoven contaba sesenta granos. Y cuando en Boston tiraron el té al mar, Estados Unidos se volvió país de café. Su mejor cliente, por cierto."),
                    L(Yo, "Y a Colombia, ¿cómo llegó?"),
                    L("Kaldi", "Eso pregúnteselo a un cura. Hay uno en La Candelaria, por el Chorro de Quevedo, que pone penitencias raras."),
                } },
                new Mission { id = "francisco1", title = "Visita al Padre Francisco en el Chorro de Quevedo", kind = MissionKind.Talk, npc = "francisco", codex = new[] { "jesuitas", "romero" }, done = new[]
                {
                    L("Padre Francisco", "Los jesuitas trajeron el café a la Nueva Granada a comienzos del siglo XVIII. Pero sembrarlo era lento, y la gente perezosa. Así que en Salazar de las Palmas se me ocurrió algo."),
                    L(Yo, "La penitencia."),
                    L("Padre Francisco", "Tres o cuatro matas por pecado. Confesión a confesión se llenaron las montañas de Norte de Santander. En 1835 salieron por Cúcuta los primeros 2.500 sacos. Todo empezó con pecadores y semillas."),
                    L("Padre Francisco", "Coffee Land puso una caseta de inspección frente a mi iglesia. Quítemela de encima y le cuento cómo se organizaron los caficultores."),
                } },
                new Mission { id = "cap_candelaria", title = "Captura el Chorro de Quevedo", kind = MissionKind.Capture, zone = "candelaria", reward = 120, codex = new[] { "fnc", "mitaca", "lavado" }, done = new[]
                {
                    L("Padre Francisco", "En 1927 los caficultores se juntaron en Medellín y fundaron una federación para defender el precio e investigar. De ahí salió el laboratorio de Chinchiná y el arriero con la mula que el mundo conoce."),
                    L(Yo, "Una federación de campesinos. Lo contrario de Coffee Land."),
                    L("Padre Francisco", "Exacto, muchacho: la idea era buena, el problema es cuando alguien la convierte en sello y candado. Usted siga: dos cosechas al año, café lavado, recogido a mano. Eso es lo que defiende."),
                    L("Padre Francisco", "Para el siguiente paso necesita una lengua entrenada. En la Zona T hay una catadora que juzga campeonatos. No le va a gustar su café. Todavía."),
                } },
            } },
            new Chapter { id = "c3", title = "Capítulo 3 · Taza de Excelencia", place = "Zona T y Centro Internacional", missions = new[]
            {
                new Mission { id = "catalina1", title = "Preséntate ante Catalina, la catadora Q, en la Zona T", kind = MissionKind.Talk, npc = "catalina", codex = new[] { "sca", "catacion", "notas" }, done = new[]
                {
                    L("Catalina", "Así que el barista hidropónico. Le explico cómo funciona esto: cien puntos. Fragancia, sabor, acidez, cuerpo, balance, dulzor, taza limpia, posgusto. Ochenta es especialidad."),
                    L(Yo, "¿Y cómo se cata?"),
                    L("Catalina", "8,25 gramos por 150 mililitros a 93 grados. Cuatro minutos. Se rompe la costra, se huele, y se sorbe con ruido para que llegue a la nariz. A ciegas. Su historia no suma puntos; su taza sí."),
                    L("Catalina", "Vaya a su laboratorio y tráigame una taza de 84. Sin excusas. Si no sabe a qué sabe, use la rueda de sabores: frutal, floral, dulce, nuez, especias."),
                } },
                new Mission { id = "lab2", title = "Prepara un café de 84 puntos o más (Taza de Excelencia)", kind = MissionKind.Brew, minScore = 84, reward = 150, codex = new[] { "geisha", "agua", "natural", "invertido" }, done = new[]
                {
                    L("Catalina", "Ochenta y cuatro. Dulce, limpio, con acidez. No me lo esperaba. ¿Qué agua usó?"),
                    L(Yo, "La de Bogotá, que es blanda. Y bajé la temperatura para la variedad."),
                    L("Catalina", "Bien. El agua es el 98 % de la taza. Ahora, si quiere competir de verdad, hay una ingeniera en el Centro Internacional que fue campeona de AeroPress. Es insoportable y tiene razón en todo."),
                } },
                new Mission { id = "alanita1", title = "Busca a Alanita en el Centro Internacional", kind = MissionKind.Talk, npc = "alanita", codex = new[] { "wac", "melitta", "espresso", "chemex" }, done = new[]
                {
                    L("Alanita", "El primer mundial de AeroPress fue en Oslo, 2008, con tres personas en una tostaduría. Hoy compiten decenas de países: ocho minutos, una taza, tres jueces que señalan la ganadora a la vez."),
                    L(Yo, "Quiero llegar ahí. Representando a Bogotá. Con café cultivado aquí."),
                    L("Alanita", "Entonces aprenda historia de métodos: Melitta y el filtro de papel en 1908, el espresso de Moriondo y Bezzera, la Chemex del 41. La AeroPress es la hija de todos. Invertida y con bloom, no me discuta."),
                    L("Alanita", "Pero hay un problema: Coffee Land tiene la Zona T y el Centro sitiados. Sin esos barrios no hay público ni cafés donde servir. Libérelos y le entreno."),
                } },
                new Mission { id = "cap_zonat", title = "Captura la Zona T", kind = MissionKind.Capture, zone = "zonat", reward = 140, codex = new[] { "pergamino" }, done = new[]
                {
                    L("Catalina", "Zona T libre. Ya hay tres cafés sirviendo lo suyo. Falta el Centro."),
                } },
                new Mission { id = "cap_centro", title = "Captura el Centro Internacional", kind = MissionKind.Capture, zone = "centro", reward = 140, codex = new[] { "tueste", "precio" }, done = new[]
                {
                    L("Alanita", "Listo. Ahora sí: el tueste. Primer crack a 196 grados, segundo a 224. Usted quiere parar entre los dos, claro, para que la fruta siga ahí."),
                    L(Yo, "Y el precio lo pone la bolsa de Nueva York, en centavos por libra. Pero un café de especialidad se vende por su nombre."),
                    L("Alanita", "Por su nombre y su puntaje. Coffee Land vive de la bolsa; usted va a vivir de la taza. Les queda Puente Aranda y la torre. Vaya a cerrar esto."),
                } },
            } },
            new Chapter { id = "c4", title = "Capítulo 4 · Coffee Land", place = "Puente Aranda y Plaza de Bolívar", missions = new[]
            {
                new Mission { id = "cap_aranda", title = "Captura Puente Aranda, el centro logístico de Coffee Land", kind = MissionKind.Capture, zone = "aranda", reward = 160, codex = new[] { "coffeeland", "huila" }, intro = new[]
                {
                    L("Abuela Rosa", "Mijo, Coffee Land movió los camiones a Puente Aranda. Desde las bodegas controlan todo lo que entra y sale de la ciudad. Si recupera eso, Don Monopolio se queda sin sello que poner."),
                }, done = new[]
                {
                    L("Abuela Rosa", "Bodegas llenas de café de Huila, de Nariño, del Cauca... retenido por no tener sello. Repártalo, mijo. Ese café ya tenía nombre antes de que llegaran ellos."),
                } },
                new Mission { id = "boss", title = "Enfrenta a Don Monopolio, CEO de Coffee Land, en la Plaza de Bolívar", kind = MissionKind.Boss, reward = 300, codex = new[] { "narino", "sierra" }, intro = new[]
                {
                    L("Catalina", "Don Monopolio salió de la torre. Dice que va a decomisar hasta la última AeroPress de la ciudad."),
                    L("Alanita", "Hoy es su final, barista. Ocho minutos. Una taza. Vaya."),
                }, done = new[]
                {
                    L("Don Monopolio", "...Esto no es café. No tiene sello."),
                    L(Yo, "Pruébelo."),
                    L("Don Monopolio", "..."),
                    L("Don Monopolio", "Mandarina. Panela. ¿Cómo hizo eso en una bodega?"),
                    L("Abuela Rosa", "Con raíces en el agua y gente que ayudó, Don Monopolio. Se llama compartir. Como las siete semillas de Baba Budan."),
                    L("Alanita", "Y ahora, barista, el mundo. Oslo, Melbourne, Tokio, Addis Abeba. Pero primero: las expediciones por Colombia. Vaya a su PC."),
                } },
            } },
        };

        // ---------- Estado ----------
        public bool Finished => S.chapter >= Chapters.Length;
        public Chapter CurrentChapter => Finished ? null : Chapters[S.chapter];
        public Mission Current => Finished ? null : CurrentChapter.missions[Mathf.Min(S.mission, CurrentChapter.missions.Length - 1)];

        public void Spawn(Transform parent)
        {
            foreach (var def in NPCs)
            {
                Vector3 basePos = def.zone == "hq" ? g.city.hqCenter : g.city.zoneCenters[def.zone];
                Vector3 pos = basePos + def.offset;
                pos.y = City.BaseY(pos) + 0.02f;
                npcs.Add(StoryNPC.Spawn(def, pos, parent));
            }
            RefreshMarkers();
        }

        /// <summary>Al entrar a jugar: si la misión activa tiene presentación pendiente, la muestra.</summary>
        public void OnPlay()
        {
            if (Finished) return;
            var m = Current;
            if (!S.introSeen && m.intro.Length > 0)
            {
                S.introSeen = true;
                Dialogue(m.intro, null);
            }
            CheckInstant();
            RefreshMarkers();
        }

        // Misiones que ya se cumplieron antes de activarse (zona capturada en una partida vieja, jefe vencido)
        void CheckInstant()
        {
            var m = Current;
            if (m == null) return;
            if (m.kind == MissionKind.Capture && S.captured.Contains(m.zone)) Complete();
            else if (m.kind == MissionKind.Boss && S.bossDefeated) Complete();
        }

        public void OnEvent(MissionKind kind, int n = 1, string id = null, float score = 0)
        {
            if (Finished || g.State == GameManager.GameState.Dialogue) return;
            var m = Current;
            if (m.kind != kind) return;
            switch (kind)
            {
                case MissionKind.Harvest:
                case MissionKind.Sell:
                    S.missionProgress += n;
                    g.hud.Toast(m.title.Split('(')[0].Trim() + " · " + Mathf.Min(S.missionProgress, m.count) + "/" + m.count);
                    if (S.missionProgress >= m.count) Complete();
                    break;
                case MissionKind.Capture:
                    if (id == m.zone) Complete();
                    break;
                case MissionKind.Brew:
                    if (score >= m.minScore) Complete();
                    break;
                case MissionKind.Boss:
                    Complete();
                    break;
            }
        }

        /// <summary>El jugador habla con un personaje. Si es el de la misión, la cumple; si no, dice algo suyo.</summary>
        public void Talk(StoryNPC npc)
        {
            var m = Current;
            if (m != null && m.kind == MissionKind.Talk && m.npc == npc.def.id) { Complete(); return; }
            string line = npc.def.idle[UnityEngine.Random.Range(0, npc.def.idle.Length)];
            Dialogue(new[] { L(npc.def.name, line) }, null);
        }

        void Complete()
        {
            var m = Current;
            foreach (var c in m.codex) UnlockCodex(c, false);
            g.AddPoints(m.reward);
            g.hud.Toast("Misión cumplida: +" + m.reward + " ★ · Códice actualizado");
            SFX.Play("capture");
            Dialogue(m.done, Advance);
        }

        void Advance()
        {
            S.mission++;
            S.missionProgress = 0;
            S.introSeen = false;
            if (S.mission >= CurrentChapter.missions.Length)
            {
                S.chapter++;
                S.mission = 0;
                if (Finished)
                {
                    g.hud.Banner("FIN DEL CAPÍTULO 4", "Bogotá cultiva su propio café. Las expediciones por Colombia están abiertas.", true);
                    UnlockRegions();
                }
                else
                {
                    g.hud.Banner(CurrentChapter.title.ToUpperInvariant(), CurrentChapter.place, true);
                    UnlockRegions();
                }
            }
            g.Persist();
            if (!Finished)
            {
                var next = Current;
                if (next.intro.Length > 0 && g.State == GameManager.GameState.Playing)
                {
                    S.introSeen = true;
                    Dialogue(next.intro, null);
                }
                else g.hud.Toast("Nueva misión: " + next.title);
                CheckInstant();
            }
            RefreshMarkers();
        }

        void UnlockRegions()
        {
            foreach (var r in Codex.Regions)
                if (r.chapter <= S.chapter && !S.regions.Contains(r.id))
                {
                    S.regions.Add(r.id);
                    foreach (var c in r.codex) UnlockCodex(c, false);
                    foreach (var v in r.varieties) if (!S.varieties.Contains(v)) S.varieties.Add(v);
                    g.hud.KillFeed("<color=#7dffb0>Expedición abierta:</color> " + r.name);
                }
        }

        public void UnlockCodex(string id, bool announce)
        {
            if (Codex.Get(id) == null || S.codex.Contains(id)) return;
            S.codex.Add(id);
            if (announce) g.hud.Toast("Códice: " + Codex.Get(id).title);
        }

        void Dialogue(Line[] lines, Action then)
        {
            if (lines == null || lines.Length == 0) { then?.Invoke(); return; }
            g.StartDialogue(lines, then);
        }

        void RefreshMarkers()
        {
            var m = Current;
            foreach (var n in npcs) n.SetMarker(m != null && m.kind == MissionKind.Talk && m.npc == n.def.id);
        }

        public GameManager.Objective_ Objective()
        {
            var m = Current;
            var o = new GameManager.Objective_ { title = m.title, sub = CurrentChapter.title };
            switch (m.kind)
            {
                case MissionKind.Talk:
                    var npc = npcs.Find(n => n.def.id == m.npc);
                    if (npc != null) { o.target = npc.transform.position; o.hasTarget = true; }
                    break;
                case MissionKind.Harvest:
                    o.sub += " · " + Mathf.Min(S.missionProgress, m.count) + "/" + m.count;
                    o.target = g.city.hqCenter; o.hasTarget = true;
                    break;
                case MissionKind.Sell:
                    o.sub += " · " + Mathf.Min(S.missionProgress, m.count) + "/" + m.count;
                    break;
                case MissionKind.Capture:
                    var z = g.zones.Find(x => x.def.id == m.zone);
                    if (z != null) { o.target = z.transform.position; o.hasTarget = true; }
                    break;
                case MissionKind.Brew:
                    o.target = g.city.hqLab; o.hasTarget = true;
                    break;
                case MissionKind.Boss:
                    o.target = g.city.bossArena; o.hasTarget = true;
                    break;
            }
            return o;
        }

        /// <summary>Variedades disponibles en el laboratorio (siempre al menos Castillo).</summary>
        public List<Codex.Variety> AvailableVarieties()
        {
            var list = new List<Codex.Variety>();
            if (!S.varieties.Contains("castillo")) S.varieties.Insert(0, "castillo");
            foreach (var id in S.varieties) { var v = Codex.GetVariety(id); if (!list.Contains(v)) list.Add(v); }
            return list;
        }
    }
}
