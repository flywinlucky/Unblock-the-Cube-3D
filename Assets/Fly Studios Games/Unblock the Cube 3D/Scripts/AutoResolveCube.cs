using UnityEngine;
using System.Collections;

public class AutoResolveCube : MonoBehaviour
{
    [Tooltip("Intervalul de timp (în secunde) între mișcările fiecărui bloc.")]
    public float moveInterval = 1.0f;

    private bool _isResolving = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartAutoResolve();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StopAutoResolve();
        }
    }

    /// <summary>
    /// Pornește procesul de rezolvare automată a cubului.
    /// </summary>
    public void StartAutoResolve()
    {
        if (!_isResolving)
        {
            StartCoroutine(ResolveCube());
        }
    }

    /// <summary>
    /// Oprește procesul de rezolvare automată.
    /// </summary>
    public void StopAutoResolve()
    {
        StopAllCoroutines();
        _isResolving = false;
    }

    private IEnumerator ResolveCube()
    {
        _isResolving = true;

        while (transform.childCount > 0) // Continuăm până când nu mai există copii
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Block block = child.GetComponent<Block>();

                if (block != null)
                {
                    // Simulăm un click pe bloc pentru a forța mișcarea
                    block.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver);
                }

                // Așteptăm intervalul specificat înainte de a trece la următorul bloc
                yield return new WaitForSeconds(moveInterval);
            }
        }

        _isResolving = false;
        Debug.Log("Auto-resolve stopped: no more blocks.");
    }
}
