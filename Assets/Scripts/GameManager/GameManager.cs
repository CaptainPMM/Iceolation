using System.Collections;
using UnityEngine;

namespace LD54.Game {
    public class GameManager : MonoBehaviour {

        public static GameManager Instance;
        public float ProgressSpeed { get; private set; } = 1f;
        public bool IsRunning { get; private set; } = false;


        private void Awake() {
            if (Instance == null) {
                Instance = this;
                return;
            }
            else {
                if (Instance != this) {
                    DestroyImmediate(this);
                }
            }
        }


        private void Start() {
            StartCoroutine(StartGame());                // CHANGE ME! Do this when the game is actually started, not in the Start()
        }

        // Start 'counter'
        private IEnumerator StartGame() {
            IsRunning = true;

            while(IsRunning) {
                yield return new WaitForSeconds(15f);
                ProgressSpeed += 0.1f;
            }
        }
    }
}
