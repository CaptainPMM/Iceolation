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
                (MoveSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed * GameManager.Instance.FloeFlowDirection),
                transform.position.y, transform.position.z);
            }

            if (transform.position.x < -GameManager.Instance.GameViewBounds.x)
            {
                Destroy(gameObject);
            }

            float waveProbability = (2.5f + Weight) * MoveSpeed * Time.deltaTime;
            if (Random.Range(0.0f, 1.0f) < waveProbability)
            {
                Vector3 posOffset = new Vector3(0.0f, -0.0f, 0.0f);
                float duration = 1f / MoveSpeed + Weight;
                float size = 6.0f + Weight*2.0f;
                GameManager.Instance.Ocean.CreateWave(this.transform.position + posOffset, 0.2f, size, duration, 0.6f);
            }
        }
    }
}
