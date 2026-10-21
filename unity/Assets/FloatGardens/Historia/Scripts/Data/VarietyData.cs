// Variedades de café que se pueden cultivar en la huerta flotante. Datos de referencia: Cenicafé y la historia de cada variedad.
// La primera de la lista es la semilla con la que empieza la historia.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    public static class VarietyData
    {
        static Color C(string hex) => FGMaterials.Hex(hex);

        public static readonly List<Variety> All = new List<Variety>
        {
            new Variety
            {
                id = "caturra", name = "Caturra", origin = "Minas Gerais, Brasil (1915–1918)",
                description = "Mutación natural enana del Borbón. Sus entrenudos cortos permiten sembrar muchas más matas por hectárea y desde los años setenta llenó los cafetales tecnificados de Colombia. Buena taza y buena producción, pero susceptible a la roya.",
                yield = 1.15f, rustResistance = 0.1f, cupPotential = 84f, growthDays = 9f, cherryColor = C("#c0262d"),
                notes = "panela, cítricos, caramelo", price = HConfig.SeedPrice, unlockRegion = "",
            },
            new Variety
            {
                id = "typica", name = "Típica", origin = "Etiopía → Yemen → Java → América",
                description = "La variedad que predominó en Colombia durante buena parte de su historia cafetera: la de los cafetos que llegaron con los misioneros. Porte alto, hojas nuevas color bronce, poca producción y una taza limpia y elegante. Susceptible a la roya.",
                yield = 0.7f, rustResistance = 0.05f, cupPotential = 86f, growthDays = 12f, cherryColor = C("#b3202a"),
                notes = "limpio, floral, dulce", price = HConfig.SeedPrice * 2, unlockRegion = "nortesantander",
            },
            new Variety
            {
                id = "bourbon", name = "Borbón", origin = "Isla Bourbon (hoy La Reunión), siglo XVIII",
                description = "Los franceses llevaron café de Yemen a la isla Bourbon a comienzos del siglo XVIII. Produce más que la Típica, sus hojas nuevas son verdes y es muy apreciado por su dulzura. Hay de fruto rojo y de fruto amarillo. Susceptible a la roya.",
                yield = 0.9f, rustResistance = 0.05f, cupPotential = 86f, growthDays = 11f, cherryColor = C("#c8242c"),
                notes = "dulce, caramelo, frutos rojos", price = HConfig.SeedPrice * 2, unlockRegion = "cundinamarca",
            },
            new Variety
            {
                id = "colombia", name = "Colombia", origin = "Cenicafé, Chinchiná (1982)",
                description = "Cenicafé cruzó Caturra con Híbrido de Timor y en 1982 entregó la primera variedad resistente a la roya del país, ¡un año antes de que la enfermedad llegara! Es compuesta: mezcla varias líneas para que la resistencia dure más.",
                yield = 1.15f, rustResistance = 0.85f, cupPotential = 83f, growthDays = 9f, cherryColor = C("#c22a2a"),
                notes = "chocolate, panela, nuez", price = HConfig.SeedPrice, unlockRegion = "bogota",
            },
            new Variety
            {
                id = "castillo", name = "Castillo", origin = "Cenicafé, Chinchiná (2005)",
                description = "Derivada de Caturra por Híbrido de Timor, resistente a la roya, productiva y de buen tamaño de grano. Fue la gran protagonista de la renovación de los cafetales después de la epidemia de roya de 2008–2011. Tiene componentes regionales adaptados a cada zona.",
                yield = 1.25f, rustResistance = 0.9f, cupPotential = 84f, growthDays = 9f, cherryColor = C("#c42b2b"),
                notes = "caramelo, cítricos, chocolate", price = HConfig.SeedPrice, unlockRegion = "caldas",
            },
            new Variety
            {
                id = "cenicafe1", name = "Cenicafé 1", origin = "Cenicafé, Chinchiná (2016)",
                description = "De porte bajo como el Caturra, muy productiva y resistente tanto a la roya como a la antracnosis de los frutos (CBD), una enfermedad que existe en África pero no en Colombia: una variedad pensada para el futuro.",
                yield = 1.3f, rustResistance = 0.92f, cupPotential = 84f, growthDays = 9f, cherryColor = C("#bf2a2e"),
                notes = "dulce, panela, frutal suave", price = HConfig.SeedPrice * 2, unlockRegion = "risaralda",
            },
            new Variety
            {
                id = "tabi", name = "Tabi", origin = "Cenicafé (2002)",
                description = "Variedad de porte alto derivada de Híbrido de Timor con Típica y Borbón, pensada para fincas tradicionales con sombrío. Resistente a la roya, con buen tamaño de grano y buena calidad en taza.",
                yield = 0.95f, rustResistance = 0.85f, cupPotential = 85f, growthDays = 11f, cherryColor = C("#b8262c"),
                notes = "floral, caramelo, durazno", price = HConfig.SeedPrice * 2, unlockRegion = "quindio",
            },
            new Variety
            {
                id = "pinkbourbon", name = "Pink Bourbon", origin = "Fincas del Huila (análisis genéticos: origen etíope)",
                description = "Cerezas de un rosado anaranjado que se hicieron famosas en fincas del Huila. Por el nombre se creía un cruce de Borbón rojo y amarillo, pero análisis genéticos de 2017 y 2023 indican un origen etíope. Muy buscado por su taza floral y frutal.",
                yield = 0.8f, rustResistance = 0.25f, cupPotential = 89f, growthDays = 11f, cherryColor = C("#f08a6b"),
                notes = "flores blancas, mandarina, té negro", price = HConfig.SeedPrice * 3, unlockRegion = "huila",
            },
            new Variety
            {
                id = "geisha", name = "Geisha (Gesha)", origin = "Bosque de Gori Gesha, Etiopía (años treinta)",
                description = "Recolectada en Etiopía en los años treinta, pasó por Tanzania y Costa Rica (1953) hasta Panamá, donde en 2004 deslumbró en una subasta por sus notas a jazmín y bergamota. Porte alto, ramas frágiles, muy poca producción y una de las tazas más cotizadas del mundo.",
                yield = 0.6f, rustResistance = 0.2f, cupPotential = 92f, growthDays = 12f, cherryColor = C("#c3222c"),
                notes = "jazmín, bergamota, durazno", price = HConfig.SeedPrice * 4, unlockRegion = "narino",
            },
            new Variety
            {
                id = "sidra", name = "Sidra", origin = "Popular en Ecuador (origen discutido)",
                description = "Muy popular en Ecuador y en las competencias de baristas. Su origen se discute: unos la consideran un cruce de Típica y Borbón y otros una selección etíope. Su taza suele ser compleja, dulce y frutal.",
                yield = 0.85f, rustResistance = 0.2f, cupPotential = 88f, growthDays = 11f, cherryColor = C("#c9282e"),
                notes = "frutas tropicales, panela, floral", price = HConfig.SeedPrice * 3, unlockRegion = "cauca",
            },
            new Variety
            {
                id = "maragogype", name = "Maragogipe", origin = "Maragogipe, Bahía, Brasil (~1870)",
                description = "Mutación natural de la Típica de hojas, cerezas y granos enormes: le dicen 'grano elefante'. Produce muy poco y por su tamaño exige ajustar la tostión y la molienda.",
                yield = 0.6f, rustResistance = 0.05f, cupPotential = 85f, growthDays = 12f, cherryColor = C("#b22128"),
                notes = "suave, cuerpo sedoso, cacao", price = HConfig.SeedPrice * 2, unlockRegion = "santander",
            },
            new Variety
            {
                id = "wushwush", name = "Wush Wush", origin = "Wushwush, Kafa, Etiopía",
                description = "Variedad tradicional del pueblo de Wushwush, cerca del bosque de Bonga en Kafa (Etiopía). Llegó a Sudamérica en los años noventa y hoy se cultiva en fincas colombianas de café especial.",
                yield = 0.75f, rustResistance = 0.2f, cupPotential = 88f, growthDays = 11f, cherryColor = C("#a81f34"),
                notes = "frutos rojos, vino, cacao", price = HConfig.SeedPrice * 3, unlockRegion = "tolima",
            },
        };
    }
}
