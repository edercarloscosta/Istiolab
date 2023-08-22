# Istio Lab

This project is a proof of concept focusing on Kubernetes deployment with Istio as a Service Mesh.

**The purpose is to create two simple apps:**

***First one:*** 

A gRPC server that is going to the web to get [random jokes](https://official-joke-api.appspot.com/).

***Second one:*** 

A Client to the gRPC server consumption that triggers a call every 20 sec.

## Tech Stack
- NET7 WebApi gRPC template as gRPC Server
- NET7 Worker template as gRPC Client

## The Environment
- Docker
- Kind for kubernetes simulation
- Ubuntu 22.04 LTS as OS
- Rider as IDE

## Requirement

- [Docker Install](https://docs.docker.com/engine/install/)
- [Kind Install](https://kind.sigs.k8s.io/docs/user/quick-start/)
- [Istio](https://istio.io/latest/docs/setup/install/)

## Steps to reproduce

:memo: Create cluster
> kind create cluster --name <give_a_name>

:memo: Apply the Deployments

:warning: Those .yaml files are in the Infra folder, and I suggest following the step-by-step for a better understanding of Istio behavior

> kubectl apply -f infra/grpc_server.yaml
> kubectl apply -f infra/grpc_client.yaml

:memo: Install Istio

We'll install Istio setting the `demo` profile as the documentation suggests, here is the [Istio profile list](https://istio.io/v1.5/docs/setup/install/istioctl/#display-the-list-of-available-profiles) 

> istioctl install --set profile=demo -y

:warning: Following the prior steps will apply all this on default K8s namespace, I mean if you deploy this in a different
namespace you need to apply the next command indicating the namespace -n <namespace>

Execute the command below to see the deployments (pods/services) running on, the instruction `-n default` is optional but refers the warning above. 

> kubectl get po,svc -n default

Result Sample

```
pod/istiogrpc-v1-746844d9cc-pblmt   1/1     Running   8 (35m ago)    11d
pod/istiogrpc-v2-7dc7dc58c4-9wmh4   1/1     Running   8 (35m ago)    11d
pod/istiogrpc-v3-958c77b7f-48kdb    1/1     Running   8 (35m ago)    11d
pod/istioworker-7bc7c5d495-jr5h4    1/1     Running   26 (35m ago)   11d
pod/istioworker-7bc7c5d495-x7gwh    1/1     Running   24 (35m ago)   11d
```

So far so good, right now that we have our Grpc Server and Grpc Client running properly we need to apply Istio on our pods context.
(All we have done was a tradition k8s deployment, nothing new)

> kubectl label namespace default istio-injection=enabled

:warning: Trying to translate the command above, we are injecting the Istio "Infrastructure" that came from `demo` profile into the namespace default.

If all those steps is working well lets see the side cars raising up "beside" our Pods. We just need to "shake" the deployed pods, in other words, the deployed apps needs to understand somehow that
we changed "something" there, after this the magic will happen.

The easiest way to do this is simply delete Pods from the deployments. Look at the generated pods on your terminal copy/paste as I did below. I copied each one of Pods and paste with space between each one.

As defined into the K8s manifest, there is replica configured and it will never truly "remove" pods,  this command will to "kill" the current pod and a new one will raise up immediately.

> kubectl delete po -n default istiogrpc-v1-746844d9cc-pblmt istiogrpc-v2-7dc7dc58c4-9wmh4 istiogrpc-v3-958c77b7f-48kdb istioworker-7bc7c5d495-jr5h4 istioworker-7bc7c5d495-x7gwh

Give a minute or two or three :P and see the Pods on terminal again with the same prior command.

> kubectl get po,svc -n default

Now you will see the Pods on terminal again with the Label as `Ready 2/2`

Result Sample

```
NAME                                READY   STATUS    RESTARTS      AGE
pod/istiogrpc-v1-746844d9cc-x92lr   2/2     Running   0             15m
pod/istiogrpc-v2-7dc7dc58c4-9wmh4   2/2     Running   8 (81m ago)   10s
pod/istiogrpc-v3-958c77b7f-zbvqv    2/2     Running   0             24s
pod/istioworker-7bc7c5d495-7pttv    2/2     Running   2 (18s ago)   24s
pod/istioworker-7bc7c5d495-rgl5w    2/2     Running   2 (17s ago)   24s

```

### Using Istio infra:

As you can see on the Infra folder in the project root, there are many other .yaml files to be deployed on k8s, such as:

- [Grafana](https://grafana.com/)
- [Jaeger](https://www.jaegertracing.io/)
- [Prometheus](https://prometheus.io/)
- [Kiali](https://kiali.io/)

You may apply each one individually

> kubectl apply -f infra/<file_name>.yml

Or you may apply all those files at the same time, don't worry if it includes the already implemented files

> kubectl apply -f infra

### Opening the Infrastructure Services based on Istio:

Kiali Dashboard

> istioctl dashboard kiali

Prometheus Dashboard

> istioctl dashboard prometheus

Grafana Dashboard

> istioctl dashboard grafana

Jaeger Dashboard 
> istioctl dashboard jaeger

