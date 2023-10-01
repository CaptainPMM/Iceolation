using UnityEngine;
using LD54.Game;

namespace LD54.Floatables.Items
{
    public class Sunglasses : Floatable
    {
        [field: Header("Settings")]
        [field: SerializeField] public float MoveSpeed { get; private set; } = 1f; // negative values move to the right

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

        private void Update()
        {
            if(_isFloating)
            {
                transform.position = new Vector3(transform.position.x -
                (MoveSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed),
                transform.position.y, transform.position.z);

                float waveProbability = 3.5f * MoveSpeed * Time.deltaTime;
                if (Random.Range(0.0f, 1.0f) < waveProbability)
                {
                    Vector3 posOffset = new Vector3(-0.5f, -0.0f, 0.0f);
                    float duration = 0.8f / MoveSpeed + 0.0f;
                    float size = 3.0f;
                    GameManager.Instance.Ocean.CreateWave(
                        this.transform.position + posOffset, 0.2f, size, duration, 1.0f, Ocean.Ocean.Shape.Trail
                    );
                }
            }

            if (transform.position.x < -GameManager.Instance.GameViewBounds.x)
            {
                Destroy(gameObject);
            }
        }
    }
}
