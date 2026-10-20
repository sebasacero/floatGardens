// La historia: capítulos, misiones, competencias, rivales, correos, diálogos y preguntas de los retos.
// Personajes ficticios. Los datos sobre el café vienen de la investigación del juego (ver LoreData).
using System.Collections.Generic;

namespace FloatGardens.Historia
{
    public static class StoryData
    {
        const string J = "{jugador}";
        const string N = "Narrador";
        const string V = "Valentina";

        /// <summary>Líneas de diálogo en formato "Personaje|texto".</summary>
        static DialogLine[] D(params string[] lines)
        {
            var r = new DialogLine[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                int k = lines[i].IndexOf('|');
                r[i] = k < 0 ? new DialogLine(N, lines[i]) : new DialogLine(lines[i].Substring(0, k), lines[i].Substring(k + 1));
            }
            return r;
        }

        static QuizQuestion Q(string q, int correct, string explanation, string wiki, params string[] options)
            => new QuizQuestion { question = q, options = options, correct = correct, explanation = explanation, wiki = wiki };

        // ============================ CAPÍTULOS ============================

        public static readonly List<Chapter> Chapters = new List<Chapter>
        {
            new Chapter { number = 0, title = "Prólogo", subtitle = "2.600 metros más cerca de las estrellas", introDialog = "cap0_intro", firstMission = "m0_tinto" },
            new Chapter { number = 1, title = "La semilla", subtitle = "Bogotá y Cundinamarca", introDialog = "cap1_intro", firstMission = "m1_centro" },
            new Chapter { number = 2, title = "El Eje Cafetero", subtitle = "Quindío, Risaralda y Caldas", introDialog = "cap2_intro", firstMission = "m2_quindio" },
            new Chapter { number = 3, title = "El Sur", subtitle = "Huila, Tolima, Cauca y Nariño", introDialog = "cap3_intro", firstMission = "m3_huila" },
            new Chapter { number = 4, title = "Raíces", subtitle = "Antioquia, los Santanderes y la Sierra Nevada", introDialog = "cap4_intro", firstMission = "m4_antioquia" },
            new Chapter { number = 5, title = "El Mundo", subtitle = "De Bogotá al mundo", introDialog = "cap5_intro", firstMission = "m5_invitacion" },
        };

        // ============================ MISIONES ============================

        public static readonly List<Mission> Missions = new List<Mission>
        {
            // ---- Prólogo ----
            new Mission { id = "m0_tinto", chapter = 0, title = "Tu primera AeroPress", desc = "Ve a la barra de la bodega y prepara una AeroPress con el café de La Candelaria.",
                evt = HEvents.Brew, startDialog = "m0_tinto_start", endDialog = "m0_tinto_end", rewardWiki = new[] { "bog_altitud_ebullicion", "pre_aeropress_metodos" }, next = "m0_publicar" },
            new Mission { id = "m0_publicar", chapter = 0, title = "Abre acero-press", desc = "Prepara otra taza y publícala en acero-press con el botón Publicar.",
                evt = HEvents.Publish, startDialog = "m0_publicar_start", endDialog = "m0_publicar_end", rewardWiki = new[] { "hist_aeropress_2005" }, next = "m0_sembrar" },
            new Mission { id = "m0_sembrar", chapter = 0, title = "La primera semilla flotante", desc = "En la huerta flotante, siembra tu semilla de Caturra en una balsa.",
                evt = HEvents.Plant, startDialog = "m0_sembrar_start", endDialog = "m0_sembrar_end", rewardWiki = new[] { "cul_hidroponia", "cul_fosforito_chapola" }, next = "m0_dormir" },
            new Mission { id = "m0_dormir", chapter = 0, title = "Mañana será otro día", desc = "Duerme en el rincón de la bodega: las matas crecen mientras descansas.",
                evt = HEvents.Sleep, startDialog = "m0_dormir_start", endDialog = "m0_dormir_end", rewardMoney = 50000, rewardWiki = new[] { "bog_2600", "cul_clima_ideal" }, rewardFlags = new[] { "f_cap1" } },

            // ---- Capítulo 1 ----
            new Mission { id = "m1_centro", chapter = 1, title = "El centro histórico", desc = "Sal por el portón, abre el mapa y viaja a Bogotá (centro). Doña Inés te espera en el Café La Tertulia.",
                evt = HEvents.Visit, arg = "bogota", startDialog = "m1_centro_start", endDialog = "m1_centro_end", next = "m1_historia" },
            new Mission { id = "m1_historia", chapter = 1, title = "La lección de Doña Inés", desc = "Habla con Doña Inés y supera su reto de historia del café.",
                evt = HEvents.Reto, arg = "bogota", target = 60, threshold = true, endDialog = "m1_historia_end", rewardMoney = 40000, rewardFlags = new[] { "f_ines" }, next = "m1_viota" },
            new Mission { id = "m1_viota", chapter = 1, title = "Rumbo a Cundinamarca", desc = "Desde el mapa, toma el bus a Viotá (Cundinamarca), a unas 3 horas bajando de la sabana.",
                evt = HEvents.Visit, arg = "cundinamarca", startDialog = "m1_viota_start", next = "m1_lavado" },
            new Mission { id = "m1_lavado", chapter = 1, title = "El lavado clásico", desc = "Don Aurelio te enseña el beneficio de la hacienda: logra un lavado perfecto.",
                evt = HEvents.Reto, arg = "cundinamarca", target = 60, threshold = true, endDialog = "m1_lavado_end", rewardMoney = 60000, rewardFlags = new[] { "f_copa" }, next = "m1_receta" },
            new Mission { id = "m1_receta", chapter = 1, title = "Afina tu receta", desc = "Logra una AeroPress de 80 puntos o más. Usa el agua más caliente posible en Bogotá (~91 °C) y ajusta molienda y tiempo.",
                evt = HEvents.Brew, target = 80, threshold = true, startDialog = "m1_receta_start", endDialog = "m1_receta_end", rewardWiki = new[] { "pre_carta_control", "pre_molienda" }, next = "m1_copa" },
            new Mission { id = "m1_copa", chapter = 1, title = "Copa Bogotá de AeroPress", desc = "Inscríbete desde el Correo del computador y gana la Copa Bogotá (2 rondas).",
                evt = HEvents.Competition, arg = "copa_bogota", target = 1, threshold = true, startDialog = "m1_copa_start", rewardFlags = new[] { "f_cap2" } },

            // ---- Capítulo 2 ----
            new Mission { id = "m2_quindio", chapter = 2, title = "Corazón del Paisaje Cultural", desc = "Viaja al Quindío (Salento) y busca a Doña Rosalba en su finca.",
                evt = HEvents.Visit, arg = "quindio", startDialog = "m2_quindio_start", next = "m2_recoleccion" },
            new Mission { id = "m2_recoleccion", chapter = 2, title = "Grano a grano", desc = "Supera el reto de recolección selectiva de Doña Rosalba: solo cerezas maduras.",
                evt = HEvents.Reto, arg = "quindio", target = 60, threshold = true, rewardMoney = 50000, next = "m2_caldas" },
            new Mission { id = "m2_caldas", chapter = 2, title = "La ciencia del café", desc = "Viaja a Chinchiná (Caldas) y supera el reto de plagas de la Dra. Camila Arango.",
                evt = HEvents.Reto, arg = "caldas", target = 60, threshold = true, startDialog = "m2_caldas_start", rewardMoney = 50000, next = "m2_risaralda" },
            new Mission { id = "m2_risaralda", chapter = 2, title = "La AeroPress de la finca", desc = "En Belén de Umbría (Risaralda), Julián te reta a una AeroPress de 82 puntos con el café de su finca.",
                evt = HEvents.Reto, arg = "risaralda", target = 60, threshold = true, rewardMoney = 60000, rewardFlags = new[] { "f_eje" }, next = "m2_cosecha" },
            new Mission { id = "m2_cosecha", chapter = 2, title = "La primera cosecha bogotana", desc = "En la bodega, cosecha cerezas maduras de tus balsas. Si aún no hay, duerme y cuida el clima (18 – 22 °C, 12 h de luz).",
                evt = HEvents.Harvest, startDialog = "m2_cosecha_start", endDialog = "m2_cosecha_end", rewardWiki = new[] { "cul_maduracion_colores", "cul_anatomia_cereza" }, next = "m2_beneficio" },
            new Mission { id = "m2_beneficio", chapter = 2, title = "Beneficio en casa", desc = "Lleva tus cerezas al beneficio de la bodega: despulpa, fermenta y seca.",
                evt = HEvents.Process, endDialog = "m2_beneficio_end", rewardMoney = 80000, rewardWiki = new[] { "ben_frio_bogota" }, rewardFlags = new[] { "f_cap3" } },

            // ---- Capítulo 3 ----
            new Mission { id = "m3_huila", chapter = 3, title = "El maestro de las fermentaciones", desc = "Viaja a Pitalito (Huila) y supera el reto de beneficio de Don Hernando.",
                evt = HEvents.Reto, arg = "huila", target = 60, threshold = true, startDialog = "m3_huila_start", rewardMoney = 60000, next = "m3_tostion" },
            new Mission { id = "m3_tostion", chapter = 3, title = "Fuego y paciencia", desc = "En la tostadora de la bodega, logra una tostión de 70 puntos o más (desarrollo entre 15 y 25 %).",
                evt = HEvents.Roast, target = 70, threshold = true, startDialog = "m3_tostion_start", endDialog = "m3_tostion_end", rewardWiki = new[] { "tos_reposo" }, next = "m3_cauca" },
            new Mission { id = "m3_cauca", chapter = 3, title = "La Ciudad Blanca", desc = "Viaja al Cauca y supera el reto de tostión de Doña Lucía.",
                evt = HEvents.Reto, arg = "cauca", target = 60, threshold = true, rewardMoney = 60000, next = "m3_narino" },
            new Mission { id = "m3_narino", chapter = 3, title = "Café de altura", desc = "Viaja a La Unión (Nariño) y supera la catación de Don Efraín.",
                evt = HEvents.Reto, arg = "narino", target = 60, threshold = true, rewardMoney = 70000, next = "m3_tolima" },
            new Mission { id = "m3_tolima", chapter = 3, title = "La catadora de Planadas", desc = "Viaja a Planadas (Tolima) y supera la catación de Yesenia.",
                evt = HEvents.Reto, arg = "tolima", target = 60, threshold = true, rewardMoney = 70000, rewardFlags = new[] { "f_nacional" }, next = "m3_taza" },
            new Mission { id = "m3_taza", chapter = 3, title = "Taza de campeonato", desc = "Prepara una AeroPress de 84 puntos o más (prueba con los cafés que te regalaron los mentores).",
                evt = HEvents.Brew, target = 84, threshold = true, startDialog = "m3_taza_start", endDialog = "m3_taza_end", next = "m3_nacional" },
            new Mission { id = "m3_nacional", chapter = 3, title = "Campeonato Nacional", desc = "Inscríbete desde el Correo del computador y gana el Campeonato Nacional en Medellín (1.495 m: ¡el agua hierve más caliente!).",
                evt = HEvents.Competition, arg = "nacional", target = 1, threshold = true, startDialog = "m3_nacional_start", rewardFlags = new[] { "f_cap4" } },

            // ---- Capítulo 4 ----
            new Mission { id = "m4_antioquia", chapter = 4, title = "Tierra de arrieros", desc = "Viaja a Jardín (Antioquia) y supera el reto de historia de Don Gildardo.",
                evt = HEvents.Reto, arg = "antioquia", target = 60, threshold = true, startDialog = "m4_antioquia_start", rewardMoney = 70000, next = "m4_salazar" },
            new Mission { id = "m4_salazar", chapter = 4, title = "La cuna del café", desc = "Viaja a Salazar de las Palmas (Norte de Santander) y supera el reto del Profesor Contreras.",
                evt = HEvents.Reto, arg = "nortesantander", target = 60, threshold = true, rewardMoney = 70000, next = "m4_santander" },
            new Mission { id = "m4_santander", chapter = 4, title = "Café con pájaros", desc = "Viaja a Barichara (Santander) y supera el reto de plagas de Doña Mercedes.",
                evt = HEvents.Reto, arg = "santander", target = 60, threshold = true, rewardMoney = 70000, next = "m4_sierra" },
            new Mission { id = "m4_sierra", chapter = 4, title = "El corazón del mundo", desc = "Viaja a Minca (Sierra Nevada de Santa Marta) y supera la recolección con Doña Marina.",
                evt = HEvents.Reto, arg = "sierranevada", target = 60, threshold = true, rewardMoney = 80000, rewardFlags = new[] { "f_raices" }, next = "m4_comunidad" },
            new Mission { id = "m4_comunidad", chapter = 4, title = "Una comunidad cafetera", desc = "Llega a 8.000 seguidores en acero-press: publica tus mejores tazas y escribe artículos en el computador.",
                evt = HEvents.Followers, target = 8000, startDialog = "m4_comunidad_start", next = "m4_tazas" },
            new Mission { id = "m4_tazas", chapter = 4, title = "Rumbo a las 50.000", desc = "Llega a 40 tazas preparadas. Cada AeroPress cuenta.",
                evt = HEvents.Cups, target = 40, endDialog = "m4_tazas_end", rewardMoney = 150000, rewardFlags = new[] { "f_cap5" } },

            // ---- Capítulo 5 ----
            new Mission { id = "m5_invitacion", chapter = 5, title = "La invitación", desc = "Llega a 12.000 seguidores en acero-press: el Mundial invita a los creadores con comunidad.",
                evt = HEvents.Followers, target = 12000, startDialog = "m5_invitacion_start", endDialog = "m5_invitacion_end", rewardFlags = new[] { "f_mundial" }, next = "m5_mundial" },
            new Mission { id = "m5_mundial", chapter = 5, title = "El Mundial", desc = "Inscríbete desde el Correo y gana el Mundial en Lisboa, casi a nivel del mar: el agua hierve a 100 °C. ¡Adapta tu receta!",
                evt = HEvents.Competition, arg = "mundial", target = 1, threshold = true, startDialog = "m5_mundial_start", endDialog = "final" },
        };

