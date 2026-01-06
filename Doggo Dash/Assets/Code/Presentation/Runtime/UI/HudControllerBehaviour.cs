using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.UI
{
    public sealed class HudControllerBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public EnergySpeedControllerBehaviour energy = default!;
        public RunRewardTrackerBehaviour rewards = default!;
        public ScoreDistanceControllerBehaviour scoreDistance = default!;

        [Header("UI")]
        public Image energyFill = default!;
        public TMP_Text kibbleText = default!;
        public TMP_Text gemsText = default!;

        [Header("Optional UI")]
        public TMP_Text distanceText;
        public TMP_Text scoreText;

        [Header("Formatting")]
        public string kibblePrefix = "";
        public string gemsPrefix = "";
        public string distanceSuffix = "m";

        private void Update()
        {
            if (energy != null && energyFill != null)
                energyFill.fillAmount = Mathf.Clamp01(energy.Energy01);

            if (rewards != null)
            {
                if (kibbleText != null) kibbleText.text = $"{kibblePrefix}{rewards.Kibble}";
                if (gemsText != null) gemsText.text = $"{gemsPrefix}{rewards.Gems}";
            }

            if (scoreDistance != null)
            {
                if (distanceText != null) distanceText.text = $"{Mathf.FloorToInt(scoreDistance.DistanceMeters)}{distanceSuffix}";
                if (scoreText != null) scoreText.text = $"{scoreDistance.Score}";
            }
        }
    }
}
