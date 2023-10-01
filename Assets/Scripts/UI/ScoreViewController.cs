using LD54.Game;
using System.Collections;
using TMPro;
using UnityEngine;

namespace LD54.UI
{
    public class ScoreViewController : BaseViewController
    {
        [SerializeField]
        private TextMeshProUGUI _scoreText;

        private float _score = 0f;

        private void Start()
        {
            GameManager.Instance.onGameStarted += StartTimer;
        }

        private void OnDestroy()
        {
            GameManager.Instance.onGameStarted -= StartTimer;
        }

        private void StartTimer()
        {
            Activate();
            StartCoroutine(CountScore());
        }

        private IEnumerator CountScore()
        {
            yield return null;
            while(GameManager.Instance.IsRunning)
            {
                _score += Time.deltaTime;
                _scoreText.text = "" + (int)_score;
                yield return null;
            }
        }
    }
}

