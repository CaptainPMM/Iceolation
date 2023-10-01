using LD54.Game;
using UnityEngine;

namespace LD54.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody2D _rb;
        public Rigidbody2D RB => _rb;

        [SerializeField]
        private CircleCollider2D _col;

        [SerializeField]
        private LayerMask _playerFloeLayer;

        [SerializeField]
        private float speed = 3f;

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

        private void Start()
        {
            animController = GetComponent<Animator>();
            GameManager.Instance.onGameStarted += AttachInput;
            GameManager.Instance.onGameEnded += DetachInput;
        }

        private void OnDestroy()
        {
            GameManager.Instance.onGameStarted -= AttachInput;
            GameManager.Instance.onGameEnded -= DetachInput;
        }

        private void AttachInput()
        {
            InputManager.onMoveInput += OnMove;
        }

        private void DetachInput(bool win)
        {
            InputManager.onMoveInput -= OnMove;
        }

        private void OnMove(Vector2 rawInput)
        {
            moveInput = rawInput;

            animController.SetFloat("moveX", moveInput.x);
            animController.SetFloat("moveY", moveInput.y);

            /*
            if (rbController.velocity.magnitude < maxVelocity)
                rbController.AddForce(rawInput * acceleration * 200f * Time.deltaTime);     // 200f = factor, so acceleration doesn't need to be 1000 but can be 5 instead
            */

            Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0f).normalized * speed * Time.deltaTime;

            // Check floe bounds
            RaycastHit2D hitR = Physics2D.Raycast((Vector2)_col.bounds.center + Vector2.right * _col.radius, Vector2.right, 0.001f, _playerFloeLayer);
            RaycastHit2D hitL = Physics2D.Raycast((Vector2)_col.bounds.center + Vector2.left * _col.radius, Vector2.left, 0.001f, _playerFloeLayer);
            RaycastHit2D hitU = Physics2D.Raycast((Vector2)_col.bounds.center + Vector2.up * _col.radius, Vector2.up, 0.001f, _playerFloeLayer);
            RaycastHit2D hitD = Physics2D.Raycast((Vector2)_col.bounds.center + Vector2.down * _col.radius, Vector2.down, 0.001f, _playerFloeLayer);

            if (!hitR.collider) movement.x = Mathf.Min(movement.x, 0f);
            if (!hitL.collider) movement.x = Mathf.Max(movement.x, 0f);
            if (!hitU.collider) movement.y = Mathf.Min(movement.y, 0f);
            if (!hitD.collider) movement.y = Mathf.Max(movement.y, 0f);

            transform.position += movement;
        }
    }
}
