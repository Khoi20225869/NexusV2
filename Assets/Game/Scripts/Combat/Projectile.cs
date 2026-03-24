using UnityEngine;
using SoulForge.Feedback;

namespace SoulForge.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private CombatTeam team = CombatTeam.Neutral;
        [SerializeField] private TransientVisualEffect impactEffectPrefab;
        [SerializeField] private Color playerImpactColor = new(1f, 0.82f, 0.32f, 1f);
        [SerializeField] private Color enemyImpactColor = new(1f, 0.28f, 0.28f, 1f);

        private Vector2 direction = Vector2.right;
        private float age;
        private GameFeelController gameFeelController;

        private void Awake()
        {
            Collider2D projectileCollider = GetComponent<Collider2D>();
            if (projectileCollider != null)
            {
                projectileCollider.isTrigger = true;
            }

            Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
            if (rigidbody2D == null)
            {
                rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody2D.gravityScale = 0f;
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void Initialize(Vector2 shootDirection, float projectileSpeed, float projectileDamage, CombatTeam projectileTeam)
        {
            direction = shootDirection.sqrMagnitude > 0.0001f ? shootDirection.normalized : Vector2.right;
            speed = projectileSpeed;
            damage = projectileDamage;
            team = projectileTeam;
        }

        private void Reset()
        {
            Collider2D projectileCollider = GetComponent<Collider2D>();
            projectileCollider.isTrigger = true;
        }

        private void Update()
        {
            if (gameFeelController == null)
            {
                gameFeelController = FindFirstObjectByType<GameFeelController>();
            }

            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            age += Time.deltaTime;

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            MonoBehaviour[] directBehaviours = other.GetComponents<MonoBehaviour>();
            if (TryApplyDamage(directBehaviours, other))
            {
                return;
            }

            MonoBehaviour[] parentBehaviours = other.GetComponentsInParent<MonoBehaviour>();
            TryApplyDamage(parentBehaviours, other);
        }

        private bool TryApplyDamage(MonoBehaviour[] behaviours, Collider2D hitCollider)
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IDamageable damageable)
                {
                    continue;
                }

                if (damageable.Team == team || damageable.Team == CombatTeam.Neutral)
                {
                    return true;
                }

                damageable.ApplyDamage(damage);
                gameFeelController?.PlayImpact(team == CombatTeam.Player, damage);
                SpawnImpact(hitCollider.ClosestPoint(transform.position));
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void SpawnImpact(Vector3 position)
        {
            if (impactEffectPrefab == null)
            {
                return;
            }

            Color color = team == CombatTeam.Player ? playerImpactColor : enemyImpactColor;
            TransientVisualEffect effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);
            effect.Play(color, 0.12f, 0.35f);
        }
    }
}
