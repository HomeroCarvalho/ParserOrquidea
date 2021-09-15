using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace parser
{


    public class EvalExpression
    {

        public static LinguagemOrquidea linguagem = new LinguagemOrquidea();
        public static List<string> nomesTiposNativos = new List<string>() { "int", "float","double", "bool", "char", "string" };

      
        public object EvalPosOrdem(Expressao expss, Escopo escopo)
        {

            if (expss.isModify == false)
                return expss.oldValue;
            else
            {
                if (!expss.isInPosOrdem)
                {
             
                    expss = expss.PosOrdemExpressao();
                    expss.isInPosOrdem = true;
                }

                expss.isModify = false;
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
                if (Expressao.Instance.IsTipoDouble(expss.ToString()))
                    return double.Parse(expss.ToString());
                else
                    if (expss.GetType() == typeof(ExpressaoObjeto))
                        return ((ExpressaoObjeto)expss).objeto.GetValor();
                else
                    if (expss.GetType() == typeof(ExpressaoVetor))
                    return ((ExpressaoVetor)expss).vetor.GetValor();
            }
            string tipoDaExpressao = Expressao.GetTipoExpressao(expss, escopo);

            object result1 = 0;
            Pilha<object> pilhaOperandos = new Pilha<object>("pilhaOperandos");

            for (int x = 0; x < expss.Elementos.Count; x++)
            {

                if (expss.Elementos[x].GetType() == typeof(ExpressaoAtribuicaoPropriedadesAninhadas))
                {
                    // coleta os dados do aninhamento de propriedades com atribuicao.
                    ExpressaoAtribuicaoPropriedadesAninhadas expressaAninhamentoAtribuicao = (ExpressaoAtribuicaoPropriedadesAninhadas)expss.Elementos[x];
                    List<Objeto> propriedades = expressaAninhamentoAtribuicao.aninhamento;
                    Expressao exprssAtribuicao = expressaAninhamentoAtribuicao.expresaoAtribuicao;

                    // constoi a propriedade a receber a atribuicao.
                    Objeto objetoChamada = expressaAninhamentoAtribuicao.objetoInicial;

                    for (int i = 1; i < propriedades.Count; i++)
                        objetoChamada = objetoChamada.GetField(propriedades[i].GetNome());

                    // calcula a expressao de atribuicao.
                    EvalExpression eval = new EvalExpression();
                    object result = eval.EvalPosOrdem(exprssAtribuicao, escopo);

                    // associa a expressão de atribuição, para a propriedade aninhada.
                    objetoChamada.SetValor(result);
      
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoChamadaDeMetodo))
                {
                    ExpressaoChamadaDeMetodo expssMain = (ExpressaoChamadaDeMetodo)expss.Elementos[x];
                    
                    Objeto objetoCaller = expssMain.objectCaller;
                    Objeto objetoChamada = null;

                    List<Objeto> propriedades = expssMain.proprieades.aninhamento;
                    if ((propriedades != null) && (propriedades.Count > 0))
                    {


                        foreach (Objeto objPropriedade in propriedades)
                            objetoChamada = objetoChamada.GetField(objPropriedade.GetNome());

                    }
                    else
                        objetoChamada = objetoCaller;
                       
                        // coleta os dados do metodo (funcao, parametros).
                        List<ExpressaoChamadaDeFuncao> expssChamada = expssMain.chamadaDoMetodo;
                        object result = null;
                        for (int i = 0; i < expssChamada.Count; i++)
                        {
                            Funcao metodoDaChamada = expssChamada[i].funcao;
                            List<Expressao> parametros = expssChamada[i].expressoesParametros;
                            // chama a avaliação do método.
                            result = metodoDaChamada.ExecuteAMethod(parametros, escopo, objetoChamada);
                            if (result.GetType() == typeof(Objeto))
                                objetoChamada = (Objeto)result;
                        }


                        if (result != null)
                            pilhaOperandos.Push(result);
                    
                }

                else
                if (linguagem.VerificaSeEhNumero(expss.Elementos[x].ToString()))
                    pilhaOperandos.Push((object)expss.Elementos[x].ToString());
                else
                if (expss.Elementos[x].GetType() == typeof(Expressao))
                {
                    // o elemento da expressão é outra expressão.
                    EvalExpression evalExpressaoElemento = new EvalExpression();
                    object result = evalExpressaoElemento.EvalPosOrdem(expss.Elementos[x], escopo); // avalia a expressão elemento.
                    pilhaOperandos.Push(result);
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoNumero))
                {
                    string strNumero = ((ExpressaoNumero)expss.Elementos[x]).numero;
                    object objNumero = Expressao.Instance.ConverteNumeroParaObjeto(strNumero, escopo);
                    pilhaOperandos.Push(objNumero);
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoVetor))
                {
                    // calculo de vetores multidimensionais!
                    Vetor vExp = ((ExpressaoVetor)expss.Elementos[x]).vetor;
                    Vetor vetor = escopo.tabela.GetVetor(vExp.nome, escopo);
                    int[] indices = new int[vetor.dimensoes.Length];

                    for (int i = 0; i < vetor.dimensoes.Length; i++)
                        indices[i] = (int)new EvalExpression().Eval((Expressao)expss.Elementos[x].Elementos[i].GetElemento(), escopo);

                    pilhaOperandos.Push(vetor.GetElemento(escopo, indices));

                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoObjeto))
                {
                    Objeto v = escopo.tabela.GetObjeto(((ExpressaoObjeto)expss.Elementos[x]).objeto.GetNome(), escopo);
                    if (v != null)
                    {
                        object valor = v.GetValor();
                        pilhaOperandos.Push(valor);
                    }
                    else
                        pilhaOperandos.Push(0);
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
                    object resultFunction = funcaoExpressao.ExecuteAFunction(parametrosDaChamada, funcaoExpressao.caller, escopo);
                    pilhaOperandos.Push(resultFunction);
                }
                else
                if (expss.Elementos[x].GetType() == typeof(ExpressaoElemento))
                {

                    // nova funcionalidade: elementos que podem ser o nome de variáveis.
                    Objeto v = escopo.tabela.GetObjeto(expss.Elementos[x].GetElemento().ToString(), escopo);
                    if (v != null)
                        pilhaOperandos.Push(v.GetValor());
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
                                Objeto vAtribuicao = escopo.tabela.GetObjeto(expss.Elementos[0].ToString(), escopo);
                                if (vAtribuicao != null)
                                {
                                    object valor = pilhaOperandos.Pop();
                                    vAtribuicao.SetValor(valor);
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

                        Objeto vAtribuicaoUnario = escopo.tabela.GetObjeto(expss.Elementos[0].ToString(), escopo);
                        if (vAtribuicaoUnario != null)
                        {
                            object valor = result1;
                            vAtribuicaoUnario.SetValor(valor);
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
            if (Expressao.Instance.IsTipoDouble(number.ToString()))
                return double.Parse(number.ToString());
            return number;
        }

    } //class EvalExpression

} // namespace
