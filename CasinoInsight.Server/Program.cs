using System;
using Akka.Actor;
using Akka.Configuration;
using CasinoInsight.Actors;

namespace CasinoInsight.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
            akka {
                actor {
                    provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                }

                remote {
                    helios.tcp {
                        port = 8080
                        hostname = localhost
                    }
                }
            }
            ");
            Console.WriteLine("Starting CasinoInsight Server");
            using (var system = ActorSystem.Create("CasinoInsight", config))
            {
                system.ActorOf<InsightActor>("insight");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}