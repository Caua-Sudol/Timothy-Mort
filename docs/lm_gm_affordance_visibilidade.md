# Affordance e visibilidade (LM-GM)

Este documento mapeia a primeira fase do protótipo, `affordance_visibilidade`, por uma adaptação do modelo Learning-Game Mechanics (LM-GM). Ele registra uma **hipótese de design**: a relação esperada entre objetivo de aprendizagem, atividade do jogador e mecânicas implementadas. Não demonstra, por si só, que a aprendizagem ocorreu; essa avaliação depende de testes com jogadores e, futuramente, de telemetria.

## Referência

Arnab, S. et al. (2012). *Pedagogy-driven design of Serious Games: An overall view on learning and game mechanics mapping, and cognition-based models*.

O artigo propõe o LM-GM como uma forma de analisar ou apoiar o design de jogos sérios ao relacionar intenções pedagógicas a mecânicas de jogo e às ações do jogador. A coluna adicional `Relação esperada com o objetivo` adapta essa leitura ao projeto para tornar explícito o raciocínio de design.

## Escopo da fase

O jogador encontra plataformas sólidas e volumes de gelatina visualmente semelhantes. A gelatina restaura um dash gasto, mas não restaura o pulo e reduz a velocidade de queda enquanto o jogador está dentro dela. A falta de diferenciação visual torna o percurso pouco confiável.

O ciclo da fase é:

```text
exploração -> dificuldade de interpretação -> intervenção -> solução
-> justificativa -> aplicação -> teste -> progressão ou revisão
```

O objetivo de aprendizagem central é **analisar uma possibilidade de ação pouco perceptível e avaliar se uma intervenção visual comunica apenas saliência, uma diferença de superfície ou a função dessa superfície**.

## Mapeamento

| Objetivo de aprendizagem                                                      | Learning Mechanic                        | Game Mechanic                                              | Implementação no puzzle                                                                                                                                    | Relação esperada com o objetivo                                                                                                           |
|-------------------------------------------------------------------------------|------------------------------------------|------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| Perceber que uma possibilidade de ação não está claramente representada.      | Observação e experimentação.             | Movimento, pulo, dash, colisão e queda.                    | O percurso mistura sólidos e gelatinas inicialmente ambíguas. Avançar de modo confiável exige recuperar um dash em gelatina.                               | A dificuldade deixa de ser apenas abstrata: o jogador encontra uma consequência prática de não reconhecer a propriedade da superfície.    |
| Distinguir orientação de percurso de informação sobre uma propriedade.        | Análise e comparação.                    | Feedback visual e navegação.                               | Soluções inadequadas podem indicar caminho, destacar destino, adicionar decoração, rastro ou flash, mas não identificam a propriedade relevante.           | O jogador pode observar que chamar atenção para algo não garante que sua função seja compreendida.                                        |
| Formular uma hipótese sobre o que uma intervenção visual deve resolver.       | Reflexão, decisão e explicitação.        | Ação/tarefa, seleção e interface de cartas.                | O jogador escolhe uma solução e uma justificativa entre seis intenções apresentadas em ordem embaralhada.                                                  | A escolha transforma uma interpretação implícita do problema em uma resposta observável para análise posterior.                           |
| Comparar uma diferenciação parcial com uma representação que comunica função. | Observação, comparação e experimentação. | Mudança de estado, feedback e movimento.                   | Soluções parciais tornam a gelatina identificável; adequadas também relacionam a superfície à recuperação ou disponibilidade do dash.                      | O jogador testa a diferença entre reconhecer uma superfície especial e prever o que ela permite fazer.                                    |
| Confrontar uma decisão com sua consequência no mundo.                         | Experimentação e feedback.               | Aplicação de módulos visuais, travessia, porta e objetivo. | A solução escolhida muda as gelatinas da sala intermediária e do mapa principal. Parciais e adequadas liberam a saída; inadequadas mantêm a porta fechada. | A consequência é jogável, não uma correção textual: a intervenção precisa sustentar a travessia.                                          |
| Revisar uma solução após observar evidência de falha.                         | Reflexão e revisão.                      | Falha, retorno, nova escolha e progressão.                 | Depois de uma inadequada, o jogador retorna ao botão e escolhe entre P4 e A4, sem nova justificativa.                                                      | A revisão registra uma decisão posterior informada pela consequência; ela complementa, mas não substitui, a análise da primeira resposta. |
| Verificar a intervenção no percurso de origem.                                | Aplicação e avaliação.                   | Retorno de cenário, objetivo e progressão.                 | Após sair da sala de intervenção, o jogador retorna ao mapa do problema com a representação visual aplicada.                                               | O teste no percurso original verifica se a mudança apoia a ação no contexto em que o problema apareceu.                                   |

## Leitura das soluções

As soluções são classificadas internamente como inadequadas, parciais ou adequadas. A classificação não é mostrada ao jogador.

- **Inadequada:** chama atenção para percurso, destino ou contato, mas não permite reconhecer de forma confiável quais superfícies têm a propriedade relevante. A porta permanece fechada.
- **Parcial:** permite identificar uma diferença relevante, ou comunica parte do estado do movimento, sem comunicar completamente a função da superfície. A porta abre.
- **Adequada:** diferencia a superfície e oferece uma pista sobre recuperação ou disponibilidade do dash. A porta abre.

P4 e A4 pertencem somente à revisão. Como essa segunda escolha ocorre após uma consequência e com opções reduzidas, ela não deve ser interpretada com o mesmo peso da resposta inicial.

## Limites e avaliação futura

O LM-GM organiza a intenção do protótipo, não substitui avaliação empírica. Para verificar a hipótese, uma etapa posterior deve observar, entre outros pontos:

- se o jogador distingue superfície especial, função da superfície e orientação de caminho;
- a primeira solução, justificativa, IDs e posições apresentadas;
- a consequência observada, ocorrência de revisão e solução final;
- diferenças entre intervenção adequada com justificativa inadequada, intervenção inadequada com justificativa adequada e demais combinações.

A telemetria será definida depois que as três fases do jogo tiverem estrutura comum suficiente para comparação.
