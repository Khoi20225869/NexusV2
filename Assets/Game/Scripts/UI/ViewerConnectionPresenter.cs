using SoulForge.Viewer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulForge.UI
{
    public sealed class ViewerConnectionPresenter : MonoBehaviour
    {
        [SerializeField] private ViewerWebSocketClient viewerWebSocketClient;
        [SerializeField] private TMP_InputField hostIpInput;
        [SerializeField] private TMP_InputField sessionCodeInput;
        [SerializeField] private Button connectButton;
        [SerializeField] private TMP_Text statusText;

        private void Awake()
        {
            if (viewerWebSocketClient == null)
            {
                viewerWebSocketClient = FindFirstObjectByType<ViewerWebSocketClient>();
            }
        }

        private void OnEnable()
        {
            if (connectButton != null)
            {
                connectButton.onClick.AddListener(Connect);
            }

            if (hostIpInput != null)
            {
                hostIpInput.onEndEdit.AddListener(OnHostIpEdited);
            }

            if (sessionCodeInput != null)
            {
                sessionCodeInput.onEndEdit.AddListener(OnSessionCodeEdited);
            }

            if (viewerWebSocketClient != null)
            {
                viewerWebSocketClient.ConnectionStateChanged += OnConnectionStateChanged;
                viewerWebSocketClient.StatusChanged += OnStatusChanged;
            }

            RefreshUi();
        }

        private void OnDisable()
        {
            if (connectButton != null)
            {
                connectButton.onClick.RemoveListener(Connect);
            }

            if (hostIpInput != null)
            {
                hostIpInput.onEndEdit.RemoveListener(OnHostIpEdited);
            }

            if (sessionCodeInput != null)
            {
                sessionCodeInput.onEndEdit.RemoveListener(OnSessionCodeEdited);
            }

            if (viewerWebSocketClient != null)
            {
                viewerWebSocketClient.ConnectionStateChanged -= OnConnectionStateChanged;
                viewerWebSocketClient.StatusChanged -= OnStatusChanged;
            }
        }

        private void Connect()
        {
            if (viewerWebSocketClient == null)
            {
                return;
            }

            if (hostIpInput != null)
            {
                viewerWebSocketClient.SetHostIp(hostIpInput.text);
            }

            if (sessionCodeInput != null)
            {
                viewerWebSocketClient.SetSessionCode(sessionCodeInput.text);
            }

            viewerWebSocketClient.Connect();
            RefreshUi();
        }

        private void OnHostIpEdited(string value)
        {
            viewerWebSocketClient?.SetHostIp(value);
            RefreshUi();
        }

        private void OnSessionCodeEdited(string value)
        {
            viewerWebSocketClient?.SetSessionCode(value);
            RefreshUi();
        }

        private void OnConnectionStateChanged(bool _)
        {
            RefreshUi();
        }

        private void OnStatusChanged(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }

            RefreshUi();
        }

        private void RefreshUi()
        {
            if (viewerWebSocketClient == null)
            {
                return;
            }

            if (hostIpInput != null && string.IsNullOrWhiteSpace(hostIpInput.text))
            {
                hostIpInput.text = viewerWebSocketClient.HostIp;
            }

            if (sessionCodeInput != null && string.IsNullOrWhiteSpace(sessionCodeInput.text))
            {
                sessionCodeInput.text = viewerWebSocketClient.SessionCode;
            }

            bool isConnected = viewerWebSocketClient.IsConnected;
            if (connectButton != null)
            {
                connectButton.interactable = !isConnected;
            }

            if (statusText != null)
            {
                statusText.text = viewerWebSocketClient.LastStatus;
            }
        }
    }
}
