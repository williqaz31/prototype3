
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
    
    [SerializeField] public Canvas aucunApportWarning;
    
    [SerializeField] public GestionNourriture gestionNourriture;
    [SerializeField] public GameObject panelBouff;
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
                gestionNourriture.setApport(etat.appartParJour);
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
            simState = new SimulationState(100); //valeur initiale du stock de nourriture
            gestionNourriture.setApport(150); // Quantité initial par défaut, l'affiche visuellement
            popCounter.simState = simState;
            clockManager.fromLoad = false;
            mapLoader.LoadDefault();
           
        }

       

    }

   

    void OnEnable()
    {
        popCounter.GameOver += FinDePartie;
        gestionNourriture.onChangementApport += ChangerApport;
       
        
       
        
       
    }

    void OnDisable()
    {
        popCounter.GameOver -= FinDePartie;
        gestionNourriture.onChangementApport -= ChangerApport;
       
        popCounter.simState.AucunApport -= AucunApport;
        
        
    }

    void AucunApport()
    {
        Time.timeScale = 0f;
        aucunApportWarning.gameObject.SetActive(true);
     
    }

    public void CloseWarning()
    {
        Time.timeScale = 1f;
        aucunApportWarning.gameObject.SetActive(false);
    }

    private void FinDePartie()
    {
        State state = new State();
        state.gameOver = true;
        SaveSystem.Save(saveName, state);
        AfficherFinDePartie();
       
        
    }

    private void ChangerApport(int newApport)
    {
      
        if (popCounter.simState != null)
        {
            popCounter.simState.AucunApport += AucunApport;
            popCounter.simState.apport = newApport;
        }
        else
        {
            Debug.Log("popcounter na pas de simulation");
        }
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
        if (IsGameSaved())
        {
            
            SceneManager.LoadScene("MaiMenue");
           
         
        }
        else
        {
            Debug.Log("Game not saved");
            SceneManager.LoadScene("MaiMenue");
        }
        
        
      
    }

    private bool IsGameSaved()
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

    public void BouffPanelToggle()
    {
        panelBouff.SetActive(!panelBouff.activeSelf);
    }
}
