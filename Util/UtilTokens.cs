using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using parser.ProgramacaoOrentadaAObjetos;
namespace parser
{
    public class UtilTokens
    {
        private static LinguagemOrquidea linguagem = new LinguagemOrquidea();

        /// <summary>
        /// escreve uma mensagem de erro na lista de mensagens no objeto escopo, com localização do código.
        /// </summary>
        public static void WriteAErrorMensage(Escopo escopo, string mensagemDeErro, List<string> tokensDOProcessamento)
        {
            PosicaoECodigo posicao = new PosicaoECodigo(tokensDOProcessamento);
            escopo.GetMsgErros().Add(mensagemDeErro + " , linha: " + posicao.linha + " , coluna: " + posicao.coluna + ".");
            
        }

        public static List<string> GetCodigoEntreOperadores(int indiceInicio, string operadorAbre, string operadorFecha, List<string> tokensEntreOperadores)
        {
            List<string> tokens = new List<string>();
            int pilhaInteiros = 0;
            pilhaInteiros = 0;
            int indexToken = indiceInicio;
            if (indiceInicio < 0)
                return new List<string>();


            while (indexToken < tokensEntreOperadores.Count)
            {
                if (tokensEntreOperadores[indexToken] == operadorAbre)
                {
                    tokens.Add(operadorAbre);
                    pilhaInteiros++;
                }
                else
                if (tokensEntreOperadores[indexToken] == operadorFecha)
                {
                    tokens.Add(operadorFecha);
                    pilhaInteiros--;
                    if (pilhaInteiros == 0)
                        return tokens;

                } // if
                else
                    tokens.Add(tokensEntreOperadores[indexToken]);
                indexToken++;
            } // While

            return tokens;
        } // GetCodigoEntreOperadores()



        /// <summary>
        /// método especializado para retirar listas de tokens, como na instrução "casesOfUse".
        /// 
        /// se houver [ini] e [fini1], retira a lista de tokens entre [ini] e [fini1].
        /// se não houver mais [ini1], retorna as listas de tokens.
        /// se não houver mais [fini1], e houver [ini], retira a lista de tokens entre [ini] até o final da lista de tokens.
        /// </summary>
        public static List<List<string>> GetCodigoEntreOperadoresCases(string tokenMarcadorAbre, string tokenMarcadorFecha, List<string> tokens)
        {
            List<List<string>> tokensRetorno = new List<List<string>>();
            int x = tokens.IndexOf(tokenMarcadorAbre);

            int offsetMarcadores = 0;
            while ((x >= 0) && (x < tokens.Count))
            {
                int indexStartBloco = tokens.IndexOf(tokenMarcadorAbre, offsetMarcadores);
                List<string> blocoDoCaseCurrente = GetCodigoEntreOperadores(indexStartBloco, tokenMarcadorAbre, tokenMarcadorFecha, tokens);


                if ((blocoDoCaseCurrente != null) && (blocoDoCaseCurrente.Count > 0))
                    tokensRetorno.Add(blocoDoCaseCurrente);

                offsetMarcadores += blocoDoCaseCurrente.Count;
                indexStartBloco = tokens.IndexOf(tokenMarcadorAbre, offsetMarcadores); // passa para o proximo bloco da instrucao case.
                if (indexStartBloco == -1)
                    return tokensRetorno;
            }

            return tokensRetorno;

        }  // GetCodigoEntreOperadoresCases()





        /// faz a conversao de tipos basicos de classes importados, para o sistema de tipos da linguagem.
        public static string Casting(string tipo)
        {
            if (tipo == "Single")
                return "float";

            if (tipo == "double")
                return "float";

            if (tipo == "Int32")
                return "int";

            if (tipo == "Float")
                return "float";

            if (tipo == "Double")
                return "double";

            if (tipo == "Bool")
                return "bool";

            if (tipo == "String")
                return "string";

            if (tipo == "Char")
                return "char";

            if (tipo == "Boolean")
                return "bool";
            return tipo;
        }

