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
        public float calmBreathingRate = 0.28f;
        public float panicBreathingRate = 0.72f;
        public float verticalAmplitude = 0.018f;
        public float pitchAmplitude = 1.15f;

        [Header("Neural Headset Transfer")]
        public Transform headsetAssembly;
        public Renderer innerHudRenderer;
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

            // Setup intimate 2D audio sources for patient breathing & mechanical headset clicks
            _breathAudioSource = gameObject.AddComponent<AudioSource>();
            _breathAudioSource.spatialBlend = 0f;
            _breathAudioSource.volume = 0.55f;

            _sfxAudioSource = gameObject.AddComponent<AudioSource>();
            _sfxAudioSource.spatialBlend = 0f;
            _sfxAudioSource.volume = 0.75f;

            _calmBreathClip = GenerateBreathingClip("Patient_Calm_Breath", 3.3f, 0.32f);
            _panicBreathClip = GenerateBreathingClip("Patient_Panic_Breath", 1.35f, 0.68f);
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

            if (headsetAssembly != null)
            {
                headsetAssembly.localPosition = new Vector3(0f, 0.52f, 0.28f);
                headsetAssembly.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            float currentRate = _transferStarted
                ? Mathf.Lerp(calmBreathingRate, panicBreathingRate, Mathf.Clamp01(_transferProgress * 1.6f))
                : calmBreathingRate;

            _breathPhase += Time.deltaTime * currentRate;
            float cycleFraction = _breathPhase % 1.0f;
            float wave = Mathf.Sin(cycleFraction * Mathf.PI * 2f);

            // Trigger synchronized inhale/exhale audio at the start of each breath cycle
            if (cycleFraction < 0.15f && !_lastInhaleTriggered)
            {
                _lastInhaleTriggered = true;
                PlayBreathCycle();
            }
            else if (cycleFraction > 0.5f)
            {
                _lastInhaleTriggered = false;
            }

            // Head & chest breathing motion + subtle recoil when headset clamps onto face
            float clampRecoil = 0f;
            if (_transferStarted && _transferProgress > 0.35f && _transferProgress < 0.55f)
            {
                clampRecoil = Mathf.Sin(((_transferProgress - 0.35f) / 0.20f) * Mathf.PI) * 2.4f;
            }

            float ampScale = _transferStarted ? 1.45f : 1.0f;
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
                    ecgMonitor.beatsPerMinute = 145f;
                    ecgMonitor.scrollSpeed = 0.85f;
                }
            }

            if (_transferStarted)
            {
                _transferProgress += Time.deltaTime * 0.24f;

                // Stage 1: Lower detailed headset assembly smoothly onto face
                if (headsetAssembly != null)
                {
                    float lowerT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_transferProgress / 0.45f));
                    float posY = Mathf.Lerp(0.52f, -0.015f, lowerT);
                    float posZ = Mathf.Lerp(0.28f, 0.14f, lowerT);
                    float rotX = Mathf.Lerp(-18f, 2f, lowerT);

                    headsetAssembly.localPosition = new Vector3(0f, posY, posZ);
                    headsetAssembly.localRotation = Quaternion.Euler(rotX, 0f, 0f);

                    if (lowerT >= 0.92f && !_clampSoundPlayed)
                    {
                        _clampSoundPlayed = true;
                        if (_sfxAudioSource != null && _headsetClampClip != null)
                        {
                            _sfxAudioSource.PlayOneShot(_headsetClampClip);
                        }
                    }
                }

                // Stage 2: Illuminate inner cyan neural HUD optics after visor settles
                if (_transferProgress >= 0.42f)
                {
                    float hudT = Mathf.Clamp01((_transferProgress - 0.42f) / 0.20f);
                    float hudFadeOut = 1.0f - Mathf.Clamp01((_transferProgress - 0.72f) / 0.22f);
                    SetHudAlpha(hudT * hudFadeOut * 0.78f);
                }

                // Stage 3: ECG flatline & total blackout transition
                if (_transferProgress >= 0.74f && ecgMonitor != null)
                {
                    ecgMonitor.isFlatline = true;
                }

                float fadeAlpha = Mathf.Clamp01((_transferProgress - 0.68f) / 0.32f);
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
                _breathAudioSource.volume = _transferStarted ? 0.85f : 0.48f;
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
            Color baseC = new Color(0.1f, 0.9f, 1.0f, alpha);
            _hudMat.SetColor("_BaseColor", baseC);
            _hudMat.SetColor("_Color", baseC);
            _hudMat.SetColor("_EmissionColor", new Color(0.1f, 0.92f, 1.0f) * (alpha * 2.6f));
        }

        private AudioClip GenerateBreathingClip(string clipName, float totalDuration, float gain)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * totalDuration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(clipName.GetHashCode());
            float lowPass1 = 0f;
            float lowPass2 = 0f;

            float inhaleEnd = totalDuration * 0.42f;
            float exhaleStart = totalDuration * 0.48f;
            float exhaleEnd = totalDuration * 0.94f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);

                // Two-stage low-pass filter to shape warm organic airway noise
                float cutoff = t < inhaleEnd ? 0.085f : 0.055f;
                lowPass1 += cutoff * (white - lowPass1);
                lowPass2 += (cutoff * 0.8f) * (lowPass1 - lowPass2);

                float env = 0f;
                if (t < inhaleEnd)
                {
                    float u = t / inhaleEnd;
                    env = Mathf.Sin(u * Mathf.PI) * 0.85f;
                }
                else if (t >= exhaleStart && t <= exhaleEnd)
                {
                    float u = (t - exhaleStart) / (exhaleEnd - exhaleStart);
                    env = Mathf.Sin(u * Mathf.PI) * 1.0f;
                }

                samples[i] = Mathf.Clamp(lowPass2 * 3.8f * env * gain, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateHeadsetClampClip()
        {
            int sampleRate = 44100;
            float duration = 0.35f;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float val = 0f;

                // First mechanical latch click at 0.02s
                if (t >= 0.02f && t <= 0.07f)
                {
                    float lt = (t - 0.02f) / 0.05f;
                    val += Mathf.Sin(2f * Mathf.PI * 320f * lt) * Mathf.Exp(-lt * 18f) * 0.6f;
                }

                // Second locking click + neural power-up chime at 0.16s
                if (t >= 0.16f && t <= 0.34f)
                {
                    float lt = (t - 0.16f) / 0.18f;
                    val += Mathf.Sin(2f * Mathf.PI * 240f * lt) * Mathf.Exp(-lt * 16f) * 0.5f;
                    val += Mathf.Sin(2f * Mathf.PI * 1320f * lt) * Mathf.Exp(-lt * 8f) * 0.22f;
                }

                samples[i] = Mathf.Clamp(val, -0.95f, 0.95f);
            }

            AudioClip clip = AudioClip.Create("Headset_Clamp_SFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}