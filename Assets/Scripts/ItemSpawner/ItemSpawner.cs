using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LD54.Game;
using LD54.Floatables.Floes;
using LD54.Floatables;
using LD54.Floatables.Items;
using LD54.Floatables.Obstacles;

namespace LD54.ItemGenerator
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> itemPrefabs = new();

        [SerializeField]
        private float minTimeBetweenSpawns = 4f;

        [SerializeField]
        private float maxTimeBetweenSpawns = 12f;

        void Start()
        {
            StartCoroutine(SpawnObjects());
        }

        private IEnumerator SpawnObjects()
        {
            yield return null;
            while (GameManager.Instance.IsRunning)
            {
                SpawnObject();
                yield return WaitBetweenSpawn();
            }
        }

        private IEnumerator WaitBetweenSpawn()
        {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns) / GameManager.Instance.ProgressSpeed);
        }

        private void SpawnObject()
        {
            Vector3 spawnPosition = new Vector3(
                GameManager.Instance.GameViewBounds.x,
                GameManager.Instance.GameViewBounds.y * Random.Range(-1.0f, 1.0f),
                0f
            );
            GameObject item = Instantiate(itemPrefabs[Random.Range(0, Mathf.FloorToInt(itemPrefabs.Count))], spawnPosition, Quaternion.identity);

            Floatable fItem = item.GetComponent<Floatable>();

            // Object specific initialization
            switch (fItem.Type)
            {
                case FloatableType.Floe:
                    (fItem as FloeTile).IsFloating = true;
                    break;
                case FloatableType.Obstacle:
                    (fItem as Iceberg).IsFloating = true;
                    break;
                case FloatableType.Item:
                    (fItem as Sunglasses).IsFloating = true;
                    break;
            }
        }
    }
}
