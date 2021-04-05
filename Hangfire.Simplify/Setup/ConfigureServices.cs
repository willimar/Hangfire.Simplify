using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.Simplify.Entities;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigureServices
    {
        private static ConfigureOptions _options;

        private static MongoUrlBuilder GetMongoUrlBuilder(string connnectionString)
        {
            return new MongoUrlBuilder(connnectionString);
        }

        private static IMongoClient GetMongoClient(MongoUrlBuilder mongoUrlBuilder)
        {
            return new MongoClient(mongoUrlBuilder.ToMongoUrl());
        }

        public static void ConfigureHangfireService(this IServiceCollection services, ConfigureOptions options)
        {
            _options = options;
            var mongoUrlBuilder = GetMongoUrlBuilder(options.ConnnectionString);
            var mongoClient = GetMongoClient(mongoUrlBuilder);

            // Add Hangfire services. Hangfire.AspNetCore nuget required
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(mongoClient as MongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    },
                    Prefix = options.MongoPrefix,
                    CheckConnection = true
                })
            );

            var camelCaseConventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", camelCaseConventionPack, type => true);
        }

        public static void RemoveUnusedHangfireServers(this IApplicationBuilder app)
        {
            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

            foreach (var item in monitoringApi.Servers())
            {
                var name = item.Name.Split(':')[0];

                if (!_options.HangfireServerName.Any(x => x.ToLower().Equals(name.ToLower())))
                {
                    JobStorage.Current.GetConnection().RemoveServer(item.Name);
                }
            }
        }

        public static void SetupHangfireServers(this IApplicationBuilder app)
        {
            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

            foreach (var item in _options.HangfireServerName)
            {
                if (!monitoringApi.Servers().Any(x => x.Name.ToLower().Contains(item.ToLower())))
                {
                    var options = new BackgroundJobServerOptions
                    {
                        ServerName = $"{item}"
                    };
                    app.UseHangfireServer(options);
                }
            }
        }

        public static void ConfigureHangfireApplication(this IApplicationBuilder app, bool useLikeStandartPage)
        {
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            if (useLikeStandartPage)
            {
                var option = new RewriteOptions();
                option.AddRedirect("^$", "hangfire");
                app.UseRewriter(option);
            }
        }
    }
}
