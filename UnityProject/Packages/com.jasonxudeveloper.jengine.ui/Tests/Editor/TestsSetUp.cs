// TestsSetUp.cs
// Assembly-level test setup for JEngine.UI.Editor.Tests

using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

/// <summary>
/// Assembly-level setup that runs once before all tests in this assembly.
/// By not having a namespace, this SetUpFixture applies to ALL tests in the assembly.
/// Disables auto-baking to reduce "Bake Ambient Probe" errors during scene operations.
/// </summary>
[SetUpFixture]
public class JEngineUITestsSetUp
{
#pragma warning disable 618
    private Lightmapping.GIWorkflowMode _oldGiWorkflowMode;
#pragma warning restore 618

    [OneTimeSetUp]
    public void GlobalSetUp()
    {
        // Disable auto-baking to reduce "Bake Ambient Probe" errors during scene operations
#pragma warning disable 618
        _oldGiWorkflowMode = Lightmapping.giWorkflowMode;
        Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#pragma warning restore 618
    }

    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        // Restore old lighting mode
#pragma warning disable 618
        Lightmapping.giWorkflowMode = _oldGiWorkflowMode;
#pragma warning restore 618
    }
}
