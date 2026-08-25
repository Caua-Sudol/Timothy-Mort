# Feedback

Este documento descreve o mapeamento conceitual entre Learning Mechanics (LM) e Game Mechanics (GM) usado no protótipo da fase de feedback de dano. Ele registra a intenção pedagógica da fase; não substitui validação com participantes nem demonstra, por si só, eficácia de aprendizagem.

## Referência

Neste documento, LM designa a ação cognitiva solicitada ao jogador — observar, comparar, justificar, testar e revisar uma intervenção. GM designa a regra, o estado ou a resposta jogável que sustenta essa ação. A relação esperada descreve por que a mecânica de jogo pode tornar a distinção conceitual observável.

A classificação das soluções adota como problema-alvo a comunicação de duas dimensões complementares:

- evento: tornar perceptíveis o instante, o local e a direção do dano;
- estado temporário: indicar durante quanto tempo novos danos estão bloqueados.

## Escopo da fase

O cenário-base retira um dos cinco corações ao contato com um volume perigoso e ativa internamente 1,5 segundo de invulnerabilidade. Antes de qualquer intervenção, somente o HUD muda. Essa ausência deliberada de resposta localizada cria o problema que o jogador deve analisar.

A fase não ensina a evitar todo perigo nem a calcular intensidade de dano. Sinalização antecipada, leitura da vida restante e rotas seguras podem melhorar a travessia, mas não resolvem sozinhas a comunicação do evento de dano e da proteção temporária.

O ciclo pedagógico é:

1. Vivenciar o problema-base.
2. Comparar três intervenções de qualidades distintas.
3. Escolher uma intervenção e justificar o resultado esperado.
4. Testar a solução aplicada na sala intermediária e no mapa original.
5. Revisar a escolha quando a intervenção inicial não resolve o problema-alvo.

## Mapeamento

| Objetivo de aprendizagem                                                 | Learning Mechanic (LM)             | Game Mechanic (GM)                                                                           | Relação esperada                                                                                               |
|--------------------------------------------------------------------------|------------------------------------|----------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------|
| Perceber a insuficiência do HUD como único feedback                      | Observar e identificar ambiguidade | Dano retira um coração, mas o personagem não reage e a invulnerabilidade permanece invisível | O jogador experimenta a diferença entre consequência acumulada e comunicação do evento/estado.                 |
| Distinguir intervenção inadequada, parcial e adequada                    | Comparar hipóteses concorrentes    | O menu apresenta uma solução de cada qualidade em ordem aleatória                            | A comparação reduz a dependência da posição e exige considerar o efeito esperado de cada proposta.             |
| Explicitar o critério usado na escolha                                   | Justificar uma previsão            | J1–J6 representam consequência, antecipação, desvio, evento, estado e evento + estado        | A justificativa torna observável qual dimensão o jogador associou à intervenção.                               |
| Relacionar proposta e efeito jogável                                     | Aplicar e testar                   | Módulos visuais e físicos da solução escolhida são ativados nos dois cenários da fase        | O resultado pode ser confrontado com a previsão feita no menu.                                                 |
| Reconhecer que evitar ou antecipar o perigo não resolve feedback de dano | Detectar inadequação e revisar     | I1–I3 mantêm a porta fechada; a revisão oferece somente P4 e A4                              | O bloqueio conduz a uma nova comparação centrada no evento e no estado temporário.                             |
| Diferenciar uma dimensão isolada da combinação completa                  | Classificar evidências             | P1, P3 e P4 comunicam evento; P2 comunica estado; A1–A4 combinam evento e estado             | Os efeitos permitem observar por que uma resposta parcial não cobre as duas dimensões do problema.             |
| Transferir a análise para o contexto original                            | Reaplicar e verificar              | A solução também atua no mapa do problema depois do retorno                                  | A intervenção deixa de ser apenas uma demonstração na sala de escolha e pode ser avaliada durante a travessia. |

## Leitura das escolhas

### Inadequadas

- I1 e J1 concentram-se na consequência acumulada: tornam a vida restante mais legível, mas não explicam quando o dano ocorreu nem quando a proteção termina.
- I2 e J2 concentram-se na antecipação: ajudam a reconhecer o risco antes do contato, mas não comunicam a resposta ao dano.
- I3 e J3 desviam do problema: a rota segura pode evitar os espinhos, porém elimina a oportunidade de observar o feedback sem resolver sua ausência.

### Parciais

- P1, P3 e P4, alinhadas a J4, tornam o evento de dano perceptível, mas não representam toda a duração da invulnerabilidade.
- P2, alinhada a J5, representa o estado temporário, mas não fornece uma reação física localizada para o instante do impacto.

### Adequadas

- A1, A2, A3 e A4 combinam uma reação de impacto com um sinal que permanece durante toda a proteção.
- J6 explicita o alinhamento completo: relacionar o contato à perda de vida e indicar enquanto a invulnerabilidade continua ativa.

A qualidade registrada no catálogo descreve alinhamento com o problema pedagógico desta fase. Ela não é uma avaliação universal da qualidade visual ou de usabilidade de cada intervenção.

## Evidências disponíveis

O estado da fase mantém a primeira solução, a primeira justificativa, a solução efetivamente aplicada e a ocorrência de revisão. Durante o protótipo, a validação é manual e pode observar:

- se a justificativa escolhida corresponde à dimensão da solução;
- se o jogador reconhece a diferença entre evento e estado temporário;
- se uma revisão troca a intervenção aplicada sem acumular efeitos;
- se o comportamento percebido na sala intermediária é reconhecido no mapa original.

Não há telemetria neste protótipo. Portanto, esses registros de estado sustentam o fluxo jogável, mas ainda não formam uma coleta de dados de pesquisa.

## Limites e ressalvas atuais

- Os efeitos são procedurais e provisórios; acabamento visual pode alterar sua legibilidade.
- I1 possui contraste sutil, I2 não colore toda a plataforma, I3 ainda não completa a rota até o portão e A4 apresenta a silhueta apenas à esquerda.
- A atribuição das qualidades é uma hipótese de design a ser confrontada com validação empírica.
- Áudio, assets finais, telemetria e variações de intensidade do dano não fazem parte deste mapeamento.
- A fase de affordance e a fase de teste de assets permanecem isoladas e não devem ser usadas como evidência do comportamento desta fase.

Os detalhes de implementação e a validação funcional dos checkpoints estão em [checkpoints_feedback_dano.md](checkpoints_feedback_dano.md).
