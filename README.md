# Timothy & Mort

Protótipo de um jogo sério de plataforma 2D sobre princípios de design.

## O projeto foca em testar o ciclo:
   > Problema, intervenção, justificativa, consequência e revisão

### Affordance e Visibilidade

```text
  Cenário como seletor de mapas (entrada)
  affordance_visibilidade (problema)
  affordance_intervencao (escolha e teste)
  affordance_visibilidade (aplicação da solução)
  Volta ao seletor (conclusão e reset da fase)
```
## Feedback

A fase de feedback de dano isola o problema de comunicar o instante do impacto e a duração da invulnerabilidade. Ela é acessada por uma entrada própria em `SimpleCutscene`, usa os cenários `feedback_dano` e `feedback_intervencao` e não compartilha estado nem catálogo com as fases de affordance ou de teste de assets.

O problema-base mostra somente a perda de corações. Depois da escolha, a intervenção selecionada é aplicada tanto na sala intermediária quanto no mapa original. Soluções inadequadas mantêm a saída fechada e abrem uma revisão; soluções parciais ou adequadas liberam a continuação.

A implementação e as ressalvas aceitas para o protótipo estão registradas em [docs/checkpoints_feedback_dano.md](docs/checkpoints_feedback_dano.md).

## Controles

| Controle            | Ação                                                     |
|---------------------|----------------------------------------------------------|
| `A` / `D`           | Mover e navegar horizontalmente nos menus.               |
| `W` / `S`           | Direção vertical do dash e navegação vertical nos menus. |
| `Space`             | Pular.                                                   |
| `LeftShift`         | Impulso (Dash) em oito direções.                         |
| `Enter`             | Confirmar uma escolha.                                   |
| `Escape`            | Abrir ou fechar pause.                                   |
| `Ctrl esquerdo + P` | Ativar ou desativar o debug visual.                      |
| `Ctrl esquerdo + R` | Recarregar o TMX atual durante jogo ou pause.            |

## Mapas e edição no Tiled

Abra [Timothy-Mort.tiled-project](Timothy-Mort.tiled-project) no Tiled. Ele aponta para a pasta `Content` e fornece classes/propriedades para spawns, entradas de intervenção e saídas.

Os cenários atuais são:

| ID                        | TMX                                  | Papel                                      |
|---------------------------|--------------------------------------|--------------------------------------------|
| `simple_cutscene`         | `Content/SimpleCutsceneMap.tmx`      | Entrada e retorno de conclusão da fase.    |
| `affordance_visibilidade` | `Content/AffordanceVisibilidade.tmx` | Percurso com o problema de affordance.     |
| `affordance_intervencao`  | `Content/AffordanceIntervencao.tmx`  | Seleção, teste e revisão das intervenções. |

O contrato completo de layers, objetos, propriedades e reload está em [docs/convencoes_cenarios.md](docs/convencoes_cenarios.md). As medidas de movimento e os marcadores de alcance estão em [docs/metricas_mov.md](docs/metricas_mov.md).

## LM-GM das fases

O mapeamento pedagógico da fase de affordance está em [docs/lm_gm_affordance_visibilidade.md](docs/lm_gm_affordance_visibilidade.md).

O mapeamento pedagógico da fase de feedback de dano está em [docs/lm_gm_feedback_dano.md](docs/lm_gm_feedback_dano.md).

Ele adapta o modelo Learning-Game Mechanics de (Arnab et al., 2015) 

> DOI: [10.1111/bjet.12113](https://doi.org/10.1111/bjet.12113)

Embora os próprios autores comentem sobre as limitações do modelo, a ideia é ter uma base estruturada
para formalizar uma documentação relacionando mecânicas de jogo e atividades de apoio a compreensão
para facilitar a replicação por outros profissionais e pesquisadores, caso tenham interesse.

## Estrutura de código

| Pasta      | Responsabilidade                                         |
|------------|----------------------------------------------------------|
| `App`      | Loop principal, transições, telas e overlays.            |
| `Core`     | Câmera e debug visual.                                   |
| `Gameplay` | Player, input e colisão.                                 |
| `Scenes`   | Estado da fase, cenários, escolhas e fluxo de jogo.      |
| `UI`       | Menus e interface de escolhas.                           |
| `World`    | Leitura e desenho dos TMX, objetos e validação de mapa.  |
| `docs`     | Contratos de cenário, métricas e documentação de design. |

`Game1` controla telas, overlays e transições. `PhaseState` preserva as decisões e apresentações sorteadas da fase. `Scene` coordena o cenário jogável. `LevelMap` lê os objetos TMX e fornece geometria, entradas, saídas, gelatinas e marcadores de medição.

## Executar

```bash
dotnet run
```
