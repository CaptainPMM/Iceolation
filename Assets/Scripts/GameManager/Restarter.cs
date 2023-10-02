using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD54.Game
{
    public class Restarter : MonoBehaviour
    {
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SceneManager.LoadScene("Main");
			}
		}
    }
}
