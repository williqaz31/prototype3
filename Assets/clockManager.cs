using TMPro;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;


public class clockManager : MonoBehaviour
{
    [SerializeField] private TMP_Text clock;
    private float elapsedTime;
    [SerializeField] private Slider slider;
    [SerializeField] private pausePlay pausePlay;
    [SerializeField] private ToggleManager  UpdateCycle;
    
    private float multiplier = 1.0f;
    public popCounter popCounter;
    
    
    private const float SECONDS_IN_DAY = 86400f;
    public int day = 0;
    private int week = 0;
    private int month = 0;
    private bool newWeek;
    private bool newMonth;


    private int? intervalPauseAuto;
    const int WEEKLY_PAUSE = 7;
    const int MONTHLY_PAUSE = 30;
        

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime *  multiplier;
     
        UpdateClockUI();

    }

    void UpdateClockUI()
    {
        int days = Mathf.FloorToInt(elapsedTime / SECONDS_IN_DAY);
        if (days > day)
        {
            day = days;
            popCounter.NoveauJour();
        }

        if (days != 0)
        {
            if (days % 7 == 0 && days / 7 != week)
            {
                week++;
                newWeek = true;
                string info = popCounter.EndWeek();
                pausePlay.PopUp.Information = info;
                /* if (pausePlay.PopUp.Titre == "Bilan hebdomadaire")
                 {
                     pausePlay.TogglePopUp();
                 }*/


            }
            else newWeek = false;
            
            if (days % 30 == 0 && days / 30 != month)
            {
                month++;
                newMonth = true;
                
            } 
            else newMonth = false;
            
            
        }
       
        

        if (intervalPauseAuto != null && days % intervalPauseAuto== 0 && (newWeek || newMonth))
        {
            pausePlay.TogglePopUp();
            
        }
       
       
        int hours = Mathf.FloorToInt(((elapsedTime - (days * 24f) * 3600f) / 3600f));
       
        
        int minutes = Mathf.FloorToInt((elapsedTime - (days * 24f + hours )*3600f)/ 60f);
        int seconds = Mathf.FloorToInt((elapsedTime -(days * 24f + hours )*3600f) - (minutes * 60f));

        
       // string clockString = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);
        string clockString = string.Format("{0:0000}",days);
        clock.text = clockString;
        
        
    }

    public void UpdateAutoPause(string name)
    
    {
        switch (name)
        {
            case "Mensuel":
                pausePlay.PopUp.Titre = $"Bilan du {month} mois";
                intervalPauseAuto = MONTHLY_PAUSE;
                break;
            case "Hebdomadaire":
                pausePlay.PopUp.Titre =  $"Bilan de la semaine {week}";
                intervalPauseAuto = WEEKLY_PAUSE;
                break;
           case "None":
                intervalPauseAuto = null;
                break;
        }
        
    }

    public void OnSliderValueChanged()
    {
       if (slider.value > 0) multiplier = slider.value;
        
    }
}
