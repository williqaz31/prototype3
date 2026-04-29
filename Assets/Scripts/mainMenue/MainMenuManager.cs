using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Dropdown dropdown;

    [SerializeField] private TMP_Text buttonText;

    
    public dropdownUI dropdownUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dropdown.onValueChanged.AddListener(SaveSelection);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SaveSelection(int index)
    {
       
        if (index == 0 )
        {
            inputField.interactable = true;
            buttonText.text = "Nouvelle partie";
        }
        else
        
        { 
            inputField.interactable = false;
            string saveName = dropdown.options[index].text;
            buttonText.text = "Charger " + saveName;
            SaveSystem.Instance.currentSaveName = saveName;

        }
    }

    public void DeleteSelection()
    {
        
        string saveName = dropdown.options[dropdown.value].text;
       
        if (saveName == null || saveName == ""||saveName == "Aucune Sauvegarde")
        {
            
        }
        else
        {
            SaveSystem.Instance.DeleteSave(saveName);
           
        }
      

        dropdownUI.RefreshDropdown();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
