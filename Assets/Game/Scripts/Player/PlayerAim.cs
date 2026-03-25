using SoulForge.Combat;
using SoulForge.Data;
using SoulForge.UI;
using UnityEngine;
using SoulForge.CameraSystem;
using SoulForge.Feedback;
using System.Collections.Generic;

namespace SoulForge.Player
{
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private WeaponDefinition weaponDefinition;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpumCharacterView characterView;
        [SerializeField] private WeaponRuntime weaponRuntime;
        [SerializeField] private CameraFeedback cameraFeedback;
        [SerializeField] private TransientVisualEffect hitEffectPrefab;
        [SerializeField] private GameFeelController gameFeelController;
        [SerializeField] private float meleeRadius = 0.7f;

        private float fireCooldown;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (characterView == null)
            {
                characterView = GetComponent<SpumCharacterView>();
            }

            if (weaponRuntime == null)
            {
                weaponRuntime = GetComponent<WeaponRuntime>();
            }

            if (cameraFeedback == null)
            {
                cameraFeedback = FindFirstObjectByType<CameraFeedback>();
            }

            if (gameFeelController == null)
            {
                gameFeelController = FindFirstObjectByType<GameFeelController>();
            }
        }

        private void Update()
        {
            Vector2 aimDirection = GetAimDirection();

            fireCooldown -= Time.deltaTime;

            if (!Input.GetMouseButton(0) || fireCooldown > 0f)
            {
                return;
            }

            Fire(aimDirection);
        }

        private Vector2 GetAimDirection()
        {
            if (targetCamera == null)
            {
                return Vector2.right;
            }

            Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimDirection = mouseWorld - transform.position;
            return aimDirection.sqrMagnitude > 0.0001f ? aimDirection.normalized : Vector2.right;
        }

        private void UpdateVisualFacing(Vector2 aimDirection)
        {
            if (characterView != null)
            {
                characterView?.SetFacing(aimDirection.x);
                return;
            }

            if (visualRoot == null || Mathf.Approximately(aimDirection.x, 0f))
            {
                return;
            }

            Vector3 localScale = visualRoot.localScale;
            localScale.x = Mathf.Abs(localScale.x) * (aimDirection.x >= 0f ? -1f : 1f);
            visualRoot.localScale = localScale;
        }

        private void Fire(Vector2 aimDirection)
        {
            UpdateVisualFacing(aimDirection);

            Vector3 attackCenter = GetAttackCenter(aimDirection);
            HashSet<IDamageable> damagedTargets = new();
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, meleeRadius);
            bool hitAny = false;

            for (int i = 0; i < hits.Length; i++)
            {
                if (!TryHitTarget(hits[i], damagedTargets))
                {
                    continue;
                }

                hitAny = true;
            }

            fireCooldown = 1f / Mathf.Max(GetFireRate(), 0.01f);
            characterView?.PlayAttack();
            cameraFeedback?.PlayShotKick(aimDirection);
            gameFeelController?.PlayPlayerShot();
            if (hitAny)
            {
                gameFeelController?.PlayImpact(true, GetDamage());
            }

            SpawnHitEffect(attackCenter, aimDirection);
        }

        private float GetDamage()
        {
            if (weaponRuntime != null && weaponRuntime.WeaponDefinition != null)
            {
                return weaponRuntime.Damage;
            }

            return weaponDefinition != null ? weaponDefinition.Damage : 1f;
        }

        private float GetFireRate()
        {
            if (weaponRuntime != null && weaponRuntime.WeaponDefinition != null)
            {
                return weaponRuntime.FireRate;
            }

            return weaponDefinition != null ? weaponDefinition.FireRate : 4f;
        }

        private float GetAttackRange()
        {
            if (weaponRuntime != null && weaponRuntime.WeaponDefinition != null)
            {
                return weaponRuntime.AttackRange;
            }

            return weaponDefinition != null ? weaponDefinition.AttackRange : 1.3f;
        }

        private Vector3 GetAttackCenter(Vector2 aimDirection)
        {
            float range = Mathf.Max(0.2f, GetAttackRange());
            if (firePoint != null)
            {
                range = Mathf.Max(range, Vector2.Distance(transform.position, firePoint.position));
            }

            return transform.position + (Vector3)(aimDirection * range);
        }

        private bool TryHitTarget(Collider2D hitCollider, HashSet<IDamageable> damagedTargets)
        {
            MonoBehaviour[] directBehaviours = hitCollider.GetComponents<MonoBehaviour>();
            if (TryApplyDamage(directBehaviours, damagedTargets))
            {
                return true;
            }

            MonoBehaviour[] parentBehaviours = hitCollider.GetComponentsInParent<MonoBehaviour>();
            return TryApplyDamage(parentBehaviours, damagedTargets);
        }

        private bool TryApplyDamage(MonoBehaviour[] behaviours, HashSet<IDamageable> damagedTargets)
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not IDamageable damageable)
                {
                    continue;
                }

                if (damageable.Team == CombatTeam.Player || damageable.Team == CombatTeam.Neutral || damagedTargets.Contains(damageable))
                {
                    continue;
                }

                damagedTargets.Add(damageable);
                damageable.ApplyDamage(GetDamage());
                return true;
            }

            return false;
        }

        private void SpawnHitEffect(Vector3 position, Vector2 aimDirection)
        {
            if (hitEffectPrefab == null)
            {
                return;
            }

            TransientVisualEffect effect = Instantiate(hitEffectPrefab, position + (Vector3)(aimDirection * 0.1f), Quaternion.identity);
            effect.Play(new Color(1f, 0.88f, 0.42f, 1f), 0.1f, 0.45f);
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 aimDirection = Application.isPlaying ? GetAimDirection() : Vector2.right;
            Gizmos.color = new Color(1f, 0.25f, 0.2f, 0.45f);
            Gizmos.DrawWireSphere(GetAttackCenter(aimDirection), meleeRadius);
        }
    }
}
