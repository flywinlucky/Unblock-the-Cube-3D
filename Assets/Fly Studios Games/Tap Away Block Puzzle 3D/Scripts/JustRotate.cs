using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
	/// <summary>
	/// Simple continuous rotation component for small decorative objects.
	/// </summary>
	public class JustRotate : MonoBehaviour
	{
		#region Inspector

		[Header("Rotation Settings")]
		[Tooltip("Enable or disable automatic rotation.")]
		public bool canRotate = true;

		[Tooltip("Rotation speed (degrees per second).")]
		public float speed = 10;

		#endregion

		void Update()
		{
			if (canRotate)
				transform.Rotate(speed * Vector3.forward * Time.deltaTime);
		}
	}
}