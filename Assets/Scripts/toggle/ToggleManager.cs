using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    [SerializeField] public ToggleGroup myToggleGroup;


    public ClockManager clockManager;


    // public string selectedOption;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    /*public void OnToggleValueChanged(Toggle toggle)

    {
        if (toggle.isOn)
        {
            selectedOption = toggle.name;
        }
    }*/
    private void Start()
    {
        var toggles = myToggleGroup.gameObject.GetComponentsInChildren<Toggle>(true);
        if (toggles != null)
            foreach (var toggle in toggles)
            {
                var currentToggle = toggle;
                //Lorsqu'un toggle change d'état
                currentToggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn) OnToggleSelected(currentToggle);
                    else
                        OnToggleSelected(null);
                });
                // Si sélectionner par défaut
                if (currentToggle.isOn) OnToggleSelected(currentToggle);
            }
    }


    // Update is called once per frame
    private void Update()
    {
    }

    private void OnToggleSelected(Toggle activeToggle)
    {
        if (activeToggle is null)
            clockManager.UpdateAutoPause("None");
        else clockManager.UpdateAutoPause(activeToggle.name);
    }
}