
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using SimulationFourmiliere;
using UnityEngine.SceneManagement;

public class GameInitialiser : MonoBehaviour

{
    [SerializeField] public ClockManager clockManager;
    [SerializeField] public PopCounter popCounter;
    [SerializeField] public MapLoader mapLoader;
    [SerializeField] public CanvasGroup intro;
    [SerializeField] public GameObject tout;
    [SerializeField] public Canvas gameOverCanvas;
   // [SerializeField] public CanvasGroup gameOver;

   public SimulationState simState;
   private string saveName;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
      
        saveName = SaveSystem.Instance.currentSaveName;
        
        State etat = SaveSystem.Load(saveName);

        if (etat != null)
        {
            //Si la partie à terminée nous la gardons ainsi
            if (etat.gameOver)
            {
               
               AfficherFinDePartie(); 
            }
            else
            {
                StartCoroutine(Debut());
                simState = new SimulationState(etat);
                popCounter.simState = simState;
                clockManager.fromLoad = true;
                clockManager.day = etat.gameTime;

                // Load la map d'enregistrer

                mapLoader.LoadMap(etat.mapData, etat.rows, etat.cols);
                mapLoader.LoadOdds(etat.odds, etat.rows, etat.cols);
            }



        }
        else
        {
            StartCoroutine(Debut());
            Debug.Log("Nouvelle partie");
            simState = new SimulationState(100); //valeur initiale
            popCounter.simState = simState;
            clockManager.fromLoad = false;
            mapLoader.LoadDefault();
           
        }

       

    }

    void OnEnable()
    {
        popCounter.GameOver += FinDePartie;
    }

    void OnDisable()
    {
        popCounter.GameOver -= FinDePartie;
    }

    public void FinDePartie()
    {
        State state = new State();
        state.gameOver = true;
        SaveSystem.Save(saveName, state);
        AfficherFinDePartie();
       
        
    }

    void AfficherFinDePartie()
    {
        tout.SetActive(false);
        gameOverCanvas.gameObject.SetActive(true);
    }

    
    public void Quit()
    { 
        gameOverCanvas.gameObject.SetActive(false);
        
        Time.timeScale = 1f;
        if (isGameSaved())
        {
            
            SceneManager.LoadScene("MaiMenue");
           
         
        }
        else
        {
            Debug.Log("Game not saved");
            SceneManager.LoadScene("MaiMenue");
        }
        
        
      
    }

    private bool isGameSaved()
    {
        string[] saves = SaveSystem.GetAllSaves();

        for (int i = 0; i < saves.Length; i++)
        {
            if (saves[i] == saveName)
            {
                return true;

            }
        }
        return false;
    }

    public void SaveGame()
    {
        Debug.Log("Save Game");
        State etat = SaveSystem.Instance.CreateSaveState();
        SaveSystem.Save(saveName, etat);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameStart()
    {
        StartCoroutine(Fin());
        tout.SetActive(true);
        SaveSystem.Instance.JumpStart();
        
        
        
    }
    IEnumerator Debut()
    {
        float temps = 0f;

        while (temps <= 2f)
        {
            temps += Time.deltaTime;
            intro.alpha = (temps/2f);
            yield return null;
        }
    }
    IEnumerator Fin()
    {
        float temps = 0f;

        while (temps <= 1.5f)
        {
            temps += Time.deltaTime;
            intro.alpha = 1 - (temps/1.5f);
            yield return null;
        }
    }
}
