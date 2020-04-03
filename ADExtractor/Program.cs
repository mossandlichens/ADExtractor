namespace ADExtractor
{
    using CommandLine;
    using CsvHelper;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using System.Globalization;
    using System.IO;
    public class Program
    {
        static ILogger Logger { get; set; }
        static void Main(string[] args)
        {
            Logger = new LoggerConfiguration()
                                .WriteTo.Console()
                                .WriteTo.File("ADExtractor.log").CreateLogger();

            Logger.Information("ADExtractor started...");

            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);

            Logger.Information("ADExtractor ended.");
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Information("ADExtractor:UnhandledExceptionHandler");
            var exception = e.ExceptionObject as Exception;
            var errorMessage = "Message:" + exception.Message + Environment.NewLine +
                               "Source:" + exception.Source + Environment.NewLine +
                               "TargetSite:" + exception.TargetSite + Environment.NewLine +
                               "StackTrace:" + exception.StackTrace + Environment.NewLine;
            Logger.Error(errorMessage);
            Console.ReadLine();
            Environment.Exit(1);
        }

        static void RunOptions(CommandLineOptions opts)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            Logger.Information("ADExtractor:RunOptions");
            
            using (var context = new PrincipalContext(ContextType.Domain, opts.Domain))
            {
                Logger.Information("ADExtractor:Domain valid");
                var userPrincipal = new UserPrincipal(context);
                Logger.Information("ADExtractor:UserPrincipal valid");
                using (var searcher = new PrincipalSearcher(userPrincipal))
                {
                    Logger.Information("ADExtractor:PrincipalSearcher valid");
                    var attributeValuesCollection = new List<List<string>>();
                    int resultsCount = 0;

                    var attributes = new List<string> { "givenName", "sn", "samAccountName", "userPrincipalName" };
                    foreach (var attribute in opts.Attributes)
                    {
                        attributes.Add(attribute);
                    }

                    Logger.Information("Attributes:" + string.Join(",", attributes));

                    foreach (var result in searcher.FindAll())
                    {
                        var attributeValues = new List<string>();

                        Logger.Information("Extracting AD Item:" + resultsCount++);

                        var directoryEntry = result.GetUnderlyingObject() as DirectoryEntry;                        

                        foreach (var attribute in attributes)
                        {
                            var attributeValue = GetAttributeValue(attribute, directoryEntry);
                            if (opts.Verbose)
                            {
                                Logger.Information("Attribute:" + attribute + " Value:" + attributeValue);
                            }
                            attributeValues.Add(attributeValue);
                        }

                        attributeValuesCollection.Add(attributeValues);

                        if(opts.Count > 0 && resultsCount == opts.Count)
                        {
                            break;
                        }
                    }

                    Logger.Information("ADExtractor:AD Items collected:" + attributeValuesCollection.Count);

                    if (string.IsNullOrEmpty(opts.OutputFile) == false)
                    {
                        Logger.Information("ADExtractor:OutputFile:" + opts.OutputFile);
                        using (var writer = new StreamWriter(opts.OutputFile))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            // Write the headers
                            foreach (var attribute in attributes)
                            {
                                csv.WriteField(attribute);
                            }
                            csv.NextRecord();

                            // Write the rows
                            foreach (var attributeValues in attributeValuesCollection)
                            {
                                foreach(var attributeValue in attributeValues)
                                {
                                    csv.WriteField(attributeValue);
                                }
                                csv.NextRecord();
                            }
                        }
                        Logger.Information("ADExtractor:OutputFile written");
                    }
                }
                userPrincipal.Dispose();
            }

            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:00}",ts.Hours, ts.Minutes, ts.Seconds,ts.Milliseconds / 10);
            Logger.Information("ADExtractor:Time taken:" + elapsedTime);
            Logger.Information("ADExtractor:Please any key to close the application.");

            Console.ReadLine();
        }

        private static string GetAttributeValue(string attribute, DirectoryEntry directoryEntry)
        {
            string value = string.Empty;
            if (directoryEntry.Properties[attribute].Value != null)
            {
                value = directoryEntry.Properties[attribute].Value.ToString();                
            }
            return value;
        }
    }
}
