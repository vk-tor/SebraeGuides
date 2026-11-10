using System.Collections.Generic;
using System.Linq;
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
    /// guia, pontos de interesse com painéis, pontos de teleporte, as 5 interações e o loop.
    /// As posições são as mesmas da planta do documento de mapeamento.
    /// </summary>
    public static class MontarVisita
    {
        // ------------------------------------------------------------------ caminhos
        const string CenaOrigem = "Assets/Scenes/SampleScene.unity";
        const string CenaVisita = "Assets/Scenes/Visita.unity";
        const string Pasta = "Assets/_SebraeGuides";
        const string PastaDados = Pasta + "/Dados";
        const string PastaMateriais = Pasta + "/Materiais";
        const string CaminhoRoteiro = PastaDados + "/RoteiroGuia.asset";
        const string CaminhoModelo = "Assets/Sebraelab3D/Sebrae Lab.fbx";
        const string PastaModelos = Pasta + "/Modelos";
        // Professor Otávio: convertido do GLB enviado ao grupo (com as animações) e reduzido de 350 mil para 42 mil triângulos para o Quest 2.
        const string CaminhoAvatar = PastaModelos + "/ProfessorOtavio_Guia.fbx";
        const string CaminhoAnimador = PastaModelos + "/Guia.controller";
        const float AlturaAvatar = 1.43f; // altura do modelo original, usada só se o FBX vier em outra escala
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

        // O simulador tira o headset de verdade do Input System: salvo na cena, quebra o rastreamento da cabeça
        // no Quest (Link e APK). Sem óculos ele aparece sozinho no Play (Project Settings > XR Plug-in Management >
        // XR Interaction Toolkit > Use XR Device Simulator in scenes), então não precisa ficar na cena.
        static readonly string[] TiposDeSimulador = { "XRDeviceSimulator", "XRInteractionSimulator" };

        // Partes do modelo que se mexem na visita e não podem ser Static.
        static readonly string[] PartesMoveis = { "Backroom GlassDoor" };

        const string PastaLuzVisita = "Assets/Scenes/Visita";

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
                    detalhes = "Nesta visita você passa pela arena de eventos, pelo coworking, pela copa e pela sala dos fundos.",
                    falaAoChegar = "", evento = "pegou_cracha", falaAoConcluir = "R5",
                    ancora = new Vector2(-59.8f, -60.7f), yawAncora = 162.8f,
                    guia = new Vector2(-58.9f, -63.6f), painel = new Vector2(-60.58f, -62.54f), raio = 2f
                },
                new DadosParada
                {
                    codigo = "A1", chave = "arena", titulo = "Arena de eventos",
                    resumo = "Espaço para palestras, oficinas e apresentações de projetos.",
                    detalhes = "[Completar com o material do professor: tipos de evento e capacidade.]",
                    falaAoChegar = "A1", evento = "tv_ligada", falaAoConcluir = "A3", rotuloBotao = "Ligar a TV",
                    ancora = new Vector2(-65.6f, -59.8f), yawAncora = -5f,
                    guia = new Vector2(-64.1f, -58.0f), painel = new Vector2(-65.77f, -57.81f), raio = 2.5f
                },
                new DadosParada
                {
                    codigo = "A2", chave = "coworking", titulo = "Coworking",
                    resumo = "Estações compartilhadas para trabalhar, trocar ideias e fazer contatos.",
                    detalhes = "As etapas do design thinking: imersão, ideação, prototipação e implementação (Sebrae, 2015).",
                    falaAoChegar = "C1", evento = "postit_colado", falaAoConcluir = "C4",
                    ancora = new Vector2(-68.2f, -65.6f), yawAncora = 180f,
                    guia = new Vector2(-69.7f, -66.0f), painel = new Vector2(-66.72f, -67.08f), raio = 2f,
                    balaoAEsquerda = true
                },
                new DadosParada
                {
                    codigo = "A3", chave = "copa", titulo = "Copa e convivência",
                    resumo = "Um espaço para pausas, conversas e reuniões rápidas.",
                    detalhes = "[Completar com o material do professor: mentorias e encontros que acontecem aqui.]",
                    falaAoChegar = "K1", evento = "", falaAoConcluir = "",
                    ancora = new Vector2(-64.7f, -69.2f), yawAncora = -70f,
                    guia = new Vector2(-66.4f, -70.2f), painel = new Vector2(-66.58f, -68.52f), raio = 2f
                },
                new DadosParada
                {
                    codigo = "A4", chave = "fundos", titulo = "Sala dos fundos",
                    resumo = "[Função da sala a confirmar com o material do professor.]",
                    detalhes = "",
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
                    "• ligar os colisores do Sebraelab e marcá-lo como Static;\n" +
                    "• criar (ou atualizar) a cena Visita, com luz própria;\n" +
                    "• montar o guia, os 5 pontos de interesse, o teleporte e as 5 interações;\n" +
                    "• tirar o XR Device Simulator da cena (sem óculos ele aparece sozinho no Play).\n\n" +
                    "Rodar de novo refaz a montagem: ajustes à mão nos objetos gerados se perdem, o roteiro fica.",
                    "Montar", "Cancelar"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ControleVisita controle;
            try
            {
                EditorUtility.DisplayProgressBar("Sebrae Guides", "Ajustando o modelo do Sebraelab...", 0.1f);
                ConfigurarModelo();

                EditorUtility.DisplayProgressBar("Sebrae Guides", "Ajustando o avatar do guia...", 0.2f);
                ConfigurarAvatar();

                EditorUtility.DisplayProgressBar("Sebrae Guides", "Abrindo a cena Visita...", 0.3f);
                var cena = AbrirCenaVisita();
                if (!cena.IsValid()) return;

                GarantirPasta(PastaDados);
                GarantirPasta(PastaMateriais);
                var roteiro = CarregarOuCriarRoteiro(false);
                VincularAudiosDasFalas(roteiro);

                EditorUtility.DisplayProgressBar("Sebrae Guides", "Desligando objetos do template...", 0.45f);
                var antiga = BuscarRaiz(cena, NomeRaiz);
                if (antiga) Object.DestroyImmediate(antiga);
                DesligarSobras(cena);
                RemoverSimuladores(cena);

                var modelo = BuscarRaiz(cena, NomeModeloNaCena);
                if (!modelo)
                {
                    EditorUtility.DisplayDialog("Sebrae Guides",
                        "Não encontrei o objeto 'Sebrae Lab' na raiz da cena. Confira se o modelo está na SampleScene.", "OK");
                    return;
                }

                // Antes de Construir: a porta de vidro (criada lá) volta a não ser Static.
                MarcarModeloComoEstatico(modelo.transform);
                AjustarLuzDaCena(cena, modelo.transform);

                EditorUtility.DisplayProgressBar("Sebrae Guides", "Montando guia, pontos e interações...", 0.6f);
                var raiz = new GameObject(NomeRaiz);
                controle = Construir(raiz.transform, modelo.transform, roteiro);

                EditorUtility.DisplayProgressBar("Sebrae Guides", "Posicionando o visitante...", 0.85f);
                AjustarXROrigin();
                ConfigurarBuild();

                EditorSceneManager.MarkSceneDirty(cena);
                EditorSceneManager.SaveScene(cena);
                GerarLuzSeNecessario(cena);
                Selection.activeGameObject = controle.gameObject;
                EditorGUIUtility.PingObject(controle.gameObject);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("Sebrae Guides",
                "Pronto! A cena Visita foi salva.\n\n" +
                "Para testar: conecte o Quest pelo Meta Quest Link e aperte Play.\n" +
                "Os textos das falas ficam em Assets/_SebraeGuides/Dados/RoteiroGuia.asset.\n\n" +
                "Veja o Console (Window > General > Console) para o resumo do que foi feito.", "OK");
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
            AplicarNomeDoApp();
            EditorUtility.DisplayDialog("Sebrae Guides",
                "Nome do app: Sebraelab 360\nPacote Android: com.SebraeGuides.Sebraelab360", "OK");
        }

        // Sem isto o APK sai como com.DefaultCompany.VRTemplate, e outro projeto do template no mesmo óculos substitui o nosso.
        internal static void AplicarNomeDoApp()
        {
            PlayerSettings.companyName = "Sebrae Guides";
            PlayerSettings.productName = "Sebraelab 360";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.SebraeGuides.Sebraelab360");
        }

        [MenuItem("Sebrae Guides/Refazer a luz da Visita (bake)", false, 22)]
        static void RefazerLuz()
        {
            var cena = SceneManager.GetActiveScene();
            if (cena.path != CenaVisita)
            {
                EditorUtility.DisplayDialog("Sebrae Guides", "Abra a cena Visita primeiro (item 2 do menu).", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try { GerarLuz(cena); }
            finally { EditorUtility.ClearProgressBar(); }
        }

        [MenuItem("Sebrae Guides/Editar as falas do guia", false, 20)]
        static void SelecionarRoteiro()
        {
            GarantirPasta(PastaDados);
            var roteiro = CarregarOuCriarRoteiro(false);
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
            Selection.activeObject = CarregarOuCriarRoteiro(true);
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
                Debug.Log("[Sebrae Guides] Modelo reimportado: colisores ligados, câmera e luzes do Blender removidas, UVs de lightmap geradas.");
            }
        }

        // Animações que repetem sem parar. O aceno (Wave_Welcome) toca uma vez.
        static readonly string[] ClipesEmLoop = { "Idle_Tour", "Talk_Explain", "Walk_Tour" };

        static void ConfigurarAvatar()
        {
            var importador = AssetImporter.GetAtPath(CaminhoAvatar) as ModelImporter;
            if (importador == null)
            {
                Debug.LogWarning($"[Sebrae Guides] Não encontrei {CaminhoAvatar}. O guia vai usar a cápsula provisória.");
                return;
            }

            bool mudou = false;
            if (importador.animationType != ModelImporterAnimationType.Generic) { importador.animationType = ModelImporterAnimationType.Generic; mudou = true; }
            if (importador.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel) { importador.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; mudou = true; }
            if (!importador.importAnimation) { importador.importAnimation = true; mudou = true; }
            if (importador.importCameras) { importador.importCameras = false; mudou = true; }
            if (importador.importLights) { importador.importLights = false; mudou = true; }

            // Nomes curtos ("Professor|Idle_Tour" vira "Idle_Tour") e loop nas animações contínuas.
            var clipes = importador.defaultClipAnimations;
            foreach (var clipe in clipes)
            {
                string nome = clipe.takeName;
                int barra = nome.LastIndexOf('|');
                if (barra >= 0) nome = nome.Substring(barra + 1);
                clipe.name = nome;
                clipe.loopTime = System.Array.IndexOf(ClipesEmLoop, nome) >= 0;
            }

            var atuais = importador.clipAnimations;
            bool clipesIguais = atuais != null && atuais.Length == clipes.Length &&
                                clipes.All(c => atuais.Any(a => a.name == c.name && a.takeName == c.takeName && a.loopTime == c.loopTime));
            if (!clipesIguais && clipes.Length > 0)
            {
                importador.clipAnimations = clipes;
                mudou = true;
            }

            if (mudou)
            {
                importador.SaveAndReimport();
                Debug.Log($"[Sebrae Guides] Avatar do guia reimportado: {string.Join(", ", clipes.Select(c => c.name))}.");
            }
        }

        static AnimationClip BuscarClipe(List<AnimationClip> clipes, string nome)
        {
            return clipes.Find(c => c.name == nome) ?? clipes.Find(c => c.name.EndsWith("|" + nome));
        }

        /// <summary>Cria o Animator do guia: Parado (padrão), Falando e Acenando.</summary>
        static RuntimeAnimatorController CriarAnimador()
        {
            var clipes = AssetDatabase.LoadAllAssetsAtPath(CaminhoAvatar).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToList();
            var parado = BuscarClipe(clipes, "Idle_Tour");
            var falando = BuscarClipe(clipes, "Talk_Explain");
            var acenando = BuscarClipe(clipes, "Wave_Welcome");
            if (!parado)
            {
                Debug.LogWarning("[Sebrae Guides] O avatar não tem a animação Idle_Tour; o guia vai ficar parado sem animação.");
                return null;
            }

            AssetDatabase.DeleteAsset(CaminhoAnimador);
            var controlador = AnimatorController.CreateAnimatorControllerAtPath(CaminhoAnimador);
            controlador.AddParameter("Falando", AnimatorControllerParameterType.Bool);
            controlador.AddParameter("Acenar", AnimatorControllerParameterType.Trigger);

            var maquina = controlador.layers[0].stateMachine;
            var estadoParado = maquina.AddState("Parado", new Vector3(300f, 0f));
            estadoParado.motion = parado;
            maquina.defaultState = estadoParado;

            AnimatorState estadoFalando = null;
            if (falando)
            {
                estadoFalando = maquina.AddState("Falando", new Vector3(300f, 120f));
                estadoFalando.motion = falando;

                var ida = estadoParado.AddTransition(estadoFalando);
                ida.hasExitTime = false;
                ida.duration = 0.25f;
                ida.AddCondition(AnimatorConditionMode.If, 0f, "Falando");

                var volta = estadoFalando.AddTransition(estadoParado);
                volta.hasExitTime = false;
                volta.duration = 0.3f;
                volta.AddCondition(AnimatorConditionMode.IfNot, 0f, "Falando");
            }

            if (acenando)
            {
                var estadoAcenando = maquina.AddState("Acenando", new Vector3(560f, 60f));
                estadoAcenando.motion = acenando;

                var entrada = maquina.AddAnyStateTransition(estadoAcenando);
                entrada.hasExitTime = false;
                entrada.duration = 0.2f;
                entrada.canTransitionToSelf = false;
                entrada.AddCondition(AnimatorConditionMode.If, 0f, "Acenar");

                if (estadoFalando)
                {
                    var paraFalando = estadoAcenando.AddTransition(estadoFalando);
                    paraFalando.hasExitTime = true;
                    paraFalando.exitTime = 0.9f;
                    paraFalando.duration = 0.25f;
                    paraFalando.AddCondition(AnimatorConditionMode.If, 0f, "Falando");
                }

                var paraParado = estadoAcenando.AddTransition(estadoParado);
                paraParado.hasExitTime = true;
                paraParado.exitTime = 0.9f;
                paraParado.duration = 0.25f;
                if (estadoFalando) paraParado.AddCondition(AnimatorConditionMode.IfNot, 0f, "Falando");
            }

            AssetDatabase.SaveAssets();
            return controlador;
        }

        /// <summary>Coloca o professor dentro de "Modelo", em pé, virado para a frente do guia e com os pés no chão.</summary>
        static Animator ColocarAvatar(Transform modelo)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaminhoAvatar);
            if (!prefab) return null;

            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, modelo.gameObject.scene);
            avatar.name = "Professor Otavio";
            avatar.transform.SetParent(modelo, false);
            avatar.transform.localPosition = Vector3.zero;
            avatar.transform.localRotation = Quaternion.identity;
            AlinharAvatar(avatar.transform, modelo);

            var animador = avatar.GetComponent<Animator>();
            if (!animador) animador = avatar.AddComponent<Animator>();
            animador.runtimeAnimatorController = CriarAnimador();
            animador.applyRootMotion = false;
            animador.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            foreach (var colisor in avatar.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(colisor);
            return animador;
        }

        // O FBX vem do Blender; em vez de confiar na conversão de eixos, o avatar é alinhado pelo próprio esqueleto:
        // cabeça acima do quadril, dedos dos pés para a frente do guia e sola no chão.
        static void AlinharAvatar(Transform avatar, Transform referencia)
        {
            var quadril = BuscarNoModelo(avatar, "mixamorig:Hips");
            var cabeca = BuscarNoModelo(avatar, "mixamorig:Head");
            var pe = BuscarNoModelo(avatar, "mixamorig:LeftFoot");
            var dedos = BuscarNoModelo(avatar, "mixamorig:LeftToeBase");

            if (quadril && cabeca)
            {
                Vector3 cima = cabeca.position - quadril.position;
                if (cima.sqrMagnitude > 1e-6f)
                    avatar.rotation = Quaternion.FromToRotation(cima.normalized, referencia.up) * avatar.rotation;
            }

            if (pe && dedos)
            {
                Vector3 frente = Vector3.ProjectOnPlane(dedos.position - pe.position, referencia.up);
                if (frente.sqrMagnitude > 1e-6f)
                    avatar.RotateAround(avatar.position, referencia.up, Vector3.SignedAngle(frente, referencia.forward, referencia.up));
            }

            var limites = Limites(avatar);
            if (limites.size.y > 0.01f && (limites.size.y < 1f || limites.size.y > 2.2f))
            {
                avatar.localScale *= AlturaAvatar / limites.size.y;
                limites = Limites(avatar);
            }
            if (limites.size.y > 0.01f) avatar.position += referencia.up * (referencia.position.y - limites.min.y);

            Debug.Log($"[Sebrae Guides] Avatar do guia colocado: altura {limites.size.y:0.00} m.");
        }

        static Bounds Limites(Transform raiz)
        {
            var renderers = raiz.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(raiz.position, Vector3.zero);
            var limites = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) limites.Encapsulate(renderers[i].bounds);
            return limites;
        }

        static Scene AbrirCenaVisita()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CenaVisita) == null)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CenaOrigem) == null)
                {
                    EditorUtility.DisplayDialog("Sebrae Guides", $"Não encontrei {CenaOrigem}.", "OK");
                    return default;
                }
                if (!AssetDatabase.CopyAsset(CenaOrigem, CenaVisita))
                {
                    EditorUtility.DisplayDialog("Sebrae Guides", "Não consegui copiar a SampleScene.", "OK");
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

        static void RemoverSimuladores(Scene cena)
        {
            var removidos = new List<string>();
            foreach (var objeto in cena.GetRootGameObjects())
            {
                foreach (var componente in objeto.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (!componente || System.Array.IndexOf(TiposDeSimulador, componente.GetType().Name) < 0) continue;
                    var alvo = componente.gameObject;
                    removidos.Add(alvo.name);
                    if (alvo.transform.parent == null)
                    {
                        Object.DestroyImmediate(alvo); // objeto da raiz (o caso do projeto): apaga inteiro
                        break;
                    }
                    alvo.SetActive(false); // dentro de outro objeto: só desliga
                }
            }

            if (removidos.Count > 0)
                Debug.Log($"[Sebrae Guides] Simulador tirado da cena: {string.Join(", ", removidos)}. Sem óculos, ele aparece sozinho no Play.");
        }

        // Static no Sebraelab: o Unity junta as malhas fixas (menos chamadas de desenho no Quest) e a sonda de reflexo
        // passa a enxergar o interior. Sem "Contribute GI": o bake não gera lightmaps (o modelo não tem UV de lightmap).
        static void MarcarModeloComoEstatico(Transform modelo)
        {
            const StaticEditorFlags estatico = StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic;
            int marcados = 0;
            MarcarEstatico(modelo, estatico, ref marcados);
            Debug.Log($"[Sebrae Guides] Sebraelab marcado como Static (Batching e Reflection Probe): {marcados} objeto(s) alterado(s). Porta de vidro fica móvel.");
        }

        static void MarcarEstatico(Transform objeto, StaticEditorFlags estatico, ref int marcados)
        {
            bool movel = System.Array.IndexOf(PartesMoveis, objeto.name.Trim()) >= 0;
            if (movel)
            {
                foreach (var parte in objeto.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(parte.gameObject, 0);
                return;
            }

            if (GameObjectUtility.GetStaticEditorFlags(objeto.gameObject) != estatico)
            {
                GameObjectUtility.SetStaticEditorFlags(objeto.gameObject, estatico);
                marcados++;
            }
            foreach (Transform filho in objeto) MarcarEstatico(filho, estatico, ref marcados);
        }

        // A Visita nasceu da SampleScene e herdou a luz pré-calculada de lá: as sondas de luz e de reflexo ficam na sala
        // do template, a uns 85 m do Sebraelab, e eram elas que iluminavam o guia, as mãos e o crachá.
        // Aqui: sondas de luz desligadas (sem bake de GI, o guia usa a luz ambiente do céu, como o resto da cena),
        // sonda de reflexo levada para dentro do Sebraelab e o sol sem sombra. A sombra do teto só era calculada perto
        // do visitante (2,5 m no Quest, 10 m no PC) e fazia um círculo escuro em volta de quem está de óculos.
        static void AjustarLuzDaCena(Scene cena, Transform modelo)
        {
            const string NomeSondaReflexo = "Reflection Probe (Sebraelab)";
            var limites = LimitesVisiveis(modelo);
            var sondas = new List<ReflectionProbe>();
            int sondasLuz = 0, sois = 0;

            foreach (var raiz in cena.GetRootGameObjects())
            {
                if (raiz.name == NomeRaiz || !raiz.activeSelf) continue;

                foreach (var grupo in raiz.GetComponentsInChildren<LightProbeGroup>(true))
                {
                    if (!grupo.gameObject.activeInHierarchy || !grupo.enabled) continue;
                    if (grupo.GetComponents<Component>().Length == 2) grupo.gameObject.SetActive(false);
                    else grupo.enabled = false;
                    sondasLuz++;
                }

                sondas.AddRange(raiz.GetComponentsInChildren<ReflectionProbe>(true));

                foreach (var luz in raiz.GetComponentsInChildren<Light>(true))
                {
                    if (luz.type != LightType.Directional || luz.shadows == LightShadows.None) continue;
                    luz.shadows = LightShadows.None;
                    EditorUtility.SetDirty(luz);
                    sois++;
                }
            }

            var principal = sondas.Find(s => s.name == NomeSondaReflexo) ??
                            sondas.Find(s => s.gameObject.activeInHierarchy) ??
                            (sondas.Count > 0 ? sondas[0] : null);
            foreach (var sonda in sondas)
            {
                if (sonda != principal)
                {
                    if (sonda.gameObject.activeSelf && sonda.GetComponents<Component>().Length == 2) sonda.gameObject.SetActive(false);
                    else sonda.enabled = false;
                    continue;
                }
                if (limites.size == Vector3.zero) continue;
                sonda.gameObject.SetActive(true);
                sonda.enabled = true;
                sonda.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
                sonda.boxProjection = false;
                sonda.transform.position = new Vector3(limites.center.x, Piso + 1.6f, limites.center.z);
                sonda.size = limites.size + Vector3.one;
                sonda.center = limites.center - sonda.transform.position;
                sonda.name = NomeSondaReflexo;
                EditorUtility.SetDirty(sonda);
            }

            Debug.Log($"[Sebrae Guides] Luz da cena: {sondasLuz} grupo(s) de sondas de luz do template desligado(s); " +
                      (principal ? $"sonda de reflexo levada para o Sebraelab ({limites.size.x:0} x {limites.size.z:0} m); " : "nenhuma sonda de reflexo; ") +
                      $"sombra do sol desligada em {sois} luz(es).");
        }

        static Bounds LimitesVisiveis(Transform raiz)
        {
            bool achou = false;
            var limites = new Bounds();
            foreach (var renderer in raiz.GetComponentsInChildren<Renderer>(false))
            {
                if (!renderer.enabled) continue;
                if (!achou) { limites = renderer.bounds; achou = true; }
                else limites.Encapsulate(renderer.bounds);
            }
            return limites;
        }

        // Na primeira montagem (ou se a cena ainda usa a luz da SampleScene), calcula a luz da própria Visita.
        // Sem objetos com "Contribute GI", o bake é rápido: só a luz ambiente do céu e a sonda de reflexo.
        static void GerarLuzSeNecessario(Scene cena)
        {
            var dados = Lightmapping.lightingDataAsset;
            string caminho = dados ? AssetDatabase.GetAssetPath(dados) : string.Empty;
            if (caminho.StartsWith(PastaLuzVisita + "/")) return;
            GerarLuz(cena);
        }

        static void GerarLuz(Scene cena)
        {
            EditorUtility.DisplayProgressBar("Sebrae Guides", "Calculando a luz da Visita (bake)...", 0.95f);
            bool terminou = Lightmapping.Bake();
            EditorSceneManager.MarkSceneDirty(cena);
            EditorSceneManager.SaveScene(cena);
            var dados = Lightmapping.lightingDataAsset;
            if (terminou && dados)
                Debug.Log($"[Sebrae Guides] Luz da Visita calculada: {AssetDatabase.GetAssetPath(dados)}.");
            else
                Debug.LogWarning("[Sebrae Guides] O cálculo da luz não terminou. Use Window > Rendering > Lighting > Generate Lighting com a cena Visita aberta.");
        }

        const string PastaFalas = Pasta + "/Audio/Falas";

        // Vozes: cada arquivo Audio/Falas/<ID>.ogg (ou .mp3 / .wav) é ligado à fala de mesmo ID no roteiro.
        // Falas sem arquivo continuam só com texto. Os textos do roteiro não são alterados.
        static void VincularAudiosDasFalas(RoteiroGuia roteiro)
        {
            if (!roteiro || roteiro.falas == null || !AssetDatabase.IsValidFolder(PastaFalas)) return;

            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { PastaFalas }))
            {
                var importador = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as AudioImporter;
                if (importador == null) continue;
                var config = importador.defaultSampleSettings;
                if (importador.forceToMono && config.loadType == AudioClipLoadType.DecompressOnLoad &&
                    config.compressionFormat == AudioCompressionFormat.Vorbis) continue;
                importador.forceToMono = true; // voz espacial: mono basta e o APK fica menor
                config.loadType = AudioClipLoadType.DecompressOnLoad;
                config.compressionFormat = AudioCompressionFormat.Vorbis;
                config.quality = 0.6f;
                importador.defaultSampleSettings = config;
                importador.SaveAndReimport();
            }

            int ligadas = 0;
            foreach (var fala in roteiro.falas)
            {
                if (fala == null || string.IsNullOrEmpty(fala.id)) continue;
                AudioClip clipe = null;
                foreach (var extensao in new[] { ".ogg", ".mp3", ".wav" })
                {
                    clipe = AssetDatabase.LoadAssetAtPath<AudioClip>($"{PastaFalas}/{fala.id}{extensao}");
                    if (clipe) break;
                }
                if (!clipe) continue;
                fala.audio = clipe;
                ligadas++;
            }

            EditorUtility.SetDirty(roteiro);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Sebrae Guides] Vozes: {ligadas} de {roteiro.falas.Count} falas com áudio ({PastaFalas}).");
        }

        static RoteiroGuia CarregarOuCriarRoteiro(bool restaurar)
        {
            var roteiro = AssetDatabase.LoadAssetAtPath<RoteiroGuia>(CaminhoRoteiro);
            if (roteiro && !restaurar) return roteiro;

            if (!roteiro)
            {
                roteiro = ScriptableObject.CreateInstance<RoteiroGuia>();
                RoteiroPadrao.Preencher(roteiro);
                AssetDatabase.CreateAsset(roteiro, CaminhoRoteiro);
            }
            else
            {
                RoteiroPadrao.Preencher(roteiro);
                EditorUtility.SetDirty(roteiro);
            }
            AssetDatabase.SaveAssets();
            return roteiro;
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
            CriarCameraDoProjetor(raiz);

            EditorUtility.SetDirty(controle);
            Debug.Log("[Sebrae Guides] Visita montada: guia, 5 POIs, 6 pontos de teleporte, crachá, TV, post-it, mural e porta.");
            return controle;
        }

        static ParadaVisita Parada(ControleVisita controle, string chave)
        {
            return controle.paradas.Find(p => p.chave == chave);
        }

        static DialogoGuia CriarGuia(Transform raiz, RoteiroGuia roteiro, AudioClip clique, out GuiaNPC guia)
        {
            var objeto = Filho(raiz, "Guia").gameObject;

            var modelo = Filho(objeto.transform, "Modelo");
            var animador = ColocarAvatar(modelo);
            if (!animador)
            {
                // Sem o FBX do professor: cápsula provisória.
                var corpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                corpo.name = "Corpo";
                corpo.transform.SetParent(modelo, false);
                corpo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                corpo.transform.localScale = new Vector3(0.5f, 0.85f, 0.5f);
                Object.DestroyImmediate(corpo.GetComponent<Collider>());
                Pintar(corpo, Mat("Guia_Corpo", new Color(0.0f, 0.37f, 0.72f)));

                var visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visor.name = "Visor";
                visor.transform.SetParent(modelo, false);
                visor.transform.localPosition = new Vector3(0f, 1.48f, 0.21f);
                visor.transform.localScale = new Vector3(0.34f, 0.1f, 0.08f);
                Object.DestroyImmediate(visor.GetComponent<Collider>());
                Pintar(visor, Mat("Guia_Visor", new Color(0.05f, 0.05f, 0.08f)));
            }

            // O guia não é interagível de propósito: como alvo grande, ele "roubava" o raio do controle
            // (o raio de longe procura alvos num cone) e impedia de apertar os botões do balão e de pegar o crachá.

            guia = objeto.AddComponent<GuiaNPC>();
            guia.modelo = modelo;

            // A voz sai da altura da cabeça do guia (o objeto Guia fica no chão).
            var pontoDaVoz = Filho(objeto.transform, "Voz");
            pontoDaVoz.localPosition = new Vector3(0f, 1.35f, 0f);
            var voz = pontoDaVoz.gameObject.AddComponent<AudioSource>();
            voz.playOnAwake = false;
            voz.spatialBlend = 1f;
            voz.minDistance = 1f;
            voz.maxDistance = 15f;
            voz.rolloffMode = AudioRolloffMode.Linear;

            var dialogo = objeto.AddComponent<DialogoGuia>();
            dialogo.roteiro = roteiro;
            dialogo.voz = voz;
            dialogo.somClique = clique;

            if (animador && animador.runtimeAnimatorController)
            {
                var animacao = objeto.AddComponent<AnimacaoGuia>();
                animacao.animador = animador;
                animacao.dialogo = dialogo;
            }

            // O balão fica ao lado do guia, na altura dos olhos e um pouco à frente dele.
            // Em cima da cabeça, o corpo do guia ficava na frente dos botões e bloqueava o raio do controle.
            var pivoBalao = Filho(objeto.transform, "Balao (pivo)");
            pivoBalao.gameObject.AddComponent<ViradoParaCamera>();
            var canvas = CriarCanvas("Balao", pivoBalao, new Vector2(900f, 480f));
            canvas.transform.localPosition = new Vector3(0.75f, 1.55f, -0.25f);
            var rt = (RectTransform)canvas.transform;

            Imagem("Fundo", rt, CorFundo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
            dialogo.textoNome = Texto("Nome", rt, roteiro ? roteiro.nomeDoGuia : "Guia", 34f, CorDestaque, TextAlignmentOptions.TopLeft,
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
            // De pé num suporte na borda do balcão (tampo a 0,731 m, borda do lado do visitante em z ≈ -62,02),
            // virado para o ponto de chegada A0. Deitado no meio do tampo, o crachá ocupava menos de 1 grau
            // na vista do visitante e o raio batia no balcão antes dele. Assim ocupa cerca de 8 graus.
            const float YawCracha = 160.6f;
            var suporte = GameObject.CreatePrimitive(PrimitiveType.Cube);
            suporte.name = "I2 Suporte do cracha";
            suporte.transform.SetParent(pai, false);
            suporte.transform.position = new Vector3(-59.30f, 0.738f, -62.16f);
            suporte.transform.rotation = Quaternion.Euler(0f, YawCracha, 0f);
            suporte.transform.localScale = new Vector3(0.16f, 0.014f, 0.08f);
            Object.DestroyImmediate(suporte.GetComponent<Collider>());
            Pintar(suporte, Mat("Metal", new Color(0.75f, 0.77f, 0.8f)));

            var raiz = Filho(pai, "I2 Cracha de visitante");
            raiz.position = new Vector3(-59.30f, 0.826f, -62.15f);
            raiz.rotation = Quaternion.Euler(15f, YawCracha, 0f); // inclinado 15 graus para trás, como num porta-crachá

            // A parte visual fica separada para crescer quando o raio aponta (DestaqueAoApontar).
            var visual = Filho(raiz, "Visual");

            var corpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corpo.name = "Corpo";
            corpo.transform.SetParent(visual, false);
            corpo.transform.localScale = new Vector3(0.12f, 0.16f, 0.012f);
            Object.DestroyImmediate(corpo.GetComponent<Collider>());
            Pintar(corpo, Mat("Cracha", new Color(0.93f, 0.95f, 0.98f)));

            var faixa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            faixa.name = "Faixa";
            faixa.transform.SetParent(visual, false);
            faixa.transform.localPosition = new Vector3(0f, 0.062f, 0f);
            faixa.transform.localScale = new Vector3(0.12f, 0.036f, 0.013f);
            Object.DestroyImmediate(faixa.GetComponent<Collider>());
            Pintar(faixa, Mat("Destaque", CorDestaque));

            var rotulo = new GameObject("Texto", typeof(RectTransform));
            rotulo.transform.SetParent(visual, false);
            rotulo.transform.localPosition = new Vector3(0f, -0.012f, -0.0068f);
            var texto = rotulo.AddComponent<TextMeshPro>();
            texto.text = "VISITANTE";
            texto.fontSize = 0.19f;
            texto.fontStyle = FontStyles.Bold;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = new Color(0.07f, 0.11f, 0.19f);
            ((RectTransform)rotulo.transform).sizeDelta = new Vector2(0.12f, 0.05f);

            // Área de pegar maior que o crachá (20 x 24 cm). Como ele é guardado ao ser solto,
            // a caixa maior não atrapalha nenhuma física.
            var area = raiz.gameObject.AddComponent<BoxCollider>();
            area.size = new Vector3(0.20f, 0.24f, 0.06f);

            var corpoFisico = raiz.gameObject.AddComponent<Rigidbody>();
            corpoFisico.mass = 0.05f;
            corpoFisico.isKinematic = true;
            corpoFisico.useGravity = false;
            var pegar = raiz.gameObject.AddComponent<XRGrabInteractable>();
            pegar.throwOnDetach = false;
            pegar.useDynamicAttach = true;
            // Vem para a mão quando pego de longe. No padrão do template o objeto fica na ponta do raio,
            // e assim o post-it não chegaria até o mural.
            pegar.farAttachMode = InteractableFarAttachMode.Near;
            var gatilho = raiz.gameObject.AddComponent<GatilhoInteracao>();
            gatilho.evento = "pegou_cracha";
            gatilho.som = clique;
            raiz.gameObject.AddComponent<DestaqueAoApontar>().alvo = visual;
            raiz.gameObject.AddComponent<GuardarAoSoltar>();
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
            var legenda = Texto("Legenda", crt, "", 44f, CorTextoClaro, TextAlignmentOptions.Bottom,
                Vector2.zero, Vector2.one, new Vector2(24f, 18f), new Vector2(-24f, -24f));

            var tela = canvas.gameObject.AddComponent<TelaTV>();
            tela.tela = conteudo;
            tela.imagem = foto;
            tela.legenda = legenda;
            tela.som = clique;

            var fotos = CarregarFotosTV(out var legendas);
            if (fotos.Length > 0)
            {
                tela.fotos = fotos;
                tela.legendas = legendas;
            }
            else
            {
                tela.legendas = new[]
                {
                    "Palestra de abertura\n(troque por uma foto real)",
                    "Oficina de inovação\n(troque por uma foto real)",
                    "Apresentação de projetos\n(troque por uma foto real)",
                };
            }
            return tela;
        }

        const string PastaFotos = Pasta + "/Fotos";

        // Fotos da TV: todas as imagens da pasta Fotos, em ordem de nome (TV_01, TV_02...).
        // Legendas em Fotos/legendas.txt, uma linha por foto, no formato "legenda | crédito".
        static Sprite[] CarregarFotosTV(out string[] legendas)
        {
            legendas = new string[0];
            if (!AssetDatabase.IsValidFolder(PastaFotos)) return new Sprite[0];

            var caminhos = AssetDatabase.FindAssets("t:Texture2D", new[] { PastaFotos })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(c => c, System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            var fotos = new List<Sprite>();
            foreach (var caminho in caminhos)
            {
                var importador = AssetImporter.GetAtPath(caminho) as TextureImporter;
                if (importador != null && (importador.textureType != TextureImporterType.Sprite || !importador.mipmapEnabled))
                {
                    importador.textureType = TextureImporterType.Sprite;
                    importador.spriteImportMode = SpriteImportMode.Single;
                    importador.mipmapEnabled = true; // evita a imagem "piscar" de longe no óculos
                    importador.SaveAndReimport();
                }
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
                if (sprite) fotos.Add(sprite);
            }

            var linhas = new List<string>();
            var arquivo = AssetDatabase.LoadAssetAtPath<TextAsset>(PastaFotos + "/legendas.txt");
            if (arquivo)
            {
                foreach (var bruta in arquivo.text.Split('\n'))
                {
                    string linha = bruta.Trim();
                    if (linha.Length == 0 || linha.StartsWith("#")) continue;
                    int barra = linha.IndexOf('|');
                    linhas.Add(barra < 0
                        ? linha
                        : $"{linha.Substring(0, barra).Trim()}\n<size=60%>{linha.Substring(barra + 1).Trim()}</size>");
                }
            }

            legendas = new string[fotos.Count];
            for (int i = 0; i < fotos.Count; i++) legendas[i] = i < linhas.Count ? linhas[i] : string.Empty;

            Debug.Log($"[Sebrae Guides] TV da arena: {fotos.Count} foto(s) e {System.Math.Min(linhas.Count, fotos.Count)} legenda(s) de {PastaFotos}.");
            return fotos.ToArray();
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
            // Vem para a mão quando pego de longe. No padrão do template o objeto fica na ponta do raio,
            // e assim o post-it não chegaria até o mural.
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
                foreach (var parte in vidro.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(parte.gameObject, 0);
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

        // Câmera estável para o projetor: só liga com o óculos no Link (no simulador e no APK fica desligada).
        // Com a janela Game em foco: F1 primeira pessoa suave, F2 terceira pessoa, F3 esta posição fixa, F4 liga/desliga.
        static void CriarCameraDoProjetor(Transform raiz)
        {
            var objeto = Filho(raiz, "Camera do projetor (so com oculos no Link)");
            objeto.position = new Vector3(-60.05f, 2.0f, -59.95f); // atrás da recepção (A0)
            objeto.rotation = Quaternion.LookRotation(new Vector3(-58.9f, 1.3f, -63.6f) - objeto.position);
            var camera = objeto.gameObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.enabled = false; // a CameraEspectador liga quando há óculos
            objeto.gameObject.AddComponent<SebraeGuides.Apresentacao.CameraEspectador>();
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

        // Alguns nomes do FBX vêm com espaço no fim ("Entrada "), por isso a comparação ignora espaços.
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
            string caminho = $"{PastaMateriais}/{nome}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (material) return material;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader) { name = nome };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", cor);
            else material.color = cor;
            AssetDatabase.CreateAsset(material, caminho);
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
            // A cor fica no Image e o ColorBlock só escurece. Assim nenhum estado do botão fica branco
            // (o "Recomeçar" aparecia branco com texto branco).
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
