// Agentes de Coffee Land: inspectores (cuerpo a cuerpo), guardias (lanzan sellos) y el jefe Don Monopolio.
using UnityEngine;

namespace FloatGardens
{
    public class Enemy : MonoBehaviour
    {
        public FGConfig.EnemyType type;
        public string displayName;
        public float hp, maxHp, speed;
        public bool alerted, isBoss;
        public string zoneId;
        public Humanoid rig;
        public CharacterController cc;
        public float scale = 1f;
        protected float cool, phase, koTime, avoid, avoidDir = 1, strafe = 1, vy;
        Vector3 home, goal;
        bool hasGoal;
        Transform barFill, barRoot;

        public bool Dead => hp <= 0;
        public bool Gone { get; protected set; }
        public float HeadHeight => transform.position.y + 1.6f * scale;
        public Vector3 Chest => transform.position + Vector3.up * 1.2f * scale;

        public static Enemy Spawn(FGConfig.EnemyType t, Vector3 pos, float hpMul, bool alerted, string zoneId, Transform parent)
        {
            var go = new GameObject(t.name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos; // antes de crear el CharacterController
            var e = go.AddComponent<Enemy>();
            e.Setup(t, pos, hpMul, alerted, zoneId);
            return e;
        }

        protected void Setup(FGConfig.EnemyType t, Vector3 pos, float hpMul, bool alerted, string zoneId)
        {
            type = t;
            displayName = t.name;
            maxHp = hp = t.hp * hpMul;
            speed = t.speed * Random.Range(0.9f, 1.1f);
            this.alerted = alerted;
            this.zoneId = zoneId;
            cool = 1 + Random.value;
            phase = Random.value * 6;
            strafe = Random.value < 0.5f ? 1 : -1;
            home = pos;
            cc = gameObject.AddComponent<CharacterController>();
            cc.height = 1.9f * scale;
            cc.radius = 0.45f * scale;
            cc.center = new Vector3(0, 0.95f * scale, 0);
            cc.stepOffset = 0.35f;
            BuildRig();
            BuildBar();
        }

        protected virtual void BuildRig()
        {
            rig = Humanoid.Create("Modelo", new HumanoidStyle
            {
                shirt = type.color, pants = FGMaterials.Hex("#26282d"), hat = type.hat, tie = FGMaterials.Hex("#c0392b"), badge = true,
                mustache = Random.value < 0.5f, skin = Random.value < 0.5f ? FGMaterials.Hex("#d9a273") : FGMaterials.Hex("#a8714a"),
            }, transform, GameManager.I != null ? GameManager.I.enemyModel : null);
        }

        void BuildBar()
        {
            barRoot = new GameObject("BarraVida").transform;
            barRoot.SetParent(transform, false);
            barRoot.localPosition = Vector3.up * (2.45f * scale);
            var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(bg.GetComponent<Collider>());
            bg.transform.SetParent(barRoot, false);
            bg.transform.localScale = new Vector3(0.96f, 0.14f, 1) * (isBoss ? 2 : 1);
            bg.GetComponent<MeshRenderer>().sharedMaterial = FGMaterials.Transparent("barraFondo", new Color(0, 0, 0, 0.6f));
            var fill = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(fill.GetComponent<Collider>());
            barFill = fill.transform;
            barFill.SetParent(barRoot, false);
            barFill.localPosition = new Vector3(0, 0, -0.01f);
            barFill.localScale = new Vector3(0.9f, 0.09f, 1) * (isBoss ? 2 : 1);
            fill.GetComponent<MeshRenderer>().sharedMaterial = FGMaterials.Transparent("barraVida", FGMaterials.Hex("#ff4b3a"));
            barRoot.gameObject.SetActive(false);
        }

        public virtual void Hit(float dmg)
        {
            hp -= dmg;
            alerted = true;
        }

        public virtual void Tick(float dt, GameManager g)
        {
            if (Dead) { Die(dt); return; }
            var p = g.player;
            Vector3 to = p.transform.position - transform.position;
            to.y = 0;
            float dist = to.magnitude;
            if (!alerted && dist < 32 && !p.dead) { alerted = true; g.Say(this, FGConfig.EnemyLines[Random.Range(0, FGConfig.EnemyLines.Length)]); }
            if (alerted && (dist > 90 || p.dead)) alerted = false;

            Vector3 mv = Vector3.zero;
            float? face = null;
            if (alerted)
            {
                face = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                Vector3 n = dist > 0.01f ? to / dist : Vector3.forward;
                if (type.ranged <= 0)
                {
                    if (dist > 1.3f) mv = n;
                    cool -= dt;
                    if (dist < 1.6f && cool <= 0) { cool = 1.2f; p.Damage(type.melee, transform.position, 0, g); }
                }
                else
                {
                    float want = type.range * 0.6f;
                    if (dist > want + 4) mv = n;
                    else if (dist < want - 6) mv = -n;
                    else mv = new Vector3(-n.z, 0, n.x) * strafe * 0.6f;
                    if (Random.value < dt * 0.3f) strafe = -strafe;
                    cool -= dt;
                    if (cool <= 0 && dist < type.range && Phys.LineOfSight(Chest + Vector3.up * 0.3f, p.transform.position + Vector3.up * 1.3f, cc, p.cc))
                    {
                        cool = type.cooldown * Random.Range(0.8f, 1.2f);
                        g.ThrowStamp(this, type.ranged, Random.Range(-3.5f, 3.5f), 1f);
                    }
                }
            }
            else
            {
                if (!hasGoal || Vector3.Distance(Flat(goal), Flat(transform.position)) < 1)
                {
                    goal = home + new Vector3(Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f));
                    hasGoal = true;
                }
                Vector3 gd = Flat(goal - transform.position);
                mv = gd.normalized * 0.45f;
            }
            Steer(mv, speed, dt);
            float moving = mv.magnitude;
            if (moving > 0.05f) { phase += dt * 10 * moving; if (!face.HasValue) face = Mathf.Atan2(mv.x, mv.z) * Mathf.Rad2Deg; }
            if (face.HasValue) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, face.Value, 0), Mathf.Min(1, dt * 10));
            rig.Animate(phase, Mathf.Min(1, moving * 1.5f), type.ranged > 0 && alerted);
            UpdateBar(g);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        /// <summary>Avanza; si se atasca contra algo, se desvía de lado un momento.</summary>
        protected void Steer(Vector3 mv, float spd, float dt)
        {
            if (avoid > 0)
            {
                avoid -= dt;
                mv = mv * 0.3f + new Vector3(-mv.z, 0, mv.x) * avoidDir;
            }
            Vector3 before = transform.position;
            if (cc.isGrounded && vy < 0) vy = -2;
            vy -= FGConfig.Gravity * dt;
            cc.Move((mv * spd + Vector3.up * vy) * dt);
            float want = Flat(mv).magnitude * spd * dt;
            float moved = Flat(transform.position - before).magnitude;
            if (want > 0.01f && moved < want * 0.3f && avoid <= 0)
            {
                avoid = Random.Range(0.8f, 1.4f);
                avoidDir = Random.value < 0.5f ? 1 : -1;
            }
        }

