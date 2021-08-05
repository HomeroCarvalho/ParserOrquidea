using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Reflection;
using ModuloTESTES;
using System.Windows.Forms;
namespace META_GAME.Testes
{


    /// <summary>
    /// INTERPRETA TEXTOS EM SCRIPT, PARA TESTES ASSOCIADOS AO SCRIPT.
    /// </summary>
    public class ParserScript
    {
        /// <summary>
        /// um design pattern para execucao de comando, desacoplado, permitindo a inserção de novos comandos script.
        /// </summary>
        /// <param name="comando">linha de script a ter o comando script a ser executado.</param>
        /// <returns>retorna [true] se o comando foi encontado e executado corretamente.</returns>
        delegate bool Command(string comando);

        /// <summary>
        /// objetos a serem usados durante a execucao do script.
        /// </summary>
        ManipulationObjects objects = new ManipulationObjects();

        /// <summary>
        /// Banco de dados para serem passados durante a execução do script.
        /// </summary>
        Dictionary<string, List<string>> BancoDeDados = new Dictionary<string, List<string>>();

        /// <summary>
        /// lista de comandos do script que são reconhecidos pelo Interpretador.
        /// </summary>
        List<string> ComandosScript = new List<string>();

        
        public ParserScript()
        {
            this.BancoDeDados = new Dictionary<string, List<string>>();
        }  // ParserScript()


        /// <summary>
        /// Fábrica de Objetos Primitivos.
        /// </summary>
        /// <param name="typeObject">tipo do objeto a ser gerado.</param>
        /// <param name="valueObject">valor do objeto a ser gerado.</param>
        /// <returns>retorna um objeto do tipo [typeObject] setado com valor [valueObjet].</returns>
        private object FatoryObjects(string typeObject, string valueObject)
        {
            Type tipo = Type.GetType(typeObject);
            if (tipo == typeof(int))
                return int.Parse(valueObject);
            if (tipo == typeof(float))
                return float.Parse(valueObject);
            if (tipo == typeof(double))
                return double.Parse(valueObject);
            if (tipo == typeof(char))
                return char.Parse(valueObject);
            if (tipo == typeof(string))
                return valueObject;
            return null;
        }// FactoryObjects()


        /// <summary>
        /// executa um método a partir de uma linha script.
        /// </summary>
        /// <param name="comando">linha de comando script.</param>
        /// <returns>retorna [true] se a chamada for bem-sucedida, [false] se não.</returns>
        private bool ExecuteMethod(string comando)
        {
            string[] parameters =  ExtraiTermosVariaveis(comando);
            string objectCaller = parameters[1];
            string objectTarget = parameters[0];
            string metodo = parameters[1];
            List<object> objectsParameters = new List<object>();
            int i = 2;
            while (i < 2 + parameters.Length)
            {
                object objetoParametro = FatoryObjects(parameters[i], parameters[i + i]);
                if (objetoParametro == null)
                {
                    try
                    {
                        object objRegistrado = (objects.GetObject(parameters[i + 1]));
                        if (objRegistrado == null)
                        {
                            LoggerTests.AddMessage("falha no comando script: EXECUTE_METHOD");
                            return false;
                        }
                        objectsParameters.Add(objRegistrado);
                        i += 1;
                    } // try
                    catch (Exception ex)
                    {
                        LoggerTests.AddMessage("Erro na tentativa de acesso a um objeto nao registrado. Mensagem de Erro: " + ex.Message + ". Stack: " + ex.StackTrace);
                    } // catch
                } // if
                else
                {
                    objectsParameters.Add(objetoParametro);
                    i += 2;
                }// else
            }// while
            try
            {
                objects.SetObjectValue(objectTarget, objects.CallMethod(objectCaller, metodo, objectsParameters));
                LoggerTests.AddMessage("Método: " + metodo + " do objeto: " + objectCaller + ", executado com Sucesso.");
                return true;
            }
            catch (Exception ex)
            {
                LoggerTests.AddMessage("falha na execucao do metodo: " + metodo + " no comando EXECUTE_METHOD. Mensagem de Erro: " + ex.Message + " Stack: " + ex.StackTrace);
                return false;
            }
        } // ExecuteMethod()