        // ============================ COMPETENCIAS ============================

        public static readonly List<Competition> Competitions = new List<Competition>
        {
            new Competition { id = "copa_bogota", name = "Copa Bogotá de AeroPress", city = "Bogotá", chapter = 1, rounds = 2, rivalSkill = 0.35f, altitude = 2600f,
                prize = 300000, trophy = "Copa Bogotá", introDialog = "copa_bogota_intro", winDialog = "copa_bogota_win", loseDialog = "copa_bogota_lose", wikiUnlocks = new[] { "comp_wac" } },
            new Competition { id = "nacional", name = "Campeonato Nacional de AeroPress", city = "Medellín", chapter = 3, rounds = 3, rivalSkill = 0.6f, altitude = 1495f,
                prize = 1000000, trophy = "Campeón nacional", introDialog = "nacional_intro", winDialog = "nacional_win", loseDialog = "nacional_lose", wikiUnlocks = new[] { "comp_brewers_cup", "hist_taza_excelencia" } },
            new Competition { id = "mundial", name = "Mundial de AeroPress", city = "Lisboa, Portugal", chapter = 5, rounds = 4, rivalSkill = 0.85f, altitude = 20f,
                prize = 5000000, trophy = "Campeón mundial", introDialog = "mundial_intro", winDialog = "mundial_win", loseDialog = "mundial_lose", wikiUnlocks = new[] { "comp_wbc" } },
        };

        public static readonly List<Rival> Rivals = new List<Rival>
        {
            new Rival { name = "Tomás Gaitán", city = "Bogotá", skill = 0.35f, style = "invertida, 18 g, agua a 85 °C y diluye al final" },
            new Rival { name = "Mariana Ospina", city = "Pereira", skill = 0.45f, style = "estándar, 15 g, molienda media, 1:45 de infusión" },
            new Rival { name = "Lucía Restrepo", city = "Medellín", skill = 0.55f, style = "invertida, molienda gruesa, 4 minutos" },
            new Rival { name = "Andrés Bastidas", city = "Pasto", skill = 0.6f, style = "estándar, agua bien caliente y muy poca agitación" },
            new Rival { name = "Felipe Arce", city = "San José, Costa Rica", skill = 0.65f, style = "honey de altura, 16 g, 2 minutos" },
            new Rival { name = "Ana Lucía Cifuentes", city = "Antigua, Guatemala", skill = 0.7f, style = "concentrado de 120 g y bypass de agua" },
            new Rival { name = "Rafael Monteiro", city = "São Paulo, Brasil", skill = 0.7f, style = "natural brasileño, molienda fina y 1 minuto" },
            new Rival { name = "Wanjiru Kamau", city = "Nairobi, Kenia", skill = 0.75f, style = "SL28 lavado, 90 °C, revuelve 20 veces" },
            new Rival { name = "Abebe Tesfaye", city = "Addis Abeba, Etiopía", skill = 0.8f, style = "Yirgacheffe natural, invertida, 3 minutos" },
            new Rival { name = "Kenji Watanabe", city = "Tokio, Japón", skill = 0.85f, style = "agua de baja dureza medida al miligramo" },
            new Rival { name = "Seo-yeon Park", city = "Seúl, Corea", skill = 0.88f, style = "doble filtro de papel y enfriado rápido" },
            new Rival { name = "Ingrid Solberg", city = "Oslo, Noruega", skill = 0.9f, style = "tueste muy claro, 1:16, sin revolver" },
        };

        // ============================ CORREO ============================

