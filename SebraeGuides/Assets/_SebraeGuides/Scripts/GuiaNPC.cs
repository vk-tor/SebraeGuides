using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SebraeGuides
{
    /// <summary>
    /// Controla o Professor Otávio (NPC Guia 3D):
    /// - vira suavemente para o visitante;
    /// - troca de lugar entre as paradas da visita;
    /// - reproduz e faz loop das animações do esqueleto Mixamo (Idle_Tour, Wave_Welcome, Talk_Explain, Walk_Tour)
    ///   sincronizadas com a fala (AudioSource) e com a troca de paradas.
    /// </summary>
    public class GuiaNPC : MonoBehaviour
    {
        [Tooltip("Parte visual do guia. Encolhe e cresce na troca de lugar.")]
        public Transform modelo;

        [Tooltip("Objeto raiz da instância do FBX que recebe SampleAnimation / Animator.")]
        public GameObject alvoAnimacao;

        [Tooltip("Animator configurado com ProfessorOtavio.controller.")]
        public Animator animador;

        [Tooltip("Fonte de áudio da voz do guia (usada para animar gestos enquanto fala).")]
        public AudioSource voz;

        [Header("Clipes de Animação (Mixamo / FBX)")]
        public AnimationClip clipeIdle;
        public AnimationClip clipeFala;
        public AnimationClip clipeAceno;
        public AnimationClip clipeAndar;

        [Header("Movimento e Olhar")]
        public bool olharParaVisitante = true;
        public float velocidadeGiro = 5f;
        public float duracaoTroca = 0.35f;

        public event Action AoSelecionar;

        XRBaseInteractable interagivel;
        Transform cabeca;
        Vector3 escalaOriginal = Vector3.one;
        Coroutine troca;

        float tempoAceno;
        float tempoFalaManual;
        bool emTransicao;

        static readonly int HashFalando = Animator.StringToHash("Falando");
        static readonly int HashAndando = Animator.StringToHash("Andando");
        static readonly int HashAcenar = Animator.StringToHash("Acenar");

        public bool EstaFalando => (voz != null && voz.isPlaying) || tempoFalaManual > 0f;

        void Awake()
        {
            interagivel = GetComponentInChildren<XRBaseInteractable>(true);
            if (!voz) voz = GetComponent<AudioSource>();
            if (!animador) animador = GetComponentInChildren<Animator>(true);
            if (!alvoAnimacao && animador) alvoAnimacao = animador.gameObject;
            if (modelo) escalaOriginal = modelo.localScale;
        }

        void OnEnable()
        {
            if (interagivel) interagivel.selectEntered.AddListener(AoSerSelecionado);
        }

        void OnDisable()
        {
            if (interagivel) interagivel.selectEntered.RemoveListener(AoSerSelecionado);
        }

        void AoSerSelecionado(SelectEnterEventArgs args)
        {
            AoSelecionar?.Invoke();
        }

        /// <summary>Aplica uma pose dos clipes do FBX no Editor (antes do Play) ou em capturas de tela.</summary>
        public void AplicarPoseNoEditor(float tempoNormalizado = 0.25f, float aceno = 0f, float fala = 0f)
        {
            var alvo = alvoAnimacao ? alvoAnimacao : (animador ? animador.gameObject : (modelo ? modelo.gameObject : gameObject));
            if (!alvo) return;

            AnimationClip clipe = clipeIdle;
            if (aceno > 0.5f && clipeAceno) clipe = clipeAceno;
            else if (fala > 0.5f && clipeFala) clipe = clipeFala;

            if (clipe && clipe.length > 0.001f)
            {
                float t = Mathf.Clamp01(tempoNormalizado) * clipe.length;
                clipe.SampleAnimation(alvo, t);
            }
        }

        public void Acenar(float duracao = 2.5f)
        {
            tempoAceno = Mathf.Max(tempoAceno, duracao);
            if (animador && animador.runtimeAnimatorController)
            {
                animador.SetTrigger(HashAcenar);
            }
        }

        public void IndicarFala(float duracaoMinima = 3.5f)
        {
            tempoFalaManual = Mathf.Max(tempoFalaManual, duracaoMinima);
        }

        public void Posicionar(Transform alvo)
        {
            if (!alvo) return;
            transform.SetPositionAndRotation(alvo.position, alvo.rotation);
        }

        public void IrPara(Transform alvo)
        {
            if (!alvo) return;
            if (troca != null) StopCoroutine(troca);
            troca = StartCoroutine(Trocar(alvo));
        }

        IEnumerator Trocar(Transform alvo)
        {
            emTransicao = true;
            yield return Escalar(1f, 0f);
            Posicionar(alvo);
            yield return Escalar(0f, 1f);
            emTransicao = false;
            troca = null;
        }

        IEnumerator Escalar(float de, float para)
        {
            if (!modelo) yield break;
            float tempo = 0f;
            while (tempo < duracaoTroca)
            {
                tempo += Time.deltaTime;
                float k = Mathf.Clamp01(tempo / duracaoTroca);
                modelo.localScale = escalaOriginal * Mathf.SmoothStep(de, para, k);
                yield return null;
            }
            modelo.localScale = escalaOriginal * para;
        }

        void Update()
        {
            if (tempoAceno > 0f) tempoAceno -= Time.deltaTime;
            if (tempoFalaManual > 0f) tempoFalaManual -= Time.deltaTime;

            bool falandoAgora = EstaFalando;

            if (animador && animador.runtimeAnimatorController)
            {
                animador.SetBool(HashFalando, falandoAgora);
                animador.SetBool(HashAndando, emTransicao);
            }

            if (!olharParaVisitante) return;
            if (!cabeca)
            {
                var cam = Camera.main;
                if (!cam) return;
                cabeca = cam.transform;
            }

            Vector3 direcao = cabeca.position - transform.position;
            direcao.y = 0f;
            if (direcao.sqrMagnitude < 0.01f) return;

            var alvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, alvo, Time.deltaTime * velocidadeGiro);
        }

        void LateUpdate()
        {
            var alvo = alvoAnimacao ? alvoAnimacao : (animador ? animador.gameObject : null);
            if (!alvo) return;

            AnimationClip clipe = clipeIdle;
            if (tempoAceno > 0f && clipeAceno) clipe = clipeAceno;
            else if (emTransicao && clipeAndar) clipe = clipeAndar;
            else if (EstaFalando && clipeFala) clipe = clipeFala;

            if (clipe && clipe.length > 0.001f)
            {
                float t = Mathf.Repeat(Time.time, clipe.length);
                clipe.SampleAnimation(alvo, t);
            }
        }
    }
}
