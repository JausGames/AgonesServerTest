/*
Copyright 2016 The Kubernetes Authors.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

// Note: the example only works with the code within the same release/branch.
package main

import (
	"context"
	"encoding/json"
	"flag"
	"fmt"
	"net/http"
	"path/filepath"

	metav1 "k8s.io/apimachinery/pkg/apis/meta/v1"

	"agones.dev/agones/pkg/client/clientset/versioned"
	"k8s.io/client-go/tools/clientcmd"
	"k8s.io/client-go/util/homedir"
	//
	// Uncomment to load all auth plugins
	// _ "k8s.io/client-go/plugin/pkg/client/auth"
	//
	// Or uncomment to load specific auth plugins
	// _ "k8s.io/client-go/plugin/pkg/client/auth/azure"
	// _ "k8s.io/client-go/plugin/pkg/client/auth/gcp"
	// _ "k8s.io/client-go/plugin/pkg/client/auth/oidc"
)

type GameServer struct {
	Name      string `json:"name"`
	Kind      string `json:"kind"`
	Namespace string `json:"namespace"`
	Status    string `json:"status"`
	Ip        string `json:"ip"`
	Port      int32  `json:"port"`
	Players   int64  `json:"player_count"`
	MaxPlayer int64  `json:"player_max"`
	UID       string `json:"uid"`
}

func main() {
	var kubeconfig *string
	if home := homedir.Expand(); home != "" {
		kubeconfig = flag.String("kubeconfig", filepath.Join(home, "/usr/src/app", ".kube", "config"), "(optional) absolute path to the kubeconfig file")
	} else {
		kubeconfig = flag.String("kubeconfig", "C:\\Users\\user\\Documents\\GO\\k8s\\.kube\\config", "absolute path to the kubeconfig file")
	}
	//kubeconfig = flag.String("kubeconfig", "C:\\Users\\user\\Documents\\GO\\k8s\\.kube\\config", "absolute path to the kubeconfig file")
	flag.Parse()

	fmt.Printf(*kubeconfig + "\n")

	// use the current context in kubeconfig
	config, err := clientcmd.BuildConfigFromFlags("", *kubeconfig)
	if err != nil {
		panic(err.Error())
	}

	http.HandleFunc("/servers/ready", func(w http.ResponseWriter, r *http.Request) {

		// namespace := "gameserver"
		// //create the clientset
		// clientset, err := kubernetes.NewForConfig(config)
		// if err != nil {
		// 	panic(err.Error())
		// }

		// Access to the Agones resources through the Agones Clientset
		// Note that we reuse the same config as we used for the Kubernetes Clientset
		agonesClient, err := versioned.NewForConfig(config)
		if err != nil {
			//logger.WithError(err).Fatal("Could not create the agones api clientset")
		}

		listGS, err := agonesClient.AgonesV1().GameServers("gameserver").List(context.TODO(), metav1.ListOptions{})

		fmt.Printf("There are %d GS in the cluster\n", len(listGS.Items))

		jsonBytes := "{\"gameservers\":"

		for i := 0; i < len(listGS.Items); i++ {
			podname := listGS.Items[i].GetName()
			fmt.Printf(" - %s %s", podname, "\n")

			//pod, err := clientset.CoreV1().Pods(namespace).Get(context.TODO(), podname, metav1.GetOptions{})
			getGS, err := agonesClient.AgonesV1().GameServers("gameserver").Get(context.TODO(), listGS.Items[i].GetName(), metav1.GetOptions{})

			if err != nil {
				panic(err.Error())
			}
			var players int64
			players = 0
			if getGS.Status.Players != nil {
				players = getGS.Status.Players.Count
			}
			var max int64
			players = 0
			if getGS.Status.Players != nil {
				players = getGS.Status.Players.Capacity
			}

			gs := GameServer{
				Name:      getGS.Name,
				Kind:      getGS.Kind,
				Namespace: getGS.Namespace,
				Status:    string(getGS.Status.State),
				Ip:        getGS.Status.Address,
				Port:      getGS.Status.Ports[0].Port,
				Players:   players,
				MaxPlayer: max,
				UID:       string(getGS.UID),
			}

			json, err := json.Marshal(gs)
			if err != nil {
				panic(err)
			}
			if i > 0 {
				jsonBytes += ", "
			}
			// Convert the byte slice to a string and print it
			jsonString := string(json)
			jsonBytes += jsonString

			//append(jsonBytes, json)

		}
		jsonBytes += "}"
		fmt.Fprintf(w, jsonBytes)
	})

	if err := http.ListenAndServe(":8080", nil); err != nil {
		panic(err)
	}

	// for {
	// 	pods, err := clientset.CoreV1().Pods("").List(context.TODO(), metav1.ListOptions{})
	// 	if err != nil {
	// 		panic(err.Error())
	// 	}
	// 	fmt.Printf("There are %d pods in the cluster\n", len(pods.Items))

	// 	for i := 0; i < len(pods.Items); i++ {
	// 		fmt.Printf(" - ", pods.Items[i].Status.PodIP)
	// 	}

	// 	// Examples for error handling:
	// 	// - Use helper functions like e.g. errors.IsNotFound()
	// 	// - And/or cast to StatusError and use its properties like e.g. ErrStatus.Message
	// 	namespace := "gameserver"
	// 	pod := "game-server-xppts"

	// 	mypod, err = clientset.CoreV1().Pods(namespace).Get(context.TODO(), pod, metav1.GetOptions{})
	// 	if errors.IsNotFound(err) {
	// 		fmt.Printf("Pod %s in namespace %s not found\n", pod, namespace)
	// 	} else if statusError, isStatus := err.(*errors.StatusError); isStatus {
	// 		fmt.Printf("Error getting pod %s in namespace %s: %v\n",
	// 			pod, namespace, statusError.ErrStatus.Message)
	// 	} else if err != nil {
	// 		panic(err.Error())
	// 	} else {

	// 		fmt.Printf("Found pod %s in namespace %s\n", pod, namespace)
	// 	}

	// 	time.Sleep(10 * time.Second)
	// }
}
