# Métricas de movimento

Este documento serve como régua inicial para desenhar mapas no Tiled. Os valores foram pensados para o estado atual de `Gameplay/Player.cs` e para mapas com tiles de `32 x 32` pixels.

## Unidade base

| Medida    | Pixels | Tiles |
|-----------|-------:|------:|
| Meio tile |   16px |   0.5 |
| 1 tile    |   32px |     1 |
| 2 tiles   |   64px |     2 |
| 3 tiles   |   96px |     3 |
| 5 tiles   |  160px |     5 |
| 10 tiles  |  320px |    10 |

O player atual tem `16 x 16px`, então ocupa meio tile de largura e altura. Para desenhar no Tiled, pense primeiro em tiles inteiros e use meio tile apenas para ajuste fino.

## Valores atuais do player

| Metrica                              |      Valor |       Em tiles | Leitura pratica                    |
|--------------------------------------|-----------:|---------------:|------------------------------------|
| Tile do mapa                         |       32px |         1 tile | Unidade principal no Tiled         |
| Player                               |    16x16px | 0.5 x 0.5 tile | Hitbox pequena para tile de 32px   |
| Velocidade X maxima                  |   528 px/s |   16.5 tiles/s | Movimento horizontal rapido        |
| Aceleracao no chao                   | 3300 px/s² |   103 tiles/s² | Chega rapido na velocidade maxima  |
| Aceleracao no ar                     | 1800 px/s² | 56.25 tiles/s² | Controle aereo forte               |
| Gravidade                            | 1800 px/s² | 56.25 tiles/s² | Pulo/queda arcade                  |
| Velocidade inicial do pulo           |   600 px/s |  18.75 tiles/s | Pulo forte                         |
| Tempo ate o topo do pulo             |     ~0.33s |              - | Tempo subindo antes de cair        |
| Altura maxima teorica                |     ~100px |     ~3.1 tiles | Limite vertical aproximado         |
| Tempo total no ar                    |     ~0.66s |              - | Subir e cair ate a mesma altura    |
| Distancia horizontal partindo parado |     ~274px |     ~8.5 tiles | Pulo horizontal mais realista      |
| Distancia horizontal com embalo      |     ~352px |      ~11 tiles | Limite teorico com velocidade alta |
| Dash horizontal                      |     ~173px |     ~5.4 tiles | `960 px/s * 0.18s`                 |

## Regua para desenhar fases

| Situacao                         |    Confortavel |       Dificil | Evitar como caminho obrigatorio |
|----------------------------------|---------------:|--------------:|--------------------------------:|
| Pulo vertical                    |    1 a 2 tiles |       3 tiles |                 mais de 3 tiles |
| Buraco sem dash, partindo parado |    3 a 5 tiles |   6 a 8 tiles |                 mais de 8 tiles |
| Buraco sem dash, com embalo      |    5 a 7 tiles |  8 a 10 tiles |                mais de 10 tiles |
| Dash horizontal puro             |    3 a 4 tiles |       5 tiles |               mais de 5.5 tiles |
| Pulo + dash                      |   8 a 11 tiles | 12 a 14 tiles |                15 ou mais tiles |
| Plataforma de pouso              |    3 a 4 tiles |   1 a 2 tiles |           1 tile em rota normal |
| Corredor/passagem                | 1 tile ou mais |             - |               menos de 0.5 tile |

## Como usar no Tiled

No Tiled, use a grid como regua principal.
Se o tile tem `32px`, uma plataforma 2 tiles acima esta `64px` acima.
Uma plataforma 3 tiles acima esta `96px` acima.
Um buraco de 5 tiles tem `160px`.

Uma forma simples de trabalhar:

1. Desenhe a rota principal usando desafios confortaveis.
2. Use desafios dificeis apenas quando quiser testar precisao.
3. Evite colocar o caminho obrigatorio no limite teorico do movimento.
4. Para testar a metrica, crie plataformas com diferenca de altura entre 1 e 2 tiles.
5. Para testar dash, use buracos de 4 a 5 tiles.

## Guia visual opcional

Se quiser facilitar no Tiled, crie uma object layer que nao sera usada pelo jogo, por exemplo:

```text
metricas_guia
```

Nessa layer, desenhe retangulos de referencia:

| Guia               | Tamanho sugerido                          |
|--------------------|-------------------------------------------|
| `jump_confortavel` | 5 tiles de largura por 2 tiles de altura  |
| `jump_dificil`     | 8 tiles de largura por 3 tiles de altura  |
| `dash`             | 5 tiles de largura                        |
| `jump_dash`        | 11 tiles de largura por 3 tiles de altura |

Essa layer serve apenas como régua visual dentro do editor. O jogo continua lendo somente as layers declaradas em [convencoes_cenarios.md](convencoes_cenarios.md).

## Medir uma travessia no mapa

Ative o debug com `Ctrl esquerdo + P`. O HUD mostra cenário, posição em pixels e tiles, velocidade, estado, contato com o chão e disponibilidade de dash.

Para medir uma travessia declarada no TMX, use a layer opcional `medicaoDebug` e crie dois pontos com as propriedades `parId`, `papel` e `acao`. O contrato completo está em [convencoes_cenarios.md](convencoes_cenarios.md).

Depois de salvar o mapa, use `Ctrl esquerdo + R` para recarregá-lo sem reiniciar o jogo. A linha de debug deve acompanhar os marcadores e informar ΔX, ΔY, pixels e tiles.