using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DigitalCurtain.Hospital
{
    public class PatientBreathingAndIntro : MonoBehaviour
    {
        [Header("Breathing Motion")]
        public float breathingRate = 0.28f;
        public float verticalAmplitude = 0.012f;
        public float pitchAmplitude = 0.65f;

        [Header("Neural Headset Transfer")]
        public Transform headsetVisor;
        public Renderer blackoutOverlay;
        public ECGMonitorController ecgMonitor;
        public float autoTransferDelay = 16f;
        public bool enableAutoTransfer = true;

        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;
        private float _elapsed;
        private bool _transferStarted;
        private float _transferProgress;
        private Material _fadeMat;

        private void Start()
        {
            _initialLocalPos = transform.localPosition;
            _initialLocalRot = transform.localRotation;

            if (blackoutOverlay != null)
            {
                _fadeMat = blackoutOverlay.material;
                SetFadeAlpha(0f);
            }

            if (headsetVisor != null)
            {
                headsetVisor.localPosition = new Vector3(0f, 0.35f, 0.25f);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            // Subtle breathing sway while lying on the hospital bed
            float wave = Mathf.Sin(_elapsed * Mathf.PI * 2f * breathingRate);
            transform.localPosition = _initialLocalPos + new Vector3(0f, wave * verticalAmplitude, 0f);
            transform.localRotation = _initialLocalRot * Quaternion.Euler(-wave * pitchAmplitude, 0f, 0f);

            // Manual trigger with Space key or automatic timer
            bool spacePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                spacePressed = true;
            }
#endif
            if (!_transferStarted && (spacePressed || (enableAutoTransfer && _elapsed >= autoTransferDelay)))
            {
                _transferStarted = true;
                if (ecgMonitor != null)
                {
                    ecgMonitor.beatsPerMinute = 135f;
                    ecgMonitor.scrollSpeed = 0.65f;
                }
            }

            if (_transferStarted)
            {
                _transferProgress += Time.deltaTime * 0.35f;

                // Lower neural headset visor toward eyes
                if (headsetVisor != null)
                {
                    float visorY = Mathf.Lerp(0.35f, 0.0f, Mathf.Clamp01(_transferProgress * 1.4f));
                    headsetVisor.localPosition = new Vector3(0f, visorY, 0.22f);
                }

                // Trigger flatline and blackout near the end of transfer
                if (_transferProgress >= 0.7f && ecgMonitor != null)
                {
                    ecgMonitor.isFlatline = true;
                }

                float fadeAlpha = Mathf.Clamp01((_transferProgress - 0.55f) / 0.45f);
                SetFadeAlpha(fadeAlpha);
            }
        }

        private void SetFadeAlpha(float alpha)
        {
            if (_fadeMat == null) return;
            Color c = new Color(0f, 0f, 0f, alpha);
            _fadeMat.SetColor("_BaseColor", c);
            _fadeMat.SetColor("_Color", c);
        }
    }
}