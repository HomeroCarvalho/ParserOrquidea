using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace parser
{
    public class Log
    {
        
        private static string nomeArquivoLog;

        public Log(string filePath)
        {
            string caminhoDir = Path.GetFullPath(filePath);
            Log.nomeArquivoLog = caminhoDir;
        }

        /// <summary>
        /// adiciona uma linha de informação no log.
        /// </summary>
        /// <param name="logMessage"></param>
        public static void addMessage(string logMessage)
        {
            if (Log.nomeArquivoLog == null)
                Log.nomeArquivoLog = Path.GetFullPath("erros.txt");

            FileStream stream = new FileStream(nomeArquivoLog, FileMode.Append);
            StreamWriter stmwrt = new StreamWriter(stream);
            stmwrt.Write("Time: " + DateTime.Now.ToString() + "  ");
            stmwrt.WriteLine("Message: "+logMessage);

            stmwrt.Close();
            stream.Close();

            /// sai prematuramente para o Windows, pois
            /// a informação vem geralmente de um erro.
            System.Environment.Exit(1);
        } // addMessage()


    } // class Log
} // namespace
