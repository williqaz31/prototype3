using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;


    public bool exists;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void OnClick()
    {
        string saveName;

        // Si le nameInput est intéractif, alors l'utilisateur crée une nouvelle partie donc le nom de la save va avec le nom entrer
        if (nameInput.IsInteractable())
        {
           
            saveName = nameInput.text;
            SaveSystem.Instance.currentSaveName = saveName;
        }

        SceneManager.LoadScene("MainScene");
    }
}