namespace SpaceWarp.Installer {
    public class Program {
        public static void Main(string[] args) {
            Download? bepInExDownload = Versions.GetLatestDownload(ModLoader.BepInEx);
            Download? doorstopDownload = Versions.GetLatestDownload(ModLoader.Doorstop);

            if (args.Length >= 1 && args[0].ToLower() == "doorstop" && doorstopDownload != null) {
                Installer.Install(doorstopDownload);
            } else if(bepInExDownload != null) {
                Installer.Install(bepInExDownload);
            } else {
                Console.WriteLine("Could not find a release binary!");
            }
        }
    }
}
