using UnityEngine;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DigitalCurtain.Hospital
{
    public class PatientBreathingAndIntro : MonoBehaviour
    {
        [Header("Breathing Motion")]
        public float breathingRate = 0.30f;
        public float verticalAmplitude = 0.018f;
        public float pitchAmplitude = 1.1f;

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
            // Release camera lock when testing on Mac without an active VR headset
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

            if (blackoutOverlay != null)
            {
                _fadeMat = blackoutOverlay.material;
                SetFadeAlpha(0f);
            }

            if (headsetVisor != null)
            {
                headsetVisor.localPosition = new Vector3(0f, 0.45f, 0.42f);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            float wave = Mathf.Sin(_elapsed * Mathf.PI * 2f * breathingRate);
            transform.localPosition = _initialLocalPos + new Vector3(0f, wave * verticalAmplitude, 0f);
            transform.localRotation = _initialLocalRot * Quaternion.Euler(-wave * pitchAmplitude, 0f, 0f);

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
                if (ecgMonitor != null)
                {
                    ecgMonitor.beatsPerMinute = 140f;
                    ecgMonitor.scrollSpeed = 0.80f;
                }
            }

            if (_transferStarted)
            {
                _transferProgress += Time.deltaTime * 0.35f;

                if (headsetVisor != null)
                {
                    float visorY = Mathf.Lerp(0.45f, 0.0f, Mathf.Clamp01(_transferProgress * 1.4f));
                    headsetVisor.localPosition = new Vector3(0f, visorY, 0.42f);
                }

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