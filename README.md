# http

A commandline utility for sending raw HTTP requests.

## Usage

Send a HTTP request defined in the given file.

```
http [<request-file>] [--http]
```

If `request-file` is supplied, it must be a path to a file containing an HTTP
request, or the string `-` to indicate that the request will be read from
`stdin`. If `request-file` is not supplied, the request will be read from
`stdin`.

The host and port for the request are read from the `Host` header of the
request. The request will use HTTPS by default, and will only use HTTP if the
`--http` option is present or the port defined in the `Host` header is `80`.

## Samples

This sample sends a HTTP/1.1 GET request to httpstat.us:443/200 via HTTPS.

```
$ cat ./request.txt
GET /200 HTTP/1.1
Host: httpstat.us
Connection: close

$ http ./request.txt
HTTP/1.1 200 OK
Content-Length: 6
Connection: close
Content-Type: text/plain
Date: ...
Server: ...
Set-Cookie: ...
Set-Cookie: ...
Domain=httpstat.us
Request-Context: ...
Strict-Transport-Security: ...

200 OK
```

This sample sends a HTTP/1.0 GET request to httpstat.us:80/418 via HTTP

```
$ cat ./request.txt
GET /418 HTTP/1.0
Host: httpstat.us

$ cat ./request.txt | http --http
HTTP/1.1 418 I'm a teapot
Content-Length: 16
Connection: close
Content-Type: text/plain
Date: ...
Server: ...
Set-Cookie: ...
Request-Context: ...

418 I'm a teapot
```

## Notes

Below are a few helpful reminders when using this utility.

* The `Host` header is required,
* The request must use CRLF (`\r\n`) linebreaks,
* There must be a blank line ending with CRLF after the headers (your text
    editor will likely display this as two blank lines),
* For HTTP/1.1 requests, you should specify `Connection: close` to avoid the
    connection remaining open after the response is received,
* When providing the request via `stdin`, it's recommended to pipe from the
    output of another program, e.g. `cat`. If you would rather paste a request,
    remember to send the `EOF` marker (`Ctrl`+`D` on Unix or `Ctrl`+`Z` on
    Windows).
