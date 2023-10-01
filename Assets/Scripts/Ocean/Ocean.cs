using System.Collections.Generic;
using LD54.Game;
using UnityEngine;

namespace LD54.Ocean
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Ocean : MonoBehaviour
    {
        public float WaterSpeed = 3.0f;
        public GameObject WavePrefab;

        [ContextMenu("Create Wave")]
        public void CreateWave()
        {
            CreateWave(new Vector2(2.0f, 2.0f), 10.0f, 3.0f);
        }

        public void CreateWave(Vector2 worldPosition, float progress = 0.0f, float radius = 2.0f, float duration = 1.0f)
        {
            WaveData wave = new();
            wave.progress = progress;
            wave.duration = duration;
            wave.waveInstance = Instantiate(WavePrefab);
            wave.waveInstance.transform.position = worldPosition;
            wave.waveInstance.transform.localScale = new Vector3(radius, radius, 1.0f);
            wave.waveInstance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Pixels", radius * 15.0f); // one "pixel" is 15 pixels big
            wave.waveInstance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Wave_Width", 1.5f / radius);
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

        private void Start()
        {
            this.transform.localScale = new Vector3(
                GameManager.Instance.GameViewBounds.x * 2.1f,
                GameManager.Instance.GameViewBounds.y * 2.1f,
                1.0f
            );
            OnValidate();
        }

        private void Update()
        {
            foreach (var wave in waveData)
            {
                wave.progress += Time.deltaTime / wave.duration;
                wave.waveInstance.GetComponent<SpriteRenderer>().material
                    .SetFloat("_Progress", wave.progress);
                Vector3 position = wave.waveInstance.transform.position;
                // looks better if the wave moves only with half the water speed
                // position.x += Time.deltaTime * WaterSpeed * 0.5f; // actually looks better if the wave does not move
                wave.waveInstance.transform.position = position;
                if (wave.progress > 1.0f) Destroy(wave.waveInstance);
            }
            waveData.RemoveAll(wave => wave.progress > 1.0f);
        }

        private void OnValidate()
        {
            float dflajl = GameManager.Instance ? 10.0f / GameManager.Instance.GameViewBounds.y : 1.0f;
            float shaderWaterSpeed = WaterSpeed * 0.5f * dflajl;
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
