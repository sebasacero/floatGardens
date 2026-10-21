// Equipos de la tienda del PC. Cada uno cambia algo en el juego y explica por qué importa en el café de verdad.
using System.Collections.Generic;

namespace FloatGardens.Historia
{
    public static class GearData
    {
        public static readonly List<Gear> All = new List<Gear>
        {
            new Gear { id = "bascula", name = "Báscula de 0,1 g", price = 60000, chapter = 0,
                desc = "Sin báscula cada dosis varía un gramo para arriba o para abajo. En café, un gramo cambia la proporción y la fuerza de la taza: los baristas pesan todo." },
            new Gear { id = "hervidor_cuello", name = "Hervidor cuello de cisne con temperatura", price = 180000, chapter = 0,
                desc = "Sin él, el agua puede quedar 3 °C arriba o abajo de lo que querías. Controla la temperatura exacta y vierte con precisión (recuerda: en Bogotá el agua no pasa de ~91 °C)." },
            new Gear { id = "camara", name = "Cámara para acero-press", price = 250000, chapter = 0,
                desc = "Fotos y videos que dan ganas de tomarse la taza: +30 % de seguidores en cada publicación." },
            new Gear { id = "molino_manual", name = "Molino manual de muelas cónicas", price = 320000, chapter = 1,
                desc = "Las muelas cortan el grano parejo y con pocos finos, así la extracción es uniforme: +1,5 puntos en cada taza." },
            new Gear { id = "filtro_metal", name = "Filtro metálico para AeroPress", price = 70000, chapter = 1,
                desc = "Deja pasar aceites y partículas finas: más cuerpo y textura, menos claridad. Activa la opción de filtro metálico." },
            new Gear { id = "luces_pro", name = "Luces LED de espectro completo", price = 450000, chapter = 1,
                desc = "Más luz útil para la fotosíntesis de las hojas: las plantas de la huerta flotante crecen 15 % más rápido." },
            new Gear { id = "refractometro", name = "Refractómetro de café", price = 600000, chapter = 2,
                desc = "Mide los sólidos disueltos (TDS) de la taza; con la dosis y el peso de la bebida calculas la extracción (EY). Sin él solo tienes tu paladar." },
            new Gear { id = "marquesina", name = "Secador tipo marquesina", price = 280000, chapter = 2,
                desc = "Como las marquesinas de techo plástico curvo de las fincas: protege el pergamino de la lluvia y seca 30 % más rápido." },
            new Gear { id = "tanque_anaerobico", name = "Tanque de fermentación anaeróbica", price = 350000, chapter = 3,
                desc = "Tanque sellado con válvula para fermentar sin oxígeno. Abre el proceso anaeróbico: más riesgo, sabores más intensos." },
            new Gear { id = "tostadora_pro", name = "Sondas y software de tostión", price = 500000, chapter = 3,
                desc = "Dos sondas (grano y aire) conectadas al PC: ves la temperatura del aire (ET) y la tasa de aumento (RoR) en vivo, como en una tostadora profesional." },
        };
    }
}
