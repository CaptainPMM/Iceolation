using LD54.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LD54.Player {
[RequireComponent(typeof(PlayerController))]
public class Steps : MonoBehaviour
{
    public GameObject StepPrefab;
    public GameObject StepContainer;

    public void StepEvent()
    {
        Vector2 direction = _playerController.MoveInput;
        if (direction == Vector2.zero) direction = Vector2.down;

        left = !left;

        Vector2 footstepPosition = this.transform.position;
        Vector2 directionRight = new (direction.y, -direction.x);

        Vector2 footstepOffset = new(0.2f, 0.1f);

        if (left)
        {
            footstepPosition -= directionRight * footstepOffset;
        }
        else
        {
            footstepPosition += directionRight * footstepOffset;
        }

        CreateStep(footstepPosition);
    }

    private PlayerController _playerController;
    private bool left = false;
    private List<ActiveStep> steps = new();

    private void CreateStep(Vector3 wsPosition)
    {
        ActiveStep step = new()
        {
            progress = 0.0f,
            lifeTime = 1.0f,
            instance = Instantiate(StepPrefab)
        };

        step.instance.transform.parent = StepContainer.transform;
        step.instance.transform.position = wsPosition;
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
            if (!GameManager.Instance.IsRunning) { return; }

            step.progress += Time.deltaTime / step.lifeTime;
            step.instance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Progress", step.progress);

            if (step.progress > 1.0f) Destroy(step.instance);
        }
        steps.RemoveAll(step => step.progress > 1.0f);
    }

    private class ActiveStep
    {
        public float progress;
        public float lifeTime;
        public GameObject instance;
    }
}
} // namespace LD54.Player
