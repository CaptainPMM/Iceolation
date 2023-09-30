using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iceberg : MonoBehaviour
{
    [field: Header("Settings")]
    [field: SerializeField] public float MoveSpeed { get; private set; } = 1f; // negative values move to the right

    private void Update()
    {
        transform.position = new Vector3(transform.position.x - (MoveSpeed * Time.deltaTime), transform.position.y, transform.position.z);
    }
}
