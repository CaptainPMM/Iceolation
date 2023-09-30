using UnityEngine;

namespace LD54.Player {
    public class PlayerController : MonoBehaviour {

        [SerializeField]
        private float accelerateSpeed = 4;

        [SerializeField]
        private float maxVelocity = 4;

        private Rigidbody2D rbController;

        private void OnEnable() {

            rbController = GetComponent<Rigidbody2D>();
            InputManager.onMoveInput += OnMove;
        }

        private void OnDisable() {
            InputManager.onMoveInput -= OnMove;
        }

        private void OnMove(Vector2 rawInput) {

            if(rbController.velocity.magnitude < maxVelocity)
                rbController.AddForce(rawInput * accelerateSpeed * 200f * Time.deltaTime);
        }
    }
}
