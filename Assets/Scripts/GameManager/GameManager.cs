using UnityEngine;

namespace LD54.Game {
    public class GameManager : MonoBehaviour {

        public static GameManager Instance;
        public float ProgressSpeed { get; private set; } = 1f;

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

        // Start 'counter'
        private void StartGame() {

        }
    }
}