        private bool ConstructorObject(string commando)
        {
            string[] parameters = ExtraiTermosVariaveis(commando);
            string nomeObjeto = parameters[0];

            try
            {
                int i = 0;
                List<object> objetos_parametros = new List<object>();
                while (i < parameters.Length)
                {
                    object obj = FatoryObjects(parameters[i + 1], parameters[i + 2]);
                    objetos_parametros.Add(obj);
                    i += 2;
                } // while
                object objToBuild = objects.GetObject(nomeObjeto);
                object objBuildded = objects.ConstrutorObjectsRegistred(nomeObjeto, objToBuild.GetType(), objetos_parametros);
                objects.SetObjectValue(nomeObjeto, objBuildded);
                LoggerTests.AddMessage("Objeto: " + nomeObjeto + " construido com Sucesso");
                return true;
            }
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro ao construir objeto: " + nomeObjeto + ". Erro: " + e.Message + ". Stack: " + e.StackTrace);
                return false;
            }

        } // ConstructorObject()
  
        /// <summary>
        /// este é o coração do Interpretador de Scripts de Testes.
        /// Para os Testes, é recomendado utilizar o banco de dados [DataSet] para carregar dados para a execução do Teste.
        /// </summary>
        /// <param name="linhasScript">linhas de código script.</param>
        public void Parser(string fileNameScript)
        {
            List<string> commands = new List<string>();
            List<Command> ComandosDoScriptr = new List<Command>();

            commands.Add("USE SCRIPT [arquivo]");
            commands.Add("EXECUTE SCRIPT [nomeScript]");
            commands.Add("CHANGE_LOG_FILE_NAME [arquivo]");
            commands.Add("SET_NEW_OBJECT [objeto] [PARAMETER_TYPE PARAMETER_ID]");
            commands.Add("SET_FIELD_OBJECT [OBJECT] [FIELD] [VALUE]");
            commands.Add("EXECUTE_METHOD_AND_ASSIGN [OBJECT_TARGET] [OBJECT_CALLER] [METHOD] [PARAMETERS]");
            commands.Add("EVAL [OBJECT],[OBJECT], [PROPERTY], [PROPERTY]");
            commands.Add("EVAL_WITH_DATABASE [OBJECT] [PROPERTY]");
            commands.Add("SET_DATABASE_FILE [NAME_FILE]");

            ComandosDoScriptr.Add(this.UseScript);
            ComandosDoScriptr.Add(this.ExecuteScript);
            ComandosDoScriptr.Add(this.ChangeNameFileLog);
            ComandosDoScriptr.Add(this.ConstructorObject);
            ComandosDoScriptr.Add(this.SetFieldObject);
            ComandosDoScriptr.Add(this.ExecuteMethod);
            ComandosDoScriptr.Add(this.EvalObjects);
            ComandosDoScriptr.Add(this.EvalObjectWithDataBase);

            this.ComandosScript = this.Interpretador(fileNameScript);

            for (int linha = 0; linha < ComandosScript.Count; linha++)
            {
                string keyCommando = ExtraiParametros(ComandosScript[linha])[0];
                for (int comandosDoScriptModel = 0; comandosDoScriptModel < commands.Count; comandosDoScriptModel++)
                {
                    string keyCommandosScriptModel = ExtraiParametros(commands[comandosDoScriptModel])[0];
                    if (keyCommando.Equals(keyCommandosScriptModel))
                    {
                        ComandosDoScriptr[comandosDoScriptModel](ComandosScript[linha]);
                    }  // if
                }// linha
            } // for comando
        } // Parser_tests()

