using UnityEngine;

namespace Floatables
{
    public abstract class Floatable : MonoBehaviour
    {
        [field: Header("Setup")]
        [field: SerializeField] public FloatableType Type { get; protected set; }
    }
}