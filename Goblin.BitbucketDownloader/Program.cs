﻿using System;
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
        public static string RepoBaseFolder = "Repositories";
        
        static async Task Main(string[] args)
        {
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
           
            var stopWatch = Stopwatch.StartNew();
            var stopWatchGlobal = Stopwatch.StartNew();

            var listRepositories = new List<RepositoryModel>();

            try
            {
                var url = "https://api.bitbucket.org/2.0/repositories?role=member&pagelen=100"; // &sort=-updated_on

                listRepositories =
                    await GetRepositoriesAsync(url, userName, password)
                        .ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[Error][{stopWatchGlobal.Elapsed.TotalSeconds} s]: {e.Message}");

                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Success][{stopWatchGlobal.Elapsed.TotalSeconds} s] {listRepositories.Count} repo found!");

            stopWatch.Stop();

            Console.ResetColor();
            Console.WriteLine("Start to clone repo...");

            for (int i = 0; i < listRepositories.Count; i++)
            {
                try
                {
                    CloneAndPullRepo(i, listRepositories, userName, password);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"[Error] [Skipped and Continue]: {e.Message}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{stopWatchGlobal.Elapsed.TotalMinutes} m] Finish clone all repositories.");
            stopWatchGlobal.Stop();

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

        private static void CloneAndPullRepo(int i, List<RepositoryModel> listRepositories, string userName, string password)
        {
            var stopWatch = Stopwatch.StartNew();
          
            var repoModel = listRepositories[i];
            
            var repoLink = repoModel.links.clone.FirstOrDefault()?.href;
            
            var repoFolder = Path.Combine(RepoBaseFolder, repoModel.full_name);
                
            var repoFolderInfo = new DirectoryInfo(repoFolder);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{i + 1}.");
            Console.ResetColor();
            
            Console.Write($" {repoLink} > {repoFolder}");

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

            if (repoFolderInfo.Exists)
            {
                stopWatch.Stop();

                Console.Write($"[{branches.Count()} Branches]");
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($" [{stopWatch.Elapsed.TotalSeconds} s]");

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(" [Skipped]");
                Console.ResetColor();
                
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
            
            stopWatch.Stop();
            
            stopWatch.Stop();

            Console.Write($"[{branches.Count()} Branches]");
                
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" [{stopWatch.Elapsed.TotalSeconds} s]");

            Console.ResetColor();
        }
    }
}