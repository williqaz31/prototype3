 using UnityEngine;
using UnityEngine.UI;

public class PausePlay : MonoBehaviour
{
    [SerializeField] public Sprite playSprite;
    [SerializeField] public Sprite pauseSprite;

    [SerializeField] public Image buttonImage;
    [SerializeField] public PopUpManager PopUp;

    private bool isPlaying = true;

   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonImage = GetComponent<Image>();
        
        buttonImage.sprite = buttonImage.sprite;
        
    }

    public void Toggle()
    {
        
        if (isPlaying)
        {
            buttonImage.sprite = playSprite;
            Time.timeScale = 0;
            
        }
        else
        {
            buttonImage.sprite = pauseSprite;
           
            Time.timeScale = 1;
            PopUp.ToggleOff();

        }
        isPlaying = !isPlaying;
    }

    public void TogglePopUp()
    {
        Toggle();
        PopUp.ToggleMenu();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
