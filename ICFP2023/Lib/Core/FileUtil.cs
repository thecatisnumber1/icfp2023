namespace ICFP2023
{
    public static class FileUtil
    {
        public static string PathFor(string pathFromProjectRoot)
        {
            pathFromProjectRoot = pathFromProjectRoot.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Join(ProjectRoot, pathFromProjectRoot);
        }

        public static string Read(string pathFromProjectRoot)
        {
            return File.ReadAllText(PathFor(pathFromProjectRoot));
        }

        public static void Write(string pathFromProjectRoot, string contents)
        {
            File.WriteAllText(PathFor(pathFromProjectRoot), contents);
        }

        public static string[] GetFilesInDir(string pathFromProjectRoot)
        {
            return Directory.GetFiles(PathFor(pathFromProjectRoot));
        }

        public static string ProjectRoot => projectRoot ??= FindProjectRoot();

        private static string projectRoot = null;

        private static string FindProjectRoot()
        {
            string directoryToLookFor = "problems";
            string currentDir = ".";
            while (!Directory.GetDirectories(currentDir)
                .Any(dir => Path.GetFileName(dir) == directoryToLookFor))
            {
                currentDir = Path.Join(currentDir, "..");

                if (currentDir.Length > 100)
                {
                    throw new Exception($"Couldn't find a {directoryToLookFor} directory above me");
                }
            }

            return Path.GetFullPath(currentDir);
        }
    }
}
