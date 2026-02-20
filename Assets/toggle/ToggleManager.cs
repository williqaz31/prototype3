using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    [SerializeField] public ToggleGroup myToggleGroup;
   
    
    public clockManager clockManager;
    

   
   // public string selectedOption;

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    /*public void OnToggleValueChanged(Toggle toggle)
    
    {
        if (toggle.isOn)
        {
            selectedOption = toggle.name;
        }
    }*/
    void Start()
    {
        Toggle[] toggles = myToggleGroup.gameObject.GetComponentsInChildren<Toggle>(true);
        if (toggles != null) 
        { 
            foreach (Toggle toggle in toggles) 
            {
                Toggle currentToggle = toggle;
                //Lorsqu'un toggle change d'état
                currentToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) OnToggleSelected(currentToggle);
                    else 
                        OnToggleSelected(null);
                });
                // Si sélectionner par défaut
                if (currentToggle.isOn) OnToggleSelected(currentToggle);
                
                    
                

            } 
        }
    }

    void OnToggleSelected(Toggle activeToggle)
    {
        if (activeToggle is null)
        {
            clockManager.UpdateAutoPause("None");
            
        } else clockManager.UpdateAutoPause(activeToggle.name);
    }

   

    // Update is called once per frame
    void Update()
    {
        
    }
}
