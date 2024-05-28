﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Server("Server", cancellationToken);

            Console.WriteLine("Нажмите любую клавишу для завершения работы сервера...");
            Console.ReadKey();

            cancellationTokenSource.Cancel();
        }

        public static void Server(string name, CancellationToken cancellationToken)
        {
            UdpClient udpClient = new UdpClient(12345);
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("Сервер ждет сообщение от клиента");

            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] buffer = udpClient.Receive(ref iPEndPoint);
                var messageText = Encoding.UTF8.GetString(buffer);

                if (messageText.ToLower() == "exit")
                {
                    Console.WriteLine("Сервер завершает работу...");
                    break;
                }

                ThreadPool.QueueUserWorkItem(obj =>
                {
                    Message message = Message.DeserializeFromJson(messageText);
                    message.Print();

                    byte[] confirmationData = Encoding.UTF8.GetBytes("Сообщение успешно получено");
                    udpClient.Send(confirmationData, confirmationData.Length, iPEndPoint);
                });
            }

            udpClient.Close();
        }
    }
}