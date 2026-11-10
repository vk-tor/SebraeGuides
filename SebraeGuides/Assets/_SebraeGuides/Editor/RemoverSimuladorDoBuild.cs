using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SebraeGuides.Ferramentas
{
    /// <summary>
    /// No build (APK do Quest ou executável do PC), tira das cenas o XR Device Simulator e o XR Interaction Simulator.
    /// No óculos, o simulador tiraria o headset do Input System e a imagem não acompanharia a cabeça.
    /// A cena salva no projeto não é alterada; o Play no Editor é tratado pelo SimuladorSoSemOculos.
    /// </summary>
    class RemoverSimuladorDoBuild : IProcessSceneWithReport
    {
        static readonly string[] TiposDeSimulador = { "XRDeviceSimulator", "XRInteractionSimulator" };

        public int callbackOrder => 0;

        public void OnProcessScene(Scene cena, BuildReport relatorio)
        {
            if (relatorio == null) return; // chamada ao entrar no Play: não é build

            foreach (var raiz in cena.GetRootGameObjects())
            {
                foreach (var componente in raiz.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (componente == null || System.Array.IndexOf(TiposDeSimulador, componente.GetType().Name) < 0) continue;
                    string nome = componente.gameObject.name;
                    Object.DestroyImmediate(componente.gameObject);
                    Debug.Log($"[Sebrae Guides] Build: '{nome}' tirado da cena {cena.name} (o simulador não vai para o óculos).");
                }
            }
        }
    }
}
