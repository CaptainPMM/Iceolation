using System;
using System.Collections.Generic;
using UnityEngine;

namespace LD54.Utils
{
    [Serializable]
    public struct WeightedValue<T>
    {
        [Min(0f)]
        public float weight;
        [GenericLabel] public T value;

        public WeightedValue(float weight, T value)
        {
            this.weight = Mathf.Abs(weight);
            this.value = value;
        }

        public static T GetWeightedRandom(IEnumerable<WeightedValue<T>> weightedValues, Func<WeightedValue<T>, bool> elementCondition = null)
        {
            float weightsSum = 0f;
            foreach (var wv in weightedValues)
            {
                if (elementCondition == null || elementCondition.Invoke(wv)) weightsSum += Mathf.Abs(wv.weight);
            }
            float rand = UnityEngine.Random.Range(0f, weightsSum);
            weightsSum = 0f;
            foreach (var wv in weightedValues)
            {
                if (elementCondition == null || elementCondition.Invoke(wv))
                {
                    weightsSum += wv.weight;
                    if (rand < weightsSum) return wv.value;
                }
            }
            return default;
        }
    }
}
