using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Steps")]
    public List<GameObject> tutorialSteps; // Listă de pași pentru flexibilitate
    public Transform levelTarget;
    public int ignoreIndex; // Indexul copilului care va fi sărit

    public CameraControler cameraControler;

    private const string TutorialStateKey = "TutorialState"; // Cheie pentru salvarea progresului în PlayerPrefs
    private int _currentStep = 0;

    void Start()
    {
        // Citim progresul din PlayerPrefs
        _currentStep = PlayerPrefs.GetInt(TutorialStateKey, 0);

        // Activăm pasul corespunzător
        ActivateStep(_currentStep);

        // Conectăm evenimentul OnBlockActivated
        Block.OnBlockActivated += OnBlockActivated;
    }

    private void InitializeCamera()
    {
        cameraControler.rotationEnabled = false;
    }

    private void OnDestroy()
    {
        // Deconectăm evenimentul pentru a evita erorile
        Block.OnBlockActivated -= OnBlockActivated;
    }

    private void ActivateStep(int step)
    {
        // Dezactivăm toți pașii
        foreach (var stepObject in tutorialSteps)
        {
            if (stepObject != null) stepObject.SetActive(false);
        }

        // Activăm pasul curent
        if (step >= 0 && step < tutorialSteps.Count && tutorialSteps[step] != null)
        {
            tutorialSteps[step].SetActive(true);
            Debug.Log($"Tutorial step {step} activated.");

            // NOU: Apelăm ApplyDisableExcept cu un delay de 1 secundă dacă este pasul 1
            if (step == 0)
            {
                InitializeCamera();
                StartCoroutine(DelayedApplyDisableExcept());
            }
        }
        else
        {
            Debug.LogWarning($"Tutorial step {step} is out of range or null.");
        }
    }

    // NOU: Corutină pentru delay înainte de ApplyDisableExcept
    private IEnumerator DelayedApplyDisableExcept()
    {
        yield return new WaitForSeconds(0.1f);
        ApplyDisableExcept();
    }

    public void CompleteCurrentStep()
    {
        // Marcăm pasul curent ca finalizat și trecem la următorul
        _currentStep++;
        PlayerPrefs.SetInt(TutorialStateKey, _currentStep);
        PlayerPrefs.Save();

        // Activăm următorul pas
        ActivateStep(_currentStep);
    }

    public void ResetTutorial()
    {
        // Resetăm progresul tutorialului
        _currentStep = 0;
        PlayerPrefs.SetInt(TutorialStateKey, _currentStep);
        PlayerPrefs.Save();

        // Activăm primul pas
        ActivateStep(_currentStep);
        Debug.Log("Tutorial has been reset.");
    }

    // Dezactivează interactivitatea pentru toate Block-urile din levelTarget,
    // cu excepția copilului cu index == ignore (dacă este valid).
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

            if (i == ignoreIndex)
            {
                block._isInteractible = true;
                Debug.Log($"Child at index {i} (ignored) - _isInteractible set to TRUE.");
            }
            else
            {
                block._isInteractible = false;
                Debug.Log($"Child at index {i} - _isInteractible set to FALSE.");
            }
        }
    }

    private void OnBlockActivated(Block block)
    {
        // Verificăm dacă blocul activat este cel ignorat
        int childIndex = block.transform.GetSiblingIndex();
        if (childIndex == ignoreIndex)
        {
            Debug.Log($"Block at index {ignoreIndex} activated. Moving to next tutorial step.");
            CompleteCurrentStep();

            // NOU: Activăm toți copiii după ce blocul ignorat este activat
            EnableAllBlocks();
        }
    }

    // NOU: Activează interactivitatea pentru toate Block-urile din levelTarget
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
            Debug.Log($"Child at index {i} - _isInteractible set to TRUE.");
        }
    }
}