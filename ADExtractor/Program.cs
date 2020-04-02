namespace ADExtractor
{
    using CommandLine;
    using CsvHelper;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using System.Globalization;
    using System.IO;

    public class Program
    {
        static void Main(string[] args)
        {
            Globals.Logger = new LoggerConfiguration().WriteTo.File("ADExtractor.log").CreateLogger();

            Globals.Logger.Information("ADExtractor started...");

            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);

            Globals.Logger.Information("ADExtractor ended.");
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Globals.Logger.Information("ADExtractor:UnhandledExceptionHandler");
            var exception = e.ExceptionObject as Exception;
            var errorMessage = "Message:" + exception.Message + Environment.NewLine +
                               "Source:" + exception.Source + Environment.NewLine +
                               "TargetSite:" + exception.TargetSite + Environment.NewLine +
                               "StackTrace:" + exception.StackTrace + Environment.NewLine;
            Globals.Logger.Error(errorMessage);
            Console.WriteLine(errorMessage);
            Console.ReadLine();
            Environment.Exit(1);
        }

        static void RunOptions(CommandLineOptions opts)
        {
            Globals.Logger.Information("ADExtractor:RunOptions");
            
            using (var context = new PrincipalContext(ContextType.Domain, opts.Domain))
            {
                Globals.Logger.Information("ADExtractor:Domain valid");
                var userPrincipal = new UserPrincipal(context);
                Globals.Logger.Information("ADExtractor:UserPrincipal valid");
                using (var searcher = new PrincipalSearcher(userPrincipal))
                {
                    Globals.Logger.Information("ADExtractor:PrincipalSearcher valid");
                    var attributesCollection = new List<ActiveDirectoryAttributes>();
                    foreach (var result in searcher.FindAll())
                    {
                        Globals.Logger.Information("ADExtractor:Search results available");
                        var attributes = new ActiveDirectoryAttributes();
                        
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;

                        if (de.Properties["givenName"].Value != null)
                        {
                            attributes.givenName = de.Properties["givenName"].Value.ToString();
                        }
                        Console.WriteLine("First Name: " + attributes.givenName);

                        if (de.Properties["sn"].Value != null)
                        {
                            attributes.sn = de.Properties["sn"].Value.ToString();
                        }
                        Console.WriteLine("Last Name: " + attributes.sn);

                        if (de.Properties["samAccountName"].Value != null)
                        {
                            attributes.samAccountName = de.Properties["samAccountName"].Value.ToString();
                        }                        
                        Console.WriteLine("SAM Account Name: " + attributes.samAccountName);

                        if (de.Properties["userPrincipalName"].Value != null)
                        {
                            attributes.userPrincipalName = de.Properties["userPrincipalName"].Value.ToString();
                        }
                        Console.WriteLine("User Principal Name: " + attributes.userPrincipalName);

                        foreach(var attribute in opts.Attributes)
                        {
                            Console.WriteLine(de.Properties[attribute].PropertyName + ": " + de.Properties[attribute].Value);
                        }

                        attributesCollection.Add(attributes);
                        
                        Console.WriteLine();
                    }

                    Globals.Logger.Information("ADExtractor:Records collected:" + attributesCollection.Count);

                    if (string.IsNullOrEmpty(opts.OutputFile) == false)
                    {
                        Globals.Logger.Information("ADExtractor:OutputFile:" + opts.OutputFile);
                        using (var writer = new StreamWriter(opts.OutputFile))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(attributesCollection);
                        }
                    }
                }
                userPrincipal.Dispose();
            }
            Console.ReadLine();
        }    
    }
}
