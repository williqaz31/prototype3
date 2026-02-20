using UnityEngine;
using UnityEngine.UI;

public class ToggleManager : MonoBehaviour
{
    [Header("weakly toggle")] 
    [SerializeField] public Toggle Weekly;
    [Header("monthly toggle")] 
    [SerializeField] public Toggle Mensuel;

    public int gestionPauseAuto = 7;

    private const int WEEKLY = 7;
    private const int MONTHLY = 30;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void OnToggleValueChanged(bool newValue)
    {
        if (newValue)
        {
            
        }
    }
    void Start()
    {
        Weekly.isOn = true;
        Mensuel.isOn = false;
        Weekly.onValueChanged.AddListener((isOn) => OnWeeklyChanged(isOn));
        Mensuel.onValueChanged.AddListener((isOn) => OnMonthlyChanged(isOn));
        
    }

    public void OnWeeklyChanged(bool isOn)
    {
        if (isOn)
        {
            gestionPauseAuto = WEEKLY;
        }
    }

    public void OnMonthlyChanged(bool isOn)
    {
        if (isOn)
        {
            gestionPauseAuto = MONTHLY;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
