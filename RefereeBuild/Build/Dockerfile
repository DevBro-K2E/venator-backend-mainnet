# syntax=docker/dockerfile:1
FROM ubuntu:latest
WORKDIR /server
COPY /server .
RUN apt-get update && apt-get install -y ca-certificates
CMD ["./rserver.x86_64"]