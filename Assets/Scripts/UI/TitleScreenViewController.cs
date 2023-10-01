using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD54.UI
{
    public class TitleScreenViewController : BaseViewController
    {
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
