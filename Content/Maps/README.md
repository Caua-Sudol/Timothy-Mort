# Affordance — projeto LDtk

Abra `Content/Maps/Affordance.ldtk`. Este projeto controla as tres telas e a intervencao. O arquivo de teste continua separado.

## Salas e destinos

| Nivel no LDtk | RoomExit | RoomBack |
| --- | --- | --- |
| AffordanceTela1 | Tela 2, passagem livre | — |
| AffordanceTela2 | Tela 3, passagem livre | Tela 1 |
| AffordanceTela3 | Conclui a fase depois de aplicar uma solucao | Tela 2 |
| Sala_intervencao | Retorna a tela de onde entrou, mesmo sem escolher | — |

Mantenha esses identificadores: o jogo seleciona cada nivel pelo nome, independentemente da ordem e da posicao no World. Todas as coordenadas de jogo sao locais a sala.

## Camadas

- Salas: 2880 x 1600; terreno em grid de 96.
- Colisao: solido (1) bloqueia; gelatina (2) permite atravessar, limita queda e recarrega dash; espinhos (3) causam dano.
- Correcoes: tiles manuais sobre a arte automatica, sem criar colisao.
- Entidades: grid de 16 para ajustar os marcadores.
- Atlas: ../../assets/teste_ldtk/root_tileset.png.

## Entidades

| Entidade | Uso |
| --- | --- |
| PlayerSpawn | Inicio e checkpoint da sala, com pivo nos pes. |
| RoomExit | Area de contato para avancar ou sair da intervencao. |
| RoomBack | Area de contato para voltar a tela anterior. |
| RoomReturn | Posicao dos pes ao voltar da proxima tela. Mantenha fora de RoomExit. |
| InterventionEntrance | Acesso com E: aparece apos duas mortes na tela 1 e fica disponivel na entrada da tela 2. |
| InterventionReturn | Posicao dos pes ao voltar da intervencao. |
| InterventionSword | Retangulo interativo com E que abre a escolha da solucao. |

Spawns e retornos devem ficar com os pes no topo de um solido, fora dos espinhos e das areas de passagem automatica. As areas podem ser redimensionadas no editor. O retorno da intervencao e direcionado automaticamente para a sala de origem.

Salve no LDtk e use Ctrl+R na sala atual para recarregar e voltar ao spawn, ou recompile o jogo. Erros de recarga preservam o mapa anterior. Alteracoes em PNGs exigem recompilar o conteudo.

Todas estas salas usam escala 1 do jogador e referencia de 14% da altura da tela. O shader experimental continua exclusivo da sala de teste.

Depois desta alteracao externa, abra Affordance.ldtk no editor antes de editar; nao salve uma copia antiga ainda aberta por cima dele.
