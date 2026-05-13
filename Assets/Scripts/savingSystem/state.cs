using System;
using System.Collections.Generic;

[Serializable]
public class State
{
    public int appartParJour;
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