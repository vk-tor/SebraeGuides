using UnityEngine;

namespace SebraeGuides
{
    /// <summary>Mantém um painel ou texto virado para o visitante (só gira no eixo vertical).</summary>
    public class ViradoParaCamera : MonoBehaviour
    {
        Transform cabeca;

        void LateUpdate()
        {
            if (!cabeca)
            {
                var cam = Camera.main;
                if (!cam) return;
                cabeca = cam.transform;
            }

            Vector3 direcao = transform.position - cabeca.position;
            direcao.y = 0f;
            if (direcao.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direcao);
        }
    }
}
