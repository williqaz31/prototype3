using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SimulationFourmiliere;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class State
{
    public bool gameOver;
    public int ageReine;
    public int dureeDeVieReine;
    public string name;
    public char[] mapData;
    public int rows;
    public int cols;
    public int[] odds;
    public int gameTime;
    

    public List<float> graphPop;
    public List<float> graphBouff;


}





