using UnityEngine;
using LD54.Game;

namespace LD54.Floatables.Floes
{
    public class FloeTile : Floatable
    {
        [SerializeField] private BoxCollider2D _col;
        [SerializeField] private GameObject _botSprite;

        [field: Header("Settings")]
        [field: SerializeField] public float FloatSpeed { get; private set; } = 1f; // negative values float to the right
        [SerializeField] private float _floatingScaleFactor = 0.9f;

        [Header("State")]
        [SerializeField] private bool _isFloating;
        [SerializeField] private bool _wasDetached;

        public bool IsFloating
        {
            get => _isFloating;
            set
            {
                _isFloating = value;
                enabled = _isFloating;
                if (_isFloating)
                {
                    transform.localScale = Vector3.one * _floatingScaleFactor;
                    _col.compositeOperation = Collider2D.CompositeOperation.None;
                }
                else
                {
                    transform.localScale = Vector3.one;
                    _col.compositeOperation = Collider2D.CompositeOperation.Merge;
                }
            }
        }


        public bool WasDetached
        {
            get => _wasDetached;
            set
            {
                _wasDetached = value;
                // if (value)
                // {
                //     if (_resetWasAttachedRoutine != null) StopCoroutine(_resetWasAttachedRoutine);
                //     _resetWasAttachedRoutine = StartCoroutine(ResetWasAttached());
                // }

                // IEnumerator ResetWasAttached()
                // {
                //     // Wait minimum for PlayerFloe geo update
                //     yield return new WaitForFixedUpdate();
                //     yield return new WaitForFixedUpdate();
                //     _wasDetached = false;
                // }
            }
        }

        // Coroutine _resetWasAttachedRoutine;

        private void Update()
        {
            transform.position = new Vector3(transform.position.x -
                (FloatSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed),
                transform.position.y, transform.position.z);

            if (transform.position.x < -GameManager.Instance.GameViewBounds.x - 3.0f)
            {
                Destroy(gameObject);
            }

            float waveProbability = 3.5f * FloatSpeed * Time.deltaTime;
            if (Random.Range(0.0f, 1.0f) < waveProbability)
            {
                Vector3 posOffset = new Vector3(-0.6f, -0.5f, 0.0f);
                float duration = 0.8f / FloatSpeed + 0.5f;
                float size = 4.0f;
                GameManager.Instance.Ocean.CreateWave(
                    this.transform.position + posOffset, 0.2f, size, duration, 1.0f, Ocean.Ocean.Shape.Trail
                );
            }
        }

        public void AnimatedDestroy(out float tileDestroyedDelay)
        {
            Animator anim = GetComponentInChildren<Animator>();
            anim.enabled = true;

            Vector3 posOffset = new(0.0f, -0.5f, 0.0f);
            float duration = 1.0f;
            float size = 6.0f;
            GameManager.Instance.Ocean.CreateWave(
                this.transform.position + posOffset, 0.0f, size, duration, 1.0f, Ocean.Ocean.Shape.Circular
            );

            float animLength = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            Destroy(_botSprite, animLength - 0.33f);
            Destroy(gameObject, animLength);

            tileDestroyedDelay = animLength;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            IsFloating = _isFloating;
        }
#endif
    }
}