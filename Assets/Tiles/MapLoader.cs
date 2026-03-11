using System.IO;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Tilemaps;



public class MapLoader : MonoBehaviour

{

    public Tilemap tilemap;

    public TileBase DarkDirt; // Le '1' (Terre pleine)

    public TileBase Dirt;     // Le '0' (Galerie creusée)



    public string filePath = "/Tiles/Maps.txt";

    public Camera mainCamera;



    private char[,] mapData; // Matrice en mémoire pour manipuler les 0 et 1

    private int rows;

    private int cols;

    private float timer = 0;



    void Start()

    {

        InitializeMap();

        RefreshTilemap();

        AdjustCamera();

    }



    void Update()

    {

        timer += Time.deltaTime;

        if (timer >= 1f)

        {

            CreuserAleatoireAdjacent();

            RefreshTilemap();

            timer = 0;

        }

    }



    // Charge le fichier TXT une seule fois au début

    void InitializeMap()

    {

        string path = Application.dataPath + filePath;

        if (!File.Exists(path)) { Debug.LogError("Fichier introuvable : " + path); return; }



        string[] lines = File.ReadAllLines(path);

        rows = lines.Length;

        cols = lines[0].Length;

        mapData = new char[rows, cols];



        for (int y = 0; y < rows; y++)

        {

            for (int x = 0; x < cols; x++)

            {

                mapData[y, x] = lines[y][x];

            }

        }

    }



    // Algorithme : trouve un '1' qui touche un '0' et le transforme en '0'

    void CreuserAleatoireAdjacent()

    {

        List<Vector2Int> ciblesPossibles = new List<Vector2Int>();



        for (int y = 0; y < rows; y++)

        {

            for (int x = 0; x < cols; x++)

            {

                // Si c'est de la terre (1), on regarde si un voisin est une galerie (0)

                if (mapData[y, x] == '1')

                {

                    if (IsAdjacentToZero(x, y))

                    {

                        ciblesPossibles.Add(new Vector2Int(x, y));

                    }

                }

            }

        }



        if (ciblesPossibles.Count > 0)

        {

            Vector2Int choix = ciblesPossibles[Random.Range(0, ciblesPossibles.Count)];

            mapData[choix.y, choix.x] = '0'; // On creuse !

        }

    }



    bool IsAdjacentToZero(int x, int y)

    {

        // On vérifie les 4 directions (Haut, Bas, Gauche, Droite)

        int[] dx = { 0, 0, -1, 1 };

        int[] dy = { -1, 1, 0, 0 };



        for (int i = 0; i < 4; i++)

        {

            int nx = x + dx[i];

            int ny = y + dy[i];



            if (nx >= 0 && nx < cols && ny >= 0 && ny < rows)

            {

                if (mapData[ny, nx] == '0') return true;

            }

        }

        return false;

    }



    // Traduit la matrice mémoire vers la Tilemap Unity

    void RefreshTilemap()

    {

        tilemap.ClearAllTiles();

        for (int y = 0; y < rows; y++)

        {

            for (int x = 0; x < cols; x++)

            {

                // Centrage identique à ton code original

                Vector3Int pos = new Vector3Int(x - cols / 2, -y + rows / 2, 0);

                tilemap.SetTile(pos, mapData[y, x] == '0' ? Dirt : DarkDirt);

            }

        }

    }



    void AdjustCamera()

    {

        tilemap.CompressBounds();

        Bounds mapBounds = tilemap.localBounds;

        if (mainCamera != null)

        {

            float padding = 1f;

            float sizeByHeight = mapBounds.size.y / 2f + padding;

            float sizeByWidth = (mapBounds.size.x / 2f + padding) / mainCamera.aspect;

            mainCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

            mainCamera.transform.position = mapBounds.center + new Vector3(0, 0, -10);

        }

    }

}