#!/usr/bin/env bash
# End-to-end check of Stage Manager on a kind cluster: installs the chart with the operator image built from
# this checkout, applies a Performance and asserts what the operator does with it. Needs docker, kind, kubectl,
# helm and the .NET SDK on the path, and a kind cluster as the current kubectl context. CI runs it in the
# "Stage Manager (kind)" job of build.yml.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cluster="${KIND_CLUSTER_NAME:-stage-manager}"
image="cratis/showrunner-operator:e2e"
namespace="apps"
secret_value="e2e-fake-client-secret-7c2d"

fail() { echo "FAILED: $*" >&2; exit 1; }

wait_for() { # description, shell condition (evaluated on every attempt)
  local description="$1" condition="$2"
  for _ in $(seq 1 60); do
    if eval "$condition" >/dev/null 2>&1; then return 0; fi
    sleep 2
  done
  fail "timed out waiting for $description"
}

cd "$root"
dotnet publish Source/DotNET/StageManager/StageManager.csproj -c Release -p:CratisProxiesSkip=true -o Source/DotNET/StageManager/out
docker build -f Docker/StageManager/Dockerfile -t "$image" .
kind load docker-image "$image" --name "$cluster"
"$root/scripts/stage-manager-crds.sh"

# The operator may reach Secrets and workloads only in the namespaces it manages, and the chart creates a Role
# there, so the namespace of the Performances exists first.
kubectl create namespace "$namespace" --dry-run=client -o yaml | kubectl apply -f -

helm upgrade --install stage-manager Source/Helm/showrunner-operator --namespace stage-manager --create-namespace \
  --set image.repository=cratis/showrunner-operator --set image.tag=e2e --set image.pullPolicy=Never \
  --set "managedNamespaces={$namespace}" --wait --timeout 180s

for crd in performances productions understudies restores rehearsals backstagepasses controlrooms; do
  kubectl wait --for=condition=Established "crd/$crd.showrunner.cratis.io" --timeout=60s
done

# The operator has no cluster-wide access to Secrets or workloads, only access in the namespaces it manages.
operator="system:serviceaccount:stage-manager:stage-manager-showrunner-operator"
for access in "get secrets" "create deployments.apps"; do
  kubectl auth can-i $access --as="$operator" -n "$namespace" >/dev/null \
    || fail "the operator cannot $access in the namespace it manages"
  if kubectl auth can-i $access --as="$operator" -n default >/dev/null; then
    fail "the operator can $access outside the namespaces it manages"
  fi
done

# A Chronicle connection Secret, an environment file with plain values and a Performance that misses one key.
kubectl -n "$namespace" create secret generic chronicle-connection --from-literal=connectionString=chronicle://e2e:e2e@chronicle:35000 --dry-run=client -o yaml | kubectl apply -f -
cat <<YAML | kubectl apply -f -
apiVersion: v1
kind: ConfigMap
metadata:
  name: crm-environment
  namespace: $namespace
data:
  production.yaml: |
    secretsProvider: passphrase
    config:
      authentication.providers.GitHub.clientId: e2e-client-id
---
apiVersion: showrunner.cratis.io/v1alpha1
kind: Performance
metadata:
  name: crm
  namespace: $namespace
spec:
  chronicle:
    connectionSecretRef:
      name: chronicle-connection
  application:
    image: registry.k8s.io/pause:3.10
    eventStores: [Crm]
  environment:
    name: production
    configMapRef:
      name: crm-environment
  authProxy:
    enabled: true
    image: registry.k8s.io/pause:3.10
    providers:
      - kind: GitHub
        identity: GitHub
  ingress:
    enabled: true
    hosts: [crm.example.test]
YAML

# 1. A missing environment key blocks the rollout and is reported; nothing is created.
wait_for "the Performance to be Blocked" '[ "$(kubectl -n "$namespace" get performance crm -o jsonpath="{.status.phase}")" = "Blocked" ]'
kubectl -n "$namespace" get performance crm -o jsonpath='{.status.missingEnvironmentKeys[*]}' | grep -q 'authentication.providers.GitHub.clientSecret' \
  || fail "the missing client secret was not reported"
test -z "$(kubectl -n "$namespace" get deployments -o name)" || fail "a Deployment was created while the environment was incomplete"

# 2. Completing the environment rolls the Performance out.
environment_file="$(mktemp)"
trap 'rm -f "$environment_file"' EXIT
cat > "$environment_file" <<YAML
secretsProvider: passphrase
config:
  authentication.providers.GitHub.clientId: e2e-client-id
  authentication.providers.GitHub.clientSecret: $secret_value
YAML
kubectl -n "$namespace" create configmap crm-environment --from-file=production.yaml="$environment_file" --dry-run=client -o yaml | kubectl apply -f -
wait_for "the Performance to roll out" 'kubectl -n "$namespace" get deployment crm-authproxy -o name'
wait_for "the Performance to leave Blocked" '[ "$(kubectl -n "$namespace" get performance crm -o jsonpath="{.status.phase}")" != "Blocked" ]'

