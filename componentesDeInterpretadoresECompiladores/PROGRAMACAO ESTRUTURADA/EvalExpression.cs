using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace parser
{


    public class EvalExpression
    {

        public static LinguagemOrquidea linguagem = new LinguagemOrquidea();
        public static List<string> nomesTiposNativos = new List<string>() { "int", "float", "bool", "char", "string" };

      
        public object EvalPosOrdem(Expressao expss, Escopo escopo)
        {

            if (expss.isModdfy == false)
                return expss.oldValue;
            else
            {
                if (!expss.isInPosOrdem)
                {
             
                    expss = expss.PosOrdemExpressao();
                    expss.isInPosOrdem = true;
                }

                expss.isModdfy = false;
                return this.Eval(expss, escopo);
            }
        } // EvalPosOrdem()

        protected object Eval(Expressao expss, Escopo escopo)
        {
            if (expss.Elementos.Count == 0)
            {
                if (Expressao.Instance.IsTipoInteiro(expss.ToString()))
                    return int.Parse(expss.ToString());
                else
                if (Expressao.Instance.IsTipoFloat(expss.ToString()))
                    return float.Parse(expss.ToString());
                else
                    return ((object)expss.ToString());
            }
            string tipoDaExpressao = expss.tipo;

            object result1 = 0;
            Pilha<object> pilhaOperandos = new Pilha<object>("pilhaOperandos");

            for (int x = 0; x < expss.Elementos.Count; x++)
            {

                if (linguagem.VerificaSeEhNumero(expss.Elementos[x].ToString()))
                    pilhaOperandos.Push((object)expss.Elementos[x].ToString());
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoNumero))
                {
                    string strNumero = ((ExpressaoNumero)expss.Elementos[x]).numero;
                    if (Expressao.Instance.IsTipoInteiro(strNumero))
                    {
                        int numero = int.Parse(strNumero);
                        pilhaOperandos.Push(numero);
                    }
                    else
                    if (Expressao.Instance.IsTipoFloat(strNumero))
                    {
                        float numero = float.Parse(strNumero);
                        pilhaOperandos.Push(numero);
                    }
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoVariavelVetor))
                {
                    // calculo de vetores multidimensionais!
                    VariavelVetor v = ((ExpressaoVariavelVetor)expss.Elementos[x]).variavel;

                    int[] indices = new int[v.dimensoes.Length];

                    for (int i = 0; i < v.dimensoes.Length; i++)
                        indices[i] = (int)new EvalExpression().Eval((Expressao)expss.Elementos[x].Elementos[i].GetElemento(), escopo);
                    pilhaOperandos.Push(v.GetElemento(indices));

                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoVariavel))
                {
                    Variavel v = ((ExpressaoVariavel)expss.Elementos[x]).variavel;
                    object valor = v.valor;
                    pilhaOperandos.Push(valor);
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoChamadaDeFuncao))
                {
                    ExpressaoChamadaDeFuncao exprssChamada = (ExpressaoChamadaDeFuncao)expss.Elementos[x];
                    // obtem a funcao da chamada da funcao!
                    Funcao funcaoExpressao = exprssChamada.funcao;

                    // obtem os parametros da chamada de funcao!
                    List<Expressao> parametrosDaChamada = exprssChamada.expressoesParametros;


                    // executa a funcao, com parametros, e objeto que criou o metodo da funcao!
                    object resultFunction = funcaoExpressao.ExecuteAFunction(parametrosDaChamada, funcaoExpressao.caller);
                    pilhaOperandos.Push(resultFunction);
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoElemento))
                {

                    // nova funcionalidade: elementos que podem ser o nome de variáveis.
                    Variavel v = escopo.tabela.GetVar(expss.Elementos[x].GetElemento().ToString(), escopo);
                    if (v != null)
                        pilhaOperandos.Push(v.valor);
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoOperador))
                {

                    Operador operador = ((ExpressaoOperador)expss.Elementos[x]).operador;
                    if (operador.GetTipo() == "BINARIO")
                    {
                        if (operador != null)
                        {
                            if (operador.nome == "=")
                            {
                                Variavel vAtribuicao = escopo.tabela.GetVar(expss.Elementos[0].ToString(), escopo);
                                if (vAtribuicao != null)
                                {
                                    object valor = pilhaOperandos.Pop();
                                    vAtribuicao.SetValor(valor, escopo);
                                    return valor;
                                }
                            }
                            else
                            {
                                object oprnd2 = pilhaOperandos.Pop();
                                object oprnd1 = pilhaOperandos.Pop();

                                oprnd1 = this.Parser_Number(oprnd1);
                                oprnd2 = this.Parser_Number(oprnd2);
                     
                                // execução de operador nativo.
                                result1 = operador.ExecuteOperador(operador.nome, escopo, oprnd1, oprnd2);
                                pilhaOperandos.Push(result1);
                            } // else
                        } // if
                    }
                    else
                    if (operador.GetTipo() == "UNARIO")
                    {
                        object oprnd2 = pilhaOperandos.Pop();
                        oprnd2 = this.Parser_Number(oprnd2);
            
                        result1 = operador.ExecuteOperador(operador.nome, escopo, oprnd2);
                        pilhaOperandos.Push(result1);

                        Variavel vAtribuicaoUnario = escopo.tabela.GetVar(expss.Elementos[0].ToString(), escopo);
                        if (vAtribuicaoUnario != null)
                        {
                            object valor = result1;
                            vAtribuicaoUnario.SetValor(valor, escopo);
                        }
                    }
                } // if

            } // for x
            if (pilhaOperandos.lenghtPilha > 0)
                result1 = pilhaOperandos.Pop();

            return result1;
        } // Eval()
        private object Parser_Number(object number)
        {
            if (number == null)
                return null;
            if (Expressao.Instance.IsTipoInteiro(number.ToString()))
                return int.Parse(number.ToString());
            if (Expressao.Instance.IsTipoFloat(number.ToString()))
                return float.Parse(number.ToString());
            return number;
        }

    } //class EvalExpression

} // namespace
