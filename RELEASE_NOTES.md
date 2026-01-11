# Release Notes - v0.1.0-alpha

## 🚀 Two-Tier Enterprise Architecture
This alpha release introduces a production-ready OpenTelemetry architecture simulating a Kubernetes environment using Docker Compose.

### Key Features
*   **Split Collector Architecture**:
    *   **Agent**: Sidecar pattern for PII scrubbing and enrichment.
    *   **Gateway**: Centralized aggregator for Tail-Based Sampling.
*   **Security Hardening**:
    *   **TLS Termination**: NGINX reverse proxy enforces HTTPS.
    *   **Authentication**: Grafana is secured with `admin/admin` credentials.
    *   **Privacy**: Automatic redaction of user emails in traces.
*   **Golden Signals**: Pre-configured Grafana dashboards for Latency, Traffic, Errors, and Saturation.

### 🛠 Tech Stack
*   .NET 9 Microservices
*   OpenTelemetry Collector Contrib
*   Prometheus, Loki, Tempo, Grafana

### 📝 Documentation
*   New [Wiki](docs/Home.md) launched with Architecture and Security guides.