        protected void UpdateBar(GameManager g)
        {
            bool show = !Dead && (hp < maxHp || alerted) && !isBoss;
            barRoot.gameObject.SetActive(show);
            if (!show) return;
            barRoot.rotation = g.cam.transform.rotation;
            float f = Mathf.Clamp01(hp / maxHp);
            barFill.localScale = new Vector3(0.9f * f, 0.09f, 1);
            barFill.localPosition = new Vector3(-0.45f * (1 - f), 0, -0.01f);
        }

        void Die(float dt)
        {
            if (koTime == 0) { cc.enabled = false; barRoot.gameObject.SetActive(false); }
            koTime += dt;
            rig.SetDead(koTime);
            if (koTime > 1.2f) transform.position += Vector3.down * dt * 1.5f;
            if (koTime > (isBoss ? 4 : 2)) Gone = true;
        }
    }

    public class Boss : Enemy
    {
        float volley = 2.5f, summon = 8f, slam;

        public static Boss SpawnBoss(Vector3 pos, Transform parent)
        {
            var go = new GameObject(FGConfig.BossName);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var b = go.AddComponent<Boss>();
            b.scale = 2.6f;
            b.isBoss = true;
            b.Setup(FGConfig.Guardia, pos, 1, true, "boss");
            b.displayName = FGConfig.BossName;
            b.maxHp = b.hp = FGConfig.BossHp;
            b.speed = 2.6f;
            return b;
        }

        protected override void BuildRig()
        {
            rig = Humanoid.Create("Modelo", new HumanoidStyle
            {
                shirt = FGMaterials.Hex("#2f3136"), pants = FGMaterials.Hex("#18191c"), hat = "tophat", sash = FGMaterials.Hex("#c0392b"),
                badge = true, mustache = true, scale = 2.6f, skin = FGMaterials.Hex("#d9a273"),
            }, transform, GameManager.I != null ? GameManager.I.bossModel : null);
        }

        public override void Tick(float dt, GameManager g)
        {
            if (Dead) { base.Tick(dt, g); return; }
            var p = g.player;
            Vector3 to = p.transform.position - transform.position;
            to.y = 0;
            float dist = Mathf.Max(0.01f, to.magnitude);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), Mathf.Min(1, dt * 4));
            float rage = hp < maxHp * 0.5f ? 1.35f : 1f;
            if (dist > 8) { Steer(to / dist, speed * rage, dt); phase += dt * 5; }
            else Steer(Vector3.zero, 0, dt);
            rig.Animate(phase, dist > 8 ? 0.8f : 0, true);

            volley -= dt * rage;
            if (volley <= 0 && !p.dead)
            {
                volley = 2.6f;
                int n = rage > 1 ? 7 : 5;
                for (int i = 0; i < n; i++) g.ThrowStamp(this, 12, (i - (n - 1) / 2f) * 7f, 1.05f);
                g.Say(this, FGConfig.BossLines[Random.Range(0, FGConfig.BossLines.Length)]);
            }
            summon -= dt;
            if (summon <= 0) { summon = rage > 1 ? 11 : 15; g.SummonAround(transform.position, 3); }
            slam -= dt;
            if (dist < 4.5f && slam <= 0) { slam = 2; p.Damage(25, transform.position, 14, g); g.camRig.Shake(0.5f); }
        }
    }
}
