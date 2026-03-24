using TMPro;
using UnityEngine;

namespace SoulForge.UI
{
    public sealed class ViewerControlsPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text controlsText;

        private void Start()
        {
            if (controlsText == null)
            {
                return;
            }

            controlsText.text =
                "Viewer Test Controls\n" +
                "0 = +Currency\n" +
                "1 = Spawn Weak Enemy\n" +
                "2 = Spawn Elite Enemy\n" +
                "3 = Drop Heal\n" +
                "4 = Drop Weapon";
        }
    }
}
