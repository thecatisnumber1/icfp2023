using AI;

namespace Runner
{
    public record Args(string AIName, AIArgs AIArgs)
    {
        public static Args ParseArgs(string[] args)
        {
            string? aiType = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--aiType":
                    case "-a":
                        aiType = args[++i];
                        break;
                }
            }

            if (aiType == null)
            {
                throw new ArgumentException("--aiType argument is required");
            }

            return new Args(aiType, AIArgs.ParseArgs(args));
        }
    }
}
