using Octokit;

namespace SpaceWarp.Installer {
    public class Versions {
        internal static GitHubClient client = new GitHubClient(new ProductHeaderValue("SpaceWarpInstaller"));

        public static int? GetLatestVersion() {
            IReadOnlyList<Release> releases = client.Repository.Release.GetAll("X606", "SpaceWarp").Result;
            return releases[0]?.Id;
        }

        public static string? GetDownloadUrl(int? version) {
            int realVersion = version ?? 0;
            IReadOnlyList<ReleaseAsset> assets = client.Repository.Release.GetAllAssets("X606", "SpaceWarp", realVersion).Result;

            foreach (ReleaseAsset asset in assets) {
                if (asset.Name.EndsWith(".zip")) {
                    return asset.BrowserDownloadUrl;
                }
            }
            
            return null;
        }
    }
}