        public static readonly List<MailItem> Mail = new List<MailItem>
        {
            new MailItem { id = "bienvenida", from = "Valentina · Float Gardens", subject = "¡Bienvenido a la bodega!",
                body = "Parce, ya quedó todo listo: tres tanques con balsas flotantes, bomba de aire, luces LED y tu barra de AeroPress. En Bogotá el café no aguanta afuera (hace demasiado frío), pero aquí adentro le podemos dar su clima de montaña.\n\nRecuerda: entre 18 y 22 °C, unas 12 horas de luz, humedad entre 60 y 80 % y la solución con pH entre 5,5 y 6,5. Y no te asustes con la cuenta de la luz: calentar una bodega bogotana cuesta.\n\n— Valentina" },
            new MailItem { id = "mama1", from = "Mamá", subject = "¿Ya comiste?",
                body = "Mijo, ¿ya comió? No se me vaya a quedar solo a punta de tinto. Me contó su tía que ahora tiene matas de café en una bodega. Usted siempre tan inventor. Le mando la bendición y una ruana para el frío de Puente Aranda.\n\nLo quiere, Mamá." },
            new MailItem { id = "proveedor", from = "Equipos Cafeteros del Centro", subject = "Lo que todo barista necesita",
                body = "¿Sabías que un gramo de diferencia en la dosis cambia la fuerza de tu taza? ¿Y que en Bogotá un hervidor sin control de temperatura te deja adivinando? En la tienda del computador tenemos báscula de 0,1 g, hervidor cuello de cisne, molino de muelas y, para los más pilos, refractómetro." },
            new MailItem { id = "fan1", from = "cafeteritx_93", subject = "¡Te descubrí en acero-press!", requiresFlag = "f_cap1",
                body = "No sabía que el agua en Bogotá hierve a unos 91 °C. ¡Con razón mi café siempre me salía ácido! Ahora muelo un poquito más fino. Sigue publicando, porfa. ¿Es verdad que estás cultivando café en una bodega?" },
            new MailItem { id = "ines", from = "Doña Inés Galindo", subject = "Una tertulia pendiente", requiresFlag = "f_ines",
                body = "Joven: en los cafés del centro se discutía de todo, de poesía a política, con un tinto de por medio. Usted me recordó a esos muchachos. Cuando vaya a Viotá, salúdeme a Don Aurelio: él sabe de beneficio lo que yo sé de historia. Y no olvide que el café llegó a Colombia por los caminos del oriente, mucho antes de que alguien soñara con cultivarlo en una bodega." },
            new MailItem { id = "copa_inv", from = "Organización Copa Bogotá", subject = "Invitación: Copa Bogotá de AeroPress", requiresFlag = "f_copa",
                body = "Te invitamos a la Copa Bogotá de AeroPress. Formato: llaves de eliminación; en cada ronda tres jueces prueban a ciegas y, al mismo tiempo, señalan su taza favorita. El agua de la sede hierve a unos 91 °C: los locales sabemos jugar con eso.\n\nInscríbete desde este mismo correo." },
            new MailItem { id = "valentina_eje", from = "Valentina · Float Gardens", subject = "¡Campeón de Bogotá!", requiresFlag = "f_cap2",
                body = "¡Lo logramos! Ya hay gente preguntando por el café de la bodega. Ahora te toca ir a la fuente: el Eje Cafetero. Mira bien cómo recogen, cómo secan y cómo luchan contra la roya. Lo que aprendas allá lo aplicamos acá con las balsas." },
            new MailItem { id = "camila_lab", from = "Dra. Camila Arango", subject = "Sobre la roya en interiores", requiresFlag = "f_eje",
                body = "Me quedé pensando en su bodega. La roya necesita agua líquida sobre la hoja para germinar, así que en interiores evite que las hojas queden mojadas por horas y no se pase del 80 % de humedad. Y si siembra variedades susceptibles, vigílelas cada día. La ciencia está para ayudar a los caficultores, también a los urbanos." },
            new MailItem { id = "nacional_inv", from = "Campeonato Nacional de AeroPress", subject = "Clasificaste al Nacional en Medellín", requiresFlag = "f_nacional",
                body = "Felicitaciones: estás entre los finalistas del Campeonato Nacional de AeroPress, que este año se juega en Medellín. Tres rondas de eliminación. Atención: a 1.495 m el agua hierve más caliente que en Bogotá, cerca de 95 °C.\n\nInscríbete desde este correo." },
            new MailItem { id = "primo", from = "Juancho (tu primo en Medellín)", subject = "Aquí te esperamos", requiresFlag = "f_nacional",
                body = "¡Quiubo, primo! Me dijeron que viene al Nacional. Aquí tiene cama y arepa. Y le cuento algo que no sabía: la Federación de Cafeteros se fundó aquí en Medellín, en 1927. O sea que viene a competir a la casa del café colombiano, ¡no me vaya a quedar mal!" },
            new MailItem { id = "hernando", from = "Don Hernando Perdomo", subject = "Paciencia con el mucílago", requiresFlag = "f_cap3",
                body = "Mijo, en su bodega fría la fermentación va a ir más despacio que en Pitalito: los microorganismos trabajan menos con frío. Mire el grano, no solo el reloj, y mantenga estable la temperatura. Lo que se hace con afán, en el café se paga en la taza." },
            new MailItem { id = "mama2", from = "Mamá", subject = "Lo vi en las noticias", requiresFlag = "f_cap4",
                body = "¡Mijo, salió en las noticias! Campeón nacional de café. Su abuelo, que en paz descanse, recogía café en Quindío cuando era joven. Estaría orgulloso. Venga a almorzar el domingo y me trae de ese café de la bodega." },
            new MailItem { id = "fan_japon", from = "Haruka · Tokio", subject = "Hello from Tokyo!", requiresFlag = "f_cap4",
                body = "Hola desde Tokio (estoy aprendiendo español con tus videos). En Japón amamos el café colombiano y tu historia de la bodega en Bogotá. ¿Algún día publicarás tu receta para la AeroPress a nivel del mar?" },
            new MailItem { id = "gildardo", from = "Don Gildardo Restrepo", subject = "Caminos de herradura", requiresFlag = "f_raices",
                body = "Antes de las carreteras, el café bajaba de la montaña a lomo de mula, por caminos de herradura, hasta el río o el tren. Usted lo lleva ahora por internet. Cambian los caminos, mijo, pero el café sigue juntando a la gente." },
            new MailItem { id = "mundial_inv", from = "Mundial de AeroPress", subject = "Estás invitado a Lisboa", requiresFlag = "f_mundial",
                body = "Tu comunidad y tus resultados te ganaron un cupo en el Mundial de AeroPress en Lisboa. Cuatro rondas. La sede está casi a nivel del mar: el agua hierve a 100 °C. Quien llegue con la receta de la altura sin ajustarla, se va a sobreextraer.\n\nInscríbete desde este correo." },
            new MailItem { id = "valentina_final", from = "Valentina · Float Gardens", subject = "Pase lo que pase", requiresFlag = "f_mundial",
                body = "Empezamos con tres tanques, una AeroPress y un computador viejo. Hoy la gente sabe que en Bogotá también se puede cultivar café. Pase lo que pase en Lisboa, ya llegamos al mundo. Ahora ve y prepárales la mejor taza de su vida." },
        };

        // ============================ DIÁLOGOS ============================

