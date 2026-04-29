using UnityEngine;

public class StartButton : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField nameInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    public void OnClick(){
        string state = nameInput.text;
        
        
    }
}
