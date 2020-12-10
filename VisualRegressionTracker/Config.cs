using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace VisualRegressionTracker
{
    public class Config
    {
        private static readonly JsonSerializer serializer = new JsonSerializer();
        public const string DefaultPath = "vrt.json";

        [JsonProperty("apiUrl")]
        public string ApiUrl { get; set; } = "http://localhost:4200";
        [JsonProperty("ciBuildId")]
        public string CiBuildId { get; set; }
        [JsonProperty("branchName")]
        public string BranchName { get; set; }
        [JsonProperty("project")]
        public string Project { get; set; }
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
        [JsonProperty("enableSoftAssert")]
        public bool EnableSoftAssert { get; set; }

        public void CheckComplete()
        {
            if (string.IsNullOrEmpty(ApiUrl))
                throw new MissingConfigurationError(nameof(ApiUrl), $"{nameof(ApiUrl)} is not specified.'");
            if (string.IsNullOrEmpty(BranchName))
                throw new MissingConfigurationError(nameof(BranchName), $"{nameof(BranchName)} is not specified.'");
            if (string.IsNullOrEmpty(Project))
                throw new MissingConfigurationError(nameof(Project), $"{nameof(Project)} is not specified.'");
            if (string.IsNullOrEmpty(ApiKey))
                throw new MissingConfigurationError(nameof(ApiKey), $"{nameof(ApiKey)} is not specified.'");
        }

        public static Config GetDefault(string path = null)
        {
            var default_path = Path.Join(determine_config_path(), DefaultPath);
            Config cfg;

            if (path != null)
                cfg = FromFile(path);
            else if (File.Exists(default_path))
                cfg = FromFile(default_path);
            else 
                cfg = new Config();

            cfg.ApplyEnvironment();

            try
            {
                cfg.CheckComplete();
            }
            catch (MissingConfigurationError ex)
            {
                throw new MissingConfigurationError(
                    ex.FieldName,
                    $"Incomplete configuration, {ex.FieldName} was not specified."
                    + $"Please provide via the constructor, a config file '{DefaultPath}', or environment variables.");
            }

            return cfg;
        }

        public static Config FromFile(string path)
        {
            using (var file = File.OpenText(path))
            {
                var config = (Config)serializer.Deserialize(file, typeof(Config));
                return config;
            }
        }

        public void ApplyEnvironment()
        {
            maybe_apply_env("VRT_APIURL", v => ApiUrl = v);
            maybe_apply_env("VRT_CIBUILDID", v => CiBuildId = v);
            maybe_apply_env("VRT_BRANCHNAME", v => BranchName = v);
            maybe_apply_env("VRT_PROJECT", v => Project = v);
            maybe_apply_env("VRT_APIKEY", v => ApiKey = v);
            maybe_apply_env("VRT_ENABLESOFTASSERT", v => 
                EnableSoftAssert = new[] {"true", "1"}.Contains(v.ToLowerInvariant()));
        }

        private static void maybe_apply_env(string name, Action<string> action)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(value))
                action(value);
        }

        private static string determine_config_path()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}