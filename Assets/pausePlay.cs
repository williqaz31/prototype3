 using UnityEngine;
using UnityEngine.UI;

public class pausePlay : MonoBehaviour
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
            PopUp.ToggleMenu();
            

        }
        else
        {
            buttonImage.sprite = pauseSprite;
            PopUp.ToggleMenu();
            Time.timeScale = 1;
            
        }
        isPlaying = !isPlaying;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
