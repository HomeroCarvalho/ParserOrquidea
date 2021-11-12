using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public class ExpressaoSemValidacao: Expressao
    {

        public List<string> msgErros { get; set; }
        private List<string> tokensNotProcessed { get; set; }
        private List<string> tokensProcessed { get; set; }
        public ExpressaoSemValidacao(List<string>tokens):base()
        {
            this.tokensNotProcessed = tokens.ToList<string>();
            LinguagemOrquidea lng = LinguagemOrquidea.Instance();
            this.tokensProcessed = new Tokens(lng, tokensNotProcessed).GetTokens();

            this.msgErros = new List<string>();

            this.Elementos.Clear();

            foreach (string token in this.tokensProcessed)
                this.Elementos.Add(new ExpressaoElemento(token));
        }

     

        private void Processamento()
        {
            LinguagemOrquidea lng = LinguagemOrquidea.Instance();
            List<string> tokensRaw = new Tokens(lng, tokensNotProcessed).GetTokens();

            for (int x = 0; x < tokensRaw.Count; x++)
            {
                if (tokensRaw[x] == ";")
                    return;

                if (tokensRaw[x] == "[")
                {
                    List<string> tokensIndicesMatriz = UtilTokens.GetCodigoEntreOperadores(x + 1, "[", "]", tokensRaw);
                    if ((tokensIndicesMatriz != null) && (tokensIndicesMatriz.Count > 0))
                    {
                        this.tokensProcessed.AddRange(tokensIndicesMatriz);
                        x += tokensIndicesMatriz.Count;
                        continue;
                    }
                    else
                    {
                        this.msgErros.Add("Erro na colocacao de operadores de matriz, na expressao: " + Util.UtilString.UneLinhasLista(tokensRaw));
                        this.tokensProcessed = new List<string>();
                        return;
                    }
                }
                
                if (!lng.IsNumero(tokensRaw[x]))
                {
                    if ((lng.IsID(tokensRaw[x])) && ((x + 1) < tokensRaw.Count) && (tokensRaw[x + 1] == "(")) 
                    {
                        List<string> tokensChamadaDeFuncao = UtilTokens.GetCodigoEntreOperadores(x + 1, "(", ")", tokensRaw);
                        if ((tokensChamadaDeFuncao != null) && (tokensChamadaDeFuncao.Count > 0))
                        {
                            this.tokensProcessed.AddRange(tokensChamadaDeFuncao);
                            x += tokensChamadaDeFuncao.Count;
                            continue;
                        }
                        else
                        {
                            this.msgErros.Add("Erro na colocacao de parenteses: " + tokensRaw[x] + " na expressao: " + Util.UtilString.UneLinhasLista(tokensRaw));
                            this.tokensProcessed = new List<string>();
                            return;
                        }
                    } //if
                    if ((lng.IsID(tokensRaw[x])) && (tokensRaw[x] == ")"))
                    {
                        this.tokensProcessed.Add(tokensRaw[x]);
                        continue;
                    } // if

                    if ((lng.IsID(tokensRaw[x])) && (tokensRaw[x] != "(") && (tokensRaw[x] != ")"))
                    {
                        this.tokensProcessed.Add(tokensRaw[x]);
                        continue;
                    } // if
                } // if
                if (lng.IsNumero(tokensRaw[x]))
                {
                    this.tokensProcessed.Add(tokensRaw[x]);
                    continue;
                }
                if (lng.VerificaSeEhOperadorBinario(tokensRaw[x]))
                {
                    if (((x - 1) >= 0) && ((x + 1) < tokensRaw.Count) && (!lng.IsID(tokensRaw[x - 1])) && (!lng.IsID(tokensRaw[x + 1])) && (tokensRaw[x + 1] != ")") && (tokensRaw[x - 1] != "("))
                    {
                        this.msgErros.Add("Erro na colocacao de operador binario: " + tokensRaw[x] + " nos tokens da expressao: " + Util.UtilString.UneLinhasLista(tokensRaw));
                        this.tokensProcessed = new List<string>();
                        return;
                    }
                    if (((x + 1) < tokensRaw.Count) && (lng.IsID(tokensRaw[x + 1])))
                    {
                        this.tokensProcessed.Add(tokensRaw[x]);
                        continue;
                    }
                }
                if (lng.VerificaSeEhOperadorUnario(tokensRaw[x]))
                {
                    if (((x - 1) >= 0) && (tokensRaw[x-1] == "("))
                    {
                        this.tokensProcessed.Add(tokensRaw[x]);
                        continue;
                    }
                    if (((x + 1) < tokensRaw.Count) && (tokensRaw[x + 1] == ")"))
                    {
                        this.tokensProcessed.Add(tokensRaw[x]);
                        continue;
                    }
                    if (((x + 1) < tokensRaw.Count) && (lng.IsID(tokensRaw[x + 1]))) 
                        {
                        this.msgErros.Add("Erro na colocacao de operador unario: " + tokensRaw[x] + " nos tokens da expressao: " + Util.UtilString.UneLinhasLista(tokensRaw));
                        this.tokensProcessed = new List<string>();
                        return;
                    }
                    if (((x - 1) >= 0) && (lng.IsID(tokensRaw[x - 1]))) 
                    {
                        this.msgErros.Add("Erro na colocacao de operador unario: " + tokensRaw[x] + " nos tokens da expressao: " + Util.UtilString.UneLinhasLista(tokensRaw));
                        this.tokensProcessed = new List<string>();
                        return;

                    }
                    if (((x - 1) >= 0) && ((x + 1) < tokensRaw.Count) && (!lng.IsID(tokensRaw[x - 1])) && (!lng.IsID(tokensRaw[x + 1])))
                    {
                        this.msgErros.Add("Erro na colocacao de operador unario: " + tokensRaw[x] + " nos tokens da expressao: " + Util.UtilString.UneLinhasLista(tokensRaw));
                        this.tokensProcessed = new List<string>();
                        return;
                    }
                   
                }

            } // for x
                  
        }
    }
}
