using System.Collections;
using UnityEngine;
using LD54.Player;
using LD54.Game;
using LD54.Floatables.Obstacles;

namespace LD54.Floatables.Floes
{
    public class PlayerFloe : Floatable
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private CompositeCollider2D _col;
        [SerializeField] private Transform _steeringAxisX;
        [SerializeField] private Transform _steeringAxisY;
        [SerializeField] private Transform _tilesParent;

        [Header("Settings")]
        [SerializeField] private Vector2 _moveSpeed = new(1f, 1f);
        [SerializeField] private float _steeringAxisXDeadzone = 0.5f;
        [SerializeField] private float _steeringAxisYDeadzone = 0.5f;
        [SerializeField, Min(0f)] private float _obstacleImpactFactor = 0.03f;

        [Header("State")]
        [SerializeField] private Vector2 _cg; // center of gravity in local space

#if UNITY_EDITOR
        [Header("Editor/Settings")]
        [SerializeField] private float _gizmosCGRadius = 0.5f;
#endif

        private PlayerController _player;
        private Coroutine _bounceRoutine;
        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
            if (!_player) Debug.LogWarning("[PlayerFloe] no player found");

            CalcCG();
        }

        private void Update()
        {
            // Calc desired movement
            Vector3 cgToPlayer = _player.transform.position - transform.TransformPoint(_cg);

            float playerSteeringXMoment = 0f;
            if (Mathf.Abs(cgToPlayer.x) * 2f > _steeringAxisXDeadzone)
                playerSteeringXMoment = _player.Weight * (Mathf.InverseLerp(-_col.bounds.extents.x, _col.bounds.extents.x, cgToPlayer.x) - 0.5f) * 2f;

            float playerSteeringYMoment = 0f;
            if (Mathf.Abs(cgToPlayer.y) * 2f > _steeringAxisYDeadzone)
                playerSteeringYMoment = _player.Weight * (Mathf.InverseLerp(-_col.bounds.extents.y, _col.bounds.extents.y, cgToPlayer.y) - 0.5f) * 2f;

            Vector3 movement = new Vector3(playerSteeringXMoment, playerSteeringYMoment, 0f) * _moveSpeed * Time.deltaTime;

            // Check bounds
            if (movement.x >= 0f)
            {
                if (_col.bounds.max.x + movement.x > GameManager.Instance.GameViewBounds.x) movement.x = 0f;
            }
            else
            {
                if (_col.bounds.min.x + movement.x < -GameManager.Instance.GameViewBounds.x) movement.x = 0f;
            }

            if (movement.y >= 0f)
            {
                if (_col.bounds.max.y + movement.y > GameManager.Instance.GameViewBounds.y) movement.y = 0f;
            }
            else
            {
                if (_col.bounds.min.y + movement.y < -GameManager.Instance.GameViewBounds.y) movement.y = 0f;
            }

            // Move result
            transform.position += movement;
            _player.transform.position += movement;

            // spawn some waves on the side the floe is "tilted"
            // playerSteeringMoment (down,up) [-1;1]
            foreach (Transform tile in _tilesParent)
            {
                float cgDirection = tile.localPosition.y - _cg.y;

                float baseChance = 0.05f;
                float waveProbabilityPerFloe = baseChance;
                if (cgDirection * playerSteeringYMoment >= 0.0f)
                {
                    // tile is at the side the floe is moving towards
                    // the more the floe is tilted the higher the probability of a wave
                    waveProbabilityPerFloe += Mathf.Abs(playerSteeringYMoment) * Mathf.Abs(cgDirection) + baseChance;
                }
                waveProbabilityPerFloe *= Time.deltaTime;

                if (Random.Range(0.0f, 1.0f) < waveProbabilityPerFloe)
                {
                    Vector3 posOffset = new Vector3(0.0f, -0.25f, 0.0f);
                    GameManager.Instance.Ocean.CreateWave(tile.position + posOffset, 0.6f, 4.0f, 1.5f);
                }
            }
        }

        [ContextMenu("Calculate CG")]
        private void CalcCG()
        {
            BoxCollider2D[] cols = _tilesParent.GetComponentsInChildren<BoxCollider2D>();
            Vector3 summedPositions = Vector3.zero;
            foreach (BoxCollider2D col in _tilesParent.GetComponentsInChildren<BoxCollider2D>())
            {
                summedPositions += col.transform.parent.localPosition;
            }
            _cg = summedPositions / cols.Length; // every tile has the same weight - otherwise multiply postions with weight and divide by total weight

            _steeringAxisX.transform.localPosition = new Vector3(_cg.x, transform.InverseTransformPoint(_col.bounds.center).y, 0f);
            _steeringAxisX.transform.localScale = new Vector3(_steeringAxisXDeadzone, _col.bounds.extents.y * 2f, 1f);

            _steeringAxisY.transform.localPosition = new Vector3(transform.InverseTransformPoint(_col.bounds.center).x, _cg.y, 0f);
            _steeringAxisY.transform.localScale = new Vector3(_col.bounds.extents.x * 2f, _steeringAxisYDeadzone, 1f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Floatable floatable = other.GetComponentInParent<Floatable>();
            switch (floatable?.Type)
            {
                case FloatableType.Floe:
                    AttachFloe(floatable as FloeTile);
                    break;
                case FloatableType.Item:
                    if (!_player.PlayerSunglassesVisible) _player.PlayerSunglassesVisible = true;
                    Destroy(other.gameObject);
                    break;
                case FloatableType.Obstacle:
                    CollideObstacle(floatable as Iceberg);
                    break;
            }
        }

        private void AttachFloe(FloeTile floe)
        {
            floe.IsFloating = false;
            floe.transform.SetParent(_tilesParent);

            SnapToGrid(_col.ClosestPoint(floe.transform.position), out _, out Vector2 localCoords, floe.FloatSpeed);
            floe.transform.localPosition = localCoords;

            GameManager.Instance.Ocean.CreateWave(floe.transform.position, 0.5f, 3.5f, 1.5f);

            StartCoroutine(DelayedGeoUpdate());
        }

        private void CollideObstacle(Iceberg iceberg)
        {
            // Convert hitpoint to local space
            SnapToGrid(_col.ClosestPoint(iceberg.transform.position), out _, out Vector2 localCoords, iceberg.MoveSpeed);

            // Impact
            float impact = iceberg.Weight * _tilesParent.childCount * GameManager.Instance.ProgressSpeed;
            DestroyInRadius(localCoords, 2 + Mathf.RoundToInt(impact * _obstacleImpactFactor));

            // Geo Update
            StartCoroutine(DelayedGeoUpdate());
            
            // Bounce if not already bouncing
            if(_bounceRoutine == null)
                _bounceRoutine = StartCoroutine(Bounce(iceberg.transform));


            IEnumerator Bounce(Transform collidedObject)
            {
                Vector3 bounceDir = -1 * (collidedObject.position - transform.position);
                float beta = 0f;

                // Bounce the floe off the iceberg
                while (beta < 1)
                {
                    // move character and floe away from obstacle
                    
                    transform.position += bounceDir * Time.deltaTime;
                    _player.transform.localPosition += bounceDir * Time.deltaTime;
                    yield return null;
                    beta += Time.deltaTime;
                }

                _bounceRoutine = null;
            }
        }

        private void SnapToGrid(Vector2 globalPos, out Vector2 globalCoords, out Vector2 localCoords, float floatDir = 1f)
        {
            Vector2 localPos = transform.InverseTransformPoint(globalPos);

            float tileSize = _tilesParent.GetChild(0).transform.localScale.x;
            float sign = Mathf.Sign(floatDir); // floating left -> positive sign, floating right -> negative sign

            localCoords = new Vector2(Mathf.RoundToInt(localPos.x + tileSize * 0.5f * sign), Mathf.RoundToInt(localPos.y));
            globalCoords = transform.TransformPoint(localCoords);

            // Check for wrong placement
            RaycastHit2D rayHitHoriz = Physics2D.Raycast(globalCoords, Vector2.left * sign, tileSize);
            if (!rayHitHoriz.collider)
            {
                Vector2 rayHitVertDir = localCoords.y <= localPos.y ? Vector2.up : Vector2.down;
                RaycastHit2D rayHitVert = Physics2D.Raycast(globalCoords, rayHitVertDir, tileSize);
                if (!rayHitVert.collider)
                {
                    localCoords = new Vector2(localCoords.x - tileSize * sign, localCoords.y);
                    globalCoords = transform.TransformPoint(localCoords);
                }
            }
        }

        private void DestroyInRadius(Vector2 _normalizedHitPosition, int _radius)
        {
            int childCount = _tilesParent.childCount;
            // Traverse the square created by the radius in both x- and y- direction
            for (int x = -_radius; x < _radius; x++)                                    // Change here to make hemisphere
            {
                for (int y = -_radius; y <= _radius; y++)
                {
                    // Check only in circle
                    float distance_squared = x * x + y * y;                             // Used this in order not to use sqrt
                    if (distance_squared < (float)_radius * (float)_radius)             // Only destroy tile if its inside the radius
                    {
                        GameObject tile = FindTile((int)_normalizedHitPosition.x + x,
                        (int)_normalizedHitPosition.y + y)?.gameObject;                 // Try finding tile at this position

                        // Destroy tile if it exists
                        if (tile)
                        {
                            Destroy(tile);
                            childCount--;
                        }
                    }
                }
            }

            if (childCount <= 0)
            {
                // Loose condition
                GameManager.Instance.EndGame(false);
                Destroy(gameObject);
                Destroy(_player.gameObject);
            }
        }

        // Try to find tile in position x,y
        private Transform FindTile(int tileCoordX, int tileCoordY)
        {
            foreach (Transform tile in _tilesParent)
            {
                if (tile.localPosition.x == tileCoordX
                    && tile.localPosition.y == tileCoordY)
                {
                    return tile;
                }
            }
            return null;
        }

        private IEnumerator DelayedGeoUpdate()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            _col.GenerateGeometry(); // this method needs some delay...
            CalcCG();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + new Vector3(_cg.x, _cg.y, 0f), _gizmosCGRadius);
        }
#endif
    }
}