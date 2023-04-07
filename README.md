# AgonesServerTest

## Build images : 
`docker build --target GameServer -t jausseran/gameserver .` 
`docker build --target MatchMakingAPI -t jausseran/mmapi .` 

## Run API : 
`docker run -d jausseran/mmapi -n mmapi` 

## Run gameserver : 
### Minikube using docker : 

``` 
sh minikube.sh start --namespace gameserver --driver docker --kubernetes-version v1.26.3 -p gamecluster 
sh minikube.sh start --namespace gameserver --driver docker --kubernetes-version v1.26.3
sh minikube.sh kubectl -- create namespace agones-system
sh minikube.sh kubectl -- create -f https://raw.githubusercontent.com/googleforgames/agones/main/install/yaml/install.yaml
sh minikube.sh kubectl -- create namespace gameserver
sh minikube.sh kubectl -- create -f https://raw.githubusercontent.com/jausgames/agonesservertest/main/gameserver.yaml --namespace gameserver
```

