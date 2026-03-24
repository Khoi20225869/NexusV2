using UnityEngine;

namespace SoulForge.UI
{
    public sealed class SpriteFlashFeedback : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField] private Color flashColor = new(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float flashDuration = 0.08f;

        private Color[] baseColors;
        private float flashTimer;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<SpriteRenderer>();
            }

            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                baseColors[i] = renderers[i] != null ? renderers[i].color : Color.white;
            }
        }

        private void Update()
        {
            if (flashTimer <= 0f)
            {
                return;
            }

            flashTimer -= Time.deltaTime;
            if (flashTimer > 0f)
            {
                return;
            }

            RestoreColors();
        }

        public void Flash()
        {
            if (renderers == null)
            {
                return;
            }

            flashTimer = flashDuration;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = flashColor;
                }
            }
        }

        private void RestoreColors()
        {
            if (renderers == null || baseColors == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = baseColors[i];
                }
            }
        }
    }
}
