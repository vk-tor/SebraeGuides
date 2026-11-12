using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SebraeGuides
{
    /// <summary>
    /// Interação I4 (coworking). O visitante escolhe uma ideia nos botões da fala C3 e ela aparece escrita
    /// no post-it, que está no canto do mural. Depois ele cola no centro do mural de um destes jeitos:
    /// - aponta para "Colar aqui" e aperta o gatilho (igual no simulador e no óculos);
    /// - de óculos, pega o post-it com o botão lateral e solta em cima do quadro.
    /// Os dois avisam a visita com o evento "postit_colado" uma única vez.
    /// </summary>
    public class MuralDeIdeias : MonoBehaviour
    {
        /// <summary>Botões da fala C3 enviam "ideia:texto que vai no post-it".</summary>
        public const string PrefixoIdeia = "ideia:";

        [Header("Referências (montadas pelo menu Sebrae Guides)")]
        public DialogoGuia dialogo;
        public PostIt postIt;

        [Tooltip("Onde o post-it fica colado. Vazio = este objeto.")]
        public Transform encaixe;

        [Tooltip("Botão \"Colar aqui\" no mural. Só aparece depois que a ideia foi escrita.")]
        public Button botaoColar;

        [Header("Colagem")]
        public string eventoColado = "postit_colado";
        public AudioClip somColar;
        public float duracaoVoo = 0.45f;

        XRGrabInteractable pegar;
        Rigidbody corpo;
        XRSocketInteractor socket;
        bool colado;

        void Awake()
        {
            if (!encaixe) encaixe = transform;
            socket = GetComponent<XRSocketInteractor>();
            if (postIt)
            {
                pegar = postIt.GetComponent<XRGrabInteractable>();
                corpo = postIt.GetComponent<Rigidbody>();
            }
            if (botaoColar)
            {
                botaoColar.onClick.AddListener(Colar);
                botaoColar.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            if (dialogo) dialogo.AoEvento += AoEvento;
            if (socket) socket.selectEntered.AddListener(AoColarComAMao);
        }

        void OnDisable()
        {
            if (dialogo) dialogo.AoEvento -= AoEvento;
            if (socket) socket.selectEntered.RemoveListener(AoColarComAMao);
        }

        void AoEvento(string evento)
        {
            if (colado || !postIt || string.IsNullOrEmpty(evento) || !evento.StartsWith(PrefixoIdeia)) return;

            if (!postIt.gameObject.activeSelf) postIt.gameObject.SetActive(true);
            postIt.Escrever(evento.Substring(PrefixoIdeia.Length).Trim());
            if (pegar) pegar.enabled = true; // agora dá para pegar com a mão (antes, o post-it estava em branco)
            if (botaoColar) botaoColar.gameObject.SetActive(true);
            Debug.Log($"[Mural] Ideia escrita no post-it: {postIt.Ideia}");
        }

        /// <summary>Botão "Colar aqui": o post-it voa até o centro do mural, de onde estiver (canto, mão ou chão).</summary>
        public void Colar()
        {
            if (colado || !postIt || !postIt.Escrito) return;
            colado = true;
            if (botaoColar) botaoColar.gameObject.SetActive(false);
            if (pegar) pegar.enabled = false; // se estiver na mão, solta
            if (corpo) corpo.isKinematic = true; // depois do "soltar", para não cair durante o voo
            StartCoroutine(VoarAteOMural());
        }

        IEnumerator VoarAteOMural()
        {
            var objeto = postIt.transform;
            Vector3 posicaoInicial = objeto.position;
            Quaternion rotacaoInicial = objeto.rotation;
            for (float t = 0f; t < duracaoVoo; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / duracaoVoo);
                objeto.SetPositionAndRotation(Vector3.Lerp(posicaoInicial, encaixe.position, k),
                    Quaternion.Slerp(rotacaoInicial, encaixe.rotation, k));
                yield return null;
            }
            objeto.SetPositionAndRotation(encaixe.position, encaixe.rotation);

            if (somColar) AudioSource.PlayClipAtPoint(somColar, encaixe.position, 0.8f);
            Debug.Log("[Mural] Post-it colado pelo botão \"Colar aqui\".");
            ControleVisita.Enviar(eventoColado);
        }

        // Colado com a mão (óculos): o GatilhoInteracao do mural já avisa a visita; aqui o post-it fica preso no quadro.
        void AoColarComAMao(SelectEnterEventArgs args)
        {
            if (colado) return;
            colado = true;
            if (botaoColar) botaoColar.gameObject.SetActive(false);
            Debug.Log("[Mural] Post-it colado com a mão.");
            StartCoroutine(PrenderNoMural());
        }

        IEnumerator PrenderNoMural()
        {
            yield return null; // deixa o socket terminar o encaixe
            if (pegar) pegar.enabled = false;
            if (corpo) corpo.isKinematic = true;
            postIt.transform.SetPositionAndRotation(encaixe.position, encaixe.rotation);
        }
    }
}
