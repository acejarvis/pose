using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NuitrackSDK.VicoVRCalibration
{
    public class RGBCalibrationVisualizer : MonoBehaviour
    {
        [SerializeField] GameObject background;
        [SerializeField] GameObject connectionLostItems;
        [SerializeField] GameObject visualiserItems;

        [SerializeField] Slider progressBar;

        bool calibrationInProgress = false;

        bool calibratedOnce = false;

        bool firstCalibrationEvent = true;

        float calibrationTimeOut = 0;

        void OnEnable()
        {
            if (TPoseCalibration.Instance != null)
            {
                TPoseCalibration.Instance.onStart += ShowCalibrationScreen;
                TPoseCalibration.Instance.onSuccess += HideCalibrationScreen;
                TPoseCalibration.Instance.onProgress += ChangeProgress;
                TPoseCalibration.Instance.onFail += OnCalibrationFail;
            }

            SensorDisconnectChecker.SensorConnectionTimeOut += ShowConnectionProblem;
            SensorDisconnectChecker.SensorReconnected += HideConnectionProblem;

            StartCoroutine(FIrstStart());
        }

        private void OnCalibrationFail()
        {
            if (!calibratedOnce)
            {
                ChangeProgress(0);
            }
        }

        void ShowConnectionProblem()
        {
            background.SetActive(true);
            connectionLostItems.SetActive(true);
            visualiserItems.SetActive(false);
        }

        void HideConnectionProblem()
        {
            connectionLostItems.SetActive(false);

            background.SetActive(calibrationInProgress);
            visualiserItems.SetActive(calibrationInProgress);
        }

        void ChangeProgress(float progress)
        {
            progressBar.value = progress;

            if (progress > 0.1f)
            {
                calibrationTimeOut = 0;
                ShowCalibrationScreen();
            }
        }

        IEnumerator FIrstStart()
        {
            yield return new WaitForSeconds(0.1f);
            ShowCalibrationScreen();
            firstCalibrationEvent = true;
        }

        public void ShowCalibrationScreen()
        {
            firstCalibrationEvent = false;
            calibrationInProgress = true;

            background.SetActive(true);
            connectionLostItems.SetActive(false);
            visualiserItems.SetActive(true);
        }

        void Update()
        {
            if (!firstCalibrationEvent)
            {
                calibrationTimeOut += Time.deltaTime;
                if (calibrationTimeOut > 0.5f && calibratedOnce)
                {
                    HideCalibrationScreen(Quaternion.identity);
                    calibrationTimeOut = 0;
                    firstCalibrationEvent = true;
                }
            }

        }

        public void HideCalibrationScreen(Quaternion a)
        {
            calibrationInProgress = false;

            background.SetActive(false);
            connectionLostItems.SetActive(false);
            visualiserItems.SetActive(false);
            connectionLostItems.SetActive(false);
            calibratedOnce = true;
        }

        void OnDisable()
        {
            HideCalibrationScreen(Quaternion.identity);

            if (TPoseCalibration.Instance != null)
            {
                TPoseCalibration.Instance.onStart -= ShowCalibrationScreen;
                TPoseCalibration.Instance.onSuccess -= HideCalibrationScreen;
                TPoseCalibration.Instance.onProgress -= ChangeProgress;
                TPoseCalibration.Instance.onFail -= OnCalibrationFail;
            }

            SensorDisconnectChecker.SensorConnectionTimeOut -= ShowConnectionProblem;
            SensorDisconnectChecker.SensorReconnected -= HideConnectionProblem;
        }
    }
}
