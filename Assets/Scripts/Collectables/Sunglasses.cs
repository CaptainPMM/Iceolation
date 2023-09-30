using LD54.Floatables;
using LD54.Game;
using UnityEngine;

namespace LD54.Collectables 
{
    public class Sunglasses : Floatable
    {
        [field: Header("Settings")]
        [field: SerializeField] public float MoveSpeed { get; private set; } = 1f; // negative values move to the right

        private void Update() 
        {
            transform.position = new Vector3(transform.position.x -
                (MoveSpeed * Time.deltaTime * GameManager.Instance.ProgressSpeed),
                transform.position.y, transform.position.z);

            if (transform.position.x < -15) 
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.tag);
        }

    }
}
