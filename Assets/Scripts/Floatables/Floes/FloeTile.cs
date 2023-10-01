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

            if (transform.position.x < -GameManager.Instance.GameViewBounds.x)
            {
                Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            IsFloating = _isFloating;
        }
#endif
    }
}