# Telemetry Governance & Quality Standards

This document defines the standards for Observability at [Company/Project]. adherence is mandatory for all new services.

## 1. Telemetry Naming Standards

### Metrics
Format: `[service_name].[component].[measurement]`
-   **Structure**: Lowercase, snake_case.
-   **Units**: Suffix with unit (e.g., `_duration_seconds`, `_total`, `_bytes`).
-   **Examples**:
    -   ✅ `service_api_http_request_duration_seconds`
    -   ✅ `service_worker_jobs_processed_total`
    -   ❌ `ServiceAPI.HttpRequest` (Wrong case)
    -   ❌ `jobs_processed` (Missing unit/type)

### Attributes (Labels)
-   **Allowlist** (Safe to use):
    -   `customer_id` (High cardinality warning: only if < 100k active/hour)
    -   `region` (us-east-1, eu-west-1)
    -   `deployment_environment` (prod, staging)
    -   `error_type` (exception class name)
-   **Blocklist** (Strictly Forbidden in Metrics):
    -   `user_email` (PII)
    -   `session_id` (Unlimited cardinality)
    -   `request_id` (Unlimited cardinality - Use Traces for this!)
    -   `url_path` (Unless normalized, e.g., `/user/{id}` not `/user/123`)

## 2. CI Gates & Definition of Done (DoD)

### Definition of Done
A feature is NOT done until:
1.  [ ] **Golden Signals**: 4 Golden Signals (RED metrics) are visible in the Service Overview dashboard.
2.  [ ] **Alerts**: Appropriate alerts are configured for high error rates or latency.
3.  [ ] **Runbooks**: A runbook exists for every critical alert.
4.  [ ] **No PII**: Telemetry verified to be free of PII (or masked by Collector).

### CI Checks (Pipeline)
-   **Schema Validation**: Dashboard JSON must pass linting.
-   **Unit Tests**: `dotnet test` must pass (ensures instrumentation code compiles).
-   **Integration**: App must start and emit `up` metric in local Docker Compose.

## 3. Key Performance Indicators (KPIs)

We measure the success of our Observability practice using these metrics:

| KPI | Definition | Goal | Measurement |
| :--- | :--- | :--- | :--- |
| **MTTR** | Mean Time To Recovery | < 30 mins | Time from Alert Trigger -> Resolved Status |
| **Coverage** | % of Services with Golden Signals | 100% | Count of Services in Grafana vs CMDB |
| **False Positive Rate** | % of Alerts closed as "Not an Issue" | < 10% | Weekly Alert Review |
| **Overhead** | CPU/Mem cost of Telemetry | < 5% | Profiler / Container Metrics |
