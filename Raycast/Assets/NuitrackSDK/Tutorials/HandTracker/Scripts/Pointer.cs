using UnityEngine;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    public enum Hands { left = 0, right = 1 };

    [SerializeField]
    Hands currentHand;

    [Header ("Visualization")]
    [SerializeField]
    RectTransform parentRectTransform;

    [SerializeField]
    RectTransform baseRect;

    [SerializeField]
    Image background;

    [SerializeField]
    Sprite defaultSprite;

    [SerializeField]
    Sprite pressSprite;

    [SerializeField]
    [Range(0, 50)]
    float minVelocityInteractivePoint = 2f;

    float lastTime = 0;
    bool active = false;

    private void Start()
    {
        NuitrackManager.onHandsTrackerUpdate += NuitrackManager_onHandsTrackerUpdate;
        lastTime = Time.time;
    }

    private void OnDestroy()
    {
        NuitrackManager.onHandsTrackerUpdate -= NuitrackManager_onHandsTrackerUpdate;
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public bool Press
    {
        get; private set;
    }

    private void NuitrackManager_onHandsTrackerUpdate(nuitrack.HandTrackerData handTrackerData)
    {
        active = false;

        nuitrack.UserHands userHands = handTrackerData.GetUserHandsByID(CurrentUserTracker.CurrentUser);    

        if (userHands != null)
        {
            nuitrack.HandContent? handContent = currentHand == Hands.right ? userHands.RightHand : userHands.LeftHand;

            if (handContent != null)
            {
                Vector2 pageSize = parentRectTransform.rect.size;
                Vector3 lastPosition = baseRect.position;
                baseRect.anchoredPosition = new Vector2(handContent.Value.X * pageSize.x, -handContent.Value.Y * pageSize.y);

                float velocity = (baseRect.position - lastPosition).magnitude / (Time.time - lastTime);

                if (velocity < minVelocityInteractivePoint)
                    Press = handContent.Value.Click;

                active = true;
            }
        }

        Press = Press && active;

        lastTime = Time.time;
        background.enabled = active;
        background.sprite = active && Press ? pressSprite : defaultSprite;
    }
}
