using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc.DataModules
{
    public class Coordinate
    {
        public double X
        {
            get; set;
        } = 0;
        public double Y
        {
            get; set;
        } = 0;
        public double Z
        {
            get; set;
        } = 0;

        public Coordinate(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
