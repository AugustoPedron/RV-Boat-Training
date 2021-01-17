﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Animator animator;
    public Image blackImage;
    public Slider loadingBar;
    public GameObject loadingBarObj;
    public GameObject loadingText;
    private AsyncOperation loading;

    private bool activeScene = false;

    void Start()
    {
         //StartCoroutine(LoadScene());
    }

    //IEnumerator LoadScene()
    //{
    //    yield return new WaitForSeconds(2);

    //    AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
    //    asyncOperation.allowSceneActivation = false;

    //    while (!asyncOperation.isDone)
    //    {
    //        //Output the current progress
    //        //m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";

    //        // Check if the load has finished
    //        if (asyncOperation.progress >= 0.9f)
    //        {
    //            //Change the Text to show the Scene is ready
    //            //m_Text.text = "Press the space bar to continue";
    //            //Wait to you press the space key to activate the Scene
    //            if (activeScene)
    //            {
    //                //Activate the Scene
    //                animator.SetBool("Fade", true);
    //                yield return new WaitUntil(() => blackImage.color.a == 1);
    //                asyncOperation.allowSceneActivation = true;
    //            }
    //        }

    //        yield return null;
    //    }
    //}

    private void Update()
    {
        if (loading != null)
        {
            float progressValue = Mathf.Clamp01(loading.progress / 0.9f);
            loadingBar.value = Mathf.Clamp01(loading.progress / 0.9f);
        }
    }

    public void LoadScene()
    {
        animator.SetBool("Fade", true);
        loadingText.SetActive(true);
        loadingBarObj.SetActive(true);
    }

    public void StartLoading()
    {
        loading = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
    }
}
