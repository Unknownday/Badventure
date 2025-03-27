using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMatrix2d.Generator
{
    public class RangeModel
    {
        public float Min { get; }
        public float Max { get; }

        public RangeModel(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public bool Fits(float value)
        {
            return Min <= value && value <= Max;
        }
    }
}
