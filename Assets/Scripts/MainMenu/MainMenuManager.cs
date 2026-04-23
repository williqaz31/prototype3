using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Dropdown dropdown;

    [SerializeField] private TMP_Text buttonText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SaveSelection(int index)
    {
        if (index == 0)
        {
            inputField.interactable = true;
            buttonText.text = "Nouvelle partie";
        }
        else
        
        { 
            inputField.interactable = false;
            buttonText.text = "Charger " +  dropdown.options[index].text;
            
        }
    }
}
