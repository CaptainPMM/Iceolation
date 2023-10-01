using UnityEngine;
using LD54.Game;
using TMPro;

namespace LD54.Floatables.Obstacles
{
    public class Iceberg : Floatable
    {
        [field: Header("Settings")]
        [field: SerializeField] public float MoveSpeed { get; private set; } = 1f; // negative values move to the right

        [SerializeField] private float _weight = 1f;

        [Header("State")]
        [SerializeField] private bool _isFloating;
        public bool IsFloating
        {
            get => _isFloating;
            set
            {
                _isFloating = value;
                enabled = _isFloating;
            }
        }

        public float Weight => _weight;

        private void Update()
        {
            if(_isFloating)
            {
                transform.position = new Vector3(transform.position.x -
                (MoveSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed),
                transform.position.y, transform.position.z);
            }

            if (transform.position.x < -GameManager.Instance.GameViewBounds.x)
            {
                Destroy(gameObject);
            }
        }
    }
}
