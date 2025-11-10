using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    public class Rotator : MonoBehaviour
    {
        public float rotationSpeed = 5f; // Viteza de rotație

        // Update is called once per frame
        void Update()
        {
            // Rotim transformul pe axa -z
            transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
    }

}