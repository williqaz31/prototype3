using TMPro;
using UnityEngine;

public class PopUpManager : MonoBehaviour

{
    public GameObject PopUp;
    [SerializeField] public TMP_Text Title;
    [SerializeField] public TMP_Text Info;
    public string Titre;

    public string Information = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void ToggleMenu()
    {
        Title.text = Titre;
        Info.text = Information;
        PopUp.SetActive(!PopUp.activeSelf);
    }

    public void ToggleOff()
    {
        PopUp.SetActive(false);
    }
}