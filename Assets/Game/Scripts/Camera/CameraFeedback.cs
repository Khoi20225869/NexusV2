using UnityEngine;

namespace SoulForge.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFeedback : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new(0f, 0f, -10f);
        [SerializeField] private float followSmoothTime = 0.12f;
        [SerializeField] private float shotKickDistance = 0.18f;
        [SerializeField] private float shotKickDuration = 0.08f;
        [SerializeField] private float hitShakeStrength = 0.22f;
        [SerializeField] private float hitShakeDuration = 0.14f;

        private Vector3 currentVelocity;
        private Vector3 shakeOffset;
        private float shakeTime;
        private float shakeStrength;
        private Vector3 kickOffset;
        private float kickTime;
        private float kickDuration;

        private void Awake()
        {
            if (target == null)
            {
                var playerHealth = FindFirstObjectByType<SoulForge.Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    target = playerHealth.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            UpdateKick();
            UpdateShake();

            Vector3 desired = target.position + followOffset + kickOffset + shakeOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref currentVelocity, followSmoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void PlayShotKick(Vector2 aimDirection)
        {
            Vector2 normalized = aimDirection.sqrMagnitude > 0.0001f ? aimDirection.normalized : Vector2.right;
            kickOffset = new Vector3(normalized.x, normalized.y, 0f) * shotKickDistance;
            kickDuration = shotKickDuration;
            kickTime = shotKickDuration;
        }

        public void PlayHitShake(float multiplier = 1f)
        {
            shakeTime = hitShakeDuration;
            shakeStrength = hitShakeStrength * Mathf.Max(0.2f, multiplier);
        }

        private void UpdateKick()
        {
            if (kickTime <= 0f)
            {
                kickOffset = Vector3.zero;
                return;
            }

            kickTime -= Time.deltaTime;
            float normalized = kickDuration > 0f ? Mathf.Clamp01(kickTime / kickDuration) : 0f;
            kickOffset *= normalized;
        }

        private void UpdateShake()
        {
            if (shakeTime <= 0f)
            {
                shakeOffset = Vector3.zero;
                return;
            }

            shakeTime -= Time.deltaTime;
            float strength = shakeStrength * Mathf.Clamp01(shakeTime / hitShakeDuration);
            shakeOffset = (Vector3)Random.insideUnitCircle * strength;
        }
    }
}
