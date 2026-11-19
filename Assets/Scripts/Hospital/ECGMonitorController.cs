 using UnityEngine;

namespace DigitalCurtain.Hospital
{
    public class ECGMonitorController : MonoBehaviour
    {
        [Header("Screen & Indicator References")]
        public Renderer screenRenderer;
        public Renderer ledRenderer;

        [Header("Heartbeat Settings")]
        public float beatsPerMinute = 64f;
        public float scrollSpeed = 0.35f;
        public bool isFlatline = false;

        private AudioSource _audioSource;
        private AudioClip _beepClip;
        private AudioClip _flatlineClip;
        private float _uvOffsetX;
        private float _beatTimer;
        private Material _screenMat;
        private Material _ledMat;
        private bool _flatlinePlaying;

        private void Awake()
        {
            if (screenRenderer == null)
            {
                Transform screenTr = transform.Find("Terminal_Monitor_Head/Monitor_Screen");
                if (screenTr != null) screenRenderer = screenTr.GetComponent<Renderer>();
            }

            if (ledRenderer == null)
            {
                Transform ledTr = transform.Find("Terminal_Monitor_Head/Monitor_PowerLED");
                if (ledTr != null) ledRenderer = ledTr.GetComponent<Renderer>();
            }
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            _audioSource.spatialBlend = 1.0f;
            _audioSource.volume = 0.5f;

            _beepClip = GenerateToneClip("ECG_Beep", 880f, 0.11f);
            _flatlineClip = GenerateToneClip("ECG_Flatline", 920f, 1.0f);

            if (screenRenderer != null)
            {
                _screenMat = screenRenderer.material;
                ConfigureRepeatTexture(_screenMat, "_BaseMap");
                ConfigureRepeatTexture(_screenMat, "_MainTex");
                ConfigureRepeatTexture(_screenMat, "_EmissionMap");
            }

            if (ledRenderer != null)
            {
                _ledMat = ledRenderer.material;
            }
        }

        private void Update()
        {
            if (_screenMat != null)
            {
                _uvOffsetX = (_uvOffsetX - Time.deltaTime * scrollSpeed) % 1.0f;
                Vector2 offset = new Vector2(_uvOffsetX, 0f);
                _screenMat.SetTextureOffset("_BaseMap", offset);
                _screenMat.SetTextureOffset("_MainTex", offset);
                _screenMat.SetTextureOffset("_EmissionMap", offset);
            }

            if (isFlatline)
            {
                HandleFlatline();
                return;
            }

            float beatInterval = 60f / Mathf.Max(20f, beatsPerMinute);
            _beatTimer += Time.deltaTime;

            if (_beatTimer >= beatInterval)
            {
                _beatTimer = 0f;
                TriggerHeartbeatPulse();
            }

            float pulseRatio = Mathf.Clamp01(_beatTimer / (beatInterval * 0.55f));
            float pulseGlow = Mathf.Lerp(3.8f, 0.45f, pulseRatio);

            if (_ledMat != null)
            {
                _ledMat.SetColor("_EmissionColor", new Color(0.1f, 1.0f, 0.4f) * pulseGlow);
            }

            if (_screenMat != null)
            {
                float screenGlow = Mathf.Lerp(1.65f, 1.15f, pulseRatio);
                _screenMat.SetColor("_EmissionColor", Color.white * screenGlow);
            }
        }

        private void ConfigureRepeatTexture(Material mat, string propName)
        {
            if (mat != null && mat.HasProperty(propName))
            {
                Texture tex = mat.GetTexture(propName);
                if (tex != null)
                {
                    tex.wrapMode = TextureWrapMode.Repeat;
                }
            }
        }

        private void TriggerHeartbeatPulse()
        {
            if (_audioSource != null && _beepClip != null)
            {
                _audioSource.PlayOneShot(_beepClip);
            }
        }

        private void HandleFlatline()
        {
            if (_ledMat != null)
            {
                _ledMat.SetColor("_EmissionColor", new Color(1.0f, 0.15f, 0.1f) * 3.5f);
            }

            if (!_flatlinePlaying && _audioSource != null && _flatlineClip != null)
            {
                _flatlinePlaying = true;
                _audioSource.clip = _flatlineClip;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }

        private AudioClip GenerateToneClip(string clipName, float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1.0f;
                if (duration < 0.5f)
                {
                    float fadeIn = Mathf.Clamp01(t / 0.01f);
                    float fadeOut = Mathf.Clamp01((duration - t) / 0.02f);
                    envelope = fadeIn * fadeOut;
                }
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.35f * envelope;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}