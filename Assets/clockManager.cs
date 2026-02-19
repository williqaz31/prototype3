using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class clockManager : MonoBehaviour
{
    [SerializeField] private TMP_Text clock;
    private float elapsedTime;
    [SerializeField] private Slider slider;
    private float multiplier = 1.0f;

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
        
        int hours = Mathf.FloorToInt(elapsedTime / 3600f);
        int days = Mathf.FloorToInt(hours / 24);
        
        int minutes = Mathf.FloorToInt((elapsedTime- hours *3600f)/ 60f);
        int seconds = Mathf.FloorToInt((elapsedTime -hours *3600f) - (minutes * 60f));
        hours -= days * 24;
        
        
        string clockString = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);
        clock.text = clockString;
        
        
    }

    public void OnSliderValueChanged()
    {
        multiplier = slider.value;
        UpdateClockUI();
    }
}
