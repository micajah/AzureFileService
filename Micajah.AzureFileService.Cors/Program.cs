using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Micajah.AzureFileService.Cors
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["mafs:StorageConnectionString"]);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            ServiceProperties serviceProperties = client.GetServiceProperties();
            CorsProperties corsSettings = serviceProperties.Cors;

            AddRule(corsSettings);

            //corsSettings.CorsRules.RemoveAt(0);

            client.SetServiceProperties(serviceProperties);

            DisplayCorsSettings(corsSettings);

            Console.ReadKey();
        }

        private static void AddRule(CorsProperties corsSettings)
        {
            CorsRule corsRule = new CorsRule()
            {
                AllowedHeaders = new List<string> { "x-ms-*" },
                AllowedMethods = CorsHttpMethods.Put | CorsHttpMethods.Get,
                AllowedOrigins = new List<string>(ConfigurationManager.AppSettings["mafs:AllowedOrigins"].Split(',')),
                MaxAgeInSeconds = int.MaxValue,
            };

            corsSettings.CorsRules.Add(corsRule);
        }

        private static void DisplayCorsSettings(CorsProperties corsSettings)
        {
            if (corsSettings != null)
            {
                if (corsSettings != null)
                {
                    Console.WriteLine("Cors.CorsRules.Count          : " + corsSettings.CorsRules.Count);
                    for (int index = 0; index < corsSettings.CorsRules.Count; index++)
                    {
                        var corsRule = corsSettings.CorsRules[index];
                        Console.WriteLine("corsRule[index]              : " + index);
                        foreach (var allowedHeader in corsRule.AllowedHeaders)
                        {
                            Console.WriteLine("corsRule.AllowedHeaders      : " + allowedHeader);
                        }
                        Console.WriteLine("corsRule.AllowedMethods      : " + corsRule.AllowedMethods);

                        foreach (var allowedOrigins in corsRule.AllowedOrigins)
                        {
                            Console.WriteLine("corsRule.AllowedOrigins      : " + allowedOrigins);
                        }
                        foreach (var exposedHeaders in corsRule.ExposedHeaders)
                        {
                            Console.WriteLine("corsRule.ExposedHeaders      : " + exposedHeaders);
                        }
                        Console.WriteLine("corsRule.MaxAgeInSeconds     : " + corsRule.MaxAgeInSeconds);
                    }
                }
            }
        }
    }
}
