// El Códice del Café: lo que el barista va aprendiendo. Historia, Colombia, cultivo, proceso, preparación y cata.
// Cada entrada se desbloquea con una misión, una expedición o una variedad bien preparada.
// Las entradas sobre personas y hechos reales son divulgación; Arcadia y sus personajes son ficción y parodia.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public static class Codex
    {
        public class Entry
        {
            public string id, category, title, text;
            public Entry(string id, string category, string title, string text) { this.id = id; this.category = category; this.title = title; this.text = text; }
        }

        public static readonly string[] Categories = { "Historia", "Colombia", "Cultivo", "Proceso", "Preparación", "Cata", "Variedades", "Arcadia" };

        public static readonly Entry[] Entries =
        {
            // ---------- Historia ----------
            new Entry("kaldi", "Historia", "Kaldi y las cabras bailarinas",
                "La leyenda dice que un pastor etíope llamado Kaldi vio a sus cabras brincar después de comer unas cerezas rojas de un arbusto de Kaffa. Probó las cerezas, sintió la energía y se las llevó a un monasterio. De Kaffa, y del árabe qahwa, viene la palabra café."),
            new Entry("yemen", "Historia", "Los sufíes de Yemen y el puerto de Moca",
                "Los primeros en tostar y preparar el café como bebida fueron los monjes sufíes de Yemen en el siglo XV: lo tomaban para mantenerse despiertos en sus rezos nocturnos. Durante dos siglos casi todo el café del mundo salió por el puerto de Al-Makha, que le dejó el nombre al moca."),
            new Entry("babaBudan", "Historia", "Las siete semillas de Baba Budan",
                "Yemen prohibía sacar semillas fértiles: solo exportaba café tostado o hervido. Hacia 1670 el peregrino Baba Budan escondió siete semillas en su ropa y las sembró en las montañas de Chikmagalur, en India. El número siete era sagrado, así que no lo consideraron robo."),
            new Entry("deClieu", "Historia", "El marino que compartió su agua con una mata",
                "En 1723 Gabriel de Clieu cruzó el Atlántico con una plántula de café del jardín del rey de Francia. Cuentan que, cuando el agua escaseó en el barco, compartió su ración con la planta. De esa mata sembrada en Martinica descienden millones de cafetos del Caribe y Suramérica."),
            new Entry("papa", "Historia", "El café bautizado",
                "Hacia 1600 le pidieron al papa Clemente VIII prohibir el café por ser bebida de infieles. Según la historia, lo probó y dijo que era tan delicioso que sería una lástima dejárselo solo a ellos, y lo bautizó. Verdad o no, el café conquistó Europa en el siglo XVII."),
            new Entry("londres", "Historia", "Las universidades de un penique",
                "A los cafés de Londres del siglo XVII los llamaban penny universities: por un penique tenías café y conversación con científicos, comerciantes y poetas. En uno de ellos, el de Edward Lloyd, los armadores se reunían a asegurar barcos: hoy es Lloyd's of London."),
            new Entry("bach", "Historia", "Bach, Beethoven y Balzac",
                "Bach compuso una cantata sobre una joven adicta al café (BWV 211). Beethoven, dicen, contaba 60 granos exactos por taza. Y Balzac escribió sobre cómo el café le hacía marchar las ideas como batallones. El café lleva tres siglos acompañando a los que crean de noche."),
            new Entry("boston", "Historia", "Cuando el té se volvió antipatriótico",
                "Tras el Motín del Té de Boston (1773), tomar café se volvió un gesto patriótico en Estados Unidos. Ese cambio de costumbre convirtió al país en el mayor consumidor de café del mundo, y en el gran cliente del café colombiano durante el siglo XX."),
            new Entry("melitta", "Historia", "Melitta y el filtro de papel",
                "En 1908, en Dresde, Melitta Bentz se cansó del sedimento en la taza: perforó una olla de latón y puso adentro una hoja de papel secante del cuaderno de su hijo. Patentó el filtro de papel y fundó una empresa que todavía lleva su nombre."),
            new Entry("espresso", "Historia", "El espresso: presión y crema",
                "Angelo Moriondo patentó en Turín (1884) la primera máquina de café a vapor; Luigi Bezzera la hizo para una taza a la vez en 1901. La crema llegó en 1947 con la palanca de Achille Gaggia, que empujaba el agua a 9 bares. La AeroPress trabaja con la misma idea, a mano y a menos presión."),
            new Entry("chemex", "Historia", "La Chemex y la tercera ola",
                "La Chemex la diseñó el químico Peter Schlumbohm en 1941 y está en el MoMA. Medio siglo después, Trish Rothgeb bautizó como tercera ola a la cultura que trata el café como un producto de origen, igual que el vino: trazabilidad, tueste claro y métodos de filtrado."),
            new Entry("aeropress", "Historia", "La AeroPress: inventada por un ingeniero de frisbees",
                "Alan Adler, el inventor del frisbee Aerobie, quería una sola taza buena sin amargor. En 2005 presentó la AeroPress: un émbolo que empuja el agua a través del café y un filtro. Es barata, irrompible y viaja en cualquier maleta. Por eso se volvió el método de los baristas nómadas."),
            new Entry("wac", "Historia", "El Campeonato Mundial de AeroPress",
                "El primer World AeroPress Championship se hizo en Oslo en 2008, con tres competidores en una tostaduría. Hoy compiten decenas de países: cada barista tiene ocho minutos para preparar la mejor taza y un jurado la elige a ciegas, señalando la taza ganadora al mismo tiempo."),

            // ---------- Colombia ----------
            new Entry("jesuitas", "Colombia", "Cómo llegó el café a Colombia",
                "Los jesuitas trajeron el café a la Nueva Granada a comienzos del siglo XVIII. Las primeras siembras con vocación comercial fueron en Santander y Norte de Santander, y la primera exportación registrada salió por Cúcuta en 1835 rumbo a Venezuela: unos 2.500 sacos."),
            new Entry("romero", "Colombia", "La penitencia del padre Romero",
                "Cuentan que en Salazar de las Palmas el cura Francisco Romero imponía como penitencia sembrar cafetos: tres o cuatro matas por pecado. Así, confesión a confesión, se poblaron de café las montañas de Norte de Santander. Es la historia fundacional favorita del café colombiano."),
            new Entry("tinto", "Colombia", "El tinto, el perico y el pintado",
                "En Colombia tinto no es vino: es el café negro de todos los días, servido en vaso pequeño y a veces con panela. En Bogotá, si le pones un poco de leche, es un perico; en otras regiones, un pintado. El tinto se vende en termo en las esquinas, en vaso de icopor, a cualquier hora."),
            new Entry("fnc", "Colombia", "1927: los caficultores se organizan",
                "En 1927 los caficultores colombianos fundaron en Medellín una federación gremial para defender el precio, investigar y promover el café del país. De ahí nacieron el centro de investigación de Chinchiná y, en 1959, una campaña publicitaria con un arriero y una mula que volvió famoso el café colombiano en el mundo."),
            new Entry("cenicafe", "Colombia", "Cenicafé, el laboratorio de Chinchiná",
                "Fundado en 1938 en Chinchiná (Caldas), Cenicafé es el centro que investiga el café colombiano: creó las variedades Colombia (1982), Tabi (2002), Castillo (2005) y Cenicafé 1 (2016), todas resistentes a la roya, y estudia desde el suelo hasta el secado."),
            new Entry("roya", "Colombia", "La roya de 2008 y la variedad Castillo",
                "Entre 2008 y 2011 la roya, un hongo que seca las hojas del cafeto, tumbó la cosecha colombiana a su nivel más bajo en décadas. La respuesta fue renovar millones de cafetos con la variedad Castillo, resistente y de buena taza. Hoy la mayoría del café colombiano es Castillo."),
            new Entry("paisaje", "Colombia", "El Paisaje Cultural Cafetero",
                "En 2011 la UNESCO declaró Patrimonio de la Humanidad el Paisaje Cultural Cafetero: 47 municipios de Caldas, Quindío, Risaralda y Valle donde el café se siembra en laderas imposibles, entre casas de bahareque de colores, platanales y guaduales."),
            new Entry("chapoleras", "Colombia", "Chapoleras y yipaos",
                "A las recolectoras de café del Eje Cafetero las llaman chapoleras, por la chapola, la mariposa que revolotea entre los cafetales. Y el yipao es el jeep Willys cargado hasta lo imposible con bultos, plátanos y hasta la familia: en Calarcá hay un desfile de yipaos cada año."),
            new Entry("huila", "Colombia", "Huila, la nueva capital del café",
                "Desde la década de 2010 el Huila es el departamento que más café produce en Colombia, y el que más gana en la Taza de Excelencia, el concurso que subasta los mejores lotes del país. Pitalito, Acevedo y San Agustín son nombres que los compradores del mundo buscan en las etiquetas."),
            new Entry("narino", "Colombia", "Nariño: café a 2.300 metros",
                "En Nariño el café crece hasta los 2.300 metros, una altura casi imposible en otros países. Es posible por la cercanía al ecuador y por la inversión térmica de los cañones del Juanambú y el Guáitara: el aire caliente de abajo sube de noche y protege los cafetales del frío."),
            new Entry("sierra", "Colombia", "La Sierra Nevada y los pueblos arhuacos",
                "En la Sierra Nevada de Santa Marta, la montaña costera más alta del mundo, los pueblos arhuaco, kogui y wiwa cultivan café bajo sombra, orgánico, como parte de su relación con la tierra. Lo exportan con sus propias marcas y lo tuestan a 20 kilómetros del mar Caribe."),
            new Entry("mitaca", "Colombia", "Dos cosechas al año",
                "Colombia es de los pocos países con café fresco todo el año: por estar sobre el ecuador tiene una cosecha principal y una segunda más pequeña, la mitaca. Y se recoge a mano, cereza por cereza, escogiendo solo las maduras: por eso el café colombiano se exporta como suave lavado."),
            new Entry("bogota", "Colombia", "Café en Bogotá, a 2.600 metros",
                "A 2.600 metros y 14 grados promedio, Bogotá está demasiado alta y fría para que el café madure al aire libre. Por eso Float Gardens cultiva adentro: casas, bodegas y azoteas con invernaderos, luz LED y raíces en agua. La ciudad que toma más tinto del país, cultivándolo por primera vez."),

            // ---------- Cultivo ----------
            new Entry("arabica", "Cultivo", "Arábica y robusta",
                "Hay dos especies que importan: Coffea arabica, de altura, aromática, con 44 cromosomas y 1,2-1,5 % de cafeína; y Coffea canephora (robusta), de tierras bajas, más amarga, con el doble de cafeína y más resistente. Colombia siembra solo arábica."),
            new Entry("altura", "Cultivo", "Por qué importa la altura",
                "Entre más alto crece el café, más despacio madura la cereza: el grano sale más denso, con más azúcares y ácidos, y la taza es más compleja. El café colombiano crece entre 1.200 y 2.000 metros; abajo es suave y achocolatado, arriba es floral y vibrante."),
            new Entry("cereza", "Cultivo", "Anatomía de una cereza",
                "La cereza de café tiene piel, pulpa dulce, un mucílago pegajoso, una cáscara de pergamino y una película plateada. Adentro van dos semillas planas, una contra la otra. Cuando solo se forma una, redonda, se llama caracol. El fruto tarda unos ocho meses en madurar."),
            new Entry("sombra", "Cultivo", "Sombra, suelo y agua",
                "El cafeto es un arbusto de sotobosque: le gustan los 18-22 grados, la sombra parcial de plátanos y guamos, y suelos volcánicos con buena agua. En hidroponía esas condiciones se recrean con luz LED de 14 horas, solución nutritiva y temperatura controlada."),
            new Entry("hidroponia", "Cultivo", "Hidroponía: raíces en el agua",
                "Hidroponía es cultivar sin suelo: las raíces flotan en agua con nutrientes disueltos y oxígeno. Usa hasta 90 % menos agua que un cultivo en tierra y permite sembrar en pisos apilados dentro de una bodega. Con café todavía es experimental; por eso Float Gardens es un proyecto y una apuesta."),

            // ---------- Proceso ----------
            new Entry("lavado", "Proceso", "El proceso lavado",
                "El café colombiano clásico es lavado: el mismo día de la cosecha se despulpa la cereza, el grano fermenta entre 12 y 36 horas para soltar el mucílago, se lava y se seca al sol o en silos hasta el 10-12 % de humedad. El resultado es una taza limpia y de acidez brillante."),
            new Entry("natural", "Proceso", "Naturales, honey y anaeróbicos",
                "En el proceso natural la cereza entera se seca al sol durante semanas y el grano absorbe la fruta: taza dulce y vinosa. El honey seca el grano con parte del mucílago. Y en los anaeróbicos la fermentación se hace en tanques sin oxígeno, con sabores de canela, licor o fruta tropical."),
            new Entry("pergamino", "Proceso", "Pergamino, trilla y excelso",
                "El caficultor vende café pergamino seco. En la trilladora se quita esa cáscara y queda el café verde, que se clasifica por tamaño y defectos. El grado de exportación colombiano se llama excelso. Un saco de exportación pesa 70 kilos."),
            new Entry("tueste", "Proceso", "El tueste y los dos cracks",
                "Al tostar, el grano verde pasa de 120 grados sin aroma a un primer crack cerca de los 196 grados, cuando el vapor revienta la celulosa. Si sigue, viene un segundo crack hacia los 224 grados y el tueste oscuro. La reacción de Maillard entre azúcares y aminoácidos crea los cientos de aromas del café."),

            // ---------- Preparación ----------
            new Entry("variables", "Preparación", "Las cinco variables",
                "Toda preparación se controla con cinco perillas: la proporción café/agua (1:15 a 1:17 es el punto clásico), la molienda, la temperatura del agua (90-96 grados para la mayoría, menos para tuestes claros delicados), el tiempo de contacto y la agitación. Cambia una a la vez."),
            new Entry("extraccion", "Preparación", "TDS y porcentaje de extracción",
                "El café disuelto en la taza se mide como sólidos totales disueltos (TDS): entre 1,15 y 1,35 % es el rango de la taza dorada. El porcentaje de extracción dice cuánto del café molido terminó en el agua: 18-22 %. Menos, y sabe ácido y aguado; más, y sabe amargo y astringente."),
            new Entry("invertido", "Preparación", "AeroPress: método estándar e invertido",
                "En el método estándar el agua empieza a gotear por el filtro desde el primer segundo. En el invertido la AeroPress se arma boca abajo, se deja infusionar sin goteo y se voltea sobre la taza para presionar: más control, más cuerpo. Casi todos los campeones del mundo han usado una variante del invertido."),
            new Entry("agua", "Preparación", "El agua es el 98 % de la taza",
                "El agua de Bogotá es blanda, y eso ayuda: poca dureza deja brillar la acidez. Lo ideal son 75-150 ppm de minerales y nada de cloro. Un café extraordinario con agua mala es un café malo; por eso los campeonatos llevan su propia agua."),
            new Entry("molienda", "Preparación", "Molienda: la variable que más manda",
                "Moler más fino expone más superficie y acelera la extracción; más grueso la frena. Para AeroPress se usa una molienda media-fina, entre el espresso y el filtrado. Un molino de muelas da partículas parejas; uno de cuchillas, polvo y piedras al mismo tiempo, y la taza sale amarga y aguada a la vez."),

            // ---------- Cata ----------
            new Entry("sca", "Cata", "La escala de 100 puntos",
                "Los catadores profesionales (Q graders) puntúan fragancia, sabor, acidez, cuerpo, balance, dulzor, uniformidad, taza limpia y posgusto. Un café con 80 puntos o más es de especialidad; de 86 para arriba gana concursos; de 90 en adelante casi no existe."),
            new Entry("catacion", "Cata", "Cómo se cata: sorber con ruido",
                "La catación tiene protocolo: 8,25 gramos por 150 ml de agua a 93 grados, cuatro minutos de reposo, se rompe la costra con la cuchara y se huele. Luego se sorbe con fuerza, haciendo ruido, para rociar el café por toda la boca y la nariz. Las tazas se evalúan a ciegas."),
            new Entry("notas", "Cata", "La rueda de sabores",
                "El café tiene más de 800 compuestos aromáticos, más que el vino. Para nombrarlos existe una rueda de sabores: frutal, floral, dulce, nuez y cacao, especias, tostado, verde y defectos. Cuando alguien dice que un café sabe a mandarina y panela, no está inventando: está leyendo la rueda."),
            new Entry("geisha", "Cata", "La Geisha de La Esmeralda",
                "La variedad Geisha, originaria del bosque de Gesha en Etiopía, era casi desconocida hasta que en 2004 un lote de la Hacienda La Esmeralda, en Panamá, ganó el Best of Panama con notas de jazmín y bergamota. Desde entonces es el café más caro del mundo y también se siembra en Colombia."),
            new Entry("precio", "Cata", "El precio C y la prima colombiana",
                "El café se negocia en la bolsa de Nueva York con el contrato C, en centavos de dólar por libra. El café colombiano se paga con una prima sobre ese precio por su calidad. Pero un café de especialidad sale de la bolsa: se vende por su nombre, su finca y su puntaje."),

            // ---------- Arcadia (ficción) ----------
            new Entry("arcadia", "Arcadia", "Arcadia Bean Corp.",
                "Arcadia Bean Corp. es el conglomerado que, en la Neo-Bogotá de 2087, patentó la genética de las variedades nativas y decidió que el café bueno se produce en fincas certificadas por ella y se vende con su sello. Inspectores, formularios y multas por innovar. Su CEO, Don Monopolio, no ha probado un café que no haya firmado antes. Ficción y parodia: nada de esto existe."),
            new Entry("floatgardens", "Arcadia", "Float Gardens: la idea",
                "Float Gardens empezó con una AeroPress, un computador y una pregunta: ¿por qué la ciudad que más tinto toma no cultiva ni una mata? La respuesta fue colaborativa: casas, bodegas y azoteas prestadas, raíces en el agua, datos compartidos en un PC y café gratis para quien ayude a farmear."),
        };

        static readonly Dictionary<string, Entry> byId = new Dictionary<string, Entry>();

        public static Entry Get(string id)
        {
            if (byId.Count == 0) foreach (var e in Entries) byId[e.id] = e;
            return byId.TryGetValue(id, out var r) ? r : null;
        }

        // ---------- Variedades ----------
        public class Variety
        {
            public string id, name, origin, notes;
            // Preferencias de preparación en AeroPress: temperatura y molienda (1 fina .. 10 gruesa) ideales
            public float temp, grind;
            public Variety(string id, string name, string origin, string notes, float temp, float grind)
            { this.id = id; this.name = name; this.origin = origin; this.notes = notes; this.temp = temp; this.grind = grind; }
        }

        public static readonly Variety[] Varieties =
        {
            new Variety("castillo", "Castillo", "Cenicafé, 2005", "Caramelo, manzana roja, cuerpo medio. La variedad de la Colombia que resistió la roya.", 93, 5.5f),
            new Variety("caturra", "Caturra", "Mutación de Bourbon, Brasil", "Cítrico, panela, limpio. Pequeña y productiva; fue la reina del Eje Cafetero.", 92, 5.5f),
            new Variety("tipica", "Típica", "Yemen → Java → Martinica", "Dulce, delicado, té negro. La variedad que llegó con los jesuitas.", 91, 5f),
            new Variety("bourbon", "Bourbon", "Isla de Reunión", "Chocolate, frutos rojos, cuerpo sedoso.", 92, 5.5f),
            new Variety("tabi", "Tabi", "Cenicafé, 2002", "Floral, naranja, elegante. Típica + Bourbon + Híbrido de Timor.", 90, 5f),
            new Variety("pink", "Pink Bourbon", "Huila", "Hibisco, lulo, miel. La sensación de los concursos recientes.", 89, 4.5f),
            new Variety("geisha", "Geisha", "Gesha, Etiopía → Panamá", "Jazmín, bergamota, durazno. Delicada: agua más fría y molienda más fina.", 87, 4.5f),
            new Variety("cenicafe1", "Cenicafé 1", "Cenicafé, 2016", "Dulce, nuez, cítrico suave. Resistente a roya y a la enfermedad de las cerezas.", 93, 6f),
            new Variety("wush", "Wush Wush", "Etiopía (sembrada en Colombia)", "Fresa, flores, tropical. Rara y de bajo rendimiento.", 88, 4.5f),
            new Variety("maragogipe", "Maragogipe", "Bahía, Brasil", "Grano gigante; suave, herbal, cuerpo ligero.", 91, 6f),
        };

        public static Variety GetVariety(string id)
        {
            foreach (var v in Varieties) if (v.id == id) return v;
            return Varieties[0];
        }

        // ---------- Regiones de Colombia (expediciones) ----------
        public class Region
        {
            public string id, name, dept, altitude, profile, unlockHint;
            public float lat, lon;
            public string[] varieties; // ids de variedades que regala
            public string[] codex;     // entradas que desbloquea
            public int chapter;        // capítulo que la abre (0 = desde el inicio)
            public Region(string id, string name, string dept, float lat, float lon, string altitude, string profile, int chapter, string[] varieties, string[] codex, string unlockHint)
            {
                this.id = id; this.name = name; this.dept = dept; this.lat = lat; this.lon = lon; this.altitude = altitude; this.profile = profile;
                this.chapter = chapter; this.varieties = varieties; this.codex = codex; this.unlockHint = unlockHint;
            }
        }

        public static readonly Region[] Regions =
        {
            new Region("bogota", "Bogotá", "Cundinamarca", 4.71f, -74.07f, "2.600 m (invernaderos)", "Cultivo urbano hidropónico bajo LED. Donde empieza todo.", 0,
                new[] { "castillo" }, new[] { "bogota", "hidroponia" }, "Disponible desde el inicio"),
            new Region("cundinamarca", "Tequendama y Sumapaz", "Cundinamarca", 4.35f, -74.4f, "1.400-1.900 m", "Viotá y Fusagasugá: las primeras grandes haciendas del centro del país, a dos horas de Bogotá.", 1,
                new[] { "tipica" }, new[] { "cereza", "sombra" }, "Capítulo 1: una AeroPress y un PC"),
            new Region("santander", "Santander y Norte de Santander", "Santander", 7.1f, -73.1f, "1.200-1.700 m", "La cuna: jesuitas, el padre Romero y la primera exportación por Cúcuta (1835). Café bajo sombra, achocolatado.", 2,
                new[] { "bourbon" }, new[] { "jesuitas", "romero", "fnc" }, "Capítulo 2: los que saben"),
            new Region("eje", "Eje Cafetero", "Caldas, Quindío, Risaralda", 4.95f, -75.6f, "1.300-1.900 m", "Paisaje Cultural Cafetero, Cenicafé en Chinchiná, chapoleras y yipaos. Cítrico, panela, balance.", 2,
                new[] { "caturra", "cenicafe1" }, new[] { "cenicafe", "paisaje", "chapoleras", "roya" }, "Capítulo 2: los que saben"),
            new Region("huila", "Huila", "Huila", 2.0f, -75.9f, "1.500-2.000 m", "El mayor productor y el rey de la Taza de Excelencia. Pitalito, Acevedo, San Agustín. Frutal y dulce.", 3,
                new[] { "pink", "geisha" }, new[] { "huila", "geisha", "precio" }, "Capítulo 3: Taza de Excelencia"),
            new Region("narino", "Nariño", "Nariño", 1.45f, -77.3f, "1.800-2.300 m", "El café más alto de Colombia, cañones con inversión térmica. Acidez eléctrica, cuerpo ligero.", 3,
                new[] { "tabi" }, new[] { "narino", "altura" }, "Capítulo 3: Taza de Excelencia"),
            new Region("cauca", "Cauca", "Cauca", 2.45f, -76.6f, "1.700-2.100 m", "Popayán, Inzá, Piendamó: meseta alta con mañanas frías. Floral, limpio, posgusto largo.", 3,
                new[] { "wush" }, new[] { "mitaca", "natural" }, "Capítulo 3: Taza de Excelencia"),
            new Region("tolima", "Tolima", "Tolima", 3.6f, -75.6f, "1.500-2.000 m", "Planadas y Chaparral: el café de las zonas que dejaron atrás la guerra. Dulce, panela, frutos rojos.", 4,
                new[] { "maragogipe" }, new[] { "lavado", "pergamino" }, "Capítulo 4: Arcadia"),
            new Region("antioquia", "Suroeste antioqueño", "Antioquia", 5.8f, -75.9f, "1.500-2.100 m", "Jardín, Ciudad Bolívar, Urrao: pueblos de balcones y fondas. Cuerpo, chocolate, naranja.", 4,
                new[] { "bourbon", "castillo" }, new[] { "tueste", "tinto" }, "Capítulo 4: Arcadia"),
            new Region("sierra", "Sierra Nevada de Santa Marta", "Magdalena y Cesar", 10.8f, -73.7f, "900-1.700 m", "Café orgánico bajo sombra de los pueblos arhuaco, kogui y wiwa, a la vista del mar. Cacao, nuez, suave.", 4,
                new[] { "tipica" }, new[] { "sierra", "agua" }, "Capítulo 4: Arcadia"),
        };

        public static Region GetRegion(string id)
        {
            foreach (var r in Regions) if (r.id == id) return r;
            return null;
        }

        // Contorno aproximado de Colombia (latitud, longitud) para dibujar el mapa de expediciones
        public static readonly Vector2[] Outline =
        {
            new Vector2(12.45f, -71.67f), new Vector2(11.9f, -72.3f), new Vector2(11.3f, -73.3f), new Vector2(11.05f, -74.2f), new Vector2(10.9f, -74.8f),
            new Vector2(10.4f, -75.5f), new Vector2(9.4f, -75.9f), new Vector2(8.7f, -76.9f), new Vector2(8.2f, -76.9f), new Vector2(8.6f, -77.3f),
            new Vector2(7.9f, -77.4f), new Vector2(7.2f, -77.9f), new Vector2(6.3f, -77.4f), new Vector2(5.5f, -77.4f), new Vector2(4.3f, -77.5f),
            new Vector2(3.3f, -77.6f), new Vector2(2.4f, -78.5f), new Vector2(1.5f, -79.0f), new Vector2(1.2f, -78.8f), new Vector2(0.9f, -77.6f),
            new Vector2(0.3f, -77.0f), new Vector2(0.0f, -76.3f), new Vector2(-0.5f, -75.3f), new Vector2(-1.5f, -74.5f), new Vector2(-2.4f, -72.9f),
            new Vector2(-4.2f, -69.9f), new Vector2(-3.9f, -70.3f), new Vector2(-1.2f, -69.6f), new Vector2(0.6f, -69.5f), new Vector2(1.1f, -69.8f),
            new Vector2(1.9f, -67.1f), new Vector2(2.5f, -67.2f), new Vector2(3.5f, -67.4f), new Vector2(6.2f, -67.5f), new Vector2(7.1f, -70.7f),
            new Vector2(7.0f, -72.0f), new Vector2(7.9f, -72.5f), new Vector2(8.6f, -72.9f), new Vector2(10.0f, -72.9f), new Vector2(11.0f, -72.0f),
        };
    }
}