        public static readonly Dictionary<string, DialogLine[]> Dialogs = new Dictionary<string, DialogLine[]>
        {
            // ---- Capítulos ----
            ["cap0_intro"] = D(
                "Bogotá, 2.600 metros sobre el nivel del mar. Llovizna sobre Puente Aranda.",
                J + "|Dicen que Bogotá está 2.600 metros más cerca de las estrellas. Yo digo que está 2.600 metros más lejos del cafetal.",
                J + "|Aquí hace unos 14 °C. El café se da mejor entre 1.200 y 2.000 metros, con temperaturas de 19 a 21 grados. Afuera, una mata de café se moriría de frío.",
                J + "|Pero adentro de esta bodega... es otra historia.",
                V + "|¡" + J + "! Ya quedaron listas las balsas flotantes. Las raíces cuelgan en el agua con nutrientes y la bomba les da oxígeno.",
                V + "|Esto es Float Gardens: café cultivado dentro de las casas y bodegas de Bogotá. Nadie lo ha hecho así.",
                J + "|Y yo con mi AeroPress y mi computador. La meta de acero-press: 50.000 tazas. Y algún día, ser un referente del café en el mundo.",
                V + "|Primero lo primero: prepárate un tinto. Esa AeroPress no se va a usar sola."),
            ["cap1_intro"] = D(
                "Capítulo 1. La semilla.",
                V + "|Tus matas ya están germinando. En la vida real una semilla tarda unos dos meses en salir... aquí vamos más rápido, pero igual toca paciencia.",
                V + "|Mientras tanto tienes que aprender. ¿Sabes cómo llegó el café a Colombia? ¿Quién lo sembró primero?",
                J + "|Ni idea. Sé prepararlo, pero no sé de dónde viene.",
                V + "|Entonces ve al centro. En el Café La Tertulia trabaja Doña Inés Galindo: barista e historiadora. Sabe más de café que cualquier libro.",
                V + "|Sal por el portón, abre el mapa de Colombia y elige Bogotá."),
            ["cap2_intro"] = D(
                "Capítulo 2. El Eje Cafetero.",
                J + "|Campeón de la Copa Bogotá... Todavía no me lo creo.",
                V + "|Créetelo. Pero si quieres ser un referente de verdad, tienes que conocer el campo. Quindío, Risaralda y Caldas: el Paisaje Cultural Cafetero.",
                V + "|La UNESCO lo declaró patrimonio mundial en 2011. Allá vas a ver cómo se recoge el café grano a grano y cómo la ciencia salvó los cafetales de la roya.",
                J + "|Empaco la AeroPress y salgo para el Quindío."),
            ["cap3_intro"] = D(
                "Capítulo 3. El Sur.",
                V + "|¿Sabías que hoy el Huila es el departamento que más café produce en Colombia? Casi uno de cada cinco sacos del país sale de allá.",
                V + "|El sur es tierra de experimentos: fermentaciones, procesos nuevos, cafés de altura cerca del ecuador.",
                J + "|Huila, Tolima, Cauca y Nariño. Y al final, el Campeonato Nacional.",
                V + "|Aprende a fermentar, a tostar y a catar. Un barista que entiende el café desde la semilla hasta la taza no tiene rival."),
            ["cap4_intro"] = D(
                "Capítulo 4. Raíces.",
                J + "|Campeón nacional. Y pensar que todo empezó con tres tanques en una bodega.",
                V + "|Ahora toca volver a las raíces: de dónde salió el café colombiano, quiénes lo llevaron por las montañas y quiénes lo cuidan hoy.",
                V + "|Antioquia, los dos Santanderes y la Sierra Nevada de Santa Marta. Y mientras tanto, haz crecer la comunidad de acero-press."),
            ["cap5_intro"] = D(
                "Capítulo 5. El Mundo.",
                V + "|Llegaron correos de Japón, de Corea, de Noruega... La gente quiere saber del café que crece en una bodega a 2.600 metros.",
                J + "|El Mundial de AeroPress. Lisboa. Casi a nivel del mar.",
                V + "|Allá el agua hierve a 100 °C. Todo lo que aprendiste a 91 grados tendrás que adaptarlo.",
                J + "|Entonces a practicar. De Bogotá al mundo."),

            // ---- Prólogo ----
            ["m0_tinto_start"] = D(
                V + "|La barra está a la derecha del portón. Tienes medio kilo de café tostado de Cundinamarca, cortesía de una tienda de La Candelaria.",
                V + "|Un consejo: aquí el agua hierve a unos 91 °C, no a 100. Por más que pite la tetera, no vas a tener agua más caliente.",
                V + "|Así que no le tengas miedo al agua a temperatura máxima. Acércate a la barra y presiona E."),
            ["m0_tinto_end"] = D(
                J + "|Mi primera taza en la bodega. El café es inmersión más presión: se infusiona y después se presiona por un filtro.",
                V + "|¡Huele delicioso! La AeroPress la inventó Alan Adler en 2005, el mismo ingeniero del disco volador Aerobie. Quería una taza individual, menos amarga.",
                V + "|Ahora muéstrale al mundo lo que estás haciendo."),
            ["m0_publicar_start"] = D(
                J + "|acero-press: mi canal para documentar el camino a 50.000 tazas. Receta, origen y foto de cada preparación.",
                V + "|Prepara otra y dale a Publicar. Con buenas tazas llegan los seguidores."),
            ["m0_publicar_end"] = D(
                J + "|¡Primeros seguidores! Alguien en Medellín comentó: ¿café cultivado en Bogotá? ¡No puede ser!",
                V + "|Pues sí puede ser. Vamos a la huerta: es hora de sembrar."),
            ["m0_sembrar_start"] = D(
                V + "|La huerta flotante está al centro. Cada balsa es una lámina de icopor que flota sobre la solución nutritiva; las raíces cuelgan en el agua.",
                V + "|Las raíces del café necesitan oxígeno: por eso la bomba de aire no se apaga nunca.",
                V + "|Tienes semillas de Caturra. Acércate a la huerta, elige una balsa vacía y siémbrala."),
            ["m0_sembrar_end"] = D(
                J + "|Sembrada. Primero sale un tallito con la semilla encima, como un fósforo: el fosforito. Después se abren las dos primeras hojas: la chapola.",
                V + "|Chapola quiere decir mariposa. Cuida el clima: entre 18 y 22 grados, unas 12 horas de luz y el pH entre 5,5 y 6,5."),
            ["m0_dormir_start"] = D(
                V + "|Por hoy suficiente. Las matas crecen mientras duermes. Tu rincón está al fondo, junto a la estantería."),
            ["m0_dormir_end"] = D(
                "Al día siguiente, el sol de la sabana entra por los ventanales. Monserrate aparece entre las nubes.",
                J + "|Ayer fue el primer día. Hoy empieza la historia."),

            // ---- Capítulo 1 ----
            ["m1_centro_start"] = D(
                V + "|Para llegar al centro sal por el portón: se abre el mapa de Colombia. Bogotá está marcada con una estrella."),
            ["m1_centro_end"] = D(
                J + "|La Candelaria: ladrillo, tejas de barro, calles empinadas y palomas. El Café La Tertulia debe estar por aquí.",
                "Busca a Doña Inés en la puerta del café."),
            ["m1_historia_end"] = D(
                "Doña Inés Galindo|Muy bien, joven. Quien no sabe de dónde viene el café no sabe lo que tiene en la taza.",
                "Doña Inés Galindo|Ahora vaya a Viotá. Allá, Don Aurelio le va a enseñar lo que pasa entre la cereza y el grano verde."),
            ["m1_viota_start"] = D(
                J + "|Viotá queda bajando de la sabana: en tres horas se pasa de la neblina fría al aire tibio. Ahí sí se da el café al aire libre."),
            ["m1_lavado_end"] = D(
                "Don Aurelio Rincón|Así se hacía en las haciendas y así se sigue haciendo en las fincas: fermentar, lavar y secar con cuidado.",
                J + "|Me llegó un correo... ¡Me invitan a la Copa Bogotá de AeroPress!"),
            ["m1_receta_start"] = D(
                V + "|Antes de competir, afina tu receta. Busca una extracción entre 18 y 22 %: ni ácida por falta, ni amarga por exceso.",
                V + "|En Bogotá el agua no pasa de 91 °C. Si te queda ácida, muele más fino o deja más tiempo."),
            ["m1_receta_end"] = D(
                J + "|¡Ochenta puntos! Eso ya es café de especialidad.",
                V + "|Ahora sí: inscríbete en la Copa desde el correo del computador."),
            ["m1_copa_start"] = D(
                V + "|La Copa Bogotá es en una bodega de Chapinero. Dos rondas. Tres jueces señalan al tiempo la taza que más les gusta.",
                V + "|Inscríbete desde el correo del computador cuando estés listo."),

            // ---- Capítulo 2 ----
            ["m2_quindio_start"] = D(
                J + "|Ocho horas de bus hasta Salento. Dicen que en el Quindío hasta la plaza huele a café."),
            ["m2_caldas_start"] = D(
                V + "|En Chinchiná se investiga el café desde 1938. Allá crearon las variedades que salvaron los cafetales de la roya. Ve con la Dra. Camila Arango."),
            ["m2_cosecha_start"] = D(
                V + "|¿Cómo van tus matas? Si ya tienen cerezas rojas, cosecha solo las maduras: una cereza verde en la mezcla daña la taza.",
                V + "|Si todavía no hay frutos, duerme unos días y revisa el clima de la huerta. De la flor a la cereza madura, en la vida real, pasan siete u ocho meses."),
            ["m2_cosecha_end"] = D(
                J + "|¡Mi primera cosecha bogotana! Cerezas de café cultivadas a 2.600 metros, dentro de una bodega.",
                V + "|Esto es historia, " + J + ". Ahora al beneficio: despulpar, fermentar y secar."),
            ["m2_beneficio_end"] = D(
                J + "|El pergamino ya está en las camas de secado. En unos días tendré mi propio café verde.",
                V + "|En la bodega fría todo va más despacio, hasta la fermentación. Lo aprendiste bien en Viotá."),

            // ---- Capítulo 3 ----
            ["m3_huila_start"] = D(
                J + "|Pitalito, Huila. Dicen que aquí hay fincas que fermentan el café como si fuera vino."),
            ["m3_tostion_start"] = D(
                V + "|Ya tienes café verde. La tostadora está al lado del beneficio. Escucha el primer crack: suena como crispetas, hacia los 196 °C.",
                V + "|Después del crack viene el desarrollo. Entre 15 y 25 % del tiempo total es lo que buscan los tostadores de especialidad."),
            ["m3_tostion_end"] = D(
                J + "|Huele a pan, a caramelo... Eso es la reacción de Maillard.",
                V + "|Déjalo reposar un par de días antes de prepararlo: el café recién tostado suelta CO2."),
            ["m3_taza_start"] = D(
                V + "|El Nacional se gana con tazas de 84 puntos para arriba. Usa los cafés que te regalaron los mentores: son una joya."),
            ["m3_taza_end"] = D(
                J + "|84 puntos. Dulce, limpio, con la acidez brillante de la montaña.",
                V + "|Estás listo. Inscríbete en el Nacional desde el correo."),
            ["m3_nacional_start"] = D(
                V + "|El Nacional es en Medellín, a 1.495 metros. Allá el agua hierve cerca de 95 °C: más caliente que en Bogotá.",
                V + "|Si usas la misma receta, vas a extraer más. Ajusta la molienda o el tiempo."),

            // ---- Capítulo 4 ----
            ["m4_antioquia_start"] = D(
                J + "|Jardín, Antioquia. Plaza enorme, balcones floridos y montañas casi verticales sembradas de café."),
            ["m4_comunidad_start"] = D(
                V + "|acero-press está creciendo. Escribe artículos en el computador sobre lo que has aprendido: la gente quiere saber la historia detrás de cada taza."),
            ["m4_tazas_end"] = D(
                J + "|Cuarenta tazas en este viaje. Faltan muchas para las 50.000... pero cada una me enseñó algo.",
                V + "|Y ahora, " + J + ", te escribieron desde muy lejos."),

            // ---- Capítulo 5 ----
            ["m5_invitacion_start"] = D(
                V + "|El Mundial invita a creadores con comunidad. Sigue publicando y escribiendo: cuando llegues a 12.000 seguidores te van a llamar."),
            ["m5_invitacion_end"] = D(
                J + "|¡Llegó la invitación! Lisboa, Portugal. El Mundial de AeroPress."),
            ["m5_mundial_start"] = D(
                V + "|Recuerda: en Lisboa el agua hierve a 100 °C. Lo que en Bogotá era agua a tope, allá puede quemar tu extracción.",
                V + "|Inscríbete desde el correo cuando estés listo. Toda la bodega va a estar mirando."),

            // ---- Competencias ----
            ["copa_bogota_intro"] = D(
                "Una bodega de Chapinero llena de luces y gente con AeroPress en la mano.",
                "Presentadora|¡Bienvenidos a la Copa Bogotá de AeroPress! Mismo café, mismo tiempo, tres jueces y un dedo.",
                "Presentadora|Cuando dé la señal, cada juez señalará su taza favorita. El que tenga más votos sigue en la llave.",
                J + "|Respira. Moler, verter, revolver, esperar, presionar."),
            ["copa_bogota_win"] = D(
                "Presentadora|¡Y el campeón de la Copa Bogotá es... " + J + ", de Float Gardens!",
                J + "|¡Esta copa es para la bodega! ¡Café cultivado en Bogotá!",
                V + "|¡Lo sabía! Ahora el Eje Cafetero te espera."),
            ["copa_bogota_lose"] = D(
                J + "|Me eliminaron. Mi taza estaba buena, pero no era la mejor.",
                V + "|Así se aprende. Ajusta tu receta, practica y vuelve a inscribirte desde el correo."),
            ["nacional_intro"] = D(
                "Un recinto ferial de Medellín. Banderas de Colombia y baristas de todo el país.",
                "Presentador|¡Bienvenidos al Campeonato Nacional de AeroPress! Tres rondas. Solo uno representará a Colombia ante el mundo.",
                J + "|Aquí el agua hierve más caliente que en Bogotá. Ojo con la extracción."),
            ["nacional_win"] = D(
                "Presentador|¡El campeón nacional de AeroPress viene de una bodega de Bogotá!",
                J + "|Gracias a todos los mentores: Doña Inés, Don Aurelio, Doña Rosalba, la Dra. Camila, Julián, Don Hernando, Doña Lucía, Don Efraín y Yesenia. Esta taza es de ustedes.",
                V + "|Campeón nacional. Ahora, a las raíces."),
            ["nacional_lose"] = D(
                J + "|No pasé de ronda. Medellín no perdona.",
                V + "|Medellín no, el agua caliente. Ajusta la receta para 1.495 metros y vuelve a intentarlo desde el correo."),
            ["mundial_intro"] = D(
                "Lisboa. Un antiguo almacén frente al río Tajo convertido en arena. Banderas de decenas de países productores y consumidores.",
                "Presentadora|Welcome! ¡Bienvenidos! Cuatro rondas. Mismo café, tres jueces, un dedo.",
                J + "|A nivel del mar. Agua a 100 grados. De Bogotá al mundo."),
            ["mundial_win"] = D(
                "Presentadora|The champion of the world... ¡" + J + ", de Bogotá, Colombia!",
                J + "|¡Esto es de Bogotá! ¡De una bodega de Puente Aranda!"),
            ["mundial_lose"] = D(
                J + "|No esta vez. Pero llegué hasta aquí con café cultivado en Bogotá.",
                V + "|El Mundial se repite. Vuelve a practicar e inscríbete otra vez desde el correo."),
            ["final"] = D(
                "De regreso en Bogotá. La bodega está llena: vecinos, mentores y seguidores de acero-press.",
                V + "|Mírate: campeón del mundo. Y todo empezó con tres tanques y una AeroPress.",
                "Doña Inés Galindo|Joven, usted ya es parte de la historia del café en Colombia. Y eso que la historia del café es larga: viene de Etiopía, pasó por el Yemen, cruzó océanos...",
                "Don Aurelio Rincón|...llegó por los caminos del oriente, se regó por las montañas y hoy lo cultivan cientos de miles de familias.",
                "Doña Rosalba Marín|Y ahora también crece en Bogotá, a 2.600 metros, dentro de una bodega. ¡Quién lo creyera!",
                J + "|Float Gardens no reemplaza a las fincas. Las celebra. Cada taza que preparo cuenta la historia de quienes la hicieron posible.",
                V + "|¿Y ahora qué sigue?",
                J + "|Lo de siempre: la próxima taza. Todavía faltan muchas para las 50.000.",
                "De Bogotá al mundo. Fin... por ahora."),

            // ---- Objetos de la bodega ----
            ["mural"] = D(
                J + "|El mural de Float Gardens: una mata de café flotando sobre la sabana, con Monserrate al fondo.",
                J + "|'2.600 metros más cerca de las estrellas'. El lema de la ciudad. Y ahora, también, de su primer café."),
            ["ventana"] = D(
                J + "|Los cerros orientales y Monserrate en la cima, con su santuario blanco. Aquí arriba hace demasiado frío para el café.",
                J + "|Por eso hay que bajar a las montañas templadas... o traer la montaña a la bodega."),
            ["tablero"] = D(
                J + "|Metas: 50.000 tazas. Café cultivado en Bogotá. El Mundial.",
                J + "|Abajo, con la letra de Valentina: 'Y dormir de vez en cuando'."),
            ["estanteria"] = D(
                J + "|Libros de botánica, una rueda de sabores, frascos con cafés de varias regiones y un manual de hidroponía lleno de notas.",
                J + "|En una esquina, la primera AeroPress que tuve. El émbolo ya está gastado de tanto presionar."),
            ["tintero"] = D(
                "Don Chucho|¡Tinto, tintico! Con azúcar o con panela, mijo.",
                "Don Chucho|Durante años el mejor café se iba para afuera y aquí nos tomábamos la pasilla. Ahora la gente pide café de especialidad hasta en la calle.",
                J + "|Uno con panela, Don Chucho. Por la tradición."),
            ["placa_cafe"] = D(
                J + "|La placa dice que en los cafés del centro se reunían escritores y poetas a conversar alrededor de un tinto.",
                J + "|A mediados del siglo XX había decenas de cafés de tertulia. Hoy quedan muy pocos."),

            // ---- Regiones ----
            ["bogota_mentor"] = D(
                "Doña Inés Galindo|Bienvenido al Café La Tertulia, joven. Usted es el de las matas en la bodega, ¿no? Me contaron.",
                "Doña Inés Galindo|Yo soy Inés Galindo. Llevo cuarenta años sirviendo tinto y estudiando la historia del café.",
                "Doña Inés Galindo|¿Sabe cuál es el registro más antiguo del café en el territorio colombiano? El del jesuita José Gumilla, en su libro 'El Orinoco ilustrado' de 1741.",
                "Doña Inés Galindo|Contaba que se cultivaba en una misión cerca de donde el río Meta cae al Orinoco. Los historiadores ubican esa siembra hacia 1730.",
                "Doña Inés Galindo|Y en Bogotá, los cafés del centro fueron escuelas de poetas y escritores. El tinto era la excusa para conversar.",
                "Doña Inés Galindo|Le propongo una tertulia: unas preguntas sobre la historia del café. Si me responde bien, le regalo semillas de la variedad Colombia."),
            ["bogota_despues"] = D(
                "Doña Inés Galindo|Vuelva cuando quiera, joven. Aquí siempre hay tinto y tertulia.",
                "Doña Inés Galindo|Y recuerde: la variedad Colombia salió en 1982, ¡un año antes de que llegara la roya! Eso es planear."),
            ["bogota_reto_ok"] = D(
                "Doña Inés Galindo|¡Bravo! Así me gusta: un barista que sabe de dónde viene lo que sirve.",
                "Doña Inés Galindo|Tome estas semillas de la variedad Colombia y un kilo de café verde. Que crezcan en su bodega."),

            ["cundinamarca_mentor"] = D(
                "Don Aurelio Rincón|Bienvenido a Viotá, muchacho. Aquí hubo grandes haciendas cafeteras desde el siglo XIX. Hoy la tierra es de pequeños productores.",
                "Don Aurelio Rincón|Yo administro lo que queda de una de esas haciendas. Mire: el beneficiadero viejo, de ladrillo, todavía funciona.",
                "Don Aurelio Rincón|El café que sale de aquí es balanceado, con notas a panela y chocolate. La cosecha grande es en el primer semestre y la mitaca en el segundo.",
                "Don Aurelio Rincón|Pero la calidad no se queda en el árbol. Se hace en el beneficio: despulpar el mismo día, fermentar lo justo, lavar bien y secar hasta 10 a 12 % de humedad.",
                "Don Aurelio Rincón|¿Me hace un lavado clásico perfecto? Si lo logra, le regalo semillas de Borbón, de las que se sembraban antes."),
            ["cundinamarca_despues"] = D(
                "Don Aurelio Rincón|Ese lavado le quedó como de hacienda grande. Recuerde: el café se dañó más en los beneficios que en los árboles."),
            ["cundinamarca_reto_ok"] = D(
                "Don Aurelio Rincón|¡Eso es un café bien beneficiado! Ni sobrefermentado ni húmedo.",
                "Don Aurelio Rincón|Llévese estas semillas de Borbón y un kilo de nuestro café verde. Dulcecito, como debe ser."),

            ["quindio_mentor"] = D(
                "Doña Rosalba Marín|¡Bienvenido a Salento, mijo! Aquí empieza el Paisaje Cultural Cafetero.",
                "Doña Rosalba Marín|Mire esas casas de bahareque con balcones de colores, y allá arriba, en el Valle de Cocora, las palmas de cera, el árbol nacional.",
                "Doña Rosalba Marín|Y los Willys: llegaron hacia 1946 y se volvieron mulitas mecánicas. Un 'yipao' es todo lo que le cabe a uno: café, plátano, ¡hasta un trasteo!",
                "Doña Rosalba Marín|Yo he recogido café desde niña. Aquí se cosecha a mano, grano por grano, con el coco amarrado a la cintura, pasando varias veces por el mismo lote.",
                "Doña Rosalba Marín|Solo se coge la cereza madura. Unos pocos granos verdes bastan para dañar una taza.",
                "Doña Rosalba Marín|A ver qué tan buen recolector es usted. Coja solo las maduras... y no deje las secas en la mata, que ahí se esconde la broca."),
            ["quindio_despues"] = D(
                "Doña Rosalba Marín|Ya sabe recoger como un quindiano. Y si va a Armenia en octubre, no se pierda el Desfile del Yipao."),
            ["quindio_reto_ok"] = D(
                "Doña Rosalba Marín|¡Qué ojo para las maduras! Así da gusto.",
                "Doña Rosalba Marín|Le regalo semillas de Tabi, que es de porte alto y resistente a la roya, buena para fincas con sombra. Y un kilo de café de la finca."),

            ["risaralda_mentor"] = D(
                "Julián Henao|¡Quiubo, parcero! Soy Julián. En Belén de Umbría tostamos y preparamos nuestro propio café, aquí mismo en la finca.",
                "Julián Henao|Risaralda tiene lomas empinadas, guadua en las quebradas y árboles de sombrío llenos de pájaros. Y de fondo, los nevados de la cordillera Central.",
                "Julián Henao|La cosecha grande aquí es de septiembre a diciembre y la mitaca de abril a junio.",
                "Julián Henao|Me contaron que usted es campeón de AeroPress en Bogotá. Pero aquí el agua hierve más caliente y el café es otro.",
                "Julián Henao|Le propongo algo: prepáreme una AeroPress de 82 puntos o más con el café de la finca. Si lo logra, le doy semillas de Cenicafé 1."),
            ["risaralda_despues"] = D(
                "Julián Henao|Esa taza la voy a recordar. Cuando gane algo grande, me invita, ¿cierto?"),
            ["risaralda_reto_ok"] = D(
                "Julián Henao|¡Uff, parcero! Esa taza estaba limpia, dulce y con la acidez justa.",
                "Julián Henao|Tome semillas de Cenicafé 1, resistente a la roya y muy productiva, y un kilo de nuestro café."),

            ["caldas_mentor"] = D(
                "Dra. Camila Arango|Bienvenido a Chinchiná. Soy Camila Arango, agrónoma. Aquí se investiga el café desde 1938.",
                "Dra. Camila Arango|En 1982 se entregó la variedad Colombia, la primera resistente a la roya del país. Y en 1983 se confirmó la roya aquí mismo, en Chinchiná.",
                "Dra. Camila Arango|La roya la causa un hongo, Hemileia vastatrix: forma un polvillo naranja debajo de las hojas y las tumba. Entre 2008 y 2011 tuvimos una gran epidemia.",
                "Dra. Camila Arango|La respuesta fue renovar cafetales con variedades resistentes como Castillo, entregada en 2005.",
                "Dra. Camila Arango|Y luego está la broca, un cucarroncito de menos de 2 mm que llegó en 1988. Se controla con el Re-Re: recoger y repasar, sin dejar frutos olvidados.",
                "Dra. Camila Arango|Le propongo un examen de laboratorio: identifique plagas y responda. Si lo pasa, le doy semillas de Castillo."),
            ["caldas_despues"] = D(
                "Dra. Camila Arango|En su bodega no deje las hojas mojadas por horas: la roya necesita agua líquida para germinar."),
            ["caldas_reto_ok"] = D(
                "Dra. Camila Arango|Excelente. Usted piensa como un investigador.",
                "Dra. Camila Arango|Le doy semillas de Castillo, la variedad de la renovación, y un kilo de café de nuestras parcelas."),

            ["tolima_mentor"] = D(
                "Yesenia Lozano|Bienvenido a Planadas, al sur del Tolima. Soy Yesenia, catadora.",
                "Yesenia Lozano|Esta región sufrió mucho el conflicto armado. Hoy los pequeños productores, incluidas comunidades nasa, apuestan por cafés especiales de muy alta calidad.",
                "Yesenia Lozano|El café del Tolima tiene denominación de origen desde 2017: acidez media-alta, cuerpo limpio, notas cítricas y frutales.",
                "Yesenia Lozano|Para saber qué tan bueno es un café hay que catarlo con un protocolo: 8,25 gramos por cada 150 ml de agua a unos 93 grados, romper la costra a los 4 minutos y sorber con la cuchara.",
                "Yesenia Lozano|¿Catamos juntos? Si me demuestra buen paladar, le regalo semillas de Wush Wush."),
            ["tolima_despues"] = D(
                "Yesenia Lozano|Siga entrenando el paladar. Un buen catador se hace taza a taza, igual que un buen barista."),
            ["tolima_reto_ok"] = D(
                "Yesenia Lozano|¡Tiene paladar, " + J + "!",
                "Yesenia Lozano|Le regalo semillas de Wush Wush, una variedad de origen etíope, y un kilo de café de Planadas."),

            ["huila_mentor"] = D(
                "Don Hernando Perdomo|Bienvenido a Pitalito, joven. El Huila es hoy el primer productor de café de Colombia.",
                "Don Hernando Perdomo|Aquí nace el río Magdalena, en el Macizo Colombiano. Y cerca están las estatuas de piedra de San Agustín.",
                "Don Hernando Perdomo|Yo me dedico a las fermentaciones. El mucílago es la miel del grano: con el tiempo y los microorganismos se suelta, y en ese proceso nacen sabores.",
                "Don Hernando Perdomo|Lavado, honey, natural, anaeróbico... Cada proceso es distinto. Pero ojo: si se pasa de fermentación, el café sabe a vinagre.",
                "Don Hernando Perdomo|Mi reto: saque notas frutales sin defectos. Natural o anaeróbico, en su punto y bien secado. Si lo logra, le doy semillas de Pink Bourbon."),
            ["huila_despues"] = D(
                "Don Hernando Perdomo|Lo que se hace con afán, en el café se paga en la taza. Acuérdese siempre."),
            ["huila_reto_ok"] = D(
                "Don Hernando Perdomo|¡Frutal, limpio y sin vinagre! Usted tiene manos de beneficiador.",
                "Don Hernando Perdomo|Tome semillas de Pink Bourbon, el de las cerezas rosadas que se hizo famoso aquí en el Huila, y un kilo de café de la finca."),

            ["cauca_mentor"] = D(
                "Doña Lucía Muñoz|Bienvenido al Cauca. Soy Lucía y tuesto café desde hace veinte años en Popayán, la Ciudad Blanca.",
                "Doña Lucía Muñoz|Aquí el café lo cultivan pequeños productores campesinos e indígenas, entre ellos comunidades nasa y misak. Tiene denominación de origen desde 2011.",
                "Doña Lucía Muñoz|Acidez alta y brillante, notas florales, buen dulzor. Pero ese café se puede arruinar en la tostadora.",
                "Doña Lucía Muñoz|Un tueste tiene tres fases: secado, Maillard y desarrollo. El primer crack suena como crispetas, cerca de los 196 grados.",
                "Doña Lucía Muñoz|Mi reto: un tueste medio con un desarrollo entre 18 y 22 %, sin quemar el grano. Si lo logra, le regalo semillas de Sidra."),
            ["cauca_despues"] = D(
                "Doña Lucía Muñoz|La tostadora no se apura: se escucha, se huele y se mira la curva."),
            ["cauca_reto_ok"] = D(
                "Doña Lucía Muñoz|¡Qué curva tan bonita! Ni cruda ni quemada.",
                "Doña Lucía Muñoz|Llévese semillas de Sidra y un kilo de café verde del Cauca."),

            ["narino_mentor"] = D(
                "Don Efraín Benavides|Bienvenido a La Unión, en Nariño. Aquí, cerca del ecuador, el café se siembra altísimo: casi a 2.200 metros.",
                "Don Efraín Benavides|Las fincas son pequeñitas, en laderas casi verticales. Y allá, el volcán Galeras vigila a Pasto.",
                "Don Efraín Benavides|Nariño tuvo la primera denominación de origen regional de café en Colombia, en 2011.",
                "Don Efraín Benavides|A más altura el grano madura más despacio, y eso suele dar más acidez y complejidad: acidez alta, muy dulce, muy aromático.",
                "Don Efraín Benavides|Hagamos una catación. Si me reconoce bien los sabores, le regalo semillas de Geisha."),
            ["narino_despues"] = D(
                "Don Efraín Benavides|Cuando prepare un café de Nariño, acuérdese de estas montañas."),
            ["narino_reto_ok"] = D(
                "Don Efraín Benavides|Buen paladar, mijo. Así se reconoce un café de altura.",
                "Don Efraín Benavides|Le regalo semillas de Geisha, la de las notas a jazmín, y un kilo de café de la finca. Cuídelas mucho: producen poquito."),

            ["antioquia_mentor"] = D(
                "Don Gildardo Restrepo|¡Bienvenido a Jardín, hombre! Soy Gildardo, arriero de familia e historiador por terquedad.",
                "Don Gildardo Restrepo|Antes de las carreteras, el café bajaba a lomo de mula por caminos de herradura. El arriero, con su carriel, su poncho y su sombrero, es símbolo de esta tierra.",
                "Don Gildardo Restrepo|En el siglo XIX, familias antioqueñas migraron al sur fundando pueblos: Manizales en 1849, Armenia en 1889. Con ellas viajaron el café y el bahareque.",
                "Don Gildardo Restrepo|Y en Medellín, en 1927, los cafeteros fundaron la Federación Nacional de Cafeteros para defender el precio y la calidad de su café.",
                "Don Gildardo Restrepo|Le tengo unas preguntas de historia. Si las responde, le regalo semillas de Caturra de las mías."),
            ["antioquia_despues"] = D(
                "Don Gildardo Restrepo|Vuelva cuando quiera, que aquí sobra conversación y café."),
            ["antioquia_reto_ok"] = D(
                "Don Gildardo Restrepo|¡Eso, hombre! Sabe más historia que muchos.",
                "Don Gildardo Restrepo|Tome semillas de Caturra y un kilo de café del Suroeste antioqueño."),

            ["santander_mentor"] = D(
                "Doña Mercedes Ardila|Bienvenido a Barichara, el pueblo de piedra. Soy Mercedes.",
                "Doña Mercedes Ardila|En Santander hay gran tradición de café bajo sombra densa, con muchos árboles nativos. Eso favorece a las aves, incluidas las migratorias.",
                "Doña Mercedes Ardila|El guamo da nitrógeno al suelo, el plátano da sombra mientras crecen los árboles y además comida. La sombra protege el suelo y regula la humedad.",
                "Doña Mercedes Ardila|Aquí hay una sola cosecha al año, de septiembre a diciembre. Y el café es de cuerpo medio-alto, con chocolate y frutos secos.",
                "Doña Mercedes Ardila|Pero la sombra demasiado densa y la humedad traen hongos, como el ojo de gallo. ¿Sabe reconocer las plagas? Si pasa mi examen, le regalo semillas de Maragogipe."),
            ["santander_despues"] = D(
                "Doña Mercedes Ardila|Escuche: ese que canta es un pájaro que viene del norte cada año. Nuestros cafetales también son su casa."),
            ["santander_reto_ok"] = D(
                "Doña Mercedes Ardila|¡Muy bien! Así se cuida una finca sin pelear con la naturaleza.",
                "Doña Mercedes Ardila|Le regalo semillas de Maragogipe, el grano elefante, y un kilo de café de sombra."),

            ["nortesantander_mentor"] = D(
                "Profesor Álvaro Contreras|Bienvenido a Salazar de las Palmas. Aquí, según dicen, empezó el café comercial de Colombia.",
                "Profesor Álvaro Contreras|Cuenta la tradición que hacia 1834 el padre Francisco Romero ponía como penitencia sembrar cafetos: entre más grave el pecado, más matas.",
                "Profesor Álvaro Contreras|Ojo: es más leyenda que hecho comprobado. El café ya se sembraba en la región. Pero la anécdota se volvió símbolo de cómo se regó el grano por el nororiente.",
                "Profesor Álvaro Contreras|Lo que sí está registrado: en 1835 salieron por la aduana de Cúcuta unos 2.500 sacos rumbo a Maracaibo, por el río Zulia. La primera exportación formal.",
                "Profesor Álvaro Contreras|Le tengo un examen sobre los orígenes del café en Colombia. Si lo pasa, le regalo semillas de Típica, la variedad de los primeros cafetales."),
            ["nortesantander_despues"] = D(
                "Profesor Álvaro Contreras|La historia no es solo fechas, joven: es gente sembrando con esperanza."),
            ["nortesantander_reto_ok"] = D(
                "Profesor Álvaro Contreras|¡Excelente! Distingue la leyenda de la historia. Eso es lo más difícil.",
                "Profesor Álvaro Contreras|Tome semillas de Típica y un kilo de café de Salazar de las Palmas."),

            ["sierranevada_mentor"] = D(
                "Doña Marina Izquierdo|Bienvenido a Minca. Soy Marina. Desde aquí se ve cómo la Sierra Nevada sube del mar Caribe hasta picos nevados de más de 5.700 metros.",
                "Doña Marina Izquierdo|Es la montaña costera más alta del mundo y el territorio de los pueblos arhuaco, kogui, wiwa y kankuamo, que la consideran el corazón del mundo.",
                "Doña Marina Izquierdo|Muchas familias indígenas cultivan café orgánico bajo sombra y se han organizado en asociaciones que exportan directamente. Yo trabajo con algunas cooperativas.",
                "Doña Marina Izquierdo|Aquí hay una sola cosecha, hacia el final del año. Y aquí aprendimos a respetar: no se entra a ningún territorio sin permiso, y se escucha antes de hablar.",
                "Doña Marina Izquierdo|Ayúdeme a recoger. Solo las maduras, como debe ser. Si lo hace bien, le regalo semillas de Típica de nuestro vivero."),
            ["sierranevada_despues"] = D(
                "Doña Marina Izquierdo|Cuando cuente esta historia en su canal, cuéntela con respeto. Este café tiene detrás a mucha gente y mucho territorio."),
            ["sierranevada_reto_ok"] = D(
                "Doña Marina Izquierdo|Gracias por su trabajo. Así se recoge en la Sierra: con cuidado y sin afán.",
                "Doña Marina Izquierdo|Llévese semillas de Típica y un kilo de café de la cooperativa."),

            // ---- Letreros de las fincas ----
            ["cundinamarca_cartel"] = D(J + "|'Viotá, Cundinamarca. Tierra de haciendas cafeteras desde el siglo XIX.' El cañón del río Bogotá se ve al fondo."),
            ["quindio_cartel"] = D(J + "|'Salento, Quindío. Paisaje Cultural Cafetero, patrimonio de la humanidad (UNESCO, 2011).' Arriba, en Cocora, las palmas de cera se pierden en la neblina."),
            ["risaralda_cartel"] = D(J + "|'Belén de Umbría, Risaralda.' Huele a café recién despulpado y la quebrada suena entre la guadua."),
            ["caldas_cartel"] = D(J + "|'Chinchiná, Caldas. Parcelas experimentales: no tocar las matas.' Hay letreros con nombres de variedades en cada surco."),
            ["tolima_cartel"] = D(J + "|'Planadas, Tolima. Café de Tolima, denominación de origen.' Los cañones son tan profundos que las nubes se quedan abajo."),
            ["huila_cartel"] = D(J + "|'Pitalito, Huila.' Cada finca tiene su marquesina, ese secador de techo plástico curvo que protege el café de la lluvia."),
            ["cauca_cartel"] = D(J + "|'Inzá, Cauca. Cerca están los hipogeos de Tierradentro, tumbas subterráneas pintadas, patrimonio de la humanidad.'"),
            ["narino_cartel"] = D(J + "|'La Unión, Nariño. Café de altura.' Las parcelas parecen colgadas de la montaña, como cuadros de colores."),
            ["antioquia_cartel"] = D(J + "|'Jardín, Antioquia.' Por el camino de herradura baja una recua de mulas cargada de café, como hace cien años."),
            ["santander_cartel"] = D(J + "|'Barichara, Santander. Café bajo sombra, amigo de las aves.' Al fondo se abre el Cañón del Chicamocha."),
            ["nortesantander_cartel"] = D(J + "|'Salazar de las Palmas, Norte de Santander. Cuna del café en Colombia.' Los techos de teja brillan con el sol de la tarde."),
            ["sierranevada_cartel"] = D(J + "|'Minca, Sierra Nevada de Santa Marta.' Desde aquí se oye el mar a lo lejos y se ven los picos nevados entre las nubes."),
        };

