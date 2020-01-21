using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CalculateAzureBlobStorageContainersSize
{
    class Program
    {
        private const int TimeRoundDigits = 2;

        private static string FormatSize(double size)
        {
            if (size >= 1024)
            {
                double sizeInKB = size / 1024.0;

                if (sizeInKB >= 1024)
                {
                    double sizeInMB = sizeInKB / 1024.0;

                    if (sizeInMB >= 1024)
                    {
                        double sizeInGB = sizeInMB / 1024.0;

                        return string.Format(CultureInfo.InvariantCulture, "{0:N2} GB", sizeInGB);
                    }
                    else
                    {
                        return string.Format(CultureInfo.InvariantCulture, "{0:N2} MB", sizeInMB);
                    }
                }
                else
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:N2} KB", sizeInKB);
                }
            }

            if (size == 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:N0} B", size);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0:N2} B", size);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Tool to calculate size of containers in Windows Azure Blob Storage.\r\n");

            try
            {
                Stopwatch watch = Stopwatch.StartNew();

                double totalSize = 0;
                int totalCount = 0;
                int successCount = 0;
                Dictionary<string, double> dict = new Dictionary<string, double>();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["mafs:StorageConnectionString"]);
                var client = storageAccount.CreateCloudBlobClient();

                var containers = client.ListContainers();

                foreach (var container in containers)
                {
                    try
                    {
                        Console.Write("Container #{0} \"{1}\"", totalCount, container.Name);

                        double containerSize = 0;
                        long blobCount = 0;

                        IEnumerable<IListBlobItem> blobList = container.ListBlobs(null, true);
                        foreach (IListBlobItem item in blobList)
                        {
                            CloudBlockBlob blob = item as CloudBlockBlob;
                            if (blob != null)
                            {
                                if (blob.BlobType == BlobType.BlockBlob)
                                {
                                    containerSize += blob.Properties.Length;

                                    blobCount++;
                                }
                            }
                        }

                        Console.WriteLine(" size: {0}, Block Blob count: {1}.", FormatSize(containerSize), blobCount);

                        dict.Add(container.Name, containerSize);

                        totalSize += containerSize;

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" calculation failed:\r\n{0}", ex.ToString());
                    }

                    totalCount++;
                }

                Console.WriteLine("\r\nSorted results:\r\n");

                var ordered = dict.OrderBy(x => x.Value);

                foreach (var k in ordered)
                {
                    Console.WriteLine("Container \"{0}\" size: {1}.", k.Key, FormatSize(k.Value));
                }

                watch.Stop();

                double elapsedTime = Math.Round(watch.Elapsed.TotalMinutes, TimeRoundDigits);

                Console.WriteLine(@"
Total containers found: {0}
Success: {1}
Failed: {2}
Total size: {3}
Process was run for {4} minutes.
"
                    , totalCount, successCount, totalCount - successCount, FormatSize(totalSize), elapsedTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\n{0}\r\n", ex.ToString());
            }

            Console.WriteLine("\r\nPress any key to quit.");

            Console.ReadKey();
        }
    }
}
