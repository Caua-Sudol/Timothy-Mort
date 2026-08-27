# Checkpoints: modelos conceituais e restrições

Este documento registra o protótipo validado da fase `modelos_conceituais_restricoes`. A fase é independente das de affordance, feedback e teste de assets: possui dois cenários, catálogo, estado e efeitos próprios. A única integração compartilhada é a entrada correspondente em `SimpleCutscene`.

## Estado atual

Os seis checkpoints funcionais foram concluídos e validados manualmente. O percurso apresenta comportamentos visualmente inconsistentes, a sala intermediária oferece escolha e justificativa, e a solução aplicada permanece ativa ao retornar ao mapa do problema.

Os textos de cartas e justificativas são provisórios e podem ser ajustados sem alterar a estrutura funcional descrita aqui.

## Problema-base e cenários

- `modelos_conceituais_restricoes`: mapa principal, no arquivo `Content/ModelosConceituaisRestricoes.tmx`.
- `modelos_conceituais_restricoes_intervencao`: sala de escolha, teste e revisão, no arquivo `Content/ModelosConceituaisRestricoesIntervencao.tmx`.

O mapa principal mistura pares visualmente semelhantes com propriedades distintas:

- a gelatina padrão reduz a queda e restaura um dash gasto;
- a gelatina-trampolim usa a mesma apresentação inicial, mas aplica um impulso vertical de `360` e não recupera o dash;
- o espinho de dano remove um coração;
- o espinho inofensivo usa a mesma apresentação inicial, mas não remove vida.

O jogador possui cinco corações e recebe 1,5 segundo de invulnerabilidade após dano. Para tornar a restrição de vida parte observável do problema, a saída final também pode ser liberada no mapa principal quando o jogador está com dois corações ou menos. Essa condição não encerra a intervenção: a entrada da sala intermediária continua ativa até uma solução parcial ou adequada ser aplicada.

## Fluxo

```text
mapa do problema -> entrada de intervenção -> escolha de solução
-> justificativa -> teste na sala intermediária -> retorno ao mapa principal
-> conclusão ou revisão
```

Depois de duas mortes no mapa principal, a entrada antecipada da sala intermediária torna-se ativa. Uma primeira escolha inadequada mantém a porta fechada; o jogador pode sair do botão e retornar para escolher P4 ou A4, sem nova justificativa. Uma escolha parcial ou adequada abre a porta e permite o retorno ao cenário principal.

## Checkpoint 1 — isolamento e cenários

Foram registrados os dois IDs de cenário, a definição de fase e os dois mapas TMX. A entrada em `SimpleCutscene` aponta para essa fase sem depender do estado das fases anteriores.

## Checkpoint 2 — problema-base e restrições

Foram configurados os tipos mistos de gelatina e espinho, vida máxima, invulnerabilidade, limiar de dois corações e a sala intermediária. A gelatina-trampolim aplica o impulso somente uma vez; ela volta a ficar disponível depois que o jogador toca um chão sólido, evitando pulos infinitos.

## Checkpoint 3 — soluções inadequadas

I1–I3 foram testadas com posições fixas para validar seus efeitos isoladamente. Elas preservam os comportamentos contraditórios e mantêm a porta fechada.

## Checkpoint 4 — soluções parciais

P1–P3 foram testadas com posições fixas. Cada uma corrige apenas o modelo das gelatinas ou dos espinhos e, por isso, abre a porta sem resolver toda a inconsistência.

## Checkpoint 5 — soluções adequadas

A1–A3 foram testadas com posições fixas. Elas alinham os dois modelos por normalização dos comportamentos ou por diferenciação visual acompanhada de propriedades compatíveis.

## Checkpoint 6 — catálogo final e revisão

O catálogo final sorteia uma solução inadequada, uma parcial e uma adequada, embaralha suas posições e embaralha as seis justificativas. P4 e A4 aparecem somente na revisão posterior a uma inadequada.

## Soluções iniciais

