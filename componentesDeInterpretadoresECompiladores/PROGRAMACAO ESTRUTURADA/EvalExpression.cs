using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{


    public class EvalExpression
    {

  
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

            if (Expressao.Instance.IsNumero(expss.ToString()))
                return Expressao.Instance.ConverteParaNumero(expss.ToString(), escopo);
            
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

                else
                    return null;
            }
            string tipoDaExpressao = Expressao.GetTipoExpressao(expss, escopo);

            object result1 = 0;
            Pilha<object> pilhaOperandos = new Pilha<object>("pilhaOperandos");
           
            for (int x = 0; x < expss.Elementos.Count; x++)
            {

                if (expss.Elementos[x].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                {
                    object valorPropriedadeAninhada = GetValorPropriedadeAninhada(expss.Elementos[0]);
                    pilhaOperandos.Push(valorPropriedadeAninhada);
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

                        Objeto objAninhado = objetoCaller;
                        for (int k=1; k> propriedades.Count;x++)

                        foreach (Objeto objPropriedade in propriedades)
                            objAninhado = objAninhado.GetField(objPropriedade.GetNome());

                        objetoChamada = objAninhado;
                    }
                    else
                        objetoChamada = objetoCaller;

                    // coleta os dados do metodo (funcao, parametros).
                    List<ExpressaoChamadaDeFuncao> expssChamada = expssMain.chamadaDoMetodo;
                    object result = null;
                    for (int i = 0; i < expssChamada.Count; i++)
                    {
                        Funcao metodoDaChamada = expssChamada[i].funcao;
                        result= metodoDaChamada.ExecuteAMethod(expssChamada[i].expressoesParametros, escopo,  objetoChamada); // chama o metodo, com o objetoChamada que contém o método.
                        // o objeto "objetoChamada é modificado com o processamento do método. FIXAR PARA METODOS ANINHADOS.
                        if (result != null)
                        {

                            if ((result.GetType() == typeof(Objeto)) || (result.GetType() == typeof(ExpressaoPropriedadesAninhadas))) 
                            {
                                objetoChamada = (Objeto)result;
                                result = objetoChamada;
                            }
                            else
                            if (result.GetType() == typeof(Vetor))
                            {
                                objetoChamada = (Vetor)result;
                                result = objetoChamada;
                            }
                            else
                                objetoChamada.SetValor(result); // para objetos importados da linguagem base.

                            pilhaOperandos.Push(result);
                        }
                        
                    }
               }
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
                    object objNumero = Expressao.Instance.ConverteParaNumero(strNumero, escopo);
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
                        if (v.GetValor() != null)
                        {
                            if (Expressao.Instance.IsNumero(v.GetValor().ToString())) // o objeto tem valor como numero.
                                pilhaOperandos.Push(v.GetValor().ToString());
                        }
                        else
                            pilhaOperandos.Push(v); // o objeto não é um numero, nem como valor- numero

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
                                object novoValor = pilhaOperandos.Pop();

                                if (expss.Elementos[0].GetType() == typeof(ExpressaoObjeto))
                                    escopo.tabela.GetObjeto(((ExpressaoObjeto)expss.Elementos[0]).objeto.GetNome(), escopo).SetValor(novoValor);
                                else
                                if (expss.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                                {
                                    Objeto objResult = ((ExpressaoPropriedadesAninhadas)expss.Elementos[0]).objetoInicial;
                                    SetValorPropriedadeAninhada(((ExpressaoPropriedadesAninhadas)expss.Elementos[0]), objResult, novoValor, escopo); 

                                }
                                else
                                 if (expss.Elementos[0].GetType() == typeof(ExpressaoVetor))
                                {
                                    ExpressaoVetor expssVetor = ((ExpressaoVetor)expss.Elementos[0]);
                                    ExpressaoOperadorMatricial enderacamentoIndice = expssVetor.indicesVetor;
                                    Vetor vetorAtribuicao = escopo.tabela.GetVetor(expssVetor.vetor.GetNome(), escopo);
                                    vetorAtribuicao.SetElementoAninhado(novoValor, escopo, enderacamentoIndice.indices.ToArray());
                                }
                                return novoValor;

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

                        object valor = operador.ExecuteOperador(operador.nome, escopo, oprnd2);
                        pilhaOperandos.Push(result1);
                        if (expss.Elementos[0].GetType() == typeof(ExpressaoObjeto))
                            ((ExpressaoObjeto)expss.Elementos[0]).objeto.SetValor(valor);
                        else
                        if (expss.Elementos[0].GetType() == typeof(ExpressaoPropriedadesAninhadas))
                            ((ExpressaoPropriedadesAninhadas)expss.Elementos[0]).SetValor(escopo, valor);

                    }
                } // if

            } // for x
            if (pilhaOperandos.lenghtPilha > 0)
                result1 = pilhaOperandos.Pop();

            return result1;
        } // Eval()

        private static object GetValorPropriedadeAninhada(Expressao expss)
        {
            ExpressaoPropriedadesAninhadas expressaoPropriedades = (ExpressaoPropriedadesAninhadas)expss;
            Objeto objAninhamento = expressaoPropriedades.aninhamento[0];


            for (int p = 1; p < expressaoPropriedades.aninhamento.Count; p++)
                objAninhamento = objAninhamento.GetField(expressaoPropriedades.aninhamento[p].GetNome());


            object valorPropriedadeAninhada = objAninhamento.GetValor();
            return valorPropriedadeAninhada;
        }

        private static object SetValorPropriedadeAninhada(ExpressaoPropriedadesAninhadas expressaoPropriedades, Objeto objAninhamento, object novoValor, Escopo escopo)
        {
          
            objAninhamento = expressaoPropriedades.aninhamento[0];


            for (int p = 1; p < expressaoPropriedades.aninhamento.Count; p++)
                objAninhamento = objAninhamento.GetField(expressaoPropriedades.aninhamento[p].GetNome());

            if (objAninhamento != null)
                objAninhamento.SetValor(novoValor);

            return novoValor;
        }




        /// <summary>
        ///  se for um numero, transforma para object. se não for um número, retorta o objeto de entrada (pois não é um número).
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
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
