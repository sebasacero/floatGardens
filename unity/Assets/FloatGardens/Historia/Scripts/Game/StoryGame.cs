// Corazón del modo historia: lugares (bodega, mapa de Colombia, regiones, competencias), paneles, tiempo,
// economía, misiones y guardado. Los constructores de escenarios y los paneles se conectan aquí por id.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens.Historia
{
    public enum Lugar { Titulo, Bodega, Mapa, Region, Competencia }

    /// <summary>Lo que devuelve cada constructor de escenario.</summary>
    public class SceneInfo
    {
        public Vector3 spawn;
        public float yaw;
        public bool walkable = true;          // false: no hay barista caminando (el mapa)
        public Vector3 cameraPos, cameraLook; // encuadre fijo cuando walkable = false
        public float altitude = HConfig.BogotaAltitude;
        public Transform root;
    }

    /// <summary>Estado de una competencia en curso (rondas de eliminación con 3 jueces, como en los campeonatos de AeroPress).</summary>
    public class CompetitionRun
    {
        public Competition comp;
        public int round;                     // 0 .. comp.rounds - 1
        public Rival rival;
        public float lastScore, rivalScore;
        public int votesFor;                  // votos de los jueces en la última ronda (0 – 3)
        public bool finished, won;
        public readonly List<string> log = new List<string>();
        public readonly List<Rival> bracket = new List<Rival>();
    }

    public class StoryGame : MonoBehaviour
    {
        public static StoryGame I;

        public HistoriaSave S;
        public Camera cam;
        public Atmosphere atmo;
        public StoryPlayer player;
        public HistoriaHUD hud;
        public DialogUI dialog;
        public TitleMenu menu;
        public ColombiaMap map;
        public GameObject playerModel;        // opcional (FGBootstrap-style)
        public Lugar lugar = Lugar.Titulo;
        public SceneInfo scene;
        public Transform world;
        public CompetitionRun run;
        public System.Random rng = new System.Random();

        RectTransform panelRoot;
        HPanel panel;
        float clockAccum;
        bool paused;

        public HPanel CurrentPanel => panel;
        public bool UIBusy => panel != null || (dialog != null && dialog.Active) || (menu != null && menu.Visible) || paused;
        public float Altitude => scene != null ? scene.altitude : HConfig.BogotaAltitude;
        public Region CurrentRegion => lugar == Lugar.Region ? Catalog.Region(S.region) : null;

        // ============================ ARRANQUE ============================

        public void Init()
        {
            I = this;
            FGTextures.Init();
            UIKit.EnsureEventSystem();
            gameObject.AddComponent<SFX>();

            cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 1200f;

            atmo = new GameObject("Atmosfera").AddComponent<Atmosphere>();
            atmo.transform.SetParent(transform, false);
            atmo.Setup(cam);

            world = new GameObject("Mundo").transform;

            var panelCanvas = UIKit.Canvas("Paneles", 25);
            panelCanvas.transform.SetParent(transform, false);
            panelRoot = (RectTransform)panelCanvas.transform;

            hud = new GameObject("HUD").AddComponent<HistoriaHUD>();
            hud.transform.SetParent(transform, false);
            hud.Build(this);
            dialog = new GameObject("Dialogo").AddComponent<DialogUI>();
            dialog.transform.SetParent(transform, false);
            dialog.Build(this);
            menu = new GameObject("Menu").AddComponent<TitleMenu>();
            menu.transform.SetParent(transform, false);
            menu.Build(this);

            S = HState.Exists ? HState.Load() : HState.NewGame(HConfig.DefaultName);
            if (SFX.I != null) SFX.I.muted = S.muted;
            GoTo(Lugar.Titulo);
        }

        /// <summary>Empieza una partida nueva con el nombre que escribió el jugador.</summary>
        public void NewGame(string playerName)
        {
            S = HState.NewGame(playerName);
            foreach (var v in VarietyData.All) if (string.IsNullOrEmpty(v.unlockRegion)) { S.AddSeeds(v.id, 2); break; }
            // un lote de café de Cundinamarca para las primeras AeroPress (cortesía de una tienda de La Candelaria)
            var first = S.NewLot(VarietyData.All.Count > 0 ? VarietyData.All[0].id : "caturra", "cundinamarca", "tostado", 500f, 82f);
            first.roast = "medio"; first.roastLevel = 0.45f; first.roastScore = 80f; first.process = "lavado";
            first.notes = "panela, chocolate, cítricos"; first.day = 0;
            Save();
            StartChapter(0);
            GoTo(Lugar.Bodega);
        }

        public void Continue()
        {
            if (S.location == "mapa") GoTo(Lugar.Mapa);
            else if (S.location == "region" && Catalog.Region(S.region) != null) GoTo(Lugar.Region, S.region);
            else GoTo(Lugar.Bodega);
            var m = Catalog.Mission(S.mission);
            if (m != null) hud.Objective(m.title, m.desc);
        }

        // ============================ LUGARES ============================

        public void GoTo(Lugar l, string id = null)
        {
            ClosePanel();
            for (int i = world.childCount - 1; i >= 0; i--) Destroy(world.GetChild(i).gameObject);
            if (player != null) { Destroy(player.gameObject); player = null; }
            lugar = l;
            run = l == Lugar.Competencia ? run : null;
            var root = new GameObject(l.ToString()).transform;
            root.SetParent(world, false);

            switch (l)
            {
                case Lugar.Titulo:
                    scene = BodegaBuilder.Build(root, S);
                    scene.walkable = false;
                    scene.cameraPos = scene.spawn + new Vector3(4.5f, 2.6f, -5.5f);
                    scene.cameraLook = scene.spawn + new Vector3(0f, 1.2f, 2f);
                    menu.ShowTitle();
                    break;
                case Lugar.Bodega:
                    S.location = "bodega";
                    scene = BodegaBuilder.Build(root, S);
                    break;
                case Lugar.Mapa:
                    S.location = "mapa";
                    map = ColombiaMap.Build(root, S);
                    scene = map.info;
                    OpenPanel<MapPanel>();
                    break;
                case Lugar.Region:
                    var r = Catalog.Region(id);
                    if (r == null) { GoTo(Lugar.Mapa); return; }
                    S.location = "region";
                    S.region = r.id;
                    scene = r.id == "bogota" ? BodegaBuilder.BuildCity(root, S) : FincaBuilder.Build(root, r, S);
                    break;
                case Lugar.Competencia:
                    var c = run != null ? run.comp : null;
                    scene = StageBuilder.Build(root, c, S);
                    break;
            }
            scene.root = root;

            foreach (var it in root.GetComponentsInChildren<Interactable>(true)) Wire(it);

            if (scene.walkable)
            {
                player = StoryPlayer.Create(this, playerModel);
                player.Teleport(scene.spawn, scene.yaw);
            }
            else if (cam != null)
            {
                cam.transform.position = scene.cameraPos;
                cam.transform.LookAt(scene.cameraLook);
            }
            hud.Show(l != Lugar.Titulo);
            hud.OnPlaceChanged();
            if (l != Lugar.Titulo) Save();

            if (l == Lugar.Region) ArriveRegion(Catalog.Region(id));
            if (l == Lugar.Competencia && run != null) Say(run.comp.introDialog, () => OpenPanel<CompetitionPanel>());
        }

        /// <summary>Conecta las estaciones que crean los constructores con su acción, según el id.</summary>
        void Wire(Interactable it)
        {
            switch (it.id)
            {
                case "cultivo": it.onUse = () => OpenPanel<GardenPanel>(); break;
                case "beneficio": it.onUse = () => OpenPanel<ProcessPanel>(); break;
                case "tostadora": it.onUse = () => OpenPanel<RoastPanel>(); break;
                case "aeropress": it.onUse = () => OpenPanel<BrewPanel>(); break;
                case "pc": it.onUse = () => OpenPanel<PCPanel>(); break;
                case "cama": it.onUse = AskSleep; break;
                case "puerta":
                case "volver": it.onUse = () => GoTo(Lugar.Mapa); break;
                case "bodega": it.onUse = () => GoTo(Lugar.Bodega); break;
                case "mentor": it.onUse = TalkToMentor; break;
                case "jueces": it.onUse = () => { if (run != null) OpenPanel<CompetitionPanel>(); }; break;
                default:
                    // id libre: si existe un diálogo con ese id, se reproduce (personajes de ambiente, placas, murales...)
                    if (it.onUse == null && Catalog.Dialog(it.id) != null) { string d = it.id; it.onUse = () => { Say(d); Emit(HEvents.Talk, d); }; }
                    break;
            }
        }

        // ============================ PANELES ============================

        public T OpenPanel<T>(Action<T> setup = null) where T : HPanel
        {
            ClosePanel();
            var go = new GameObject(typeof(T).Name);
            go.transform.SetParent(transform, false);
            var p = go.AddComponent<T>();
            setup?.Invoke(p);
            panel = p;
            FGInput.CursorLocked = false;
            p.Open(panelRoot);
            AdvanceHours(p.HoursCost);
            return p;
        }

        public void ClosePanel(HPanel p = null)
        {
            if (panel == null || (p != null && p != panel)) return;
            var old = panel;
            panel = null;
            old.OnClosed();
            if (old.root != null) Destroy(old.root.gameObject);
            Destroy(old.gameObject);
            if (lugar == Lugar.Mapa && !(old is MapPanel) && map != null) OpenPanel<MapPanel>();
            hud.Refresh();
        }

        // ============================ BUCLE ============================

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            if (FGInput.Pressed(FGAction.Pause) && lugar != Lugar.Titulo)
            {
                if (dialog.Active) { }
                else if (panel != null && !(panel is MapPanel)) ClosePanel();
                else if (menu.Visible) menu.Hide();
                else menu.ShowPause();
            }
            paused = menu.Visible;

            if (!UIBusy && scene != null && scene.walkable && player != null)
            {
                player.Tick(dt);
                clockAccum += dt;
                if (clockAccum >= 1f) { AdvanceHours(clockAccum / HConfig.RealSecondsPerGameHour); clockAccum = 0f; }
            }
            else if (player != null) player.Idle(dt);

            if (panel != null) panel.Tick(dt);
            if (lugar == Lugar.Mapa && map != null) map.Tick(dt);
            if (lugar == Lugar.Titulo && cam != null)
            {
                float t = Time.time * 0.08f;
                cam.transform.position = scene.cameraPos + new Vector3(Mathf.Sin(t) * 1.5f, 0f, Mathf.Cos(t) * 0.8f);
                cam.transform.LookAt(scene.cameraLook);
            }
            hud.Tick(dt);
            FGInput.EndFrame();
        }

        // ============================ DIÁLOGOS Y AVISOS ============================

        public void Say(string dialogId, Action onDone = null)
        {
            var lines = Catalog.Dialog(dialogId);
            if (lines == null || lines.Length == 0) { onDone?.Invoke(); return; }
            Say(lines, onDone);
        }

        public void Say(DialogLine[] lines, Action onDone = null)
        {
            FGInput.CursorLocked = false;
            dialog.Play(lines, onDone);
        }

        /// <summary>Reemplaza {jugador} y {canal} en los textos.</summary>
        public string Fill(string text)
            => string.IsNullOrEmpty(text) ? "" : text.Replace("{jugador}", S.playerName).Replace("{canal}", HConfig.Channel);

        public void Toast(string msg) { if (hud != null) hud.Toast(Fill(msg)); }

        // ============================ ECONOMÍA Y PROGRESO ============================

        public void AddMoney(int n, string reason = null)
        {
            S.money += n;
            if (n != 0) { SFX.Play("cash", 0.6f); Toast((n > 0 ? "+" : "") + HConfig.Money(n) + (reason != null ? " · " + reason : "")); }
        }

        public bool Spend(int n, string reason)
        {
            if (S.money < n) { Toast("No te alcanza: necesitas " + HConfig.Money(n)); SFX.Play("empty"); return false; }
            S.money -= n;
            SFX.Play("cash", 0.5f);
            if (reason != null) Toast("-" + HConfig.Money(n) + " · " + reason);
            return true;
        }

        public void AddCups(int n)
        {
            if (n <= 0) return;
            S.cups += n;
            Emit(HEvents.Cups, "", S.cups);
        }

        public void AddFollowers(int n)
        {
            if (n == 0) return;
            S.followers = Mathf.Max(0, S.followers + n);
            Emit(HEvents.Followers, "", S.followers);
        }

        public void UnlockWiki(string id)
        {
            var w = Catalog.Wiki(id);
            if (w == null || S.wiki.Contains(id)) return;
            S.wiki.Add(id);
            Toast("Nueva entrada en la Wiki del Café: " + w.title);
            Emit(HEvents.Wiki, id);
        }

        public void GiveSeeds(string variety, int n)
        {
            var v = Catalog.Variety(variety);
            if (v == null || n <= 0) return;
            S.AddSeeds(variety, n);
            Toast("+" + n + " semillas de " + v.name);
        }

        public Lot GiveLot(string variety, string origin, string stage, float grams, float potential, string notes = "", string process = "")
        {
            var l = S.NewLot(variety, origin, stage, grams, potential);
            l.notes = notes;
            l.process = process;
            if (stage == "tostado") { l.roast = "medio"; l.roastLevel = 0.45f; l.roastScore = 82f; }
            Toast("Recibiste " + HConfig.Grams(grams) + " de café: " + l.Label);
            return l;
        }

        public void AwardPuntos(int n, string reason)
        {
            HState.AwardPuntos(S, n);
            Toast("+" + n + " Puntos Float · " + reason);
        }

        // ============================ TIEMPO ============================

        public void AdvanceHours(float h)
        {
            if (h <= 0f) return;
            S.hour += h;
            while (S.hour >= 24f) { S.hour -= 24f; NextDay(); }
            hud.Refresh();
        }

        void AskSleep()
        {
            Say(new[] { new DialogLine("{jugador}", S.hour >= HConfig.SleepHour || S.hour < 5f
                ? "Ya es tarde. A dormir: mañana las plantas habrán crecido."
                : "Una siesta y sigo mañana. Las plantas crecen mientras duermo.") }, Sleep);
        }

        public void Sleep()
        {
            float h = 24f - S.hour + HConfig.StartHour;
            if (S.hour < 5f) h = HConfig.StartHour - S.hour;
            AdvanceHours(h);
            Emit(HEvents.Sleep);
            Save();
            Toast("Día " + S.day + " · " + HConfig.Clock(S.hour));
        }

        /// <summary>Procesa un día: crecen las plantas, se paga la energía, se secan los lotes, llega el correo.</summary>
        public void NextDay()
        {
            S.day++;
            int active = 0;
            var notes = new List<string>();
            foreach (var p in S.garden.rafts)
            {
                if (p.Empty) continue;
                active++;
                string n = CoffeeModel.GrowDay(p, S.garden, rng, S.gear.Contains("luces_pro") ? 1.15f : 1f);
                if (n != null && !notes.Contains(n)) notes.Add(n);
            }
            if (active > 0)
            {
                int cost = CoffeeModel.DailyEnergyCost(S.garden, active);
                S.money -= cost;
                notes.Add("Energía de la bodega: -" + HConfig.Money(cost));
            }
            foreach (var l in S.lots)
            {
                if (l.stage == "secando" && S.day >= l.readyDay)
                {
                    l.stage = "verde";
                    notes.Add("El lote " + l.id + " ya está seco y trillado: café verde listo para tostar");
                }
            }
            foreach (var n in notes) Toast(n);
        }

        // ============================ MISIONES Y CAPÍTULOS ============================

        public void StartChapter(int n)
        {
            var c = Catalog.Chapter(n);
            if (c == null) return;
            S.chapter = n;
            Say(c.introDialog, () =>
            {
                hud.ChapterCard(c);
                StartMission(c.firstMission);
            });
        }

        public void StartMission(string id)
        {
            var m = Catalog.Mission(id);
            if (m == null) { S.mission = ""; hud.Objective(null, null); return; }
            S.mission = id;
            S.missionProgress = 0f;
            Say(m.startDialog, () => hud.Objective(m.title, m.desc));
            hud.Objective(m.title, m.desc);
            Save();
            // si ya se cumple (p. ej. ya tienes los seguidores pedidos), se completa sola
            if (m.evt == HEvents.Cups) Emit(HEvents.Cups, "", S.cups);
            else if (m.evt == HEvents.Followers) Emit(HEvents.Followers, "", S.followers);
            else if (m.evt == HEvents.Visit && (S.visited.Contains(m.arg) || (lugar == Lugar.Region && S.region == m.arg))) Emit(HEvents.Visit, m.arg);
            else if (m.evt == HEvents.Reto && S.stamps.Contains(m.arg)) Emit(HEvents.Reto, m.arg, 100f);
            else if (m.evt == HEvents.Competition && S.trophies.Contains(m.arg)) Emit(HEvents.Competition, m.arg, 1f);
        }

        /// <summary>Todos los sistemas avisan aquí lo que pasa; la misión activa decide si avanza.</summary>
        public void Emit(string evt, string arg = "", float value = 1f)
        {
            var m = Catalog.Mission(S.mission);
            if (m == null || m.evt != evt) return;
            if (!string.IsNullOrEmpty(m.arg) && m.arg != arg) return;
            if (m.threshold || evt == HEvents.Cups || evt == HEvents.Followers) S.missionProgress = Mathf.Max(S.missionProgress, value);
            else S.missionProgress += 1f;
            hud.Refresh();
            if (S.missionProgress >= m.target) CompleteMission(m);
        }

        void CompleteMission(Mission m)
        {
            if (S.missionsDone.Contains(m.id)) return;
            S.missionsDone.Add(m.id);
            S.mission = "";
            SFX.Play("capture");
            Toast("¡Misión cumplida! " + m.title);
            if (m.rewardMoney > 0) AddMoney(m.rewardMoney, m.title);
            foreach (var w in m.rewardWiki) UnlockWiki(w);
            if (!string.IsNullOrEmpty(m.rewardSeed)) GiveSeeds(m.rewardSeed, Mathf.Max(1, m.rewardSeedCount));
            foreach (var f in m.rewardFlags) S.Flag(f);
            Save();
            Say(m.endDialog, () =>
            {
                if (!string.IsNullOrEmpty(m.next)) StartMission(m.next);
                else
                {
                    AwardPuntos(HConfig.PuntosPerChapter, "capítulo completado");
                    if (Catalog.Chapter(S.chapter + 1) != null) StartChapter(S.chapter + 1);
                    else { hud.Objective(null, null); menu.ShowEnding(); }
                }
            });
        }

        public string MissionProgressText()
        {
            var m = Catalog.Mission(S.mission);
            if (m == null) return "";
            if (m.threshold) return "Mejor: " + Mathf.RoundToInt(S.missionProgress) + " / " + Mathf.RoundToInt(m.target);
            if (m.target > 1f) return Mathf.FloorToInt(S.missionProgress) + " / " + Mathf.RoundToInt(m.target);
            return "";
        }

        // ============================ VIAJES Y REGIONES ============================

        public bool RegionUnlocked(Region r) => r != null && r.chapter <= S.chapter;

        /// <summary>Viaja en bus desde el mapa: cuesta pasaje y horas de camino.</summary>
        public void Travel(string regionId)
        {
            var r = Catalog.Region(regionId);
            if (r == null) return;
            if (!RegionUnlocked(r)) { Toast("Se abre en el capítulo " + r.chapter); return; }
            if (r.id == "bogota") { GoTo(Lugar.Region, r.id); return; }
            int fare = r.travelHours * HConfig.BusFarePerHour;
            if (!Spend(fare, "pasaje a " + r.name)) return;
            ClosePanel();
            map.AnimateTravel(r, () =>
            {
                AdvanceHours(r.travelHours);
                GoTo(Lugar.Region, r.id);
            });
        }

        void ArriveRegion(Region r)
        {
            if (r == null) return;
            bool first = !S.visited.Contains(r.id);
            if (first) S.visited.Add(r.id);
            hud.PlaceCard(r.name, r.department + " · " + Mathf.RoundToInt(r.altitudeMin) + "–" + Mathf.RoundToInt(r.altitudeMax) + " m s. n. m.");
            if (first) foreach (var w in r.wikiUnlocks) UnlockWiki(w);
            Emit(HEvents.Visit, r.id);
        }

        void TalkToMentor()
        {
            var r = CurrentRegion;
            if (r == null) return;
            Emit(HEvents.Talk, r.id);
            bool done = S.stamps.Contains(r.id);
            string key = done ? r.id + "_despues" : r.id + "_mentor";
            Say(key, () => { if (!done && r.reto != Reto.Ninguno) OpenReto(r); });
        }

        public void OpenReto(Region r)
        {
            switch (r.reto)
            {
                case Reto.Recoleccion: OpenPanel<PickingPanel>(p => p.regionId = r.id); break;
                case Reto.Plagas: OpenPanel<QuizPanel>(p => { p.regionId = r.id; p.topic = "plagas"; }); break;
                case Reto.Historia: OpenPanel<QuizPanel>(p => { p.regionId = r.id; p.topic = "historia"; }); break;
                case Reto.Catacion: OpenPanel<CuppingPanel>(p => p.regionId = r.id); break;
                case Reto.Beneficio: OpenPanel<ProcessPanel>(p => p.retoRegion = r.id); break;
                case Reto.Tostion: OpenPanel<RoastPanel>(p => p.retoRegion = r.id); break;
                case Reto.Aeropress: OpenPanel<BrewPanel>(p => p.retoRegion = r.id); break;
            }
        }

        /// <summary>Lo llaman los paneles de reto al terminar. score: 0 – 100; se supera con 60 o más.</summary>
        public void RetoDone(string regionId, float score)
        {
            var r = Catalog.Region(regionId);
            if (r == null) return;
            Emit(HEvents.Reto, regionId, score);
            if (score < 60f)
            {
                Say(new[] { new DialogLine(r.mentorName, "Casi. Vuelve a intentarlo cuando quieras: el café enseña con paciencia.") });
                return;
            }
            bool first = !S.stamps.Contains(regionId);
            if (first)
            {
                S.stamps.Add(regionId);
                SFX.Play("stamp");
                Toast("Sello de " + r.name + " en tu Pasaporte Cafetero");
                if (!string.IsNullOrEmpty(r.giftVariety)) GiveSeeds(r.giftVariety, 3);
                var v = Catalog.Variety(r.giftVariety);
                GiveLot(v != null ? v.id : r.giftVariety, r.id, "verde", 1000f, Mathf.Clamp((v != null ? v.cupPotential : 84f) - 1f, 80f, 92f),
                    v != null ? v.notes : r.cupProfile, "lavado");
            }
            Save();
            Say(r.id + "_reto_ok");
        }

        // ============================ COMPETENCIAS ============================

        public void StartCompetition(string id)
        {
            var c = Catalog.Competition(id);
            if (c == null) return;
            run = new CompetitionRun { comp = c };
            var pool = new List<Rival>(StoryData.Rivals);
            for (int i = 0; i < c.rounds && pool.Count > 0; i++)
            {
                int k = rng.Next(pool.Count);
                var rv = pool[k];
                pool.RemoveAt(k);
                run.bracket.Add(new Rival { name = rv.name, city = rv.city, style = rv.style, skill = Mathf.Clamp01(c.rivalSkill + (rv.skill - 0.5f) * 0.3f + i * 0.05f) });
            }
            run.rival = run.bracket.Count > 0 ? run.bracket[0] : new Rival { name = "Rival", city = "Bogotá", skill = c.rivalSkill };
            GoTo(Lugar.Competencia);
        }

        /// <summary>Lo llama el panel de preparación en modo competencia: 3 jueces señalan la mejor taza.</summary>
        public void CompetitionBrew(float score)
        {
            if (run == null || run.finished) return;
            run.lastScore = score;
            float rivalMean = Mathf.Lerp(74f, 91f, run.rival.skill);
            run.rivalScore = rivalMean + (float)(rng.NextDouble() * 2 - 1) * 2.5f;
            run.votesFor = 0;
            for (int j = 0; j < 3; j++)
            {
                float noise = (float)(rng.NextDouble() * 2 - 1) * 2f; // cada juez tiene su paladar
                if (score + noise > run.rivalScore) run.votesFor++;
            }
            bool win = run.votesFor >= 2;
            run.log.Add("Ronda " + (run.round + 1) + " vs " + run.rival.name + ": " + run.votesFor + "–" + (3 - run.votesFor) + (win ? " · ganaste" : " · perdiste"));
            if (!win) { run.finished = true; run.won = false; }
            else if (run.round + 1 >= run.comp.rounds) { run.finished = true; run.won = true; }
            else { run.round++; run.rival = run.bracket[Mathf.Min(run.round, run.bracket.Count - 1)]; }

            if (run.finished)
            {
                Emit(HEvents.Competition, run.comp.id, run.won ? 1f : 0f);
                if (run.won && !S.trophies.Contains(run.comp.id))
                {
                    S.trophies.Add(run.comp.id);
                    AddMoney(run.comp.prize, run.comp.name);
                    AddFollowers(500 + run.comp.chapter * 1500);
                    foreach (var w in run.comp.wikiUnlocks) UnlockWiki(w);
                    AwardPuntos(HConfig.PuntosPerTrophy, run.comp.trophy);
                }
                Save();
            }
            OpenPanel<CompetitionPanel>();
        }

        public void EndCompetition()
        {
            var c = run != null ? run.comp : null;
            bool won = run != null && run.won;
            ClosePanel();
            GoTo(Lugar.Bodega);
            if (c != null) Say(won ? c.winDialog : c.loseDialog);
        }

        // ============================ GUARDADO ============================

        public void Save()
        {
            if (S == null || lugar == Lugar.Titulo) return;
            if (SFX.I != null) S.muted = SFX.I.muted;
            HState.Write(S);
        }

        void OnApplicationPause(bool p) { if (p) Save(); }
        void OnApplicationQuit() { Save(); }
    }
}