| ID | Título | Intervenção | Qualidade |
|----|--------|-------------|-----------|
| I1 | Realce uniforme | Destaca igualmente gelatinas e espinhos. | Inadequada |
| I2 | Regras explícitas | Exibe lembretes curtos de `DANO` e `DASH`. | Inadequada |
| I3 | Rota indicada | Indica visualmente a rota até a saída. | Inadequada |
| P1 | Espinhos ativos | Faz todos os espinhos causarem dano e realça as plataformas especiais. | Parcial |
| P2 | Gelatinas restauradas | Restaura as gelatinas e mantém lembretes de `DANO` e `DASH`. | Parcial |
| P3 | Perigo consistente | Faz todos os espinhos causarem dano e indica a rota. | Parcial |
| A1 | Plataformas restauradas | Restaura gelatinas e espinhos às propriedades originais. | Adequada |
| A2 | Variantes diferenciadas | Diferencia ambas as variantes e ajusta suas propriedades. | Adequada |
| A3 | Gelatina restaurada | Restaura as gelatinas e diferencia os espinhos alterados. | Adequada |

## Soluções exclusivas de revisão

| ID | Título | Intervenção | Qualidade |
|----|--------|-------------|-----------|
| P4 | Gelatinas originais | Restaura somente as gelatinas. | Parcial |
| A4 | Gelatina distinta | Diferencia a gelatina-trampolim e restaura os espinhos. | Adequada |

## Comportamentos aplicados

| Grupo | Resultado no protótipo |
|-------|------------------------|
| I1–I3 | Melhoram saliência, regra textual ou orientação, mas não alinham aparência e comportamento. |
| P1 e P3 | Todos os espinhos passam a causar um dano; a gelatina-trampolim continua visualmente ambígua. |
| P2 e P4 | A gelatina-trampolim volta a reduzir queda e restaurar dash; os espinhos continuam visualmente ambíguos. |
| A1 | Todas as gelatinas usam o comportamento padrão e todos os espinhos causam um dano. |
| A2 | A gelatina-trampolim fica verde, mantém o impulso inicial de velocidade igual ao dash e não recupera dash; o espinho alterado fica magenta e causa dois danos. |
| A3 | Todas as gelatinas usam o comportamento padrão; o espinho alterado fica magenta e causa dois danos. |
| A4 | A gelatina-trampolim fica verde, mantém o impulso equivalente ao dash e não recupera dash; todos os espinhos voltam a causar um dano. |

As soluções de revisão substituem os módulos da escolha inadequada anterior; os efeitos não são acumulados.

## Justificativas

| ID | Resultado esperado |
|----|--------------------|
| J1 | Facilitar a identificação das plataformas importantes para a travessia. |
| J2 | Explicar diretamente quais recursos cada plataforma deveria oferecer. |
| J3 | Indicar o percurso necessário para alcançar a saída. |
| J4 | Fazer plataformas visualmente iguais manterem o mesmo comportamento. |
| J5 | Diferenciar plataformas que possuem comportamentos diferentes. |
| J6 | Alinhar aparência, comportamento e restrições do percurso. |

A qualidade da solução é definida pelo catálogo; a justificativa continua uma previsão escolhida pelo jogador e pode ou não corresponder à intervenção selecionada.

## Componentes principais

- `Scenes/ConceptualModelsConstraintsChoiceCatalog.cs`: soluções, justificativas, sorteio inicial e revisão.
- `Gameplay/ConceptualModelsConstraintsSettings.cs`: vida, invulnerabilidade e impulsos da gelatina-trampolim.
- `Scenes/PhaseCatalog.cs`, `PhaseDefinition.cs` e `PhaseState.cs`: definição e estado isolado da fase.
- `Scenes/Scene.cs` e `World/LevelMap.cs`: aplicação de comportamento, dano, colisão e apresentação procedural.
- `Scenes/ConceptualRuleReminderPresentation.cs`: lembretes de regra usados por I2 e P2.
- `Content/ModelosConceituaisRestricoes.tmx` e `Content/ModelosConceituaisRestricoesIntervencao.tmx`: geometria, tipos de plataforma, entradas, botão e porta.

## Fora do escopo do protótipo

- Redação final das cartas e justificativas.
- Assets finais, áudio e acabamento de layout.
- Telemetria ou persistência analítica das escolhas.
- Balanceamento final de dano, vida e alcance do impulso da gelatina-trampolim.

O mapeamento pedagógico correspondente está em [lm_gm_modelos_conceituais_restricoes.md](lm_gm_modelos_conceituais_restricoes.md).
