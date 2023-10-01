using LD54.Game;
using UnityEngine;

namespace LD54.UI
{
    public class EndscreenViewController : BaseViewController
    {
        [SerializeField]
        private bool isWinScreen;

        private void Start()
        {
            GameManager.Instance.onGameEnded += GameEnded;
        }

        private void OnDestroy()
        {
            GameManager.Instance.onGameEnded -= GameEnded;
        }

        private void GameEnded(bool win)
        {
            if(isWinScreen == win)
            {
                Activate();
            }
        }
    }
}
