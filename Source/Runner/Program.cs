// See https://aka.ms/new-console-template for more information
using Lib;
using Runner;

var logger = new RunLogger();
var parsedArgs = Args.ParseArgs(args);
Run(parsedArgs, logger);

static void Run(Args args, LoggerBase logger)
{
    if (args.AIName == null)
    {
        throw new Exception("Specify an AI to run.");
    }

    var ai = AICatalog.GetAI(args.AIName);
    ai(args.AIArgs, logger);
}