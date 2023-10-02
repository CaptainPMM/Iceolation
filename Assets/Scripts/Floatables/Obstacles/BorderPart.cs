using UnityEngine;
using LD54.Game;
using TMPro;

namespace LD54.Floatables.Obstacles
{
    public class BorderPart : Floatable
    {
        [field: Header("Settings")]
        [field: SerializeField] public float MoveSpeed { get; private set; } = 1f; // negative values move to the right

        private void Update()
        {
            transform.position = new Vector3(transform.position.x -
            (MoveSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed),
            transform.position.y, transform.position.z);

            if (transform.position.x < -GameManager.Instance.GameViewBounds.x - 3.0f)
            {
                Destroy(gameObject);
            }

            float waveProbability = 0.5f * MoveSpeed * Time.deltaTime;
            if (Random.Range(0.0f, 1.0f) < waveProbability)
            {
                Vector3 posOffset = new Vector3(0.0f, -0.0f, 0.0f);
                float duration = 2.5f / MoveSpeed;
                float size = 6.0f;
                GameManager.Instance.Ocean.CreateWave(
                    this.transform.position + posOffset, 0.5f, size, duration, 1.0f, Ocean.Ocean.Shape.Oval
                );
            }
        }
    }
}
