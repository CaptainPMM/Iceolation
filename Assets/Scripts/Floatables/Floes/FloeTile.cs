using LD54.Game;
using UnityEngine;

namespace LD54.Floatables.Floes
{
    public class FloeTile : Floatable
    {
        [SerializeField] private BoxCollider2D _col;

        [field: Header("Settings")]
        [field: SerializeField] public float FloatSpeed { get; private set; } = 1f; // negative values float to the right

        [Header("State")]
        [SerializeField] private bool _isFloating;

        public bool IsFloating
        {
            get => _isFloating;
            set
            {
                _isFloating = value;
                enabled = _isFloating;
                if (_isFloating) _col.compositeOperation = Collider2D.CompositeOperation.None;
                else _col.compositeOperation = Collider2D.CompositeOperation.Merge;
            }
        }

        private void Update()
        {
            transform.position = new Vector3(transform.position.x - 
                (FloatSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed), 
                transform.position.y, transform.position.z);

            if (transform.position.x < -15) 
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