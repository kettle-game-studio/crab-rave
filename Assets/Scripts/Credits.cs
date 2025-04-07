using TMPro;
using UnityEngine;

public class Credits : MonoBehaviour
{
    public Plot plot;
    public PlayerController playerController;
    public Camera playerCamera;
    public Camera creditsCamera;
    public TextMeshProUGUI pressSpace;
    public TextMeshProUGUI credits;

    bool isGoing;

    void Start()
    {
        pressSpace.enabled = false;
        credits.enabled = false;
    }

    void Update()
    {
        if (!isGoing) return;

        credits.transform.position += new Vector3(0, 50 * Time.deltaTime, 0);

        if (playerController.jumpAction.IsPressed())
        {
            Debug.Log("Enable");
            playerController.MagicEnable();
            playerCamera.gameObject.SetActive(true);
            creditsCamera.gameObject.SetActive(false);
            plot.ReturnText();
            pressSpace.enabled = false;
            credits.enabled = false;
        }
    }

    public void StartCredits()
    {
        if (isGoing) return;
        isGoing = true;

        pressSpace.enabled = true;
        credits.enabled = true;
        plot.RemoveText();
        playerCamera.gameObject.SetActive(false);
        creditsCamera.gameObject.SetActive(true);
        playerController.MagicDisable();

        credits.text = @$"
        YOU HAVE WON!

        Thank you for playing!

        You have swum:
        
        {(int)playerController.movedWithWasd} meters with your crab-legs;
        {(int)playerController.movedWithHook} meters with your grappling hook;
        {(int)playerController.movedWithBack} meters rewinding the air hose;

        You have collected:

        {plot.shells}/{plot.totalShells} Shells;
        {plot.gems}/{plot.totalGems} Gems;

        CREDITS:

        Oleg Arutyunov
        Nicole Akopdzhanova
        Sergey Duyunov
        Viacheslav Ivanchenko
        Adam Arutyunov
        Tigran Mamedov
        ";
    }
}
