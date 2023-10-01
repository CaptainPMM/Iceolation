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
        public float BorderUpperOffset = -1.0f;
        public float BorderLowerOffset = -0.5f;
        public GameObject BorderObjectPrefab;
        public GameObject BordersContainer;
        public float BorderObjectFrequency = 0.3f;

        [SerializeField]
        private List<WeightedValue<GameObject>> spawnElements = new();

        [SerializeField]
        private float minTimeBetweenSpawns = 4f;

        [SerializeField]
        private float maxTimeBetweenSpawns = 12f;

        private void Start()
        {
            GameManager.Instance.onGameStarted += StartItemSpawning;
            StartCoroutine(SpawnBorderObjects());
        }

        private void OnDestroy()
        {
            GameManager.Instance.onGameStarted -= StartItemSpawning;
        }

        private void StartItemSpawning()
        {
            StartCoroutine(SpawnObjects());
        }

        private IEnumerator SpawnBorderObjects()
        {
            yield return null;
            while (true)
            {
                SpawnBorderObject();
                yield return new WaitForSeconds(1.0f/(BorderObjectFrequency + Random.Range(-0.1f, 0.1f)));
            }
        }

        private void SpawnBorderObject()
        {
            SpawnBorderObject(GameManager.Instance.GameViewBounds.y + BorderUpperOffset + Random.Range(0f, 1.0f));
            SpawnBorderObject(-GameManager.Instance.GameViewBounds.y - BorderLowerOffset - Random.Range(0f, 1.0f));
        }

        private void SpawnBorderObject(float y)
        {
            Vector3 spawnPosition = new (GameManager.Instance.GameViewBounds.x, y, 0f);

            GameObject element = Instantiate(BorderObjectPrefab, spawnPosition, Quaternion.identity);
            element.GetComponent<Iceberg>().IsFloating = true;
            element.transform.parent = BordersContainer.transform;
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
