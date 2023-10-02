using System.Collections.Generic;
using LD54.Game;
using UnityEngine;

namespace LD54.Ocean
{
    public class Ocean : MonoBehaviour
    {
        public float WaterSpeed = 3.0f;
        public GameObject WavePrefab;
        public Material OvalWaveMaterial;
        public Material TrailWaveMaterial;
        public Material PurpleTrailWaveMaterial;

        public enum Shape { Circular, Oval, Trail, PurpleTrail }

        [ContextMenu("Create Wave")]
        public void CreateWave()
        {
            CreateWave(new Vector2(2.0f, 2.0f), 10.0f, 3.0f);
        }

        public void CreateWave(
            Vector2 worldPosition, float progress = 0.0f, float radius = 2.0f,
            float duration = 1.0f, float y_stretch = 1.0f, Shape shape = Shape.Circular
        )
        {
            WaveData wave = new();
            wave.progress = progress;
            wave.duration = duration;
            wave.waveInstance = Instantiate(WavePrefab);
            wave.waveInstance.transform.position = worldPosition;
            wave.waveInstance.transform.localScale = new Vector3(radius, radius * y_stretch, 1.0f);
            switch (shape)
            {
                case Shape.Oval:
                    wave.waveInstance.GetComponent<SpriteRenderer>().material = OvalWaveMaterial;
                    break;
                case Shape.Trail:
                    wave.waveInstance.GetComponent<SpriteRenderer>().material = TrailWaveMaterial;
                    break;
                case Shape.PurpleTrail:
                    wave.waveInstance.GetComponent<SpriteRenderer>().material = PurpleTrailWaveMaterial;
                    break;
            }
            wave.waveInstance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Pixels", radius * 15.0f); // one "pixel" is 15 pixels big
            wave.waveInstance.GetComponent<SpriteRenderer>().material
                .SetFloat("_Wave_Width", 1.5f / radius);
            wave.waveInstance.transform.parent = this.transform;
            waveData.Add(wave);
        }

        private List<WaveData> waveData = new();

        public SpriteRenderer OceanRenderer1;
        public SpriteRenderer OceanRenderer2;
        public SpriteRenderer WavesRenderer;
        private Material oceanMaterial1 { get { return OceanRenderer1.sharedMaterial; } }
        private Material oceanMaterial2 { get { return OceanRenderer2.sharedMaterial; } }
        private Material wavesMaterial { get { return WavesRenderer.sharedMaterial; } }

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
                // if the water is slow, it looks better if the wave moves only with a fraction of the water speed
                position.x += Time.deltaTime * WaterSpeed * 1.0f;
                wave.waveInstance.transform.position = position;
                if (wave.progress > 1.0f) Destroy(wave.waveInstance);
            }
            waveData.RemoveAll(wave => wave.progress > 1.0f);
        }

        private void OnValidate()
        {
            float dflajl = GameManager.Instance ? 10.0f / GameManager.Instance.GameViewBounds.y : 1.0f;
            float shaderWaterSpeed = WaterSpeed * 0.5f * dflajl;
            oceanMaterial1.SetFloat("_Scroll_Speed", shaderWaterSpeed);
            oceanMaterial2.SetFloat("_Scroll_Speed", shaderWaterSpeed);
            wavesMaterial.SetFloat("_Scroll_Speed", shaderWaterSpeed);
        }

        private class WaveData
        {
            public float progress;
            public float duration;
            public GameObject waveInstance;
        }
    }
}
