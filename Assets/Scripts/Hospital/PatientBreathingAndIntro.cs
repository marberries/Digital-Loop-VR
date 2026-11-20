using UnityEngine;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DigitalCurtain.Hospital
{
    public class PatientBreathingAndIntro : MonoBehaviour
    {
        [Header("Breathing Motion & Audio")]
        public float calmBreathingRate = 0.27f;
        public float panicBreathingRate = 0.78f;
        public float verticalAmplitude = 0.016f;
        public float pitchAmplitude = 1.05f;

        [Header("Neural Headset Transfer")]
        public Transform headsetAssembly;
        public Renderer innerHudRenderer;
        public Renderer innerGlassRenderer;
        public Renderer blackoutOverlay;
        public ECGMonitorController ecgMonitor;
        public float autoTransferDelay = 18f;
        public bool enableAutoTransfer = true;

        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;
        private float _elapsed;
        private float _breathPhase;
        private bool _transferStarted;
        private float _transferProgress;
        private bool _clampSoundPlayed;
        private Material _fadeMat;
        private Material _hudMat;
        private Material _glassMat;

        private AudioSource _breathAudioSource;
        private AudioSource _sfxAudioSource;
        private AudioClip _calmBreathClip;
        private AudioClip _panicBreathClip;
        private AudioClip _headsetClampClip;
        private bool _lastInhaleTriggered;

        private void Start()
        {
            if (!XRSettings.isDeviceActive)
            {
                Behaviour[] behaviours = GetComponents<Behaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] != null && behaviours[i].GetType().Name.Contains("TrackedPoseDriver"))
                    {
                        behaviours[i].enabled = false;
                    }
                }
            }

            _initialLocalPos = transform.localPosition;
            _initialLocalRot = transform.localRotation;

            _breathAudioSource = gameObject.AddComponent<AudioSource>();
            _breathAudioSource.spatialBlend = 0f;
            _breathAudioSource.volume = 0.72f;

            _sfxAudioSource = gameObject.AddComponent<AudioSource>();
            _sfxAudioSource.spatialBlend = 0f;
            _sfxAudioSource.volume = 0.80f;

            _calmBreathClip = GenerateHumanBreathingClip("Patient_Calm_Breath", 3.5f, false);
            _panicBreathClip = GenerateHumanBreathingClip("Patient_Panic_Breath", 1.25f, true);
            _headsetClampClip = GenerateHeadsetClampClip();

            if (blackoutOverlay != null)
            {
                _fadeMat = blackoutOverlay.material;
                SetFadeAlpha(0f);
            }

            if (innerHudRenderer != null)
            {
                _hudMat = innerHudRenderer.material;
                SetHudAlpha(0f);
            }

            if (innerGlassRenderer != null)
            {
                _glassMat = innerGlassRenderer.material;
                SetGlassAlpha(0f);
            }

            if (headsetAssembly != null)
            {
                headsetAssembly.localPosition = new Vector3(0.05f, 0.34f, 0.44f);
                headsetAssembly.localRotation = Quaternion.Euler(18f, -30f, 8f);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            float currentRate = _transferStarted
                ? Mathf.Lerp(calmBreathingRate, panicBreathingRate, Mathf.Clamp01(_transferProgress * 1.5f))
                : calmBreathingRate;

            _breathPhase += Time.deltaTime * currentRate;
            float cycleFraction = _breathPhase % 1.0f;

            // Asymmetric organic breathing curve: faster inhale (0..0.38), slower exhale (0.42..0.95)
            float wave = cycleFraction < 0.40f
                ? Mathf.Sin((cycleFraction / 0.40f) * Mathf.PI * 0.5f)
                : Mathf.Cos(((cycleFraction - 0.40f) / 0.60f) * Mathf.PI * 0.5f);
            wave = (wave - 0.45f) * 1.6f;

            if (cycleFraction < 0.12f && !_lastInhaleTriggered)
            {
                _lastInhaleTriggered = true;
                PlayBreathCycle();
            }
            else if (cycleFraction > 0.5f)
            {
                _lastInhaleTriggered = false;
            }

            // Head jolt when headset locks onto temples
            float clampRecoil = 0f;
            if (_transferStarted && _transferProgress >= 0.54f && _transferProgress <= 0.68f)
            {
                clampRecoil = Mathf.Sin(((_transferProgress - 0.54f) / 0.14f) * Mathf.PI) * 2.6f;
            }

            float ampScale = _transferStarted ? 1.35f : 1.0f;
            transform.localPosition = _initialLocalPos + new Vector3(0f, wave * verticalAmplitude * ampScale, 0f);
            transform.localRotation = _initialLocalRot * Quaternion.Euler(-wave * pitchAmplitude * ampScale + clampRecoil, 0f, 0f);

            bool triggerPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                triggerPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Space))
            {
                triggerPressed = true;
            }
