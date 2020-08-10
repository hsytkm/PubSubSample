using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PubSubNto1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("PubSubNto1 Start.");

            var channel = Channel.CreateUnbounded<string>(
                new UnboundedChannelOptions
                {
                    SingleReader = true
                });

            var consumer = Task.Run(async () =>
            {
                while (await channel.Reader.WaitToReadAsync())
                {
                    Console.WriteLine(await channel.Reader.ReadAsync());
                }
            });

            var producers = Enumerable.Range(1, 3)
                .Select(producerNumber => Task.Run(async () =>
                {
                    var rnd = new Random();
                    for (var i = 0; i < 5; i++)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                        await channel.Writer.WriteAsync($"Producer:{producerNumber} Message {i}");
                    }
                }));

            await Task.WhenAll(producers);
            channel.Writer.Complete();
            Console.WriteLine("Writer Complete");

            await consumer;
            Console.WriteLine("End.");
        }
    }
}