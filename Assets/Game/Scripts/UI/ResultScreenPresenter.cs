using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ResultScreenPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;

        public void Show()
        {
            SetVisible(true);
        }

        public void ShowVictory()
        {
            if (titleText != null)
            {
                titleText.text = "Run Complete";
            }

            if (summaryText != null)
            {
                summaryText.text = "Host escaped the run.";
            }

            SetVisible(true);
        }

        public void ShowDefeat()
        {
            if (titleText != null)
            {
                titleText.text = "Run Failed";
            }

            if (summaryText != null)
            {
                summaryText.text = "Host was defeated.";
            }

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
