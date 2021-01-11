using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public Animator animator;
    public Image blackImage;

    private bool activeScene = false;

    void Start()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(2);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            //Output the current progress
            //m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                //Change the Text to show the Scene is ready
                //m_Text.text = "Press the space bar to continue";
                //Wait to you press the space key to activate the Scene
                if (activeScene)
                {
                    //Activate the Scene
                    animator.SetBool("Fade", true);
                    yield return new WaitUntil(() => blackImage.color.a == 1);
                    asyncOperation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    public void SetActiveScene()
    {
        activeScene = true;
    }
}
