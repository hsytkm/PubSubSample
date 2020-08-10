using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PubSub1to1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("PubSub1to1 Start.");

            var channel = Channel.CreateUnbounded<string>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = true
                });

            var consumer = Task.Run(async () =>
            {
                while (await channel.Reader.WaitToReadAsync())
                {
                    Console.WriteLine(await channel.Reader.ReadAsync());
                }

                // 以下でもOK
                //await foreach (var item in channel.Reader.ReadAllAsync())
                //{
                //    Console.WriteLine(item);
                //}
            });

            var producer = Task.Run(async () =>
            {
                var rnd = new Random();
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                    await channel.Writer.WriteAsync($"Message {i}");
                }

                channel.Writer.Complete();
                Console.WriteLine("Writer Complete");
            });

            await Task.WhenAll(producer, consumer);
            Console.WriteLine("End.");
        }
    }
}
