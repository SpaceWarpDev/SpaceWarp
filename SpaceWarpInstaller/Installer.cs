using System.Net;
using System.IO.Compression;

namespace SpaceWarp.Installer {
    public class Installer {
        public static void Install(Download download) {
            using WebClient client = new WebClient();
            
            Console.WriteLine($"Downloading {download.Name} from {download.Url}...");

            client.DownloadFile(download.Url, download.Name);
            client.Dispose();

            Console.WriteLine($"Extracting {download.Name}...");

            ZipArchive archive = ZipFile.OpenRead(download.Name);
            archive.ExtractToDirectory(Directory.GetCurrentDirectory(), true);

            Console.WriteLine($"Cleaning up {download.Name}...");

            File.Delete(download.Name);
        }
    }
}