        /// <summary>
        /// carrega um banco de dados a partir de um arquivo.
        /// </summary>
        /// <param name="comando">linha script que contem o nome do arquivo do banco de dados</param>
        /// <returns>retorna [true] se o bancode dados for carregado com Sucesso, [false] se não.</returns>
        private bool SetDataBaseFile(string comando)
        {
            try
            {
                if (BancoDeDados == null)
                    BancoDeDados = new Dictionary<string, List<string>>();
                BancoDeDados.Clear();
                string nomeArquivo = ExtraiTermosVariaveis(comando)[0];
                StreamReader stream = new StreamReader(nomeArquivo);
                while (!stream.EndOfStream)
                {
                    string linhaBancoDeDados = stream.ReadLine();
                    string nomeObjeto = ExtraiTermosVariaveisBancoDeDados(linhaBancoDeDados)[0];
                    string tipoObjeto = ExtraiTermosVariaveisBancoDeDados(linhaBancoDeDados)[1];
                    List<string> dataObjeto = ExtraiTermosVariaveisBancoDeDados(linhaBancoDeDados).ToList<string>();
                    dataObjeto.RemoveAt(0);
                    dataObjeto.RemoveAt(1);
                    dataObjeto.Insert(0, tipoObjeto);
                    BancoDeDados.Add(nomeObjeto, dataObjeto);
                } //while
                return true;
            } catch (Exception e)
            {
                LoggerTests.AddMessage("Erro ao carregar banco de dados em arquivo. Erro: " + e.Message + ". Stack: " + e.StackTrace);
                return false;
            }
        } // SetDataBaseFile().

