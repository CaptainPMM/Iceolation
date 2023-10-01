using UnityEngine;

namespace LD54.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseViewController : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        // Start is called before the first frame update
        void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            Debug.Assert(_canvasGroup != null, "CanvasGroup not found!!");
        }

        protected void Activate()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1;
        }

        protected void Deactivate()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0;
        }
    }
}
