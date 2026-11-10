using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SebraeGuides.Ferramentas
{
    /// <summary>
    /// Menu "Sebrae Guides > 4. Preparar o projeto para o Quest". Mexe só nas configurações do projeto (não na cena):
    /// nome do app, luzes e sombras do URP iguais no PC e no Quest, e Single Pass Instanced no Windows (Link).
    /// Pode rodar quantas vezes quiser.
    /// </summary>
    static class PrepararQuest
    {
        // Perfil de qualidade do Android (Quest) e do Windows. Ver Project Settings > Quality.
        const string UrpQuest = "Assets/Settings/Project Configuration/Performance URP Config.asset";
        const string UrpPC = "Assets/Settings/Project Configuration/Quality URP Config.asset";
        const string ConfigOpenXR = "Assets/XR/Settings/OpenXRPackageSettings.asset";

        // Valores do URP. Se o Quest não segurar 72 FPS, baixe LuzesPorObjeto para 2.
        const int LuzesPorObjeto = 4;
        const float DistanciaSombra = 25f;     // cobre o Sebraelab inteiro, caso alguém religue a sombra do sol
        const int ResolucaoSombraQuest = 2048; // 4096 pesa no Quest 2

        [MenuItem("Sebrae Guides/4. Preparar o projeto para o Quest", false, 4)]
        static void Preparar()
        {
            var resumo = new List<string>();

            MontarVisita.AplicarNomeDoApp();
            resumo.Add("• App: Sebraelab 360 (com.SebraeGuides.Sebraelab360)");
            resumo.Add(AjustarUrp(UrpQuest, "Quest", true));
            resumo.Add(AjustarUrp(UrpPC, "PC", false));
            resumo.Add(SinglePassNoWindows());

            AssetDatabase.SaveAssets();
            string texto = string.Join("\n", resumo);
            Debug.Log("[Sebrae Guides] Projeto preparado para o Quest:\n" + texto);
            EditorUtility.DisplayDialog("Sebrae Guides", "Projeto preparado para o Quest:\n\n" + texto, "OK");
        }

        // As 4 luzes da visita são em tempo real: com "Additional Lights" desligado no perfil do Android, elas não
        // acendiam no Quest. Os dois perfis passam a ter o mesmo modo de luz, então todos veem a mesma cena.
        static string AjustarUrp(string caminho, string rotulo, bool quest)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(caminho);
            if (!asset) return $"• {rotulo}: não achei {caminho}";

            var serializado = new SerializedObject(asset);
            DefinirInt(serializado, "m_AdditionalLightsRenderingMode", 1); // 1 = Per Pixel
            DefinirInt(serializado, "m_AdditionalLightsPerObjectLimit", LuzesPorObjeto);
            DefinirFloat(serializado, "m_ShadowDistance", DistanciaSombra);
            if (quest)
            {
                DefinirInt(serializado, "m_MainLightShadowmapResolution", ResolucaoSombraQuest);
                DefinirBool(serializado, "m_SoftShadowsSupported", false);
            }
            serializado.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            return quest
                ? $"• {rotulo}: luzes extras Per Pixel (até {LuzesPorObjeto} por objeto), sombra até {DistanciaSombra:0} m, mapa {ResolucaoSombraQuest}, sombra dura"
                : $"• {rotulo}: luzes extras Per Pixel (até {LuzesPorObjeto} por objeto), sombra até {DistanciaSombra:0} m";
        }

        // Multi Pass desenha a cena duas vezes no Play com o Link. O Android já usa Single Pass Instanced,
        // então os shaders do projeto já funcionam nesse modo.
        static string SinglePassNoWindows()
        {
            foreach (var objeto in AssetDatabase.LoadAllAssetsAtPath(ConfigOpenXR))
            {
                if (!objeto || objeto.name != "Standalone") continue;
                var serializado = new SerializedObject(objeto);
                var modo = serializado.FindProperty("m_renderMode");
                if (modo == null) continue;
                if (modo.intValue != 1)
                {
                    modo.intValue = 1; // 0 = Multi Pass, 1 = Single Pass Instanced
                    serializado.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(objeto);
                }
                return "• Windows (Link): OpenXR em Single Pass Instanced";
            }
            return "• Windows (Link): não achei a configuração do OpenXR";
        }

        static void DefinirInt(SerializedObject serializado, string nome, int valor)
        {
            var propriedade = serializado.FindProperty(nome);
            if (propriedade != null) propriedade.intValue = valor;
            else Debug.LogWarning($"[Sebrae Guides] Campo {nome} não encontrado em {serializado.targetObject.name}.");
        }

        static void DefinirFloat(SerializedObject serializado, string nome, float valor)
        {
            var propriedade = serializado.FindProperty(nome);
            if (propriedade != null) propriedade.floatValue = valor;
            else Debug.LogWarning($"[Sebrae Guides] Campo {nome} não encontrado em {serializado.targetObject.name}.");
        }

        static void DefinirBool(SerializedObject serializado, string nome, bool valor)
        {
            var propriedade = serializado.FindProperty(nome);
            if (propriedade != null) propriedade.boolValue = valor;
            else Debug.LogWarning($"[Sebrae Guides] Campo {nome} não encontrado em {serializado.targetObject.name}.");
        }
    }
}
