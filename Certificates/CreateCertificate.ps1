# https://oidref.com/2.5.29.19 Basic constraints
# https://oidref.com/2.5.29.37 Certificate extension: "extKeyUsage" (Extended key usage)
# https://oidref.com/1.3.6.1.5.5.7.3.3 Indicates that a certificate can be used for code signing

# Import-PfxCertificate .\DeployableApp.pfx Cert:\CurrentUser\My -Password (ConvertTo-SecureString $env:CERTIFICATEPASSWORD -AsPlainText -Force)
# dir -Recurse -Force Cert:\ -Include 8BEDD2041F652587AF7F26A70A6B0CC83BC8AE8F, 244E9E97D19E462B62735D2C67A7024BFC559E33

pushd $PSScriptRoot
try {
    if (![Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'This script must be run as Administrator.'
    }
    if (!$env:CERTIFICATEPASSWORD) {
        throw 'Store the Certificate''s Password in $env:CERTIFICATEPASSWORD'
    }
    $password = ConvertTo-SecureString $env:CERTIFICATEPASSWORD -AsPlainText -Force

    $caCertificate = New-SelfSignedCertificate `
        -DnsName 'CN=Leon''s Root Certificate Authority' `
        -KeyUsage CRLSign, CertSign, DigitalSignature `
        -Type CodeSigningCert `
        -TextExtension @("2.5.29.19={text}cA=true")
    '{0} {1:o}' -f $caCertificate.Thumbprint, (Get-Date) | Out-File -Encoding utf8 -Append Thumbprints.txt

    $cert = New-SelfSignedCertificate `
        -Signer $caCertificate `
        -KeyUsage DigitalSignature `
        -Subject 'Leon''s Code Signing Certificate' `
        -Type CodeSigningCert
    '{0} {1:o}' -f $cert.Thumbprint, (Get-Date) | Out-File -Encoding utf8 -Append Thumbprints.txt

    $caCertificate = move $caCertificate.PSPath Cert:\LocalMachine\Root -PassThru

    Export-PfxCertificate $cert DeployableApp.pfx -Password $password -ChainOption BuildChain

    $bytes = cat DeployableApp.pfx -Encoding Byte -Raw
    $encoded = [Convert]::ToBase64String($bytes)
    $encoded | Out-File DeployableApp.pfx.txt -Encoding utf8
} finally {
    popd
}
