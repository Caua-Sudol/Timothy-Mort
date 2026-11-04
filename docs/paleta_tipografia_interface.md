# Paleta e tipografia da interface

Referência das telas integradas até 20/09/2026. Documento de registro; não altera o jogo.
Layout de referência: 1920 × 1080 pixels lógicos.

## Cores

| Uso                          | Hex       | RGB           | Origem / aplicação                                                        |
|------------------------------|-----------|---------------|---------------------------------------------------------------------------|
| Fundo carvão                 | `#15191F` | 21, 25, 31    | Sessão, seleção de fases, explicações e finalizações.                     |
| Texto principal / marfim     | `#D8D0BB` | 216, 208, 187 | Títulos, conteúdo principal e opções selecionadas.                        |
| Texto secundário             | `#B5ADA1` | 181, 173, 161 | Instruções, navegação, opções não selecionadas e mensagem da tela inicial. |
| Texto sobre cards claros     | `#25282B` | 37, 40, 43    | Texto dos cards na seleção de fases.                                      |
| Progresso salvo              | `#A69BAE` | 166, 155, 174 | Mensagem de persistência bem-sucedida na conclusão da fase.               |
| Fundo da arte do seletor     | `#302833` | 48, 40, 51    | card selecionado                                                          |
| Papel do card não selecionado | `#B8AE91` | 184, 174, 145 | card não selecionado                                                      |
| Erro                         | `#FF4500` | 255, 69, 0    | `Color.OrangeRed`: erros de identificação/persistência.                   |
| Icones flutuantes            | `#A69BAE` | 166, 155, 174 | Icones em loop                                                            |

As cores de fundo e texto acima foram conferidas em `UI/ParticipantIdentificationScreen.cs`,
`UI/PhaseSelectMenu.cs`, `UI/PhaseIntroductionScreen.cs`, `UI/PhaseResultScreen.cs`,
`UI/TitleScreen.cs`, `UI/PauseMenu.cs` e `UI/DeathMenu.cs`.

As cores do seletor e do papel são referências fornecidas pelo autor, não amostragens dos PNGs.
A aparência do card selecionado e do fundo da tela de título está embutida nas imagens;
não há um único hexadecimal no código que descreva essas artes integralmente.
Os PNGs são desenhados com `Color.White`, que preserva suas cores originais.

Pausa e morte mantêm o cenário visível: usam textos e seletor, sem fundo próprio.
As folhas decorativas atuais usam `Color.ForestGreen` (`#228B22`) multiplicado por 0,5;
esse é um efeito provisório, não uma cor definida para os futuros símbolos místicos.

## Tipografia

| Função                                           | Família                 | Arquivo em Content           | Asset carregado | Tamanho-base no SpriteFont |
|--------------------------------------------------|-------------------------|------------------------------|-----------------|----------------------------|
| Principal: títulos e títulos dos cards           | Walter Turncoat Regular | `WalterTurncoat-Regular.ttf` | `walter`        | 64                         |
| Leitura: textos, opções, explicações e navegação | Patrick Hand Regular    | `PatrickHand-Regular.ttf`    | `patrick`       | 32                         |

Configurações: `Content/walter.spritefont` e `Content/patrick.spritefont`.
Ambas usam estilo Regular, espaçamento adicional 0 e kerning ativado.
Faixas incluídas: caracteres 32–126 e 160–255, incluindo acentos portugueses comuns.

Os tamanhos acima são os valores de compilação do SpriteFont, não uma garantia de altura
em pixels nem uma equivalência direta ao tamanho mostrado no Affinity. As telas aplicam
escalas e ajustes para caber nos limites; validar a aparência no jogo em 1920 × 1080.

Os arquivos TTF locais alimentam a compilação do conteúdo MonoGame. O jogo carrega
os SpriteFonts compilados: o jogador não precisa instalar essas fontes no sistema.
No Affinity, usar as mesmas famílias para aproximar o protótipo da apresentação no jogo.

A tela inicial usa a arte do título já no PNG e Patrick na mensagem para continuar.
A etapa de soluções usa Walter nos títulos e Patrick nas descrições e navegação.
A etapa de justificativas usa Walter na pergunta e Patrick nas alternativas e navegação.

## Quadro de soluções

Paleta definida com o autor para a arte no Affinity. As texturas preservam as cores
e variações pintadas; os valores abaixo são referências, não amostragens dos PNGs.

| Elemento | Hex | RGB |
| --- | --- | --- |
| Papel envelhecido do quadro | `#B8AE91` | 184, 174, 145 |
| Carvão dos contornos da arte | `#15191F` | 21, 25, 31 |
| Fundo escuro dos cards | `#2A2C35` | 42, 44, 53 |
| Contorno e divisor selecionados | `#6C5274` | 108, 82, 116 |
| Olho do quadro | `#C2901C` | 194, 144, 28 |
| Texto sobre os cards | `#D8D0BB` | 216, 208, 187 |
| Pergunta e navegação sobre o papel | `#25282B` | 37, 40, 43 |

A cor `#302833` permanece como referência do seletor anterior.
O dourado `#C2901C` é o detalhe do olho, não o indicador de seleção.

### Layout aprovado no Affinity

Coordenadas em pixels no documento mestre de 1920 × 1080, com origem no canto superior esquerdo.