        // ============================ PREGUNTAS ============================

        public static readonly Dictionary<string, QuizQuestion[]> Quizzes = new Dictionary<string, QuizQuestion[]>
        {
            ["historia"] = new[]
            {
                Q("¿Dónde nació el café arábica?", 1, "El Coffea arabica nació en los bosques de montaña del suroeste de Etiopía, como híbrido natural de dos especies.", "hist_kaldi",
                    "En Brasil", "En Etiopía", "En Colombia", "En Indonesia"),
                Q("La historia del pastor Kaldi y sus cabras inquietas es...", 2, "Es una leyenda: aparece escrita por primera vez en 1671, en un tratado en latín de Antoine Faustus Nairon.", "hist_kaldi",
                    "Un hecho comprobado del año 850", "Un relato de un viajero inglés", "Una leyenda escrita por primera vez en 1671"),
                Q("¿Quiénes son los primeros bebedores de café de los que hay evidencia confiable?", 0, "Los místicos sufíes del Yemen, a mediados del siglo XV, lo tomaban para aguantar sus rezos nocturnos.", "hist_yemen_moca",
                    "Los sufíes del Yemen", "Los monjes de Venecia", "Los incas", "Los samuráis de Japón"),
                Q("¿De dónde viene la palabra 'moka'?", 1, "Del puerto yemení de Moca, en el mar Rojo, por donde salía casi todo el café del mundo en los siglos XVI y XVII.", "hist_yemen_moca",
                    "De una marca de chocolate", "De un puerto del Yemen", "De una cafetera italiana", "De una ciudad de Brasil"),
                Q("A los cafés de Londres del siglo XVII les decían 'penny universities' porque...", 2, "Por un penique se tomaba un café y se escuchaba o participaba en los debates.", "hist_europa_cafes",
                    "Allí estudiaban los banqueros", "Eran parte de Oxford", "Por un penique uno tomaba café y escuchaba debates"),
                Q("¿Quién llevó el café a Brasil en 1727, según la historia?", 0, "Francisco de Melo Palheta llevó semillas desde la Guayana Francesa a Brasil en 1727.", "hist_palheta_brasil",
                    "Francisco de Melo Palheta", "Gabriel de Clieu", "Alan Adler", "José Gumilla"),
                Q("¿En qué año y ciudad se fundó la Federación Nacional de Cafeteros de Colombia?", 1, "Se fundó el 27 de junio de 1927 en Medellín, durante el Segundo Congreso Nacional de Cafeteros.", "hist_fnc_1927",
                    "1810, Bogotá", "1927, Medellín", "1958, Manizales", "1983, Chinchiná"),
                Q("Cenicafé, el centro de investigación del café, funciona en Chinchiná desde...", 2, "Desde 1938. Allí se desarrollaron variedades resistentes a la roya como Colombia y Castillo.", "hist_cenicafe_1938",
                    "1835", "1990", "1938", "2005"),
                Q("La variedad Colombia, resistente a la roya, se entregó en 1982. ¿Cuándo se confirmó la roya en el país?", 1, "En 1983, en Chinchiná (Caldas): ¡la variedad resistente llegó antes que la enfermedad!", "hist_roya_llega",
                    "En 1950", "En 1983", "En 2008", "Nunca llegó"),
                Q("¿Qué variedad resistente fue protagonista de la renovación tras la epidemia de roya de 2008–2011?", 3, "Castillo, entregada por Cenicafé en 2005, derivada de Caturra por Híbrido de Timor.", "hist_epidemia_roya_castillo",
                    "Típica", "Geisha", "Borbón", "Castillo"),
                Q("¿En qué año la UNESCO declaró patrimonio mundial el Paisaje Cultural Cafetero?", 0, "En 2011. Incluye zonas de Caldas, Quindío, Risaralda y el norte del Valle del Cauca.", "hist_paisaje_cultural_unesco",
                    "2011", "1927", "1999", "2020"),
                Q("¿Quién inventó la AeroPress y cuándo salió al mercado?", 2, "El ingeniero Alan Adler, creador del aro volador Aerobie, la presentó en noviembre de 2005.", "hist_aeropress_2005",
                    "Un barista italiano en 1947", "Una empresa japonesa en 1990", "Alan Adler, en 2005", "Melitta Bentz, en 1908"),
                Q("Para promocionar el café colombiano en Estados Unidos, entre 1958 y 1959 nació...", 1, "El personaje publicitario de un caficultor con su mula, creado para una campaña de los cafeteros colombianos.", "hist_juan_valdez_1958",
                    "El primer café de especialidad", "Un personaje publicitario: un caficultor con su mula", "La AeroPress", "La variedad Caturra"),
            },
            ["plagas"] = new[]
            {
                Q("¿Qué causa la roya del cafeto?", 0, "Un hongo, Hemileia vastatrix, que forma un polvillo naranja en el envés de las hojas.", "cul_roya",
                    "Un hongo: Hemileia vastatrix", "Un escarabajo", "La falta de agua", "Un virus de las raíces"),
                Q("¿Qué necesitan las esporas de la roya para germinar?", 2, "Agua líquida sobre la hoja. Por eso en interiores hay que evitar hojas mojadas por horas.", "cul_roya",
                    "Mucho sol", "Frío intenso", "Agua líquida sobre la hoja", "Suelo seco"),
                Q("La broca del café es...", 1, "Un cucarroncito de menos de 2 mm (Hypothenemus hampei) que perfora la cereza y se come la almendra.", "cul_broca",
                    "Un hongo naranja", "Un cucarrón diminuto que perfora la cereza", "Una enfermedad de la raíz", "Un pájaro"),
                Q("¿En qué año se detectó la broca en Colombia?", 2, "En 1988, en el sur de Nariño.", "cul_broca",
                    "1835", "1927", "1988", "2015"),
                Q("¿Qué es el Re-Re?", 0, "Recolección y repase: recoger a tiempo todos los frutos maduros y no dejar frutos maduros, sobremaduros o secos en el árbol ni en el suelo.", "cul_rere",
                    "Recolección y repase", "Riego y resiembra", "Revisar y renovar", "Recoger y regalar"),
                Q("¿Qué hongo se usa como control biológico de la broca?", 3, "Beauveria bassiana: infecta a la broca y la cubre con un moho blanco.", "cul_rere",
                    "Hemileia vastatrix", "Mycena citricolor", "Penicillium", "Beauveria bassiana"),
                Q("¿Cuál de estas variedades es resistente a la roya?", 1, "Castillo, de Cenicafé. Típica, Borbón y Caturra son susceptibles.", "cul_variedades_resistentes",
                    "Típica", "Castillo", "Borbón", "Caturra"),
                Q("El ojo de gallo (gotera) se dispara con...", 0, "Lluvias frecuentes, humedad por encima del 80 %, neblina, poco sol y sombrío muy denso.", "cul_ojo_de_gallo",
                    "Mucha humedad, neblina y sombrío muy denso", "Sol fuerte y sequía", "Exceso de abono", "Recolección temprana"),
                Q("¿Por qué no se deben dejar frutos secos en la mata o en el suelo?", 2, "Porque de esos frutos olvidados salen las brocas que atacan la nueva cosecha.", "cul_rere",
                    "Porque se pudren y huelen mal", "Porque atraen la roya", "Porque ahí se esconde la broca que ataca la nueva cosecha"),
            },
            ["bogota"] = new[]
            {
                Q("¿Cuál es el registro escrito más antiguo del café en territorio colombiano?", 1, "El del jesuita José Gumilla en 'El Orinoco ilustrado' (1741), sobre una siembra hacia 1730.", "hist_gumilla",
                    "Un diario de Simón Bolívar", "El Orinoco ilustrado, del jesuita José Gumilla", "Un contrato de una hacienda de Viotá", "Una carta del virrey"),
                Q("¿A qué temperatura hierve más o menos el agua en Bogotá?", 2, "A unos 91–92 °C: a 2.600 metros la presión del aire es menor que a nivel del mar.", "bog_altitud_ebullicion",
                    "100 °C", "96 °C", "91 °C", "80 °C"),
                Q("¿Por qué no hay cafetales al aire libre en Bogotá?", 0, "Porque a 2.600 m y unos 14 °C hace demasiado frío. En Colombia el café se da sobre todo entre 1.200 y 2.000 m.", "bog_2600",
                    "Porque hace demasiado frío", "Porque está prohibido", "Porque no llueve", "Porque el suelo es salado"),
                Q("En Colombia, un 'tinto' es...", 1, "Una taza pequeña de café negro, muchas veces con azúcar o panela.", "bog_tinto",
                    "Una copa de vino", "Una taza pequeña de café negro", "Café con leche", "Un café con licor"),
                Q("A mediados del siglo XX, los cafés del centro de Bogotá eran famosos por...", 3, "Las tertulias de escritores y poetas alrededor de un tinto.", "bog_cafes_historicos",
                    "Tostar café para exportar", "Las competencias de baristas", "Ser bancos", "Las tertulias de escritores y poetas"),
                Q("¿Por dónde salió la primera exportación formal de café colombiano, en 1835?", 0, "Por la aduana de Cúcuta, rumbo a Maracaibo por el río Zulia.", "hist_primeras_exportaciones",
                    "Por Cúcuta", "Por Cartagena", "Por Buenaventura", "Por Bogotá"),
                Q("La variedad Colombia (1982) se creó cruzando Caturra con...", 2, "Con el Híbrido de Timor, un cruce natural entre arábica y robusta que aporta resistencia a la roya.", "hist_variedad_colombia_1982",
                    "Geisha", "Típica", "Híbrido de Timor", "Pink Bourbon"),
            },
            ["antioquia"] = new[]
            {
                Q("¿Cómo bajaba el café de las montañas antes de las carreteras?", 1, "A lomo de mula, por caminos de herradura, guiado por los arrieros.", "col_arrieros",
                    "En helicóptero", "A lomo de mula, con arrieros", "Por tubería", "En globo"),
                Q("¿Qué ciudades nacieron de la colonización antioqueña del siglo XIX?", 0, "Manizales (1849) y Armenia (1889), entre muchos otros pueblos.", "hist_colonizacion_antioquena",
                    "Manizales y Armenia", "Cartagena y Santa Marta", "Bogotá y Tunja", "Pasto y Popayán"),
                Q("¿En qué ciudad se fundó la Federación Nacional de Cafeteros en 1927?", 2, "En Medellín, durante el Segundo Congreso Nacional de Cafeteros.", "hist_fnc_1927",
                    "Bogotá", "Manizales", "Medellín", "Cali"),
                Q("¿Cuál era la carga que llevaba el cable aéreo Manizales–Mariquita (terminado en 1922)?", 1, "Café en vagonetas colgantes, hacia el río Magdalena y de ahí al Caribe.", "col_cable_aereo",
                    "Pasajeros a la playa", "Café en vagonetas colgantes", "Petróleo", "Ganado"),
                Q("¿Con qué materiales se construyen las casas de bahareque del Eje Cafetero?", 3, "Con guadua y tierra, con balcones y puertas de colores.", "hist_colonizacion_antioquena",
                    "Concreto y acero", "Piedra y mármol", "Vidrio", "Guadua y tierra"),
                Q("El carriel, el poncho y el sombrero son símbolos de...", 0, "Del arriero paisa, que llevaba el café por los caminos de herradura.", "col_arrieros",
                    "El arriero paisa", "El llanero", "El pescador del Pacífico", "El barista de Bogotá"),
            },
            ["nortesantander"] = new[]
            {
                Q("La historia del padre Romero, que ponía como penitencia sembrar cafetos, es...", 2, "Es más tradición oral que hecho comprobado; el café ya se sembraba en la región. Pero se volvió un símbolo.", "hist_padre_romero",
                    "Un hecho documentado en un acta de 1834", "Un invento del siglo XXI", "Una tradición oral, más leyenda que hecho comprobado"),
                Q("¿Cuántos sacos de café salieron en la primera exportación formal de 1835?", 1, "Unos 2.500 sacos; las fuentes hablan de entre 2.560 y 2.592.", "hist_primeras_exportaciones",
                    "Unos 25", "Unos 2.500", "Unos 250.000", "Ninguno"),
                Q("¿Por qué río bajó la primera exportación hacia Maracaibo?", 0, "Por el río Zulia y el Catatumbo hasta el lago de Maracaibo, en Venezuela.", "hist_primeras_exportaciones",
                    "Por el río Zulia", "Por el Magdalena", "Por el Amazonas", "Por el Cauca"),
                Q("Según algunos historiadores, el café entró a Colombia...", 3, "Probablemente por Venezuela, desde las Guayanas y el Caribe.", "hist_jesuitas_colombia",
                    "Desde Japón", "Desde Brasil, por el Amazonas", "En avión", "Por Venezuela, desde las Guayanas y el Caribe"),
                Q("¿Qué religiosos dejaron el registro más antiguo del café en territorio colombiano?", 1, "Los jesuitas: José Gumilla escribió sobre café en una misión del Orinoco.", "hist_gumilla",
                    "Los franciscanos de Bogotá", "Los jesuitas del Orinoco", "Los monjes sufíes", "Los benedictinos de Viena"),
                Q("¿Qué variedad predominó en Colombia durante buena parte de su historia cafetera?", 0, "La Típica: porte alto, hojas nuevas color bronce y taza limpia, aunque susceptible a la roya.", "cul_variedades",
                    "Típica", "Castillo", "Geisha", "Cenicafé 1"),
            },
        };
    }
}
