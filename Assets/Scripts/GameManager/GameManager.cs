using System.Collections;
using UnityEngine;

namespace LD54.Game
{
    public class GameManager : MonoBehaviour
    {
        public delegate void OnGameStateUpdated();
        public event OnGameStateUpdated onGameStarted;
        public event OnGameStateUpdated onGameEnded;

        public static GameManager Instance { get; private set; }

        [field: SerializeField] public Vector2 GameViewBounds { get; private set; } = new(15f, 5f);

        [field: SerializeField] public float ProgressSpeed { get; private set; } = 1f;

        public bool IsRunning { get; private set; } = false;

        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
                return;
            }
            else Instance = this;
        }

        // Start 'counter'
        private IEnumerator StartProgress()
        {
            IsRunning = true;

            while (IsRunning)
            {
                yield return new WaitForSeconds(15f);
                ProgressSpeed += 0.1f;
            }
        }

        public void StartGame()
        {
            onGameStarted?.Invoke();
            StartCoroutine(StartProgress());
        }
    }
}
