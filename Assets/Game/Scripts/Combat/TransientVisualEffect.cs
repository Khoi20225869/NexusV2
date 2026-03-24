using UnityEngine;

namespace SoulForge.Combat
{
    public sealed class TransientVisualEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1.1f);
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private float duration;
        private float age;
        private Color baseColor = Color.white;
        private Vector3 startScale = Vector3.one;

        public void Play(Color color, float lifeTime, float startSize)
        {
            duration = Mathf.Max(0.01f, lifeTime);
            baseColor = color;
            startScale = Vector3.one * startSize;
            transform.localScale = startScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            transform.localScale = startScale * Mathf.Max(0.01f, scaleCurve.Evaluate(t));

            if (spriteRenderer != null)
            {
                Color color = baseColor;
                color.a = alphaCurve.Evaluate(t);
                spriteRenderer.color = color;
            }

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
