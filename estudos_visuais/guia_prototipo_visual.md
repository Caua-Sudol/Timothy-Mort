# Protótipo visual e shaders

> **Documento histórico.** Guia do protótipo visual de agosto/2026, anterior à arte e ao LDtk atuais. As medidas do jogador e da física mudaram; ver [docs/metricas_mov.md](../docs/metricas_mov.md).

Este documento organiza um protótipo visual isolado, anterior ao layout definitivo
da fase de affordance. Ele serve para testar escala, leitura, integração entre
personagem e cenário, camadas de profundidade e shaders simples. Não define a arte
final do jogo.

## Objetivo do protótipo

Construir um mapa curto e jogável com blockout, mantendo a física atual, para
responder a estas perguntas:

- O jogador com hitbox de 16 x 16 é legível com arte visual maior?
- O personagem parece pertencer ao cenário, ou parece sobreposto a ele?
- As três camadas mantêm a leitura de plataformas, perigos e personagem?
- Uma gelatina com efeito visual é atraente sem esconder sua função mecânica?
- Fundo, primeiro plano e movimento de câmera criam profundidade sem prejudicar
  o platforming?

## Separação entre física, fonte e tamanho desenhado

O tamanho de um arquivo-fonte não determina seu tamanho no mundo do jogo. Um
desenho pode ser produzido em vetor ou em uma tela 4K no Affinity e exportado em
uma resolução adequada; no jogo, ele será desenhado na escala visual escolhida.

A hitbox do jogador permanece em 16 x 16 por enquanto. Quando o sprite visual for
maior, seu ponto de apoio deve continuar no centro inferior da hitbox: os pés ficam
alinhados ao chão, enquanto cabeça, asas, chapéu e efeitos podem ultrapassar os
limites físicos.

| Elemento                  |           Fonte sugerida | Tamanho visual inicial no jogo | Regra de alinhamento                                  |
|---------------------------|-------------------------:|-------------------------------:|-------------------------------------------------------|
| Jogador/coruja            |         96 x 96 ou vetor |                        24 x 24 | centro inferior na hitbox 16 x 16                     |
| Plataforma sólida         |       128 x 128 ou vetor |               bloco de 32 x 32 | topo coincide com a colisão                           |
| Gelatina                  |       128 x 128 ou vetor |               bloco de 32 x 32 | corpo visual pode passar levemente da colisão         |
| Arbusto de primeiro plano |       256 x 192 ou vetor |               cerca de 64 x 48 | pode ocultar parte do corpo, nunca informação crítica |
| Fundo                     | 1920 x 1080, 4K ou vetor |                 ocupa a câmera | sem exigir alinhamento à colisão                      |

Evitar microdetalhes no jogador: uma textura que parece boa em 4K pode desaparecer
quando for exibida a 24 x 24. A silhueta, as massas de cor e o contraste precisam
funcionar antes do detalhe.

## Camadas do mapa

| Camada         | Função                               | Primeiro elemento de teste            | Regra principal                                              |
|----------------|--------------------------------------|---------------------------------------|--------------------------------------------------------------|
| Fundo          | atmosfera, profundidade e composição | céu/ruínas/manchas de vegetação       | menos contraste e menos detalhe que o gameplay               |
| Gameplay       | colisão e leitura de percurso        | jogador, plataforma sólida e gelatina | todas as superfícies relevantes devem ser legíveis           |
| Primeiro plano | profundidade e oclusão controlada    | arbusto/folhagem                      | não esconder pé do jogador, saída, perigo ou gelatina em uso |

O blockout e as hitboxes continuam sendo a fonte de verdade da jogabilidade. A arte
de gameplay acompanha os retângulos existentes; a arte de fundo e primeiro plano
não deve criar colisão acidental.

## Paleta provisória

A coruja deve evitar branco e preto puros. Um marfim acinzentado conserva a leitura
de "coruja da igreja" sem parecer um elemento luminoso desconectado do ambiente.
Marrom e carvão dão materialidade; um acento frio e pouco saturado no chapéu de Mort
pode sugerir mistério sem transformar a cena em uma paleta roxa vibrante.

| Papel                 | Cor sugerida           | Hex       | Uso                                                      |
|-----------------------|------------------------|-----------|----------------------------------------------------------|
| Plumagem clara        | marfim envelhecido     | `#D8D0BB` | áreas claras da coruja; nunca uma tela branca pura       |
| Sombra da plumagem    | bege acinzentado       | `#9B9284` | volumes, asas e transição para o cenário                 |
| Marrom principal      | marrom de casca        | `#604B3D` | manchas, detalhes e materiais naturais                   |
| Contorno escuro       | carvão azulado         | `#25282B` | olhos, separação de silhueta e detalhes mínimos          |
| Chapéu de Mort        | marrom muito escuro    | `#342A2A` | base material e leitura próxima à coruja                 |
| Acento de Mort        | ameixa acinzentada     | `#544653` | costura, forro ou brilho muito contido de mistério/magia |
| Fundo distante        | azul-cinza noturno     | `#252D38` | céu, ruínas e massas de distância                        |
| Fundo médio           | ardósia dessaturada    | `#3E4950` | vegetação/arquitetura sem competir com o jogador         |
| Plataforma sólida     | oliva-pedra            | `#596054` | terreno jogável; valor médio, material estável           |
| Detalhe da plataforma | pedra seca             | `#77715F` | bordas e textura leve de superfície                      |
| Gelatina base         | verde-petróleo apagado | `#4F7775` | distinta, mas sem parecer automaticamente uma recarga    |
| Gelatina iluminada    | água-marinha opaca     | `#75AAA1` | shader, borda ou pulso controlado                        |
| Primeiro plano        | verde-musgo profundo   | `#303B35` | arbustos com valor escuro e baixa saturação              |

