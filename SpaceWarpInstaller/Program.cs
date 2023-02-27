namespace SpaceWarp.Installer {
    public class Program {
        public static void Main(string[] args) {
            int? latest = Versions.GetLatestVersion();
            string? release = Versions.GetDownloadUrl(latest);

            Console.WriteLine($"Latest version: {latest}");
            Console.WriteLine($"Download URL: {release}");
        }
    }
}
