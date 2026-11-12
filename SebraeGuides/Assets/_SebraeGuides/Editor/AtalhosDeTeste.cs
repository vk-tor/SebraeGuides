using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;

namespace SebraeGuides.Ferramentas
{
    /// <summary>
    /// Menu "Sebrae Guides > Testes no Play": leva o visitante direto a uma parada, sem refazer a visita
    /// desde a recepção. Só funciona com o Play ligado (no simulador ou com o óculos no Link).
    /// </summary>
    static class AtalhosDeTeste
    {
        const string Menu = "Sebrae Guides/Testes no Play/";

        [MenuItem(Menu + "Ir para a arena", false, 40)] static void Arena() => IrPara("arena");
        [MenuItem(Menu + "Ir para o coworking (post-it)", false, 41)] static void Coworking() => IrPara("coworking");
        [MenuItem(Menu + "Ir para a copa", false, 42)] static void Copa() => IrPara("copa");
        [MenuItem(Menu + "Ir para a sala dos fundos", false, 43)] static void Fundos() => IrPara("fundos");

        [MenuItem(Menu + "Apertar o 1º botão do balão", false, 60)] static void PrimeiroBotao() => ApertarBotaoDoBalao(0);
        [MenuItem(Menu + "Apertar \"Colar aqui\" no mural", false, 61)] static void ColarAqui()
        {
            var mural = Object.FindFirstObjectByType<MuralDeIdeias>();
            if (mural && mural.botaoColar && mural.botaoColar.gameObject.activeInHierarchy) mural.botaoColar.onClick.Invoke();
            else Debug.LogWarning("[Teste] O botão \"Colar aqui\" ainda não apareceu (escolha uma ideia primeiro).");
        }

        [MenuItem(Menu + "Ir para a arena", true)]
        [MenuItem(Menu + "Ir para o coworking (post-it)", true)]
        [MenuItem(Menu + "Ir para a copa", true)]
        [MenuItem(Menu + "Ir para a sala dos fundos", true)]
        [MenuItem(Menu + "Apertar o 1º botão do balão", true)]
        [MenuItem(Menu + "Apertar \"Colar aqui\" no mural", true)]
        static bool SoNoPlay() => EditorApplication.isPlaying;

        // Mesmo efeito de apontar e apertar o gatilho no botão (útil quando a cabeça do simulador sai do lugar).
        static void ApertarBotaoDoBalao(int indice)
        {
            var dialogo = Object.FindFirstObjectByType<DialogoGuia>();
            if (!dialogo || !dialogo.areaBotoes) return;
            int n = 0;
            foreach (Transform filho in dialogo.areaBotoes)
            {
                var botao = filho.GetComponent<UnityEngine.UI.Button>();
                if (!botao || botao == dialogo.modeloBotao || !filho.gameObject.activeInHierarchy) continue;
                if (n++ != indice) continue;
                Debug.Log($"[Teste] Apertando o botão \"{botao.GetComponentInChildren<TMPro.TMP_Text>()?.text}\" da fala {dialogo.FalaAtual}.");
                botao.onClick.Invoke();
                return;
            }
            Debug.LogWarning($"[Teste] A fala {dialogo.FalaAtual} não tem botão número {indice + 1}.");
        }

        static void IrPara(string chave)
        {
            var controle = ControleVisita.Instancia;
            var origem = Object.FindFirstObjectByType<XROrigin>();
            if (!controle || !origem || !origem.Camera)
            {
                Debug.LogWarning("[Teste] Abra a cena Visita e aperte Play antes de usar este atalho.");
                return;
            }

            var parada = controle.paradas.Find(p => p != null && p.chave == chave);
            if (parada == null || !parada.pontoTeleporte)
            {
                Debug.LogWarning($"[Teste] Parada '{chave}' sem ponto de teleporte.");
                return;
            }

            // Mesmo efeito de teleportar para o círculo da parada: pés no ponto, olhando para onde ele aponta.
            var ponto = parada.pontoTeleporte.transform;
            float alturaDaCabeca = origem.Camera.transform.position.y - origem.transform.position.y;
            origem.MatchOriginUpCameraForward(Vector3.up, Vector3.ProjectOnPlane(ponto.forward, Vector3.up).normalized);
            origem.MoveCameraToWorldLocation(ponto.position + Vector3.up * alturaDaCabeca);

            Debug.Log($"[Teste] Visitante levado para a parada '{chave}'.");
            ControleVisita.Enviar("ir_" + chave);
        }
    }
}