# 3. AuthProxy is the only public entry point; the application stays ClusterIP.
test "$(kubectl -n "$namespace" get service crm-app -o jsonpath='{.spec.type}')" = "ClusterIP" || fail "the application service is not ClusterIP"
test "$(kubectl -n "$namespace" get ingress crm -o jsonpath='{.spec.rules[0].http.paths[0].backend.service.name}')" = "crm-authproxy" || fail "the ingress does not route to AuthProxy"

# 4. No secret value in a Deployment, the status or the operator log.
if kubectl -n "$namespace" get deployments -o yaml | grep -q "$secret_value"; then fail "a secret value is in a Deployment"; fi
if kubectl -n "$namespace" get performance crm -o yaml | grep -q "$secret_value"; then fail "a secret value is in the status"; fi
if kubectl -n stage-manager logs deployment/stage-manager-showrunner-operator | grep -q "$secret_value"; then fail "a secret value is in the operator log"; fi
test "$(kubectl -n "$namespace" get secret crm-authproxy-environment -o jsonpath='{.data.authentication\.providers\.GitHub\.clientSecret}' | base64 -d)" = "$secret_value" \
  || fail "the client secret is not in the mounted Secret"

# 5. A published Stage application: the generic runtime image (a placeholder here, so it never becomes ready) with a
# package from an immutable ConfigMap and a storage Secret. The rollout is not ready in time, so it is recorded as failed.
storage_value="mongodb://e2e-user:e2e-storage-password-4f1@mongo.example.test:27017/shop"
kubectl -n "$namespace" create secret generic shop-storage --from-literal=connectionString="$storage_value" --dry-run=client -o yaml | kubectl apply -f -
cat <<YAML | kubectl apply -f -
apiVersion: v1
kind: ConfigMap
metadata:
  name: shop-package-1-0-0
  namespace: $namespace
immutable: true
binaryData:
  package.tar.gz: H4sIAHdd52oC/+3BAQ0AAADCoPdPbQ43oAAAAAAAAAAAAIA3A5reHScAKAAA
---
apiVersion: showrunner.cratis.io/v1alpha1
kind: Performance
metadata:
  name: shop
  namespace: $namespace
spec:
  environment:
    name: production
    configMapRef:
      name: crm-environment
  application:
    image: registry.k8s.io/pause:3.10
    published:
      version: 1.0.0
      packageConfigMapRef:
        name: shop-package-1-0-0
      storageConnectionSecretRef:
        name: shop-storage
      trustedProxies: ["10.244.0.0/16"]
      rolloutTimeoutSeconds: 10
  authProxy:
    enabled: true
    image: registry.k8s.io/pause:3.10
    providers:
      - kind: GitHub
        identity: GitHub
  ingress:
    enabled: true
    hosts: [shop.example.test]
YAML

wait_for "the published application to be applied" 'kubectl -n "$namespace" get deployment shop-app -o name'
test "$(kubectl -n "$namespace" get deployment shop-app -o jsonpath='{.spec.template.spec.volumes[?(@.name=="package")].configMap.name}')" = "shop-package-1-0-0" || fail "the package ConfigMap is not mounted"
kubectl -n "$namespace" get deployment shop-app -o jsonpath='{.spec.template.spec.containers[0].env[*].name}' | grep -q 'Stage__Published__Storage__ConnectionStringFile' || fail "Stage is not told where its storage connection is"
kubectl -n "$namespace" get networkpolicy shop-app -o name || fail "the application is not restricted to AuthProxy"
test "$(kubectl -n "$namespace" get service shop-app -o jsonpath='{.spec.type}')" = "ClusterIP" || fail "the published application service is not ClusterIP"
test "$(kubectl -n "$namespace" get ingress shop -o jsonpath='{.spec.rules[0].http.paths[0].backend.service.name}')" = "shop-authproxy" || fail "the ingress does not route to AuthProxy"
if kubectl -n "$namespace" get deployments,performance -o yaml | grep -q "$storage_value"; then fail "the storage connection string is in a Deployment or the status"; fi
wait_for "the version that never became ready to be recorded as failed" '[ "$(kubectl -n "$namespace" get performance shop -o jsonpath="{.status.versions[0].outcome}")" = "Failed" ]'

# 6. A lifetime that has ended stops the application and removes its public route.
kubectl -n "$namespace" patch performance shop --type merge -p '{"spec":{"lifetime":{"until":"2020-01-01T00:00:00Z"}}}'
wait_for "the published application to expire" '[ "$(kubectl -n "$namespace" get performance shop -o jsonpath="{.status.phase}")" = "Expired" ]'
wait_for "the public route to be removed" '! kubectl -n "$namespace" get ingress shop'
test "$(kubectl -n "$namespace" get deployment shop-app -o jsonpath='{.spec.replicas}')" = "0" || fail "the expired application still runs"

echo "Stage Manager end-to-end checks passed."
