
$pfx_cert = Get-Content '.\Package_TemporaryKey.pfx' -Encoding Byte 
[System.Convert]::ToBase64String($pfx_cert) | Out-File 'SigningCertificate_Encoded.txt'