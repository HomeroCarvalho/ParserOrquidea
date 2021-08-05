using System;
using System.Collections.Generic;
using System.Linq;
using parser;
namespace stringUtilities
{
    public class CaracteresID
    {

        private static CaracteresID localizador = null;
        private static UmaGramaticaComputacional linguagem = null;

        public static CaracteresID GetInstance()
        {
            if (localizador == null)
            {
                localizador = new CaracteresID();
                linguagem = new LinguagemOrquidea();
            }
            return localizador;
        }

        private static List<char> caracteresValidos = new List<char>()
       {
            'a', 'b' , 'c' , 'd' , 'e' , 'f' , 'g' , 'h' , 'k' , 'i', 'j' , 'k' , 'l', 'm' , 'n',
            'o', 'p' , 'q' , 'r' , 's' , 't' , 'u' , 'v' , 'x' , 'y' , 'w' , 'z',
            'A', 'B' , 'c' , 'D' , 'E' , 'F' , 'G' , 'H' ,'I','J', 'K' , 'L' , 'M' , 'N',
            'O', 'P' , 'Q' , 'R' , 'S' , 'T' , 'U' , 'V' , 'X' , 'Y' , 'W' , 'Z',
             '_' };


        /// <summary>
        /// verifica se o caracter em [index(token, tokensJuntos]] + offset é uma letra.
        /// Necessário para eliminar tokens que contém em seu texto, tokens pesquisados (falsa ocorrência).
        /// </summary>
        public static bool IsValidCaracter(char token)
        {
            if (char.IsWhiteSpace(token))
                return false;
            int index = caracteresValidos.FindIndex(k => k == token);
            return (index != -1);

        } 
       
    }// class localizadorDeStrings
} // namespace stringUtilies
