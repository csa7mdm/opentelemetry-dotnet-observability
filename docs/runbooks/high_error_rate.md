# Runbook: High Error Rate (Alert: HighErrorRate)

## 🚨 Trigger Logic
- **Condition**: `Error Rate > 5%` for 2 minutes.
- **Metric**: `http_server_duration_count{status_code=~"5.."}` relative to total traffic.
- **Severity**: Critical.

## 🔍 Investigation Steps

1.  **Check the Dashboard**
    - Go to [Service Overview Dashboard](http://localhost:3000/d/ddn1k8v9lz0000/service-overview).
    - Look at the **Golden Signals** row -> **Error Rate** panel.
    - Check the **Top 5 Slowest Endpoints** table to see if specific endpoints are failing.

2.  **Check Logs**
    - Scroll down to the **Logs** panel.
    - Filter for `level="error"` and the `service_name`.
    - Look for recurring Exceptions (e.g., `PaymentFailedException`, `DatabaseTimeout`).

3.  **Trace Investigation**
    - If specific endpoints are failing, copy the `TraceID` from the logs (if available).
    - Go to the **Traces** panel and query by TraceID.
    - Identify the failing Span (often red).

## 🛠 Mitigation Strategies

- **Database Issues**: If logs show DB connection timeouts, check the Database health.
- **Bad Deployment**: If the error rate spiked immediately after a deployment, **Rollback** immediately.
- **Traffic Spike**: Check the **Request Rate** panel. If traffic is abnormal, it might be a DDOS or a misconfigured client.

## 🛑 Escalation
- If unable to resolve within 15 minutes, ping `@sre-team` in `#incidents` on Slack.
