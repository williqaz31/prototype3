using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dropdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    [SerializeField] private Button deleteButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Invoke(nameof(RefreshDropdown), 0.1f);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void RefreshDropdown()
    {
        int newIndex;


        dropdown.ClearOptions();

        var saves = SaveSystem.GetAllSaves();

        var options = new List<string>(saves);
        if (options.Count == 0)
        {
            options.Add("Aucune Sauvegarde");
            deleteButton.interactable = false;
        }
        else
        {
            options.Insert(0, "");
            deleteButton.interactable = true;
        }


        newIndex = dropdown.value;
        dropdown.onValueChanged.Invoke(newIndex);

        dropdown.AddOptions(options);
    }
}