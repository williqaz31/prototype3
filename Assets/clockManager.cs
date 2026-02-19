using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class clockManager : MonoBehaviour
{
    [SerializeField] private TMP_Text clock;
    private float elapsedTime;
    [SerializeField] private Slider slider;
    private float multiplier = 1.0f;
    public popCounter popCounter;
    private float timeInADay = 86400f;
    public int day = 0;

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
        int days = Mathf.FloorToInt(elapsedTime / timeInADay);
        if (days > day)
        {
            day = days;
            popCounter.NoveauJour();
        }
       
       
        int hours = Mathf.FloorToInt(((elapsedTime - (days * 24f) * 3600f) / 3600f));
       
        
        int minutes = Mathf.FloorToInt((elapsedTime - (days * 24f + hours )*3600f)/ 60f);
        int seconds = Mathf.FloorToInt((elapsedTime -(days * 24f + hours )*3600f) - (minutes * 60f));

        
        string clockString = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);
        clock.text = clockString;
        
        
    }

    public void OnSliderValueChanged()
    {
        multiplier = slider.value;
        UpdateClockUI();
    }
}
