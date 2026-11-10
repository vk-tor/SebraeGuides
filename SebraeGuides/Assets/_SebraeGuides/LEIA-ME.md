# Sebraelab 360 – Visita Guiada (Pasta `_SebraeGuides`)

Esta pasta reúne todos os scripts, modelos 3D, materiais URP, áudios de voz e dados da visita guiada do **Sebraelab 360**. Os arquivos originais do template (`SampleScene`, `VRTemplateAssets`, `Samples`) não são alterados.

## 1. Antes de abrir o Unity

1. Feche o Unity se estiver aberto.
2. Atualize o projeto com o GitHub (`git pull`).
3. Abra o projeto no Unity Hub (Unity 6 `6000.0.83f1`) e aguarde a compilação terminar.

## 2. Montar a visita (um clique)

No menu superior do Unity: **Sebrae Guides > 1. Montar a visita (cria a cena Visita)** e clique em **"Montar"**.

O que é configurado automaticamente:

- **Modelo do Sebraelab (`Sebrae Lab.fbx`):** colisores ativados (para o teleporte funcionar no piso e respeitar paredes) e UVs de lightmap geradas;
- **Cena `Assets/Scenes/Visita.unity`:** criada/atualizada a partir da `SampleScene`, desligando objetos de exemplo do template;
- **Guia 3D (`Prof. Otávio`):** instancia o modelo 3D Chibi riggado (`Assets/_SebraeGuides/NPC/ProfessorOtavio_Mixamo.fbx`), aplica os 18 materiais URP, configura o `AnimatorController` (`ProfessorOtavio.controller`) e ativa a animação do esqueleto `mixamorig:` (estados: *Idle respirando*, *Acenando nas boas-vindas/despedida*, *Gesticulando sincronizado com a voz* e *Caminhando nas trocas de parada*);
- **21 Áudios de Voz (`Assets/_SebraeGuides/Audio/Vozes/*.mp3`):** vinculados automaticamente às 21 falas do `RoteiroGuia.asset` (`R1` a `FIM`) com áudio espacial 3D (`Spatial Blend = 1`);
- **3 Slides da TV da Arena (`Assets/_SebraeGuides/TV/tv_slide*.png`):** configurados como `Sprite` e conectados ao componente `TelaTV` (`I3`);
- **5 POIs + 6 Pontos de Teleporte (`A0` a `A4b`) + Área Livre de Piso + 5 Interações (`I1` a `I5`).**

## 3. Como Testar

- **Com o Meta Quest 2:** conecte via Meta Quest Link (cabo ou Air Link) e aperte **Play** no Unity, ou gere o APK em *File > Build Profiles > Build*.
- **No PC (sem óculos):** a cena já conta com suporte ao **XR Device Simulator** (mouse e teclado simulam o headset e os controles).

| Ação | Controle no Quest 2 |
|---|---|
| Teleportar | Analógico para a frente, mire no círculo ou no piso e solte |
| Virar (Snap Turn 45°) | Analógico para os lados |
| Apertar botão de painel / balão | Aponte o raio e aperte o **gatilho (Trigger)** |
| Pegar crachá / post-it / maçaneta | Aponte e segure o **botão lateral (Grip)** |

## 4. Roteiro Completo da Visita (Loop 5/5)

| Parada | Ponto de Teleporte | Guia | Interação que libera a próxima etapa |
|---|---|---|---|
| **1. Recepção (Início)** | `A0` (-59.8, -60.7) | `N0` | Ouvir `R1..R4` (**I1**) e pegar o crachá no balcão (**I2**) |
| **2. Arena de Eventos** | `A1` (-65.6, -59.8) | `N1` | Apertar o botão **"Ligar a TV"** no painel (**I3**) |
| **3. Coworking** | `A2` (-68.2, -65.6) | `N2` | Pegar o post-it amarelo na estante e colar no quadro (**I4**) |
| **4. Copa e Convivência** | `A3` (-64.7, -69.2) | `N3` | Ouvir as falas `K1` e `K2` e avançar no diálogo |
| **5. Sala dos Fundos** | `A4` → `A4b` | `N4` | Puxar a maçaneta para abrir a porta de vidro (**I5**) |
| **Retorno à Recepção** | `A0` (-59.8, -60.7) | `N0` | Painel final **5 de 5 espaços** com botões *Recomeçar* e *Encerrar* |

## 5. Como Customizar Áudios ou Animações do Mixamo

- **Trocar gravações de voz:** basta substituir qualquer arquivo `R1.mp3`, `R2.mp3`, etc. dentro de `Assets/_SebraeGuides/Audio/Vozes/` (ou arrastar no Inspector em `Assets/_SebraeGuides/Dados/RoteiroGuia.asset`).
- **Usar animações extras do Mixamo:** o modelo `ProfessorOtavio_Mixamo.fbx` já possui o esqueleto padrão `mixamorig:Hips`. Você pode associar qualquer clipe `.fbx` do Mixamo aos estados `Idle_Tour`, `Talk_Explain`, `Wave_Welcome` e `Walk_Tour` no arquivo `Assets/_SebraeGuides/NPC/ProfessorOtavio.controller`.
