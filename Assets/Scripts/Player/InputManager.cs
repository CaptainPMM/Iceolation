using UnityEngine;

namespace LD54.Player
{
    public class InputManager : MonoBehaviour
    {
        public delegate void OnMoveInput(Vector2 movement);
        public static event OnMoveInput onMoveInput;

        public delegate void OnAnyKeyPressed();
        public static event OnAnyKeyPressed onAnyKeyPressed;

        private KeyCode moveUp = KeyCode.W;
        private KeyCode moveDown = KeyCode.S;
        private KeyCode moveLeft = KeyCode.A;
        private KeyCode moveRight = KeyCode.D;

        bool isDirty = true;

        public Vector2 moveInput;

        void Update()
        {
            moveInput = new Vector2(0, 0);

            if(Input.anyKeyDown)
            {
                onAnyKeyPressed?.Invoke();
            }

            // UP
            if (Input.GetKey(moveUp))
            {
                moveInput.y = 1;
                isDirty = true;
            }

            // DOWN
            if (Input.GetKey(moveDown))
            {
                moveInput.y = -1;
                isDirty = true;
            }

            // LEFT
            if (Input.GetKey(moveLeft))
            {
                moveInput.x = -1;

                isDirty = true;
            }

            // RIGHT
            if (Input.GetKey(moveRight))
            {
                moveInput.x = 1;
                isDirty = true;
            }

            if (moveInput.magnitude != 0 || isDirty)
            {
                onMoveInput?.Invoke(moveInput);
                if (moveInput.magnitude == 0)
                    isDirty = false;
            }
        }

        private void OnDestroy()
        {
            if (onMoveInput != null)
            {
                foreach(var d in onMoveInput.GetInvocationList())
                {
                    onMoveInput -= (OnMoveInput) d;
                }
            }
        }
    }
}
