using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Interpolation;
using SM_Oil.Forms;
namespace SM_Oil.Controllers
{
    public static class InterpolationFunctions
    {
        public static CubicSpline CubicInterpolation(List<double> x , List<double>y) 
        {
            IEnumerable<double> xx = (IEnumerable<double>)x;
            CubicSpline spline = CubicSpline.InterpolateAkima(x, y);

            return spline;
        }
        public static List<double> Recutting (CubicSpline spline , List<double> newXList)
        {
            List<double> newY = new List<double>();
            foreach(var x in newXList)
            {
                newY.Add(spline.Interpolate(x));
            }
            return newY;
        }
    }
}
