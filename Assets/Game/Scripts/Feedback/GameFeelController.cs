using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.Feedback
{
    public sealed class GameFeelController : MonoBehaviour
    {
        [SerializeField] private Image screenFlashImage;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float playerShotHitstop = 0.015f;
        [SerializeField] private float playerHitHitstop = 0.04f;
        [SerializeField] private Color playerHitFlashColor = new(1f, 0.18f, 0.18f, 0.22f);

        private float flashTimeRemaining;
        private float flashDuration;
        private Color flashColor;
        private float hitstopTimer;
        private float restoreFixedDeltaTime;
        private AudioClip playerShotClip;
        private AudioClip playerHitClip;
        private AudioClip impactClip;

        private void Awake()
        {
            restoreFixedDeltaTime = Time.fixedDeltaTime;

            if (audioSource == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                audioSource.volume = 0.18f;
            }

            if (screenFlashImage != null)
            {
                screenFlashImage.color = Color.clear;
                screenFlashImage.raycastTarget = false;
            }

            playerShotClip = CreateToneClip("shot", 920f, 0.05f, 0.12f);
            playerHitClip = CreateToneClip("hurt", 180f, 0.09f, 0.18f);
            impactClip = CreateToneClip("impact", 420f, 0.04f, 0.09f);
        }

        private void Update()
        {
            UpdateFlash();
            UpdateHitstop();
        }

        public void PlayPlayerShot()
        {
            PlayOneShot(playerShotClip, 1f);
        }

        public void PlayPlayerHit(float intensity)
        {
            float normalized = Mathf.Clamp01(intensity / 3f);
            Flash(playerHitFlashColor * new Color(1f, 1f, 1f, Mathf.Lerp(0.75f, 1.35f, normalized)), 0.14f);
            PlayHitstop(playerHitHitstop * Mathf.Lerp(0.75f, 1.2f, normalized));
            PlayOneShot(playerHitClip, Mathf.Lerp(0.9f, 1.1f, normalized));
        }

        public void PlayImpact(bool fromPlayerShot, float intensity)
        {
            if (fromPlayerShot)
            {
                PlayHitstop(playerShotHitstop * Mathf.Clamp(intensity, 0.75f, 1.6f));
            }

            PlayOneShot(impactClip, Mathf.Lerp(0.92f, 1.08f, Random.value));
        }

        public void Flash(Color color, float duration)
        {
            flashColor = color;
            flashDuration = Mathf.Max(0.01f, duration);
            flashTimeRemaining = flashDuration;

            if (screenFlashImage != null)
            {
                screenFlashImage.color = color;
            }
        }

        public void PlayHitstop(float duration)
        {
            hitstopTimer = Mathf.Max(hitstopTimer, Mathf.Max(0f, duration));
            if (hitstopTimer <= 0f)
            {
                return;
            }

            Time.timeScale = 0.01f;
            Time.fixedDeltaTime = restoreFixedDeltaTime * Time.timeScale;
        }

        private void UpdateFlash()
        {
            if (screenFlashImage == null)
            {
                return;
            }

            if (flashTimeRemaining <= 0f)
            {
                if (screenFlashImage.color.a > 0.001f)
                {
                    screenFlashImage.color = Color.Lerp(screenFlashImage.color, Color.clear, 18f * Time.unscaledDeltaTime);
                }

                return;
            }

            flashTimeRemaining -= Time.unscaledDeltaTime;
            float t = flashDuration > 0f ? Mathf.Clamp01(flashTimeRemaining / flashDuration) : 0f;
            Color color = flashColor;
            color.a *= t;
            screenFlashImage.color = color;
        }

        private void UpdateHitstop()
        {
            if (hitstopTimer <= 0f)
            {
                if (Time.timeScale < 0.999f)
                {
                    Time.timeScale = 1f;
                    Time.fixedDeltaTime = restoreFixedDeltaTime;
                }

                return;
            }

            hitstopTimer -= Time.unscaledDeltaTime;
            if (hitstopTimer <= 0f)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = restoreFixedDeltaTime;
            }
        }

        private void PlayOneShot(AudioClip clip, float pitch)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }

        private static AudioClip CreateToneClip(string clipName, float frequency, float duration, float amplitude)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.CeilToInt(sampleRate * Mathf.Max(0.01f, duration));
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float envelope = 1f - (i / (float)sampleCount);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * amplitude * envelope;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
