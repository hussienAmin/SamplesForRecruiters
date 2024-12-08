using ClientShared;
using KafApp.Models;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace KafApp.Config
{
    public static class LicenseService
    {
        static LicenseModel model;

        public static string ProductName => model.ProductName;
        public static string LicenseKey => model.LicenseKey;
        public static string DeviceFingerPrint => model.DeviceFingerPrint;
        public static string Version => model.Version;
        public static string IssueDate => model.IssueDate;



        public static async Task<bool> LoadLicense()
        {


            try
            {
                var jsn = (await File.ReadAllTextAsync("KafLicense.json")).decrypt("5ourceytr!@k4k8y", "4our4eguyfb@k$iv");

                model = JsonSerializer.Deserialize<LicenseModel>(jsn!);
                if (!ISLicenseValid(model))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        static bool ISLicenseValid(LicenseModel md)
        {
            return (md.DeviceFingerPrint == (new DeviceFingerprintGenerator()).GetDeviceFingerprint())
                && md.ProductName == "BonusOffline"
                && md.ProductVersion == "1000"
                && md.Version == "1000"
                ;
        }
        public static async Task BuidTempLicense()
        {
            DeviceFingerprintGenerator generator = new DeviceFingerprintGenerator();
            var g = generator.GetDeviceFingerprint();
            model = new LicenseModel()
            {
                Version = "1000"
                ,
                DeviceFingerPrint = g,

                IssueDate = DateTime.Now.ToString()
                ,
                LicenseKey = "cghkcghkgfchk"
                ,
                ProductName = "BonusOffLine"
                ,
                ProductVersion = "1000"
                ,

                CustomerId = "hussien"
            };

            var json = JsonSerializer.Serialize<LicenseModel>(model);
            File.WriteAllText("KafLicense.json", json.encrypt("5ourceytr!@k4k8y", "4our4eguyfb@k$iv"));
        }

        public static async Task<bool> SetNewLicense(string EncreptedLicense)
        {
            try
            {

                var md = JsonSerializer.Deserialize<LicenseModel>(EncreptedLicense.decrypt("5ourceytr!@k4k8y", "4our4eguyfb@k$iv"));
                if (!ISLicenseValid(md))
                {
                    return false;
                }

                var json = JsonSerializer.Serialize<LicenseModel>(md);
                File.WriteAllText("KafLicense.json", json.encrypt("5ourceytr!@k4k8y", "4our4eguyfb@k$iv"));
                return true;
            }
            catch
            {
                return false;
            }

        }
        public static async Task<bool> SetNewLicenseFromFireStore(FirestoreDocument document)
        {
            try
            {
                var json = JsonSerializer.Serialize<LicenseModel>(document.GetLicense());
                File.WriteAllText("KafLicense.json", json.encrypt("5ourceytr!@k4k8y", "4our4eguyfb@k$iv"));
                return true;
            }
            catch
            {
                return false;
            }

        }

    }
    public class LicenseModel
    {

        public required string CustomerId { get; set; }

        public required string LicenseKey { get; set; }
        public required string DeviceFingerPrint { get; set; }
        public required string ProductName { get; set; }
        public required string ProductVersion { get; set; }
        public required string Version { get; set; }
        public required string IssueDate { get; set; }
    }

    public class FirestoreDocument
    {
        public string name { get; set; }
        public Dictionary<string, FirestoreField> fields { get; set; }
        public string createTime { get; set; }
        public string updateTime { get; set; }
        public LicenseModel GetLicense()
        {
            return new LicenseModel
            {
                Version = fields["Version"].StringValue
                ,
                DeviceFingerPrint = fields["DeviceFingerPrint"].StringValue,

                IssueDate = fields["IssueDate"].StringValue
                ,
                LicenseKey = fields["LicenseKey"].StringValue
                ,
                ProductName = fields["ProductName"].StringValue
                ,
                ProductVersion = fields["ProductVersion"].StringValue
                ,

                CustomerId = fields["CustomerId"].StringValue
            };
        }

    }
    public class FirestoreField
    {
        [JsonPropertyName("stringValue")]
        public string StringValue { get; set; } = string.Empty;
    }

}