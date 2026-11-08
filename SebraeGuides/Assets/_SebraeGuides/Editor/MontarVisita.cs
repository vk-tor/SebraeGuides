using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SebraeGuides.Ferramentas
{
    /// <summary>
    /// Menu "Sebrae Guides" do Unity. Monta a visita inteira na cena Visita.unity:
    /// guia 3D (Prof. Otávio), pontos de interesse com painéis, pontos de teleporte,
    /// as 5 interações, os 21 áudios de voz, os slides da TV e o loop completo.
    /// </summary>
    public static class MontarVisita
    {
        // ------------------------------------------------------------------ caminhos
        const string CenaOrigem = "Assets/Scenes/SampleScene.unity";
        const string CenaVisita = "Assets/Scenes/Visita.unity";
        const string Pasta = "Assets/_SebraeGuides";
        const string PastaDados = Pasta + "/Dados";
        const string PastaMateriais = Pasta + "/Materiais";
        const string PastaNPC = Pasta + "/NPC";
        const string PastaVozes = Pasta + "/Audio/Vozes";
        const string PastaTV = Pasta + "/TV";
        const string CaminhoRoteiro = PastaDados + "/RoteiroGuia.asset";
        const string CaminhoModelo = "Assets/Sebraelab3D/Sebrae Lab.fbx";
        const string CaminhoGuiaFBX = PastaNPC + "/ProfessorOtavio_Mixamo.fbx";
        const string CaminhoControllerGuia = PastaNPC + "/ProfessorOtavio.controller";
        const string CaminhoAncora = "Assets/VRTemplateAssets/Prefabs/Teleport/Teleport Anchor.prefab";
        const string CaminhoClique = "Assets/VRTemplateAssets/Audio/Button_22_click.wav";
        const string NomeRaiz = "Sebraelab360 (gerado)";
        const string NomeModeloNaCena = "Sebrae Lab";
        const float Piso = -0.03f;
        const float AlturaPainel = 1.55f;

        // ------------------------------------------------------------------ cores
        static readonly Color CorFundo = new Color(0.07f, 0.11f, 0.19f, 0.94f);
        static readonly Color CorDestaque = new Color(0.08f, 0.72f, 0.65f, 1f);
        static readonly Color CorBotaoAceso = new Color(0.12f, 0.72f, 0.68f, 1f);
        static readonly Color CorTextoClaro = new Color(0.95f, 0.97f, 1f, 1f);
        static readonly Color CorTextoSuave = new Color(0.74f, 0.83f, 0.95f, 1f);
        static readonly Color CorDica = new Color(1f, 0.86f, 0.45f, 1f);

        // Objetos da raiz da cena que vêm do template (ou são provisórios) e atrapalham a visita.
        static readonly string[] RaizesParaDesligar =
            { "Environment", "Interactables", "UI", "Teleport Area Setup", "UI Sample", "Cube", "posicoes_npc" };

        // ------------------------------------------------------------------ dados das paradas
        struct DadosParada
        {
            public string codigo, chave, titulo, resumo, detalhes;
            public string falaAoChegar, evento, falaAoConcluir, rotuloBotao;
            public Vector2 ancora;   // X, Z do ponto de teleporte
            public float yawAncora;  // para onde o visitante fica virado ao chegar
            public Vector2 guia;     // X, Z do guia
            public bool balaoAEsquerda; // parede à direita do guia
            public Vector2 painel;   // X, Z do painel
            public float raio;
        }

        static DadosParada[] Paradas()
        {
            return new[]
            {
                new DadosParada
                {
                    codigo = "A0", chave = "recepcao", titulo = "Bem-vindo ao Sebraelab",
                    resumo = "O hub de inovação aberta do Sebrae, para conectar empreendedores, startups, universidades e parceiros.",
                    detalhes = "Nesta visita guiada pelo Prof. Otávio você passa pela arena de eventos, pelo coworking, pela copa e pela sala dos fundos.",
                    falaAoChegar = "", evento = "pegou_cracha", falaAoConcluir = "R5",
                    ancora = new Vector2(-59.8f, -60.7f), yawAncora = 162.8f,
                    guia = new Vector2(-58.9f, -63.6f), painel = new Vector2(-60.58f, -62.54f), raio = 2f
                },
                new DadosParada
                {
                    codigo = "A1", chave = "arena", titulo = "Arena de eventos",
                    resumo = "Espaço aberto com arquibancada para palestras, oficinas práticas, maratonas de inovação e apresentações de projetos.",
                    detalhes = "A Arena do Sebraelab recebe demodays, hackathons, painéis com especialistas e encontros gratuitos que conectam empreendedores e estudantes.",
                    falaAoChegar = "A1", evento = "tv_ligada", falaAoConcluir = "A3", rotuloBotao = "Ligar a TV",
                    ancora = new Vector2(-65.6f, -59.8f), yawAncora = -5f,
                    guia = new Vector2(-64.1f, -58.0f), painel = new Vector2(-65.77f, -57.81f), raio = 2.5f
                },
                new DadosParada
                {
                    codigo = "A2", chave = "coworking", titulo = "Coworking",
                    resumo = "Estações compartilhadas para trabalhar, trocar ideias e fazer contatos.",
                    detalhes = "As etapas do design thinking aplicadas na prática: imersão, ideação, prototipação e implementação (Sebrae, 2015).",
                    falaAoChegar = "C1", evento = "postit_colado", falaAoConcluir = "C4",
                    ancora = new Vector2(-68.2f, -65.6f), yawAncora = 180f,
                    guia = new Vector2(-69.7f, -66.0f), painel = new Vector2(-66.72f, -67.08f), raio = 2f,
                    balaoAEsquerda = true
                },
                new DadosParada
                {
                    codigo = "A3", chave = "copa", titulo = "Copa e convivência",
                    resumo = "Um espaço acolhedor para pausas, conversas informais, conexões e reuniões rápidas.",
                    detalhes = "Na área de convivência e nas mesas laterais acontecem mentorias rápidas, trocas de experiências e conexões que geram novas parcerias.",
                    falaAoChegar = "K1", evento = "", falaAoConcluir = "",
                    ancora = new Vector2(-64.7f, -69.2f), yawAncora = -70f,
                    guia = new Vector2(-66.4f, -70.2f), painel = new Vector2(-66.58f, -68.52f), raio = 2f
                },
                new DadosParada
                {
                    codigo = "A4", chave = "fundos", titulo = "Sala dos fundos",
                    resumo = "Sala reservada para reuniões estratégicas, oficinas focadas e atendimento especializado a empreendedores.",
                    detalhes = "Um ambiente tranquilo nos fundos do Sebraelab, ideal para validação de modelos de negócio e mentorias em pequenos grupos.",
                    falaAoChegar = "F1", evento = "porta_aberta", falaAoConcluir = "F2",
                    ancora = new Vector2(-62.9f, -75.3f), yawAncora = 175f,
                    guia = new Vector2(-64.05f, -76.4f), painel = new Vector2(-61.6f, -76.2f), raio = 2f,
                    balaoAEsquerda = true
                },
            };
        }

        // ------------------------------------------------------------------ menus
        [MenuItem("Sebrae Guides/1. Montar a visita (cria a cena Visita)", false, 1)]
        public static void Montar()
        {
            if (!EditorUtility.DisplayDialog("Sebrae Guides - Montar a visita",
                    "Isto vai:\n\n" +
                    "• ligar os colisores do modelo do Sebraelab;\n" +
                    "• criar (ou atualizar) a cena Assets/Scenes/Visita.unity, copiada da SampleScene;\n" +
                    "• montar o Prof. Otávio (NPC 3D), os 5 pontos de interesse, o teleporte, os áudios e as 5 interações.\n\n" +
                    "A SampleScene não é alterada. Rodar de novo apaga e refaz a montagem.",
                    "Montar", "Cancelar"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            MontarInterno(true);
        }

        /// <summary>Execução direta via linha de comando (-executeMethod SebraeGuides.Ferramentas.MontarVisita.ExecutarViaCLI).</summary>
        public static void ExecutarViaCLI()
        {
            Debug.Log("[Sebrae Guides CLI] Iniciando montagem automática e validação da visita...");
            AjustarAppInterno();
            MontarInterno(false);
            ValidarCenaVisita();
            AssetDatabase.SaveAssets();
            RenderizarPreviewsDaCena();
            Debug.Log("[Sebrae Guides CLI] Montagem e validação concluídas com sucesso!");
        }

        static void MontarInterno(bool interativo)
        {
            ControleVisita controle;
            try
            {
                if (interativo) EditorUtility.DisplayProgressBar("Sebrae Guides", "Ajustando modelos e sprites...", 0.1f);
                ConfigurarModelo();
                ConfigurarGuia3D();
                ConfigurarSpritesTV();

                if (interativo) EditorUtility.DisplayProgressBar("Sebrae Guides", "Abrindo a cena Visita...", 0.3f);
                var cena = AbrirCenaVisita();
                if (!cena.IsValid()) return;

                GarantirPasta(PastaDados);
                GarantirPasta(PastaMateriais);
                var roteiro = CarregarOuCriarRoteiro(false);
                VincularAudiosAoRoteiro(roteiro);

                if (interativo) EditorUtility.DisplayProgressBar("Sebrae Guides", "Desligando objetos do template...", 0.45f);
                var antiga = BuscarRaiz(cena, NomeRaiz);
                if (antiga) Object.DestroyImmediate(antiga);
                DesligarSobras(cena);

                var modelo = BuscarRaiz(cena, NomeModeloNaCena);
                if (!modelo)
                {
                    Debug.LogError("[Sebrae Guides] Não encontrei o objeto 'Sebrae Lab' na raiz da cena.");
                    if (interativo)
                        EditorUtility.DisplayDialog("Sebrae Guides",
                            "Não encontrei o objeto 'Sebrae Lab' na raiz da cena. Confira se o modelo está na SampleScene.", "OK");
                    return;
                }

                if (interativo) EditorUtility.DisplayProgressBar("Sebrae Guides", "Montando Prof. Otávio, pontos e interações...", 0.6f);
                var raiz = new GameObject(NomeRaiz);
                controle = Construir(raiz.transform, modelo.transform, roteiro);

                if (interativo) EditorUtility.DisplayProgressBar("Sebrae Guides", "Posicionando o visitante...", 0.85f);
                AjustarXROrigin();
                ConfigurarBuild();

                EditorSceneManager.MarkSceneDirty(cena);
                EditorSceneManager.SaveScene(cena);
                if (interativo && controle)
                {
                    Selection.activeGameObject = controle.gameObject;
                    EditorGUIUtility.PingObject(controle.gameObject);
                }
            }
            finally
            {
                if (interativo) EditorUtility.ClearProgressBar();
            }

            if (interativo)
            {
                EditorUtility.DisplayDialog("Sebrae Guides",
                    "Pronto! A cena Visita foi salva com o Prof. Otávio 3D, animações, 21 áudios e 5 interações.\n\n" +
                    "Para testar: conecte o Quest pelo Meta Quest Link (ou use o XR Device Simulator) e aperte Play.", "OK");
            }
        }

        [MenuItem("Sebrae Guides/2. Abrir a cena Visita", false, 2)]
        static void AbrirVisita()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CenaVisita) == null)
            {
                EditorUtility.DisplayDialog("Sebrae Guides", "A cena Visita ainda não existe. Use o item 1 do menu.", "OK");
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(CenaVisita, OpenSceneMode.Single);
        }

        [MenuItem("Sebrae Guides/3. Ajustar o nome do app no Quest", false, 3)]
        static void AjustarApp()
        {
            AjustarAppInterno();
            EditorUtility.DisplayDialog("Sebrae Guides",
                "Nome do app: Sebraelab 360\nPacote Android: com.SebraeGuides.Sebraelab360", "OK");
        }

        static void AjustarAppInterno()
        {
            PlayerSettings.companyName = "Sebrae Guides";
            PlayerSettings.productName = "Sebraelab 360";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.SebraeGuides.Sebraelab360");
        }

        [MenuItem("Sebrae Guides/4. Validar a cena Visita", false, 4)]
        public static void ValidarCenaVisita()
        {
            var controle = Object.FindFirstObjectByType<ControleVisita>();
            if (!controle)
            {
                Debug.LogError("[VALIDACAO] ERRO: ControleVisita não encontrado na cena atual!");
                return;
            }

            int audiosOk = 0;
            if (controle.dialogo && controle.dialogo.roteiro)
            {
                foreach (var f in controle.dialogo.roteiro.falas)
                {
                    if (f != null && f.audio != null) audiosOk++;
                }
            }

            int ossosMixamo = 0;
            if (controle.guia)
            {
                foreach (var t in controle.guia.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.StartsWith("mixamorig")) ossosMixamo++;
                }
            }

            var tv = Object.FindFirstObjectByType<TelaTV>();
            int fotosTv = (tv && tv.fotos != null) ? tv.fotos.Length : 0;

            Debug.Log($"[VALIDACAO SEBRAE GUIDES] " +
                      $"Guia='{controle.dialogo?.roteiro?.nomeDoGuia}' | " +
                      $"OssosMixamo={ossosMixamo} | " +
                      $"Paradas={controle.paradas.Count}/5 | " +
                      $"FalasComAudio={audiosOk}/{controle.dialogo?.roteiro?.falas.Count} | " +
                      $"SlidesTV={fotosTv}");
        }

        public static void RenderizarPreviewsDaCena()
        {
            ShaderUtil.allowAsyncCompilation = false;
            var cena = EditorSceneManager.OpenScene(CenaVisita, OpenSceneMode.Single);
            if (!cena.IsValid()) return;

            var controle = Object.FindFirstObjectByType<ControleVisita>();
            if (!controle || !controle.guia || !controle.dialogo) return;

            // Mostra o balão na fala R1
            if (controle.dialogo.balao) controle.dialogo.balao.SetActive(true);
            if (controle.dialogo.textoNome) controle.dialogo.textoNome.text = "Prof. Otávio";
            if (controle.dialogo.textoContador) controle.dialogo.textoContador.text = "Espaços visitados: 0/5";
            if (controle.dialogo.textoFala)
                controle.dialogo.textoFala.text = "Olá! Bem-vindo ao Sebraelab. Eu sou o Professor Otávio e vou te acompanhar nesta visita guiada.";

            string outDir = @"C:\Users\victo\.gemini\antigravity\brain\cff11d72-d20b-40e7-9052-4a7a3e1bba99";

            // Captura 1: Recepção (A0) olhando para o Prof. Otávio acenando + Balão + Crachá no Balcão
            controle.guia.Posicionar(controle.paradas[0].posicaoGuia);
            controle.guia.AplicarPoseNoEditor(0.28f, aceno: 1f, fala: 0f);
            Vector3 posGuia0 = controle.paradas[0].posicaoGuia.position;
            Vector3 dirGuia0 = controle.paradas[0].posicaoGuia.forward;
            Vector3 camPos1 = posGuia0 + dirGuia0 * 2.15f + new Vector3(0.25f, 1.25f, 0f);
            Vector3 lookAt1 = posGuia0 + new Vector3(0.30f, 1.15f, 0f);
            if (controle.dialogo.balao && controle.dialogo.balao.transform.parent)
            {
                var pivo = controle.dialogo.balao.transform.parent;
                pivo.rotation = Quaternion.LookRotation(Plano(pivo.position - camPos1));
            }
            CapturarCamera(camPos1, lookAt1, System.IO.Path.Combine(outDir, "unity_cena_recepcao.png"));

            // Captura 2: Arena de Eventos (A1) com a TV ligada e o Prof. Otávio explicando em N1
            var tv = Object.FindFirstObjectByType<TelaTV>();
            if (tv) tv.Ligar();
            controle.guia.Posicionar(controle.paradas[1].posicaoGuia);
            controle.guia.AplicarPoseNoEditor(0.75f, aceno: 0f, fala: 1f);
            if (controle.paradas[1].poi && controle.paradas[1].poi.painel)
                controle.paradas[1].poi.painel.SetActive(true);
            if (controle.dialogo.textoContador) controle.dialogo.textoContador.text = "Espaços visitados: 1/5";
            if (controle.dialogo.textoFala)
                controle.dialogo.textoFala.text = "Esta é a arena de eventos. Aqui acontecem palestras, oficinas práticas e apresentações.";

            Vector3 posGuia1 = controle.paradas[1].posicaoGuia.position;
            Vector3 dirGuia1 = controle.paradas[1].posicaoGuia.forward;
            Vector3 camPos2 = posGuia1 + dirGuia1 * 2.5f + new Vector3(-0.4f, 1.30f, 0f);
            Vector3 lookAt2 = posGuia1 + new Vector3(-0.3f, 1.15f, 0f);
            if (controle.dialogo.balao && controle.dialogo.balao.transform.parent)
            {
                var pivo = controle.dialogo.balao.transform.parent;
                pivo.rotation = Quaternion.LookRotation(Plano(pivo.position - camPos2));
            }
            CapturarCamera(camPos2, lookAt2, System.IO.Path.Combine(outDir, "unity_cena_arena.png"));

            // Restaura a cena salva limpa
            EditorSceneManager.OpenScene(CenaVisita, OpenSceneMode.Single);
            Debug.Log("[Sebrae Guides] Capturas da cena Visita.unity geradas com sucesso!");
        }

        static void CapturarCamera(Vector3 pos, Vector3 alvo, string caminhoPng)
        {
            var camObj = new GameObject("TempRenderCam");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.10f, 0.14f, 0.22f, 1f);
            cam.fieldOfView = 52f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;
            cam.transform.position = pos;
            cam.transform.LookAt(alvo);

            int w = 1280, h = 720;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            // Warm-up render para garantir compilação de shaders URP no primeiro frame
            cam.Render();
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            System.IO.File.WriteAllBytes(caminhoPng, tex.EncodeToPNG());

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(camObj);
        }

        [MenuItem("Sebrae Guides/Editar as falas do guia", false, 20)]
        static void SelecionarRoteiro()
        {
            GarantirPasta(PastaDados);
            var roteiro = CarregarOuCriarRoteiro(false);
            VincularAudiosAoRoteiro(roteiro);
            Selection.activeObject = roteiro;
            EditorGUIUtility.PingObject(roteiro);
        }

        [MenuItem("Sebrae Guides/Restaurar as falas padrão (apaga as edições)", false, 21)]
        static void RestaurarRoteiro()
        {
            if (!EditorUtility.DisplayDialog("Sebrae Guides",
                    "Isto substitui todas as falas do RoteiroGuia pelo texto padrão. As edições feitas no Inspector se perdem.",
                    "Restaurar", "Cancelar"))
                return;
            GarantirPasta(PastaDados);
            var roteiro = CarregarOuCriarRoteiro(true);
            VincularAudiosAoRoteiro(roteiro);
            Selection.activeObject = roteiro;
        }

        // ------------------------------------------------------------------ preparação
        static void ConfigurarModelo()
        {
            var importador = AssetImporter.GetAtPath(CaminhoModelo) as ModelImporter;
            if (importador == null)
            {
                Debug.LogWarning($"[Sebrae Guides] Não encontrei {CaminhoModelo}. Os colisores não foram ligados.");
                return;
            }

            bool mudou = false;
            if (!importador.addCollider) { importador.addCollider = true; mudou = true; }
            if (importador.importCameras) { importador.importCameras = false; mudou = true; }
            if (importador.importLights) { importador.importLights = false; mudou = true; }
            if (!importador.generateSecondaryUV) { importador.generateSecondaryUV = true; mudou = true; }

            if (mudou)
            {
                importador.SaveAndReimport();
                Debug.Log("[Sebrae Guides] Modelo Sebraelab reimportado: colisores ligados e UVs de lightmap geradas.");
            }
        }

        static void ConfigurarGuia3D()
        {
            var importador = AssetImporter.GetAtPath(CaminhoGuiaFBX) as ModelImporter;
            if (importador == null)
            {
                Debug.LogWarning($"[Sebrae Guides] Modelo do Prof. Otávio não encontrado em {CaminhoGuiaFBX}.");
                return;
            }

            bool mudou = false;
            if (importador.importCameras) { importador.importCameras = false; mudou = true; }
            if (importador.importLights) { importador.importLights = false; mudou = true; }
            if (!importador.importAnimation) { importador.importAnimation = true; mudou = true; }
            if (importador.addCollider) { importador.addCollider = false; mudou = true; }

            if (mudou)
            {
                importador.SaveAndReimport();
            }
        }

        static void ConfigurarSpritesTV()
        {
            string[] arquivos =
            {
                $"{PastaTV}/tv_slide1_abertura.png",
                $"{PastaTV}/tv_slide2_oficina.png",
                $"{PastaTV}/tv_slide3_pitchday.png"
            };
            foreach (var arq in arquivos)
            {
                var imp = AssetImporter.GetAtPath(arq) as TextureImporter;
                if (imp != null && imp.textureType != TextureImporterType.Sprite)
                {
                    imp.textureType = TextureImporterType.Sprite;
                    imp.spriteImportMode = SpriteImportMode.Single;
                    imp.SaveAndReimport();
                }
            }
        }

        static Scene AbrirCenaVisita()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CenaVisita) == null)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CenaOrigem) == null)
                {
                    Debug.LogError($"[Sebrae Guides] Não encontrei {CenaOrigem}.");
                    return default;
                }
                if (!AssetDatabase.CopyAsset(CenaOrigem, CenaVisita))
                {
                    Debug.LogError("[Sebrae Guides] Não consegui copiar a SampleScene.");
                    return default;
                }
                Debug.Log($"[Sebrae Guides] Cena criada: {CenaVisita} (cópia da SampleScene).");
            }
            return EditorSceneManager.OpenScene(CenaVisita, OpenSceneMode.Single);
        }

        static void DesligarSobras(Scene cena)
        {
            var desligados = new List<string>();
            foreach (var objeto in cena.GetRootGameObjects())
            {
                if (System.Array.IndexOf(RaizesParaDesligar, objeto.name) >= 0 && objeto.activeSelf)
                {
                    objeto.SetActive(false);
                    desligados.Add(objeto.name);
                }
            }

            var modelo = BuscarRaiz(cena, NomeModeloNaCena);
            int sobras = 0;
            if (modelo)
            {
                foreach (Transform filho in modelo.transform)
                {
                    string nome = filho.name.Trim();
                    if ((nome.StartsWith("SM_") || nome.StartsWith("chrome_") || nome == "Camera") && filho.gameObject.activeSelf)
                    {
                        filho.gameObject.SetActive(false);
                        sobras++;
                    }
                }
            }

            Debug.Log($"[Sebrae Guides] Desligados na raiz: {string.Join(", ", desligados)}. Sobras do Blender desligadas no modelo: {sobras}.");
        }

        static RoteiroGuia CarregarOuCriarRoteiro(bool restaurar)
        {
            var roteiro = AssetDatabase.LoadAssetAtPath<RoteiroGuia>(CaminhoRoteiro);
            if (!roteiro)
            {
                roteiro = ScriptableObject.CreateInstance<RoteiroGuia>();
                RoteiroPadrao.Preencher(roteiro);
                AssetDatabase.CreateAsset(roteiro, CaminhoRoteiro);
            }
            else if (restaurar || roteiro.nomeDoGuia == "Guia")
            {
                RoteiroPadrao.Preencher(roteiro);
                EditorUtility.SetDirty(roteiro);
            }
            AssetDatabase.SaveAssets();
            return roteiro;
        }

        static void VincularAudiosAoRoteiro(RoteiroGuia roteiro)
        {
            if (!roteiro || roteiro.falas == null) return;
            bool mudou = false;

            if (roteiro.nomeDoGuia != "Prof. Otávio")
            {
                roteiro.nomeDoGuia = "Prof. Otávio";
                mudou = true;
            }

            foreach (var fala in roteiro.falas)
            {
                if (fala == null || string.IsNullOrEmpty(fala.id)) continue;

                if (fala.id == "R1" && fala.texto.Contains("Eu sou o guia"))
                {
                    fala.texto = "Olá! Bem-vindo ao Sebraelab. Eu sou o Professor Otávio e vou te acompanhar nesta visita guiada.";
                    mudou = true;
                }
                if (fala.id == "F2" && fala.texto.Contains("["))
                {
                    fala.texto = "Esta sala reservada é usada para reuniões estratégicas, oficinas focadas e atendimento especializado a empreendedores.";
                    mudou = true;
                }

                string caminhoMp3 = $"{PastaVozes}/{fala.id}.mp3";
                string caminhoWav = $"{PastaVozes}/{fala.id}.wav";
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(caminhoMp3);
                if (!clip) clip = AssetDatabase.LoadAssetAtPath<AudioClip>(caminhoWav);

                if (clip && fala.audio != clip)
                {
                    fala.audio = clip;
                    mudou = true;
                }
            }

            if (mudou)
            {
                EditorUtility.SetDirty(roteiro);
                AssetDatabase.SaveAssets();
            }
        }

        static void AjustarXROrigin()
        {
            var origem = Object.FindFirstObjectByType<XROrigin>();
            if (!origem)
            {
                Debug.LogWarning("[Sebrae Guides] Não encontrei o XR Origin na cena.");
                return;
            }

            var raiz = origem.transform.root;
            raiz.SetPositionAndRotation(new Vector3(-59.8f, Piso, -60.7f), Quaternion.Euler(0f, 162.8f, 0f));

            int desligados = 0;
            foreach (var componente in raiz.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!componente) continue;
                var serializado = new SerializedObject(componente);
                var propriedade = serializado.FindProperty("m_SmoothMotionEnabled");
                if (propriedade != null && propriedade.propertyType == SerializedPropertyType.Boolean && propriedade.boolValue)
                {
                    propriedade.boolValue = false;
                    serializado.ApplyModifiedProperties();
                    desligados++;
                }
            }

            Debug.Log($"[Sebrae Guides] Visitante posicionado na recepção (A0). Movimento contínuo desligado em {desligados} controle(s).");
        }

        static void ConfigurarBuild()
        {
            var cenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            cenas.RemoveAll(c => c.path == CenaVisita);
            foreach (var c in cenas)
            {
                if (c.path == CenaOrigem) c.enabled = false;
            }
            cenas.Insert(0, new EditorBuildSettingsScene(CenaVisita, true));
            EditorBuildSettings.scenes = cenas.ToArray();
        }

        // ------------------------------------------------------------------ montagem
        static ControleVisita Construir(Transform raiz, Transform modelo, RoteiroGuia roteiro)
        {
            var clique = AssetDatabase.LoadAssetAtPath<AudioClip>(CaminhoClique);
            var prefabAncora = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoAncora);
            if (!prefabAncora) Debug.LogWarning("[Sebrae Guides] Prefab Teleport Anchor não encontrado; usando um disco simples.");

            var grupoTeleporte = Filho(raiz, "Teleporte");
            var grupoGuia = Filho(raiz, "Posicoes do guia");
            var grupoPois = Filho(raiz, "Pontos de interesse");
            var grupoInteracoes = Filho(raiz, "Interacoes");

            var controle = Filho(raiz, "ControleVisita").gameObject.AddComponent<ControleVisita>();
            controle.dialogo = CriarGuia(raiz, roteiro, clique, out var guia);
            controle.guia = guia;
            controle.marcador = CriarMarcador(raiz);

            Button botaoTV = null;
            GameObject primeiraAncora = null;

            foreach (var d in Paradas())
            {
                var posAncora = new Vector3(d.ancora.x, Piso, d.ancora.y);
                var ancora = CriarAncora(prefabAncora, grupoTeleporte, $"Teleporte {d.codigo} ({d.chave})", posAncora, d.yawAncora);
                if (!primeiraAncora) primeiraAncora = ancora;

                var posGuia = Filho(grupoGuia, $"Guia em {d.chave}");
                posGuia.position = new Vector3(d.guia.x, Piso, d.guia.y);
                posGuia.rotation = Quaternion.LookRotation(Plano(posAncora - posGuia.position));

                var objetoPoi = Filho(grupoPois, $"POI {d.codigo} {d.titulo}");
                objetoPoi.position = posAncora;
                var poi = objetoPoi.gameObject.AddComponent<PontoDeInteresse>();
                poi.titulo = d.titulo;
                poi.resumo = d.resumo;
                poi.detalhes = d.detalhes;
                poi.raio = d.raio;

                var posPainel = new Vector3(d.painel.x, AlturaPainel, d.painel.y);
                var painel = CriarPainel(objetoPoi, posPainel, posAncora, d.titulo, d.resumo, d.rotuloBotao,
                    out var textoTitulo, out var textoCorpo, out var botao);
                poi.painel = painel.gameObject;
                poi.textoTitulo = textoTitulo;
                poi.textoCorpo = textoCorpo;
                if (d.chave == "arena") botaoTV = botao;

                controle.paradas.Add(new ParadaVisita
                {
                    chave = d.chave,
                    pontoTeleporte = ancora,
                    posicaoGuia = posGuia,
                    balaoAEsquerda = d.balaoAEsquerda,
                    poi = poi,
                    falaAoChegar = d.falaAoChegar,
                    eventoDeConclusao = d.evento,
                    falaAoConcluir = d.falaAoConcluir,
                });
            }

            guia.transform.SetPositionAndRotation(controle.paradas[0].posicaoGuia.position, controle.paradas[0].posicaoGuia.rotation);

            // I2 - crachá no balcão da recepção
            CriarCracha(grupoInteracoes, clique);

            // I3 - TV da arena, ligada pelo botão do painel
            var tv = CriarTV(grupoInteracoes, clique);
            if (botaoTV) UnityEventTools.AddPersistentListener(botaoTV.onClick, tv.Ligar);

            // I4 - post-it na estante e mural no quadro do coworking
            var postIt = CriarPostIt(grupoInteracoes);
            CriarMural(grupoInteracoes, clique);
            Parada(controle, "coworking").ativarAoChegar.Add(postIt);

            // I5 - maçaneta e porta de vidro da sala dos fundos
            var macaneta = CriarPorta(grupoInteracoes, modelo, clique);
            Parada(controle, "fundos").ativarAoChegar.Add(macaneta);
            var ancoraSala = CriarAncora(prefabAncora, grupoTeleporte, "Teleporte A4b (dentro da sala)",
                new Vector3(-66.1f, Piso, -80.6f), 200f);
            Parada(controle, "fundos").ativarAoConcluir.Add(ancoraSala);

            CriarAreaLivre(modelo, primeiraAncora);
            CriarLuzes(raiz);

            EditorUtility.SetDirty(controle);
            Debug.Log("[Sebrae Guides] Visita montada: Prof. Otávio 3D, 5 POIs, 6 pontos de teleporte, 21 áudios, crachá, TV com slides, post-it, mural e porta.");
            return controle;
        }

        static ParadaVisita Parada(ControleVisita controle, string chave)
        {
            return controle.paradas.Find(p => p.chave == chave);
        }

        static Dictionary<string, Material> CriarPaletaMateriaisProfOtavio()
        {
            var dict = new Dictionary<string, Material>
            {
                ["Mat_Skin"] = MatPBR("Otavio_Skin", new Color(0.96f, 0.76f, 0.64f), 0.45f, 0f),
                ["Mat_Cheeks"] = MatPBR("Otavio_Cheeks", new Color(0.94f, 0.52f, 0.48f), 0.50f, 0f),
                ["Mat_Hair_Silver"] = MatPBR("Otavio_Hair", new Color(0.78f, 0.80f, 0.84f), 0.55f, 0f),
                ["Mat_Mustache"] = MatPBR("Otavio_Mustache", new Color(0.88f, 0.89f, 0.92f), 0.50f, 0f),
                ["Mat_Eye_Dark"] = MatPBR("Otavio_Eye_Dark", new Color(0.04f, 0.05f, 0.08f), 0.12f, 0f),
                ["Mat_Eye_Highlight"] = MatPBR("Otavio_Eye_Highlight", Color.white, 0.05f, 0f),
                ["Mat_Glasses_Gold"] = MatPBR("Otavio_Glasses_Gold", new Color(0.85f, 0.64f, 0.20f), 0.22f, 0.85f),
                ["Mat_Jacket_Tweed"] = MatPBR("Otavio_Jacket_Tweed", new Color(0.42f, 0.27f, 0.17f), 0.72f, 0f),
                ["Mat_Jacket_Lapel"] = MatPBR("Otavio_Jacket_Lapel", new Color(0.35f, 0.22f, 0.13f), 0.70f, 0f),
                ["Mat_Elbow_Patch"] = MatPBR("Otavio_Elbow_Patch", new Color(0.25f, 0.15f, 0.09f), 0.65f, 0f),
                ["Mat_Vest_Burgundy"] = MatPBR("Otavio_Vest_Burgundy", new Color(0.50f, 0.12f, 0.16f), 0.62f, 0f),
                ["Mat_Shirt_Cream"] = MatPBR("Otavio_Shirt_Cream", new Color(0.95f, 0.93f, 0.87f), 0.55f, 0f),
                ["Mat_Bowtie_Red"] = MatPBR("Otavio_Bowtie_Red", new Color(0.78f, 0.14f, 0.16f), 0.42f, 0f),
                ["Mat_Badge_Gold"] = MatPBR("Otavio_Badge_Gold", new Color(0.95f, 0.76f, 0.22f), 0.20f, 0.88f),
                ["Mat_Badge_Blue"] = MatPBR("Otavio_Badge_Blue", new Color(0.12f, 0.36f, 0.72f), 0.30f, 0f),
                ["Mat_Trousers_Navy"] = MatPBR("Otavio_Trousers_Navy", new Color(0.16f, 0.20f, 0.28f), 0.65f, 0f),
                ["Mat_Shoes_Leather"] = MatPBR("Otavio_Shoes_Leather", new Color(0.14f, 0.08f, 0.05f), 0.28f, 0f),
                ["Mat_Sole_Dark"] = MatPBR("Otavio_Sole_Dark", new Color(0.06f, 0.05f, 0.05f), 0.70f, 0f),
            };
            return dict;
        }

        static AnimatorController CriarOuAtualizarControllerGuia()
        {
            GarantirPasta(PastaNPC);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(CaminhoControllerGuia);
            controller.AddParameter("Falando", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Andando", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Acenar", AnimatorControllerParameterType.Trigger);

            AnimationClip clipIdle = null;
            AnimationClip clipTalk = null;
            AnimationClip clipWave = null;
            AnimationClip clipWalk = null;

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(CaminhoGuiaFBX))
            {
                if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    string n = clip.name.ToLowerInvariant();
                    if (n.Contains("idle")) clipIdle = clip;
                    else if (n.Contains("talk")) clipTalk = clip;
                    else if (n.Contains("wave")) clipWave = clip;
                    else if (n.Contains("walk")) clipWalk = clip;
                }
            }

            var sm = controller.layers[0].stateMachine;
            var stIdle = sm.AddState("Idle_Tour");
            stIdle.motion = clipIdle;
            sm.defaultState = stIdle;

            var stTalk = sm.AddState("Talk_Explain");
            stTalk.motion = clipTalk ? clipTalk : clipIdle;

            var stWave = sm.AddState("Wave_Welcome");
            stWave.motion = clipWave ? clipWave : clipIdle;

            var stWalk = sm.AddState("Walk_Tour");
            stWalk.motion = clipWalk ? clipWalk : clipIdle;

            var tIdleToTalk = stIdle.AddTransition(stTalk);
            tIdleToTalk.AddCondition(AnimatorConditionMode.If, 0, "Falando");
            tIdleToTalk.hasExitTime = false;
            tIdleToTalk.duration = 0.2f;

            var tTalkToIdle = stTalk.AddTransition(stIdle);
            tTalkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Falando");
            tTalkToIdle.hasExitTime = false;
            tTalkToIdle.duration = 0.25f;

            var tAnyToWave = sm.AddAnyStateTransition(stWave);
            tAnyToWave.AddCondition(AnimatorConditionMode.If, 0, "Acenar");
            tAnyToWave.hasExitTime = false;
            tAnyToWave.duration = 0.15f;

            var tWaveToIdle = stWave.AddTransition(stIdle);
            tWaveToIdle.hasExitTime = true;
            tWaveToIdle.exitTime = 0.9f;
            tWaveToIdle.duration = 0.25f;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        static DialogoGuia CriarGuia(Transform raiz, RoteiroGuia roteiro, AudioClip clique, out GuiaNPC guia)
        {
            var objeto = Filho(raiz, "Guia").gameObject;
            var modelo = Filho(objeto.transform, "Modelo (Prof. Otavio)");

            var prefabFbx = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoGuiaFBX);
            Animator animador = null;
            GameObject instanciaGuia = null;

            if (prefabFbx)
            {
                var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefabFbx, objeto.scene);
                instanciaGuia = instancia;
                instancia.name = "ProfessorOtavio_3D";
                instancia.transform.SetParent(modelo, false);
                instancia.transform.localPosition = Vector3.zero;
                instancia.transform.localScale = Vector3.one;
                instancia.transform.localRotation = Quaternion.identity;

                // Garante que a ponta do pé (LeftToeBase) aponte para a frente (+Z do Guia)
                Transform pe = null, pontaPe = null;
                foreach (var t in instancia.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.EndsWith("LeftFoot")) pe = t;
                    else if (t.name.EndsWith("LeftToeBase")) pontaPe = t;
                }
                if (pe && pontaPe)
                {
                    Vector3 dirPe = modelo.InverseTransformPoint(pontaPe.position) - modelo.InverseTransformPoint(pe.position);
                    if (dirPe.z < 0f)
                    {
                        instancia.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                }

                // Aplica materiais URP nos slots da malha
                var paleta = CriarPaletaMateriaisProfOtavio();
                foreach (var ren in instancia.GetComponentsInChildren<Renderer>(true))
                {
                    var mats = ren.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] == null) continue;
                        string nomeMat = mats[i].name.Replace(" (Instance)", "").Trim();
                        if (paleta.TryGetValue(nomeMat, out var matUrp))
                        {
                            mats[i] = matUrp;
                        }
                    }
                    ren.sharedMaterials = mats;
                }

                // Remove qualquer Collider para não roubar o raio do controle VR
                foreach (var col in instancia.GetComponentsInChildren<Collider>(true))
                {
                    Object.DestroyImmediate(col);
                }

                animador = instancia.GetComponent<Animator>();
                if (!animador) animador = instancia.AddComponent<Animator>();
                animador.runtimeAnimatorController = CriarOuAtualizarControllerGuia();
            }
            else
            {
                var corpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                corpo.name = "Corpo";
                corpo.transform.SetParent(modelo, false);
                corpo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                corpo.transform.localScale = new Vector3(0.5f, 0.85f, 0.5f);
                Pintar(corpo, Mat("Guia_Corpo", new Color(0.0f, 0.37f, 0.72f)));
            }

            var voz = objeto.AddComponent<AudioSource>();
            voz.playOnAwake = false;
            voz.spatialBlend = 1f;
            voz.minDistance = 1.2f;
            voz.maxDistance = 16f;
            voz.rolloffMode = AudioRolloffMode.Linear;

            guia = objeto.AddComponent<GuiaNPC>();
            guia.modelo = modelo;
            guia.alvoAnimacao = instanciaGuia;
            guia.animador = animador;
            guia.voz = voz;

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(CaminhoGuiaFBX))
            {
                if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    string n = clip.name.ToLowerInvariant();
                    if (n.Contains("idle")) guia.clipeIdle = clip;
                    else if (n.Contains("talk")) guia.clipeFala = clip;
                    else if (n.Contains("wave")) guia.clipeAceno = clip;
                    else if (n.Contains("walk")) guia.clipeAndar = clip;
                }
            }
            guia.AplicarPoseNoEditor(0.25f, 0f, 0f);

            var dialogo = objeto.AddComponent<DialogoGuia>();
            dialogo.roteiro = roteiro;
            dialogo.voz = voz;
            dialogo.somClique = clique;
            dialogo.guia = guia;

            var pivoBalao = Filho(objeto.transform, "Balao (pivo)");
            pivoBalao.localRotation = Quaternion.Euler(0f, 180f, 0f);
            pivoBalao.gameObject.AddComponent<ViradoParaCamera>();
            var canvas = CriarCanvas("Balao", pivoBalao, new Vector2(900f, 480f));
            canvas.transform.localPosition = new Vector3(0.75f, 1.55f, -0.25f);
            var rt = (RectTransform)canvas.transform;

            Imagem("Fundo", rt, CorFundo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            dialogo.textoNome = Texto("Nome", rt, roteiro ? roteiro.nomeDoGuia : "Prof. Otávio", 34f, CorDestaque, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f), new Vector2(0.6f, 1f), new Vector2(36f, -72f), new Vector2(0f, -20f), FontStyles.Bold);
            dialogo.textoContador = Texto("Contador", rt, "Espaços visitados: 0/5", 26f, CorTextoSuave, TextAlignmentOptions.TopRight,
                new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0f, -66f), new Vector2(-36f, -22f));
            dialogo.textoFala = Texto("Fala", rt, "...", 38f, CorTextoClaro, TextAlignmentOptions.TopLeft,
                Vector2.zero, Vector2.one, new Vector2(36f, 132f), new Vector2(-36f, -82f));

            var areaBotoes = NovoUI("Botoes", rt, Vector2.zero, new Vector2(1f, 0f), new Vector2(24f, 18f), new Vector2(-24f, 118f));
            var layout = areaBotoes.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 24f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            dialogo.areaBotoes = (RectTransform)areaBotoes.transform;
            dialogo.modeloBotao = Botao("ModeloBotao", dialogo.areaBotoes, "Continuar", 320f, 88f);

            dialogo.textoDica = Texto("Dica", rt, "", 28f, CorDica, TextAlignmentOptions.Center,
                Vector2.zero, new Vector2(1f, 0f), new Vector2(36f, 22f), new Vector2(-36f, 118f), FontStyles.Italic);

            dialogo.balao = canvas.gameObject;
            return dialogo;
        }

        static MarcadorProximaParada CriarMarcador(Transform raiz)
        {
            var objeto = new GameObject("Marcador - Proxima parada", typeof(RectTransform));
            objeto.transform.SetParent(raiz, false);
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.text = "Próxima parada";
            texto.fontSize = 1.6f;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = CorDica;
            ((RectTransform)objeto.transform).sizeDelta = new Vector2(2.6f, 0.5f);
            objeto.AddComponent<ViradoParaCamera>();
            return objeto.AddComponent<MarcadorProximaParada>();
        }

        static GameObject CriarAncora(GameObject prefab, Transform pai, string nome, Vector3 posicao, float yaw)
        {
            GameObject ancora;
            if (prefab)
            {
                ancora = (GameObject)PrefabUtility.InstantiatePrefab(prefab, pai.gameObject.scene);
                ancora.transform.SetParent(pai, true);
            }
            else
            {
                ancora = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ancora.transform.SetParent(pai, true);
                ancora.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);
                var teleporte = ancora.AddComponent<TeleportationAnchor>();
                teleporte.teleportAnchorTransform = ancora.transform;
                teleporte.matchOrientation = MatchOrientation.TargetUpAndForward;
                teleporte.interactionLayers = InteractionLayerMask.GetMask("Teleport");
            }
            ancora.name = nome;
            ancora.transform.SetPositionAndRotation(posicao, Quaternion.Euler(0f, yaw, 0f));
            return ancora;
        }

        static RectTransform CriarPainel(Transform pai, Vector3 posicao, Vector3 olhador, string titulo, string resumo, string rotuloBotao,
            out TMP_Text textoTitulo, out TMP_Text textoCorpo, out Button botao)
        {
            bool temBotao = !string.IsNullOrEmpty(rotuloBotao);
            var canvas = CriarCanvas("Painel", pai, new Vector2(1100f, temBotao ? 690f : 600f));
            var rt = (RectTransform)canvas.transform;
            rt.position = posicao;
            rt.rotation = Quaternion.LookRotation(Plano(posicao - olhador));

            Imagem("Fundo", rt, CorFundo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            Imagem("Faixa", rt, CorDestaque, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -14f), Vector2.zero, false);
            textoTitulo = Texto("Titulo", rt, titulo, 58f, CorTextoClaro, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f), Vector2.one, new Vector2(44f, -112f), new Vector2(-44f, -36f), FontStyles.Bold);
            textoCorpo = Texto("Corpo", rt, resumo, 40f, CorTextoSuave, TextAlignmentOptions.TopLeft,
                Vector2.zero, Vector2.one, new Vector2(44f, temBotao ? 140f : 40f), new Vector2(-44f, -126f));

            botao = null;
            if (temBotao)
            {
                botao = Botao("Botao " + rotuloBotao, rt, rotuloBotao, 440f, 96f);
                var brt = (RectTransform)botao.transform;
                brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
                brt.pivot = new Vector2(0.5f, 0f);
                brt.anchoredPosition = new Vector2(0f, 32f);
            }
            return rt;
        }

        static GameObject CriarCracha(Transform pai, AudioClip clique)
        {
            var raiz = Filho(pai, "I2 Cracha de visitante");
            raiz.position = new Vector3(-59.30f, 0.739f, -62.55f);
            raiz.rotation = Quaternion.Euler(90f, 160f, 0f);

            var corpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corpo.name = "Corpo";
            corpo.transform.SetParent(raiz, false);
            corpo.transform.localScale = new Vector3(0.10f, 0.14f, 0.012f);
            Pintar(corpo, Mat("Cracha", new Color(0.93f, 0.95f, 0.98f)));

            var faixa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            faixa.name = "Faixa";
            faixa.transform.SetParent(raiz, false);
            faixa.transform.localPosition = new Vector3(0f, 0.049f, 0f);
            faixa.transform.localScale = new Vector3(0.10f, 0.032f, 0.013f);
            Object.DestroyImmediate(faixa.GetComponent<Collider>());
            Pintar(faixa, Mat("Destaque", CorDestaque));

            var rotulo = new GameObject("Texto", typeof(RectTransform));
            rotulo.transform.SetParent(raiz, false);
            rotulo.transform.localPosition = new Vector3(0f, -0.012f, -0.0068f);
            var texto = rotulo.AddComponent<TextMeshPro>();
            texto.text = "VISITANTE";
            texto.fontSize = 0.16f;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = new Color(0.07f, 0.11f, 0.19f);
            ((RectTransform)rotulo.transform).sizeDelta = new Vector2(0.10f, 0.04f);

            var corpoFisico = raiz.gameObject.AddComponent<Rigidbody>();
            corpoFisico.mass = 0.05f;
            corpoFisico.isKinematic = true;
            corpoFisico.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            var pegar = raiz.gameObject.AddComponent<XRGrabInteractable>();
            pegar.throwOnDetach = false;
            pegar.useDynamicAttach = true;
            pegar.farAttachMode = InteractableFarAttachMode.Near;
            var gatilho = raiz.gameObject.AddComponent<GatilhoInteracao>();
            gatilho.evento = "pegou_cracha";
            gatilho.som = clique;
            raiz.gameObject.AddComponent<FisicaAoSoltar>();
            return raiz.gameObject;
        }

        static TelaTV CriarTV(Transform pai, AudioClip clique)
        {
            var canvas = CriarCanvas("I3 TV - tela", pai, new Vector2(1060f, 660f));
            var rt = (RectTransform)canvas.transform;
            rt.position = new Vector3(-68.275f, 0.97f, -56.235f);
            rt.rotation = Quaternion.identity;

            var conteudo = NovoUI("Conteudo", rt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var crt = (RectTransform)conteudo.transform;
            Imagem("Fundo", crt, new Color(0.02f, 0.02f, 0.03f, 1f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false);
            var foto = Imagem("Foto", crt, Color.white, Vector2.zero, Vector2.one, new Vector2(20f, 96f), new Vector2(-20f, -20f), false);
            foto.preserveAspect = true;
            foto.enabled = false;
            var legenda = Texto("Legenda", crt, "", 38f, CorTextoClaro, TextAlignmentOptions.Bottom,
                Vector2.zero, Vector2.one, new Vector2(24f, 18f), new Vector2(-24f, -24f));

            var tela = canvas.gameObject.AddComponent<TelaTV>();
            tela.tela = conteudo;
            tela.imagem = foto;
            tela.legenda = legenda;
            tela.som = clique;

            var sp1 = AssetDatabase.LoadAssetAtPath<Sprite>($"{PastaTV}/tv_slide1_abertura.png");
            var sp2 = AssetDatabase.LoadAssetAtPath<Sprite>($"{PastaTV}/tv_slide2_oficina.png");
            var sp3 = AssetDatabase.LoadAssetAtPath<Sprite>($"{PastaTV}/tv_slide3_pitchday.png");
            var listaSprites = new List<Sprite>();
            if (sp1) listaSprites.Add(sp1);
            if (sp2) listaSprites.Add(sp2);
            if (sp3) listaSprites.Add(sp3);
            tela.fotos = listaSprites.ToArray();

            tela.legendas = new[]
            {
                "Palestras e Painéis de Inovação Aberta no Sebraelab",
                "Oficinas Práticas de Design Thinking e Modelagem",
                "Demodays, Hackathons e Apresentação de Startups",
            };
            return tela;
        }

        static GameObject CriarPostIt(Transform pai)
        {
            var raiz = Filho(pai, "I4 Post-it (estante)");
            raiz.position = new Vector3(-70.72f, 1.819f, -65.30f);
            raiz.rotation = Quaternion.Euler(90f, 0f, 0f);

            var corpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corpo.name = "Corpo";
            corpo.transform.SetParent(raiz, false);
            corpo.transform.localScale = new Vector3(0.12f, 0.12f, 0.015f);
            Pintar(corpo, Mat("PostIt", new Color(1f, 0.86f, 0.2f)));

            var corpoFisico = raiz.gameObject.AddComponent<Rigidbody>();
            corpoFisico.mass = 0.02f;
            corpoFisico.isKinematic = true;
            corpoFisico.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            var pegar = raiz.gameObject.AddComponent<XRGrabInteractable>();
            pegar.throwOnDetach = false;
            pegar.useDynamicAttach = true;
            pegar.farAttachMode = InteractableFarAttachMode.Near;
            raiz.gameObject.AddComponent<PostIt>();
            raiz.gameObject.AddComponent<FisicaAoSoltar>();
            return raiz.gameObject;
        }

        static void CriarMural(Transform pai, AudioClip clique)
        {
            var raiz = Filho(pai, "I4 Mural de ideias (quadro)");
            raiz.position = new Vector3(-68.21f, 1.25f, -67.30f);
            raiz.rotation = Quaternion.identity;

            var area = raiz.gameObject.AddComponent<SphereCollider>();
            area.isTrigger = true;
            area.radius = 0.2f;
            raiz.gameObject.AddComponent<SocketMuralIdeias>();
            var gatilho = raiz.gameObject.AddComponent<GatilhoInteracao>();
            gatilho.evento = "postit_colado";
            gatilho.som = clique;
            gatilho.vibracao = 0f;

            var alvo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            alvo.name = "Alvo";
            alvo.transform.SetParent(raiz, false);
            alvo.transform.localPosition = new Vector3(0f, 0f, -0.045f);
            alvo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            alvo.transform.localScale = new Vector3(0.26f, 0.26f, 1f);
            Object.DestroyImmediate(alvo.GetComponent<Collider>());
            Pintar(alvo, Mat("Mural_Alvo", new Color(1f, 0.95f, 0.7f)));

            Texto3D("Titulo", raiz, "Mural de ideias", 0.8f, new Vector3(0f, 0.42f, -0.05f), CorDestaque);
            Texto3D("Dica", raiz, "Cole aqui", 0.45f, new Vector3(0f, 0.2f, -0.05f), new Color(0.2f, 0.2f, 0.25f));
        }

        static GameObject CriarPorta(Transform pai, Transform modelo, AudioClip clique)
        {
            var macaneta = Filho(pai, "I5 Macaneta");
            macaneta.position = new Vector3(-62.47f, 0.97f, -77.38f);

            var barra = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barra.name = "Barra";
            barra.transform.SetParent(macaneta, false);
            barra.transform.localScale = new Vector3(0.035f, 0.22f, 0.035f);
            Pintar(barra, Mat("Metal", new Color(0.75f, 0.77f, 0.8f)));

            macaneta.gameObject.AddComponent<XRSimpleInteractable>();
            var gatilho = macaneta.gameObject.AddComponent<GatilhoInteracao>();
            gatilho.som = clique;

            var porta = Filho(pai, "I5 Porta de vidro").gameObject.AddComponent<PortaDeslizante>();
            porta.som = clique;
            var vidro = BuscarNoModelo(modelo, "Backroom GlassDoor");
            if (vidro)
            {
                GameObjectUtility.SetStaticEditorFlags(vidro.gameObject, 0);
                porta.partes = new[] { vidro, macaneta };
            }
            else
            {
                Debug.LogWarning("[Sebrae Guides] Não achei 'Backroom GlassDoor' no modelo; só a maçaneta vai se mover.");
                porta.partes = new[] { macaneta };
            }

            UnityEventTools.AddPersistentListener(gatilho.aoAtivar, porta.Abrir);
            return macaneta.gameObject;
        }

        static void CriarAreaLivre(Transform modelo, GameObject ancoraModelo)
        {
            var piso = BuscarNoModelo(modelo, "Base");
            if (!piso)
            {
                Debug.LogWarning("[Sebrae Guides] Não achei o objeto 'Base' no modelo; a área livre de teleporte não foi criada.");
                return;
            }

            var area = piso.GetComponent<TeleportationArea>();
            if (!area) area = piso.gameObject.AddComponent<TeleportationArea>();

            var molde = ancoraModelo ? ancoraModelo.GetComponentInChildren<BaseTeleportationInteractable>() : null;
            area.interactionLayers = molde ? molde.interactionLayers : (InteractionLayerMask)InteractionLayerMask.GetMask("Teleport");
            area.matchOrientation = MatchOrientation.WorldSpaceUp;
            area.filterSelectionByHitNormal = true;
            area.upNormalToleranceDegrees = 25f;
            EditorUtility.SetDirty(area);
        }

        static void CriarLuzes(Transform raiz)
        {
            var grupo = Filho(raiz, "Luzes (desligue se ficar claro demais)");
            var pontos = new[]
            {
                ("Luz recepcao", new Vector3(-59.6f, 2.45f, -61.5f)),
                ("Luz arena", new Vector3(-65.5f, 2.45f, -59.5f)),
                ("Luz coworking e copa", new Vector3(-66.0f, 2.45f, -67.8f)),
                ("Luz corredor", new Vector3(-63.0f, 2.45f, -76.0f)),
            };
            foreach (var (nome, posicao) in pontos)
            {
                var luz = new GameObject(nome).AddComponent<Light>();
                luz.transform.SetParent(grupo, false);
                luz.transform.position = posicao;
                luz.type = LightType.Point;
                luz.range = 8f;
                luz.intensity = 1.2f;
                luz.shadows = LightShadows.None;
                luz.lightmapBakeType = LightmapBakeType.Realtime;
            }
        }

        // ------------------------------------------------------------------ utilitários
        static GameObject BuscarRaiz(Scene cena, string nome)
        {
            foreach (var objeto in cena.GetRootGameObjects())
            {
                if (objeto.name == nome) return objeto;
            }
            return null;
        }

        static Transform BuscarNoModelo(Transform pai, string nome)
        {
            foreach (Transform filho in pai)
            {
                if (filho.name.Trim() == nome) return filho;
                var achado = BuscarNoModelo(filho, nome);
                if (achado) return achado;
            }
            return null;
        }

        static Transform Filho(Transform pai, string nome)
        {
            var objeto = new GameObject(nome);
            objeto.transform.SetParent(pai, false);
            return objeto.transform;
        }

        static Vector3 Plano(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v : Vector3.forward;
        }

        static void GarantirPasta(string caminho)
        {
            if (AssetDatabase.IsValidFolder(caminho)) return;
            int barra = caminho.LastIndexOf('/');
            string pai = caminho.Substring(0, barra);
            GarantirPasta(pai);
            AssetDatabase.CreateFolder(pai, caminho.Substring(barra + 1));
        }

        static Material Mat(string nome, Color cor)
        {
            return MatPBR(nome, cor, 0.5f, 0f);
        }

        static Material MatPBR(string nome, Color cor, float roughness, float metallic)
        {
            GarantirPasta(PastaMateriais);
            string caminho = $"{PastaMateriais}/{nome}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (!material)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                material = new Material(shader) { name = nome };
                AssetDatabase.CreateAsset(material, caminho);
            }

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", cor);
            else material.color = cor;
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", Mathf.Clamp01(1f - roughness));
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", Mathf.Clamp01(metallic));
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Pintar(GameObject objeto, Material material)
        {
            var renderer = objeto.GetComponent<Renderer>();
            if (renderer) renderer.sharedMaterial = material;
        }

        static Canvas CriarCanvas(string nome, Transform pai, Vector2 tamanho)
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            objeto.transform.SetParent(pai, false);
            var canvas = objeto.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            objeto.AddComponent<CanvasScaler>();
            objeto.AddComponent<TrackedDeviceGraphicRaycaster>();
            var rt = (RectTransform)objeto.transform;
            rt.sizeDelta = tamanho;
            rt.localScale = Vector3.one * 0.001f;
            return canvas;
        }

        static GameObject NovoUI(string nome, RectTransform pai, Vector2 ancoraMin, Vector2 ancoraMax, Vector2 margemMin, Vector2 margemMax)
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            var rt = (RectTransform)objeto.transform;
            rt.SetParent(pai, false);
            rt.anchorMin = ancoraMin;
            rt.anchorMax = ancoraMax;
            rt.offsetMin = margemMin;
            rt.offsetMax = margemMax;
            return objeto;
        }

        static Sprite SpriteArredondado()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        static Image Imagem(string nome, RectTransform pai, Color cor, Vector2 ancoraMin, Vector2 ancoraMax, Vector2 margemMin, Vector2 margemMax, bool arredondada)
        {
            var imagem = NovoUI(nome, pai, ancoraMin, ancoraMax, margemMin, margemMax).AddComponent<Image>();
            imagem.color = cor;
            imagem.raycastTarget = false;
            if (arredondada)
            {
                imagem.sprite = SpriteArredondado();
                imagem.type = Image.Type.Sliced;
            }
            return imagem;
        }

        static TextMeshProUGUI Texto(string nome, RectTransform pai, string conteudo, float tamanho, Color cor, TextAlignmentOptions alinhamento,
            Vector2 ancoraMin, Vector2 ancoraMax, Vector2 margemMin, Vector2 margemMax, FontStyles estilo = FontStyles.Normal)
        {
            var texto = NovoUI(nome, pai, ancoraMin, ancoraMax, margemMin, margemMax).AddComponent<TextMeshProUGUI>();
            texto.text = conteudo;
            texto.fontSize = tamanho;
            texto.enableAutoSizing = true;
            texto.fontSizeMax = tamanho;
            texto.fontSizeMin = tamanho * 0.6f;
            texto.color = cor;
            texto.alignment = alinhamento;
            texto.fontStyle = estilo;
            texto.raycastTarget = false;
            return texto;
        }

        static Button Botao(string nome, RectTransform pai, string rotulo, float largura, float altura)
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            var rt = (RectTransform)objeto.transform;
            rt.SetParent(pai, false);
            rt.sizeDelta = new Vector2(largura, altura);

            var imagem = objeto.AddComponent<Image>();
            imagem.sprite = SpriteArredondado();
            imagem.type = Image.Type.Sliced;
            imagem.color = CorBotaoAceso;

            var botao = objeto.AddComponent<Button>();
            botao.targetGraphic = imagem;
            var cores = botao.colors;
            cores.normalColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cores.highlightedColor = Color.white;
            cores.selectedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            cores.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            cores.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
            cores.colorMultiplier = 1f;
            botao.colors = cores;

            var layout = objeto.AddComponent<LayoutElement>();
            layout.preferredWidth = largura;
            layout.preferredHeight = altura;

            Texto("Rotulo", rt, rotulo, altura * 0.42f, Color.white, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(12f, 4f), new Vector2(-12f, -4f), FontStyles.Bold);
            return botao;
        }

        static void Texto3D(string nome, Transform pai, string conteudo, float tamanho, Vector3 posicaoLocal, Color cor)
        {
            var objeto = new GameObject(nome, typeof(RectTransform));
            objeto.transform.SetParent(pai, false);
            objeto.transform.localPosition = posicaoLocal;
            objeto.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var texto = objeto.AddComponent<TextMeshPro>();
            texto.text = conteudo;
            texto.fontSize = tamanho;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = cor;
            ((RectTransform)objeto.transform).sizeDelta = new Vector2(1.2f, 0.2f);
        }
    }
}
