using LD54.Game;
using LD54.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LD54.UI
{
    public class TitleScreenViewController : BaseViewController
    {
        private void Start()
        {
            InputManager.onAnyKeyPressed += EmulateClick;
        }

        public void EmulateClick()
        {
            InputManager.onAnyKeyPressed -= EmulateClick;
            GameManager.Instance.StartGame();
            Deactivate();
        }

        public void ShowTitleScreen()
        {
            Activate();
        }

        public void HideTitleScreen()
        {
            Deactivate();
        }
    }
}
