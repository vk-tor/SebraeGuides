using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

namespace SebraeGuides
{
    /// <summary>
    /// Deixa o XR Device Simulator só para quem testa sem óculos.
    ///
    /// Com um óculos de verdade rodando (Quest no Link ou o APK no próprio Quest), o simulador tira o headset
    /// do Input System para usar o dele: a imagem para de acompanhar a cabeça.
    ///
    /// Antes de a primeira cena acordar, este script:
    ///  1. no Editor, desliga só neste Play o "Use XR Device Simulator in scenes" (volta ao normal ao sair do Play);
    ///  2. desativa simuladores que estejam salvos dentro da cena e avisa no Console para apagá-los.
    /// Sem óculos, nada muda: o simulador aparece no Play como sempre.
    /// </summary>
    static class SimuladorSoSemOculos
    {
        static readonly string[] TiposDeSimulador = { "XRDeviceSimulator", "XRInteractionSimulator" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AntesDaPrimeiraCena()
        {
            if (!XRSettings.isDeviceActive) return; // sem óculos: o simulador continua valendo

#if UNITY_EDITOR
            DesligarSimuladorAutomatico();
#endif
            DesativarSimuladoresDaCena();
        }

        // Nesta etapa os objetos da cena já existem, mas ainda não rodaram Awake/OnEnable.
        static void DesativarSimuladoresDaCena()
        {
            var componentes = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var componente in componentes)
            {
                if (componente == null || System.Array.IndexOf(TiposDeSimulador, componente.GetType().Name) < 0) continue;
                var objeto = componente.gameObject;
                if (!objeto.activeSelf) continue;
                objeto.SetActive(false);
                Debug.LogWarning($"[Sebrae Guides] Óculos conectado: desativei '{objeto.name}' (cena {objeto.scene.name}). " +
                                 "Apague esse objeto da cena: com ele salvo lá, o rastreamento da cabeça quebra de novo no Recomeçar. " +
                                 "Sem óculos, o simulador aparece sozinho no Play.", objeto);
            }
        }

#if UNITY_EDITOR
        static Object configuracao;
        static FieldInfo campoAutomatico;

        static void DesligarSimuladorAutomatico()
        {
            // Configuração do XR Interaction Toolkit em Assets/XRI/Settings/Resources.
            configuracao = Resources.Load("XRDeviceSimulatorSettings");
            if (configuracao == null) return;
            campoAutomatico = configuracao.GetType().GetField("m_AutomaticallyInstantiateSimulatorPrefab",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (campoAutomatico == null || campoAutomatico.FieldType != typeof(bool)) return;
            if (!(bool)campoAutomatico.GetValue(configuracao)) return;

            campoAutomatico.SetValue(configuracao, false);
            Application.quitting += Restaurar;
            Debug.Log("[Sebrae Guides] Óculos conectado: o XR Device Simulator não vai ser criado neste Play.");
        }

        // No Editor, Application.quitting roda ao sair do Play: a opção volta como estava no projeto.
        static void Restaurar()
        {
            Application.quitting -= Restaurar;
            if (configuracao != null && campoAutomatico != null) campoAutomatico.SetValue(configuracao, true);
        }
#endif
    }
}
