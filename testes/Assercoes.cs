using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
namespace ModuloTESTES

{
    public class Assercoes
    {
        /// <summary>
        /// teste associado ao objeto Assercoes currente.
        /// </summary>
        public Teste testeCurrente;
    

        /// <summary>
        /// construtor, aceita um [Teste] para fazer operação de teste de condições.
        /// </summary>
        /// <param name="testecurrente"></param>
        public Assercoes(Teste testecurrente)
        {
            this.testeCurrente = testecurrente;
            this.testeCurrente.TesteFalhou = false;
        }

     
        /// <summary>
        /// mostra a mensagem de teste bem sucedido, para o teste currente.
        /// </summary>
        public void MsgSucess()
        {
            testeCurrente.mensagens = "Teste: " + '"' + testeCurrente.GetNomeDoTeste() + '"' + " passou.";
            ModuloTESTES.LoggerTests.AddMessage(testeCurrente.mensagens);

        }

        /// <summary>
        /// mostra a mensagem de teste bem sucedido, mais a mensagem [msg].
        /// </summary>
        /// <param name="msg"></param>
        public void MsgSucess(string msg)
        {
            testeCurrente.mensagens = "Teste: " + '"' + testeCurrente.GetNomeDoTeste() + '"' + " bem sucedido!! -->" + msg;
            LoggerTests.AddMessage(testeCurrente.mensagens);
        }

        /// <summary>
        /// passa para o teste currente a mensagem de falha, somente.
        /// </summary>
        public void MsgFail()
        {
            testeCurrente.mensagens = "Teste: " + '"' + testeCurrente.GetNomeDoTeste() + '"' + " não passou!!";
            testeCurrente.TesteFalhou = true;
            LoggerTests.AddMessage(testeCurrente.mensagens);
        }


        /// <summary>
        /// passa para o teste currente a mensagem de falha, mais a mensagem [msg].
        /// </summary>
        /// <param name="msg"></param>
        public void MsgFail(string msg)
        {
            testeCurrente.mensagens = "Teste: " + '"' + testeCurrente.GetNomeDoTeste() + '"' + " falhou!! -->" + msg;
            LoggerTests.AddMessage(testeCurrente.mensagens);
            testeCurrente.TesteFalhou = true;
        }

        /// <summary>
        /// asserção genérica.Deve-se ter cuidado se os valores são objetos,
        /// pois podem comparar referências e não valores dentro deles.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valorAtual"></param>
        /// <param name="valorEsperado"></param>
        /// <returns></returns>
        public bool Equals<T>(T valorAtual, T valorEsperado)
        {
            if (valorAtual.Equals(valorEsperado))
            {
                MsgSucess();
                return true;
            }
            MsgFail();
            return false;
        } // Igual()

        /// <summary>
        /// verifica se dois objetos são iguais por valor.
        /// </summary>
        /// <param name="valorAtual">objeto encontrado no programa a ser testado.</param>
        /// <param name="valorEsperado">objeto esperado o programa calcular.</param>
        /// <returns>retorna true se os objetos são iguais.</returns>
        public bool EqualsObj(ValueType valorAtual, ValueType valorEsperado)
        {
            return (valorAtual.Equals(valorEsperado));
        }

        /// <summary>
        /// compara se duas strings são iguais.
        /// Retorna true se as strings são iguais, false se contrário.
        /// </summary>
        /// <param name="valorAtual">valor encontrado no programa testado.</param>
        /// <param name="valorEsperado">valor esperado para o programa testado.</param>
        /// <returns>[true] se as strings valorAtual e valorEsperado são iguais.</returns>
        public bool EqualsString(string valorAtual, string valorEsperado)
        {
            if (valorAtual.TrimStart(' ').TrimEnd(' ') == valorEsperado.TrimStart(' ').TrimEnd(' '))
            {
                MsgSucess();
                return true;
            }
            MsgFail();
            return false;
        }
        /// <summary>
        /// retorna true se |valorAtual-valorEsperado|<delta.
        /// </summary>
        /// <param name="valorAtual">valor encontrado no programa testado.</param>
        /// <param name="valorEsperado">valor esperado para o programa testado.</param>
        /// <param name="delta">diferença mínima para resultar true.</param>
        /// <returns>[true] se o módulo da diferença do valor atual e valor esperado for menor que delta.</returns>
        public bool EqualsDouble(double valorAtual, double valorEsperado, double delta)
        {
            if (Math.Abs(valorAtual - valorEsperado) <= delta)
            {
                MsgSucess();
                return true;
            }
            MsgFail();
            return false;
        }

        /// <summary>
        /// retorna true se |valorAtual-valorEsperado|<delta.
        /// </summary>
        /// <param name="valorAtual">valor encontrado no programa testado.</param>
        /// <param name="valorEsperado">valor esperado para o programa testado.</param>
        /// <param name="delta">diferença mínima para resultar true.</param>
        /// <returns>[true] se o módulo da diferença do valor atual e valor esperado for menor que delta.</returns>
        public bool EqualsFloat(float valorAtual, float valorEsperado, float delta)
        {
            if (Math.Abs(valorAtual - valorEsperado) <= delta)
            {
                MsgSucess();
                return true;
            }
            MsgFail();
            return false;
        }

        /// <summary>
        /// retorna true se uma determinada condição (resultado booleano) for true,
        /// retorna false se a condição for false.Útil para testar resultados que
        /// dependam de uma condição de teste.
        /// </summary>
        /// <param name="condicao"></param>
        /// <returns></returns>
        public bool IsTrue(bool condicao)
        {
            if (condicao)
            {
                MsgSucess();
                return true;
            }
            MsgFail();
            return false;
        }

        /// <summary>
        /// mostra a mensagem de falha, mais a mensagem [msg].
        /// </summary>
        /// <param name="msg"></param>
        public void Fail(string msg)
        {
            MsgFail(msg);
        }

        public void Sucess(string msg)
        {
            MsgSucess(msg);
        }
    } // class Assercoes

    
   
  

} // namespace
