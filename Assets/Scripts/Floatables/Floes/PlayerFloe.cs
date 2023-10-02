using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LD54.Player;
using LD54.Game;
using LD54.Floatables.Obstacles;

namespace LD54.Floatables.Floes
{
    public class PlayerFloe : Floatable
    {
        public float BorderBoundsOffset = 1.0f;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private CompositeCollider2D _col;
        [SerializeField] private Transform _steeringAxisX;
        [SerializeField] private Transform _steeringAxisY;
        [SerializeField] private Transform _tilesParent;
        [SerializeField] private LayerMask _playerFloeLayer;

        [Header("Settings")]
        [SerializeField] private Vector2 _moveSpeed = new(1f, 1f);
        [SerializeField] private float _steeringAxisXDeadzone = 0.5f;
        [SerializeField] private float _steeringAxisYDeadzone = 0.5f;
        // [SerializeField, Min(0f)] private float _obstacleImpactFactor = 0.03f;
        [SerializeField, Min(0f)] private float _bounceDuration = 1f;
        [SerializeField, Min(0f)] private float _bounceStrength = 10f;

        [Header("State")]
        [SerializeField] private Vector2 _cg; // center of gravity in local space

#if UNITY_EDITOR
        [Header("Editor/Settings")]
        [SerializeField] private float _gizmosCGRadius = 0.5f;
#endif

        private PlayerController _player;

        private bool _geoUpdateThisFrame;
        private Coroutine _bounceRoutine;

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
            if (!_player) Debug.LogWarning("[PlayerFloe] no player found");

            CalcCG();
        }

        private void Update()
        {
            _geoUpdateThisFrame = false;

            if (_player.IsDrowning) return;

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
                if (_col.bounds.max.y + movement.y > GameManager.Instance.GameViewBounds.y - BorderBoundsOffset) movement.y = 0f;
            }
            else
            {
                if (_col.bounds.min.y + movement.y < -GameManager.Instance.GameViewBounds.y + BorderBoundsOffset) movement.y = 0f;
            }

            // Move result
            transform.position += movement;
            _player.transform.position += movement;

