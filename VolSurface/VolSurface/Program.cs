using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolSurface
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] time = new int[2] { 100000000, 140000000 };
            ImpvCurve myCurve = new ImpvCurve(20160422,time);
        }
    }
}
