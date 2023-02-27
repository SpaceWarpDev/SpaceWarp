using Octokit;

namespace SpaceWarp.Installer {
    public class Versions {
        internal static GitHubClient client = new GitHubClient(new ProductHeaderValue("SpaceWarpInstaller"));

        public static int? GetLatestVersion() {
            IReadOnlyList<Release> releases = client.Repository.Release.GetAll("X606", "SpaceWarp").Result;
            return releases[0]?.Id;
        }

        public static List<Download> GetDownloadUrls(int? version) {
            int realVersion = version ?? 0;
            IReadOnlyList<ReleaseAsset> assets = client.Repository.Release.GetAllAssets("X606", "SpaceWarp", realVersion).Result;

            List<Download> zips = new List<Download>();

            foreach (ReleaseAsset asset in assets) {
                if (asset.Name.EndsWith(".zip")) {
                    zips.Add(new Download(asset.Name, asset.BrowserDownloadUrl));
                }
            }
            
            return zips;
        }

        public static Download? GetLatestDownload(ModLoader loader) {
            int? latest = Versions.GetLatestVersion();
            List<Download> urls = Versions.GetDownloadUrls(latest);

            Download? release = null;

            foreach (Download dl in urls) {
                if (dl.Name.Contains(loader.ToString())) {
                    release = dl;
                    break;
                }
            }

            if (release == null && urls.Count > 0) {
                release = urls[0];
            }

            return release;
        }
    }
}
