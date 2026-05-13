using UnityEngine;

public class afficherFormule : MonoBehaviour
{
    public GameObject afficherText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void Toggle()
    {
        afficherText.SetActive(!afficherText.activeSelf);
    }
}