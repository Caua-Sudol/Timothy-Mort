# Teste Eureka

Abra `Content/Maps/EurekaTeste.ldtk` no LDtk. A sala de teste do menu agora usa este arquivo, derivado de `Sala_intervencao` do Affordance. O mapa original e o antigo teste_moldura.ldtk foram preservados.

- Sala: 2880 x 1600; colisao em grade de 48 px.
- `Casa_BG`: assets/eureka/bg_casa.png, desenhado atras do jogador.
- `Casa_FG`: assets/eureka/fg_casa.png, desenhado na frente do jogador.
- As duas imagens usam o canvas original de 960 x 768, sem escala, na origem local X=960, Y=384.
- Nao existe arte MG neste teste. A camada `Colisao` define os apoios sem desenhar preenchimentos no jogo.
- `Entidades` contem apenas PlayerSpawn, com pivo nos pes. Saidas e espada foram retiradas desta copia de teste.

Colisoes iniciais provisorias, para ajustar no editor:

| Apoio | X | Y (topo) | Largura | Altura |
| --- | ---: | ---: | ---: | ---: |
| Chao da casa | 960 | 1152 | 960 | 48 |
| Plataforma esquerda | 1152 | 720 | 192 | 48 |
| Plataforma direita | 1488 | 480 | 192 | 48 |

O spawn esta em X=1056, Y=1152 (pes). Ajuste-o junto com o chao. As distancias entre plataformas sao um ponto de partida visual, nao um percurso de saltos validado.

Salve no LDtk e use Ctrl+R dentro da sala para recarregar mapa e PNGs. R retorna ao spawn. Ctrl+P alterna o debug de colisoes e grade. O preset inicial do jogador e 1, com referencia de 14% da altura da tela. O fundo de floresta e os controles de teste do jogador continuam disponiveis.

Pinte `Colisao` com solido (1) para mudar o apoio; os PNGs nao criam colisao automaticamente. Mantenha Casa_BG e Casa_FG alinhadas ao mover a composicao inteira. Para mover apenas as duas plataformas, edite seus tiles em Casa_FG e as respectivas colisoes.
