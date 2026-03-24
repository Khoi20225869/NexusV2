using UnityEngine;

namespace SoulForge.Hub
{
    public sealed class HubNpcBouncer : MonoBehaviour
    {
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobFrequency = 1.6f;

        private Vector3 startPosition;

        private void Awake()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = startPosition + new Vector3(0f, offset, 0f);
        }
    }
}
