# Modelos conceituais e restrições (LM-GM)

Este documento descreve o mapeamento entre Learning Mechanics (LM) e Game Mechanics (GM) da fase `modelos_conceituais_restricoes`. Ele registra uma hipótese de design: a relação esperada entre o que o jogador deve analisar e as regras jogáveis do protótipo. Não demonstra, por si só, que aprendizagem ocorreu; essa conclusão depende de testes com participantes.

## Referência

Arnab, S. et al. (2012). *Pedagogy-driven design of Serious Games: An overall view on learning and game mechanics mapping, and cognition-based models*.

Neste documento, LM designa a atividade cognitiva solicitada ao jogador — observar, comparar, prever, justificar, testar e revisar. GM designa a regra, o estado, a interface ou a resposta jogável que torna essa atividade observável.

## Escopo da fase

O jogador encontra dois pares de elementos visualmente semelhantes, mas com propriedades diferentes:

- gelatinas padrão restauram dash, enquanto gelatinas-trampolim aplicam impulso e não restauram dash;
- espinhos de dano removem vida, enquanto espinhos inofensivos não têm consequência.

O problema não é apenas descobrir uma regra escondida. Ele é avaliar se a aparência das plataformas permite formar um modelo conceitual confiável sobre o que elas fazem e quais restrições impõem ao percurso. A vida limitada, a invulnerabilidade temporária e a condição de saída com dois corações ou menos tornam o custo da interpretação incorreta observável durante a travessia.

O ciclo pedagógico é:

```text
vivenciar inconsistência -> identificar consequências e restrições
-> comparar intervenções -> prever e justificar -> testar
-> progredir ou revisar -> reaplicar no percurso original
```

## Mapeamento

| Objetivo de aprendizagem | Learning Mechanic (LM) | Game Mechanic (GM) | Relação esperada |
|--------------------------|------------------------|--------------------|------------------|
| Perceber que aparência semelhante não garante comportamento semelhante. | Observação e experimentação. | Movimento, dash, volumes de gelatina, espinhos e colisão. | O jogador experimenta efeitos opostos em elementos que inicialmente compartilham a mesma apresentação visual. |
| Relacionar propriedades de plataforma às restrições do percurso. | Identificação de restrições e antecipação de consequências. | Cinco corações, dano, invulnerabilidade, salto, dash e limiar de dois corações. | A escolha de tocar, evitar ou usar cada elemento tem custo observável e altera as possibilidades de progressão. |
| Distinguir saliência, explicação e orientação de uma correção do modelo conceitual. | Comparação de hipóteses concorrentes. | Cartas I1–I3, efeitos visuais e porta bloqueada. | Realçar, rotular ou indicar a rota pode melhorar a leitura local sem fazer aparência e comportamento concordarem. |
| Explicitar a hipótese usada para avaliar uma intervenção. | Previsão, decisão e justificativa. | Seleção de solução, escolha embaralhada de J1–J6 e registro no estado da fase. | O jogador torna observável se espera atenção, explicação, rota, consistência, diferenciação ou alinhamento completo. |
| Reconhecer uma correção parcial. | Classificação de evidências e comparação. | P1–P4 corrigem somente gelatinas ou espinhos. | Cada parcial elimina uma inconsistência, mas preserva a outra; a porta abre para permitir comparar o resultado sem tratá-lo como resolução completa. |
| Reconhecer duas formas adequadas de alinhamento. | Generalização e avaliação de alternativas. | A1 normaliza propriedades; A2–A4 diferenciam variantes e ajustam suas regras. | O jogador pode comparar retorno ao estado original com diferenciação visual acompanhada de comportamento compatível. |
| Revisar uma hipótese após falha observável. | Reflexão e revisão. | Inadequadas mantêm a porta fechada; P4 e A4 aparecem no retorno ao botão. | A revisão ocorre depois de testar a consequência da primeira escolha e substitui a solução aplicada, sem acumular efeitos. |
| Verificar a intervenção no contexto em que o problema surgiu. | Aplicação e avaliação. | Retorno ao mapa principal com módulos da solução ativos. | A sala intermediária não é a única evidência: o jogador reaplica a interpretação no percurso original, com seus custos e restrições. |

## Leitura das escolhas

### Inadequadas

- I1 destaca todas as plataformas especiais do mesmo modo; aumenta saliência, mas preserva a ambiguidade entre variantes.
- I2 apresenta `DANO` e `DASH` como lembretes; explica uma regra pretendida, mas ainda a aplica também às variantes que a contradizem.
- I3 indica uma rota; orienta a travessia sem corrigir o significado das plataformas.

### Parciais

- P1 e P3 tornam os espinhos consistentes, mas deixam a gelatina-trampolim com aparência ambígua.
- P2 e P4 restauram as gelatinas, mas deixam espinhos visualmente iguais com consequências diferentes.

### Adequadas

- A1 normaliza ambas as famílias: gelatinas padrão e espinhos de um dano.
- A2 diferencia as duas variantes alteradas e associa cor a propriedades distintas: verde para o trampolim com impulso equivalente ao dash e magenta para o espinho de dois danos.
- A3 normaliza as gelatinas e diferencia os espinhos alterados.
- A4 diferencia a gelatina-trampolim e normaliza os espinhos.

As soluções adequadas não dependem de uma única linguagem visual: tanto restabelecer uma convenção quanto sinalizar uma variante pode ser adequado quando aparência, comportamento e restrições voltam a formar uma relação previsível.

## Evidências disponíveis

O estado da fase mantém a primeira solução, a primeira justificativa, a solução efetivamente aplicada, a quantidade de escolhas e a ocorrência de revisão. Durante a validação manual, é possível observar:

- se o jogador identifica os dois pares de inconsistência;
- se a justificativa escolhida corresponde à hipótese declarada;
- se uma solução parcial é reconhecida como correção de apenas uma família;
- se a solução adequada permite prever comportamento e custo antes do contato;
- se a revisão substitui a intervenção inadequada e se a leitura se mantém no mapa principal.

Não há telemetria neste protótipo. Esses estados sustentam o fluxo de jogo, mas ainda não constituem coleta de dados de pesquisa.

## Limites e avaliação futura

- A classificação inadequada, parcial e adequada é uma hipótese de design e deve ser confrontada com participantes.
- As cores, textos e ícones são provisórios; sua legibilidade pode alterar a interpretação das variantes.
- O impulso vertical iguala a velocidade inicial do dash, mas seu deslocamento final depende da física vertical e do layout.
- A saída por dois corações ou menos é uma restrição deliberada do protótipo; testes futuros devem verificar se ela é percebida como condição significativa e não como regra arbitrária.
- Áudio, assets finais, telemetria e balanceamento definitivo não fazem parte deste mapeamento.

Os detalhes operacionais e os checkpoints validados estão em [checkpoints_modelos_conceituais_restricoes.md](checkpoints_modelos_conceituais_restricoes.md).
