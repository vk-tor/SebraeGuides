using System.Collections.Generic;

namespace SebraeGuides.Ferramentas
{
    /// <summary>
    /// Texto inicial do roteiro do Professor Otávio (seção 6 do mapeamento). Depois de criado,
    /// o roteiro é editado no Inspector, no arquivo RoteiroGuia.asset.
    /// Textos finais da equipe (branch edge-tts, nov/2026), com a C3/C3B do post-it.
    /// </summary>
    static class RoteiroPadrao
    {
        public static void Preencher(RoteiroGuia roteiro)
        {
            roteiro.nomeDoGuia = "Prof. Otávio";
            roteiro.falas = new List<Fala>
            {
                // Recepção
                F("R1", "Olá! Bem-vindo ao Sebraelab. Eu sou o Professor Otávio e vou te acompanhar nesta visita guiada.", null,
                    Op("Continuar", "R2")),
                F("R2", "O Sebraelab é o hub de inovação aberta do Sebrae: um espaço colaborativo para empreendedores, startups, estudantes e parceiros.", null,
                    Op("Continuar", "R3")),
                F("R3", "Para andar, empurre o analógico para a frente, mire num círculo no chão e solte. Para virar, empurre para os lados. Para apertar botões, aponte e use o gatilho.", null,
                    Op("Entendi", "R4")),
                F("R4", "Antes de começar, pegue o seu crachá de visitante aqui no balcão.",
                    "Aponte para o crachá e segure o botão lateral do controle."),
                F("R5", "Tudo certo! A primeira parada é a arena de eventos.", null,
                    Op("Vamos!", null, "ir_arena")),

                // Arena
                F("A1", "Esta é a arena de eventos. Aqui acontecem palestras, oficinas práticas, maratonas de inovação e apresentações de projetos.", null,
                    Op("Continuar", "A2")),
                F("A2", "Aperte o botão Ligar a TV, no painel à sua frente, para ver alguns eventos que já passaram por aqui.",
                    "Aponte para o botão Ligar a TV e aperte o gatilho."),
                F("A3", "Agora vamos conhecer o espaço de coworking.", null,
                    Op("Quero saber mais", "A3", "mais"), Op("Próximo ponto", null, "ir_coworking")),

                // Coworking
                F("C1", "No coworking, empreendedores dividem as estações de trabalho e os sofás e trabalham lado a lado.", null,
                    Op("Continuar", "C2")),
                F("C2", "Muitas ideias passam pelo design thinking: imersão, ideação, prototipação e implementação.", null,
                    Op("Continuar", "C3")),
                // C3: cada botão escreve a ideia no post-it (evento "ideia:<texto do post-it>", tratado pelo MuralDeIdeias).
                F("C3", "Toda ideia começa no papel. Qual destas ideias você quer escrever no seu post-it?", null,
                    Op("App de turismo", "C3B", "ideia:Um app de turismo para São Luís"),
                    Op("Feira online", "C3B", "ideia:Uma feira online para produtores locais"),
                    Op("Curso para jovens", "C3B", "ideia:Um curso de programação para jovens")),
                F("C3B", "Ótimo, sua ideia já está no post-it. Agora é só colar: aponte para o mural e aperte o gatilho.",
                    "Aponte para \"Colar aqui\", no mural, e aperte o gatilho."),
                F("C4", "Ideia registrada com sucesso! Vamos até a copa e área de convivência.", null,
                    Op("Vamos!", null, "ir_copa")),

                // Copa
                F("K1", "Esta é a copa. Muita parceria importante começa numa conversa informal durante o café.", null,
                    Op("Continuar", "K2")),
                F("K2", "Nas mesas ao lado acontecem reuniões rápidas, conexões e mentorias para novos negócios.", null,
                    Op("Quero saber mais", "K2", "mais"), Op("Próximo ponto", null, "ir_fundos")),

                // Sala dos fundos
                F("F1", "Chegamos à sala dos fundos. Puxe a maçaneta para abrir a porta de vidro.",
                    "Aponte para a maçaneta e aperte o botão lateral."),
                F("F2", "Esta sala reservada é usada para reuniões estratégicas, oficinas focadas e atendimento especializado a empreendedores.", null,
                    Op("Continuar", "F3")),
                F("F3", "Excelente! Você conheceu os cinco espaços. Volte para a recepção para encerrar a visita.", null,
                    Op("Voltar", null, "ir_retorno")),

                // Encerramento
                F("E1", "Obrigado pela visita! Você conheceu 5 de 5 espaços do Sebraelab.", null,
                    Op("Continuar", "E2")),
                F("E2", "Para conhecer o espaço pessoalmente, procure o Sebrae no Maranhão.", null,
                    Op("Recomeçar", null, "recomecar"), Op("Encerrar", null, "encerrar")),

                // Falas de apoio
                F("AVISO_IR", "Teleporte para o círculo com a placa Próxima parada. Te espero lá!", null),
                F("FIM", "Até a próxima visita! Foi um prazer te receber no Sebraelab!", null),
            };
        }

