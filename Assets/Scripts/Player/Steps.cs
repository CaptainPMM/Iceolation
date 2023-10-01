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
        Debug.Log((left ? "Left" : "Right") + " step!, Direction: " + direction);
        CreateStep(this.transform.position);
    }

    private PlayerController _playerController;
    private bool left = false;
    private List<ActiveStep> steps = new();

    private void CreateStep(Vector3 wsPosition)
    {
        ActiveStep step = new();
        step.progress = 0.0f;
        step.lifeTime = 1.0f;
        step.instance = Instantiate(StepPrefab);

        step.instance.transform.parent = StepContainer.transform;
        step.instance.transform.position = wsPosition;
        // step.instance.GetComponent<SpriteRenderer>().material
        //     .SetFloat("", x);

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
