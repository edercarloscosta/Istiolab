# Istio Lab

This project is a proof of concept focusing on Kubernetes deployment with Istio as a Service Mesh.

**The purpose is to create two simple apps:**

***First one:*** 

A gRPC server that is going to the web to get random jokes.

***Second one:*** 

A Client to the gRPC server consumption that triggers a call every 20 sec.

## Tech Stack
- NET7 WebApi gRPC template 
- NET7 Worker template

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

> istioctl install --set profile=demo -y

