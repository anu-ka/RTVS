﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    [Collection(CollectionNames.NonParallel)]
    public class PackageIndexTest : IAsyncLifetime {
        private readonly TestCoreShell _shell = TestCoreShell.CreateBasic();
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IRSessionProvider _sessionProvider;

        public PackageIndexTest(IExportProvider exportProvider) {
            REditorSettings.Initialize(new TestSettingsStorage());
            _shell.ServiceManager.AddService(new TestRToolsSettings());
            _workflowProvider = exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            _sessionProvider = UIThreadHelper.Instance.Invoke(() => _workflowProvider.GetOrCreate()).RSessions;
        }
        
        public Task InitializeAsync() => _sessionProvider.TrySwitchBrokerAsync(nameof(PackageIndexTest));

        public Task DisposeAsync() => Task.CompletedTask;

        [Test]
        public async Task BuildPackageIndexTest() {
            string[] packageNames = {
                "base",
                "boot",
                "class",
                "cluster",
                "codetools",
                "compiler",
                "datasets",
                "foreign",
                "graphics",
                "grDevices",
                "grid",
                "KernSmooth",
                "lattice",
                "MASS",
                "Matrix",
                "methods",
                "mgcv",
                "nlme",
                "nnet",
                "parallel",
                "rpart",
                "spatial",
                "splines",
                "stats",
                "stats4",
                "survival",
                "tcltk",
                "tools",
                "translations",
                "utils",
             };

            IPackageIndex packageIndex;
            using (var host = new IntelliSenseRSession(_shell, _workflowProvider)) {
                await host.StartSessionAsync();
                var functionIndex = new FunctionIndex(_shell, null, host);
                packageIndex = new PackageIndex(_workflowProvider, _shell, host, functionIndex);
                await packageIndex.BuildIndexAsync();
            }

            foreach (var name in packageNames) {
                IPackageInfo pi = await packageIndex.GetPackageInfoAsync(name);
                pi.Should().NotBeNull();
                pi.Name.Should().Be(name);
            }
        }

        [Test]
        public async Task PackageDescriptionTest() {
            PackageIndex packageIndex;
            using (var host = new IntelliSenseRSession(_shell, _workflowProvider)) {
                await host.StartSessionAsync();
                var functionIndex = new FunctionIndex(_shell, null, host);
                packageIndex = new PackageIndex(_workflowProvider, _shell, host, functionIndex);
                await packageIndex.BuildIndexAsync();
            }
            IPackageInfo pi = await packageIndex.GetPackageInfoAsync("base");
            pi.Description.Should().Be("Base R functions.");
        }
    }
}
