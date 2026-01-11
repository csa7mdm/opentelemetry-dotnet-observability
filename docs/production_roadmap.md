# Production Roadmap: Scaling to Enterprise Standards

This document outlines the evolutionary path from the current **Reference Architecture** to an **Industrial Golden Standard** suitable for high-scale, multi-cluster Kubernetes environments.

## 1. Collector Deployment Strategy

| Component | Current Implementation | Industrial Golden Standard | reason |
|-----------|------------------------|----------------------------|--------|
| **Agent** | Single Gateway (Docker Compose) | **DaemonSet** + **Gateway** | **DaemonSets** run one Collector per Node. They collect local logs/metrics (host metrics, kubelet stats) and forward app telemetry to the Gateway. |
| **Gateway** | Single Replica | **Horizontal Pod Autoscaling (HPA)** | A Deployment of Collectors (Gateway) handles aggregation, tail-sampling, and routing to backends. Auto-scales based on RAM/CPU. |

### Diagram: Enterprise Pipeline
```mermaid
graph LR
    App[App Pod] -->|OTLP| Agent[OTel Agent (DaemonSet)]
    Agent -->|Enrich w/ K8s Metadata| Gateway[OTel Gateway (Deployment)]
    Gateway -->|Load Balancer| Backend[Observability Backend]
```

## 2. Sampling Strategy

| Strategy | Current Implementation | Industrial Golden Standard | Trade-off |
|----------|------------------------|----------------------------|-----------|
| **Method** | **Head-Based** (10%) | **Tail-Based Sampling** | Head-based drops 90% of requests randomly. **Tail-based** keeps 100% of traces in memory (at Gateway), waits for completion, and *then* decides to keep them only if they contain **Errors** or **High Latency**. |
| **Result** | Statistical averages are correct, but rare errors might be missed. | 100% of errors are captured, even if they only happen 0.1% of the time. | Higher CPU/RAM usage at the Gateway. |

## 3. Metric Transport (The "Push" vs. "Pull" Debate)

| Feature | Current Implementation | Industrial Golden Standard |
|---------|------------------------|----------------------------|
| **Protocol** | **Prometheus Native** (Scrape/Pull) | **OTLP** or **Remote Write** (Push) |
| **Scraping** | Prometheus scrapes the Collector. | Collector **pushes** metrics to a scalable backend (Mimir, Thanos, Cortex) via Prometheus Remote Write. |
| **Scale** | Single Prometheus instance. | **Stateless Collectors** pushing to a clustered retention backend. |

## 4. Storage & Retention

| Feature | Current Implementation | Industrial Golden Standard |
|---------|------------------------|----------------------------|
| **Storage** | Local Filesystem (Docker Volume) | **Object Storage** (S3/GCS) |
| **Component** | Monolithic Binary (Tempo/Loki) | **Microservices Mode** (Distributor, Ingester, Querier, Compactor) |
| **Cost** | Low (Disk) | Low (Object Storage is cheaper than Block Storage for petabytes of logs). |

## 5. Security & Governance

-   **mTLS**: Implement mutual TLS between Applications and Collectors.
-   **Network Policies**: Restrict egress traffic so only the Collector can talk to the internet/backend.
-   **Data Sovereignty**: Use `routing` processors to send EU user data to EU backends and US data to US backends.

## Summary Checklist for "Going Live"

- [ ] Migrate from Docker Compose to **Helm Charts** (OpenTelemetry Operator).
- [ ] Implement **Tail Sampling Processor** on the Gateway.
- [ ] Configure **S3 Backends** for Tempo and Loki.
- [ ] Enable **mTLS** for OTLP receivers.
