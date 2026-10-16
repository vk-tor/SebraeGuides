# Sebraelab 360 – visita guiada (pasta _SebraeGuides)

Esta pasta tem todos os scripts da visita. Os arquivos do template (SampleScene, VRTemplateAssets, Samples) não são alterados.

## 1. Antes de abrir o Unity

1. Feche o Unity.
2. Atualize o projeto com o GitHub (`git pull` ou "Fetch origin" + "Pull origin" no GitHub Desktop).
3. Abra o projeto no Unity Hub e espere a barra de compilação terminar.

Se aparecer erro vermelho no Console (Window > General > Console), pare e mande um print no grupo.

## 2. Montar a visita (um clique)

Menu do topo: **Sebrae Guides > 1. Montar a visita (cria a cena Visita)** e clique em "Montar".

O que acontece:

- o modelo `Sebrae Lab.fbx` passa a ter colisores (o teleporte para no chão e nas paredes);
- é criada a cena `Assets/Scenes/Visita.unity`, cópia da SampleScene;
- os objetos do template que atrapalham (mesa, cubos, área de teleporte de exemplo, cubo do NPC) são desligados, não apagados;
- é criado o objeto **Sebraelab360 (gerado)** com tudo da visita;
- a cena Visita vira a primeira do Build.

Rodar de novo apaga e refaz o "Sebraelab360 (gerado)". O texto das falas é mantido.

## 3. Testar

**Com o Quest 2:** ligue o Meta Quest Link (cabo ou Air Link), abra o Link no óculos e aperte Play no Unity.

**Sem óculos:** Window > Package Manager > XR Interaction Toolkit > aba Samples > importe "XR Interaction Simulator". Arraste o prefab `XR Interaction Simulator` para a cena e aperte Play. O mouse e o teclado fazem o papel dos controles.

Controles no Quest:

| Ação | Como fazer |
|---|---|
| Teleportar | Analógico para a frente, mira no círculo, solta |
| Virar | Analógico para os lados |
| Apertar botão de painel | Apontar e apertar o gatilho |
| Pegar crachá / post-it / maçaneta | Apontar e segurar o botão lateral (grip) |

## 4. Roteiro da visita

| Parada | Interação que libera a próxima |
|---|---|
| A0 Recepção | Pegar o crachá no balcão |
| A1 Arena | Botão "Ligar a TV" no painel |
| A2 Coworking | Levar o post-it da estante até o mural |
| A3 Copa | (só conversa) |
| A4 Sala dos fundos | Puxar a maçaneta da porta de vidro |
| Volta à recepção | Encerramento, com "Recomeçar" ou "Encerrar" |

## 5. Onde mexer

| Quero mudar... | Onde |
|---|---|
| Texto das falas e botões | Menu Sebrae Guides > Editar as falas do guia (arquivo `Dados/RoteiroGuia.asset`) |
| Áudio de uma fala | Mesmo arquivo, campo "Audio" da fala |
| Título e texto de um painel | Hierarchy > Sebraelab360 (gerado) > Pontos de interesse > POI ... (componente Ponto De Interesse) |
| Fotos da TV | Hierarchy > Interacoes > I3 TV - tela, componente Tela TV, lista "Fotos" (importe as imagens como Sprite) |
| Modelo do guia | Hierarchy > Guia > "Modelo (troque pelo modelo final)": coloque o modelo dentro e apague "Corpo" e "Visor". Não coloque XR Interactable no guia: ele rouba o raio dos botões do balão |
| Posição do guia em cada parada | Hierarchy > Posicoes do guia |
| Luzes | Hierarchy > Luzes (desligue se ficar claro demais) |

Textos entre [colchetes] precisam ser completados com o material do professor.

Atenção: se você rodar o item 1 do menu de novo, ajustes feitos à mão dentro de "Sebraelab360 (gerado)" se perdem. As falas (RoteiroGuia.asset) ficam.

## 6. Trabalho em grupo (evitar conflito no Git)

- Só uma pessoa por vez edita a cena `Visita.unity`. Avise no grupo antes de abrir e depois de dar push.
- Falas, áudios, fotos e modelos podem ser feitos em paralelo: são arquivos separados.
- Depois de testar: commit com mensagem clara e push.

## 7. Scripts

| Script | Função |
|---|---|
| ControleVisita | Ordem das paradas, eventos, contador "Espaços visitados", fim da visita |
| DialogoGuia | Balão de fala com botões |
| RoteiroGuia | Arquivo com as falas (ScriptableObject) |
| GuiaNPC | Guia: troca de posição e olha para o visitante |
| PontoDeInteresse | Detecta o visitante e mostra o painel do espaço |
| GatilhoInteracao | Avisa a visita quando algo é pego, apertado ou encaixado |
| TelaTV | TV da arena com fotos e legendas |
| PortaDeslizante | Abre a porta de vidro da sala dos fundos |
| SocketMuralIdeias + PostIt | Mural que só aceita o post-it |
| MarcadorProximaParada | Placa "Próxima parada" sobre o próximo teleporte |
| ViradoParaCamera | Mantém painéis virados para o visitante |
| Editor/MontarVisita | O menu "Sebrae Guides" |
| Editor/RoteiroPadrao | Texto inicial das falas |

## 8. Problemas comuns

- **O menu "Sebrae Guides" não aparece:** tem erro de compilação. Abra o Console e veja a primeira linha vermelha.
- **O teleporte não acontece:** confira se a cena aberta é a Visita (o nome aparece na aba Hierarchy). Dá para teleportar para qualquer ponto do piso; os círculos das próximas paradas só aparecem quando o guia manda ir para lá, com a placa "Próxima parada" em cima.
- **Caí através do chão:** rode o item 1 de novo (ele liga os colisores do modelo).
- **Os textos aparecem como quadrados:** Window > TextMeshPro > Import TMP Essential Resources.
