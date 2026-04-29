using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField nameInput;
    
    

    public bool exists;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
    public void OnClick()
    {
        string saveName;
        
        // Si le nameInput est intéractif, alors l'utilisateur crée une nouvelle partie donc le nom de la save va avec le nom entrer
        if (nameInput.IsInteractable())
        {
            if (nameInput.text != "")
            {
                saveName = nameInput.text;
                
            }
            else
            {
                saveName = "default";
            }
           
            
            SaveSystem.Instance.currentSaveName = saveName;
           
        }
        SceneManager.LoadScene("MainScene");
        


    }
}
