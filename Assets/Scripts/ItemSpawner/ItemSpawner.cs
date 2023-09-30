using LD54.Floatables.Floes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LD54.Game;

namespace LD54.ItemGenerator {

    public class ItemSpawner : MonoBehaviour {

        [SerializeField]
        private List<GameObject> itemPrefabs = new();

        [SerializeField]
        private Transform upperBound;

        [SerializeField]
        private Transform lowerBound;

        [SerializeField]
        private float minTimeBetweenSpawns = 4f;

        [SerializeField]
        private float maxTimeBetweenSpawns = 12f;

        private bool isRunning = true;

        void Start() {
            StartCoroutine(SpawnObjects());
        }

        private IEnumerator SpawnObjects() {
            while(isRunning) {
                SpawnObject();
                yield return WaitBetweenSpawn();
            }
        }
        private IEnumerator WaitBetweenSpawn() {
            yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns) / GameManager.Instance.ProgressSpeed);
        }

        private void SpawnObject() {
            Vector3 spawnPosition = Vector3.Lerp(lowerBound.transform.position, upperBound.transform.position, Random.Range(0.0f, 1.0f));
            GameObject item = Instantiate(itemPrefabs[Random.Range(0, Mathf.FloorToInt(itemPrefabs.Count))], spawnPosition, Quaternion.identity);

            // Object specific initialization
            if(item.TryGetComponent<FloeTile>(out FloeTile floeTile)) {
                floeTile.IsFloating = true;
            }
        }
    }
}
