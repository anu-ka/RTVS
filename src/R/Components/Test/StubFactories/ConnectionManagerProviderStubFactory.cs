﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.ConnectionManager;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    public sealed class ConnectionManagerProviderStubFactory {
        public static IConnectionManagerProvider CreateDefault() {
            return Substitute.For<IConnectionManagerProvider>();
        }
    }
}