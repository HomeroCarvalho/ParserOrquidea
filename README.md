# ProjetoLinguagensOrquidea
implementacao de linguagens de programacao LISP, PROLOG, programacao estruturada, e programacao orientada a  objetos

# LinguagensDoOrquidea

Projeto ParserLinguagemOrquidea


	Este projeto é um estudo holístico de construção de linguagens de programação.	Contém classes para um interpretador LISP, um interpretador PROLOG,
	um compilador e maquina virtual para uma linguagem Orientada a Objetos, e comando que implanta Programação Orientada a Aspectos, dentro da linguagem orientada a objetos.
	
	Para descrição das instruções (comandos de uma linguagem) LISP,
	vide: 
			"Documentos do Projeto\comandos e instrucoes  do interpretador lisp[1042].txt"

	Para descrição dos comandos do interpretador PROLOG,
	vide:
			"Documentos do Projeto\comandos e funcoes interpretador Prolog[1041].txt"
			
	
	Para descrição dos comandos da linguagem de programação orientada a objetos, e programação orientada a aspectos, vide:
			"Documentos do Projeto\instrucoes Linguagem Orientada a Objeto.txt".


Projeto para criacao de uma base de linguagens: lisp, prolog, programacao estruturada, programacao orientada a objetos, programacao orientada a aspectos.
Ante de compilar o projeto, acrescente os pacotes NuGet: MathNet.Numerics, MathNet.Spatial, sem os quais resultarao em erros na compilacao.


Segue a lista original da linguagem orientada a objetos:


----> INSTRUCOES: (comandos)
		
		---> Create (object objeto, List<propriedades>)
			cria uma copia na memoria das propriedades do objeto, e registra o objeto no escopo.
			
		---> Constructor(string nomeMetodoConstructor, List<propriedades>)
			copia na memoria das propriedades do objeto, e faz a chamada do metodo da entrada, com os parametros da entrada.

		----> class NameClass +ClassesHeranca, -ClassesDeseranca { propriedades, metodos }

		-----> for (int varc=id; varc< express, varc (operacao aritimetica) { instrucoes }
				exemplo:
					for (int i; i<(b+c);x++) { k=5;}
		-----> while (varc operacao com expressao) { instrucoes }
				exemplo:
					while (i<0) {k++; i--;}


		-----> if (expressao_cond) { instrucoes}
				exemplo:
					if (x<a+b+funcaoZ()) { return true; }				


		-----> if (expressao_cond) { instrucoes } else { instrucoes}
				exemplo:
					if (a>0){a=a+1;} else {a=a-1;}
				
		
		----> casesOfUse id { operacao aritmetica, ou condicional) var_caso1: {instrucoes }
				    { operacao aritmetica, poucondicional) var_caso2: {instrucoes }

					exemplo: casesOfUse x {
							(>1: bloco_intstrucoes;)
							(>(a+b): funcaoB();)
							(==1: funcaoAB();)
							}


		---> break:  para o fluxo de repeticao.
		---> continue: prossegue o fluxo de repeticao, para a proxima repeticao.
		---> return: retorna de uma funcao/metodo
		---> return exprss: retorna de uma funcao/metodo, com um valor dado em expss.
		---> operadorBinario: cria um operador binario para uma classe especifica.
		---> operadorUnario: cria um operador unario para uma classe especifica.

		---> chamada de método/funcao:  
				exemplo:
					método:	propriedade1.FuncaoA(), propriedade1.FuncaoB(express), propriedade1.FuncaoC(express, express,..).
					função: FuncaoA(), FuncaoB(express,...).

	        ----> instanciacao de variaveis:
				exemplo:
					tipoA variavel;
					tipoB variavel= express;

		----> atribuicao de variaveis:
				exemplo:
				variavel1= express;
		----> ha tambem uma instrucao que importa classes da classe base (csharp), para dentro do codigo da programacao orientada a objetos:
				importer nomeAssemblyComAClasseAImportar.

CLASSES DA LINGUAGEM:
		---> interopabilidade entre linguagem Orquidea e C sharp: (importa tipos de um arquivo .dll gerenciado, ou .exe.
			classe ImportadorDeClasses: importa classes de um Assembly (aplicativo .exe c sharp, ou biblioteca .dll c sharp). importa classes, metodos, propriedades.. 
		

OBJETOS DA LINGUAGEM:
		---> muito semelhante a visto em Java, C sharp, mas classes com multi-heranca (com detecção do problema do "losango da morte"), desherança.
