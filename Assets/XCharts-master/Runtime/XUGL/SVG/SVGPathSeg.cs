using System.Collections.Generic;
using UnityEngine;

namespace XUGL
{
    public class SVGPathSeg
    {
        public List<float> parameters = new();
        public string raw;
        public bool relative;
        public SVGPathSegType type;

        public SVGPathSeg(SVGPathSegType type)
        {
            this.type = type;
        }

        public float value
        {
            get
            {
                if (type == SVGPathSegType.H)
                    return SVG.yMirror ? -parameters[0] : parameters[0];
                return parameters[0];
            }
        }

        public float x => parameters[0];
        public float y => SVG.yMirror ? -parameters[1] : parameters[1];
        public Vector2 p1 => new(parameters[0], SVG.yMirror ? -parameters[1] : parameters[1]);
        public Vector2 p2 => new(parameters[2], SVG.yMirror ? -parameters[3] : parameters[3]);
        public Vector2 p3 => new(parameters[4], SVG.yMirror ? -parameters[5] : parameters[5]);
    }
}