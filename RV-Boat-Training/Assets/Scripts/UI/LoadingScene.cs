using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Animator animator;
    public Image blackImage;
    public string sceneName;
    public List<Slider> loadingBar;
    public List<GameObject> loadingPanel;
    private AsyncOperation loading;

    private bool activeScene = false;
    private string actualScene;
    private bool reloadScene = false;

    private void OnEnable()
    {
        BoatEventManager.StartListening("reloadScene", ReloadScene);
    }

    private void OnDisable()
    {
        BoatEventManager.StopListening("reloadScene", ReloadScene);
    }

    private void Awake()
    {
        //Application.targetFrameRate = 60;
        //QualitySettings.vSyncCount = 1;
        Scene scene = SceneManager.GetActiveScene();
        actualScene = scene.name;
        reloadScene = false;
    }

    private void Update()
    {
        if (loading != null && loadingBar.Count > 0)
        {
            float progressValue = Mathf.Clamp01(loading.progress / 0.9f);
            loadingBar[0].value = Mathf.Clamp01(loading.progress / 0.9f);
        }
    }

    public void LoadScene()
    {
        animator.SetBool("Fade", true);
        if (loadingPanel.Count > 0)
        {
            loadingPanel[0].SetActive(true);
        }
    }

    public void StartLoading()
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        if(reloadScene)
            loading = SceneManager.LoadSceneAsync(actualScene, LoadSceneMode.Single);
        else
            loading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    public void ReloadScene()
    {
        BoatEventManager.StopListening("reloadScene", ReloadScene);
        reloadScene = true;
        LoadScene();
    }
}
