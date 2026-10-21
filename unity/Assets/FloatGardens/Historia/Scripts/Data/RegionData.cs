// Las regiones del mapa de Colombia: datos reales (altura, cosechas, perfil de taza, paisaje) y mentores ficticios.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    public static class RegionData
    {
        static Color C(string hex) => FGMaterials.Hex(hex);

        public static readonly List<Region> All = new List<Region>
        {
            new Region
            {
                id = "bogota", name = "Bogotá", department = "Bogotá D.C.", town = "La Candelaria", lat = 4.598f, lon = -74.076f,
                altitudeMin = 2600, altitudeMax = 2640, chapter = 0, travelHours = 0,
                summary = "La capital, a 2.600 m: demasiado fría para sembrar café al aire libre, pero aquí llega el café de todo el país. Aquí empezó tu sueño.",
                cupProfile = "No hay finca: Bogotá es la ciudad-taza. Aquí aprendes a reconocer cada región en tu AeroPress.",
                harvests = "Ninguna al aire libre. En Float Gardens las balsas flotantes dan cosechas pequeñas según el clima que programes.",
                landscape = "Altiplano frío y nublado, cerros orientales con Monserrate, ladrillo, teja de barro y lluvia fina.",
                reto = Reto.Historia, mentorName = "Doña Inés Galindo", mentorRole = "historiadora y barista de un café centenario de La Candelaria",
                giftVariety = "colombia", wikiUnlocks = new[] { "bog_tinto", "bog_cafes_historicos", "bog_2600" },
                style = new FincaStyle { ground = C("#5b6b4f"), foliage = C("#2f5a37"), sky = C("#a9b8c6"), fog = C("#b9c4cc"), fogEnd = 220f, slope = 0.2f, temperatureC = 14f, colonialTown = true, jeep = false, plantains = false, shadeTrees = false },
            },
            new Region
            {
                id = "cundinamarca", name = "Cundinamarca", department = "Cundinamarca", town = "Viotá", lat = 4.439f, lon = -74.522f,
                altitudeMin = 1000, altitudeMax = 1900, chapter = 1, travelHours = 3,
                summary = "La primera bajada de la sabana fría al clima templado. Tierra de viejas haciendas cafeteras del siglo XIX.",
                cupProfile = "Balanceado, de acidez y cuerpo medios, con notas dulces a panela y chocolate en las fincas altas.",
                harvests = "Principal en el primer semestre (abril a junio) y mitaca en el segundo.",
                landscape = "Carretera en curvas, cafetales bajo guamos y plátanos, casonas de hacienda blancas y el cañón del río Bogotá.",
                reto = Reto.Beneficio, mentorName = "Don Aurelio Rincón", mentorRole = "administrador de una antigua hacienda cafetera en Viotá",
                giftVariety = "bourbon", wikiUnlocks = new[] { "hist_haciendas_viota", "ben_lavado", "col_sombrio" },
                style = new FincaStyle { ground = C("#6a7f3c"), foliage = C("#2e6b2f"), sky = C("#9fc5e3"), fog = C("#cfdbe0"), fogEnd = 260f, slope = 0.55f, temperatureC = 22f, river = true },
            },
            new Region
            {
                id = "quindio", name = "Quindío", department = "Quindío", town = "Salento", lat = 4.637f, lon = -75.570f,
                altitudeMin = 1200, altitudeMax = 1800, chapter = 2, travelHours = 8,
                summary = "El corazón del Paisaje Cultural Cafetero: Jeep Willys, bahareque de colores y las palmas de cera del Valle de Cocora.",
                cupProfile = "El clásico suave colombiano: acidez media-alta, cuerpo medio, dulzor a caramelo y frutas suaves.",
                harvests = "Dos cosechas parecidas: una hacia mitad de año y otra de septiembre a diciembre.",
                landscape = "Colinas verde intenso, guaduales, casas de bahareque con balcones rojos, azules y amarillos, palmas de cera entre la neblina.",
                reto = Reto.Recoleccion, mentorName = "Doña Rosalba Marín", mentorRole = "caficultora y recolectora de Salento",
                giftVariety = "tabi", wikiUnlocks = new[] { "col_jeep_willys", "col_palma_cera", "col_recolectores", "hist_paisaje_cultural_unesco" },
                style = new FincaStyle { ground = C("#5f8a34"), foliage = C("#2d6e2c"), sky = C("#a7cbe6"), fog = C("#d5e0e4"), fogEnd = 240f, slope = 0.6f, temperatureC = 19f, waxPalms = true, colonialTown = true },
            },
            new Region
            {
                id = "risaralda", name = "Risaralda", department = "Risaralda", town = "Belén de Umbría", lat = 5.200f, lon = -75.868f,
                altitudeMin = 1200, altitudeMax = 1900, chapter = 2, travelHours = 9,
                summary = "Lomas empinadas con guadua y quebradas, cruce de caminos del Eje Cafetero y fincas que preparan su propio café.",
                cupProfile = "Taza limpia y balanceada, acidez media-alta, cuerpo medio, caramelo y cítricos.",
                harvests = "Principal de septiembre a diciembre y mitaca entre abril y junio.",
                landscape = "Lomas con cafetales y franjas de guadua, árboles de sombrío llenos de aves y los nevados de la cordillera Central entre nubes.",
                reto = Reto.Aeropress, mentorName = "Julián Henao", mentorRole = "barista de finca en Belén de Umbría",
                giftVariety = "cenicafe1", wikiUnlocks = new[] { "pre_extraccion_tds", "pre_carta_control", "col_cosecha_mitaca" },
                style = new FincaStyle { ground = C("#5c8436"), foliage = C("#2b682e"), sky = C("#a3c8e4"), fog = C("#d2dee3"), fogEnd = 250f, slope = 0.7f, temperatureC = 20f, snowPeaks = true, river = true },
            },
            new Region
            {
                id = "caldas", name = "Caldas", department = "Caldas", town = "Chinchiná", lat = 4.983f, lon = -75.604f,
                altitudeMin = 1200, altitudeMax = 1900, chapter = 2, travelHours = 8,
                summary = "Ciencia cafetera: en Chinchiná se investigan desde 1938 las variedades resistentes a la roya. El Nevado del Ruiz vigila el horizonte.",
                cupProfile = "Acidez media-alta, cuerpo medio, taza limpia con caramelo y frutas: una referencia del lavado colombiano.",
                harvests = "Principal de septiembre a diciembre y mitaca de abril a junio.",
                landscape = "Laderas de cafetal, parcelas experimentales con letreros de variedades y un volcán nevado que echa humo.",
                reto = Reto.Plagas, mentorName = "Dra. Camila Arango", mentorRole = "agrónoma investigadora en Chinchiná",
                giftVariety = "castillo", wikiUnlocks = new[] { "hist_cenicafe_1938", "cul_roya", "cul_broca", "hist_variedad_colombia_1982" },
                style = new FincaStyle { ground = C("#5a7f37"), foliage = C("#2c6a30"), sky = C("#9fc3df"), fog = C("#d0dbe0"), fogEnd = 250f, slope = 0.65f, temperatureC = 20f, volcano = true, snowPeaks = true },
            },
            new Region
            {
                id = "tolima", name = "Tolima", department = "Tolima", town = "Planadas", lat = 3.197f, lon = -75.644f,
                altitudeMin = 1200, altitudeMax = 2100, chapter = 3, travelHours = 9,
                summary = "El sur del Tolima pasó del conflicto a los cafés especiales de pequeños productores, incluidas comunidades nasa.",
                cupProfile = "Acidez media-alta, cuerpo limpio y suave, dulzor y notas cítricas y frutales.",
                harvests = "En el sur, principal en el primer semestre y mitaca en el segundo.",
                landscape = "Cañones profundos, ríos rápidos, cafetales en laderas pronunciadas rodeados de bosque nativo y atardeceres naranjas.",
                reto = Reto.Catacion, mentorName = "Yesenia Lozano", mentorRole = "catadora de Planadas",
                giftVariety = "wushwush", wikiUnlocks = new[] { "cat_protocolo_sca", "cat_rueda_sabores", "cat_puntaje_especialidad" },
                style = new FincaStyle { ground = C("#5d7d33"), foliage = C("#29632b"), sky = C("#a4c6e2"), fog = C("#cfd9de"), fogEnd = 230f, slope = 0.8f, temperatureC = 20f, river = true, mule = true },
            },
            new Region
            {
                id = "huila", name = "Huila", department = "Huila", town = "Pitalito", lat = 1.854f, lon = -76.051f,
                altitudeMin = 1200, altitudeMax = 2000, chapter = 3, travelHours = 8,
                summary = "El primer productor de café de Colombia y tierra de experimentos con fermentaciones. Aquí nace el río Magdalena.",
                cupProfile = "Aroma intenso, acidez media-alta, cuerpo medio, dulzor a caramelo y notas frutales.",
                harvests = "En el sur (Pitalito, Acevedo) la principal suele llegar en el segundo semestre.",
                landscape = "Valle alto del Magdalena entre cordilleras, cafetales con plátano y guamo y marquesinas de techo curvo en cada finca.",
                reto = Reto.Beneficio, mentorName = "Don Hernando Perdomo", mentorRole = "maestro de fermentaciones en Pitalito",
                giftVariety = "pinkbourbon", wikiUnlocks = new[] { "ben_fermentacion", "ben_anaerobico", "ben_natural", "ben_honey" },
                style = new FincaStyle { ground = C("#618436"), foliage = C("#2c6b2d"), sky = C("#a6c9e5"), fog = C("#d3dde1"), fogEnd = 260f, slope = 0.6f, temperatureC = 21f, river = true },
            },
            new Region
            {
                id = "cauca", name = "Cauca", department = "Cauca", town = "Inzá", lat = 2.550f, lon = -76.064f,
                altitudeMin = 1300, altitudeMax = 2100, chapter = 3, travelHours = 11,
                summary = "Café de pequeños productores campesinos e indígenas entre volcanes, la Ciudad Blanca de Popayán y las tumbas pintadas de Tierradentro.",
                cupProfile = "Acidez alta y brillante, notas florales, buen dulzor y cuerpo medio.",
                harvests = "Principal en el primer semestre y mitaca en el segundo.",
                landscape = "Meseta verde y ondulada con volcanes al fondo, casas de adobe y cafetales pequeños junto a la huerta familiar.",
                reto = Reto.Tostion, mentorName = "Doña Lucía Muñoz", mentorRole = "tostadora artesanal de Popayán",
                giftVariety = "sidra", wikiUnlocks = new[] { "tos_fases", "tos_primer_crack", "tos_desarrollo", "tos_niveles" },
                style = new FincaStyle { ground = C("#62853a"), foliage = C("#2e6a30"), sky = C("#a1c4e0"), fog = C("#d6dfe3"), fogEnd = 230f, slope = 0.45f, temperatureC = 19f, volcano = true, colonialTown = true },
            },
            new Region
            {
                id = "narino", name = "Nariño", department = "Nariño", town = "La Unión", lat = 1.603f, lon = -77.131f,
                altitudeMin = 1500, altitudeMax = 2200, chapter = 3, travelHours = 16,
                summary = "Tan cerca del ecuador que el café se siembra altísimo, casi a 2.200 m. Parcelas diminutas y el volcán Galeras.",
                cupProfile = "Acidez alta, cuerpo medio, muy dulce y muy aromático.",
                harvests = "Principal en el primer semestre y una mitaca pequeña en el segundo.",
                landscape = "Cañones profundos, parcelas casi verticales en cuadros de colores, el Galeras humeante y luz fuerte de altura.",
                reto = Reto.Catacion, mentorName = "Don Efraín Benavides", mentorRole = "caficultor de altura en La Unión",
                giftVariety = "geisha", wikiUnlocks = new[] { "cul_altitud_densidad", "cat_q_grader", "hist_dop_cafe_colombia" },
                style = new FincaStyle { ground = C("#6b8a3d"), foliage = C("#2f6a31"), sky = C("#8fbde8"), fog = C("#dfe7ec"), fogEnd = 300f, slope = 0.9f, temperatureC = 17f, volcano = true, jeep = false, mule = true },
            },
            new Region
            {
                id = "antioquia", name = "Antioquia", department = "Antioquia", town = "Jardín", lat = 5.598f, lon = -75.819f,
                altitudeMin = 1200, altitudeMax = 2000, chapter = 4, travelHours = 11,
                summary = "Cultura arriera de carriel, poncho y mulas. De aquí salió la colonización que llevó el café hacia el sur.",
                cupProfile = "Balanceado, de cuerpo medio, acidez media y dulzor a panela y chocolate.",
                harvests = "Principal de septiembre a diciembre; el resto en la traviesa o mitaca del primer semestre.",
                landscape = "Montañas muy empinadas, cafetales casi verticales y pueblos de plaza grande con balcones floridos como Jardín.",
                reto = Reto.Historia, mentorName = "Don Gildardo Restrepo", mentorRole = "arriero e historiador de Jardín",
                giftVariety = "caturra", wikiUnlocks = new[] { "hist_fnc_1927", "hist_juan_valdez_1958", "col_cordilleras" },
                style = new FincaStyle { ground = C("#5a8034"), foliage = C("#28622a"), sky = C("#a2c6e3"), fog = C("#d1dce1"), fogEnd = 240f, slope = 0.95f, temperatureC = 19f, mule = true, colonialTown = true },
            },
            new Region
            {
                id = "santander", name = "Santander", department = "Santander", town = "Barichara", lat = 6.636f, lon = -73.223f,
                altitudeMin = 1000, altitudeMax = 1800, chapter = 4, travelHours = 7,
                summary = "Café bajo sombra densa y diversa que protege a las aves, junto al gran Cañón del Chicamocha.",
                cupProfile = "Cuerpo medio-alto, acidez media y notas a chocolate y frutos secos.",
                harvests = "Una sola cosecha, de septiembre a diciembre.",
                landscape = "Un cañón seco y enorme frente a montañas húmedas, cafetales bajo un techo espeso de árboles y casas blancas de puertas verdes.",
                reto = Reto.Plagas, mentorName = "Doña Mercedes Ardila", mentorRole = "caficultora de café bajo sombra y amiga de las aves",
                giftVariety = "maragogype", wikiUnlocks = new[] { "col_sombrio", "cul_rere", "cul_clima_ideal" },
                style = new FincaStyle { ground = C("#7a7a3c"), foliage = C("#2a5f2c"), sky = C("#a8cbe6"), fog = C("#dad9cf"), fogEnd = 280f, slope = 0.5f, temperatureC = 22f, mule = true, colonialTown = true, jeep = false },
            },
            new Region
            {
                id = "nortesantander", name = "Norte de Santander", department = "Norte de Santander", town = "Salazar de las Palmas", lat = 7.776f, lon = -72.811f,
                altitudeMin = 1000, altitudeMax = 1900, chapter = 4, travelHours = 13,
                summary = "La cuna del café comercial colombiano: en 1835 salieron por Cúcuta los primeros sacos de exportación.",
                cupProfile = "Balanceado, de cuerpo medio y notas dulces a chocolate.",
                harvests = "En buena parte del departamento, una sola cosecha en el segundo semestre.",
                landscape = "Montañas templadas con pueblos coloniales empedrados, cafetales viejos bajo sombra y ríos que bajan al valle de Cúcuta.",
                reto = Reto.Historia, mentorName = "Profesor Álvaro Contreras", mentorRole = "historiador de Salazar de las Palmas",
                giftVariety = "typica", wikiUnlocks = new[] { "hist_padre_romero", "hist_primeras_exportaciones", "hist_jesuitas_colombia", "hist_gumilla" },
                style = new FincaStyle { ground = C("#678236"), foliage = C("#2b6430"), sky = C("#a5c8e3"), fog = C("#d4dde0"), fogEnd = 250f, slope = 0.5f, temperatureC = 21f, river = true, colonialTown = true, jeep = false },
            },
            new Region
            {
                id = "sierranevada", name = "Sierra Nevada de Santa Marta", department = "Magdalena · Cesar · La Guajira", town = "Minca", lat = 11.143f, lon = -74.118f,
                altitudeMin = 900, altitudeMax = 1800, chapter = 4, travelHours = 16,
                summary = "La montaña costera más alta del mundo, del Caribe a la nieve. Territorio de los pueblos arhuaco, kogui, wiwa y kankuamo.",
                cupProfile = "Cuerpo medio-alto, acidez media a baja y notas a chocolate y nuez. Mucho café orgánico bajo sombra.",
                harvests = "Una sola cosecha, hacia el final del año.",
                landscape = "Selva húmeda que sube desde el mar, cascadas, cafetales bajo árboles grandes, colibríes y picos nevados entre nubes.",
                reto = Reto.Recoleccion, mentorName = "Doña Marina Izquierdo", mentorRole = "caficultora de Minca que trabaja con cooperativas de la Sierra",
                giftVariety = "typica", wikiUnlocks = new[] { "col_indigenas_sierra", "col_cinturon_cafetero", "ben_recoleccion_selectiva" },
                style = new FincaStyle { ground = C("#4f7f35"), foliage = C("#245e29"), sky = C("#93c3ea"), fog = C("#d9e6e6"), fogEnd = 220f, slope = 0.75f, temperatureC = 23f, snowPeaks = true, river = true, mule = true, jeep = false },
            },
        };
    }
}
