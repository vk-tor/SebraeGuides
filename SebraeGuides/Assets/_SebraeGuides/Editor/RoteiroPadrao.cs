using System.Collections.Generic;

namespace SebraeGuides.Ferramentas
{
    /// <summary>
    /// Texto inicial do roteiro do guia (seção 6 do mapeamento). Depois de criado,
    /// o roteiro é editado no Inspector, no arquivo RoteiroGuia.asset.
    /// Falas marcadas com (validar) no documento precisam ser conferidas com o material do professor.
    /// </summary>
    static class RoteiroPadrao
    {
        public static void Preencher(RoteiroGuia roteiro)
        {
            roteiro.nomeDoGuia = "Guia";
            roteiro.falas = new List<Fala>
            {
                // Recepção
                F("R1", "Olá! Bem-vindo ao Sebraelab. Eu sou o guia e vou te acompanhar nesta visita.", null,
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
                F("A1", "Esta é a arena. Aqui acontecem palestras, oficinas e apresentações de projetos.", null,
                    Op("Continuar", "A2")),
                F("A2", "Aperte o botão Ligar a TV, no painel à sua frente, para ver alguns eventos que já passaram por aqui.",
                    "Aponte para o botão Ligar a TV e aperte o gatilho."),
                F("A3", "Agora vamos para o coworking.", null,
                    Op("Quero saber mais", "A3", "mais"), Op("Próximo ponto", null, "ir_coworking")),

                // Coworking
                F("C1", "No coworking, empreendedores dividem as estações de trabalho e os sofás e trabalham lado a lado.", null,
                    Op("Continuar", "C2")),
                F("C2", "Muitas ideias passam pelo design thinking: imersão, ideação, prototipação e implementação.", null,
                    Op("Continuar", "C3")),
                F("C3", "Pegue um post-it na estante da parede e cole no quadro à sua frente para deixar a sua ideia no mural.",
                    "Pegue o post-it amarelo com o botão lateral e solte em cima do quadro."),
                F("C4", "Ideia registrada! Vamos até a copa.", null,
                    Op("Vamos!", null, "ir_copa")),

                // Copa
                F("K1", "Esta é a copa. Muita parceria começa numa conversa durante o café.", null,
                    Op("Continuar", "K2")),
                F("K2", "Nas mesas ao lado acontecem reuniões rápidas e mentorias.", null,
                    Op("Quero saber mais", "K2", "mais"), Op("Próximo ponto", null, "ir_fundos")),

                // Sala dos fundos
                F("F1", "Chegamos à sala dos fundos. Puxe a maçaneta para abrir a porta de vidro.",
                    "Aponte para a maçaneta e aperte o botão lateral."),
                F("F2", "Esta sala é usada para [função a confirmar com o material do professor].", null,
                    Op("Continuar", "F3")),
                F("F3", "Você conheceu os cinco espaços. Volte para a recepção para encerrar a visita.", null,
                    Op("Voltar", null, "ir_retorno")),

                // Encerramento
                F("E1", "Obrigado pela visita! Você conheceu 5 de 5 espaços do Sebraelab.", null,
                    Op("Continuar", "E2")),
                F("E2", "Para conhecer o espaço pessoalmente, procure o Sebrae no Maranhão.", null,
                    Op("Recomeçar", null, "recomecar"), Op("Encerrar", null, "encerrar")),

                // Falas de apoio
                F("AVISO_IR", "Teleporte para o círculo com a placa Próxima parada. Te espero lá!", null),
                F("FIM", "Até a próxima!", null),
            };
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
