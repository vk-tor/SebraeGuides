using UnityEngine;

namespace SebraeGuides
{
    /// <summary>Texto "Próxima parada" que flutua em cima do próximo ponto de teleporte.</summary>
    public class MarcadorProximaParada : MonoBehaviour
    {
        public float altura = 1.4f;
        public float amplitude = 0.08f;
        public float velocidade = 2f;

        Transform alvo;

        public void MostrarEm(Transform novoAlvo)
        {
            alvo = novoAlvo;
            gameObject.SetActive(alvo != null);
            Atualizar();
        }

        public void Esconder()
        {
            alvo = null;
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            Atualizar();
        }

        void Atualizar()
        {
            if (!alvo) return;
            float onda = Mathf.Sin(Time.time * velocidade) * amplitude;
            transform.position = alvo.position + Vector3.up * (altura + onda);
        }
    }
}
