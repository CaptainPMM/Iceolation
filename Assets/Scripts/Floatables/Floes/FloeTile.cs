using UnityEngine;
using LD54.Game;

namespace LD54.Floatables.Floes
{
    public class FloeTile : Floatable
    {
        [SerializeField] private BoxCollider2D _col;

        [field: Header("Settings")]
        [field: SerializeField] public float FloatSpeed { get; private set; } = 1f; // negative values float to the right
        [SerializeField] private float _floatingScaleFactor = 0.9f;

        [Header("State")]
        [SerializeField] private bool _isFloating;

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

        public void AnimatedDestroy()
        {
            _col.gameObject.layer = LayerMask.NameToLayer("Player Floe");
            _col.compositeOperation = Collider2D.CompositeOperation.None;

            Animator anim = GetComponentInChildren<Animator>();
            anim.enabled = true;

            Vector3 posOffset = new(0.0f, -0.5f, 0.0f);
            float duration = 1.0f;
            float size = 6.0f;
            GameManager.Instance.Ocean.CreateWave(
                this.transform.position + posOffset, 0.0f, size, duration, 1.0f, Ocean.Ocean.Shape.Circular
            );

            Destroy(gameObject, anim.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            IsFloating = _isFloating;
        }
#endif
    }
}