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

        [SerializeField]
        private float collectableProbability = 0.05f;

        [SerializeField]
        private float floeProbability = 0.3f;

        private void Start()
        {
            GameManager.Instance.onGameStarted += StartItemSpawning;
        }

        private void OnDestroy()
        {
            GameManager.Instance.onGameStarted -= StartItemSpawning;
        }

        private void StartItemSpawning()
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

            //itemPrefabs[Random.Range(0, Mathf.FloorToInt(itemPrefabs.Count))]

            int itemIndex = 0;  // Default is iceberg
            float rng = Random.Range(0.0f, 1.0f);

            if(rng > collectableProbability && rng < collectableProbability + floeProbability) { itemIndex = 1; }
            else if(rng < collectableProbability) { itemIndex = 2; }

            GameObject item = Instantiate(itemPrefabs[itemIndex], spawnPosition, Quaternion.identity);
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
