using SoulForge.Bootstrap;
using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class RunProgressPresenter : MonoBehaviour
    {
        [SerializeField] private RunController runController;
        [SerializeField] private TMP_Text progressText;

        private void Awake()
        {
            if (runController == null)
            {
                runController = FindFirstObjectByType<RunController>();
            }
        }

        private void Update()
        {
            if (progressText == null || runController == null)
            {
                return;
            }

            progressText.text = $"Floor {runController.FloorIndex}/{runController.FloorCount}";
        }
    }
}
