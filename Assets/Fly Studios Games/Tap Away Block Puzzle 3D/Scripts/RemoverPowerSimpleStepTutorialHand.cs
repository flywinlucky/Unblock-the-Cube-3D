using System.Collections;
using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Simple one-step tutorial hand for the remover power.
    /// Call ShowHand() to display the tutorial if it has not been shown before.
    /// Uses PlayerPrefs to persist "shown" state.
    /// </summary>
    public class RemoverPowerSimpleStepTutorialHand : MonoBehaviour
    {
        [Header("Tutorial Settings")]
        [Tooltip("Unique PlayerPrefs key used to save whether this tutorial was shown.")]
        public string tutorialKey = "RemoverPowerTutorialShown";

        [Tooltip("Root GameObject containing the tutorial UI to enable/disable.")]
        public GameObject tutorialStepUI;

        private bool _hasBeenShown;

        /// <summary>
        /// Show the tutorial UI if it hasn't been shown before.
        /// </summary>
        public void ShowHand()
        {
            if (string.IsNullOrEmpty(tutorialKey))
            {
                tutorialKey = "RemoverPowerTutorialShown";
            }

            // Read saved state
            _hasBeenShown = PlayerPrefs.GetInt(tutorialKey, 0) == 1;

            if (_hasBeenShown)
            {
                if (tutorialStepUI != null)
                    tutorialStepUI.SetActive(false);
                return;
            }

            StartCoroutine(ActiveSimpleHandTutorialDelay());
        }

        private IEnumerator ActiveSimpleHandTutorialDelay()
        {
            if (tutorialStepUI != null) tutorialStepUI.SetActive(true);

            yield return new WaitForSeconds(4f);

            OnContinueButtonClicked();
        }

        private void OnContinueButtonClicked()
        {
            // Save shown state
            PlayerPrefs.SetInt(tutorialKey, 1);
            PlayerPrefs.Save();

            if (tutorialStepUI != null) tutorialStepUI.SetActive(false);

            Debug.Log("Tutorial completed and hidden.");
        }
    }
}
