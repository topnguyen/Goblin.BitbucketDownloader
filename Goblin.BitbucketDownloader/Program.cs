using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Goblin.BitbucketDownloader.Models;
using LibGit2Sharp;

namespace Goblin.BitbucketDownloader
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            var stopWatchGlobal = new Stopwatch();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Welcome to Goblin Bitbucket Downloader!");
            Console.WriteLine("- Clone and Pull all remote Branches");
            Console.WriteLine("- Auto save all repo to relative path 'Repositories\\{Repo Path}'");
            Console.WriteLine("- If Repo folder already exists, then skip.");
            Console.ResetColor();

            Console.Write("Enter username: ");
            var userName = Console.ReadLine();

            Console.Write("Enter password: ");
            var password = string.Empty;

            Console.ForegroundColor = ConsoleColor.Green;
            do
            {
                var key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Get repositories information...");
            stopWatch.Start();
            stopWatchGlobal.Start();

            var url = "https://api.bitbucket.org/2.0/repositories?role=member&pagelen=100";

            var listRepositories = new List<RepositoryModel>();

            try
            {
                listRepositories =
                    await GetRepositoriesAsync(url, userName, password)
                        .ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[Error][{stopWatch.ElapsedMilliseconds} ms]: {e.Message}");

                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Success][{stopWatch.ElapsedMilliseconds} ms] {listRepositories.Count} repo found!");

            Console.ResetColor();
            Console.WriteLine("Start to clone repo...");

            var repoBaseFolder = "Repositories";

            for (int i = 0; i < listRepositories.Count; i++)
            {
                stopWatch.Restart();

                var repo = listRepositories[i];
                var repoLink = repo.links.clone.FirstOrDefault()?.href;
                var repoFolder = Path.Combine(repoBaseFolder, repo.full_name);
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{i + 1}. ");
                Console.ResetColor();

                Console.Write($"{repoLink} > {repoFolder}");

                CloneAndPullRepo(repoLink, repoFolder, userName, password);

                Console.WriteLine($"[{stopWatch.ElapsedMilliseconds} ms]");
            }

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{stopWatchGlobal.Elapsed.TotalSeconds} m] Finish clone all repositories.");
            stopWatch.Stop();

            Console.ResetColor();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        private static async Task<List<RepositoryModel>> GetRepositoriesAsync(string url, string userName,
            string password)
        {
            var repositoriesPage =
                await url.WithBasicAuth(userName, password)
                    .GetJsonAsync<RepositoriesModel>()
                    .ConfigureAwait(true);

            var repositories = repositoriesPage.values;

            if (string.IsNullOrWhiteSpace(repositoriesPage.next))
            {
                return repositories;
            }

            var nextPageRepositories =
                await GetRepositoriesAsync(repositoriesPage.next, userName, password).ConfigureAwait(true);

            repositories.AddRange(nextPageRepositories);

            return repositories;
        }

        private static void CloneAndPullRepo(string repoLink, string repoFolder, string userName, string password)
        {
            var repoFolderInfo = new DirectoryInfo(repoFolder);
            
            if (!repoFolderInfo.Exists)
            {
                Repository.Clone(repoLink, repoFolder, new CloneOptions
                {
                    CredentialsProvider = (s, fromUrl, types) => new UsernamePasswordCredentials
                    {
                        Username = userName,
                        Password = password
                    }
                });
            }
  
            var repo = new Repository(repoFolder);

            var branches = repo.Branches;

            Console.Write($"[{branches.Count()} Branches]");

            if (repoFolderInfo.Exists)
            {
                Console.Write("[Repo Exists -> Skipped]");

                return;
            }

            foreach (var branch in branches)
            {
                if (!branch.IsRemote)
                {
                    continue;
                }
                
                var pullBranch = repo.CreateBranch(branch.FriendlyName, branch.Tip);

                var pulledBranch = repo.Branches.Update(pullBranch,
                    b => { b.TrackedBranch = branch.CanonicalName; });
            }
        }
    }
}