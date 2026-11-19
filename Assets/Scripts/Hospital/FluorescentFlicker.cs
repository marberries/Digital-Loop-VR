using UnityEngine;

namespace DigitalCurtain.Hospital
{
    public class FluorescentFlicker : MonoBehaviour
    {
        public Light fixtureLight;
        public Renderer[] tubeRenderers;

        [Header("Flicker Timing")]
        public float minStableDuration = 3.0f;
        public float maxStableDuration = 8.5f;
        public float flickerBurstDuration = 0.45f;

        private float _baseIntensity = 2.4f;
        private float _timer;
        private bool _isFlickering;
        private Material[] _tubeMaterials;
        private Color _tubeEmissionColor = new Color(0.92f, 0.97f, 1.0f) * 2.8f;

        private void Start()
        {
            if (fixtureLight != null)
            {
                _baseIntensity = fixtureLight.intensity;
            }

            if (tubeRenderers != null && tubeRenderers.Length > 0)
            {
                _tubeMaterials = new Material[tubeRenderers.Length];
                for (int i = 0; i < tubeRenderers.Length; i++)
                {
                    if (tubeRenderers[i] != null)
                    {
                        _tubeMaterials[i] = tubeRenderers[i].material;
                    }
                }
            }

            _timer = Random.Range(minStableDuration, maxStableDuration);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                _isFlickering = !_isFlickering;
                _timer = _isFlickering
                    ? Random.Range(0.15f, flickerBurstDuration)
                    : Random.Range(minStableDuration, maxStableDuration);

                if (!_isFlickering)
                {
                    SetFixtureState(1.0f);
                }
            }

            if (_isFlickering)
            {
                float factor = Random.value > 0.45f ? Random.Range(0.1f, 0.4f) : 1.0f;
                SetFixtureState(factor);
            }
        }

        private void SetFixtureState(float multiplier)
        {
            if (fixtureLight != null)
            {
                fixtureLight.intensity = _baseIntensity * multiplier;
            }

            if (_tubeMaterials != null)
            {
                for (int i = 0; i < _tubeMaterials.Length; i++)
                {
                    if (_tubeMaterials[i] != null)
                    {
                        _tubeMaterials[i].SetColor("_EmissionColor", _tubeEmissionColor * multiplier);
                    }
                }
            }
        }
    }
}