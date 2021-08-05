using System.Collections.Generic;
using System.Collections;
using System.Linq;
using parser.ProgramacaoOrentadaAObjetos;
using System.Security.Principal;
using System;
using System.IO;
using System.Reflection;

using parser.LISP;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.SqlServer.Server;
using Util;

namespace parser
{

    public class TablelaDeValores
    {


        /*
         * 
        // MÉTODOS DA CLASSE TabelaDeValores

        /// PARA FUNÇÕES:
        public Funcao GetFuncao(string nomeFuncao, string tipoRetornoFuncao, escopo)
        public Funcao GetFuncao(string nomeFuncao)
        public bool RegistraFuncao(Funcao funcao)
        public string GetTypeFunction(string nomeClasseDaFuncao, string nomeFuncao, Escopo escopo)
        public Funcao IsFunction(string nomeFuncao, Escopo escopo)
       

        // PARA VARIAVEIS:
        public List<Variavel> GetVariaveis()
        public List<VariavelVetor> GetVariaveisVetor()
        public bool IsVariavelSingle(string nomeVariavel, Escopo escopo)
        public bool IsVariavelVector(string nomeVariavel, Escopo escopo)
        public bool ValidaNomeDaVariavel(string nomeDaVariavel, Escopo escopoCurrente)
        public void AddVar(string acessor, string nome,string tipo, object valor, PosicaoECodigo posicao, Escopo escopo, bool isStatic)
        public void AddVarVetor(string acessor, string nome, string tipo, int[] dims, Escopo escopo, bool isStatic)
        public Variavel GetVar(string nomeVariavel, Escopo escopo)
        public VariavelVetor GetVarVetor(string nomeVariavel, Escopo escopo, params int[] indices)
        public object GetVarValue(string variavel, Escopo escopoCurrente)
        public bool ValidaVariavel(string nomeVariavel, string tipoDaVariavel, Escopo escopo)
        public bool ValidaVariavel(string nomeVariavel, Escopo escopo)
        public bool ValidaTipoVariavel( string tipoVariavel, Escopo escopo)
        public bool SetaVariavel(string nomeVariavel, object valor, Escopo escopo)
        
        
 
        // PARA CLASSES        
        public List<Classe> GetClasses()
        public void RegistraClasse(Classe umaClasse)
        public propriedade GetPropriedade(string nomeClasse, string nomePropriedade)
        public bool ValidaTipo(string tipo)
    
        
        // PARA OBJETOS
        public Objeto GetObjeto(string nomeObjeto)
        public void RegistraObjeto(Objeto objeto)
        public void RemoveObjeto(string nomeObjeto)
        private propriedade ObtemPropriedadeDeUmObjeto(string tipoObjeto, string propriedadeDoObjeto)
        private Funcao ObtemMetodoDeUmObjeto(string tipoObjeto, string metodoDoObjeto)
        public Objeto IsObjetoRegistrado(string nomeObjeto, Escopo escopo)
        public bool ValidaObjeto(string nomeObjeto, Escopo escopo)
        
        
        // PARA OPERADORES:
        public bool ValidaOperador(string nomeOperador, string tipoDoOPerador, Escopo escopoCurrente)
        public static int VerificaSeEhOperador(string operador, string tipoDeOperando)
       
       
        // PARA PROPRIEDADES:
        private List<propriedade> ObtemPropriedadesEncadeadas(List<string> tokens, Escopo escopo, int indiceInicio)

        // PARA EXPRESSOES:       
        private string ObtemOTipoDaExpressao(List<string> tokens, Escopo escopo)
        public static int VerificaSeEhTermoChave(string tokenChave, List<string> termosChave)
 
         
         // PROPRIEDADES PUBLICAS:
        public List<Operador> Operadores = new List<Operador>();
         
         */



        private List<Classe> Classes = new List<Classe>();
        private List<Operador> operadores = new List<Operador>();
      

        internal List<Expressao> expressoes = new List<Expressao>(); // contém as expressões validadas no escopo currente.
        private List<Funcao> Funcoes = new List<Funcao>(); // contém as funções do escopo currente.

