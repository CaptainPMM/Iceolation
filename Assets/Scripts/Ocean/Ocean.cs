using System.Collections.Generic;
using UnityEngine;

namespace LD54.Ocean
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Ocean : MonoBehaviour
    {
        public float WaterSpeed = 1.5f;
        public GameObject WavePrefab;

        [ContextMenu("Create Wave")]
        public void CreateWave()
        {
            CreateWave(new Vector2(2.0f, 2.0f));
        }

        public void CreateWave(Vector2 worldPosition, float radius = 2.0f, float duration = 1.0f)
        {
            WaveData wave = new();
            wave.progress = 0.0f;
            wave.duration = duration;
            wave.waveInstance = Instantiate(WavePrefab);
            wave.waveInstance.transform.position = worldPosition;
            wave.waveInstance.transform.localScale = new Vector3(radius, radius, 1.0f);
            wave.waveInstance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Pixels", radius * 30.0f); // one "pixel" is 30 pixels big
            wave.waveInstance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Wave_Width", 1.0f / radius);
            wave.waveInstance.transform.parent = this.transform;
            waveData.Add(wave);
        }

        private List<WaveData> waveData = new();

        private SpriteRenderer srr;
        private Material oceanMaterial
        {
            get
            {
                if (srr == null) srr = GetComponent<SpriteRenderer>();
                return srr.sharedMaterial;
            }
        }

        private void Update()
        {
            foreach (var wave in waveData)
            {
                wave.progress += Time.deltaTime / wave.duration;
                wave.waveInstance.GetComponent<SpriteRenderer>().material
                    .SetFloat("_Progress", wave.progress);
                if (wave.progress > 1.0f) Destroy(wave.waveInstance);
            }
            waveData.RemoveAll(wave => wave.progress > 1.0f);
        }

        private void OnValidate()
        {
            float shaderWaterSpeed = WaterSpeed * 2f;
            oceanMaterial.SetFloat("_Scroll_Speed", shaderWaterSpeed);
        }

        private class WaveData
        {
            public float progress;
            public float duration;
            public GameObject waveInstance;
        }
    }
}
