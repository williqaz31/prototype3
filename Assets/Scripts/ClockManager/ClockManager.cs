using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockManager : MonoBehaviour
{
    private const float SECONDS_IN_DAY = 86400f;
    private const int WEEKLY_PAUSE = 7;
    private const int MONTHLY_PAUSE = 30;
    [SerializeField] private TMP_Text clock;
    public float elapsedTime;
    [SerializeField] private Slider slider;
    [SerializeField] private PausePlay pausePlay;
    [SerializeField] private ToggleManager UpdateCycle;

    public bool fromLoad;
    public PopCounter popCounter;
    public int day;


    private int? intervalPauseAuto;
    private bool isWeekly;
    private int month;
    private string monthInfo;

    private float multiplier = 1.0f;
    private bool newMonth;
    private bool newWeek;
    private int week;

    private string weeklyInfo;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (fromLoad) elapsedTime = day * SECONDS_IN_DAY;
    }

    // Update is called once per frame
    private void Update()
    {
        elapsedTime += Time.deltaTime * multiplier;

        UpdateClockUI();
    }

    private void UpdateClockUI()
    {
        var days = Mathf.FloorToInt(elapsedTime / SECONDS_IN_DAY);
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


                if (intervalPauseAuto == WEEKLY_PAUSE)
                {
                    isWeekly = true;
                    weeklyInfo = popCounter.EndWeek();
                }
                else
                {
                    isWeekly = false;
                }

                popCounter.NouvelleSemaine(isWeekly);
            }
            else
            {
                newWeek = false;
            }

            if (days % 30 == 0 && days / 30 != month)
            {
                month++;
                newMonth = true;

                if (intervalPauseAuto == MONTHLY_PAUSE)
                {
                    isWeekly = false;
                    monthInfo = popCounter.EndMonth();
                }
                else
                {
                    isWeekly = true;
                }

                popCounter.NouveauMois(isWeekly);
            }
            else
            {
                newMonth = false;
            }
        }


        if (intervalPauseAuto != null && days % intervalPauseAuto == 0 && (newWeek || newMonth))
        {
            switch (intervalPauseAuto)
            {
                case WEEKLY_PAUSE:
                    pausePlay.PopUp.Information = weeklyInfo;
                    break;
                case MONTHLY_PAUSE:
                    pausePlay.PopUp.Information = monthInfo;
                    break;
            }

            pausePlay.TogglePopUp();
        }


        var hours = Mathf.FloorToInt((elapsedTime - days * 24f * 3600f) / 3600f);


        var minutes = Mathf.FloorToInt((elapsedTime - (days * 24f + hours) * 3600f) / 60f);
        var seconds = Mathf.FloorToInt(elapsedTime - (days * 24f + hours) * 3600f - minutes * 60f);


        // string clockString = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);
        var clockString = string.Format("{0:0000}", days);
        clock.text = clockString;
    }

    public void UpdateAutoPause(string name)

    {
        switch (name)
        {
            case "Mensuel":
                pausePlay.PopUp.Titre = "Bilan du  mois";
                intervalPauseAuto = MONTHLY_PAUSE;

                break;
            case "Hebdomadaire":
                pausePlay.PopUp.Titre = "Bilan de la semaine ";
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