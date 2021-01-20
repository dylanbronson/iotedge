// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Test.Helpers
{
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Test.Common;
    using Microsoft.Azure.Devices.Edge.Test.Common.Certs;
    using NUnit.Framework;

    public class X509ManualProvisioningFixture : ManualProvisioningFixture
    {
        protected EdgeRuntime runtime;

        [OneTimeSetUp]
        public async Task X509ProvisionEdgeAsync()
        {
            await Profiler.Run(
                async () =>
                {
                    using (var cts = new CancellationTokenSource(Context.Current.SetupTimeout))
                    {
                        CancellationToken token = cts.Token;
                        DateTime startTime = DateTime.Now;
                        string deviceId = DeviceId.Current.Generate();

                        (X509Thumbprint thumbprint, string certPath, string keyPath) = await this.CreateIdentityCertAsync(
                            deviceId, token);

                        EdgeDevice device = await EdgeDevice.GetOrCreateIdentityAsync(
                            deviceId,
                            Context.Current.ParentDeviceId,
                            this.iotHub,
                            AuthenticationType.SelfSigned,
                            thumbprint,
                            token);

                        Context.Current.DeleteList.TryAdd(device.Id, device);

                        this.runtime = new EdgeRuntime(
                            device.Id,
                            Context.Current.EdgeAgentImage,
                            Context.Current.EdgeHubImage,
                            Context.Current.Proxy,
                            Context.Current.Registries,
                            Context.Current.OptimizeForPerformance,
                            this.iotHub);

                        TestCertificates testCerts;
                        (testCerts, this.ca) = await TestCertificates.GenerateCertsAsync(device.Id, token);

                        await this.ConfigureDaemonAsync(
                            config =>
                            {
                                testCerts.AddCertsToConfig(config);
                                config.SetDeviceManualX509(
                                    device.HubHostname,
                                    device.Id,
                                    certPath,
                                    keyPath);
                                config.Update();
                                return Task.FromResult((
                                    "with x509 certificate for device '{Identity}'",
                                    new object[] { device.Id }));
                            },
                            device,
                            startTime,
                            token);
                    }
                },
                "Completed edge manual provisioning with self-signed certificate");
        }

        async Task<(X509Thumbprint, string, string)> CreateIdentityCertAsync(string deviceId, CancellationToken token)
        {
            (string, string, string) rootCa =
            Context.Current.RootCaKeys.Expect(() => new InvalidOperationException("Missing root CA keys"));
            string caCertScriptPath = Context.Current.CaCertScriptPath.Expect(
                () => new InvalidOperationException("Missing CA cert script path"));
            string idScope = Context.Current.DpsIdScope.Expect(
                () => new InvalidOperationException("Missing DPS ID scope"));

            CertificateAuthority ca = await CertificateAuthority.CreateAsync(
                deviceId,
                rootCa,
                caCertScriptPath,
                token);

            var identityCerts = await ca.GenerateIdentityCertificatesAsync(deviceId, token);

            // Generated credentials need to be copied out of the script path because future runs
            // of the script will overwrite them.
            string path = $"/etc/aziot/e2e_tests/{deviceId}";
            string certPath = $"{path}/device_id_cert.pem";
            string keyPath = $"{path}/device_id_cert_key.pem";

            Directory.CreateDirectory(path);
            File.Copy(identityCerts.CertificatePath, certPath);
            OsPlatform.Current.SetOwner(certPath, "aziotcs", "644");
            File.Copy(identityCerts.KeyPath, keyPath);
            OsPlatform.Current.SetOwner(keyPath, "aziotks", "600");

            X509Certificate2 deviceCert = new X509Certificate2(identityCerts.CertificatePath);

            return (new X509Thumbprint()
            {
                PrimaryThumbprint = deviceCert.Thumbprint,
                SecondaryThumbprint = deviceCert.Thumbprint
            },
            certPath,
            keyPath);
        }
    }
}