        private bool EvalObjectWithDataBase(string comando)
        {
            try
            {
                string[] parameters = ExtraiTermosVariaveis(comando);
                string nomeObjeto = parameters[0];
                string propriedade = parameters[1];
                object objeto = objects.GetProperty(nomeObjeto, propriedade);
                for (int i = 1; i < BancoDeDados[nomeObjeto].Count; i++)
                {
                    if (objeto.Equals(BancoDeDados[nomeObjeto][i]))
                        return true;
                } // for i
                return false;
            } // try
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro ao avaliar o um objeto com o banco de dados. Erro:" + e.Message + ". Stack: " + e.StackTrace);
                return false;
            } // catch()
        }

        private bool ExecuteScript(string comando)
        {
            try
            {
                string script_name = ExtraiTermosVariaveis(comando)[0];
                this.Parser(script_name);
                return true;
            }
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro ao executar script na linha de comando: " + comando + ". Erro:" + e.Message + " Stack: " + e.StackTrace);
                return false;
            }

        } //ExecuteScript()

        private bool ChangeNameFileLog(string commando)
        {
            try
            {
                string nomeArquivo = ExtraiTermosVariaveis(commando)[0];
                LoggerTests.SetFileName(nomeArquivo);
                return true;
            }
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro ao mudar o nome do arquivo de log. Erro: " + e.Message + ". Stack: " + e.StackTrace);
                return false;
            }
            
        }

        private bool UseScript(string comando)
        {
            string File = ExtraiTermosVariaveis(comando)[0];
            List<string> sub_script_lines = this.UseScript(File, false);
            // entra em recursão, até utilizar todos comando use_script()
            this.Parser(File);
            return true;
        } // UseScript()


        /// <summary>
        /// seta uma propriedade de um objeto com um valor.
        /// </summary>
        /// <param name="comando">uma linha de comando.</param>
        private bool SetFieldObject(string comando)
        {

            string[] parametersParser = ExtraiTermosVariaveis(comando);
            string objeto = parametersParser[0];
            string campo = parametersParser[1];
            string valor = parametersParser[2];
            object valorObjeto = this.objects.GetObject(valor);
            objects.SetValue(objeto, campo, valorObjeto.GetType().Name, valorObjeto);
            return true;
        }

        /// <summary>
        /// avalia o valor de propriedades de 2 objetos são iguais.
        /// </summary>
        /// <param name="commando">linha de script.</param>
        /// <returns>retorna true e as propriedades são iguais, false se não.</returns>
        private bool EvalObjects(string commando)
        {
            string objeto1 = ExtraiTermosVariaveis(commando)[0];
            string objeto2 = ExtraiTermosVariaveis(commando)[1];
            string propriedadeObjeto1 = ExtraiTermosVariaveis(commando)[2];
            string propriedadesObjeto2 = ExtraiTermosVariaveis(commando)[3];
            if (objects.GetProperty(objeto1, propriedadeObjeto1).Equals(objects.GetProperty(objeto2, propriedadesObjeto2)))
                return true;
            else
                return false;
        }
               

        private string[] ExtraiParametros(string commando)
        {
            string[] parametros = commando.Split(new string[] { " ", ",", "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            return EliminaLinhaVazias(parametros);
        }
        private string[] ExtraiTermosVariaveis(string commando)
        {
            string[] p = commando.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            string[] termosVariaveis = commando.Split(p, StringSplitOptions.RemoveEmptyEntries);
            return EliminaLinhaVazias(termosVariaveis);
        }

        private string[] ExtraiTermosVariaveisBancoDeDados(string comando)
        {
            string[] parameters = comando.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            return EliminaLinhaVazias(parameters);
        }
        /// <summary>
        /// elimina linhas vazias, ou com vazios, somente.
        /// </summary>
        /// <param name="lines">vetor de linhas a serem filtradas.</param>
        /// <returns>retorna um vetor filtrado de linhas com vazios.</returns>
        private string[] EliminaLinhaVazias(string[] lines)
        {
            List<string> lstLines = new List<string>();
            for (int i = 0; i < lstLines.Count; i++)
            {
                if ((lines[i] != "") && (lines[i] != " "))
                    lstLines.Add(lines[i].Trim(' '));
            } // for i
            return lstLines.ToArray();
        } // EliminaLinhaVazias()

        /// <summary>
        /// retorna os comandos script de um arquivo script.
        /// </summary>
        /// <param name="linesScript">instruções de código das linhas de script (um comando válido por linha).</param>
        /// <returns>retorna uma lista de comandos que consegue processar.</returns>
        public List<string> Interpretador(string fileNameScript)
        {
            StreamReader stream = new StreamReader(fileNameScript);
            List<string> lstCommands = new List<string>();
            while (!stream.EndOfStream)
                ComandosScript.Add(stream.ReadLine());
            return lstCommands;

        } // Interpretador()

        
        
        /// <summary>
        /// comando USE_SCRIPT, que utiliza um script colocando-o dentro de outro script.
        /// </summary>
        /// <param name="nomeArquivo">nome do arquivo contendo o script a ser adicionado.</param>
        /// <param name="compila">se [true], compila o código adicionado, se [false] retorna somente as linhas do script acrescentado.</param>
        /// <returns>retorna as linhas de script adicionado.</returns>
        public List<string> UseScript(string nomeArquivo, bool compila)
        {
            try
            {
                List<string> linesScript = new List<string>();
                // adiciona as linhas de comandos ao script principal,
                // interpretando primeiro para comandos reconhecíveis pelo Parser.
                if (compila)
                {
                    List<string> more_commands = this.Interpretador(nomeArquivo);
                    this.ComandosScript.AddRange(more_commands);
                } // if compila
                return linesScript;
            } // try
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro ao processar o script: " + nomeArquivo + ".  Erro: " + e.Message);
                return null;
            } // catch
        } // UseScript()

    } // class Parser_tests


    /// <summary>
    /// Classe que registra via reflexão objetos do Script para testes.
    /// </summary>
    public class ManipulationObjects
    {
        /// <summary>
        /// lista de objetos registrados, indexado pelo nome do objeto.
        /// </summary>1
        private Dictionary<string, object> ObjetosRegistrados = new Dictionary<string, object>();


        /// <summary>
        /// seta um objeto existente com um novo objeto parametro.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto a ser modificado.</param>
        /// <param name="objeto">novo valor para o objeto.</param>
        public void SetObjectValue(string nomeObjeto,object objeto)
        {
            object sender = null;
            if (this.ObjetosRegistrados.TryGetValue(nomeObjeto,out sender)== true)
               this.ObjetosRegistrados[nomeObjeto] = objeto;
            else
            {
                this.ObjetosRegistrados.Remove(nomeObjeto);
                this.ObjetosRegistrados.Add(nomeObjeto, objeto);
            }
        }

        /// <summary>
        /// obtém o valor do objeto de nome objeto.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto.</param>
        /// <returns>retorna o valor do objeto.</returns>
        public object GetObject(string nomeObjeto)
        {
            try
            {
                return (ObjetosRegistrados[nomeObjeto]);
            }
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro ao acessar objeto " + nomeObjeto + ". Objeto nao registrado. Erro: " + e.Message + ". Stack: " + e.StackTrace);
                return null;
            }
        } // GetObject()

        /// <summary>
        /// retorna o valor de uma propriedade de um objeto registrado.
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto registrado a ser consultado em sua propriedade.</param>
        /// <param name="propertyName">nome da propriedade a ser consultada.</param>
        /// <returns></returns>
        public object GetProperty(string nomeObjeto, string propertyName)
        {
            try
            {
                var propriedade = ObjetosRegistrados[nomeObjeto].GetType().GetProperty(propertyName);
                if (propriedade == null) return null;
                var result = propriedade.GetValue(ObjetosRegistrados[nomeObjeto]);
                if (result == null) return null;
                return (Object)result;
            }
            catch (Exception e)
            {
                LoggerTests.AddMessage("Erro na tentativa de acessar a propriedade " + propertyName + " do objeto " + nomeObjeto + ". Stack" + e.StackTrace + " tipo de erro: " + e.Message);
                return null;
            }
        } // GetProperty() 

        /// <summary>
        /// seta um valor para uma propriedade de um objeto registrado, com valor igual [objetoValor].
        /// </summary>
        /// <param name="nomeObjeto">nome do objeto a ter o valor de propriedade modificada.</param>
        /// <param name="propriedade">nome da propriedade a ter o valor alterado.</param>
        /// <param name="objetoValor">onjeto valor para a propriedade.</param>
        public void SetValue(string nomeObjeto, string propriedade, string tipoObjetoValor, object objetoValor)
        {
            try
            {
                ObjetosRegistrados[nomeObjeto].GetType().GetProperty(propriedade).SetValue(ObjetosRegistrados[nomeObjeto], objetoValor);
            } catch (Exception e)
            {
                LoggerTests.AddMessage("Erro na tentativa de modificar o valor no objeto " + nomeObjeto + ", na propriedade " + propriedade + " Stack: " + e.StackTrace);
            }
        }// SetValue()

        /// <summary>
        /// construtor para objetos, via reflexão.
        /// </summary>
        /// <param name="nameObjectToBuild">nome do objeto a ser construido.</param>
        /// <param name="typeObjectToBuild">tipo do objeto a ser construido.</param>
        /// <param name="objectsParameters">lista de objetos parâmetros a ser passados para o construtor.</param>
        public bool ConstrutorObjectsRegistred(
            string nameObjectToBuild,
            Type typeObjectToBuild,
            List<object> objectsParameters)
        {
            List<Type> typesObjectsParameters = new List<Type>();
            foreach (object objetosParametros in objectsParameters)
                typesObjectsParameters.Add(objetosParametros.GetType());
            ConstructorInfo construtor = typeObjectToBuild.GetConstructor(typesObjectsParameters.ToArray());
            if (construtor != null)
            {
                this.ObjetosRegistrados.Add(nameObjectToBuild, construtor.Invoke(objectsParameters.ToArray()));
                return true;
            }
            return false;
        } // ConstrutorObjectsRegistred().

        /// <summary>
        /// chama um método de um objeto registrado.
        /// </summary>
        /// <param name="nameObject">nome do objeto registrado.</param>
        /// <param name="nameMethod">nome do método do objeto registrado.</param>
        /// <param name="parameters">lista de Parâmetros para o método chamado,</param>
        /// <returns></returns>
        public object CallMethod(string nameObject, string nameMethod, List<object> parameters)
        {
            return (this.ObjetosRegistrados[nameObject].GetType().GetMethod(nameObject).Invoke(this.ObjetosRegistrados[nameObject], parameters.ToArray()));
        }
    }
} // namespace META_GAME.Testes
