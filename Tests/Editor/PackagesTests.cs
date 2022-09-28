using System.Linq;
using NUnit.Framework;
using Unity.ProjectAuditor.Editor;
using Unity.ProjectAuditor.Editor.Modules;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Unity.ProjectAuditor.EditorTests
{
    class PackagesTests : TestFixtureBase
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var request = Client.Add("com.unity.2d.pixel-perfect@3.0.2");
            while (!request.IsCompleted)
                System.Threading.Thread.Sleep(10);
            Assert.True(request.Status == StatusCode.Success);

            request = Client.Add("com.unity.services.vivox");
            while (!request.IsCompleted)
                System.Threading.Thread.Sleep(10);
            Assert.True(request.Status == StatusCode.Success);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            var request = Client.Remove("com.unity.2d.pixel-perfect");
            while (!request.IsCompleted)
                System.Threading.Thread.Sleep(10);
            Assert.True(request.Status == StatusCode.Success);

            request = Client.Remove("com.unity.services.vivox");
            while (!request.IsCompleted)
                System.Threading.Thread.Sleep(10);
            Assert.True(request.Status == StatusCode.Success);
        }

        [Test]
        public void Packages_Installed_AreValid()
        {
            var installedPackages = Analyze(IssueCategory.Package);
#if !UNITY_2019_1_OR_NEWER
            // for some reason com.unity.ads is missing the description in 2018.x
            installedPackages = installedPackages.Where(p => !p.GetCustomProperty(PackageProperty.PackageID).Equals("com.unity.ads")).ToArray();
#endif
            foreach (var package in installedPackages)
            {
                Assert.AreNotEqual(string.Empty, package.description, "Package: " + package.GetCustomProperty(PackageProperty.PackageID));
                Assert.AreNotEqual(string.Empty, package.GetCustomProperty(PackageProperty.PackageID), "Package: " + package.description);
                Assert.AreNotEqual(string.Empty, package.GetCustomProperty(PackageProperty.Source), "Package: " + package.description);
                Assert.AreNotEqual(string.Empty, package.GetCustomProperty(PackageProperty.Version), "Package: " + package.description);
            }
        }

        [Test]
        [TestCase("Project Auditor", "com.unity.project-auditor", "Local", new string[] { "com.unity.nuget.mono-cecil" })]
        [TestCase("Audio", "com.unity.modules.audio", "BuiltIn")]
#if UNITY_2019_1_OR_NEWER
        [TestCase("Test Framework", "com.unity.test-framework", "Registry", new[] { "com.unity.ext.nunit", "com.unity.modules.imgui", "com.unity.modules.jsonserialize"})]
#endif
        public void Package_Installed_IsReported(string description, string name, string source, string[] dependencies = null)
        {
            var installedPackages = Analyze(IssueCategory.Package);
            var matchIssue = installedPackages.FirstOrDefault(issue => issue.description == description);

            Assert.IsNotNull(matchIssue, "Package {0} not found. Packages: {1}", description, string.Join(", ", installedPackages.Select(p => p.description).ToArray()));
            Assert.AreEqual(matchIssue.GetCustomProperty(PackageProperty.PackageID), name);
            Assert.IsTrue(matchIssue.GetCustomProperty(PackageVersionProperty.RecommendedVersion).StartsWith(source), "Package: " + description);

            if (dependencies != null)
            {
                for (var i = 0; i < dependencies.Length; i++)
                {
                    Assert.IsTrue(matchIssue.dependencies.GetChild(i).GetName().Contains(dependencies[i]), "Package: " + description);
                }
            }
        }

        [Test]
        public void Package_Upgrade_IsRecommended()
        {
            var packageDiagnostics = Analyze(IssueCategory.PackageVersion);
            var diagnostic = packageDiagnostics.FirstOrDefault(issue => issue.GetCustomProperty(PackageVersionProperty.PackageID) == "com.unity.2d.pixel-perfect");

            Assert.IsNotNull(diagnostic, "Cannot find the upgrade package: com.unity.2d.pixel-perfect");
            Assert.AreEqual(diagnostic.GetCustomProperty(PackageVersionProperty.PackageID), "com.unity.2d.pixel-perfect");
            Assert.AreEqual(diagnostic.GetCustomProperty(PackageVersionProperty.CurrentVersion), "3.0.2");

            var currentVersion = diagnostic.GetCustomProperty(PackageVersionProperty.CurrentVersion);
            var recommendedVersion = diagnostic.GetCustomProperty(PackageVersionProperty.RecommendedVersion);

            Assert.AreNotEqual(currentVersion, recommendedVersion, "The current and recommended versions should be different");
        }

        [Test]
        public void Package_Preview_IsReported()
        {
            var packageDiagnostics = Analyze(IssueCategory.PackageVersion);
            var diagnostic = packageDiagnostics.FirstOrDefault(issue => issue.GetCustomProperty(PackageVersionProperty.PackageID) == "com.unity.services.vivox");

            Assert.IsNotNull(diagnostic, "Cannot find the upgrade package: com.unity.services.vivox");
            Assert.AreEqual(diagnostic.GetCustomProperty(PackageVersionProperty.PackageID), "com.unity.services.vivox");
            Assert.AreEqual(diagnostic.GetCustomProperty(PackageVersionProperty.CurrentVersion), "15.1.180001-pre.5");
            Assert.AreEqual(diagnostic.GetCustomProperty(PackageVersionProperty.RecommendedVersion), "");
            Assert.AreEqual(diagnostic.GetCustomProperty(PackageVersionProperty.Experimental), "True");
        }
    }
}