        public List<Variavel> Variaveis = new List<Variavel>();  // variáveis objeto da tabela de valores.
        private List<VariavelVetor> VariaveisVetor = new List<VariavelVetor>(); // variaveis vetor.
        


        private List<Objeto> objetos = new List<Objeto>(); // contém uma lista de objetos instanciados.
        
        private List<string> codigo { get; set; }

        private static LinguagemOrquidea lng = new LinguagemOrquidea();
        public TablelaDeValores(List<string> _codigo)
        {
            lng = new LinguagemOrquidea();
            if ((_codigo != null) && (_codigo.Count > 0))
                this.codigo = _codigo.ToList<string>();
            else
                this.codigo = new List<string>();
        } //TabelaDeValores()

        
        public List<Expressao> GetExpressoes()
        {
            return this.expressoes;
        } // GetExpressoes()

        public List<Operador> GetOperadores()
        {
            return this.operadores;
        }
        public List<Objeto> GetObjetos()
        {
            return this.objetos;
        }

        public List<Funcao> GetFuncoes()
        {
            return Funcoes;
        } // GetFuncoes()

        /// <summary>
        /// obtém a função nesta tabela de valores, e se não encontrar, obtém a função no repositorio de classes orquidea.
        /// </summary>
        public Funcao GetFuncao(string nomeFuncao, string classeDaFuncao, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                  return FindFuncao(classeDaFuncao, nomeFuncao, escopo);

            Funcao fuctionFound = FindFuncao(classeDaFuncao, nomeFuncao, escopo);
            if (fuctionFound != null) 
                return fuctionFound;
            else
                return GetFuncao(nomeFuncao, classeDaFuncao, escopo.escopoPai);
        } // GetFuncao()



        private  Funcao FindFuncao(string classeDaFuncao, string nomeFuncao, Escopo escopo)
        {
        
            Funcao umaFuncaoDasTabelas = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (umaFuncaoDasTabelas != null)
                return escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);

            Funcao umaFuncaoDoRepositorio = RepositorioDeClassesOO.Instance().ObtemUmaClasse(classeDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
            if (umaFuncaoDoRepositorio != null)
                return umaFuncaoDoRepositorio;
            return null;
        }


        /// <summary>
        /// obtem funcoes com o nome parâmetro, não importando a classe em que a função foi definida.
        /// </summary>
        public List<Funcao> GetFuncao(string nomeFuncao)        {

            List<Funcao> funcoesDaTabelaComMesmoNome = this.Funcoes.FindAll(k => k.nome.Equals(nomeFuncao));

            if (funcoesDaTabelaComMesmoNome != null)
                return funcoesDaTabelaComMesmoNome;

            List<Funcao> funcoesDoRepositorioDeClassesComMesmoNome = new List<Funcao>();
            List<Classe> lstClasses = RepositorioDeClassesOO.Instance().classesRegistradas;
            foreach (Classe umaClasse in lstClasses)
            {
                List<Funcao> lstFuncoes = umaClasse.GetMetodos().FindAll(k => k.nome.Equals(nomeFuncao));
               if (lstFuncoes!=null)
                {
                    funcoesDoRepositorioDeClassesComMesmoNome.AddRange(lstFuncoes);
                } // if
            } // foreach
            return funcoesDoRepositorioDeClassesComMesmoNome;
        } // GetFuncao()

        public void RemoveVar(string nomeDaVariavel)
        {
            Variavel v = this.Variaveis.Find(k => k.GetNome().Equals(nomeDaVariavel));
            if (v != null)
                this.Variaveis.Remove(v);
        }
        public List<Variavel> GetVariaveis()
        {
            return Variaveis;
        }
        public List<VariavelVetor> GetVariaveisVetor()
        {
            return VariaveisVetor;
        }
        public List<Classe> GetClasses()
        {
           return Classes;
        }

