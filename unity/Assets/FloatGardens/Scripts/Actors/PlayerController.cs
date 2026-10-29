// El barista: movimiento en tercera persona, salto, vida regenerable y la AeroPress X-1.
using UnityEngine;

namespace FloatGardens
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController I;

        public CharacterController cc;
        public Humanoid rig;
        public Transform muzzle;
        public float yaw = 180f, pitch = -7f; // grados
        public float hp, maxHp;
        public int ammo, magSize;
        public float reloading;
        public bool dead, aiming, sprinting;
        public float spread = 0.02f, recoil;
        public float Moving { get; private set; }
        public float lastHurt = -99, lastShot = -99;

        float vy, fireCool, phase;
        Vector3 knock;
        Light flash;
        float flashTime;

        public static PlayerController Create(GameObject modelPrefab)
        {
            var go = new GameObject("Barista");
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.9f;
            cc.radius = 0.45f;
            cc.center = new Vector3(0, 0.95f, 0);
            cc.stepOffset = 0.35f;
            cc.slopeLimit = 50;
            var p = go.AddComponent<PlayerController>();
            p.cc = cc;
            p.rig = Humanoid.Create("Modelo", new HumanoidStyle
            {
                shirt = FGMaterials.Hex("#f5f1e8"), pants = FGMaterials.Hex("#2c2a27"), apron = FGMaterials.Hex("#0f3d2e"),
                cap = FGMaterials.Hex("#2fbf71"), hair = FGMaterials.Hex("#2a1a10"), skin = FGMaterials.Hex("#c98f62"),
            }, go.transform, modelPrefab);
            p.muzzle = Humanoid.MakeAeroPress(p.rig.rightHand);
            var lightGo = new GameObject("Destello");
            lightGo.transform.SetParent(p.muzzle, false);
            p.flash = lightGo.AddComponent<Light>();
            p.flash.type = LightType.Point;
            p.flash.color = FGMaterials.Hex("#ffb35c");
            p.flash.range = 6;
            p.flash.intensity = 0;
            p.flash.shadows = LightShadows.None;
            I = p;
            return p;
        }

        public void ApplyStats(GameManager.Stats s)
        {
            float ratio = maxHp > 0 ? hp / maxHp : 1;
            maxHp = s.hp;
            hp = Mathf.Min(maxHp, Mathf.Max(hp, maxHp * ratio));
            magSize = s.mag;
        }

        public void Teleport(Vector3 pos, bool resetView)
        {
            cc.enabled = false;
            transform.position = pos;
            cc.enabled = true;
            if (resetView) { yaw = 180; pitch = -7; }
        }

        public Vector3 Forward => Quaternion.Euler(0, yaw, 0) * Vector3.forward;
        public Vector3 Right => Quaternion.Euler(0, yaw, 0) * Vector3.right;
        public bool Grounded => cc.isGrounded;

        public void Tick(float dt, GameManager g)
        {
            var look = FGInput.Look();
            float aimMul = aiming ? 0.55f : 1f;
            yaw += look.x * aimMul;
            pitch = Mathf.Clamp(pitch + look.y * aimMul, -63, 43);
            aiming = FGInput.Held(FGAction.Aim);

            Vector2 mv = FGInput.Move();
            Vector3 dir = Forward * mv.y + Right * mv.x;
            float len = Mathf.Min(1, dir.magnitude);
            sprinting = FGInput.Held(FGAction.Sprint) || FGInput.touchMove.magnitude > 0.95f;
            sprinting = sprinting && mv.y > 0.3f && !aiming && reloading <= 0 && len > 0.1f;
            float speed = (sprinting ? FGConfig.SprintSpeed : FGConfig.WalkSpeed) * g.stats.speed * (aiming ? 0.6f : 1f);

            if (cc.isGrounded && vy < 0) vy = -2f;
            if (FGInput.Pressed(FGAction.Jump) && cc.isGrounded) { vy = FGConfig.JumpSpeed; SFX.Play("jump", 0.6f); }
            vy -= FGConfig.Gravity * dt;
            Vector3 motion = dir.normalized * (len * speed) + knock + Vector3.up * vy;
            cc.Move(motion * dt);
            knock = Vector3.Lerp(knock, Vector3.zero, dt * 6);

            Moving = len;
            phase += dt * (sprinting ? 13 : 9) * len;
            bool combat = aiming || Time.time - lastShot < 0.8f || FGInput.Held(FGAction.Fire);
            float face = yaw;
            if (!combat && len > 0.1f) face = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            rig.transform.rotation = Quaternion.Slerp(rig.transform.rotation, Quaternion.Euler(0, face, 0), Mathf.Min(1, dt * 14));
            rig.Animate(phase, len * (sprinting ? 1.2f : 0.8f), combat);

            if (!dead && Time.time - lastHurt > FGConfig.RegenDelay && hp < maxHp) hp = Mathf.Min(maxHp, hp + FGConfig.RegenRate * dt);

            UpdateWeapon(dt, g);
            if (flashTime > 0) { flashTime -= dt; flash.intensity = flashTime > 0 ? 4 : 0; }
        }

        void UpdateWeapon(float dt, GameManager g)
        {
            fireCool -= dt;
            recoil = Mathf.Max(0, recoil - dt * 8.6f);
            float baseSpread = aiming ? 0.006f : 0.018f;
            float target = baseSpread + Moving * (aiming ? 0.01f : 0.03f) + (cc.isGrounded ? 0 : 0.05f);
            spread = Mathf.Lerp(spread, target, Mathf.Min(1, dt * 8));

            if (reloading > 0)
            {
                reloading -= dt;
                if (reloading <= 0) { ammo = magSize; g.cherries -= FGConfig.CherriesPerReload; }
                return;
            }
            if (FGInput.Pressed(FGAction.Reload) && ammo < magSize) Reload(g);
            if (FGInput.Held(FGAction.Fire) && fireCool <= 0 && !sprinting)
            {
                if (ammo <= 0)
                {
                    if (FGInput.Pressed(FGAction.Fire)) { SFX.Play("empty"); Reload(g); }
                    return;
                }
                Fire(g);
            }
        }

        public void Reload(GameManager g)
        {
            if (reloading > 0) return;
            if (g.cherries < FGConfig.CherriesPerReload)
            {
                g.hud.Toast("¡Sin cerezas! Cosecha en el HQ o busca cajas de café");
                return;
            }
            reloading = FGConfig.ReloadTime;
            SFX.Play("reload");
        }

        void Fire(GameManager g)
        {
            fireCool = FGConfig.FireInterval;
            ammo--;
            lastShot = Time.time;
            recoil = Mathf.Min(2f, recoil + 0.35f);
            spread += 0.008f;
            SFX.Play("shot", 0.8f);
            flashTime = 0.05f;

            Vector3 from, d;
            if (FGStyle.Miniatura)
            {
                // Cámara de maqueta: sin mira central, se dispara desde el pecho hacia donde mira el barista
                from = transform.position + Vector3.up * 1.4f;
                d = AutoAim(g, from) + (Right * Random.Range(-1f, 1f) + Vector3.up * Random.Range(-0.3f, 0.3f)) * spread;
                d.Normalize();
            }
            else
            {
                var cam = g.cam.transform;
                d = cam.forward + (cam.right * Random.Range(-1f, 1f) + cam.up * Random.Range(-1f, 1f)) * spread;
                d.Normalize();
                from = cam.position + d * g.camRig.Distance;
            }
            Vector3 hitPoint = from + d * FGConfig.Range;
            Enemy target = null;
            bool head = false;
            if (Phys.Raycast(from, d, FGConfig.Range, cc, out RaycastHit hit))
            {
                hitPoint = hit.point;
                target = hit.collider.GetComponentInParent<Enemy>();
                if (target != null) head = hit.point.y > target.HeadHeight;
            }
            // ¿una pared tapa el disparo desde la boca del arma?
            Vector3 md = hitPoint - muzzle.position;
            if (Phys.Raycast(muzzle.position, md.normalized, md.magnitude - 0.3f, cc, out RaycastHit block))
            {
                if (target == null || block.collider.GetComponentInParent<Enemy>() != target) { target = null; hitPoint = block.point; }
            }
            FX.Tracer(muzzle.position, hitPoint);
            FX.Burst(muzzle.position, FGMaterials.Hex("#8b5a2b"), 3, 2);
            FX.Burst(hitPoint, FGMaterials.Hex("#6b3b1f"), 6, 3);
            if (target != null && !target.Dead) g.HitEnemy(target, head, hitPoint);
        }

        /// <summary>
        /// Puntería asistida de la maqueta: apunta al enemigo vivo más cercano dentro de un cono de 25° al frente y a la
        /// vista (al pecho; a la cabeza si el barista está apuntando). Si no hay ninguno, dispara recto al frente.
        /// </summary>
        Vector3 AutoAim(GameManager g, Vector3 from)
        {
            Vector3 best = Forward;
            float bestScore = float.MaxValue;
            foreach (var e in g.enemies)
            {
                if (e == null || e.Dead) continue;
                Vector3 aimAt = e.transform.position + Vector3.up * (aiming ? e.HeadHeight - e.transform.position.y - 0.1f : 1.1f);
                Vector3 to = aimAt - from;
                float dist = to.magnitude;
                if (dist < 0.5f || dist > FGConfig.Range) continue;
                float angle = Vector3.Angle(Forward, new Vector3(to.x, 0, to.z));
                if (angle > 25f) continue;
                float score = angle * 1.5f + dist;
                if (score >= bestScore || !Phys.LineOfSight(from, aimAt, cc, null)) continue;
                bestScore = score;
                best = to / dist;
            }
            return best;
        }

        public void Damage(float dmg, Vector3 from, float knockForce, GameManager g)
        {
            if (dead) return;
            hp -= dmg;
            lastHurt = Time.time;
            g.hud.Hurt(dmg);
            SFX.Play("hurt");
            g.camRig.Shake(0.15f);
            if (knockForce > 0)
            {
                Vector3 k = transform.position - from;
                k.y = 0;
                knock = k.normalized * knockForce;
            }
            if (hp <= 0) g.Die();
        }

        public void Revive(Vector3 at)
        {
            dead = false;
            hp = maxHp;
            ammo = magSize;
            reloading = 0;
            rig.ResetPose();
            Teleport(at, true);
        }
    }

    /// <summary>Raycast que ignora el propio collider del jugador.</summary>
    public static class Phys
    {
        static readonly RaycastHit[] buffer = new RaycastHit[32];

        public static bool Raycast(Vector3 origin, Vector3 dir, float maxDist, Collider ignore, out RaycastHit best)
        {
            best = default;
            if (maxDist <= 0) return false;
            int n = Physics.RaycastNonAlloc(origin, dir, buffer, maxDist, ~0, QueryTriggerInteraction.Ignore);
            float bd = float.MaxValue;
            bool found = false;
            for (int i = 0; i < n; i++)
            {
                if (buffer[i].collider == ignore) continue;
                if (buffer[i].distance < bd) { bd = buffer[i].distance; best = buffer[i]; found = true; }
            }
            return found;
        }

        public static bool LineOfSight(Vector3 a, Vector3 b, Collider ignoreA, Collider ignoreB)
        {
            Vector3 d = b - a;
            int n = Physics.RaycastNonAlloc(a, d.normalized, buffer, d.magnitude - 0.3f, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                var c = buffer[i].collider;
                if (c == ignoreA || c == ignoreB) continue;
                if (c is CharacterController) continue; // otros personajes no tapan la vista
                return false;
            }
            return true;
        }
    }
}
