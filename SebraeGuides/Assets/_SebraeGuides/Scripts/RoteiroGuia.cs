using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebraeGuides
{
    /// <summary>Um botão que aparece embaixo de uma fala do guia.</summary>
    [Serializable]
    public class OpcaoFala
    {
        [Tooltip("Texto do botão.")]
        public string rotulo = "Continuar";

        [Tooltip("ID da próxima fala. Vazio = fecha o balão.")]
        public string proximaFala;

        [Tooltip("Evento enviado ao ControleVisita quando o botão é apertado (ex.: ir_arena, mais, recomecar). Pode ficar vazio.")]
        public string evento;
    }

    /// <summary>Uma fala do guia: texto, áudio opcional e os botões de resposta.</summary>
    [Serializable]
    public class Fala
    {
        [Tooltip("Identificador único da fala (ex.: R1, A2).")]
        public string id;

        [TextArea(2, 6)]
        public string texto;

        [Tooltip("Texto pequeno mostrado quando a fala não tem botões e espera uma ação do visitante.")]
        public string dica;

        [Tooltip("Áudio gravado da fala (opcional).")]
        public AudioClip audio;

        [Tooltip("Sem opções = o balão fica esperando uma interação (ex.: pegar o crachá).")]
        public List<OpcaoFala> opcoes = new List<OpcaoFala>();
    }

    /// <summary>
    /// Roteiro completo do guia. A Frente 5 edita os textos direto no Inspector,
    /// selecionando o arquivo Assets/_SebraeGuides/Dados/RoteiroGuia.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "RoteiroGuia", menuName = "Sebrae Guides/Roteiro do guia")]
    public class RoteiroGuia : ScriptableObject
    {
        public string nomeDoGuia = "Guia";
        public List<Fala> falas = new List<Fala>();

        public Fala Buscar(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var fala in falas)
            {
                if (fala != null && fala.id == id) return fala;
            }
            Debug.LogWarning($"[Roteiro] A fala '{id}' não existe em {name}.");
            return null;
        }
    }
}