Regra de teoria de cor: manter a maior parte da cena fria, escura e dessaturada;
usar o marfim da coruja como contraste de valor moderado, não como luz; reservar a
ameixa de Mort e a água-marinha da gelatina para pontos de interesse. Se o jogador
sumir no fundo, reduzir primeiro o contraste/saturação do fundo antes de aumentar
o branco ou a saturação do personagem.

## Lista de desenhos para a primeira versão

O objetivo é produzir poucos elementos reutilizáveis, não uma cena completa.

- Coruja em repouso, de frente/lado conforme a orientação atual do jogo.
- Variação simples da coruja com o chapéu de Mort.
- Sombra de contato elíptica, suave e separada do sprite do jogador.
- Tile/base de plataforma sólida e uma borda superior.
- Tile/base de gelatina com interior simples e borda reconhecível.
- Um arbusto ou tufo de folhagem de primeiro plano, maior que o jogador.
- Fundo simples: cor/gradiente, uma silhueta distante e uma massa média de ruína ou
  vegetação.
- Opcional: duas partículas pequenas para poeira de pouso ou resposta visual da
  gelatina.

Não desenhar ainda animações completas, tilesets extensos, inimigos, interface final
ou variações decorativas. Primeiro validar escala e composição com uma única peça de
cada família.

## Integração entre personagem e cenário

Uma sensação de personagem "colado" ao cenário normalmente vem da combinação de
escala, contato, paleta, contraste e profundidade — não da falta de detalhe.

- Alinhar visualmente os pés ao topo da colisão.
- Desenhar uma sombra curta de contato sob o jogador; ela é um teste simples e
  importante antes de qualquer iluminação complexa.
- Usar a mesma direção de luz e famílias de sombra no personagem e no terreno.
- Reduzir contraste, nitidez e saturação à medida que os elementos se afastam para
  o fundo.
- Permitir que o arbusto passe parcialmente à frente do jogador, mas preservar os
  pés e a leitura do percurso em trechos de plataforma.
- Fazer poeira, rastro e efeitos da gelatina nascerem da posição real da colisão ou
  do contato, não de coordenadas arbitrárias na tela.

## Shaders: primeiro experimento

O primeiro shader deve ser estritamente visual. Ele não altera hitbox, física ou
leitura da rota.

1. Gelatina: distorção/ondulação interna leve, transparência variável e brilho
   moderado, limitados ao retângulo visual da gelatina.
2. Fundo: parallax ou deslocamento de cor extremamente suave.
3. Primeiro plano: inicialmente apenas sobreposição normal; shader posterior só se
   ele for necessário para atmosfera.

Evitar aplicar o mesmo efeito ao jogador no primeiro teste. Isso facilita descobrir
se o shader melhora a percepção do material ou apenas reduz a legibilidade.

## Leituras recomendadas

- [Basic Illumination for Games — Game Developer](https://www.gamedeveloper.com/art/basic-illumination-for-games-part-1-applying-art-concepts-on-interactive-media): contraste, iluminação e relação entre primeiro plano, meio e fundo.
- [Environment Design — Game Design Decal](https://www.gamedesigndecal.com/pages/labs/lab12/lab12/): exercício de cenário 2D com parallax.
- [Game Environments and Backgrounds — Pixel Bear](https://pixelsbook.com/en/course/25/): introdução a planos, parallax e hierarquia visual.
- [Como usar luz 2D para criar clima — Unity](https://unity.com/how-to/use-2d-lights-unity-set-mood): referência conceitual de direção de arte e iluminação; a implementação não é a mesma do MonoGame.
- [MIO: direção de arte desenhada à mão explicada pelos criadores](https://www.creativebloq.com/3d/video-game-design/mio-memories-in-orbits-stunning-hand-drawn-art-direction-explained-by-its-creators): clareza da geometria jogável e riqueza concentrada nos planos não jogáveis.

## Critério de sucesso

O protótipo está pronto para orientar o layout da affordance quando for possível
jogar o trecho sem confundir a colisão, localizar o jogador em movimento, perceber
as três camadas e identificar se a gelatina é visualmente interessante sem tornar
sua propriedade pedagógica óbvia antes da intervenção.
