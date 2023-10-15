# https://oidref.com/2.5.29.19 Basic constraints
# https://oidref.com/2.5.29.37 Certificate extension: "extKeyUsage" (Extended key usage)
# https://oidref.com/1.3.6.1.5.5.7.3.3 Indicates that a certificate can be used for code signing

pushd $PSScriptRoot
try {
    $thumbprints = @()
    try {
        $password = Read-Host -Prompt 'Enter the password used to protect the certificate' -AsSecureString
        $caCertificate = New-SelfSignedCertificate `
            -DnsName 'CN=Leon''s Root Certificate Authority' `
            -KeyUsage CRLSign, CertSign, DigitalSignature `
            -TextExtension @("2.5.29.19={text}cA=true")
        $thumbprints += $caCertificate.Thumbprint

        $cert = New-SelfSignedCertificate `
            -Signer $caCertificate `
            -KeyUsage DigitalSignature,KeyEncipherment `
            -DnsName localhost `
            -Type SSLServerAuthentication `
            -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")
        $thumbprints += $cert.Thumbprint

        Export-PfxCertificate $cert DeployableApp.pfx -Password $password
    } finally {
        dir -Recurse -Force Cert:\ -Include $cert.Thumbprint, $caCertificate.Thumbprint | % PSPath | % {
            del $_ -DeleteKey
        }
    }
} finally {
    popd
}
