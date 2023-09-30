using System.Collections;
using UnityEngine;
using LD54.Player;

namespace LD54.Floatables.Floes
{
    public class PlayerFloe : Floatable
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private CompositeCollider2D _col;
        [SerializeField] private Transform _steeringAxisY;
        [SerializeField] private Transform _tilesParent;

        [Header("Settings")]
        [SerializeField] private Vector2 _moveSpeed = new(1f, 1f);

        [Header("State")]
        [SerializeField] private Vector2 _cg; // center of gravity in local space

#if UNITY_EDITOR
        [Header("Editor/Settings")]
        [SerializeField] private float _gizmosCGRadius = 0.5f;
#endif

        private PlayerController _player;

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
            if (!_player) Debug.LogWarning("[PlayerFloe] no player found");

            CalcCG();
        }

        private void Update()
        {
            Vector3 cgToPlayer = _player.transform.position - transform.TransformPoint(_cg);
            float playerSteeringMoment = _player.Weight * (Mathf.InverseLerp(-_col.bounds.extents.y, _col.bounds.extents.y, cgToPlayer.y) - 0.5f) * 2f;
            _rb.MovePosition(_rb.position + new Vector2(0f, playerSteeringMoment) * _moveSpeed * Time.deltaTime); // no x movement for now
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

            _steeringAxisY.transform.localPosition = new Vector3(transform.InverseTransformPoint(_col.bounds.center).x, _cg.y, 0f);
            _steeringAxisY.transform.localScale = new Vector3(_col.bounds.extents.x * 2f, _steeringAxisY.localScale.y, 1f);
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
            }
        }

        private void AttachFloe(FloeTile floe)
        {
            floe.IsFloating = false;
            floe.transform.SetParent(_tilesParent);

            float halfTileSize = floe.transform.localScale.x * 0.5f;
            float sign = Mathf.Sign(floe.FloatSpeed); // floating left -> positive sign, floating right -> negative sign
            Vector2 hitPos = transform.InverseTransformPoint(_col.ClosestPoint(floe.transform.position)); // needed later

            floe.transform.localPosition = new Vector3(Mathf.RoundToInt(hitPos.x + halfTileSize * sign), Mathf.RoundToInt(hitPos.y), 0f);

            // Check for wrong placement
            RaycastHit2D rayHitHoriz = Physics2D.Raycast(floe.transform.position, Vector2.left * sign, floe.transform.localScale.x);
            if (!rayHitHoriz.collider)
            {
                Vector2 rayHitVertDir = floe.transform.localPosition.y <= hitPos.y ? Vector2.up : Vector2.down;
                RaycastHit2D rayHitVert = Physics2D.Raycast(floe.transform.position, rayHitVertDir, floe.transform.localScale.x);
                if (!rayHitVert.collider) floe.transform.localPosition = new Vector3(floe.transform.localPosition.x - floe.transform.localScale.x * sign, floe.transform.localPosition.y, 0f);
            }

            StartCoroutine(DelayedGeoUpdate());

            IEnumerator DelayedGeoUpdate()
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                _col.GenerateGeometry(); // this method needs some delay...
                CalcCG();
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