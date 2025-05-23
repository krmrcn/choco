﻿// Copyright © 2017 - 2025 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace chocolatey.infrastructure.app.domain.installers
{
    public interface IInstaller
    {
        InstallerType InstallerType { get; }
        string InstallExecutable { get; }
        string SilentInstall { get; }
        string NoReboot { get; }
        string LogFile { get; }
        string CustomInstallLocation { get; }
        string Language { get; }
        string OtherInstallOptions { get; }
        string UninstallExecutable { get; }
        string SilentUninstall { get; }
        string OtherUninstallOptions { get; }
        IEnumerable<long> ValidInstallExitCodes { get; }
        IEnumerable<long> ValidUninstallExitCodes { get; }

        string BuildInstallCommandArguments(bool logFile, bool customInstallLocation, bool languageRequested);
        string BuildUninstallCommandArguments();

#pragma warning disable IDE0022, IDE1006
        [Obsolete("This overload is deprecated and will be removed in v3.")]
        string build_install_command_arguments(bool logFile, bool customInstallLocation, bool languageRequested);

        [Obsolete("This overload is deprecated and will be removed in v3.")]
        string build_uninstall_command_arguments();
#pragma warning restore IDE0022, IDE1006
    }
}
