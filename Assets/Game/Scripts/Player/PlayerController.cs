using SoulForge.Data;
using SoulForge.UI;
using UnityEngine;

namespace SoulForge.Player
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private HeroDefinition heroDefinition;
        [SerializeField] private SpumCharacterView characterView;

        public Vector2 MoveInput { get; private set; }

        private float MoveSpeed => heroDefinition != null ? heroDefinition.MoveSpeed : 5f;

        private void Update()
        {
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (MoveInput.sqrMagnitude > 1f)
            {
                MoveInput = MoveInput.normalized;
            }

            transform.position += (Vector3)(MoveInput * MoveSpeed * Time.deltaTime);
            characterView?.SetMoving(MoveInput.sqrMagnitude > 0.001f);

            if (Mathf.Abs(MoveInput.x) > 0.001f)
            {
                characterView?.SetFacing(MoveInput.x);
            }
        }

        private void Awake()
        {
            if (characterView == null)
            {
                characterView = GetComponent<SpumCharacterView>();
            }
        }

        public void ApplyHero(HeroDefinition newHeroDefinition)
        {
            heroDefinition = newHeroDefinition;
        }
    }
}
