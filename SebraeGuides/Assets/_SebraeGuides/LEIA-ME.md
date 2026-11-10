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
- o XR Device Simulator é tirado da cena (veja "Sem óculos" abaixo);
- o Sebraelab é marcado como Static (menos peso no Quest) e a cena usa a luz dela mesma, não a da SampleScene. Na primeira vez a luz é calculada (pasta `Scenes/Visita`, vai no commit);
- a cena Visita vira a primeira do Build.

Rodar de novo apaga e refaz o "Sebraelab360 (gerado)". O texto das falas é mantido.

**Uma vez por projeto (já foi feito em 08/11):** Sebrae Guides > **4. Preparar o projeto para o Quest**. Ajusta o nome do app (`com.SebraeGuides.Sebraelab360`), liga as 4 luzes da visita também no Quest e deixa o Play com o Link mais leve (Single Pass Instanced). Não mexe na cena.

## 3. Testar

**Com o Quest 2:** ligue o Meta Quest Link (cabo ou Air Link), abra o Link no óculos e aperte Play no Unity. No Console aparece "[Sebrae Guides] Óculos conectado": o simulador não é criado. Com o óculos, a janela Game do PC mostra a **câmera do projetor**, estável (F1 primeira pessoa suave, F2 terceira pessoa, F3 câmera fixa na recepção, F4 liga/desliga).

**Sem óculos:** só aperte Play. O XR Device Simulator aparece sozinho (opção *Use XR Device Simulator in scenes*, em Project Settings > XR Plug-in Management > XR Interaction Toolkit). O mouse e o teclado fazem o papel dos controles. **Não arraste o simulador para a cena:** salvo nela, ele tira o óculos de verdade do sistema e a imagem para de acompanhar a cabeça no Quest. Se alguém puser, o item 1 do menu tira de novo, e no build ele é removido sozinho.

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
| A0 Recepção | Pegar o crachá de pé no suporte do balcão (ele cresce quando o raio aponta e some quando é solto) |
| A1 Arena | Botão "Ligar a TV" no painel |
| A2 Coworking | Levar o post-it da estante até o mural |
| A3 Copa | (só conversa) |
| A4 Sala dos fundos | Puxar a maçaneta da porta de vidro |
| Volta à recepção | Encerramento, com "Recomeçar" ou "Encerrar" |

## 5. Onde mexer

| Quero mudar... | Onde |
|---|---|
| Texto das falas e botões | Menu Sebrae Guides > Editar as falas do guia (arquivo `Dados/RoteiroGuia.asset`) |
| Áudio de uma fala | Pasta `Audio/Falas`: um arquivo por fala com o ID no nome (`R1.ogg`, `A1.ogg`...). Para trocar, substitua o arquivo e rode o item 1 do menu, que liga cada áudio à sua fala. Vozes atuais: ElevenLabs, voz "Moutella", plano gratuito (crédito obrigatório: "Vozes geradas com ElevenLabs (elevenlabs.io)"). Falta a F2 |
| Título e texto de um painel | Hierarchy > Sebraelab360 (gerado) > Pontos de interesse > POI ... (componente Ponto De Interesse) |
| Fotos da TV | Pasta `Fotos`: as imagens entram em ordem de nome (TV_01, TV_02...) e as legendas ficam em `Fotos/legendas.txt` ("legenda \| crédito", uma linha por foto). Depois de trocar, rode o item 1 do menu. Fotos atuais: Google Maps (contribuições de visitantes) |
| Modelo do guia | O guia é o Professor Otávio (`Modelos/ProfessorOtavio_Guia.fbx`, 42 mil triângulos, 4 animações). Para trocar, substitua esse FBX por outro com o mesmo nome e rode o item 1 do menu. Não coloque XR Interactable nem colisor no guia: ele rouba o raio dos botões do balão |
| Animação do guia | Componente Animacao Guia no objeto Guia: falas em que ele acena e quanto tempo gesticula por fala. Os estados ficam em `Modelos/Guia.controller` (refeito pelo item 1) |
| Posição do guia em cada parada | Hierarchy > Posicoes do guia |
| Luzes | Hierarchy > Luzes (desligue se ficar claro demais). São 4 luzes em tempo real, acesas no PC e no Quest (até 4 por objeto). Se o Quest não segurar 72 FPS, troque `LuzesPorObjeto` para 2 em `Editor/PrepararQuest.cs` e rode o item 4. O sol (Lighting > Directional Light) fica sem sombra: a sombra do teto só aparecia perto do visitante |
| Luz calculada (bake) | Pasta `Scenes/Visita`. Para refazer: Sebrae Guides > Refazer a luz da Visita (bake) |
| Câmera do projetor | Hierarchy > Sebraelab360 (gerado) > Camera do projetor. Só liga com o óculos no Link |

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
| AnimacaoGuia | Guia gesticula enquanto fala e acena na chegada e na despedida |
| DestaqueAoApontar | Objeto cresce e o controle vibra quando o raio aponta para ele (crachá) |
| GuardarAoSoltar | Objeto encolhe e some quando é solto (crachá) |
| FisicaAoSoltar | Objeto fica parado até ser pego e cai normalmente depois (post-it) |
| PontoDeInteresse | Detecta o visitante e mostra o painel do espaço |
| GatilhoInteracao | Avisa a visita quando algo é pego, apertado ou encaixado |
| TelaTV | TV da arena com fotos e legendas |
| PortaDeslizante | Abre a porta de vidro da sala dos fundos |
| SocketMuralIdeias + PostIt | Mural que só aceita o post-it |
| MarcadorProximaParada | Placa "Próxima parada" sobre o próximo teleporte |
| ViradoParaCamera | Mantém painéis virados para o visitante |
| SimuladorSoSemOculos | Com o óculos conectado, não deixa o XR Device Simulator ser criado (ele quebraria o rastreamento da cabeça) |
| CameraEspectador | Câmera estável para o projetor no Play com o Link |
| Editor/MontarVisita | O menu "Sebrae Guides" |
| Editor/PrepararQuest | Item 4 do menu: nome do app, luzes do URP e Single Pass no Windows |
| Editor/RemoverSimuladorDoBuild | Tira o simulador das cenas no build (ele nunca vai para o APK) |
| Editor/RoteiroPadrao | Texto inicial das falas |

## 8. Problemas comuns

- **O menu "Sebrae Guides" não aparece:** tem erro de compilação. Abra o Console e veja a primeira linha vermelha.
- **O teleporte não acontece:** confira se a cena aberta é a Visita (o nome aparece na aba Hierarchy). Dá para teleportar para qualquer ponto do piso; os círculos das próximas paradas só aparecem quando o guia manda ir para lá, com a placa "Próxima parada" em cima.
- **Caí através do chão:** rode o item 1 de novo (ele liga os colisores do modelo).
- **Com o Quest no Link, a imagem não acompanha a cabeça:** tem um XR Device Simulator salvo na cena. Rode o item 1 (ele tira) e salve.
- **Antes do primeiro APK:** Project Settings > XR Plug-in Management > Project Validation, aba Android, e "Fix All" no que aparecer.
- **Os textos aparecem como quadrados:** Window > TextMeshPro > Import TMP Essential Resources.
