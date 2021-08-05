using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser;
namespace testesComponentes
{
    class Program
    {
        static void Main(string[] args)
        {
            linguagemOrquidea lng = new linguagemOrquidea();
            List<string> programa = new List<string>();
            programa.Add("for (a=1;a<10;a++)");
            programa.Add("{");
            programa.Add("int k=1;");
            programa.Add("}");
            producao p = lng.producoes[3];
            bool result = lng.match(lng, p, ref programa, 0);
            int x;
            string strPrograma = "";
            for (x = 0; x < programa.Count; x++)
            {
                strPrograma += programa[x] + "\n";
            } //for x
            System.Console.WriteLine("Programa a ser pesquisado: " + strPrograma);
            System.Console.WriteLine("Produção candidata: " + p.maquinaDeEstados.ToString());
            System.Console.WriteLine("Resultado de match: " + result.ToString());
            System.Console.ReadLine();
        } //  void Main()
    } // class Program
} // namespace
