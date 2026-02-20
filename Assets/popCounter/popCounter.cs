using System.Collections.Generic;
using individu;
using SimulationFourmiliere;
using TMPro;
using UnityEngine;

public class popCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text populationCounter;
    [SerializeField] private int stockNourriture = 50;
    private SimulationState state;

  

    private int jourActuelle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = new SimulationState(stockNourriture);

    }

    public void NoveauJour()

    {
        try
        {
            Program.CalculSimulation(state);
            populationCounter.text = string.Format("{0:00000}", state.colonie.Pop());
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
