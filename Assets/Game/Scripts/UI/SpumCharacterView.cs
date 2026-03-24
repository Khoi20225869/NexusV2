using UnityEngine;
using SoulForge.Data;

namespace SoulForge.UI
{
    public sealed class SpumCharacterView : MonoBehaviour
    {
        [SerializeField] private SPUM_Prefabs spumPrefab;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private int attackAnimationId = 4;

        private bool isDead;
        private bool isMoving;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            ResetState();
        }

        public void SetFacing(float directionX)
        {
            EnsureReferences();

            if (visualRoot == null || Mathf.Approximately(directionX, 0f))
            {
                return;
            }

            Vector3 localScale = visualRoot.localScale;
            float scaleX = Mathf.Abs(localScale.x);
            localScale.x = directionX >= 0f ? -scaleX : scaleX;
            visualRoot.localScale = localScale;
        }

        public void SetMoving(bool isMoving)
        {
            EnsureReferences();

            if (isDead || spumPrefab == null || this.isMoving == isMoving)
            {
                return;
            }

            this.isMoving = isMoving;
            spumPrefab.PlayAnimation(isMoving ? 1 : 0);
        }

        public void PlayAttack()
        {
            EnsureReferences();

            if (isDead || spumPrefab == null)
            {
                return;
            }

            isMoving = false;
            spumPrefab.PlayAnimation(attackAnimationId);
        }

        public void PlayDeath()
        {
            EnsureReferences();

            if (isDead)
            {
                return;
            }

            isDead = true;

            if (spumPrefab == null)
            {
                return;
            }

            spumPrefab.PlayAnimation(2);
        }

        public void ResetState()
        {
            EnsureReferences();

            isDead = false;
            isMoving = false;

            if (spumPrefab != null)
            {
                spumPrefab.PlayAnimation(0);
            }
        }

        public void ApplyHeroStyle(HeroDefinition heroDefinition)
        {
            EnsureReferences();

            if (heroDefinition == null)
            {
                return;
            }

            attackAnimationId = heroDefinition.AttackAnimationId;

            if (spumPrefab == null || spumPrefab._spriteOBj == null)
            {
                return;
            }

            if (heroDefinition.CharacterPrefab != null)
            {
                SPUM_Prefabs template = heroDefinition.CharacterPrefab.GetComponent<SPUM_Prefabs>();
                if (template != null && template._spriteOBj != null)
                {
                    spumPrefab._spriteOBj.LoadSprite(template._spriteOBj);
                }
            }

            SetRendererColor(spumPrefab._spriteOBj._eyeList, heroDefinition.EyeColor);
            SetRendererColor(spumPrefab._spriteOBj._hairList, heroDefinition.HairColor, 0, 3);
        }

