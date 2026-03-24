using SoulForge.Combat;
using SoulForge.Data;
using SoulForge.UI;
using UnityEngine;
using SoulForge.CameraSystem;
using SoulForge.Feedback;

namespace SoulForge.Player
{
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private WeaponDefinition weaponDefinition;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpumCharacterView characterView;
        [SerializeField] private WeaponRuntime weaponRuntime;
        [SerializeField] private CameraFeedback cameraFeedback;
        [SerializeField] private TransientVisualEffect muzzleFlashPrefab;
        [SerializeField] private GameFeelController gameFeelController;

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
            if (projectilePrefab == null)
            {
                return;
            }

            UpdateVisualFacing(aimDirection);

            Transform spawnPoint = firePoint != null ? firePoint : transform;
            Projectile projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            projectile.Initialize(
                aimDirection,
                GetProjectileSpeed(),
                GetDamage(),
                CombatTeam.Player);

            fireCooldown = 1f / Mathf.Max(GetFireRate(), 0.01f);
            characterView?.PlayAttack();
            cameraFeedback?.PlayShotKick(aimDirection);
            gameFeelController?.PlayPlayerShot();
            SpawnMuzzleFlash(spawnPoint.position, aimDirection);
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

        private float GetProjectileSpeed()
        {
            if (weaponRuntime != null && weaponRuntime.WeaponDefinition != null)
            {
                return weaponRuntime.ProjectileSpeed;
            }

            return weaponDefinition != null ? weaponDefinition.ProjectileSpeed : 14f;
        }

        private void SpawnMuzzleFlash(Vector3 position, Vector2 aimDirection)
        {
            if (muzzleFlashPrefab == null)
            {
                return;
            }

            TransientVisualEffect effect = Instantiate(muzzleFlashPrefab, position + (Vector3)(aimDirection * 0.15f), Quaternion.identity);
            effect.Play(new Color(1f, 0.88f, 0.42f, 1f), 0.08f, 0.35f);
        }
    }
}
