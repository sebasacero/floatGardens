// Orquesta el juego: estados, zonas, enemigos, economía, interacción, guardado y jefe final.
using System.Collections.Generic;
using UnityEngine;

namespace FloatGardens
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager I;
        public enum GameState { Lobby, Playing, Paused, Shop, Dead, Dialogue, Lab }

        public struct Stats { public float damage, hp, speed, grow; public int mag, yield; }
        public struct Objective_ { public string title, sub; public Vector3 target; public bool hasTarget; }

        // Modelos opcionales (asignados desde FGBootstrap)
        public GameObject playerModel, enemyModel, bossModel, pedestrianModel;

        public GameState State { get; private set; }
        public SaveData save;
        public Stats stats;
        public int cherries;
        public int lastLost;
        public string prompt;

        public PlayerController player;
        public Camera cam;
        public ThirdPersonCamera camRig;
        public HUD hud;
        public Menus menus;
        public TouchControls touch;
        public Atmosphere atmosphere;
        public City city;
        public Story story;
        public Dialogue dialogue;

        public readonly List<CaptureZone> zones = new List<CaptureZone>();
        public readonly List<Enemy> enemies = new List<Enemy>();
        public readonly List<Pedestrian> peds = new List<Pedestrian>();
        public readonly List<Loot> loot = new List<Loot>();
        public readonly List<Planter> planters = new List<Planter>();
        readonly List<Stamp> stamps = new List<Stamp>();
        readonly List<float> crateRespawns = new List<float>();
        public CaptureZone targetZone, capturing;
        public bool capturingContested;
        public Boss boss;

        Transform actors;
        float spawnTimer = 2, zoneColorTimer, autosaveTimer = 10;
        char currentDistrict = ' ';
        bool showMap, cursorWasLocked;
        HashSet<char> owned = new HashSet<char>();

        // ---------- Arranque ----------
        public void Init()
        {
            I = this;
            save = FGSave.Load();
            cherries = save.cherries;
            Application.targetFrameRate = 60;

            var cityGo = new GameObject("Bogota");
            city = cityGo.AddComponent<City>();
            city.Build();
            Physics.SyncTransforms();

            actors = new GameObject("Personajes").transform;
            FX.Init(transform);
            gameObject.AddComponent<SFX>().muted = save.muted;

            cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 900;
            camRig = cam.gameObject.AddComponent<ThirdPersonCamera>();
            camRig.cam = cam;

            atmosphere = new GameObject("Atmosfera").AddComponent<Atmosphere>();
            atmosphere.Setup(cam);

            player = PlayerController.Create(playerModel);
            camRig.target = player;
            RefreshStats();
            player.hp = player.maxHp;
            player.ammo = player.magSize;
            player.Teleport(city.hqSpawn, true);

            foreach (var def in FGConfig.Zones)
            {
                var z = CaptureZone.Spawn(def, city.zoneCenters[def.id], transform);
                z.ours = save.captured.Contains(def.id);
                if (z.ours) z.progress = 1;
                zones.Add(z);
            }
            foreach (var spot in city.planterSpots["hq"]) planters.Add(Planter.Spawn(spot, true, transform));
            foreach (var z in zones) if (z.ours) AddZonePlanters(z);
            for (int i = 0; i < 45; i++) peds.Add(Pedestrian.Spawn(city.SidewalkPoint(c => c != 'P' && c != 'F'), actors));
            for (int i = 0; i < 18; i++) SpawnCrate();
            UpdateZoneColors();

            UIKit.EnsureEventSystem();
            hud = new GameObject("HUD").AddComponent<HUD>();
            hud.Build();
            dialogue = new GameObject("Dialogo").AddComponent<Dialogue>();
            dialogue.Build();
            story = new Story(this);
            story.Spawn(actors);
            touch = new GameObject("Tactil").AddComponent<TouchControls>();
            touch.Build();
            menus = new GameObject("Menus").AddComponent<Menus>();
            menus.Build(this);
            ToLobby();
        }

        public void RefreshStats()
        {
            stats = new Stats
            {
                damage = FGConfig.Damage * (1 + save.Level("dano") * 0.2f),
                mag = FGConfig.MagSize + save.Level("cargador") * 4,
                hp = FGConfig.MaxHp + save.Level("vida") * 25,
                speed = 1 + save.Level("tenis") * 0.08f,
                yield = FGConfig.HarvestYield + save.Level("cultivo"),
                grow = 1 + save.Level("cultivo") * 0.25f,
            };
            if (player != null) player.ApplyStats(stats);
        }

        public void Persist()
        {
            save.cherries = cherries;
            FGSave.Write(save);
        }

        void OnApplicationPause(bool paused) { if (paused && save != null) Persist(); }
        void OnApplicationQuit() { if (save != null) Persist(); }

        // ---------- Estados ----------
        public void Play()
        {
            State = GameState.Playing;
            menus.Hide();
            hud.Show(true);
            touch.Show(FGInput.IsTouchDevice);
            FGInput.CursorLocked = true;
            cursorWasLocked = false;
            FGInput.EndFrame();
            if (currentDistrict == ' ')
            {
                currentDistrict = City.DistrictAt(player.transform.position);
                hud.Banner(FGConfig.DistrictName(currentDistrict).ToUpperInvariant(), "Float Gardens HQ · Bogotá", true);
            }
            UpdateZoneColors();
            story.OnPlay();
        }

        /// <summary>Diálogo de la campaña: el mundo se congela y al terminar vuelve a Playing (o sigue el callback).</summary>
        public void StartDialogue(Line[] lines, System.Action then)
        {
            var before = State;
            State = GameState.Dialogue;
            FGInput.CursorLocked = false;
            touch.Show(false);
            SetMap(false);
            dialogue.Play(lines, () =>
            {
                if (State == GameState.Dialogue)
                {
                    State = GameState.Playing;
                    FGInput.CursorLocked = true;
                    cursorWasLocked = false;
                    touch.Show(FGInput.IsTouchDevice);
                    FGInput.EndFrame();
                }
                then?.Invoke();
            });
        }

        public void OpenLab()
        {
            State = GameState.Lab;
            FGInput.CursorLocked = false;
            touch.Show(false);
            menus.Show("lab");
        }

        /// <summary>Resultado de una preparación en el laboratorio: guarda el récord, da puntos y avanza la campaña.</summary>
        public void OnBrew(BrewRecipe r, BrewResult res)
        {
            save.brewCount++;
            save.bestBrew = Mathf.Max(save.bestBrew, res.score);
            bool first = save.BestBrew(r.variety) <= 0;
            save.brews.Add(new BrewRecord { variety = r.variety, score = res.score });
            if (save.brews.Count > 60) save.brews.RemoveAt(0);
            int pts = res.score >= 75 ? Mathf.RoundToInt((res.score - 70) * 3) : 0;
            if (pts > 0) AddPoints(pts);
            if (first) story.UnlockCodex("variables", false);
            Persist();
            story.OnEvent(MissionKind.Brew, 1, r.variety, res.score);
        }

        public void Pause()
        {
            State = GameState.Paused;
            Persist();
            FGInput.CursorLocked = false;
            touch.Show(false);
            SetMap(false);
            menus.Show("pause");
        }

        void OpenShop()
        {
            State = GameState.Shop;
            FGInput.CursorLocked = false;
            touch.Show(false);
            menus.Show("shop");
        }

        public void ToLobby()
        {
            Persist();
            State = GameState.Lobby;
            FGInput.CursorLocked = false;
            hud.Show(false);
            touch.Show(false);
            menus.Show("lobby");
        }

        public void Die()
        {
            player.dead = true;
            player.hp = 0;
            State = GameState.Dead;
            lastLost = cherries / 2;
            cherries -= lastLost;
            FGInput.CursorLocked = false;
            touch.Show(false);
            menus.Show("dead");
        }

        public void Respawn()
        {
            player.Revive(city.hqSpawn);
            cherries = Mathf.Max(cherries, 4);
            foreach (var e in enemies) e.alerted = false;
            foreach (var s in stamps) Destroy(s.gameObject);
            stamps.Clear();
            Play();
        }

        public void ResetWorld()
        {
            save.captured.Clear();
            save.upgrades.Clear();
            save.chapter = 0;
            save.mission = 0;
            save.missionProgress = 0;
            save.introSeen = false;
            save.kos = 0;
            save.sales = 0;
            save.bossDefeated = false;
            save.cherries = FGConfig.StartCherries;
            FGSave.Write(save);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // ---------- Territorio ----------
        public bool IsMarket(Vector3 p) => owned.Contains(City.DistrictAt(p));
        public bool AllCaptured => zones.TrueForAll(z => z.ours);
        public int CapturedCount => zones.FindAll(z => z.ours).Count;

        void RecomputeOwned()
        {
            owned.Clear();
            owned.Add(FGConfig.HQDistrict);
            foreach (var z in zones) if (z.ours) owned.Add(z.def.district);
        }

        void UpdateZoneColors()
        {
            Vector3 p = player.transform.position;
            targetZone = null;
            float best = float.MaxValue;
            foreach (var z in zones)
            {
                if (z.ours) continue;
                float d = Vector3.Distance(z.transform.position, p);
                if (d < best) { best = d; targetZone = z; }
            }
            foreach (var z in zones) z.SetState(z.ours, z == targetZone);
            RecomputeOwned();
        }

        public Objective_ Objective()
        {
            if (story != null && !story.Finished) return story.Objective();
            if (!AllCaptured)
                return new Objective_ { title = "Captura: " + (targetZone != null ? targetZone.def.name : ""), sub = "Zonas " + CapturedCount + "/" + zones.Count, target = targetZone != null ? targetZone.transform.position : Vector3.zero, hasTarget = targetZone != null };
            if (!save.bossDefeated)
                return new Objective_ { title = "Derrota a " + FGConfig.BossName, sub = "Plaza de Bolívar", target = city.bossArena, hasTarget = true };
            return new Objective_ { title = "Bogotá es de Float Gardens", sub = "Sigue farmeando Puntos Float" };
        }

        // ---------- Bucle ----------
        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            foreach (var p in peds) p.Tick(dt, this);

            switch (State)
            {
                case GameState.Lobby:
                    camRig.Lobby(city.hqSpawn, Time.time);
                    player.rig.transform.rotation = Quaternion.Euler(0, 160, 0);
                    player.rig.Animate(Time.time * 2, 0.05f, false);
                    break;
                case GameState.Playing:
                    TickPlaying(dt);
                    break;
                case GameState.Dead:
                    player.rig.SetDead(1);
                    camRig.Follow(dt);
                    hud.Tick(this, dt);
                    break;
                case GameState.Dialogue:
                    player.rig.Animate(Time.time * 2, 0.05f, false);
                    camRig.Follow(dt);
                    atmosphere.Tick(dt, cam, hud);
                    break;
            }
        }

        void LateUpdate() { FGInput.EndFrame(); }

        void TickPlaying(float dt)
        {
            if (FGInput.Pressed(FGAction.Pause)) { Pause(); return; }
            // En el editor, Esc suelta el mouse: lo tratamos como pausa
            if (!FGInput.IsTouchDevice)
            {
                if (FGInput.CursorLocked) cursorWasLocked = true;
                else if (cursorWasLocked) { Pause(); return; }
                else if (FGInput.ClickedAnywhere()) FGInput.CursorLocked = true;
            }
            if (FGInput.Pressed(FGAction.Map)) SetMap(!showMap);

            player.Tick(dt, this);
            camRig.Follow(dt);
            Interaction();
            if (State != GameState.Playing) return;
            Spawns(dt);

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i];
                e.Tick(dt, this);
                if (e.Gone) { Destroy(e.gameObject); enemies.RemoveAt(i); }
            }
            Zones(dt);
            for (int i = stamps.Count - 1; i >= 0; i--)
                if (!stamps[i].Tick(dt, this)) { Destroy(stamps[i].gameObject); stamps.RemoveAt(i); }
            foreach (var pl in planters) pl.Tick(dt, stats.grow);
            Pickups();

            char d = City.DistrictAt(player.transform.position);
            if (d != currentDistrict)
            {
                currentDistrict = d;
                bool ours = owned.Contains(d);
                hud.Banner(FGConfig.DistrictName(d).ToUpperInvariant(), ours ? "Territorio Float Gardens" : "Territorio de " + FGConfig.Faction, ours);
            }
            zoneColorTimer -= dt;
            if (zoneColorTimer <= 0) { zoneColorTimer = 1; UpdateZoneColors(); }
            atmosphere.Tick(dt, cam, hud);
            autosaveTimer -= dt;
            if (autosaveTimer <= 0) { autosaveTimer = 10; Persist(); }
            if (State == GameState.Playing) hud.Tick(this, dt);
        }

        void SetMap(bool on)
        {
            showMap = on;
            hud.SetBigMap(on, this);
        }

        // ---------- Interacción ----------
        void Interaction()
        {
            prompt = null;
            Vector3 p = player.transform.position;
            bool Near(Vector3 o, float r) { o.y = p.y; return Vector3.Distance(o, p) < r; }
            string use = FGInput.IsTouchDevice ? "[E]" : "[E]";

            foreach (var pl in planters)
            {
                if (pl.Ripe && Near(pl.transform.position, pl.onWater ? 4 : 3.3f))
                {
                    prompt = use + " Cosechar café hidropónico";
                    if (FGInput.Pressed(FGAction.Use))
                    {
                        pl.Harvest();
                        cherries += stats.yield;
                        AddPoints(stats.yield * FGConfig.PointsPerCherry);
                        hud.Toast("Cosecha: +" + stats.yield + " cerezas · +" + stats.yield + " ★");
                        SFX.Play("pickup");
                        FX.Burst(pl.transform.position + Vector3.up * 1.2f, FGMaterials.Hex("#d6423a"), 10, 3);
                        story.OnEvent(MissionKind.Harvest, stats.yield);
                    }
                    return;
                }
            }
            foreach (var n in story.npcs)
            {
                if (!Near(n.transform.position, 3f)) continue;
                prompt = use + " Hablar con " + n.def.name;
                if (FGInput.Pressed(FGAction.Use)) story.Talk(n);
                return;
            }
            if (Near(city.hqLab, 2.6f))
            {
                prompt = use + " Laboratorio AeroPress (tu PC)";
                if (FGInput.Pressed(FGAction.Use)) OpenLab();
                return;
            }
            if (Near(city.hqShop, 3.2f))
            {
                prompt = use + " Tienda de mejoras";
                if (FGInput.Pressed(FGAction.Use)) OpenShop();
                return;
            }
            foreach (var c in peds)
            {
                if (!c.IconVisible || !Near(c.transform.position, 2.6f)) continue;
                if (cherries < FGConfig.CherriesPerSale) { prompt = "Necesitas " + FGConfig.CherriesPerSale + " cerezas para preparar un café"; return; }
                prompt = use + " Vender café AeroPress · " + FGConfig.Money(FGConfig.SalePrice);
                if (FGInput.Pressed(FGAction.Use))
                {
                    cherries -= FGConfig.CherriesPerSale;
                    save.money += FGConfig.SalePrice;
                    save.sales++;
                    AddPoints(FGConfig.PointsPerSale);
                    c.Served();
                    hud.Toast("¡Venta! +" + FGConfig.Money(FGConfig.SalePrice) + " · +" + FGConfig.PointsPerSale + " ★");
                    SFX.Play("cash");
                    story.OnEvent(MissionKind.Sell, 1);
                }
                return;
            }
            if (player.ammo == 0 && cherries < FGConfig.CherriesPerReload) prompt = "Sin café: cosecha en el HQ o abre cajas";
        }

        void Pickups()
        {
            Vector3 p = player.transform.position;
            for (int i = loot.Count - 1; i >= 0; i--)
            {
                var l = loot[i];
                Vector3 lp = l.transform.position;
                lp.y = p.y;
                if (Vector3.Distance(lp, p) > 1.6f) continue;
                cherries += l.amount;
                AddPoints(l.amount * FGConfig.PointsPerCherry);
                hud.Toast(l.crate ? "Caja de café: +" + l.amount + " cerezas" : "+" + l.amount + " cerezas");
                SFX.Play("pickup");
                if (l.crate) crateRespawns.Add(Time.time + 8);
                Destroy(l.gameObject);
                loot.RemoveAt(i);
            }
            for (int i = crateRespawns.Count - 1; i >= 0; i--)
                if (Time.time > crateRespawns[i]) { crateRespawns.RemoveAt(i); SpawnCrate(); }
        }

        void SpawnCrate()
        {
            loot.Add(Loot.Spawn(city.SidewalkPoint(), Random.Range(3, 7), true, transform));
        }

        void AddZonePlanters(CaptureZone z)
        {
            if (!city.planterSpots.TryGetValue(z.def.id, out var spots)) return;
            foreach (var s in spots) planters.Add(Planter.Spawn(s, z.def.id == "simon", transform));
        }

        public void AddPoints(int n)
        {
            save.puntos += n;
            save.puntosTotal += n;
        }

        // ---------- Combate ----------
        public void HitEnemy(Enemy e, bool head, Vector3 at)
        {
            float dmg = stats.damage * (head ? FGConfig.HeadshotMul : 1) * Random.Range(0.9f, 1.1f);
            e.Hit(dmg);
            hud.DamageNumber(at, dmg, head);
            hud.HitMarker(head, e.Dead);
            SFX.Play(head ? "head" : "hit");
            if (e.Dead) OnKO(e, head);
        }

        void OnKO(Enemy e, bool head)
        {
            SFX.Play("ko");
            save.kos++;
            AddPoints(e.isBoss ? FGConfig.PointsPerBoss : FGConfig.PointsPerKO);
            save.money += e.isBoss ? FGConfig.BossMoney : FGConfig.KOMoney;
            hud.KillFeed("<color=#7dffb0>Barista</color>  AeroPress  <color=#ff8a7a>" + e.displayName + "</color>" + (head ? "  <color=#ffc21a>HEADSHOT</color>" : ""));
            Say(e, FGConfig.KOLines[Random.Range(0, FGConfig.KOLines.Length)]);
            FX.Burst(e.Chest, FGMaterials.Hex("#6b3b1f"), 14, 4);
            if (!e.isBoss && Random.value < 0.45f) loot.Add(Loot.Spawn(e.transform.position, 2, false, transform));
            if (e.isBoss) OnBossDefeated();
        }

        public void ThrowStamp(Enemy from, float dmg, float angleOffDeg, float speedMul)
        {
            Vector3 start = from.transform.position + Vector3.up * (from.isBoss ? 3.6f : 1.5f);
            Vector3 target = player.transform.position + Vector3.up * 1.2f;
            Vector3 d = (target - start).normalized;
            d = Quaternion.Euler(0, angleOffDeg, 0) * d;
            stamps.Add(Stamp.Spawn(start, d * 24 * speedMul, dmg, from, transform));
        }

        public void SummonAround(Vector3 pos, int n)
        {
            for (int i = 0; i < n; i++)
            {
                float a = Random.value * Mathf.PI * 2;
                SpawnEnemy(FGConfig.Inspector, pos + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * 6, true, "boss");
            }
        }

        public void Say(Enemy e, string text) { hud.Bubble(e, text); }

        Enemy SpawnEnemy(FGConfig.EnemyType t, Vector3 pos, bool alerted, string zoneId)
        {
            var e = Enemy.Spawn(t, pos + Vector3.up * 0.3f, 1 + CapturedCount * 0.1f, alerted, zoneId, actors);
            enemies.Add(e);
            return e;
        }

        void Spawns(float dt)
        {
            Vector3 p = player.transform.position;
            spawnTimer -= dt;
            int cap = 3 + CapturedCount;
            int patrols = enemies.FindAll(e => string.IsNullOrEmpty(e.zoneId) && !e.Dead).Count;
            if (spawnTimer <= 0 && patrols < cap)
            {
                spawnTimer = 2.5f;
                for (int tries = 0; tries < 12; tries++)
                {
                    var s = city.SidewalkPoint(c => !owned.Contains(c) && c != 'P');
                    float d = Vector3.Distance(s, p);
                    if (d < 45 || d > 120) continue;
                    SpawnEnemy(Random.value < 0.3f + CapturedCount * 0.07f ? FGConfig.Guardia : FGConfig.Inspector, s, false, null);
                    break;
                }
            }
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i];
                if (string.IsNullOrEmpty(e.zoneId) && !e.alerted && Vector3.Distance(e.transform.position, p) > 150) { Destroy(e.gameObject); enemies.RemoveAt(i); }
            }
            if (AllCaptured && !save.bossDefeated && boss == null && Vector3.Distance(city.bossArena, p) < 70)
            {
                boss = Boss.SpawnBoss(city.bossArena + new Vector3(8, 0.3f, 10), actors);
                enemies.Add(boss);
                hud.Banner(FGConfig.BossName.ToUpperInvariant(), "El " + FGConfig.BossTitle + " sale de la torre", false);
                Say(boss, "¡Nadie compite con Coffee Land!");
            }
        }

        void Zones(float dt)
        {
            Vector3 p = player.transform.position;
            capturing = null;
            foreach (var z in zones)
            {
                if (z.ours) continue;
                Vector3 zp = z.transform.position;
                bool inside = !player.dead && Vector2.Distance(new Vector2(zp.x, zp.z), new Vector2(p.x, p.z)) < FGConfig.CaptureRadius;
                if (!inside) { z.progress = Mathf.Max(0, z.progress - dt * 0.01f); continue; }
                int n = CapturedCount;
                if (z.waves == 0 || (z.waves == 1 && z.progress > 0.4f) || (z.waves == 2 && z.progress > 0.75f))
                {
                    int count = 2 + n / (z.waves == 0 ? 2 : 3);
                    Vector3 cell = City.CellCenter(z.def.row, z.def.col);
                    float half = FGConfig.Cell / 2;
                    for (int i = 0; i < count; i++)
                    {
                        // los refuerzos llegan por las calles que rodean la zona
                        float t = Random.Range(-22f, 22f);
                        Vector3 pt;
                        switch (Random.Range(0, 4))
                        {
                            case 0: pt = new Vector3(cell.x + t, 0, cell.z - half); break;
                            case 1: pt = new Vector3(cell.x + t, 0, cell.z + half); break;
                            case 2: pt = new Vector3(cell.x - half, 0, cell.z + t); break;
                            default: pt = new Vector3(cell.x + half, 0, cell.z + t); break;
                        }
                        var type = i % 3 == 2 || Random.value < 0.2f + n * 0.05f ? FGConfig.Guardia : FGConfig.Inspector;
                        SpawnEnemy(type, pt, true, z.def.id);
                    }
                    if (z.waves == 0) hud.Banner(z.def.name.ToUpperInvariant(), "¡Defiende la zona hasta capturarla!", false);
                    z.waves++;
                }
                bool contested = enemies.Exists(e => !e.Dead && Vector2.Distance(new Vector2(e.transform.position.x, e.transform.position.z), new Vector2(zp.x, zp.z)) < FGConfig.CaptureRadius + 8);
                if (!contested) z.progress = Mathf.Min(1, z.progress + dt / FGConfig.CaptureTime);
                capturing = z;
                capturingContested = contested;
                if (z.progress >= 1) Capture(z);
            }
        }

        void Capture(CaptureZone z)
        {
            z.ours = true;
            z.progress = 1;
            save.captured.Add(z.def.id);
            AddPoints(FGConfig.PointsPerZone);
            save.money += FGConfig.ZoneMoney;
            AddZonePlanters(z);
            SFX.Play("capture");
            hud.Banner("¡ZONA CAPTURADA!", z.def.name + " ahora es Float Gardens · " + CapturedCount + "/" + zones.Count, true);
            hud.KillFeed("<color=#7dffb0>Float Gardens</color> capturó <b>" + z.def.name + "</b>");
            foreach (var e in enemies) if (e.zoneId == z.def.id && !e.Dead) { e.hp = 0; Say(e, "¡Retirada!"); }
            UpdateZoneColors();
            Persist();
            story.OnEvent(MissionKind.Capture, 1, z.def.id);
            if (AllCaptured) Invoke(nameof(AnnounceBoss), 3.2f);
        }

        void AnnounceBoss() { hud.Banner("LA TORRE ESTÁ EXPUESTA", "Ve a la Plaza de Bolívar y derrota a " + FGConfig.BossName, false); }

        void OnBossDefeated()
        {
            save.bossDefeated = true;
            foreach (var e in enemies) if (!e.Dead) e.hp = 0;
            Persist();
            SFX.Play("capture");
            hud.Banner("¡BOGOTÁ ES DE FLOAT GARDENS!", FGConfig.BossName + " colgó el sombrero", true);
            story.OnEvent(MissionKind.Boss);
            Invoke(nameof(ShowVictory), 3.5f);
        }

        void ShowVictory()
        {
            if (State == GameState.Dialogue) { Invoke(nameof(ShowVictory), 2f); return; }
            if (State != GameState.Playing) return;
            State = GameState.Paused;
            FGInput.CursorLocked = false;
            touch.Show(false);
            menus.Show("victory");
        }
    }
}
