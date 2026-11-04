# Modelos conceituais (LM-GM)

Este documento mapeia a fase 3, `modelos_conceituais_restricoes` (título no jogo: "Modelos conceituais"), pelo modelo Learning Mechanics–Game Mechanics (LM-GM). Ele registra uma **hipótese de design**: como as mecânicas de jogo devem sustentar as atividades de aprendizagem da fase. Não demonstra, por si só, que a aprendizagem ocorreu.

## Referência e uso do modelo

Arnab, S. et al. (2015). *Mapping learning and game mechanics for serious games analysis*. British Journal of Educational Technology, 46(2), 391–411. DOI: [10.1111/bjet.12113](https://doi.org/10.1111/bjet.12113)

Como nas outras fases, o mapeamento tem duas partes:

- uma **tabela**: game mechanic, learning mechanic, implementação e uso pelo jogador;
- um **mapa** do fluxo pelos níveis de Bloom do modelo.

Os nomes das mecânicas seguem os nós do artigo.

## Escopo da fase

A fase usa o que o jogador **já aprendeu** nas fases 1 e 2:

- a gelatina recarrega o dash;
- a rocha quente fere.

Ela também mistura **impostoras**, que têm a mesma aparência das conhecidas, mas agem de outro jeito:

| Conhecida | Impostora (mesma aparência) |
|---|---|
| Gelatina: recarrega o dash e amortece a queda | **Mini-jump** (gelatina-trampolim): lança o jogador para cima, com cerca de 2× a altura do pulo, e **não** recarrega o dash |
| Rocha quente: tira 1 coração | **Rocha impostora**: tira **2 corações** |

O feedback de dano é fixo desde o começo, porque o tema já foi trabalhado na fase 2:

- **Rocha comum:** recuo + coração rachando.
- **Rocha impostora:** recuo + tela tremendo.
- **Invencibilidade:** 0,6 s.

O objetivo de aprendizagem é **reconhecer que um modelo conceitual se apoia na aparência e na experiência anterior, e avaliar como corrigir uma incoerência entre aparência e comportamento**. As opções são diferenciar, uniformizar ou sinalizar por movimento. O jogo ensina pela ação, sem explicação em diálogo.

## Fluxo da fase

```text
Introdução da fase (texto curto: "Quando algo parece igual ao que já conhecemos...")
-> Salas 1 e 2: gelatinas, mini-jumps, rochas quentes e impostoras, todas com
   aparência de conhecidas; a passagem para a sala 3 fica bloqueada até a primeira escolha
-> Sala de intervenção (entrada com E): espada para testar; exemplos de cada
   plataforma na sala; saída com justificativa ou "Continuar testando soluções"
-> Sala 3: percurso com a solução aplicada (sem demonstração combinada)
-> Tela "Fase concluída" -> fim do estudo (tela final de envio dos dados)
```

## Tabela LM-GM

| Game mechanic | Learning mechanic | Implementação | Uso pelo jogador |
|---|---|---|---|
| Movement; Simulate/Response | Explore; Experimentation | Impostoras com a arte das plataformas conhecidas. O mini-jump lança para cima; a rocha impostora tira 2 corações. | Aplicar o modelo aprendido e ser surpreendido pelo comportamento diferente. |
| Feedback; Status | Observation; Identify | Recuo e coração na rocha comum; recuo e tremor na impostora. O mini-jump não recarrega o dash. | Perceber que a mesma aparência produziu consequências diferentes. |
| Rewards/Penalties; Levels | Hypothesis | O custo maior da impostora e a passagem para a sala 3 bloqueada até a escolha. | Atribuir o erro à aparência enganosa, e não ao próprio descuido. |
| Selecting/Collecting; Simulate/Response | Experimentation; Analyse; Generalisation/Discrimination | A espada abre **Diferenciar**, **Restaurar** e **Animar**, que podem ser trocadas e testadas nos exemplos da sala. | Comparar três formas de alinhar aparência e comportamento. |
| Feedback | Feedback; Reflect/Discuss | A espada pulsa forte depois da primeira escolha. O Mort comenta cada solução ("Nada de surpresas... mas o que perdemos?"). | Ser convidado a testar outra opção e pensar no custo de cada uma. |
| Questions and Answers | Reflect/Discuss; Hypothesis | Na saída: 3 justificativas e "Continuar testando soluções". | Explicitar o critério: prever, uniformizar ou manter a variedade. |
| Levels; Movement | Action/Task | Sala 3 com a solução aplicada. | Verificar se a solução permite prever cada plataforma antes de pisar. |
| Assessment; Feedback | Assessment; Reflect/Discuss | Tela "Fase concluída" ("Pista de aparência", "Consistência com o já conhecido", "Pista de movimento") e explicação. Em seguida, a tela final do estudo. | Comparar a hipótese com uma leitura de design e encerrar a sessão. |

## Mapa do fluxo (níveis de Bloom)

| Momento | Nível | Learning mechanics | Game mechanics |
|---|---|---|---|
| Salas 1–2 (modelo já aprendido) | Retention → Applying | Repetition, Experimentation, Observation | Movement, Simulate/Response |
| Surpresa e bloqueio | Analysing | Identify, Generalisation/Discrimination, Hypothesis | Feedback, Rewards/Penalties |
| Espada (testar soluções) | Analysing → Evaluating | Experimentation, Analyse | Selecting/Collecting, Simulate/Response |
| Saída com justificativa | Evaluating | Reflect/Discuss, Hypothesis | Questions and Answers |
| Sala 3 | Applying → Evaluating | Action/Task | Levels, Movement |
| Fase concluída | Evaluating | Assessment, Reflect/Discuss | Assessment, Feedback |

## Leitura das soluções

A qualidade fica registrada na telemetria e **não é mostrada ao jogador**. Todas as soluções liberam a sala 3. Não há demonstração combinada: Restaurar e Animar anulariam ou repetiriam o Diferenciar.

| Solução | Efeito | Qualidade registrada | Leitura |
|---|---|---|---|
| Diferenciar (`concept_differentiate`) | As impostoras ganham arte própria: mini-jump e fogo azul (camadas `Mini_jump_*` e `Blue_fire_*`). | ADEQUATE (melhor) | Cada comportamento ganha a sua aparência: o que foi aprendido continua valendo e a novidade é anunciada. |
| Restaurar (`concept_restore`) | As impostoras passam a agir como as conhecidas: o trampolim vira gelatina e a rocha tira 1 coração. | PARTIAL | Elimina a surpresa, mas também a variedade que os novos comportamentos traziam. |
| Animar (`concept_animate`) | Só as impostoras se mexem: o mini-jump estica para cima e a rocha impostora solta faíscas vermelhas. | PARTIAL | Sinaliza pelo movimento, mas depende de o jogador observá-lo. Parada, a plataforma continua igual à conhecida. |

As três justificativas são:

- `concept_predict`: "Prever o que cada plataforma faz antes de tocá-la";
- `concept_same_look`: "Fazer plataformas iguais se comportarem do mesmo jeito";
- `concept_keep_new`: "Manter os novos comportamentos disponíveis na travessia".

## Evidências na telemetria

Os campos são os mesmos das outras fases:

- **por fase:** solução, justificativa, posições, `hadRevision`, tempo e mortes;
- **por sala:** tempo, entradas e mortes;
- **na intervenção:** os eventos em ordem.

Perguntas que esses dados permitem investigar:

- se as mortes nas salas 1–2 se concentram em rochas impostoras. Hoje isso aparece só por sala, não por plataforma;
- se Restaurar é escolhido por quem justifica com `concept_same_look`;
- se Animar e Diferenciar produzem desempenho parecido na sala 3.

## Limites

- **Animar × Diferenciar:** como o Animar passou a marcar só as impostoras, a diferença entre as duas soluções ficou em movimento contra cor. A distinção de qualidade entre elas deve ser observada com cuidado.
- **Fase nova no estudo:** a fase 3 entrou no estudo depois do teste com especialistas. Não há dados de comparação anteriores.
- **Fase anterior:** a versão anterior desta fase, com espinhos inofensivos, revisão e condição de saída por vida, foi descontinuada. O código remanescente está listado para limpeza depois do TCC.
- **Ideia para depois do TCC:** transformar a solução num poder que alterna o estado das impostoras, criando um puzzle de plataforma.
