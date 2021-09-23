#!/bin/bash

edgecastKey="primary202109099dc4cf480b17a94f5eef938bdb08c18535bcc777cc0420c29133d0134d635aa78a1e28f6b883619ed5f920bd3cd79bfe10c42b5d96b7eeb84571ceee4cb51d89"
cdnDomainName="bld-cdn.geuer-pollmann.de"
storageAccountName="bltcdn"
storageContainerName="assets"
cdnOrigin="${storageContainerName}"

name="795162.jpeg"
file="/mnt/c/Users/chgeuer/Pictures/${name}"
customerName="customer2"

ipAddress="$( curl --silent "http://wtfismyip.com/text" )"

cdnToken="$( dotnet.exe fsi edgecast.fsx \
    --direction Encrypt \
    --key "${edgecastKey}" \
    --ipaddress "${ipAddress}" \
    --urls "https://${cdnDomainName}/${cdnOrigin}/${customerName}/" \
    )"

echo "The token is '${cdnToken}'"

dotnet.exe fsi edgecast.fsx \
    --direction Decrypt \
    --key "${edgecastKey}" \
    --token "${cdnToken}"

az storage blob upload \
    --account-name "${storageAccountName}" \
    --container-name "${storageContainerName}" \
    --file "${file}" \
    --name "${customerName}/${name}"

echo "https://${cdnDomainName}/${cdnOrigin}/${customerName}/${name}?t=${cdnToken}"
