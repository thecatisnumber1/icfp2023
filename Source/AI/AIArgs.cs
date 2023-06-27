namespace AI
{
    public record AIArgs(int Example)
    {
        public static AIArgs ParseArgs(string[] args)
        {
            int? example = null;

            for (int i = 0; i < args.Length; i++)
            {
                int intArg() => int.Parse(args[++i]);

                switch (args[i])
                {
                    case "--example":
                    case "-e":
                        example = intArg();
                        break;
                }
            }

            if (example == null)
            {
                throw new ArgumentException("--example argument is required");
            }

            return new AIArgs(example.Value);
        }
    }
}
