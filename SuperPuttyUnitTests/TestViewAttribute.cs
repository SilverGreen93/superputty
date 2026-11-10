using System;

namespace SuperPuttyUnitTests
{
    /// <summary>
    /// Marks a method that brings up a SuperPuTTY view by hand for eyeballing it.
    /// These are not automated tests: they need a message loop and a screen, so the
    /// test runner skips them.  They used to be listed by the WinForms harness that
    /// this project used to build as its entry point.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TestViewAttribute : Attribute
    {
    }
}
