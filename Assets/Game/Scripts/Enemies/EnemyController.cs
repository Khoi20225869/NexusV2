using SoulForge.Combat;
using SoulForge.Data;
using SoulForge.Player;
using SoulForge.Rooms;
using SoulForge.UI;
using UnityEngine;
using UnityEngine.Events;

namespace SoulForge.Enemies
{
    public sealed class EnemyController : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpumCharacterView characterView;
        [SerializeField] private SpriteFlashFeedback flashFeedback;
        [SerializeField] private UnityEvent onDeath;

        private PlayerHealth target;
        private RoomController ownerRoom;
        private float currentHealth;
        private float attackCooldown;

        public CombatTeam Team => CombatTeam.Enemy;
        public Transform Transform => transform;

        private void Awake()
        {
            if (characterView == null)
            {
                characterView = GetComponent<SpumCharacterView>();
            }

            if (flashFeedback == null)
            {
                flashFeedback = GetComponent<SpriteFlashFeedback>();
            }

            currentHealth = definition != null ? definition.MaxHealth : 3f;
        }

        private void Update()
        {
            if (target == null || !target.isActiveAndEnabled)
            {
                target = FindFirstObjectByType<PlayerHealth>();
                return;
            }

            Vector2 toTarget = target.transform.position - transform.position;
            float distance = toTarget.magnitude;
            attackCooldown -= Time.deltaTime;

            if (distance > GetAttackRange())
            {
                Vector2 direction = toTarget.normalized;
                transform.position += (Vector3)(direction * GetMoveSpeed() * Time.deltaTime);
                characterView?.SetMoving(true);
                characterView?.SetFacing(direction.x);
                return;
            }

            characterView?.SetMoving(false);

            if (attackCooldown > 0f)
            {
                return;
            }

            Attack(toTarget.normalized);
        }

        public void ApplyDamage(float damage)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            flashFeedback?.Flash();
            if (currentHealth > 0f)
            {
                return;
            }

            onDeath?.Invoke();
            characterView?.PlayDeath();
            ownerRoom?.UnregisterEnemy(this);
            Destroy(gameObject);
        }

        public void Initialize(EnemyDefinition enemyDefinition, PlayerHealth playerTarget, RoomController roomOwner = null)
        {
            definition = enemyDefinition;
            target = playerTarget;
            ownerRoom = roomOwner;
            currentHealth = definition != null ? definition.MaxHealth : 3f;
            ownerRoom?.RegisterEnemy(this);
        }

        private void OnDestroy()
        {
            ownerRoom?.UnregisterEnemy(this);
        }

        private void Attack(Vector2 direction)
        {
            attackCooldown = 1f / Mathf.Max(definition != null ? definition.AttackRate : 1f, 0.01f);
            characterView?.SetFacing(direction.x);
            characterView?.PlayAttack();

            if (projectilePrefab == null || definition == null || !definition.UsesProjectile)
            {
                target.ApplyDamage(definition != null ? definition.ContactDamage : 1f);
                return;
            }

            Transform spawnPoint = firePoint != null ? firePoint : transform;
            Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            projectile.Initialize(direction, definition.ProjectileSpeed, definition.ContactDamage, CombatTeam.Enemy);
        }

        private float GetAttackRange()
        {
            return definition != null ? definition.AttackRange : 1.5f;
        }

        private float GetMoveSpeed()
        {
            return definition != null ? definition.MoveSpeed : 2f;
        }
    }
}
