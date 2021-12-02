using UnityEngine;
using UnityEngine.UI;

public class UIFaceInfo : MonoBehaviour
{
    [Header("Info")]
    public bool autoProcessing;
    [SerializeField] bool showInfo = true;
    [SerializeField] GameObject infoPanel;
    [SerializeField] Text ageText;
    [SerializeField] Text yearsText;
    [SerializeField] Text genderText;
    [SerializeField] Slider neutral;
    [SerializeField] Slider angry;
    [SerializeField] Slider surprise;
    [SerializeField] Slider happy;

    RectTransform spawnTransform;
    RectTransform frameTransform;
    Image image;

    Instances[] instances;

    JsonInfo jsonInfo;

    public void Initialize(RectTransform spawnTransform)
    {
        this.spawnTransform = spawnTransform;

        frameTransform = GetComponent<RectTransform>();
        image = frameTransform.GetComponent<Image>();
    }

    void Update()
    {
        if (autoProcessing)
        {
            ProcessFace(CurrentUserTracker.CurrentSkeleton);
        }
    }

    public void ProcessFace(nuitrack.Skeleton skeleton)
    {
        jsonInfo = NuitrackManager.NuitrackJson;

        if (jsonInfo == null)
            return;

        if (!NuitrackManager.Instance.UseFaceTracking)
            Debug.Log("Attention: Face tracking disabled! Enable it on the Nuitrack Manager component");

        instances = jsonInfo.Instances;
        for (int i = 0; i < instances.Length; i++)
        {
            if (instances != null && i < instances.Length && skeleton.ID == instances[i].id)
            {
                Face currentFace = instances[i].face;

                if (skeleton != null && currentFace.rectangle != null && spawnTransform)
                {
                    image.enabled = true;
                    infoPanel.SetActive(showInfo);

                    Vector2 newPosition = new Vector2(
                        spawnTransform.rect.width * (Mathf.Clamp01(currentFace.rectangle.left) - 0.5f) + frameTransform.rect.width / 2,
                        spawnTransform.rect.height * (0.5f - Mathf.Clamp01(currentFace.rectangle.top)) - frameTransform.rect.height / 2);

                    frameTransform.sizeDelta = new Vector2(currentFace.rectangle.width * spawnTransform.rect.width, currentFace.rectangle.height * spawnTransform.rect.height);
                    frameTransform.anchoredPosition = newPosition;

                    ageText.text = currentFace.age.type;
                    yearsText.text = string.Format("Years: {0:F1}", currentFace.age.years);
                    genderText.text = currentFace.gender;

                    neutral.value = currentFace.emotions.neutral;
                    angry.value = currentFace.emotions.angry;
                    surprise.value = currentFace.emotions.surprise;
                    happy.value = currentFace.emotions.happy;
                }
                else
                {
                    image.enabled = false;
                    infoPanel.SetActive(false);
                }
            }
        }
    }
}
