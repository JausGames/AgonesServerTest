FROM ubuntu:18.04

WORKDIR /unity

COPY Builds/Server/ ./

# workaround
# wait until the sidecar is ready
CMD chmod +x ./Server.x86_64 && sleep 1 && ./Server.x86_64