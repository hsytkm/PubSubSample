using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PubSub1toN
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("PubSub1toN Start.");

            var channel = Channel.CreateUnbounded<string>(
                new UnboundedChannelOptions
                {
                    SingleWriter = true
                });

            var consumers = Enumerable.Range(1, 3)
                .Select(consumerNumber =>
                    Task.Run(async () =>
                    {
                        // アイテムがひとつ登録されると、一旦すべての生産者が「起こされ」ます。
                        // ReadAsyncを使った場合、2番目以降にアイテムを取りに行った消費者は、アイテムがないため例外がスローされてしまいます。
                        // そこでTryReadを利用することで、まだアイテムがあった場合だけ処理するように実装します。
                        while (await channel.Reader.WaitToReadAsync())
                        {
                            if (channel.Reader.TryRead(out var item))
                            {
                                Console.WriteLine($"Consumer:{consumerNumber} {item}");
                            }
                        }
                    }));

            var producer = Task.Run(async () =>
            {
                var rnd = new Random();
                for (var i = 0; i < 5; i++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                    await channel.Writer.WriteAsync($"Message {i}");
                }

                channel.Writer.Complete();
                Console.WriteLine("Writer Complete");
            });

            await Task.WhenAll(consumers.Union(new[] { producer }));
            Console.WriteLine("End.");
        }
    }
}