            // spawn some waves on the side the floe is "tilted"
            Vector2 playerSteeringMoment = new(playerSteeringXMoment, playerSteeringYMoment);
            // playerSteeringMoment.x (left,right) [-1;1]
            // playerSteeringMoment.y (down,up) [-1;1]
            foreach (Transform tile in _tilesParent)
            {
                Vector2 cgDirection = (Vector2)tile.localPosition - _cg;

                float baseChance = 0.10f;
                float waveProbability = baseChance; // probability of a wave per second

                if (Vector2.Dot(cgDirection, playerSteeringMoment) >= 0.0f)
                {
                    // tile is at the side the floe is moving towards
                    // the more the floe is tilted the higher the probability of a wave
                    float tiltedNess = playerSteeringMoment.magnitude;
                    // = Mathf.Abs(playerSteeringMoment.y) + Mathf.Abs(playerSteeringMoment.x);
                    waveProbability += tiltedNess * cgDirection.magnitude;
                }
                waveProbability *= Time.deltaTime;

                if (Random.Range(0.0f, 1.0f) < waveProbability)
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
            int numDestroyed = 0;
            foreach (BoxCollider2D col in _tilesParent.GetComponentsInChildren<BoxCollider2D>())
            {
                if (col.compositeOperation == Collider2D.CompositeOperation.None)
                {
                    numDestroyed++;
                    continue;
                }
                summedPositions += col.transform.parent.localPosition;
            }

            if (cols.Length - numDestroyed <= 0)
            {
                _cg = Vector2.zero;
                return;
            }

            _cg = summedPositions / (cols.Length - numDestroyed); // every tile has the same weight - otherwise multiply postions with weight and divide by total weight

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
                    _player.AddSunglasses();
                    Destroy(other.gameObject);
                    break;
                case FloatableType.Obstacle:
                    CollideObstacle(floatable as Iceberg);
                    break;
            }
        }

        private void AttachFloe(FloeTile floe)
        {
            if (floe.WasDetached) return;

            floe.IsFloating = false;
            floe.transform.SetParent(_tilesParent);

            SnapToGrid(_col.ClosestPoint(floe.transform.position), out _, out Vector2 localCoords, floe.FloatSpeed);
            floe.transform.localPosition = localCoords;

            GameManager.Instance.Ocean.CreateWave(floe.transform.position, 0.5f, 3.5f, 1.5f);

            DelayedGeoUpdate();
        }

        private void DetachFloe(FloeTile floe)
        {
            floe.WasDetached = true;
            floe.transform.SetParent(null);
            floe.IsFloating = true;

            DelayedGeoUpdate();
        }

        private void CollideObstacle(Iceberg iceberg)
        {
            // Convert hitpoint to local space
            SnapToGrid(_col.ClosestPoint(iceberg.transform.position), out _, out Vector2 localCoords, iceberg.MoveSpeed);

            // Impact
            DestroyInRadius(localCoords, 1 + iceberg.DestructionTileRadius);

            if (_bounceRoutine != null) StopCoroutine(_bounceRoutine);
            _bounceRoutine = StartCoroutine(Bounce(iceberg.transform));

            IEnumerator Bounce(Transform collidedObject)
            {
                Vector2 bounceDir = (transform.position - collidedObject.position).normalized;
                float time = 0f;

                // Bounce the floe off the iceberg
                while (time < _bounceDuration)
                {
                    Vector3 movement = (Vector3)(bounceDir * _moveSpeed * _bounceStrength * Mathf.InverseLerp(_bounceDuration, 0f, time) * Time.deltaTime);

                    if (movement.y >= 0f)
                    {
                        if (_col.bounds.max.y + movement.y > GameManager.Instance.GameViewBounds.y - BorderBoundsOffset) movement.y = 0f;
                    }
                    else
                    {
                        if (_col.bounds.min.y + movement.y < -GameManager.Instance.GameViewBounds.y + BorderBoundsOffset) movement.y = 0f;
                    }

                    transform.position += movement;
                    if (!_player.IsDrowning) _player.transform.position += movement;

                    yield return null;
                    time += Time.deltaTime;
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
                            tile.GetComponent<FloeTile>().AnimatedDestroy(out float delay);
                            DelayedGeoUpdate(delay);
                        }
                    }
                }
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

        private void DelayedGeoUpdate(float delay = -1)
        {
            StartCoroutine(DelayedGeoUpdateRoutine(delay));
        }

        private IEnumerator DelayedGeoUpdateRoutine(float delay = -1f)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            if (_geoUpdateThisFrame) yield break;
            _geoUpdateThisFrame = true;

            _col.GenerateGeometry(); // this method needs some delay...
            CalcCG();
            DetachIslands();
        }

        private void DetachIslands()
        {
            // Get maximum grid coordinates of the floe (in a weird way - but its too late)
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            int minX = int.MaxValue;
            int minY = int.MaxValue;

            foreach (Transform tile in _tilesParent)
            {
                if (tile.localPosition.x > maxX) maxX = (int)tile.localPosition.x;
                if (tile.localPosition.y > maxY) maxY = (int)tile.localPosition.y;
                if (tile.localPosition.x < minX) minX = (int)tile.localPosition.x;
                if (tile.localPosition.y < minY) minY = (int)tile.localPosition.y;
            }

            Vector2 rightAnchor = transform.position + Vector3.right * maxX;
            Vector2 topAnchor = transform.position + Vector3.up * maxY;

            List<int> sliceCandidatesX = new();
            bool lastCollisionX = true;
            for (int x = minX + 1; x < maxX; x++)
            {
                bool collision = Physics2D.Raycast(topAnchor + Vector2.right * x, Vector2.down, maxY - minY, _playerFloeLayer).collider;
                if (collision != lastCollisionX)
                {
                    sliceCandidatesX.Add(collision ? x - 1 : x);
                }
                lastCollisionX = collision;
            }

            List<int> sliceCandidatesY = new();
            bool lastCollisionY = true;
            for (int y = minY + 1; y < maxY; y++)
            {
                bool collision = Physics2D.Raycast(rightAnchor + Vector2.up * y, Vector2.left, maxX - minX, _playerFloeLayer).collider;
                if (collision != lastCollisionY)
                {
                    sliceCandidatesY.Add(collision ? y - 1 : y);
                }
                lastCollisionY = collision;
            }

            if (sliceCandidatesX.Count > 0)
            {
                float playerX = transform.InverseTransformPoint(_player.transform.position).x;

                float nearestSliceCandidateDistRight = float.MaxValue;
                int? nearestSliceCandidateRight = null;
                float nearestSliceCandidateDistLeft = float.MaxValue;
                int? nearestSliceCandidateLeft = null;
                foreach (int sliceCandidateX in sliceCandidatesX)
                {
                    float dist = Mathf.Abs(sliceCandidateX - playerX);
                    if (sliceCandidateX >= playerX)
                    {
                        if (dist < nearestSliceCandidateDistRight)
                        {
                            nearestSliceCandidateDistRight = dist;
                            nearestSliceCandidateRight = sliceCandidateX;
                        }
                    }
                    else
                    {
                        if (dist < nearestSliceCandidateDistLeft)
                        {
                            nearestSliceCandidateDistLeft = dist;
                            nearestSliceCandidateLeft = sliceCandidateX;
                        }
                    }
                }

                foreach (Transform tile in _tilesParent.transform)
                {
                    if (nearestSliceCandidateRight != null && tile.localPosition.x >= nearestSliceCandidateRight
                     || nearestSliceCandidateLeft != null && tile.localPosition.x <= nearestSliceCandidateLeft)
                    {
                        DetachFloe(tile.GetComponent<FloeTile>());
                    }
                }
            }

            if (sliceCandidatesY.Count > 0)
            {
                float playerY = transform.InverseTransformPoint(_player.transform.position).y;

                float nearestSliceCandidateDistAbove = float.MaxValue;
                int? nearestSliceCandidateAbove = null;
                float nearestSliceCandidateDistBelow = float.MaxValue;
                int? nearestSliceCandidateBelow = null;
                foreach (int sliceCandidateY in sliceCandidatesY)
                {
                    float dist = Mathf.Abs(sliceCandidateY - playerY);
                    if (sliceCandidateY >= playerY)
                    {
                        if (dist < nearestSliceCandidateDistAbove)
                        {
                            nearestSliceCandidateDistAbove = dist;
                            nearestSliceCandidateAbove = sliceCandidateY;
                        }
                    }
                    else
                    {
                        if (dist < nearestSliceCandidateDistBelow)
                        {
                            nearestSliceCandidateDistBelow = dist;
                            nearestSliceCandidateBelow = sliceCandidateY;
                        }
                    }
                }

                foreach (Transform tile in _tilesParent.transform)
                {
                    if (nearestSliceCandidateAbove != null && tile.localPosition.y >= nearestSliceCandidateAbove
                     || nearestSliceCandidateBelow != null && tile.localPosition.y <= nearestSliceCandidateBelow)
                    {
                        DetachFloe(tile.GetComponent<FloeTile>());
                    }
                }
            }
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