        public Classe GetClasse(string nomeDaClasse, Escopo escopo)
        {
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return escopo.tabela.Classes.Find(k => k.nome.Equals(nomeDaClasse));
            else
            {
                Classe classe = escopo.tabela.Classes.Find(k => k.GetNome().Equals(nomeDaClasse));
                if (classe != null)
                    return classe;

                if (classe == null)
                    return GetClasse(nomeDaClasse, escopo.escopoPai);
            }
            return null;
        }

        public Variavel GetVariavelEstatica(string nomeDaVariavel, string classeDaVariavel)
        {
            Classe classe = this.GetClasses().Find(k => k.nome.Equals(classeDaVariavel));
            if (classe == null)
                return null;
            Variavel variavelEstatica = classe.propriedadesEstaticas.Find(k => k.GetNome() == nomeDaVariavel);
            if (variavelEstatica != null)
                return variavelEstatica;
            Variavel variavelEstaticaNomeLongo = classe.propriedadesEstaticas.Find(k => k.GetNomeLongo() == nomeDaVariavel);
            if (variavelEstaticaNomeLongo != null)
                return variavelEstaticaNomeLongo;
            return null;

        }

        public static TablelaDeValores Clone(TablelaDeValores tabela)
        {
            TablelaDeValores tabelaClone = new TablelaDeValores(tabela.codigo);
            if (tabela.GetClasses().Count > 0)
                tabelaClone.GetClasses().AddRange(tabela.GetClasses());
            
            if (tabela.GetExpressoes().Count > 0)
                tabelaClone.GetExpressoes().AddRange(tabela.GetExpressoes());

            if (tabela.GetFuncoes().Count > 0)
                tabelaClone.GetFuncoes().AddRange(tabela.GetFuncoes());

            if (tabela.GetObjetos().Count > 0)
                tabelaClone.GetObjetos().AddRange(tabela.GetObjetos());

            if (tabela.GetOperadores().Count > 0)
                tabelaClone.GetOperadores().AddRange(tabela.GetOperadores());

            if (tabela.GetVariaveis().Count > 0)
                tabelaClone.GetVariaveis().AddRange(tabela.GetVariaveis());

            if (tabela.GetVariaveisVetor().Count > 0)
                tabelaClone.GetVariaveisVetor().AddRange(tabela.GetVariaveisVetor());


            return tabelaClone;
            
        }


        public void RegistraClasse(Classe umaClasse)
        {
            Classes.Add(umaClasse);
            this.operadores.AddRange(umaClasse.GetOperadores());
        } // RegistraClasse()
        public Objeto GetObjeto(string nomeObjeto)
        {
            Objeto obj = objetos.Find(k => k.GetNome().Trim(' ') == nomeObjeto.Trim(' '));
            return obj;
        } // GetObjeto().

     

        public propriedade GetPropriedade(string nomeClasse, string nomePropriedade)
        {
            // tenta localizar a classe da propriedade.
            Classe umaClasse = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasse);

            // se não localizar, retorna null.
            if (umaClasse == null)
                return null;
            
            // tenta localizar a propriedade da classe encontrada.
            int indexPropriedade = umaClasse.GetPropriedades().FindIndex(k => k.GetNome()== nomePropriedade.Trim(' '));
            // se não localizar, retorna null.
            if (indexPropriedade == -1)
            {
                indexPropriedade = umaClasse.GetPropriedades().FindIndex(k => k.GetNome() == "static." + nomePropriedade.Trim(' '));
                if (indexPropriedade == -1)
                    return null;
                else
                    return umaClasse.GetPropriedades().Find(k => k.GetNome() == "static." + nomePropriedade.Trim(' '));
            }
            // retorna a propriedade encontrada.
            return umaClasse.GetPropriedades()[indexPropriedade];

        } // GetPropriedade()
        public void RegistraObjeto(Objeto objeto)
        {
            this.objetos.Add(objeto);
        } // RegistraObjeto().
        public void RemoveObjeto(string nomeObjeto)
        {

            int index = this.objetos.FindIndex(k => k.GetNome() == nomeObjeto);
            if (index != -1)
                this.objetos.RemoveAt(index);

        } // RemoveObjeto()
        public bool RegistraFuncao(Funcao funcao)
        {
            this.Funcoes.Add(funcao);
            return true;
        }
     

