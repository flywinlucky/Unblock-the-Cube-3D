using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Manages tutorial steps. Steps are GameObjects (UI or indicators) that get activated in order.
    /// Subscribes to Block activation and object rotation events to advance the tutorial.
    /// Functionality preserved; refactored for clarity and inspector UX.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        #region Inspector

        [Header("Tutorial Steps")]
        [Tooltip("List of tutorial step GameObjects. Steps will be activated in order.")]
        public List<GameObject> tutorialSteps; // flexible list of steps

        [Tooltip("Root transform containing the level blocks that tutorial will manipulate.")]
        public Transform levelTarget;

        [Tooltip("Index of the child to keep enabled during the current step (skipped/interactive).")]
        public int ignoreIndex; // child index to keep active

        [Tooltip("Reference to the CameraController used in some tutorial steps.")]
        public CameraControler cameraControler;

        #endregion

        #region Persistence

        private const string TutorialStateKey = "TutorialState"; // PlayerPrefs key for tutorial progress
        private int _currentStep = 0;

        #endregion

        #region Unity Events

        private void Start()
        {
            // Load tutorial progress
            _currentStep = PlayerPrefs.GetInt(TutorialStateKey, 0);

            // Activate the saved/current step
            ActivateStep(_currentStep);

            // Subscribe to block activation events
            Block.OnBlockActivated += OnBlockActivated;

            // Subscribe to rotation event from CameraControler
            CameraControler.OnObjectRotated += OnObjectRotated;
        }

        private void OnDestroy()
        {
            // Unsubscribe to avoid dangling listeners
            Block.OnBlockActivated -= OnBlockActivated;
            CameraControler.OnObjectRotated -= OnObjectRotated;
        }

        #endregion

        #region Tutorial Flow

        private void ActivateStep(int step)
        {
            // Deactivate all steps first
            foreach (var stepObject in tutorialSteps)
            {
                if (stepObject != null) stepObject.SetActive(false);
            }

            // Activate the requested step
            if (step >= 0 && step < tutorialSteps.Count && tutorialSteps[step] != null)
            {
                tutorialSteps[step].SetActive(true);
                Debug.Log($"Tutorial step {step} activated.");

                if (step == 0)
                {
                    StartCoroutine(DelayedApplyDisableExcept());
                }
                else if (step == 1)
                {
                    if (cameraControler != null)
                    {
                        cameraControler.rotationEnabled = true;
                    }
                }
                else if (step == 2)
                {
                    if (cameraControler != null)
                    {
                        cameraControler.ResetRotationFlag();
                    }
                }
            }
        }

        /// <summary>
        /// Coroutine used to delay a small amount before disabling other blocks (gives frame to initialize).
        /// </summary>
        private IEnumerator DelayedApplyDisableExcept()
        {
            yield return new WaitForSeconds(0.1f);
            if (cameraControler != null) cameraControler.rotationEnabled = false;
            ApplyDisableExcept();
        }

        private void CompleteCurrentStep()
        {
            // Mark step complete and advance
            _currentStep++;
            PlayerPrefs.SetInt(TutorialStateKey, _currentStep);
            PlayerPrefs.Save();

            if (_currentStep >= tutorialSteps.Count)
            {
                foreach (var stepObject in tutorialSteps)
                {
                    if (stepObject != null) stepObject.SetActive(false);
                }
                return;
            }

            ActivateStep(_currentStep);
        }

        /// <summary>
        /// Reset tutorial progress and activate the first step.
        /// </summary>
        public void ResetTutorial()
        {
            _currentStep = 0;
            PlayerPrefs.SetInt(TutorialStateKey, _currentStep);
            PlayerPrefs.Save();

            ActivateStep(_currentStep);
            Debug.Log("Tutorial has been reset.");
        }

        #endregion

        #region Block Interactivity Helpers

        /// <summary>
        /// Disable interactivity for all Block components under levelTarget except the child at ignoreIndex.
        /// </summary>
        public void ApplyDisableExcept()
        {
            if (levelTarget == null)
            {
                Debug.LogWarning("LevelTarget is null. Cannot process children.");
                return;
            }

            int childCount = levelTarget.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = levelTarget.GetChild(i);
                if (child == null)
                {
                    Debug.Log($"Child at index {i} is null.");
                    continue;
                }

                Block block = child.GetComponent<Block>();
                if (block == null) continue;

                block._isInteractible = (i == ignoreIndex);
            }
        }

        private void OnBlockActivated(Block block)
        {
            // If the activated block is the one we were waiting for, advance the tutorial
            int childIndex = block.transform.GetSiblingIndex();
            if (childIndex == ignoreIndex)
            {
                CompleteCurrentStep();

                // Re-enable all blocks after completing this step
                EnableAllBlocks();

                if (cameraControler != null) cameraControler.rotationEnabled = true;
            }
        }

        /// <summary>
        /// Enable interactivity for all Block components under levelTarget.
        /// </summary>
        private void EnableAllBlocks()
        {
            if (levelTarget == null)
            {
                Debug.LogWarning("LevelTarget is null. Cannot process children.");
                return;
            }

            int childCount = levelTarget.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = levelTarget.GetChild(i);
                if (child == null)
                {
                    Debug.Log($"Child at index {i} is null.");
                    continue;
                }

                Block block = child.GetComponent<Block>();
                if (block == null)
                {
                    Debug.Log($"Child at index {i} does not have a Block component.");
                    continue;
                }

                block._isInteractible = true;
            }
        }

        #endregion

        #region External Event Handlers

        /// <summary>
        /// Called when the object is rotated (CameraControler raises this).
        /// Advances tutorial from step 1 to step 2.
        /// </summary>
        private void OnObjectRotated()
        {
            if (_currentStep == 1)
            {
                Debug.Log("Object rotated slightly. Moving to tutorial step 2.");
                CompleteCurrentStep();
            }
        }

        #endregion
    }
}