using SoulForge.Combat;
using SoulForge.Data;
using System;
using SoulForge.UI;
using UnityEngine;
using UnityEngine.Events;
using SoulForge.CameraSystem;
using SoulForge.Feedback;

namespace SoulForge.Player
{
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private HeroDefinition heroDefinition;
        [SerializeField] private SpumCharacterView characterView;
        [SerializeField] private SpriteFlashFeedback flashFeedback;
        [SerializeField] private CameraFeedback cameraFeedback;
        [SerializeField] private GameFeelController gameFeelController;
        [SerializeField] private UnityEvent onDeath;

        private float currentHealth;

        public event Action Died;

        public CombatTeam Team => CombatTeam.Player;
        public Transform Transform => transform;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => heroDefinition != null ? heroDefinition.MaxHealth : 6f;
        public float MaxShield => heroDefinition != null ? heroDefinition.MaxShield : 4f;

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

            if (cameraFeedback == null)
            {
                cameraFeedback = FindFirstObjectByType<CameraFeedback>();
            }

            if (gameFeelController == null)
            {
                gameFeelController = FindFirstObjectByType<GameFeelController>();
            }

            currentHealth = heroDefinition != null ? heroDefinition.MaxHealth : 6f;
        }

        public void ApplyHero(HeroDefinition newHeroDefinition, bool restoreFullHealth)
        {
            heroDefinition = newHeroDefinition;

            if (restoreFullHealth)
            {
                currentHealth = MaxHealth;
                characterView?.ResetState();
                gameObject.SetActive(true);
            }
        }

        public void ApplyDamage(float damage)
        {
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            flashFeedback?.Flash();
            cameraFeedback?.PlayHitShake(Mathf.Max(0.8f, damage));
            gameFeelController?.PlayPlayerHit(damage);

            if (currentHealth > 0f)
            {
                return;
            }

            onDeath?.Invoke();
            characterView?.PlayDeath();
            Died?.Invoke();
            gameObject.SetActive(false);
        }

        public void Restore(float amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, MaxHealth);
        }
    }
}
