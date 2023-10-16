docker run -ti --rm -v %cd%/.cert:/apps -w /apps alpine/openssl req -x509 -nodes -new -sha256 -days 1024 -newkey rsa:2048 -keyout RootCA.key -out RootCA.pem -subj "/C=US/CN=__EAVFW__-__MainApp__-ROOT-CA"
docker run -ti --rm -v %cd%/.cert:/apps -w /apps alpine/openssl x509 -outform pem -in RootCA.pem -out RootCA.crt
docker run -ti --rm -v %cd%/.cert:/apps -w /apps alpine/openssl req -new -nodes -newkey rsa:2048 -keyout localhost.key -out localhost.csr -subj "/C=US/ST=YourState/L=YourCity/O=__EAVFW__-__MainApp__-Certificates/CN=localhost"
docker run -ti --rm -v %cd%/.cert:/apps -w /apps alpine/openssl x509 -req -sha256 -days 1024 -in localhost.csr -CA RootCA.pem -CAkey RootCA.key -CAcreateserial -extfile domains.ext -out localhost.crt
docker run -ti --rm -v %cd%/.cert:/apps -w /apps alpine/openssl pkcs12 -export -in localhost.crt -inkey localhost.key -out localhost.pfx -certfile RootCA.pem -passout file:password
echo "Add 127.0.0.1 local-__EAVFW__-__MainApp__ to %WINDIR%\System32\Drivers\Etc\Hosts"