//--------------------------------------------------------------------------------------------------------------------------------
// Cartoon FX
// (c) 2012-2025 Jean Moreno
//--------------------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace CartoonFX
{
	public class CFXR_Demo_Translate : MonoBehaviour
	{
		public Vector3 direction = new Vector3(0,1,0);
		public bool randomRotation;


        private bool initialized;
        private Vector3 initialPosition;

        private void Awake()
		{
			if (!initialized)
			{
				initialized = true;
				initialPosition = transform.position;
			}
		}

        private void OnEnable()
		{
			transform.position = initialPosition;
			if (randomRotation)
			{
				transform.eulerAngles = Vector3.Lerp(Vector3.zero, Vector3.up * 360, Random.value);
			}
		}

        private void Update()
		{
			transform.Translate(direction * Time.deltaTime);
		}
	}
}
