using System.Collections;
using UnityEngine;

namespace LD54.Floatables.Floes
{
    public class PlayerFloe : Floatable
    {
        [SerializeField] private Transform _tilesParent;
        [SerializeField] private CompositeCollider2D _col;

        [Header("State")]
        [SerializeField] private Vector2 _cg; // center of gravity in local space

#if UNITY_EDITOR
        [Header("Editor/Settings")]
        [SerializeField] private float _gizmosCGRadius = 0.5f;
#endif

        private void Start()
        {
            CalcCG();
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
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Floatable floatable = other.GetComponentInParent<Floatable>();
            switch (floatable?.Type)
            {
                case FloatableType.Floe:
                    AttachFloe(floatable as FloeTile);
                    break;
            }
        }

        private void AttachFloe(FloeTile floe)
        {
            floe.IsFloating = false;
            floe.transform.SetParent(_tilesParent);

            Vector2 hitPos = transform.InverseTransformPoint(_col.ClosestPoint(floe.transform.position)); // needed later
            float sign = Mathf.Sign(floe.FloatSpeed); // floating left -> positive sign, floating right -> negative sign

            floe.transform.localPosition = new Vector3(Mathf.RoundToInt(hitPos.x + 0.5f * sign), Mathf.RoundToInt(hitPos.y), 0f);

            // Check for wrong placement (e.g. holes)
            RaycastHit2D rayHit = Physics2D.Raycast(floe.transform.position, Vector2.left * sign, 1f);
            if (!rayHit.collider)
            {
                // Wrong attachement at a corner of the player floe
                float yPos;
                if (floe.transform.localPosition.y <= hitPos.y) yPos = Mathf.CeilToInt(hitPos.y);
                else yPos = Mathf.FloorToInt(hitPos.y);
                floe.transform.localPosition = new Vector3(floe.transform.localPosition.x, yPos, 0f);
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