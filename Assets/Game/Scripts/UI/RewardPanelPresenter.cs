using SoulForge.Data;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class RewardPanelPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RewardDefinition previewReward;

        private void Start()
        {
            SetVisible(false);
        }

        public void Show(RewardDefinition reward)
        {
            previewReward = reward;
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool isVisible)
        {
            if (root != null)
            {
                root.SetActive(isVisible);
                return;
            }

            gameObject.SetActive(isVisible);
        }
    }
}
