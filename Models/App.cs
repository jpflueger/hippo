﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tomlyn;
using Tomlyn.Syntax;

namespace Hippo.Models
{
    public class App: BaseEntity
    {

        [Required]
        public string Name { get; set; }

        [Required]
        public Account Owner { get; set; }

        [Required]
        public List<Account> Collaborators { get; set; }

        [Required]
        public List<Domain> Domains { get; set; }

        [Required]
        public List<Release> Releases { get; set; }

        internal void DeployTo(string revision, string rootPath)
        {
            var release = Releases.Where(r => r.Revision == revision).Single();
            if (release == null)
            {
                throw new InvalidOperationException("release not found");
            }

            File.WriteAllText(release.Build.WagiConfigPath(rootPath), Toml.Parse(ConfigFor(release)).ToString());
            File.WriteAllText(SystemdServicePath(rootPath), SystemdServiceFor(release, rootPath));
            // TODO: start the systemd service before writing out the traefik config
            // https://github.com/deislabs/hippo/blob/e0a5ed97cd1b00ec93fb3515ed51c3c5b9ee02d0/releases/models.py#L34-L41
            // https://seshuk.com/2020-06-02-linux-exec-dotnetcore/
            File.WriteAllText(TraefikConfigPath(rootPath), Toml.Parse(TraefikConfig()).ToString());
        }

        // https://github.com/deislabs/hippo/blob/e0a5ed97cd1b00ec93fb3515ed51c3c5b9ee02d0/releases/models.py#L95-L135
        private string TraefikConfig()
        {
            if (!Domains.Any())
            {
                return "";
            }
            var routers = new Dictionary<string, object>();
            var services = new Dictionary<string, object>();
            var traefikConfig = new { Http = new { Routers = routers, Services = services}};
            // var pid = 0;
            var port = 0;
            var rule = new StringBuilder();
            var hosts = new List<string>();
            foreach (var domain in Domains)
            {
                hosts.Add(String.Format("Host(`{}`)", domain.Name));
            }
            rule.AppendJoin(" || ", hosts);
            rule.AppendFormat(" && PathPrefix(`/`)");
            routers.Add(
                String.Format("to-{}", Name),
                new Dictionary<string, string>
                {
                    {
                        "rule", rule.ToString()
                    },
                    {
                        "service", Name
                    }
                }
            );
            services.Add(
                Name,
                new Dictionary<string, object>
                {
                    {
                        "LoadBalancer",
                        new Dictionary<string, object>
                        {
                            {
                                "servers",
                                new List<Dictionary<string, string>>
                                {
                                    new Dictionary<string, string>
                                    {
                                        {
                                            "url", String.Format("http://localhost:{}", port)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            );
            return JsonSerializer.Serialize(traefikConfig, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        }

        private string TraefikConfigPath(string rootPath)
        {
            return Path.Combine(rootPath, "traefik", "conf.d", Name + ".toml");
        }

        private string SystemdServiceFor(Release release, string rootPath)
        {
            var systemdService = new StringBuilder();
            systemdService.AppendLine("[Unit]");
            systemdService.AppendFormat("Description=Hippo runtime for app {0}\n", Name);
            systemdService.AppendLine();
            systemdService.AppendLine("[Service]");
            systemdService.AppendLine("Type=simple");
            // TODO: make wagi system path configurable
            systemdService.AppendFormat("ExecStart=/usr/local/bin/wagi --config {0} --listen 0.0.0.0:0\n", release.Build.WagiConfigPath(rootPath));
            systemdService.AppendLine();
            systemdService.AppendLine("[Install]");
            systemdService.AppendLine("WantedBy=multi-user.target");
            return systemdService.ToString();
        }

        private string SystemdServicePath(string rootPath)
        {
            return Path.Combine(rootPath, "systemd", "hippo-" + Name + ".service");
        }

        private string ConfigFor(Release release)
        {
            var envvars = release.Config.EnvironmentVariables;
            var wagiConfig = new StringBuilder();
            foreach (Domain domain in Domains)
            {
                wagiConfig.AppendLine("[[module]]");
                wagiConfig.AppendFormat("module = \"{0}\"\n", release.Build.UploadUrl.ToString());
                foreach (EnvironmentVariable envvar in release.Config.EnvironmentVariables)
                {
                    wagiConfig.AppendFormat("environment.{0} = \"{1}\"\n", envvar.Key, envvar.Value);
                }
                wagiConfig.AppendLine();
            }
            return wagiConfig.ToString();
        }
    }
}