#endif
            if (!_transferStarted && (triggerPressed || (enableAutoTransfer && _elapsed >= autoTransferDelay)))
            {
                _transferStarted = true;
                _lastInhaleTriggered = false;
                if (ecgMonitor != null)
                {
                    ecgMonitor.beatsPerMinute = 142f;
                    ecgMonitor.scrollSpeed = 0.82f;
                }
            }

            if (_transferStarted)
            {
                _transferProgress += Time.deltaTime * 0.19f;

                if (headsetAssembly != null)
                {
                    if (_transferProgress <= 0.36f)
                    {
                        // Phase 1: Lower headset into 3/4 view so player sees the detailed exterior hardware
                        float t = Mathf.SmoothStep(0f, 1f, _transferProgress / 0.36f);
                        headsetAssembly.localPosition = Vector3.Lerp(
                            new Vector3(0.05f, 0.34f, 0.44f),
                            new Vector3(0.02f, 0.01f, 0.36f),
                            t
                        );
                        headsetAssembly.localRotation = Quaternion.Slerp(
                            Quaternion.Euler(18f, -30f, 8f),
                            Quaternion.Euler(10f, -22f, 5f),
                            t
                        );
                    }
                    else if (_transferProgress <= 0.60f)
                    {
                        // Phase 2: Rotate straight and pull onto the eyes
                        float t = Mathf.SmoothStep(0f, 1f, (_transferProgress - 0.36f) / 0.24f);
                        headsetAssembly.localPosition = Vector3.Lerp(
                            new Vector3(0.02f, 0.01f, 0.36f),
                            new Vector3(0f, 0f, -0.02f),
                            t
                        );
                        headsetAssembly.localRotation = Quaternion.Slerp(
                            Quaternion.Euler(10f, -22f, 5f),
                            Quaternion.identity,
                            t
                        );

                        if (t >= 0.88f && !_clampSoundPlayed)
                        {
                            _clampSoundPlayed = true;
                            if (_sfxAudioSource != null && _headsetClampClip != null)
                            {
                                _sfxAudioSource.PlayOneShot(_headsetClampClip);
                            }
                        }
                    }
                    else
                    {
                        headsetAssembly.localPosition = new Vector3(0f, 0f, -0.02f);
                        headsetAssembly.localRotation = Quaternion.identity;
                    }
                }

                // Phase 3: Activate tinted visor glass & glowing cyan binocular HUD while ward is still visible
                if (_transferProgress >= 0.56f)
                {
                    float bootT = Mathf.Clamp01((_transferProgress - 0.56f) / 0.14f);
                    float outT = 1.0f - Mathf.Clamp01((_transferProgress - 0.80f) / 0.16f);
                    SetGlassAlpha(bootT * 0.32f);
                    SetHudAlpha(bootT * outT * 0.85f);
                }

                if (_transferProgress >= 0.78f && ecgMonitor != null)
                {
                    ecgMonitor.isFlatline = true;
                }

                float fadeAlpha = Mathf.Clamp01((_transferProgress - 0.76f) / 0.24f);
                SetFadeAlpha(fadeAlpha);
            }
        }

        private void PlayBreathCycle()
        {
            if (_breathAudioSource == null) return;
            AudioClip clipToPlay = _transferStarted ? _panicBreathClip : _calmBreathClip;
            if (clipToPlay != null)
            {
                _breathAudioSource.Stop();
                _breathAudioSource.volume = _transferStarted ? 0.92f : 0.68f;
                _breathAudioSource.PlayOneShot(clipToPlay);
            }
        }

        private void SetFadeAlpha(float alpha)
        {
            if (_fadeMat == null) return;
            Color c = new Color(0f, 0f, 0f, alpha);
            _fadeMat.SetColor("_BaseColor", c);
            _fadeMat.SetColor("_Color", c);
        }

        private void SetHudAlpha(float alpha)
        {
            if (_hudMat == null) return;
            Color baseC = new Color(0.1f, 0.92f, 1.0f, alpha);
            _hudMat.SetColor("_BaseColor", baseC);
            _hudMat.SetColor("_Color", baseC);
            _hudMat.SetColor("_EmissionColor", new Color(0.1f, 0.94f, 1.0f) * (alpha * 2.8f));
        }

        private void SetGlassAlpha(float alpha)
        {
            if (_glassMat == null) return;
            Color tint = new Color(0.03f, 0.08f, 0.12f, alpha);
            _glassMat.SetColor("_BaseColor", tint);
            _glassMat.SetColor("_Color", tint);
        }

        private AudioClip GenerateHumanBreathingClip(string clipName, float totalDuration, bool isPanic)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * totalDuration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(clipName.GetHashCode());

            // 3 resonant biquad bandpass filters modeling human vocal/nasal tract formants
            BiquadBandpass f1 = new BiquadBandpass(); // Chest / pharynx resonance (~480 Hz)
            BiquadBandpass f2 = new BiquadBandpass(); // Oral / nasal cavity (~1180 Hz)
            BiquadBandpass f3 = new BiquadBandpass(); // Glottal / nostril air friction (~2450 Hz)

            float inhaleEnd = totalDuration * (isPanic ? 0.42f : 0.36f);
            float exhaleStart = totalDuration * (isPanic ? 0.46f : 0.43f);
            float exhaleEnd = totalDuration * (isPanic ? 0.94f : 0.90f);

            float prevNoise = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                // Pink-ish spectral tilt so air sounds warm rather than harsh
                float pink = 0.65f * prevNoise + 0.35f * white;
                prevNoise = pink;

                float sample = 0f;

                if (t < inhaleEnd)
                {
                    // Inhale: sharper nasal/mouth intake with rising formant pitch
                    float u = t / inhaleEnd;
                    float env = Mathf.Pow(Mathf.Sin(u * Mathf.PI), 0.75f);

                    f1.SetParams(sampleRate, Mathf.Lerp(460f, 540f, u), 4.2f);
                    f2.SetParams(sampleRate, Mathf.Lerp(1150f, 1320f, u), 5.0f);
                    f3.SetParams(sampleRate, Mathf.Lerp(2350f, 2650f, u), 5.5f);

                    float voice = f1.Process(pink) * 0.55f + f2.Process(pink) * 0.85f + f3.Process(pink) * 0.55f;
                    sample = voice * env * (isPanic ? 2.4f : 1.75f);
                }
                else if (t >= exhaleStart && t <= exhaleEnd)
                {
                    // Exhale: deeper chest-resonant sigh with soft vocal cord warmth
                    float u = (t - exhaleStart) / (exhaleEnd - exhaleStart);
                    float env = Mathf.Sin(u * Mathf.PI) * Mathf.Lerp(1.15f, 0.65f, u);

                    f1.SetParams(sampleRate, Mathf.Lerp(440f, 360f, u), 3.8f);
                    f2.SetParams(sampleRate, Mathf.Lerp(1020f, 880f, u), 4.5f);
                    f3.SetParams(sampleRate, Mathf.Lerp(2100f, 1850f, u), 5.0f);

                    // Subtle vocal cord glottal pulse during exhale
                    float glottalFreq = isPanic ? 135f : 92f;
                    float glottal = Mathf.Sin(2f * Mathf.PI * glottalFreq * t) * 0.045f * (1f - u);

                    float voice = f1.Process(pink + glottal) * 0.95f + f2.Process(pink) * 0.65f + f3.Process(pink) * 0.30f;
                    sample = voice * env * (isPanic ? 2.6f : 1.95f);
                }

                samples[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateHeadsetClampClip()
        {
            int sampleRate = 44100;
            float duration = 0.42f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float val = 0f;

                if (t >= 0.01f && t <= 0.06f)
                {
                    float lt = (t - 0.01f) / 0.05f;
                    val += Mathf.Sin(2f * Mathf.PI * 360f * lt) * Mathf.Exp(-lt * 20f) * 0.65f;
                }

                if (t >= 0.14f && t <= 0.21f)
                {
                    float lt = (t - 0.14f) / 0.07f;
                    val += Mathf.Sin(2f * Mathf.PI * 240f * lt) * Mathf.Exp(-lt * 16f) * 0.60f;
                }

                // Neural boot chime
                if (t >= 0.18f && t <= 0.41f)
                {
                    float lt = (t - 0.18f) / 0.23f;
                    val += Mathf.Sin(2f * Mathf.PI * 1046f * lt) * Mathf.Exp(-lt * 7f) * 0.24f;
                    val += Mathf.Sin(2f * Mathf.PI * 1568f * lt) * Mathf.Exp(-lt * 9f) * 0.16f;
                }

                samples[i] = Mathf.Clamp(val, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("Headset_Clamp_SFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private struct BiquadBandpass
        {
            private float _b0, _b1, _b2, _a1, _a2;
            private float _x1, _x2, _y1, _y2;

            public void SetParams(float sampleRate, float centerFreq, float q)
            {
                float omega = 2f * Mathf.PI * centerFreq / sampleRate;
                float sinW = Mathf.Sin(omega);
                float cosW = Mathf.Cos(omega);
                float alpha = sinW / (2f * q);

                float a0 = 1f + alpha;
                _b0 = alpha / a0;
                _b1 = 0f;
                _b2 = -alpha / a0;
                _a1 = (-2f * cosW) / a0;
                _a2 = (1f - alpha) / a0;
            }

            public float Process(float x)
            {
                float y = _b0 * x + _b1 * _x1 + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;
                _x2 = _x1;
                _x1 = x;
                _y2 = _y1;
                _y1 = y;
                return y;
            }
        }
    }
}