        // Textos da primeira versão, trocados pelos textos finais da equipe (branch edge-tts, nov/2026).
        static readonly Dictionary<string, string> TextosAntigos = new Dictionary<string, string>
        {
            { "R1", "Olá! Bem-vindo ao Sebraelab. Eu sou o guia e vou te acompanhar nesta visita." },
            { "A1", "Esta é a arena. Aqui acontecem palestras, oficinas e apresentações de projetos." },
            { "A3", "Agora vamos para o coworking." },
            { "C4", "Ideia registrada! Vamos até a copa." },
            { "K1", "Esta é a copa. Muita parceria começa numa conversa durante o café." },
            { "K2", "Nas mesas ao lado acontecem reuniões rápidas e mentorias." },
            { "F2", "Esta sala é usada para [função a confirmar com o material do professor]." },
            { "F3", "Você conheceu os cinco espaços. Volte para a recepção para encerrar a visita." },
            { "FIM", "Até a próxima!" },
        };

        /// <summary>
        /// Leva os textos finais para um roteiro já existente. Só troca a fala que ainda tem o texto antigo:
        /// se alguém editou uma fala à mão, ela fica como está. Devolve true se mudou algo.
        /// </summary>
        public static bool AtualizarTextosFinais(RoteiroGuia roteiro)
        {
            if (!roteiro || roteiro.falas == null) return false;
            var padrao = UnityEngine.ScriptableObject.CreateInstance<RoteiroGuia>();
            Preencher(padrao);

            bool mudou = false;
            if (roteiro.nomeDoGuia == "Guia")
            {
                roteiro.nomeDoGuia = padrao.nomeDoGuia;
                mudou = true;
            }
            foreach (var antigo in TextosAntigos)
            {
                var fala = roteiro.falas.Find(f => f != null && f.id == antigo.Key);
                var nova = padrao.falas.Find(f => f.id == antigo.Key);
                if (fala == null || nova == null || fala.texto != antigo.Value) continue;
                fala.texto = nova.texto;
                mudou = true;
            }
            UnityEngine.Object.DestroyImmediate(padrao);
            return mudou;
        }

        /// <summary>
        /// Leva as falas do post-it (C3 nova e C3B) para um roteiro já existente, sem mexer nas outras falas.
        /// Só troca a C3 se ela ainda for a versão antiga (sem os botões de ideia). Devolve true se mudou algo.
        /// </summary>
        public static bool AtualizarFalasDoPostIt(RoteiroGuia roteiro)
        {
            if (!roteiro || roteiro.falas == null) return false;
            var padrao = UnityEngine.ScriptableObject.CreateInstance<RoteiroGuia>();
            Preencher(padrao);
            var novaC3 = padrao.falas.Find(f => f.id == "C3");
            var novaC3B = padrao.falas.Find(f => f.id == "C3B");
            UnityEngine.Object.DestroyImmediate(padrao);

            bool mudou = false;
            int indiceC3 = roteiro.falas.FindIndex(f => f != null && f.id == "C3");
            if (indiceC3 >= 0)
            {
                var c3 = roteiro.falas[indiceC3];
                bool temIdeias = c3.opcoes != null &&
                                 c3.opcoes.Exists(o => o != null && !string.IsNullOrEmpty(o.evento) && o.evento.StartsWith("ideia:"));
                if (!temIdeias)
                {
                    c3.texto = novaC3.texto;
                    c3.dica = novaC3.dica;
                    c3.opcoes = novaC3.opcoes;
                    mudou = true;
                }
            }

            if (!roteiro.falas.Exists(f => f != null && f.id == "C3B"))
            {
                roteiro.falas.Insert(indiceC3 >= 0 ? indiceC3 + 1 : roteiro.falas.Count, novaC3B);
                mudou = true;
            }
            return mudou;
        }

        static Fala F(string id, string texto, string dica, params OpcaoFala[] opcoes)
        {
            return new Fala { id = id, texto = texto, dica = dica, opcoes = new List<OpcaoFala>(opcoes) };
        }

        static OpcaoFala Op(string rotulo, string proximaFala, string evento = null)
        {
            return new OpcaoFala { rotulo = rotulo, proximaFala = proximaFala, evento = evento };
        }
    }
}
