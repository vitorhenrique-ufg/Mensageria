using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MensageriaPontoMultiPonto
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Informe um nome de usuário nos parametros");
                return;
            }
            string nomeUsuario = args[0];

            using IConnection conexao = ObtenhaConexaoRabbitMQ();
            using (IModel canal = conexao.CreateModel())
            {
                Console.WriteLine($"Bem-vindo ao grupo do WhatsApp, {nomeUsuario}!");

                string fila = $"fila_{nomeUsuario}";
                DeclareTopicoInteresse(canal, fila);
                EventingBasicConsumer consumidor = new(canal);
                consumidor.Received += (sender, eventArgs) =>
                {
                    string mensagem = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                    if (mensagem.StartsWith(nomeUsuario))
                    {
                        return;
                    }

                    Console.WriteLine(mensagem);
                    return;
                };

                canal.BasicConsume(queue: fila, autoAck: true, consumer: consumidor);

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

                    string mensagem = $"{nomeUsuario}: {mensagemDigitada}";
                    byte[] body = Encoding.UTF8.GetBytes(mensagem);
                    canal.BasicPublish(exchange: "Grupo_WhatsApp", routingKey: string.Empty, basicProperties: null, body: body);
                }
            }
        }

        static IConnection ObtenhaConexaoRabbitMQ()
        {
            ConnectionFactory factory = new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            return factory.CreateConnection();
        }

        static void DeclareTopicoInteresse(IModel canal, string fila)
        {
            canal.ExchangeDeclare(exchange: "Grupo_WhatsApp", type: ExchangeType.Topic);
            canal.QueueDeclare(queue: fila, durable: false, exclusive: false, autoDelete: false, arguments: null);
            canal.QueueBind(queue: fila, exchange: "Grupo_WhatsApp", routingKey: "");
        }
    }
}
