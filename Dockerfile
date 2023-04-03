FROM ubuntu:18.04

WORKDIR /unity

COPY Builds/Server/ ./

# workaround
# wait until the sidecar is ready
CMD chmod +x ./Server.x86_64 && sleep 1 && ./Server.x86_64


# syntax=docker/dockerfile:1

FROM golang:1.16-alpine

WORKDIR /app

COPY go.mod ./
COPY go.sum ./
RUN go mod download

COPY *.go ./

RUN go build -o /fleetapi

EXPOSE 8080

CMD [ "/fleetapi" ]