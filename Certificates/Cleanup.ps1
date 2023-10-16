[CmdletBinding()]
param(
    [switch]
    $ConfirmDelete
)

pushd $PSScriptRoot
try {
    if (![Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'This script must be run as Administrator.'
    }

    $thumbprints = gc .\Thumbprints.txt -ErrorAction SilentlyContinue | % { ($_ -split ' ')[0] }
    if (!$thumbprints) {
        throw 'No Thumbprints are available. You probably never ran CreateCertificate.ps1.'
    }

    if($ConfirmDelete) {
        dir -Recurse -Force Cert:\ -Include $thumbprints | % PSPath | % { del $_ -DeleteKey -Verbose }
    } else {
        dir -Recurse -Force Cert:\ -Include $thumbprints
    }
} finally {
    popd
}
