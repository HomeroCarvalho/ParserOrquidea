﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace parser
{
    
    /// <summary>
    /// converte uma arquivo de texto, em uma lista de tokens da linguagem orquidea.
    /// </summary>
    public class ParserAFile
    {
        private List<string> tokens;
        private List<string> code;
        private static LinguagemOrquidea linguagem = LinguagemOrquidea.Instance();

        private List<char> caracteresRemover = new List<char>() { '\t', '\n' };

        public ParserAFile(string pathFile)
        {
            this.Parser(pathFile);
        }
        public List<string> GetCode()
        {
            return this.code;
        }

        public List<string> GetTokens()
        {
            return this.tokens;
        }


        /// Le o arquivo na entrada, e converte em tokens e codigos na saída.
        private void Parser(string fileName)
        {
            PosicaoECodigo.InitCalculoPosicoes();   // essencial para se contar a posicao de tokens, é preciso inicializar algumas propriedades estáticas.

            this.code = new List<string>();
            this.tokens = new List<string>();

            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read); // abre um dos arquivos texto para  leitura de código de linguagem.
            StreamReader reader = new StreamReader(stream);
            
            while (!reader.EndOfStream) // lê o arquivo currente e constroi a lista de código e tokens.
            {
                string lineOfCode = reader.ReadLine();
                PosicaoECodigo.AddLineOfCode(lineOfCode);
                if ((lineOfCode != null) && (lineOfCode.Length > 0)) // lê uma linha de código, sem tokens, apenas código.
                    code.Add(lineOfCode.Trim(' '));
                
            } // while
            if ((code != null) && (code.Count > 0))
                this.tokens = new Tokens(linguagem, code).GetTokens(); // converte o código para uma lista de tokens.

        } // Parser()
    }
}
