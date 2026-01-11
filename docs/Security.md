# Security Hardening

This document outlines the security measures implemented to protect the Observability Stack.

## 1. TLS Termination (Ingress)
Direct access to Grafana (port 3000) is **blocked**. All traffic must pass through the **NGINX Reverse Proxy** on port 443.

*   **URL**: `https://localhost`
*   **Certificates**: Self-signed `nginx/certs/localhost.crt`.
*   **Config**: `nginx/nginx.conf`.

## 2. Authentication
Anonymous access to Grafana is disabled.
*   **Default User**: `admin`
*   **Default Password**: `admin`

## 3. PII Redaction (Data Privacy)
We implement "Shift-Left Security" by scrubbing PII at the **Agent** level using the OTel `transform` processor.

```yaml
# otel-agent-config.yaml
transform:
  trace_statements:
    - context: span
      statements:
        - replace_pattern(attributes["user.email"], ".*", "*****")
```
This ensures sensitive data never leaves the node.
