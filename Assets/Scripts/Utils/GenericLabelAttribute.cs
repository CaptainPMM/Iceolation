using UnityEngine;

namespace LD54.Utils
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class GenericLabelAttribute : PropertyAttribute
    {
        public readonly string _prefix;
        public readonly string _suffix;

        public GenericLabelAttribute() : this(null, null) { }
        public GenericLabelAttribute(string prefix) : this(prefix, null) { }
        public GenericLabelAttribute(string prefix, string suffix)
        {
            _prefix = prefix;
            _suffix = suffix;
        }
    }
}
