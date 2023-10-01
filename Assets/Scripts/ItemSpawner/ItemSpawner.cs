using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LD54.Game;
using LD54.Floatables;
using LD54.Floatables.Obstacles;
using LD54.Floatables.Floes;
using LD54.Floatables.Items;
using LD54.Utils;

namespace LD54.ItemGenerator
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<WeightedValue<GameObject>> spawnElements = new();

        [SerializeField]
        private float minTimeBetweenSpawns = 4f;

        [SerializeField]
        private float maxTimeBetweenSpawns = 12f;

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
                GameManager.Instance.GameViewBounds.y * Random.Range(-1f, 1f),
                0f
            );

            GameObject spawnElement = WeightedValue<GameObject>.GetWeightedRandom(spawnElements);
            GameObject element = Instantiate(spawnElement, spawnPosition, Quaternion.identity);
            Floatable floatable = element.GetComponent<Floatable>();

            // Object specific initialization
            switch (floatable.Type)
            {
                case FloatableType.Floe:
                    (floatable as FloeTile).IsFloating = true;
                    break;
                case FloatableType.Obstacle:
                    (floatable as Iceberg).IsFloating = true;
                    break;
                case FloatableType.Item:
                    (floatable as Sunglasses).IsFloating = true;
                    break;
            }
        }
    }
}
