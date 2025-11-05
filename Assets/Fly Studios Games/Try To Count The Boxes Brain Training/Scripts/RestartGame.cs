using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    public Button restartButton;
    
    // Start is called before the first frame update
    void Start()
    {
        // --- ADAUGAT: Verificam daca butonul a fost asignat in Inspector
        if (restartButton != null)
        {
            // Adaugam un "listener" (ascultator) pentru evenimentul de click
            // Cand se da click pe buton, se va apela functia "RestartCurrentScene"
            restartButton.onClick.AddListener(RestartCurrentScene);
        }
        else
        {
            Debug.LogError("Butonul de restart nu a fost asignat in Inspector!");
        }
    }

    // --- ADAUGAT: Functia publica care va fi chemata la click
    public void RestartCurrentScene()
    {
        // Ia index-ul scenei curente si o reincarca
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}