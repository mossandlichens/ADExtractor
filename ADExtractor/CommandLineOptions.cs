namespace ADExtractor
{
    using CommandLine;
    using System.Collections.Generic;

    public class CommandLineOptions
    {
        [Option('d', "domain", Required = true, HelpText = "Domain")]
        public string Domain { get; set; }

        [Option('a', "attributes", Required = false, HelpText = "Attributes to be extracted. Default: givenName, sn, samAccountName, userPrincipalName")]
        public IEnumerable<string> Attributes { get; set; }

        [Option('o', "outputFile", Required = false, HelpText = "Output file with path for CSV. Default: Command line")]
        public string OutputFile { get; set; }

        [Option('c', "count", Required = false, HelpText = "Count of items. Default: All")]
        public int Count { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Verbose output. Default: False")]
        public bool Verbose { get; set; }
    }
}
