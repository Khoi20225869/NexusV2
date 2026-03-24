using UnityEngine;

namespace SoulForge.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool lockCursorOnPlay;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;

            if (!lockCursorOnPlay)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
