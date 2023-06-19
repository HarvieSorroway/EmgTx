using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmgTx.EmgCustom
{
    public class EmgLUT
    {
        float[] xCoords;
        float[] yCoords;

        Vector2[] coords;
        public Vector2[] Coords
        {
            get => coords;
            set
            {
                coords = value;
                SetupLUT();            
            }
        }
        public bool CanInverseLerp { get;private set; }

        public EmgLUT(Vector2[] coords)
        {
            if (coords.Length < 2)
                coords = new Vector2[] { Vector2.zero, Vector2.one };
            Coords = coords;
        }

        public EmgLUT(Func<float,float> expression,int subsections)
        {
            int totalCount = Mathf.Min(subsections + 1, 2);
            var coords = new Vector2[totalCount];

            for(int i = 0;i < totalCount; i++)
            {
                float x = Mathf.Lerp(0f, 1f, i / (float)subsections);
                coords[i] = new Vector2(x, expression(x));
            }
            Coords = coords;
        }

        void SetupLUT()
        {
            xCoords = new float[coords.Length];
            yCoords = new float[coords.Length];
            for (int i = 0; i < coords.Length; i++)
            {
                xCoords[i] = coords[i].x;
                yCoords[i] = coords[i].y;
            }

            bool monotonicity = yCoords[0] < yCoords[1];
            for (int j = 0; j < yCoords.Length - 1; j++)
            {
                if(monotonicity != (yCoords[j] < yCoords[j + 1]))
                {
                    CanInverseLerp = false;
                    return;
                }
            }
            CanInverseLerp = true;
        }

        public float Lerp(float t)
        {
            int ix = 0;
            for(int i = 0;i < xCoords.Length;i++)
            {
                if(t <= xCoords[i])
                {
                    ix = i;
                    break;
                }
            }
            int ixUp = Mathf.Clamp(0, xCoords.Length - 1, ix + 1);
            return Mathf.Lerp(yCoords[ix], yCoords[ixUp], Mathf.InverseLerp(xCoords[ix], xCoords[ixUp], t));
        }

        public float InverseLerp(float v)
        {
            if (!CanInverseLerp)
                return -1f;
            int iy = 0;
            for (int i = 0; i < yCoords.Length; i++)
            {
                if (v <= yCoords[i])
                {
                    iy = i;
                    break;
                }
            }
            int iyUp = Mathf.Clamp(0, yCoords.Length - 1, iy + 1);
            return Mathf.Lerp(xCoords[iy], xCoords[iyUp], Mathf.InverseLerp(yCoords[iy], yCoords[iyUp], v));
        }
    }
}
