// See https://aka.ms/new-console-template for more information
using CommandLine;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NugetCleaner.CLI;
using System.Net.Http.Headers;

ILogger logger = NullLogger.Instance;

await Parser.Default.ParseArguments<CleanerOptions>(args)
       .WithParsedAsync(async o =>
       {
           var repository = Repository.Factory.GetCoreV3(o.PackageSource);
           var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>();

           var searchMetadata = await packageMetadataResource.GetMetadataAsync(o.PackageId, includePrerelease: true, includeUnlisted: true, new SourceCacheContext(), NullLogger.Instance, default);
           var versions = searchMetadata.Select(metadata => metadata.Identity.Version).OrderByDescending(version => version).ToList();

           if (versions.Count <= 3)
           {
               Console.WriteLine("No versions to delete. The package has 3 or fewer versions.");
               return;
           }

           var versionsToDelete = versions.Skip(3).ToList();
           var resource = await repository.GetResourceAsync<PackageUpdateResource>();
           if (!o.Confirm)
           {
               //ask for confirmation
               Console.WriteLine($"The following versions will be deleted: {string.Join(", ", versionsToDelete)}");
               Console.WriteLine("Do you want to continue? (y/n)");
               var response = Console.ReadLine();
               if (response != "y")
               {
                   Console.WriteLine("Operation cancelled.");
                   return;
               }
           }
           foreach (var version in versionsToDelete)
           {
               Console.WriteLine($"Deleting version {version}");
               // catch error 404
               try
               {
                   await resource.Delete(
                       o.PackageId,
                       version.ToNormalizedString(),
                       getApiKey: packageSource => o.ApiKey,
                       confirm: packageSource => true,
                       noServiceEndpoint: false,
                       logger);
               }
               catch (HttpRequestException ex) //when ex.HttpRequestError. == System.Net.HttpStatusCode.NotFound
               {
                   Console.WriteLine($"Version {version} not found. Skipping.");
               }
           }
       });

