using System.IO;
using System.Linq;
using SimulationFourmiliere;
using UnityEngine;


public class SaveSystem : MonoBehaviour
{

    
    
   
    public string currentSaveName;
    
    public SimulationState simulation;
    
    public static string saveFolder;

    private MapLoader mapManager;
    private ClockManager clockManager;
    private PopCounter popCounter;

    
    public static SaveSystem Instance;

    public string mapFileName;

    void Start()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
       
        
        saveFolder = Application.persistentDataPath + "/Saves/";
       // InvokeRepeating(nameof(AutoSave), 30f, 30f);
    }

    public void JumpStart()
    {
        
        mapManager = FindObjectOfType<MapLoader>();
        clockManager = FindObjectOfType<ClockManager>(); 
        popCounter =  FindObjectOfType<PopCounter>();
      
      
        if (mapManager == null || clockManager == null || popCounter == null)
        {
            Debug.LogError("Objets manquants dans MainScene !");
            return;
        }
    }

    void AutoSave()
    {
        State state = CreateSaveState();
        if (currentSaveName != null || currentSaveName != "")
        {
            Save(currentSaveName, state);
        }
        else
        {
            Save("SystemSave", state);
        }
    }
    
    
    
    public State CreateSaveState()
    {
        State state = new State();
        simulation = popCounter.simState;

        state.name = currentSaveName;
        
        
        state.rows = mapManager.Rows;
        state.cols = mapManager.Cols;
        state.mapData = mapManager.ExportMap();
        state.odds = mapManager.ExportOdds();
        
        state.gameTime = clockManager.day;
       
        
        state.graphPop = simulation.HistoriquePopulation;
        state.graphBouff = simulation.HistoriqueNourriture;
        return state;

    }

   
    
    public static void Save(string saveName, State state){
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
        
        string json = JsonUtility.ToJson(state,true);
        Debug.Log("Saved file path : " + saveFolder + saveName +".json");
        File.WriteAllText(saveFolder + saveName +".json", json);
        
    }
    
    public static State Load(string saveName)
    {
        string path = saveFolder + saveName + ".json";
        if (!File.Exists(path))
        {
            Debug.Log("Save file doesn't exist");
            return null;
        }

        string json = File.ReadAllText(path);
       
        return JsonUtility.FromJson<State>(json);

    }

    public static string[] GetAllSaves()
    {
        if(!Directory.Exists(saveFolder))
            return new string[0];
        return Directory.GetFiles(saveFolder,"*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();
    }

   

  
}