        public bool ValidaTipo(string tipo)
        {
            for (int x = 0; x < this.Classes.Count; x++)
                if (this.Classes[x].GetNome() == tipo)
                    return true;
            return false;
        } // ValidaTipo()


        public bool ValidaOperador(string nomeOperador, string tipoDoOPerador, Escopo escopo)
        {

            Classe classeDoOperador = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoDoOPerador);
            if (classeDoOperador != null)
            {
                if (classeDoOperador.GetOperadores().Find(k => k.nome.Equals(nomeOperador)) != null)
                    return true;
            }
            if (escopo.tabela.GetOperadores().Find(k => k.nome.Equals(nomeOperador) && (k.tipoRetorno.Equals(tipoDoOPerador))) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaOperador(nomeOperador, tipoDoOPerador, escopo.escopoPai);
            return false;
        } // ValidaOperador()
    
        /// <summary>
        /// retorna [true] se a variável já foi declarada, [false] se não.
        /// </summary>
        public bool ValidaNomeDaVariavel(string nomeDaVariavel, Escopo escopoCurrente)
        {
            for (int x = 0; x < escopoCurrente.tabela.GetVariaveis().Count; x++)
            {
                if (escopoCurrente.tabela.GetVariaveis()[x].GetNome() == nomeDaVariavel.TrimStart(' ').TrimEnd(' '))
                    return true;
            } // for x
            return false;
        } // ValidaNomeDaVariavel()

        /// <summary>
        ///  obtém uma propriedade, que é de uma sequencia de propriedades aninhadas.
        /// </summary>
        internal propriedade ValidaPropriedadesAninhadas(string classeDaPropriedade, string nomeDaPropriedade)
        {
            Classe clsObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(classeDaPropriedade);
            if (clsObjeto == null)
                return null;

            // "class A { int varA;} class B {A varB;}" };
            // propriedade prop = escopo.tabela.ObtemPropriedadeAninhada("B", "varB.varA");

            List<propriedade> props = new List<propriedade>();
            string[] proppriedadesAninhadas = nomeDaPropriedade.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            for (int x = 0; x < proppriedadesAninhadas.Length; x++)
            {
                string nomeLongo = clsObjeto.GetNome() + "." + proppriedadesAninhadas[x];

                propriedade proprSendoVerificada = clsObjeto.GetPropriedades().Find(k => k.GetNome().Equals(nomeLongo));
                if (proprSendoVerificada == null)
                    return null;
                props.Add(proprSendoVerificada);

                clsObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(props[props.Count - 1].tipo);
            } // for x
            if (props.Count < 0)
                return null;
            return props[props.Count - 1];
        } //GetPropriedadeEncadeadaDeUmaExpressao()




        /// <summary>
        /// obtém uma propriedade, se tiver especificada na classe do objeto de entrada.
        /// </summary>
        private propriedade ObtemPropriedade(string tipoObjeto, string propriedadeDoObjeto)
        {
            Classe clsObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoObjeto);
            if (clsObjeto == null)
                return null;

