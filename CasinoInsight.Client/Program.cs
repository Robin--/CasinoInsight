using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using CasinoInsight.Actors.Messages;
using CasinoInsight.Actors.Models;
using Newtonsoft.Json;

namespace CasinoInsight.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting Simulator");


            var config = ConfigurationFactory.ParseString(@"
            akka {
                actor {
                    provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                }
                remote {
                    helios.tcp {
                        port = 8090
                        hostname = localhost
                    }
                }
            }
            ");

            using (var system = ActorSystem.Create("CasinoClient", config))
            {
                var cepactor = system
                    .ActorSelection("akka.tcp://CasinoInsight@localhost:8080/user/insight/cepactor");
                Console.WriteLine("Initialize Streams");
                InitializeStreams(cepactor);
                Console.WriteLine("Loading StandingQueries");
                LoadStandingQueries(cepactor);
                Console.WriteLine("Ready to Start Publishing [press any key to publish]");
                Console.ReadKey();
                Console.WriteLine("Publishing events simulation now");
                PublishEvents(cepactor);
                Console.ReadLine();
            }
        }

        private static void LoadStandingQueries(ActorSelection cepactor)
        {
            foreach (var file in new DirectoryInfo("./StandingQueries").GetFiles())
            {
                using (var reader = new StreamReader(file.FullName))
                {
                    var json = reader.ReadToEnd();
                    var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    cepactor.Tell(new StartStandingQuery(new StandingQuery
                    {
                        StandingQueryId = map["StandingQueryId"].ToString(),
                        Description = map["Description"].ToString(),
                        EplStatement = map["EplStatement"].ToString()
                    }));
                }
            }
        }

        private static void InitializeStreams(ActorSelection cepactor)
        {
            using (var reader = new StreamReader("./streams.json"))
            {
                while (!reader.EndOfStream)
                {
                    var json = reader.ReadLine();
                    var definition = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(definition["map"].ToString());
                    var @event = new AddEventType(definition["eventtype"].ToString(), map);
                    cepactor.Tell(@event);
                }
            }
        }

        private static void PublishEvents(ActorSelection cepactor)
        {
            foreach (var file in new DirectoryInfo("./EventSamples").GetFiles())
            {
                using (var reader = new StreamReader(file.FullName))
                {
                    Console.WriteLine("Press any key to publish Sample {0}", file.Name);
                    Console.ReadKey();
                    while (!reader.EndOfStream)
                    {
                        var json = reader.ReadLine();
                        var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        var @event = new NewEvent(map["eventtype"].ToString(), DateTime.Now, map, null,
                            Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                        cepactor.Tell(@event);
                        Console.WriteLine("Sending event {0} - {1}", map["eventtype"], json);
                    }
                }
            }
        }
    }
}