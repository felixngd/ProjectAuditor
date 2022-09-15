using NUnit.Framework;
using Unity.ProjectAuditor.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.ProjectAuditor.EditorTests
{
    class TextureTests : TestFixtureBase
    {
        const int resolution = 1;
        string currentPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
        Texture textureToCompare;
        TextureImporter textureViaImporter;

        [OneTimeSetUp]
        public void SetUp()
        {
            var texture = new Texture2D(resolution, resolution); //defaults: mipmaps = true & format = automatic
            texture.SetPixel(0, 0, Random.ColorHSV());
            texture.name = "ProceduralTextureForTest321.png";
            texture.Apply();

            var bytes = texture.EncodeToPNG();

            var tempTestTexture = new TempAsset(texture.name, bytes);

            var allTextures = AssetDatabase.FindAssets("t: Texture, a:assets");

            textureToCompare = (Texture)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allTextures[0]), typeof(Texture));
            textureViaImporter = (AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(allTextures[0])) as TextureImporter); //Needed since the texture/texture via TextureImporter properties are both used in the TextureModule script to access varying properties.
        }

        [Test]
        [Explicit]
        public void Texture_Properties_AreReported()
        {
            var textureTests = Analyze(IssueCategory.Texture);

            Assert.AreEqual(textureToCompare.name, textureTests[0].customProperties[0], "Checked Texture Name");

            Assert.AreEqual(textureViaImporter.textureShape.ToString(), textureTests[0].customProperties[1], "Checked Texture Shape/Dimension");

            Assert.AreEqual(textureViaImporter.textureType.ToString(), textureTests[0].customProperties[2], "Checked TextureImporterType ");

            Assert.AreEqual("AutomaticCompressed", textureTests[0].customProperties[3], "Checked Texture Compression");

            Assert.AreEqual(textureViaImporter.textureCompression.ToString(), textureTests[0].customProperties[4], "Checked Texture Shape/Dimension");

            Assert.AreEqual("True", textureTests[0].customProperties[5], "Checked MipMaps Enabled");

            Assert.AreEqual("False", textureTests[0].customProperties[6], "Checked Texture Read/Write");

            Assert.AreEqual((resolution + "x" + resolution).ToString(), textureTests[0].customProperties[7], "Checked Texture Resolution");

            Assert.AreEqual(currentPlatform, textureTests[0].customProperties[9], "Checked Platform");
        }
    }
}
