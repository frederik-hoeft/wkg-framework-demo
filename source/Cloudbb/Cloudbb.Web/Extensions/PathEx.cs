namespace Cloudbb.Web.Extensions;

internal static class PathExtensions
{
    extension(Path)
    {
        public static string GetProcessPath()
        {
            string processPath = Environment.ProcessPath ?? throw new InvalidOperationException("Environment.ProcessPath is null.");
            DirectoryInfo dirInfo = new(string.Join(Path.DirectorySeparatorChar, processPath.Split(Path.DirectorySeparatorChar)[..^1]));
            return dirInfo.FullName;
        }
    }
}
