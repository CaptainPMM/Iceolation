using System.Collections.Generic;
using UnityEngine;

namespace LD54.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class Steps : MonoBehaviour
    {
        public GameObject StepPrefab;
        public GameObject StepContainer;
        public float StepLifetime = 2.0f;

        public void StepEvent()
        {
            Vector2 directionForward = _playerController.MoveInput;
            if (directionForward == Vector2.zero) directionForward = Vector2.down;

            left = !left;

            Vector2 footstepPosition = this.transform.position;
            Vector2 directionLeft = new(-directionForward.y, directionForward.x);

            Vector2 footstepOffset = new(0.2f, 0.1f);

            if (left)
            {
                footstepPosition += directionLeft * footstepOffset;
            }
            else
            {
                footstepPosition -= directionLeft * footstepOffset;
            }

            CreateStep(footstepPosition, directionLeft);
        }

        private PlayerController _playerController;
        private bool left = false;
        private List<ActiveStep> steps = new();

        private void CreateStep(Vector3 wsPosition, Vector2 directionLeft)
        {
            ActiveStep step = new()
            {
                progress = 0.0f,
                lifetime = StepLifetime,
                instance = Instantiate(StepPrefab)
            };

            step.instance.transform.parent = StepContainer.transform;
            step.instance.transform.position = wsPosition;
            step.instance.transform.rotation = Quaternion.FromToRotation(wsPosition, directionLeft); // no clue why this works
            step.instance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Progress", step.progress);

            steps.Add(step);
        }

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            foreach (var step in steps)
            {
                step.progress += Time.deltaTime / step.lifetime;
                step.instance.GetComponent<SpriteRenderer>().material
                    .SetFloat("_Progress", step.progress);

                if (step.progress > 1.0f) Destroy(step.instance);
            }
            steps.RemoveAll(step => step.progress > 1.0f);
        }

        private class ActiveStep
        {
            public float progress;
            public float lifetime;
            public GameObject instance;
        }
    }
} // namespace LD54.Player
