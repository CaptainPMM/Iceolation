using UnityEngine;

namespace LD54.Player {
    public class PlayerController : MonoBehaviour {

        [SerializeField]
        private float acceleration = 15f;

       /* [SerializeField]
        private float deceleration = 20f;
       */

        [SerializeField]
        private float maxVelocity = 4;

        [field: SerializeField] public float Weight { get; private set; } = 1f;


        private Animator animController;
        private Vector2 moveInput;

        private Vector3 moveDelta;

        private bool _playerSunglassesVisible;

        public bool PlayerSunglassesVisible
        {
            get => _playerSunglassesVisible;
            set
            {
                _playerSunglassesVisible = value;
                animController.SetFloat("hasSunglasses", value ? 1.0f : 0.0f);
            }
        }

        private void OnEnable() {
            animController = GetComponent<Animator>();
            InputManager.onMoveInput += OnMove;
        }

        private void OnDisable() {
            InputManager.onMoveInput -= OnMove;
        }

        private void OnMove(Vector2 rawInput) {
            moveInput = rawInput;

            animController.SetFloat("moveX", moveInput.x);
            animController.SetFloat("moveY", moveInput.y);

            /*

            if (rbController.velocity.magnitude < maxVelocity)
                rbController.AddForce(rawInput * acceleration * 200f * Time.deltaTime);     // 200f = factor, so acceleration doesn't need to be 1000 but can be 5 instead
            */


            moveDelta = new Vector3(moveInput.x, moveInput.y, 0).normalized * acceleration * Time.deltaTime;
            transform.localPosition += moveDelta;
        }
    }
}
