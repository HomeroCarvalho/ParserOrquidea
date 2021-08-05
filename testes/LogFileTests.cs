using System;
using System.IO;
namespace ModuloTESTES
{
    /// <summary>
    /// Classe que encapsula o arquivo de log de ocorrência de erros, e validações.
    /// </summary>
    public class LoggerTests
    {

        private static string nameFileLog;


        public static void SetFileName(string filePath)
        {
            nameFileLog = filePath;
        }

        public static string getFileName()
        {
            return nameFileLog;
        }

        public static void ClearLoggFile()
        {
            FileStream stream = new FileStream(nameFileLog, FileMode.Create);
            stream.Close();
            stream.Dispose();
          
        }

        /// <summary>
        /// adiciona uma linha de informação no log.
        /// </summary>
        /// <param name="logMessage"></param>
        public static void AddMessage(string logMessage)
        {
            if (nameFileLog == null)
                nameFileLog = Path.GetFullPath("RelatorioTexto.txt");

            FileStream stream = new FileStream(nameFileLog, FileMode.Append);
            StreamWriter stmwrt = new StreamWriter(stream);
            stmwrt.WriteLine();
            stmwrt.WriteLine();
            stmwrt.Write("Time: " + DateTime.Now.ToString() + "  ");
            stmwrt.WriteLine("Message: " + logMessage);
            stmwrt.Close();
            stream.Close();
        } // AddMessage()
    } // class Log
} // namespace