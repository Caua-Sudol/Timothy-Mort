# Checkpoints: feedback

Este documento registra o protótipo validado da fase de feedback de dano. A fase foi implementada em três checkpoints isolados e é conectada às demais somente por uma entrada própria no mapa `SimpleCutscene`.

## Estado atual

Os três checkpoints foram concluídos e validados manualmente. O fluxo jogável, o catálogo, a revisão e os efeitos provisórios das onze soluções estão ativos. Ajustes de layout e assets permanecem fora do fechamento funcional do protótipo.

## Isolamento e cenários

- `feedback_dano`: mapa do problema e de reaplicação da solução.
- `feedback_intervencao`: sala intermediária de escolha, justificativa e revisão.
- `SimpleCutscene`: único ponto de entrada compartilhado; contém uma trigger específica para a nova fase.
- A fase de affordance mantém catálogo, estado e comportamento próprios.
- A fase usada para testes de assets não participa do fluxo de feedback.

## Checkpoint 1 — problema-base jogável

O primeiro checkpoint criou a base genérica mínima de fase e registrou os dois cenários de feedback sem alterar o comportamento da fase de affordance.

O mapa do problema usa plataformas sólidas e volumes de dano independentes, configurados no Tiled. O jogador possui cinco corações; cada contato válido remove um coração e inicia 1,5 segundo de invulnerabilidade. Se o jogador permanecer no volume perigoso, um novo dano é aplicado quando a proteção termina.

Antes de qualquer intervenção, a única resposta visível ao dano é a alteração do HUD. Não há recuo, piscar, partículas, transparência ou som. A vida zerada abre o menu de morte; tentar novamente restaura vida e invulnerabilidade, e a troca de cenário também restaura a vida. Duas mortes no mapa do problema, por queda ou perda dos corações, antecipam a entrada na sala de intervenção.

### Resultado validado

- As três saídas de `SimpleCutscene` levam às fases correspondentes.
- O percurso de feedback permite entrar na sala intermediária, retornar ao mapa original e concluir.
- Cada contato válido retira exatamente um coração.
- A proteção impede dano adicional durante 1,5 segundo.
- Permanecer no perigo causa outro dano após o término da proteção.
- Morte, retry, restauração de vida e entrada antecipada funcionam.
- Affordance e teste de assets continuam isolados.

## Checkpoint 2 — catálogo e fluxo de escolha

O catálogo `FeedbackDamageChoiceCatalog` contém soluções e justificativas exclusivas da fase. Cada apresentação inicial sorteia uma solução inadequada, uma parcial e uma adequada e embaralha suas posições. As seis justificativas são embaralhadas separadamente.

### Soluções iniciais

| ID | Título                     | Intervenção                                                              | Qualidade  |
|----|----------------------------|--------------------------------------------------------------------------|------------|
| I1 | HUD reforçado              | Aumenta os corações e reforça o contraste entre cheios e vazios.         | Inadequada |
| I2 | Perigo sinalizado          | Aplica cor de perigo e símbolos de alerta às plataformas com espinhos.   | Inadequada |
| I3 | Rota alternativa           | Revela e destaca plataformas seguras próximas às perigosas.              | Inadequada |
| P1 | Impacto físico             | Aplica recuo pequeno e partículas no ponto de contato.                   | Parcial    |
| P2 | Proteção visível           | Faz o personagem piscar e exibe um contorno durante a invulnerabilidade. | Parcial    |
| P3 | Recuo e alerta             | Combina recuo pequeno e destaque das plataformas perigosas.              | Parcial    |
| A1 | Recuo e piscar             | Combina recuo pequeno e piscar durante toda a invulnerabilidade.         | Adequada   |
| A2 | Recuo e pulso              | Combina recuo pequeno e pulso no contorno durante a proteção.            | Adequada   |
| A3 | Compressão e transparência | Combina recuo, compressão breve e alternância de transparência.          | Adequada   |

### Soluções exclusivas de revisão

| ID | Título             | Intervenção                                                                 | Qualidade |
|----|--------------------|-----------------------------------------------------------------------------|-----------|
| P4 | Reação de impacto  | Aplica recuo e mantém brevemente uma marca no ponto de contato.             | Parcial   |
| A4 | Impacto e proteção | Aplica recuo e exibe uma silhueta intermitente durante a invulnerabilidade. | Adequada  |

### Justificativas

| ID | Resultado esperado                                          | Dimensão observada     |
|----|-------------------------------------------------------------|------------------------|
| J1 | Facilitar a leitura da vida restante.                       | Consequência acumulada |
| J2 | Reconhecer plataformas perigosas antes do contato.          | Antecipação            |
| J3 | Atravessar a área sem tocar nas plataformas perigosas.      | Desvio do problema     |
| J4 | Perceber o instante e a direção do dano.                    | Evento                 |
| J5 | Perceber enquanto a proteção temporária está ativa.         | Estado temporário      |
| J6 | Relacionar contato e perda de vida e acompanhar a proteção. | Evento e estado        |