        public void ApplyAppearance(SoulForge.Bootstrap.HeroAppearanceSelection appearanceSelection)
        {
            EnsureReferences();

            if (appearanceSelection == null || spumPrefab == null || spumPrefab._spriteOBj == null)
            {
                return;
            }

            SPUM_SpriteList spriteList = spumPrefab._spriteOBj;

            Sprite hairSprite = SpumAppearanceCatalog.LoadSprite(SpumAppearanceCatalog.HairStyles[SpumAppearanceCatalog.Wrap(appearanceSelection.HairStyleIndex, SpumAppearanceCatalog.HairStyles.Length)]);
            Sprite helmetSprite = SpumAppearanceCatalog.LoadSprite(SpumAppearanceCatalog.Helmets[SpumAppearanceCatalog.Wrap(appearanceSelection.HelmetIndex, SpumAppearanceCatalog.Helmets.Length)]);

            SetSprite(spriteList._hairList, 0, helmetSprite == null ? hairSprite : null);
            SetSprite(spriteList._hairList, 3, SpumAppearanceCatalog.LoadSprite(SpumAppearanceCatalog.FaceHairs[SpumAppearanceCatalog.Wrap(appearanceSelection.FaceHairIndex, SpumAppearanceCatalog.FaceHairs.Length)]));
            SetSprite(spriteList._hairList, 1, helmetSprite);
            SetWeaponSprite(spriteList._weaponList, SpumAppearanceCatalog.LoadSprite(SpumAppearanceCatalog.Weapons[SpumAppearanceCatalog.Wrap(appearanceSelection.WeaponIndex, SpumAppearanceCatalog.Weapons.Length)]));
            SetSprite(spriteList._backList, 0, SpumAppearanceCatalog.LoadSprite(SpumAppearanceCatalog.BackItems[SpumAppearanceCatalog.Wrap(appearanceSelection.BackIndex, SpumAppearanceCatalog.BackItems.Length)]));
            SetArmorSprites(spriteList._armorList, SpumAppearanceCatalog.Armors[SpumAppearanceCatalog.Wrap(appearanceSelection.ArmorIndex, SpumAppearanceCatalog.Armors.Length)]);
            SetPantSprites(spriteList._pantList, SpumAppearanceCatalog.Pants[SpumAppearanceCatalog.Wrap(appearanceSelection.PantIndex, SpumAppearanceCatalog.Pants.Length)]);

            SetRendererColor(spriteList._eyeList, SpumAppearanceCatalog.EyeColors[SpumAppearanceCatalog.Wrap(appearanceSelection.EyeColorIndex, SpumAppearanceCatalog.EyeColors.Length)]);
            SetRendererColor(spriteList._hairList, SpumAppearanceCatalog.HairColors[SpumAppearanceCatalog.Wrap(appearanceSelection.HairColorIndex, SpumAppearanceCatalog.HairColors.Length)], 0, 3);
            SetRendererColor(spriteList._clothList, SpumAppearanceCatalog.ClothColors[SpumAppearanceCatalog.Wrap(appearanceSelection.ClothColorIndex, SpumAppearanceCatalog.ClothColors.Length)]);
            SetRendererColor(spriteList._armorList, SpumAppearanceCatalog.ClothColors[SpumAppearanceCatalog.Wrap(appearanceSelection.ClothColorIndex, SpumAppearanceCatalog.ClothColors.Length)]);
        }

        private void EnsureReferences()
        {
            if (spumPrefab == null)
            {
                spumPrefab = GetComponentInChildren<SPUM_Prefabs>();
            }

            if (visualRoot == null)
            {
                visualRoot = spumPrefab != null ? spumPrefab.transform : transform;
            }
        }

        private static void SetRendererColor(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, Color color, params int[] indexes)
        {
            if (renderers == null)
            {
                return;
            }

            if (indexes == null || indexes.Length == 0)
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    if (renderers[i] != null)
                    {
                        renderers[i].color = color;
                    }
                }

                return;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (index >= 0 && index < renderers.Count && renderers[index] != null)
                {
                    renderers[index].color = color;
                }
            }
        }

        private static void SetSprite(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, int index, Sprite sprite)
        {
            if (renderers == null || index < 0 || index >= renderers.Count || renderers[index] == null)
            {
                return;
            }

            renderers[index].sprite = sprite;
            renderers[index].gameObject.SetActive(sprite != null);
        }

        private static void SetArmorSprites(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, string resourcePath)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sprite = null;
                    renderers[i].gameObject.SetActive(false);
                }
            }

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return;
            }

            Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null)
                {
                    continue;
                }

                switch (sprite.name)
                {
                    case "Body":
                        SetSprite(renderers, 0, sprite);
                        break;
                    case "Left":
                        SetSprite(renderers, 1, sprite);
                        break;
                    case "Right":
                        SetSprite(renderers, 2, sprite);
                        break;
                }
            }
        }

        private static void SetPantSprites(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, string resourcePath)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sprite = null;
                    renderers[i].gameObject.SetActive(false);
                }
            }

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return;
            }

            Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null)
                {
                    continue;
                }

                switch (sprite.name)
                {
                    case "Left":
                        SetSprite(renderers, 0, sprite);
                        break;
                    case "Right":
                        SetSprite(renderers, 1, sprite);
                        break;
                }
            }
        }

        private static void SetWeaponSprite(System.Collections.Generic.IReadOnlyList<SpriteRenderer> renderers, Sprite sprite)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sprite = null;
                    renderers[i].gameObject.SetActive(false);
                }
            }

            if (sprite == null)
            {
                return;
            }

            SetSprite(renderers, 0, sprite);
        }
    }
}
