using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public CanvasGroup menuCanvasGroup;
    public Animator animator;
    public LoadingScene loader;

    private bool load = false;
    private bool menuEnabled = true;

    private void OnEnable()
    {
        BoatEventManager.StartListening("endNavigation", DisableMenu);
    }

    private void OnDisable()
    {
        BoatEventManager.StopListening("endNavigation", DisableMenu);
    }

    void Update()
    {
        if (menuEnabled && Input.GetKeyDown(KeyCode.Escape))
        {
            gameIsPaused = !gameIsPaused;
            PauseGame();
        }
    }

    void PauseGame()
    {
        if (gameIsPaused)
        {
            Time.timeScale = 0;
            AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menuCanvasGroup.blocksRaycasts = true;
            animator.SetBool("Start", true);
            animator.SetBool("Fade", false);
        }
        else
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            menuCanvasGroup.blocksRaycasts = false;
            animator.SetBool("Fade", true);
            animator.SetBool("Start", false);
        }
    }

    public void FadeOut()
    {
        Cursor.lockState = CursorLockMode.None;
        animator.SetBool("Fade", true);
        animator.SetBool("Start", false);
        load = true;
    }

    public void FadeOutEvent()
    {
        if (load) loader.LoadScene();
    }

    private void DisableMenu()
    {
        BoatEventManager.StopListening("endNavigation", DisableMenu);
        menuEnabled = false;
        Time.timeScale = 0;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        menuCanvasGroup.blocksRaycasts = true;
        animator.SetBool("Start", true);
        animator.SetBool("Fade", false);
    }
}
