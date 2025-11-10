using UnityEngine;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Simple tutorial step that shows a screen with a Continue button and persists the shown state.
    /// </summary>
    public class ContinueGameTutorialStep : MonoBehaviour
    {
        #region Inspector

        [Header("Persistence")]
        [Tooltip("Unique PlayerPrefs key to store whether this tutorial has been shown.")]
        public string tutorialKey;

        [Header("UI References")]
        [Tooltip("Continue button that closes the tutorial.")]
        public Button buttonContinue;

        [Tooltip("Root GameObject for the tutorial UI that will be shown/hidden.")]
        public GameObject tutorialStepUI;

        #endregion

        private bool _hasBeenShown;

        private void Start()
        {
            // Check if already shown
            _hasBeenShown = PlayerPrefs.GetInt(tutorialKey, 0) == 1;

            if (_hasBeenShown)
            {
                if (tutorialStepUI != null) tutorialStepUI.SetActive(false);
                return;
            }

            // Bind Continue button
            if (buttonContinue != null)
            {
                buttonContinue.onClick.RemoveAllListeners();
                buttonContinue.onClick.AddListener(OnContinueButtonClicked);
            }

            // Show UI if button is active
            if (buttonContinue != null && buttonContinue.gameObject.activeSelf)
            {
                if (tutorialStepUI != null) tutorialStepUI.SetActive(true);
            }
        }

        private void OnContinueButtonClicked()
        {
            // Persist shown state
            PlayerPrefs.SetInt(tutorialKey, 1);
            PlayerPrefs.Save();

            // Hide tutorial UI
            if (tutorialStepUI != null)
            {
                tutorialStepUI.SetActive(false);
            }

            Debug.Log("Tutorial completed and hidden.");
        }
    }
}