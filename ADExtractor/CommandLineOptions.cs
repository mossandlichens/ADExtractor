namespace ADExtractor
{
    using CommandLine;
    using System.Collections.Generic;

    class CommandLineOptions
    {
        [Option('d', "domain", Required = true, HelpText = "Domain")]
        public string Domain { get; set; }

        [Option('a', "attributes", Required = false, HelpText = "Attributes to be extracted. Default: givenName, sn, samAccountName, userPrincipalName")]
        public IEnumerable<string> Attributes { get; set; }

        [Option('o', "outputFile", Required = false, HelpText = "Output file with path for CSV. Default: Command line")]
        public string OutputFile { get; set; }
    }
}
