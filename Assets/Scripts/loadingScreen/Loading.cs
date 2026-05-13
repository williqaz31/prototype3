using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public GameObject tout;
    public GameObject loadingScreen;
    public Slider progressBar;
    public Button loadButton;
    public AudioSource audioSource;

    public GameObject informationSup;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void StartLoading()
    {
        Debug.Log("StartLoading");
        loadButton.interactable = false;
        StartCoroutine(LoadRoutine());

        // tout.SetActive(false);
    }

    private IEnumerator LoadRoutine()
    {
        loadingScreen.SetActive(true);
        var duration = 5f;
        audioSource.Play();
        var time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            progressBar.value = time / duration;
            yield return null;
        }

        Time.timeScale = 0;
        audioSource.Stop();
        loadingScreen.SetActive(false);
        informationSup.SetActive(true);
        //tout.SetActive(true);
    }

    public void BackToNormal()
    {
        tout.SetActive(true);
        informationSup.SetActive(false);
        loadButton.interactable = true;
        Time.timeScale = 1;
    }
}