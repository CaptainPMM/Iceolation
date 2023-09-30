using System.Collections;
using UnityEngine;

namespace LD54.Camera
{
    public class CameraAspectFitter : MonoBehaviour
    {
        [SerializeField]
        private Vector2 referenceRatio = new Vector2(16, 9);

        [SerializeField]
        private Vector2 referenceResolution = new Vector2(1920, 1080);
        // Update is called once per frame
        void Start()
        {
            StartCoroutine(UpdateScreenAspectRatio());
        }

        private IEnumerator UpdateScreenAspectRatio()
        {

            int newHeight = Screen.height;
            int newWidth = Screen.width;

            // Set screen aspect res

            if ((float)Screen.width / (float)Screen.height > referenceRatio.x / referenceRatio.y)
            {
                newHeight = Screen.width / 16 * 9;
                newWidth = Screen.width;

                // Calculate multiplier for x direction
                float xMultiplier = Mathf.Clamp01(((float)Screen.height / (float)newHeight));
                float xOffset = Mathf.Clamp01((1 - xMultiplier) / 2f);

                // Set camera rect values
                UnityEngine.Camera.main.rect = new Rect(xOffset, 0, xMultiplier, Screen.width / newWidth);

                yield return new WaitForEndOfFrame();
            }
            else
            {
                newWidth = Screen.height / 9 * 16;
                newHeight = Screen.height;

                // Calculate multiplier for y direction
                float yMultiplier = Mathf.Clamp01(((float)Screen.width / (float)newWidth));
                float yOffset = Mathf.Clamp01((1 - yMultiplier) / 2f);

                // Set camera rect values
                UnityEngine.Camera.main.rect = new Rect(0, yOffset, Screen.height / newHeight, yMultiplier);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
