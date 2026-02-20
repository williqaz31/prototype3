using TMPro;
using UnityEngine;

public class PopUpManager : MonoBehaviour

{
    public GameObject PopUp;
    [SerializeField] public TMP_Text Title;
    [SerializeField] public TMP_Text Info;
    public string Titre;
  
    public string Information = "";
    
    public void ToggleMenu(){
        Title.text = Titre;
        Info.text = Information;
        PopUp.SetActive(!PopUp.activeSelf);
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
