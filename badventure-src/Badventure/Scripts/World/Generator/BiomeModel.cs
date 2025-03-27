using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMatrix2d.Generator
{
    public class BiomeModel
    {
        public string Name { get; }
        public RangeModel HeightRange { get; }
        public Color Col { get; }

        public BiomeModel(string name, RangeModel heightRange, Color color)
        {
            Name = name;
            HeightRange = heightRange;
            Col = color;
        }
    }
}
