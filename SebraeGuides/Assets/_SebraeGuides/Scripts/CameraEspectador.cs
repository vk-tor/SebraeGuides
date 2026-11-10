using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SebraeGuides.Apresentacao
{
    /// <summary>
    /// Versão 2. Câmera para o projetor quando o Quest está ligado ao PC (Meta Horizon Link) e a cena roda no Play Mode.
    /// O óculos continua recebendo a imagem normal; a janela do PC mostra uma visão estável (sem tremor de cabeça),
    /// mais confortável para a plateia.
    /// Sem óculos (XR Device Simulator ou só mouse e teclado) ela fica desligada sozinha, para não atrapalhar quem testa.
    /// Atalhos (com a janela Game em foco): F1 primeira pessoa suave · F2 terceira pessoa · F3 câmera fixa · F4 liga/desliga.
    /// No APK do Quest ela se desliga sozinha (não existe monitor).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Sebrae Guides/Câmera de Espectador (projetor)")]
    public class CameraEspectador : MonoBehaviour
    {
        public enum Modo { PrimeiraPessoaSuave = 0, TerceiraPessoa = 1, Fixa = 2 }

        [SerializeField] Modo modo = Modo.PrimeiraPessoaSuave;
        [Tooltip("Cabeça do visitante (XR Origin > Main Camera). Se vazio, usa Camera.main.")]
        [SerializeField] Transform cabecaDoVisitante;
        [SerializeField, Range(40f, 100f)] float campoDeVisao = 75f;
        [Tooltip("Quanto maior, mais rápido a câmera acompanha a posição da cabeça.")]
        [SerializeField, Min(0.5f)] float suavidadeDaPosicao = 6f;
        [Tooltip("Quanto menor, mais suave (e atrasada) fica a rotação.")]
        [SerializeField, Min(0.5f)] float suavidadeDaRotacao = 3f;
        [Tooltip("Posição da câmera em terceira pessoa, relativa à cabeça (x, altura, distância para trás).")]
        [SerializeField] Vector3 deslocamentoTerceiraPessoa = new Vector3(0f, 0.5f, -1.8f);
        [Tooltip("Desliga o espelho do óculos na janela do PC enquanto esta câmera estiver ativa.")]
        [SerializeField] bool desligarEspelhoDoOculos = true;
        [Tooltip("Liga só quando há um óculos rodando no PC (Link). Com o XR Device Simulator a janela Game continua com a câmera normal. F4 passa para o controle manual.")]
        [SerializeField] bool somenteComOculos = true;
        [SerializeField] bool atalhosDeTeclado = true;

        Camera cam;
        bool iniciada;
        bool ativa;
        bool controleManual;
        bool espelhoAlterado;
        GameViewRenderMode modoDoEspelhoOriginal;
        Vector3 posicaoFixa;
        Quaternion rotacaoFixa;

        public Modo ModoAtual => modo;
        public bool Ativa => ativa;

        void Awake()
        {
            cam = GetComponent<Camera>();
#if UNITY_ANDROID && !UNITY_EDITOR
            // No Quest (APK) não há monitor: desligar evita gastar GPU à toa.
            cam.enabled = false;
            enabled = false;
#else
            // No URP, quem tira a câmera do óculos é o allowXRRendering (abaixo); stereoTargetEye só vale no pipeline antigo
            // e, aqui, só gerava um aviso no Console.
            cam.targetDisplay = 0;
            cam.fieldOfView = campoDeVisao;
            UniversalAdditionalCameraData dados = cam.GetUniversalAdditionalCameraData();
            if (dados != null) dados.allowXRRendering = false;
            posicaoFixa = transform.position;
            rotacaoFixa = transform.rotation;
#endif
        }

        void Start()
        {
            float maiorProfundidade = float.MinValue;
            foreach (Camera outra in Camera.allCameras)
            {
                if (outra != cam) maiorProfundidade = Mathf.Max(maiorProfundidade, outra.depth);
            }
            if (maiorProfundidade > float.MinValue) cam.depth = Mathf.Max(cam.depth, maiorProfundidade + 1f);
            iniciada = true;
            DefinirAtiva(DeveriaEstarAtiva());
            Saltar();
        }

        void OnEnable()
        {
            // Religar o componente pelo Inspector volta a mostrar a câmera (se houver óculos ou no controle manual).
            if (iniciada) DefinirAtiva(controleManual || DeveriaEstarAtiva());
        }

        void OnDisable()
        {
            RestaurarEspelho();
            if (cam != null) cam.enabled = false;
            ativa = false;
        }

        void OnValidate()
        {
            Camera c = GetComponent<Camera>();
            if (c != null) c.fieldOfView = campoDeVisao;
        }

        void Update()
        {
            // Modo automático: acompanha o óculos ligar ou desligar (por exemplo, o Link cair no meio da demonstração).
            if (!controleManual)
            {
                bool deveria = DeveriaEstarAtiva();
                if (deveria != ativa)
                {
                    DefinirAtiva(deveria);
                    if (deveria) Saltar();
                }
            }
            if (!atalhosDeTeclado) return;
            if (Pressionou(Key.F1, KeyCode.F1)) DefinirModo(Modo.PrimeiraPessoaSuave);
            if (Pressionou(Key.F2, KeyCode.F2)) DefinirModo(Modo.TerceiraPessoa);
            if (Pressionou(Key.F3, KeyCode.F3)) DefinirModo(Modo.Fixa);
            if (Pressionou(Key.F4, KeyCode.F4))
            {
                controleManual = true;
                DefinirAtiva(!ativa);
                if (ativa) Saltar();
            }
        }

        bool DeveriaEstarAtiva()
        {
            return !somenteComOculos || XRSettings.isDeviceActive;
        }

        void LateUpdate()
        {
            if (!ativa) return;
            Vector3 posicao;
            Quaternion rotacao;
            if (!CalcularAlvo(out posicao, out rotacao)) return;

            // Teleporte do visitante: corta a cena em vez de "voar" pela sala.
            if ((posicao - transform.position).sqrMagnitude > 2.25f)
            {
                transform.SetPositionAndRotation(posicao, rotacao);
                return;
            }
            float kp = 1f - Mathf.Exp(-suavidadeDaPosicao * Time.unscaledDeltaTime);
            float kr = 1f - Mathf.Exp(-suavidadeDaRotacao * Time.unscaledDeltaTime);
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, posicao, kp),
                Quaternion.Slerp(transform.rotation, rotacao, kr));
        }

        public void DefinirModo(Modo novo)
        {
            modo = novo;
            Saltar();
        }

        public void DefinirAtiva(bool valor)
        {
            ativa = valor;
            if (cam != null) cam.enabled = valor;
            if (valor) DesligarEspelho();
            else RestaurarEspelho();
        }

        void Saltar()
        {
            Vector3 posicao;
            Quaternion rotacao;
            if (CalcularAlvo(out posicao, out rotacao)) transform.SetPositionAndRotation(posicao, rotacao);
        }

        bool CalcularAlvo(out Vector3 posicao, out Quaternion rotacao)
        {
            posicao = transform.position;
            rotacao = transform.rotation;
            if (modo == Modo.Fixa)
            {
                posicao = posicaoFixa;
                rotacao = rotacaoFixa;
                return true;
            }
            Transform cabeca = Cabeca();
            if (cabeca == null) return false;

            if (modo == Modo.TerceiraPessoa)
            {
                Quaternion giro = Quaternion.Euler(0f, cabeca.eulerAngles.y, 0f);
                posicao = cabeca.position + giro * deslocamentoTerceiraPessoa;
                Vector3 foco = cabeca.position + giro * Vector3.forward * 1.5f;
                Vector3 olhar = foco - posicao;
                if (olhar.sqrMagnitude > 0.0001f) rotacao = Quaternion.LookRotation(olhar, Vector3.up);
                return true;
            }

            posicao = cabeca.position;
            Vector3 frente = cabeca.forward;
            // Remove a inclinação lateral da cabeça; olhando quase na vertical, mantém a rotação anterior.
            if (Mathf.Abs(Vector3.Dot(frente, Vector3.up)) < 0.97f) rotacao = Quaternion.LookRotation(frente, Vector3.up);
            return true;
        }

        Transform Cabeca()
        {
            if (cabecaDoVisitante != null) return cabecaDoVisitante;
            Camera principal = Camera.main;
            return principal != null && principal != cam ? principal.transform : null;
        }

        void DesligarEspelho()
        {
            if (!desligarEspelhoDoOculos || espelhoAlterado) return;
            modoDoEspelhoOriginal = XRSettings.gameViewRenderMode;
            XRSettings.gameViewRenderMode = GameViewRenderMode.None;
            espelhoAlterado = true;
        }

        void RestaurarEspelho()
        {
            if (!espelhoAlterado) return;
            XRSettings.gameViewRenderMode = modoDoEspelhoOriginal;
            espelhoAlterado = false;
        }

#if ENABLE_INPUT_SYSTEM
        static bool Pressionou(Key tecla, KeyCode teclaAntiga)
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && teclado[tecla].wasPressedThisFrame) return true;
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(teclaAntiga)) return true;
#endif
            return false;
        }
#else
        enum Key { F1, F2, F3, F4 }

        static bool Pressionou(Key tecla, KeyCode teclaAntiga)
        {
            return Input.GetKeyDown(teclaAntiga);
        }
#endif
    }
}
