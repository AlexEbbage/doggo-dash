using Game.Application.Ports;
using Game.Infrastructure.Persistence;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class PlayPageControllerBehaviour : MonoBehaviour
    {
        [Header("Selection")]
        [SerializeField] private TMP_Text selectedPetText;
        [SerializeField] private TMP_Text selectedOutfitText;

        [Header("Challenges")]
        [SerializeField] private TMP_Text challengeSummaryText;

        private MetaProgressService _progress = default!;

        private void Awake()
        {
            _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
            Refresh();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_progress == null)
            {
                _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
            }

            _progress.Reload();
            PlayerProgressData data = _progress.Data;

            if (selectedPetText != null)
            {
                selectedPetText.text = MetaProgressTextFormatter.BuildSelectionLabel("Selected pet", data.selectedPetId);
            }

            if (selectedOutfitText != null)
            {
                selectedOutfitText.text = MetaProgressTextFormatter.BuildSelectionLabel("Selected outfit", data.selectedOutfitId);
            }

            if (challengeSummaryText != null)
            {
                challengeSummaryText.text = MetaProgressTextFormatter.BuildChallengeSummary(data);
            }
        }
    }
}
