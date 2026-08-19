# Convenções de cenários

Este documento define o contrato entre os mapas TMX do Tiled e o jogo. Cada cenário é um arquivo independente: o mapa descreve geometria e pontos de fluxo; o código decide qual cenário abrir e qual estado da fase está ativo.

## Registro de cenários

O identificador do cenário é registrado em `Scenes/ScenarioCatalog.cs`, junto do caminho do TMX e de um spawn de fallback. Os IDs atuais são `simple_cutscene`, `affordance_visibilidade` e `affordance_intervencao`.

Para criar um cenário novo, primeiro registre seu ID no catálogo e depois crie o TMX com as layers obrigatórias abaixo.

## Object layers

| Layer                  | Obrigatória | Finalidade                                                                         |
|------------------------|-------------|------------------------------------------------------------------------------------|
| `mapaGeral`            | Sim         | Retângulos sólidos de colisão e blockout.                                          |
| `spawnJogador`         | Sim         | Um ou mais spawns nomeados.                                                        |
| `gelatina`             | Não         | Volumes atravessáveis: limitam a queda e restauram um dash gasto.                  |
| `saidaCena`            | Não         | Áreas que levam a outro cenário.                                                   |
| `entradaIntervencao`   | Não         | Entradas para a sala de intervenção; a fase ativa a variante tardia ou antecipada. |
| `botaoIntervencao`     | Não         | Botões acionados por cima para abrir escolhas.                                     |
| `portaSaida`           | Não         | Colisor da porta de retorno da sala de intervenção.                                |
| `destinoPortaSaida`    | Não         | Ponto para onde a porta sobe após a primeira escolha.                              |
| `rotaVerticalSolida`   | Não         | Plataformas sólidas reveladas após a primeira escolha.                             |
| `rotaVerticalGelatina` | Não         | Gelatinas reveladas após a primeira escolha.                                       |
| `medicaoDebug`         | Não         | Par de pontos para medir uma travessia no debug.                                   |
| `triggerColision`      | Não         | Trigger legado de cutscene, mantido no mapa de entrada.                            |

O validador de TMX exige `mapaGeral`, `spawnJogador` e pelo menos um spawn com coordenadas válidas. As outras layers podem estar ausentes.

## Objetos e propriedades

Todos os elementos jogáveis são objetos retangulares do Tiled, exceto pontos como `destinoPortaSaida` e os marcadores de medição. A posição superior esquerda de um objeto de `spawnJogador` é usada como posição do player.

### `spawnJogador`

Cada spawn precisa ter `name` único dentro do mapa. Saídas e entradas referenciam esse nome em `spawnDestino`.

### `saidaCena`

| Propriedade             | Tipo        | Uso                                                                                            |
|-------------------------|-------------|------------------------------------------------------------------------------------------------|
| `destino`               | `CenarioId` | ID do cenário que será aberto.                                                                 |
| `spawnDestino`          | texto       | Nome do spawn no cenário de destino.                                                           |
| `requerSolucaoAplicada` | booleano    | Quando `true`, a saída só funciona após uma solução parcial ou adequada abrir a porta da fase. |

Se `spawnDestino` não existir, o jogo usa o primeiro spawn válido do mapa de destino e, por último, o fallback do `ScenarioCatalog`.

### `entradaIntervencao`

| Propriedade    | Tipo                        | Uso                                      |
|----------------|-----------------------------|------------------------------------------|
| `destino`      | `CenarioId`                 | Normalmente `affordance_intervencao`.    |
| `spawnDestino` | texto                       | Spawn de entrada da sala de intervenção. |
| `posicao`      | `PosicaoEntradaIntervencao` | `tardia` ou `antecipada`.                |

Na fase de affordance, a entrada tardia está ativa inicialmente. Após duas mortes por queda, a entrada antecipada passa a ser a variante ativa.

### `medicaoDebug`

Um par válido possui exatamente uma origem e um destino com os mesmos valores de `parId` e `acao`.

| Propriedade | Valores                                                         |
|-------------|-----------------------------------------------------------------|
| `parId`     | Identificador livre do par, por exemplo `intervencao_teste_01`. |
| `papel`     | `origem` ou `destino`.                                          |
| `acao`      | A ação medida, por exemplo `dash`.                              |

Com o debug ativo, o jogo desenha a linha entre os pontos e informa ΔX, ΔY, pixels e tiles.

## Classes do projeto Tiled

[Timothy-Mort.tiled-project](../Timothy-Mort.tiled-project) define as classes `SpawnJogador`, `SaidaCena` e `EntradaIntervencao`, além dos enums `CenarioId` e `PosicaoEntradaIntervencao`. Use essas classes no Tiled para evitar erros de escrita nas propriedades.

## Reload do TMX

Durante jogo ou pause, salve o TMX no Tiled e pressione `Ctrl esquerdo + R`.

- O mapa atual é reconstruído e o player volta ao spawn de entrada.
- `PhaseState` e a solução aplicada permanecem.
- Se o TMX estiver inválido, o último mapa válido continua ativo e o debug mostra o erro.

## Regras de organização

- Colisão sólida fica apenas em `mapaGeral`.
- `gelatina` não deve ser duplicada em `mapaGeral`.
- O TMX não classifica soluções como inadequadas, parciais ou adequadas; isso pertence ao catálogo de escolhas.
- O TMX não contém decisões, justificativas ou telemetria.
- Use a grade de `32 x 32` pixels como referência de blockout. O player atual mede `16 x 16` pixels.

Consulte [metricas_mov.md](metricas_mov.md) antes de fixar distâncias de uma travessia.
