using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    public class RemoveAllPlayerPrefs : MonoBehaviour
    {
        public void RemoveAllPlayerPrefsData()
        {
            PlayerPrefs.DeleteAll();
        }
    }

}