Soluções parciais e adequadas abrem a saída. Uma solução inadequada mantém a porta fechada e oferece somente P4 e A4 para revisão, sem solicitar outra justificativa. O estado da fase distingue a primeira solução, a primeira justificativa, a solução efetivamente aplicada e a ocorrência de revisão.

### Resultado validado

- A fase usa somente o próprio catálogo e a própria pergunta.
- Cada apresentação contém uma solução de cada qualidade, sem posições fixas.
- P4 e A4 aparecem somente depois de uma escolha inadequada.
- A solução revisada substitui a inadequada anteriormente aplicada.
- O catálogo e o fluxo da fase de affordance continuam independentes.

## Checkpoint 3 — soluções aplicadas

Os efeitos foram implementados como módulos combináveis. A solução aplicada controla explicitamente quais módulos ficam ativos na sala intermediária e no mapa original; uma revisão substitui o conjunto anterior em vez de acumulá-lo.

| ID | Módulos aplicados                       | Leitura proposta                                             |
|----|-----------------------------------------|--------------------------------------------------------------|
| I1 | Ênfase visual do HUD                    | Consequência acumulada; não comunica evento ou proteção.     |
| I2 | Destaque e símbolos nas áreas perigosas | Antecipação; não comunica o dano recebido.                   |
| I3 | Rota segura autorada no Tiled           | Evita o problema; não comunica evento ou proteção.           |
| P1 | Recuo e partículas de contato           | Comunica o evento, não sua proteção posterior.               |
| P2 | Piscar e contorno                       | Comunica o estado de proteção, sem reação física localizada. |
| P3 | Recuo e destaque do perigo              | Comunica impacto e antecipação, não a duração da proteção.   |
| P4 | Recuo e marca de contato                | Comunica o evento, não a duração da proteção.                |
| A1 | Recuo e piscar                          | Comunica evento e estado temporário.                         |
| A2 | Recuo e pulso de contorno               | Comunica evento e estado temporário.                         |
| A3 | Recuo, compressão e transparência       | Comunica evento e estado temporário.                         |
| A4 | Recuo e silhueta intermitente           | Comunica evento e estado temporário.                         |

O recuo afasta horizontalmente o jogador do centro do volume de dano e usa sua direção atual como desempate. Os impulsos ficam centralizados em `DamageFeedbackSettings` para ajuste durante testes, não bloqueiam os controles e não garantem a saída completa da área perigosa.

Os efeitos de proteção terminam junto com a invulnerabilidade. Piscar, partículas, marcas, compressão, contornos, pulsos, transparência e silhuetas são provisórios e procedurais; não há áudio neste protótipo.

### Resultado validado

- Os onze IDs foram testados individualmente.
- Cada solução ativa somente os módulos definidos para ela.
- Dano, invulnerabilidade, morte, retry, transições e retorno continuam funcionando com os efeitos ativos.
- A revisão remove os efeitos da solução inadequada anterior.
- O fluxo completo da fase de affordance permanece funcional.

## Ressalvas aceitas do protótipo

Estas observações não impedem o fechamento funcional e devem ser tratadas junto de layout e assets:

- I1: o reforço de contraste do HUD ainda é pouco perceptível.
- I2: o destaque poderia abranger a plataforma inteira, além dos símbolos e da faixa perigosa.
- I3: a rota segura chega ao botão, mas ainda não conduz também até o portão de saída.
- A4: a silhueta intermitente aparece somente à esquerda; uma versão final deve distribuí-la ao redor do personagem ou usar um piscar simétrico.

## Fora do escopo deste protótipo

- Assets finais e acabamento de layout.
- Efeitos sonoros.
- Telemetria e persistência analítica das escolhas.
- Intensidades diferentes para tipos de dano.

## Componentes principais

- `Scenes/FeedbackDamageChoiceCatalog.cs`: soluções, justificativas e seleção inicial/revisão.
- `Scenes/PhaseCatalog.cs`, `PhaseDefinition.cs` e `PhaseState.cs`: definição e estado isolado da fase.
- `Scenes/ScenarioCatalog.cs` e `Scene.cs`: cenários, fluxo e aplicação dos efeitos.
- `Gameplay/DamageFeedbackSettings.cs` e `Player.cs`: parâmetros e resposta visual/física do jogador.
- `World/DamageVolume.cs` e `LevelMap.cs`: volumes perigosos, layers e desenho procedural.
- `Content/FeedbackDano.tmx`, `FeedbackIntervencao.tmx` e `SimpleCutsceneMap.tmx`: mapas e entrada da fase.

O mapeamento pedagógico correspondente está em [lm_gm_feedback_dano.md](lm_gm_feedback_dano.md).
