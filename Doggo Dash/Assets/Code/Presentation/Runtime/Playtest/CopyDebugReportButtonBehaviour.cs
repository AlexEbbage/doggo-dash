using UnityEngine;
using TMPro;

namespace Game.Presentation.Runtime.Playtest
{
    public sealed class CopyDebugReportButtonBehaviour : MonoBehaviour
    {
        public ProgressResetServiceBehaviour progress = default!;
        public MonoBehaviour cloudStatusSourceBehaviour; // ICloudSaveStatusSource (optional)

        public TMP_Text feedbackText;
        public float feedbackSeconds = 2f;

        private ICloudSaveStatusSource _cloud;
        private float _t;
        private bool _showing;

        private void Awake()
        {
            _cloud = cloudStatusSourceBehaviour as ICloudSaveStatusSource;
        }

        public void CopyToClipboard()
        {
            if (progress == null) return;

            var data = progress.Load();

            string extra = null;
            if (_cloud != null)
            {
                extra =
                    $"CloudProvider: {_cloud.ProviderName}\n" +
                    $"CloudSlot: {_cloud.SlotKey}\n" +
                    $"CloudLastStatus: {_cloud.LastStatus}\n" +
                    $"CloudLastSyncUtc: {_cloud.LastSyncUnixUtc}";
            }

            string report = DebugReportBuilder.Build(data, extra);
            GUIUtility.systemCopyBuffer = report;

            if (feedbackText != null)
            {
                feedbackText.text = "Copied debug report ✅";
                _t = 0f;
                _showing = true;
            }

            Debug.Log("[Playtest] Debug report copied to clipboard.");
        }

        private void Update()
        {
            if (!_showing || feedbackText == null) return;
            _t += Time.unscaledDeltaTime;
            if (_t >= feedbackSeconds)
            {
                _showing = false;
                feedbackText.text = "";
            }
        }
    }
}
