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

        GameObject instance = Instantiate(StepPrefab);
        instance.transform.parent = StepContainer.transform;
        instance.transform.position = this.transform.position;
    }

    private PlayerController _playerController;
    private bool left = false;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }
}
} // namespace LD54.Player
