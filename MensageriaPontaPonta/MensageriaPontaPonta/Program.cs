using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MensageriaPontaAPonta;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Informe o nome do remetente e do destinatário nos parâmetros");
            return;
        }

        string nomeRemetente = args[0];
        string nomeDestinatario = args[1];

        Console.WriteLine($"Conversa entre {nomeRemetente} e {nomeDestinatario}:");

        string filaRemetente = $"fila_{nomeRemetente}";
        string filaDestinatario = $"fila_{nomeDestinatario}";

        using IModel canal = ObtenhaCanalMensagem(filaRemetente);
        DefinaEventoRecebimentoMensagem(canal, nomeRemetente, filaRemetente);

        while (true)
        {
            string? mensagemDigitada = Console.ReadLine();

            if (string.IsNullOrEmpty(mensagemDigitada))
            {
                Console.WriteLine("Digite uma mensagem");
                continue;
            }

            if (mensagemDigitada.ToLower() == "sair")
                break;

            string mensagem = $"{nomeRemetente}: {mensagemDigitada}";
            byte[] body = Encoding.UTF8.GetBytes(mensagem);

            canal.BasicPublish(exchange: string.Empty, routingKey: filaDestinatario, basicProperties: default, body: body);
        }
    }

    private static IModel ObtenhaCanalMensagem(string filaRemetente)
    {
        IConnection conexao = ObtenhaConexaoRabbitMQ();
        IModel canal = conexao.CreateModel();
        canal.QueueDeclare(queue: filaRemetente, durable: false, exclusive: false, autoDelete: false, arguments: default);
        return canal;
    }

    private static void DefinaEventoRecebimentoMensagem(IModel canal, string nomeRemetente, string filaRemetente)
    {
        EventingBasicConsumer consumidor = new(canal);
        consumidor.Received += (sender, eventArgs) =>
        {
            string mensagem = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            if (mensagem.StartsWith(nomeRemetente))
            {
                return;
            }

            Console.WriteLine(mensagem);
            return;
        };
        canal.BasicConsume(queue: filaRemetente, autoAck: true, consumer: consumidor);
    }

    private static IConnection ObtenhaConexaoRabbitMQ()
    {
        ConnectionFactory conexao = new()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
        return conexao.CreateConnection();
    }
}

