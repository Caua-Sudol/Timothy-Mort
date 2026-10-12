# Primeira tela do affordance — LDtk

Abra `AffordanceTela1.ldtk` como projeto separado no LDtk. Esta copia e a fonte da primeira tela; o projeto em assets/teste_ldtk continua sendo o teste.

- Sala: 2880 x 1600. Terreno: grid de 96.
- Colisao: valor solido (1) bloqueia; gelatina (2) permite atravessar, limita queda e recarrega dash.
- Correcoes: tiles manuais sobre a arte automatica. Nao cria colisao.
- Entidades: grid de 16 para posicionar os marcadores com mais precisao.

## Marcadores

| Entidade | Uso |
| --- | --- |
| PlayerSpawn | Inicio e checkpoint da sala. Pivo no centro dos pes. |
| InterventionEntrance | Area de acesso com E, revelada depois de duas mortes. Leva a intervencao existente. |
| InterventionReturn | Posicao dos pes ao voltar da intervencao. |
| RoomExit | Area de contato para ir a affordance_tela_2, sem exigir solucao. |
| RoomReturn | Posicao dos pes ao voltar da tela 2. Mantenha fora de RoomExit. |

As areas de entrada/saida podem ser redimensionadas no editor. Os destinos desta primeira sala estao ligados pelo tipo da entidade.

Salve no LDtk e pressione Ctrl+R no jogo, na primeira tela, para recarregar o arquivo e voltar ao spawn. Arquivo invalido mostra erro e preserva o mapa carregado. Alteracoes nos PNGs exigem recompilar o conteudo.

O atlas referencia ../../assets/teste_ldtk/root_tileset.png, dentro deste projeto. As regras de gelatina usam o mesmo atlas dos solidos.

O jogador desta sala usa escala 1 e referencia de 14% da altura da tela. O shader experimental fica na sala de teste. As demais salas continuam nos arquivos TMX.

Ao editar o LDtk externamente, reabra o projeto no editor antes de salvar para evitar sobrescrever a versao nova com a antiga.
