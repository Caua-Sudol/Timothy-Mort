# Affordance (LDTK)

Abra `Content/Maps/Affordance.ldtk`. Este projeto controla as tres telas e a intervencao. O arquivo de teste continua separado.

## Salas e destinos

| Nivel no LDtk    | RoomExit                                          | RoomBack |
|------------------|---------------------------------------------------|----------|
| AffordanceTela1  | Tela 2, passagem livre                            | —        |
| AffordanceTela2  | Tela 3, passagem livre                            | Tela 1   |
| AffordanceTela3  | Conclui a fase depois de aplicar uma solucao      | Tela 2   |
| Sala_intervencao | Retorna a tela de onde entrou, mesmo sem escolher | —        |

Mantenha esses identificadores: o jogo seleciona cada nivel pelo nome, independentemente da ordem e da posicao no World. Todas as coordenadas de jogo sao locais a sala.

## Camadas

- Salas: 2880 x 1600; terreno em grid de 96.
- Colisao: solido (1) bloqueia; gelatina (2) permite atravessar, limita queda e recarrega dash; espinhos (3) causam dano.
- Correcoes: tiles manuais sobre a arte automatica, sem criar colisao.
- Entidades: grid de 16 para ajustar os marcadores.
- Tilesets: cada camada usa o PNG indicado no LDtk, relativo ao arquivo .ldtk. Mantenha as imagens dentro de assets; o build copia os PNGs para a distribuicao automaticamente.

## Aparencia por solucao

As escolhas do affordance usam um piscar por codigo: 0,26 s para fechar, 0,07 s fechado e 0,32 s para abrir (0,65 s no total). A escolha e aplicada uma unica vez no fechamento completo. A demonstracao de Mort na tela 3 usa o mesmo piscar, depois da transicao entre salas, e registra a exposicao quando aplica a combinacao com o olho fechado. Voltar a essa tela conserva a combinacao sem repetir a demonstracao. Durante o piscar, o movimento e novas interacoes ficam suspensos.

Entrada e saida da intervencao usam simbolos do atlas da tela inicial flutuando dentro dos retangulos definidos no LDtk. O alcance de E acrescenta 64 unidades horizontais e 24 verticais ao redor do jogador, sem alterar a area visual ou as colisoes do terreno. A saida da intervencao exige E; RoomExit/RoomBack entre as telas de desafio continuam por contato. Mort explica os simbolos na primeira vez que uma entrada fica disponivel na execucao da fase.

- `Solucao_gelatina`: aparece somente com a solucao Aparencia.
- `Solucao_mark`: aparece somente com a solucao Marcacao.
- Sem escolha ou com Deformacao, ambas ficam ocultas; Deformacao continua pulsando a terra comum.
- Os nomes sao reconhecidos sem distinguir maiusculas e minusculas. A visibilidade dessas duas camadas no editor nao controla sua ativacao no jogo.
- Pinte as variantes completas em tiles de 96x96 alinhados aos tiles originais, acima do terreno. Nas celulas de gelatina cobertas pela solucao ativa, os tiles inferiores coincidentes sao ocultados para preservar a transparencia do novo asset. Celulas sem variante conservam a terra original.
- A fisica continua definida por `Colisao`; trocar a solucao altera apenas o visual. A cor azul e a marca por codigo nao sao mais usadas nos mapas LDtk.

## Cenario da Sala_intervencao

A arte da intervencao nao usa tiles: o jogo desenha cinco imagens 1920x1080 de `assets/eureka` cobrindo a sala inteira (escala 1,5, recorte centralizado, igual ao modo "Cover" do LDtk). Ordem, de tras para frente:

1. `Intervencao_bg_geral.png`
2. `fumaca_frente.png`
3. `Intervencao_plataformas_fundo.png`
4. jogador, espada, portas e tiles do LDtk
5. `Intervencao_plataformas_fg.png`
6. `fg_fumaca_frente.png`

A colisao continua vindo so da camada `Colisao`. Para alinhar: nas propriedades do nivel `Sala_intervencao`, use `Intervencao_plataformas_fundo.png` como imagem de fundo do nivel com o modo "Cover"; o jogo ignora esse fundo, ele serve so de guia no editor. Tiles antigos de terreno nessa sala continuam sendo desenhados por cima da arte; apague-os ou oculte a camada no editor. As camadas de solucao (`Solucao_gelatina`, `Solucao_mark`) continuam ativas mesmo ocultas.

Trocar o PNG exige reiniciar a fase: o Ctrl+R recarrega mapa e tilesets, mas mantem essas cinco imagens.

## Entidades

| Entidade             | Uso                                                                                      |
|----------------------|------------------------------------------------------------------------------------------|
| PlayerSpawn          | Inicio e checkpoint da sala, com pivo nos pes.                                           |
| RoomExit             | Area de contato para avancar ou sair da intervencao.                                     |
| RoomBack             | Area de contato para voltar a tela anterior.                                             |
| RoomReturn           | Posicao dos pes ao voltar da proxima tela. Mantenha fora de RoomExit.                    |
| InterventionEntrance | Acesso com E: aparece apos duas mortes na tela 1 e fica disponivel na entrada da tela 2. |
| InterventionReturn   | Posicao dos pes ao voltar da intervencao.                                                |
| InterventionSword    | Retangulo interativo com E que abre a escolha da solucao.                                |

Spawns e retornos devem ficar com os pes no topo de um solido, fora dos espinhos e das areas de passagem automatica. As areas podem ser redimensionadas no editor. O retorno da intervencao e direcionado automaticamente para a sala de origem.

Salve no LDtk e use Ctrl+R na sala atual para recarregar o mapa e seus PNGs e voltar ao spawn, ou recompile o jogo. Nao e preciso cadastrar novos tilesets no Content.mgcb. Erros de recarga (incluindo imagens ausentes ou recortes invalidos) preservam o mapa anterior.

Todas estas salas usam escala 1 do jogador e referencia de 14% da altura da tela. O shader experimental continua exclusivo da sala de teste.

Depois desta alteracao externa, abra Affordance.ldtk no editor antes de editar; nao salve uma copia antiga ainda aberta por cima dele.
