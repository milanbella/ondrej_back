#!/bin/bash
set -e

# private key
set -x
openssl genrsa -out certs/key.pem 4096
set +x

# public key (no password protected)
set -x
openssl req -new -x509 -key certs/key.pem -out certs/cert.crt -days 90000 
set +x

# put public and private key in new pkcs12 keystore 
set -x
winpty openssl pkcs12 -export -out certs/key_store.pfx -inkey certs/key.pem -in certs/cert.crt
rm certs/key.pem
set +x

# to list the keystore (you must specify password to encrypt the displayed private key)
# winpty openssl pkcs12 -info -in certs/key_store.pfx

# to list the keystore (displayed private key is not encrypted by password)
# winpty openssl pkcs12 -info -in certs/key_store.pfx -nodes
