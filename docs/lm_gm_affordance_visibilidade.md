# Affordance e visibilidade (LM-GM)

Este documento mapeia a fase 1, `affordance_visibilidade`, pelo modelo Learning Mechanics–Game Mechanics (LM-GM). Ele registra uma **hipótese de design**: como as mecânicas de jogo devem sustentar as atividades de aprendizagem da fase. Não demonstra, por si só, que a aprendizagem ocorreu; isso depende dos testes com participantes e da telemetria.

## Referência e uso do modelo

Arnab, S. et al. (2015). *Mapping learning and game mechanics for serious games analysis*. British Journal of Educational Technology, 46(2), 391–411. DOI: [10.1111/bjet.12113](https://doi.org/10.1111/bjet.12113)

O LM-GM é descritivo, não prescritivo. Ele relaciona mecânicas de aprendizagem (LM) e mecânicas de jogo (GM) de duas formas:

- uma **tabela** com as relações "estáticas": a game mechanic, a learning mechanic, a implementação concreta no jogo e o uso pelo jogador;
- um **mapa** do fluxo de jogo, que mostra em que momento cada par aparece. O mapa segue os níveis de pensamento de Bloom usados pelo modelo: Retention, Understanding, Applying, Analysing, Evaluating e Creating.

Os nomes das mecânicas seguem os nós do artigo (em inglês), para facilitar a comparação com outros trabalhos.

## Escopo da fase

O jogador percorre três salas com terra comum e "terrenos estranhos": volumes de gelatina com a **mesma aparência da terra**. A gelatina restaura um dash gasto e limita a velocidade de queda, mas não restaura o pulo. Sem diferença visual, a possibilidade de ação existe, mas não é percebida.

O objetivo de aprendizagem é **reconhecer uma possibilidade de ação pouco visível e avaliar se uma intervenção visual comunica só que há algo diferente ou também o que esse algo permite fazer**.

## Fluxo da fase

```text
Tutorial (só na primeira vez da sessão; fora da pesquisa)
-> Introdução da fase (texto curto)
-> Sala 1: terra comum e gelatinas iguais à terra
   (Mort comenta no primeiro toque numa gelatina)
-> Sala 2: a passagem para a sala 3 fica bloqueada até a primeira escolha
-> Sala de intervenção (entrada com E na sala 2, ou na sala 1 após duas mortes):
     espada: testar soluções quantas vezes quiser;
     saída: justificativa (3 opções) ou "Continuar testando soluções"
-> Sala 3: percurso com a solução aplicada; com a melhor solução, o Mort
   demonstra uma combinação (aparência + pulsação)
-> Tela "Fase concluída": resumo e explicação da escolha
```

Regras de suporte:

- **Vida:** cinco corações. Quedas e espinhos tiram um coração e devolvem o jogador ao início da sala.
- **Paredes:** as bordas laterais de todas as salas são paredes invisíveis.
- **Transição:** a solução entra com uma piscada do personagem (fechar e abrir o olho). O Mort comenta cada escolha.

## Tabela LM-GM

| Game mechanic | Learning mechanic | Implementação | Uso pelo jogador |
|---|---|---|---|
| Tutorial; Cascading information | Tutorial; Guidance | Sala de tutorial com textos de teclas no cenário (`[Shift]`, `[W] + [Shift]`) e o dash para cima obrigatório. O Mort avisa, sem pausar, que o dash só funciona no ar. | Aprender os controles antes da fase, sem misturar a dificuldade motora com o problema de design. |
| Movement; Simulate/Response | Explore; Experimentation | Salas 1–3 com terra e gelatinas idênticas. A gelatina recarrega o dash e amortece a queda. | Descobrir pela ação que alguns terrenos se comportam de outro jeito. |
| Feedback; Cut scenes/Story | Observation; Identify | O Mort fala no primeiro toque numa gelatina ("Que terreno estranho..."). O dash recarregado é a consequência visível. | Notar que a diferença existe, mas que a aparência não a comunicou. |
| Rewards/Penalties; Levels | Hypothesis | Quedas custam corações. A passagem para a sala 3 fica bloqueada até a primeira escolha, com uma fala do Mort. | Relacionar a dificuldade da travessia à falta de informação visual. |
| Selecting/Collecting; Simulate/Response | Experimentation; Analyse | Na sala de intervenção, a espada (E) abre três soluções: **Aparência**, **Marcação** e **Deformação**. Cada escolha entra com a piscada e pode ser trocada à vontade. Plataformas de exemplo na própria sala. | Comparar como cada intervenção muda a leitura do terreno antes de se comprometer. |
| Feedback | Feedback; Reflect/Discuss | A espada pulsa forte depois da primeira escolha e volta ao normal ao testar uma segunda. O Mort comenta cada solução com uma pergunta ("mostra quais são diferentes... mas conta o que fazem?"). | Ser convidado, sem texto explicativo, a testar mais de uma opção. |
| Questions and Answers | Reflect/Discuss; Hypothesis | E na saída da sala (escura até a primeira escolha): "Que resultado você espera obter com essa intervenção?", com 3 justificativas embaralhadas e "Continuar testando soluções". A escolha só fica definitiva depois da justificativa. | Explicitar o critério usado na escolha. Ou voltar a testar. |
| Levels; Movement | Action/Task; Imitation | Sala 3 com a solução aplicada. Com a melhor solução, o Mort demonstra a combinação (aparência de gelatina + pulsação). | Verificar no percurso se a intervenção ajuda a prever o comportamento, e ver que ideias podem ser combinadas. |
| Assessment; Feedback | Assessment; Reflect/Discuss | Tela "Fase concluída": a solução escolhida, uma avaliação curta (por exemplo, "Pista de identificação") e uma explicação, sem nota visível. | Comparar a própria hipótese com uma leitura de design da solução. |

## Mapa do fluxo (níveis de Bloom)

| Momento | Nível | Learning mechanics | Game mechanics |
|---|---|---|---|
| Tutorial | Retention | Tutorial, Guidance, Repetition | Tutorial, Cascading information |
| Salas 1–2 | Understanding → Applying | Explore, Experimentation, Observation | Movement, Simulate/Response, Feedback |
| Bloqueio da sala 3 | Analysing | Identify, Hypothesis | Rewards/Penalties, Cut scenes/Story |
| Espada (testar soluções) | Analysing → Evaluating | Experimentation, Analyse, Feedback | Selecting/Collecting, Simulate/Response, Feedback |
| Saída com justificativa | Evaluating | Reflect/Discuss, Hypothesis | Questions and Answers |
| Sala 3 e demonstração | Applying → Evaluating | Action/Task, Imitation | Levels, Movement |
| Fase concluída | Evaluating | Assessment, Reflect/Discuss | Assessment, Feedback |

## Leitura das soluções

A qualidade fica registrada na telemetria, mas **não é mostrada ao jogador**. Todas as soluções liberam a sala 3.

| Solução | Efeito | Qualidade registrada | Leitura |
|---|---|---|---|
| Aparência (`terrain_appearance`) | A gelatina ganha arte de gelatina (camada `Solucao_gelatina`). | ADEQUATE (melhor) | O material aparente sugere o que o terreno faz: comunica a função. |
| Marcação (`terrain_mark`) | O terreno estranho ganha uma marca (camada `Solucao_mark`). | PARTIAL | Mostra onde há diferença, mas o significado da marca precisa ser aprendido. |
| Deformação (`terrain_motion`) | O terreno pulsa continuamente, por código. | PARTIAL | Chama atenção para algo incomum, mas pode sugerir elasticidade ou instabilidade. |

As três justificativas são:

- `terrain_prediction`: "Prever o comportamento do terreno antes de entrar em contato";
- `terrain_identification`: "Reconhecer quais terrenos possuem um comportamento diferente";
- `terrain_attention`: "Chamar atenção para os terrenos que merecem ser observados".

Elas acompanham, de forma aproximada, a função, a identificação e a saliência.

## Evidências na telemetria

O arquivo da sessão (JSON em Documentos/TimothyMort) registra, por fase:

- **Solução e justificativa:** a solução final, a justificativa e as posições em que cada uma apareceu.
- **Troca de solução:** `hadRevision` ("trocou para uma solução diferente") e o número de escolhas.
- **Desempenho:** o tempo ativo e as mortes.
- **Demonstração:** quando ela ocorreu, e o tempo e as mortes depois dela.

Na atividade da sessão, registra:

- tempo, entradas e mortes por sala;
- a última sala alcançada;
- o esquema de controle;
- a sequência de eventos da intervenção: soluções testadas, "continuar testando" e justificativa.

Perguntas que esses dados permitem investigar:

- se quem testa mais de uma solução escolhe a Aparência com mais frequência;
- se a justificativa escolhida combina com a solução (por exemplo, Marcação com identificação);
- se as mortes na sala 3 diminuem com a melhor solução.

## Limites

- A qualidade das soluções é uma hipótese de design. A arte das soluções é própria do projeto e influencia a leitura.
- **Mudança de método em 04/11/2026:** a justificativa passou a ser pedida **ao sair** da sala de intervenção, depois da experimentação, e as opções caíram de 6 para 3. Dados anteriores não são diretamente comparáveis.
- A dificuldade de plataforma pode mascarar o problema de design. O teste com especialistas motivou o dash maior, mais tolerância no pulo e as paredes laterais.