            int indexProp = clsObjeto.GetPropriedades().FindIndex(k => k.GetNome() == tipoObjeto + "." + propriedadeDoObjeto);
            if (indexProp != -1)
                return clsObjeto.GetPropriedades()[indexProp];
            return null;

        } //GetPropriedadeDeUmaExpressao()

        

        private Funcao ObtemMetodoDeUmObjeto(string tipoObjeto, string metodoDoObjeto)
        {
            Classe clsObjeto = RepositorioDeClassesOO.Instance().ObtemUmaClasse(tipoObjeto);
            if (clsObjeto == null)
                return null;

            int indexMetodo = clsObjeto.GetMetodos().FindIndex(k => k.nome == metodoDoObjeto);
            if (indexMetodo != -1)
                return clsObjeto.GetMetodos()[indexMetodo];
            return null;

        } // ObtemMetodoDeUmObjeto()


        public bool IsClasse(string nomeClasse, Escopo escopo)
        {
            if (escopo == null)
                return false;
            int indexClasse = escopo.tabela.GetClasses().FindIndex(k => k.GetNome() == nomeClasse);
            if (indexClasse != -1)
                return true;
            if (escopo.ID == Escopo.tipoEscopo.escopoNormal)
                return IsClasse(nomeClasse, escopo.escopoPai);
            return false;

        }  // IsClasse()

    
 

        public void AddVar(string acessor, string nome, string tipo, object valor, Escopo escopo, bool isStatic)
        {
            Variavel v = new Variavel(acessor, nome, tipo, valor);
            v.isStatic = isStatic;

            this.Variaveis.Add(v);
        }  // RegistraVariavel()


        public void AddVarVetor(string acessor, string nome, string tipo, int[] dims, Escopo escopo, bool isStatic)
        {
            VariavelVetor v = new VariavelVetor(acessor, tipo, nome, dims);
            v.SetAcessor(acessor);
            v.isStatic = isStatic;
            escopo.tabela.VariaveisVetor.Add(v);
        } // RegistraVariavelVetor()


        public Variavel GetVar(string nomeVariavel, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Variavel v = escopo.tabela.Variaveis.Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
                return v;
            Variavel vEstatica = escopo.tabela.Variaveis.Find(k => k.GetNome() == ("static." + k.GetNome()));
            if (vEstatica != null)
                return vEstatica;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetVar(nomeVariavel, escopo.escopoPai);
            return null;
        } //GetVariavel()

        public VariavelVetor GetVarVetor(string nomeVariavel, Escopo escopo, params int[] indices)
        {
            if (escopo == null)
                return null;
            VariavelVetor v = escopo.tabela.VariaveisVetor.Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
            {
                return GetElementoVetor(indices, ref v);
            } // if
            VariavelVetor vEstatica = escopo.tabela.VariaveisVetor.Find(k => k.GetNome() == "static." + nomeVariavel);
            if (vEstatica != null)
            {
                return GetElementoVetor(indices, ref v);
            }

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetVarVetor(nomeVariavel, escopo.escopoPai);
            return null;
        } // GetVarVetor()

        public VariavelVetor GetVarVetor(string nomeVariavel, Escopo escopo)
        {
            if (escopo == null)
                return null;
            VariavelVetor v = escopo.tabela.VariaveisVetor.Find(k => k.GetNome() == nomeVariavel);

            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return null;
            else
                return GetVarVetor(nomeVariavel, escopo.escopoPai);
        } // GetVarVetor()


        private static VariavelVetor GetElementoVetor(int[] indices, ref VariavelVetor v)
        {
            VariavelVetor vt_result = new VariavelVetor(v.GetAcessor(), v.GetTipo(), v.GetNome(), v.dimensoes);
            for (int index = 0; index < indices.Length; index++)
                vt_result = vt_result.tailVetor[indices[index]];
            return vt_result;
        }

        public string GetTypeFunction(string nomeClasseDaFuncao, string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Funcao funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao.tipoDoRetornoDaFuncao;

            Funcao umaFuncaoDoRepositorioDeClasses = RepositorioDeClassesOO.Instance().ObtemUmaClasse(nomeClasseDaFuncao).GetMetodos().Find(k => k.nome.Equals(nomeFuncao));
            if (umaFuncaoDoRepositorioDeClasses != null)
                return umaFuncaoDoRepositorioDeClasses.tipoDoRetornoDaFuncao;

            if (escopo.escopoPai.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetTypeFunction(nomeClasseDaFuncao, nomeFuncao, escopo.escopoPai);
            return null;
        }

      

        public Objeto IsObjetoRegistrado(string nomeObjeto, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Objeto objeto = escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto);
            if (objeto != null)
                return objeto;
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsObjetoRegistrado(nomeObjeto, escopo.escopoPai);
            return null;
        } // IsObjectRegistrade()



        public bool IsFunction(Escopo escopo, string nameFuncion)
        {
            return (IsFunction(nameFuncion, escopo) != null);
        }


        /// <summary>
        /// verifica se um token é nome de uma função.
        /// </summary>
        public Funcao IsFunction(string nomeFuncao, Escopo escopo)
        {
            if (escopo == null)
                return null;
            Funcao funcao = escopo.tabela.Funcoes.Find(k => k.nome == nomeFuncao);
            if (funcao != null)
                return funcao;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return IsFunction(nomeFuncao, escopo.escopoPai);
            return null;
        } // IsFunction()

        public object GetVarValue(string variavel, Escopo escopoCurrente)
        {
            if (escopoCurrente == null)
                return null;
            Variavel v = escopoCurrente.tabela.Variaveis.Find(k => k.GetNome() == variavel);
            if (v != null)
                return v.GetValor();
            Variavel vEstatica = escopoCurrente.tabela.Variaveis.Find(k => k.GetNome() == "static." + variavel);
            if (v != null)
                return v.GetValor();


            if (escopoCurrente.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetVarValue(variavel, escopoCurrente.escopoPai);
            return null;
        }
        /// <summary>
        /// valida a variável se já existir uma definição da variável, de mesmo tipo.
        /// </summary>
        public bool ValidaVariavel(string nomeVariavel, string tipoDaVariavel, Escopo escopo)
        {
            if (escopo == null)
                return false;
            Variavel v = escopo.tabela.Variaveis.Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
            {
                if (v.GetTipo() == tipoDaVariavel)
                    return true;
                else
                    return false;
            }

            Variavel vEstatica = escopo.tabela.Variaveis.Find(k => k.GetNome() == "static."+nomeVariavel);
            if (v != null)
            {
                if (v.GetTipo() == tipoDaVariavel)
                    return true;
                else
                    return false;
            }

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaVariavel(nomeVariavel, tipoDaVariavel, escopo.escopoPai);
            return false;
        } // ValidaVariavel()

        public bool ValidaVariavel(string nomeVariavel, Escopo escopo)
        {
            if (escopo == null)
                return false;
            Variavel v = escopo.tabela.Variaveis.Find(k => k.GetNome() == nomeVariavel);
            if (v != null)
                return true;

            Variavel vEstatica = escopo.tabela.Variaveis.Find(k => k.GetNome() == "static."+nomeVariavel);
            if (vEstatica != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaVariavel(nomeVariavel, escopo.escopoPai);
            return false;
        } // ValidaVariavel()


        public bool ValidaObjeto(string nomeObjeto, Escopo escopo)
        {

            if (escopo == null)
                return false;
            if (escopo.tabela.objetos.Find(k => k.GetNome() == nomeObjeto) != null)
                return true;

            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return ValidaObjeto(nomeObjeto, escopo.escopoPai);
            return false;
        } // ValidaVariavel()

        public bool ValidaTipoVariavel( string tipoVariavel, Escopo escopo)
        {
            if (escopo == null)
                return false;
            foreach (Classe umaClasse in escopo.tabela.Classes)
            {
                if (umaClasse.GetNome() == tipoVariavel)
                    return true;
            }// foreach

            
            if (escopo.ID != Escopo.tipoEscopo.escopoGlobal)
                return (ValidaTipoVariavel(tipoVariavel, escopo.escopoPai));
            return false;
        } // ValidaTipoVariavel()

        public bool SetaVariavel(string nomeVariavel, object valor, Escopo escopo)
        {
            if (escopo == null)
                return false;
            Variavel v= escopo.tabela.GetVar(nomeVariavel, escopo);
            if (v != null)
            {
                v.valor = valor;
                return true;
            }
            Variavel vEstatica = escopo.tabela.GetVar("static." + nomeVariavel, escopo);
            if (vEstatica != null)
            {
                vEstatica.valor = valor;
                return true;
            }
            else
            if (escopo.ID == Escopo.tipoEscopo.escopoGlobal)
                return false;
            else
                return SetaVariavel(nomeVariavel, valor, escopo);
            
        }

        

    } // class TabelaDeValores

    public class Variavel
    {
        private string acessor = "";
        private string nome = "";
        private string nomeLongo = "";
        private string tipo = "";
        public object valor = "";
        public bool isStatic = false;
   
 
        private static Random aleatorizador = new Random();
        public List<propriedade> propriedades { get; set; }


        public Variavel(string acessor, string name, string type, object value)
        {
            Init(acessor, name, type, value);
      
        }


        public Variavel(string acessor, string name, string type, object value, bool isEstatica)
        {
            Init(acessor, name, type, value);
            this.isStatic = isEstatica;
        }

        private void Init(string acessor, string name, string type, object value)
        {
            if (acessor == "null")
                this.acessor = "protected";
            else
                this.acessor = acessor;
            this.nome = name;
            this.nomeLongo = type + "." + this.nome;

            this.tipo = type;
            this.valor = value;

            this.propriedades = new List<propriedade>();
        }



        public string GetAcessor()
        {
            return acessor;
        }

        internal void SetAcessor(string acessor)
        {
            this.acessor = acessor;
        }

        public string GetNome()
        {
            return nome;
        }

        public void SetNome(string novoNome)
        {
            this.nome = novoNome;
        }

        public string GetNomeLongo()
        {
            return this.tipo + "." + this.nome;
        }
        public void SetNomeLongo(string nome)
        {
            this.nomeLongo = nome;
        }

        public string GetTipo()
        {
            return tipo;
        }
        public object GetValor()
        {
            return valor;
        }
      
     

        public static string GetTipoVariavelObjeto(string nomeVariavel, Escopo escopoCurrente)
        {
            if (escopoCurrente == null)
                return null;
            // percorre a tabela do escopo currente, para verificar se há váriavel para o nomeVariavel.
            foreach (Variavel umaVariavel in escopoCurrente.tabela.GetVariaveis())
                if (umaVariavel.GetNome() == nomeVariavel)
                    return umaVariavel.GetTipo();

            // para a pesquisa se o escopo currente for o global, não há escopo pai.
            if (escopoCurrente.ID != Escopo.tipoEscopo.escopoGlobal)
                return GetTipoVariavelObjeto(nomeVariavel, escopoCurrente.escopoPai);
            return null;
        } // GetTipoVariavel()

        public void SetValor(object newValue, Escopo escopo)
        {
            Variavel v = escopo.tabela.GetVar(this.nome, escopo);
            v.valor = newValue;
            SetModificacaoExpressao(escopo); // hook para otimização de cálculo de expressões.
        } // SetVariavelObjeto()


        /// <summary>
        /// otimização. as expressoes não são modificadas a todo tempo da execução do código.
        /// Quando uma variavel recebe um novo valor, todas expressões que tem essa variavel precisam ser
        /// recalculadas, quando for solicitada sua avaliação como expressão.
        /// </summary>
        /// <param name="escopo">escopo que contém as expressões monitoradas.</param>
        protected void SetModificacaoExpressao(Escopo escopo)
        {

            for (int umaExpressao = 0; umaExpressao < escopo.tabela.GetExpressoes().Count; umaExpressao++)
            {
                Expressao expressaoCurrente = escopo.tabela.GetExpressoes()[umaExpressao];
                for (int variavel = 0; variavel < expressaoCurrente.Elementos.Count; variavel++)
                {
                    if (expressaoCurrente.Elementos[variavel].ToString().Equals(this.GetNome()))
                    {
                        expressaoCurrente.isModdfy = true;
                        break;
                    } // if
                } // for variavel

            } // for umaExpressao
        }

        public override string ToString()
        {
            string str = "";
            if ((this.acessor != null) && (this.acessor != ""))
                str += this.acessor + " ";
            if ((this.nome != null) &&(this.nome != ""))
                str += "variavel: " + this.nome;
            if ((this.tipo != null) && (this.tipo != ""))
                str += "  tipo: " + this.tipo;
            if (this.valor != null)
                str += " valor: " + this.valor.ToString();
            return str;
        } // ToString()

    } // class Variavel


    public class VariavelVetor: Variavel
    {
        public string nome;
        private string nomeTipo;

        public List<VariavelVetor> tailVetor;

        public int[] dimensoes;


        public new string GetTipo()
        {
            return nomeTipo;
        }




        public VariavelVetor(string nomeAcessor, string nomeTipoDoVetor, string nomeVariavel, int[] dims) : base(nomeAcessor, nomeTipoDoVetor, nomeVariavel, null)
        {
            this.nomeTipo = nomeTipoDoVetor;
            this.nome = nomeVariavel;
            this.dimensoes = dims;

            this.tailVetor = new List<VariavelVetor>();
            
            if (this.dimensoes == null)
                this.dimensoes = new int[1];

            for (int x = 0; x < dims.Length; x++)
                this.tailVetor.Add(new VariavelVetor(nomeAcessor, nomeTipoDoVetor, nomeVariavel, dims));
        }

     


        /// <summary>
        /// constroi um indice de acessso de vetores com varias dimensoes. Eh um offset de
        /// endereço onde esta localizado a variavel que queremos acessar, dentro da variavel vetor.
        /// Util quando temos um vetor como: vetor[4,5,7], e queremos o elemento vetor[1,2,5].
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        public int BuildIndex(int[] indices)
        {
            if (indices.Length != this.dimensoes.Length)
                return -1;

            int indiceTotal = 0;
            int dimsAtual = 1;
            for (int x = 0; x < this.dimensoes.Length; x++)
            {
                dimsAtual = dimsAtual * this.dimensoes[x];
                indiceTotal += dimsAtual * indices[x];
            }
            return indiceTotal;
        }

        /// <summary>
        /// seta o elemento com os indices matriciais de entrada. 
        /// Util para modificar um elemento com dimensões bem definidas: M[1,5,8] um vetor, e queremos acessar
        /// a variavel: m[0,0,3].
        /// </summary>
        /// <param name="indices">indices matriciais.</param>
        /// <param name="newValue">novo valor para o elemento.</param>
        /// <param name="escopo">contexto onde expressoes que contém a variavel com novo valor.</param>
        public void SetElementoPorOffset(int[] indices, object newValue, Escopo escopo)
        {
            int indiceElemento = this.BuildIndex(indices);
            this.tailVetor[indiceElemento].SetValor(newValue, escopo);
            
        }

        /// <summary>
        /// seta elemento com elementos vetor dentro de vetores, como: [[1,5],2,6,8,[1,3,5]].
        /// </summary>
        /// <param name="newValue">valor do elemento que queremos atribuir.</param>
        /// <param name="escopo">escopo que contém expressões que contém este vetor currente.</param>
        /// <param name="indices">indices matriciais para localização do elemento a ter novo valor.</param>
        public void SetElementoAninhado(object newValue, Escopo escopo, params int[] indices)
        {
            VariavelVetor v = this;
            int x = 0;
            for (x = 0; x < indices.Length - 1; x++)
                if (v.tailVetor[indices[x]].GetType() == typeof(VariavelVetor))
                    v = v.tailVetor[indices[x]];
            v = v.tailVetor[indices[indices.Length - 1]];
            v.SetValor(newValue, escopo);
        }


        public object GetElemento(params int[] indices)
        {
            VariavelVetor vt = new VariavelVetor(this.GetAcessor(), this.GetTipo(), this.GetNome(), this.dimensoes);
            for (int x = 0; x < indices.Length; x++)
                vt = vt.tailVetor[indices[x]];
            return vt.valor;
        }


        public override string ToString()
        {
            string str = this.GetTipo() + " " + this.nome + " [ ";

            int x = 0;
            for (x = 0; x < this.tailVetor.Count - 2; x++)
                str += this.dimensoes[x] + ",";

            str += this.dimensoes[x] + " ]";


            return str;
        }
    }
} // namespace
