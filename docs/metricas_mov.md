# Métricas de movimento

Régua para desenhar salas no LDtk (`Content/Maps/Affordance.ldtk`, grade de 96). Os valores vêm de `Core/GameScaleConfig.cs` (estado de 04/11/2026) com o jogador na escala 1, usada em todas as salas. São limites **teóricos**: o caminho obrigatório deve ficar com folga.

> A versão anterior deste documento usava tiles de 32 px do Tiled. Ela foi substituída; os mapas TMX das fases não são mais carregados.

## Constantes

| Constante | Valor | Observação |
|---|---:|---|
| Velocidade horizontal máxima | 528 /s | `PlayerHorizontalSpeed` |
| Aceleração no chão / no ar | 3300 / 1800 /s² | — |
| Gravidade | 1800 /s² | — |
| Velocidade inicial do pulo | 620 /s | soltar o pulo corta a subida (240 /s) |
| Dash | 768 /s por 0,25 s | `PlayerDashSpeed` × `PlayerDashDuration`; antes, uma pausa parada de 0,06 s |
| Coyote time / buffer de pulo | 0,15 s / 0,15 s | tolerância para pular tarde ou cedo |
| Mini-jump (fase 3) | 880 /s para cima | `TrampolineVerticalSpeed` |

O dash **só funciona no ar**. Ele vai para os lados, para cima e na diagonal para cima (nunca para baixo), e é recarregado ao tocar o chão ou uma gelatina.

## Alcances teóricos

| Movimento | Unidades | Tiles de 96 |
|---|---:|---:|
| Altura máxima do pulo | ≈ 107 | ≈ 1,1 |
| Tempo no ar (mesma altura) | ≈ 0,69 s | — |
| Distância de pulo com embalo | ≈ 364 | ≈ 3,8 |
| Dash (qualquer direção) | ≈ 192 | 2 |
| Pulo + dash para cima | ≈ 300 | ≈ 3,1 |
| Pulo + dash para o lado (com embalo) | ≈ 550 | ≈ 5,7 |
| Mini-jump | ≈ 215 de altura | ≈ 2,2 |

## Régua para o caminho obrigatório

| Situação | Confortável | Difícil | Evitar |
|---|---|---|---|
| Degrau só com pulo | até 0,75 tile | 1 tile | mais de 1 tile |
| Subida com pulo + dash para cima | até 2,5 tiles | 3 tiles | mais de 3 tiles |
| Buraco só com pulo | até 3 tiles | 3,5 tiles | mais de 3,75 tiles |
| Buraco com pulo + dash | até 4,5 tiles | 5 a 5,5 tiles | mais de 5,5 tiles |
| Plataforma de pouso | 2 tiles ou mais | 1 tile | menos de 1 tile |

Combinar direções no dash (diagonal = lado + cima + dash) foi o ponto mais citado pelos especialistas. Prefira introduzir a diagonal depois do tutorial, e não como primeira exigência da Affordance.

## Medir no jogo

Em Debug, `Ctrl + P` mostra a posição, a velocidade, o estado, o contato com o chão e o dash disponível. `Ctrl + R` recarrega o mapa salvo no LDtk sem reiniciar.