        /// <summary>
        /// obtem uma funcao de uma classe especificada com parametros de funcao, compativel com o nome e parametros.
        /// </summary>
        public static Funcao ObtemFuncaoCompativelComAChamadaDeFuncao(string nomeMetodo, string nomeClasse, List<Expressao> expressaoParametros, Escopo escopo)
        {
            List<Funcao> FuncoesCandidatosDaChamada = new List<Funcao>();

            Classe classeAProcurarMetodo = RepositorioDeClassesOO.Instance().GetClasse(nomeClasse);
            if (classeAProcurarMetodo == null)
                return null;

            List<Funcao> metodosDaClasse = classeAProcurarMetodo.GetMetodos();
            if (metodosDaClasse == null)
                return null;

            if (metodosDaClasse != null)
            {
                List<Funcao> metodosCompativeis = metodosDaClasse.FindAll(k => k.nome.Equals(nomeMetodo));
                if ((metodosCompativeis != null) && (metodosCompativeis.Count > 0))
                    FuncoesCandidatosDaChamada.AddRange(metodosCompativeis);
            }

            if (FuncoesCandidatosDaChamada.Count == 0)
                return null;

            for (int umaFuncao = 0; umaFuncao < FuncoesCandidatosDaChamada.Count; umaFuncao++)
            {
                if ((FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao == null) && (expressaoParametros.Count == 0))
                    return FuncoesCandidatosDaChamada[umaFuncao];

                if (FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao.Length != expressaoParametros.Count) // numero de parametros nao combinam.
                    continue;

                bool isFound = ValidaParametrosCompativeis(expressaoParametros, escopo, FuncoesCandidatosDaChamada[umaFuncao]);
                if (isFound)
                    return FuncoesCandidatosDaChamada[umaFuncao];
            }
            return null;
        }

        private static bool ValidaParametrosCompativeis(List<Expressao> expressaoParametros, Escopo escopo, Funcao funcaoCandidata)
        {
            bool isFound = true;
            for (int x = 0; x < expressaoParametros.Count; x++)
            {
                string tipoParametroDaExpressao = UtilTokens.Casting(Expressao.GetTipoExpressao(expressaoParametros[x], escopo));
                string tipoFuncaoCandidata = UtilTokens.Casting(funcaoCandidata.parametrosDaFuncao[x].GetTipo());

                if (linguagem.VerificaSeEhNumero(expressaoParametros[x].ToString()))
                {
                    if (Expressao.Instance.IsTipoInteiro(expressaoParametros[x].ToString()))
                        tipoParametroDaExpressao = "int";
                    else
                    if (Expressao.Instance.IsTipoFloat(expressaoParametros[x].ToString()))
                        tipoParametroDaExpressao = "float";
                    else
                    if (Expressao.Instance.IsTipoDouble(expressaoParametros[x].ToString()))
                        tipoParametroDaExpressao = "double";
                }

                if (((tipoParametroDaExpressao == "float") && (tipoFuncaoCandidata == "double")) ||
                    (tipoParametroDaExpressao == "double") && (tipoFuncaoCandidata == "float"))
                    continue;

                if (tipoParametroDaExpressao != tipoFuncaoCandidata)
                {
                    isFound = false;
                    break;
                }

            }

            return isFound;
        }

        /// <summary>
        /// obtem uma funcao com nome da entrada, e mesma interface de parametros (tipos de cada parâmetro).
        /// </summary>
        public static Funcao ObtemFuncaoCompativelComAChamadaDeFuncao(string nomeMetodo,List<Expressao> expressaoParametros, Escopo escopo)
        {
            List<Funcao> FuncoesCandidatosDaChamada = new List<Funcao>();
            
            for (int x = 0; x < RepositorioDeClassesOO.Instance().GetClasses().Count; x++)
            {
                Classe classeAProcurarMetodo = RepositorioDeClassesOO.Instance().GetClasses()[x];
                List<Funcao> metodosDaClasse = classeAProcurarMetodo.GetMetodos();
                if (metodosDaClasse != null)
                {
                    List<Funcao> metodosCompativeis = metodosDaClasse.FindAll(k => k.nome.Equals(nomeMetodo));
                    if ((metodosCompativeis != null) && (metodosCompativeis.Count > 0))
                        FuncoesCandidatosDaChamada.AddRange(metodosCompativeis);
                }
          
            }
            if (FuncoesCandidatosDaChamada.Count == 0)
            {
                List<Funcao> fnc = escopo.tabela.GetFuncoes().FindAll(k => k.nome == nomeMetodo);
                if (fnc != null)
                    FuncoesCandidatosDaChamada = fnc;
            }
  
            for (int umaFuncao = 0; umaFuncao < FuncoesCandidatosDaChamada.Count; umaFuncao++)
            {
                if ((FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao == null) && (expressaoParametros.Count == 0))
                    return FuncoesCandidatosDaChamada[umaFuncao];
                if (FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao == null)
                    continue;

                if (FuncoesCandidatosDaChamada[umaFuncao].parametrosDaFuncao.Length != expressaoParametros.Count) // numero de parametros nao combinam.
                    continue;

                bool isFound = ValidaParametrosCompativeis(expressaoParametros, escopo, FuncoesCandidatosDaChamada[umaFuncao]);
                if (isFound)
                    return FuncoesCandidatosDaChamada[umaFuncao];
            }
            return null;
        }

    }

}
