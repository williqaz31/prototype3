using System;
using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public class SerieParams
    {
        public string category;
        public Color32 color;
        public List<string> columns = new();
        public int dataCount;
        public int dimension;
        public bool ignore;
        public bool isSecondaryMark;
        public string itemFormatter;
        public string marker = "●";
        public string numericFormatter;
        public SerieData serieData;
        public int serieIndex;
        public string serieName;
        public Type serieType;
        public double total;
        public double value;
    }
}