| Elemento | X | Y | Largura | Altura |
| --- | ---: | ---: | ---: | ---: |
| Quadro | 96 | 96 | 1728 | 888 |
| Card 1 | 227,3 | 300 | 480 | 480 |
| Card 2 | 736,7 | 300 | 480 | 480 |
| Card 3 | 1243,3 | 300 | 480 | 480 |
| Pergunta superior | 248,7 | 185,3 | 1456 | 82,7 |
| Navegação inferior | 488,7 | 833,3 | 994,7 | 53,3 |

Assets: `assets/frame_cards.png`, `assets/card_solution_no_selected.png` e
`assets/card_solution_selected.png`. Os textos são desenhados pelo jogo.
Os títulos dos cards ficam acima do divisor pintado e as descrições abaixo;
a escala diminui quando necessário para caber. Rodadas de revisão com menos de
três opções mantêm os cards centralizados. As justificativas usam o mesmo quadro e a mesma paleta.

## Quadro de justificativas

Layout de seis cards, em duas colunas e três linhas, no mestre de 1920 × 1080.
Quadro compartilhado com soluções: X = 96, Y = 96, largura = 1728, altura = 888.

| Elemento | X | Y | Largura | Altura |
| --- | ---: | ---: | ---: | ---: |
| Pergunta superior | 249,3 | 172 | 1456 | 72 |
| Card superior esquerdo | 249,3 | 266,7 | 720 | 160 |
| Card superior direito | 1007,3 | 266,7 | 720 | 160 |
| Card central esquerdo | 249,3 | 460 | 720 | 160 |
| Card central direito | 1007,3 | 460 | 720 | 160 |
| Card inferior esquerdo | 249,3 | 653,3 | 720 | 160 |
| Card inferior direito | 1007,3 | 653,3 | 720 | 160 |
| Navegação inferior | 488,7 | 841,3 | 994,7 | 45,3 |

Os arquivos `assets/card_intervention_no_selected.png` e
`assets/card_intervention_selected.png` são os cards de **justificativa**, apesar
do nome de exportação. A seleção troca a textura. Texto em marfim `#D8D0BB`
dentro dos cards; pergunta e navegação em `#25282B` sobre o papel.
As faixas claras do protótipo apenas indicam áreas de texto e não são desenhadas no jogo.
Alternativas usam margem interna de 32 px e ajuste de escala para caber.

## Background de Affordance

Cores informadas pelo autor para a composição do cenário no Affinity.
São referências da arte, não amostragens dos PNGs nem alterações de cor no código.

| Elemento | Hex | RGB | Observação |
| --- | --- | --- | --- |
| Fundo geral | `#262F39` | 38, 47, 57 | Base do background |
| Árvores | `#20242C` | 32, 36, 44 | Silhuetas das árvores |
| Atmosfera | `#303436` | 48, 52, 54 | Variação para deixar o background mais sujo |
| Luzes | `#2C393D` | 44, 57, 61 | 60% de transparência, conforme informado pelo autor |

Os PNGs `affordance_visibility_bg.png`, `affordance_visibility_mg.png` e
`affordance_visibility_fg.png` compõem juntos o background, atrás das plataformas
e do jogador. Apenas `affordance_visibility_lgt.png` é sobreposto ao mapa inteiro,
abaixo da interface. Todos ficam em `assets/` e compartilham o alinhamento do
panorama das três salas. Essa composição não é aplicada à sala de intervenção.

## Plataformas de tronco — Affordance, primeira sala

Paleta informada pelo autor:

| Elemento | Hex | RGB |
| --- | --- | --- |
| Base do tronco | `#664F37` | 102, 79, 55 |
| Musgo | `#4C5F33` | 76, 95, 51 |
| Sombra da madeira | `#44362D` | 68, 54, 45 |
| Contorno preto | `#000000` | 0, 0, 0 |

Asset: `assets/mg/platformer_wood.png`. Documento no Affinity com largura e altura
4 vezes maiores que a plataforma: 640 × 128 px para uma plataforma de 160 × 32 px.
No jogo, a imagem inteira é reduzida para o retângulo do objeto no Tiled,
preservando suas cores e transparência.

Aplicação apenas em `Content/AffordanceVisibilidade.tmx`, nos objetos da camada
`mapaGeral` nomeados `wood` seguido de número (atualmente `wood1` a `wood4`).
A arte substitui o retângulo visual de blockout; a colisão mantém as medidas do mapa.

> A seção acima é histórica: as plataformas de tronco eram do mapa TMX antigo da Affordance.

## Cores desenhadas por código (novembro/2026)

| Uso | Hex | RGB | Onde |
| --- | --- | --- | --- |
| Fundo padrão da tela (áreas sem arte) | `#15191F` | 21, 25, 31 | `App/GameViewport.cs`: limpeza da tela; aparece fora da sala durante o zoom do Mort. |
| Fumaça da espada da intervenção | `#D22D2D` | 210, 45, 45 | `Gameplay/InterventionTrigger.cs` |
| Faíscas da rocha impostora (Animar) | `#E63228` | 230, 50, 40 | `World/LevelMap.Ldtk.cs` |
| Tom do card "Continuar testando soluções" | `#FF968C` | 255, 150, 140 | `UI/InterventionChoiceMenu.cs` (multiplica a arte do card) |
| Luz da sala de intervenção | `#2850AA` | 40, 80, 170 | `World/LevelMap.Ldtk.cs`: brilho que sobe do chão, até 28% de opacidade |
