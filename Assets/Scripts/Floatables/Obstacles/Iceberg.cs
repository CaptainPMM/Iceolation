using UnityEngine;
using LD54.Game;
using TMPro;

namespace LD54.Floatables.Obstacles
{
    public class Iceberg : Floatable
    {
        [field: Header("Settings")]
        [field: SerializeField] public float MoveSpeed { get; private set; } = 1f; // negative values move to the right

        [SerializeField] private byte _destructionTileRadius = 1;
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

        public byte DestructionTileRadius => _destructionTileRadius;
        public float Weight => _weight;

        private void Update()
        {
            if (_isFloating)
            {
                transform.position = new Vector3(transform.position.x -
                (MoveSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed),
                transform.position.y, transform.position.z);
            }

            if (transform.position.x < -GameManager.Instance.GameViewBounds.x - 3.0f)
            {
                Destroy(gameObject);
            }

            float speed = MoveSpeed * GameManager.Instance.ProgressSpeed;
            float waveProbability = (2.5f + Weight * 0.5f) * speed * Time.deltaTime;
            if (Random.Range(0.0f, 1.0f) < waveProbability)
            {
                Vector3 posOffset = new Vector3(Weight * 0.0f, -0.0f, 0.0f);
                float duration = 1.2f / speed + Weight * 1.0f;
                float size = 4.0f + Weight * 4.0f + speed * 0.2f;
                GameManager.Instance.Ocean.CreateWave(
                    this.transform.position + posOffset, 0.2f, size, duration, 1.0f, Ocean.Ocean.Shape.Trail
                );
            }
        }
    }
}
