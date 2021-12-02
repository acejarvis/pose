using UnityEngine.UI;
using UnityEngine;

public class PunchSpeedMeter : MonoBehaviour {

    [SerializeField] Text speedMeterText;
    [SerializeField] GameObject dummy;
    [SerializeField] Transform transformTarget;

    float maximumPunchSpeed = 0;

    void Awake()
    {
        dummy.SetActive(false);
    }

    void OnEnable()
    {
        TPoseCalibration.Instance.onSuccess += OnSuccessCalibration;
    }

    void OnSuccessCalibration(Quaternion rotation)
    {
        dummy.SetActive(true);
        transform.position = transformTarget.position + new Vector3(0, -1, 1);
    }

    public void CalculateMaxPunchSpeed(float speed)
    {
        if (maximumPunchSpeed < speed)
            maximumPunchSpeed = speed;
        speedMeterText.text = maximumPunchSpeed.ToString("f2") + " m/s";
    }

    void OnDisable()
    {
        if(TPoseCalibration.Instance)
            TPoseCalibration.Instance.onSuccess -= OnSuccessCalibration;
    }
}
