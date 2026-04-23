using UnityEngine;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
public class dropdownUI : MonoBehaviour
{
    
    [SerializeField] private TMP_Dropdown dropdown;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshDropdown();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void RefreshDropdown()
    {
        Debug.Log("Refresh ");
        dropdown.ClearOptions();

        string[] saves = SaveSystem.GetAllSaves();
        
        List<string> options = new List<string>(saves);
        if (options.Count == 0)
        {
            Debug.Log("Aucune save");
            options.Add("Aucune Sauvegarde");
        }
       
        dropdown.AddOptions(options);
    }
    
}
