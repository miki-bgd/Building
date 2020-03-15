using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Building.Commands.CreateBuildings
{
    class FloorCount
    {
        public static int GetCount(string s)
        {
            int count = 0;
            s = s.Trim();
            int index = 0;
            if ((index = s.IndexOf("Pt")) >= 0)
            {
                count++;
                s = s.Remove(index, 2);
            }
            if ((index = s.IndexOf("P")) >= 0)
            {
                count++;
            }
            s = s.Replace("+", "y");
            string pattern = @"\d+";
            Match reg = Regex.Match(s, pattern);
            if (reg.Success)
            {
                try
                {
                    count += int.Parse(reg.Value);
                }
                catch (System.Exception) { }
            }
            return count;
        }
    }
}
