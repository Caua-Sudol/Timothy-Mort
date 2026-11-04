# Checkpoint 4 — sala de escala

> **Documento histórico.** Registro de outubro/2026. A sala de teste do menu passou a usar `Content/Maps/EurekaTeste.ldtk` (ver [EurekaTeste.md](../Content/Maps/EurekaTeste.md)); valores de escala podem ter mudado.

Na seleção de fases, o botão “Sala de teste / composição” fica abaixo das três fases.
Use S ou seta para baixo para selecioná-lo e Enter para abrir. W ou seta para cima
retorna aos cartões principais. Esc sai da sala e volta à seleção.
O acesso por Ctrl+T dentro das fases foi removido. Não é necessário abrir Affordance.
A sala é reutilizada ao reabrir: use R para voltar ao spawn e Ctrl+R para ler alterações no Tiled.
A sala não recebe PhaseState e não registra mortes, tempo ou resultados da pesquisa.

## Comparação

- A grade começa ligada; Ctrl+P alterna sua visibilidade.
- B alterna ANTES (zoom 1, player 1, física base) e DEPOIS (zoom escolhido, escala proporcional).
- B reinicia no mesmo spawn para permitir uma comparação consistente.
- 1/2/3/4 escolhem 1 / 0,85 / 0,75 / 0,65 no modo DEPOIS.
- R reinicia; Ctrl+R recarrega este TMX. Uma falha mantém o último mapa válido.
- Os dois modos usam o mesmo layout e centro de câmera: não são uma reconstrução da fase antiga.

## Layout no Tiled

`Content/ScaleTest.tmx`: 90×50 tiles de 32, ou 2880×1600 unidades.
A câmera desta sala usa o centro do mapa (1440, 800). No zoom 0,675,
a área visível tem aproximadamente 2844×1600 unidades: corta 18 nas laterais.

Da esquerda para a direita, na rota inferior:

| Elemento | Posição / dimensões em unidades |
|---|---|
| Spawn | (512, 1200), sobre piso em Y=1216 |
| Apoio baixo | (576, 1152), 256×64; subida de 64 |
| Apoio alto | (928, 1088), 192×32; mais 64 acima do apoio baixo |
| Vão | X=1120 até 1248; largura 128, aberto até o fundo |
| Apoio 6×1 | (1376, 1152), 192×32 |
| Apoio 1×1 | (1664, 1152), 32×32 |
| Parede | (1984, 864), 64×352, a partir do piso |

Teto, paredes laterais e seis apoios superiores compõem a tela inteira.
Os apoios superiores servem à comparação visual; não formam uma rota de pulo normal.
As texturas já existentes são aplicadas aos objetos pelo nome; não há PNG novo ou reexportado.

ANTES, com zoom 1, o enquadramento central mostra apenas parte da sala.
DEPOIS, com zoom 0,675, a câmera mostra praticamente toda a sala.
O player ocupa 14% na escala 1 ou 11,9% na escala 0,85.
O fundo preenche a tela nos dois modos; o teste não resolve o fundo da fase principal.

## Valores e validação manual

`Core/GameScaleConfig.cs` centraliza câmera, escalas e física base.
O padrão atual é zoom 0,675 e escala 1; 0,85 continua disponível para decisão visual.
Na escala 1: velocidade 528, pulo 600, gravidade -1800, dash efetivo 720.
Na escala 0,85: 448,8 / 510 / -1530 / 612. Tempos: dash 0,18 s;
coyote e buffer 0,10 s. As grandezas espaciais acompanham a escala para
preservar o movimento medido em alturas de player.

Compare o apoio baixo, o alto, o vão e a parede nas escalas 1 e 0,85.
Confira que cair no vão reinicia a sala e que B restaura o spawn e a grade
permite comparar os enquadramentos. Teste Ctrl+R e a saída por Esc.
Nenhuma captura ou execução automatizada foi realizada durante a implementação.

Para comparar com os valores originais, use B (ANTES). Para voltar ao padrão
original na fase principal, os valores de configuração são câmera/preset 0
e escala/preset 1; o TMX reorganizado manualmente não é revertido por isso.

Limitações: a hitbox original 16×16 continua menor que o desenho; o
arredondamento do collider e a amostragem point afetam escalas fracionárias.
Esticar as mesmas imagens pode mudar a aparência dos contornos. Não é possível
recuperar detalhes ausentes ou redesenhar a espessura interna dos traços apenas
com zoom e escala. A sala não altera as bordas ou plataformas da fase principal.

Exercício (10–15 min): no Tiled, ajuste a largura do apoio 6×1 para 8×1,
salve e use Ctrl+R. Compare a aterrissagem e o contorno e depois restaure 6×1.
Dívida de domínio: nenhuma nova registrada; o alinhamento da fase principal
e a revisão de suas plataformas continuam pendentes por decisão do usuário.
