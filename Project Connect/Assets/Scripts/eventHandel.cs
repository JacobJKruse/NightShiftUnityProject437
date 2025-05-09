using UnityEngine;
using TMPro;

public class eventHandel : MonoBehaviour
{
    public GameObject player;
    public TextMeshProUGUI promptText;

    public GameObject daycareTrigger;
    public Camera mainCam;
    public Camera daycareCam;

    public GameObject rewardObject; //  New GameObject to enable at 24 tokens

    public PlayerControl playerControl;
    private bool isSwitchingCam = false;
    private float camLerpSpeed = 2f;
    private bool isPlayerInEBox = false;

    private bool hasSwitchedCam = false;
    private bool playerEnteredDaycare = false;

    void Start()
    {
        Application.targetFrameRate = 60;

        if (promptText != null) promptText.enabled = false;
        if (player != null) playerControl = player.GetComponent<PlayerControl>();

        if (daycareCam != null) daycareCam.enabled = false;
        if (mainCam != null) mainCam.enabled = true;

        if (daycareTrigger != null) daycareTrigger.SetActive(false);
        if (rewardObject != null) rewardObject.SetActive(false); //  Disable reward object at start
    }

    void Update()
    {
        if (playerControl.tokens >= 24)
            promptText.text = "Return to Daycare";

        if (playerControl != null && playerControl.tokens >= 24)
        {
            if (!daycareTrigger.activeSelf)
            {
                daycareTrigger.SetActive(true);
                Debug.Log("Daycare trigger activated");
            }

            if (rewardObject != null && !rewardObject.activeSelf)
            {
                rewardObject.SetActive(true); //  Enable reward object at 24 tokens
                Debug.Log("Reward object enabled");
            }
        }

        if (isSwitchingCam && playerEnteredDaycare && daycareCam != null && mainCam != null && !hasSwitchedCam)
        {
            mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, daycareCam.transform.position, Time.deltaTime * camLerpSpeed);
            mainCam.transform.rotation = Quaternion.Lerp(mainCam.transform.rotation, daycareCam.transform.rotation, Time.deltaTime * camLerpSpeed);
        }

        if (!isPlayerInEBox && promptText != null && playerControl != null && playerControl.tokens < 24)
        {
            promptText.text = $"{playerControl.tokens}/24 tokens";
            promptText.enabled = true;
        }
    }

    public void HandleTriggerEnter(Collider other)
    {
        if (other.gameObject == player && promptText != null && playerControl != null)
        {
            isPlayerInEBox = true;

            Debug.Log("Player entered EBox trigger");
            promptText.text = "Press E to open door";
            promptText.enabled = true;
        }

        if (other.gameObject == player && daycareTrigger.activeSelf)
        {
            Debug.Log("Player entered daycare trigger");

            playerEnteredDaycare = true;
            player.SetActive(false);
            isSwitchingCam = true;
            hasSwitchedCam = false;
            Invoke(nameof(SwitchToDaycareCam), 1f);
        }
    }

    public void HandleTriggerExit(Collider other)
    {
        if (other.gameObject == player && promptText != null && playerControl != null)
        {
            isPlayerInEBox = false;

            Debug.Log("Player exited EBox trigger");

            if (playerControl.tokens < 24)
            {
                promptText.text = $"{playerControl.tokens}/24 tokens";
                promptText.enabled = true;
            }
            else
            {
                promptText.enabled = false;
            }
        }
    }

    private void SwitchToDaycareCam()
    {
        if (daycareCam != null && mainCam != null)
        {
            daycareCam.enabled = true;
            mainCam.enabled = false;
            isSwitchingCam = false;
            hasSwitchedCam = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("Switched to daycare camera");
        }
    }
}
