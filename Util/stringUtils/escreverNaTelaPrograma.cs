using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public class Utils
    {
        public static string UneLinhasPrograma(List<string> programa)
        {
            string programaTotal = "";
            foreach (string line in programa)
                programaTotal += line + " ";
            return programaTotal;
        } // UneLinhasPrograma()
    } // class Utils
} // namespace
