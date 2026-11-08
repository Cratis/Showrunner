#!/usr/bin/env bash
# Checks what the Stage Manager chart grants without a cluster: the operator's ClusterRole only watches
# Performances, reads Productions and records events, and everything it does for a Performance (Secrets,
# ConfigMaps, workloads, routes, metrics) is granted by a Role and RoleBinding rendered in the managed namespaces
# only (the release namespace by default). Needs helm on the path. CI runs it in the "Stage Manager" job of build.yml.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
chart="$root/Source/Helm/showrunner-operator"
role_template="templates/managed-namespaces-role.yaml"

fail() { echo "FAILED: $*" >&2; exit 1; }

# The resources of every rule of the ClusterRole, sorted and unique.
cluster_resources="$(helm template stage-manager "$chart" --namespace stage-manager --show-only templates/clusterrole.yaml \
  | awk '/^[[:space:]]*resources:/ { sub(/^[^[]*\[/, ""); sub(/\].*$/, ""); gsub(/[" ]/, ""); n = split($0, r, ","); for (i = 1; i <= n; i++) print r[i] }' \
  | sort -u | tr '\n' ' ')"
test "$cluster_resources" = "events performances performances/finalizers performances/status productions " \
  || fail "the ClusterRole grants more than Performances, Productions and events; grant it in the managed-namespaces Role instead (got: $cluster_resources)"

# The namespaces the Roles and RoleBindings are rendered in, sorted and unique.
role_namespaces() {
  helm template stage-manager "$chart" --namespace stage-manager --show-only "$role_template" "$@" \
    | awk '/^kind:/ { kind = $2 } /^  namespace:/ && (kind == "Role" || kind == "RoleBinding") { print kind ":" $2 }' \
    | sort -u | tr '\n' ' '
}

default_namespaces="$(role_namespaces)"
test "$default_namespaces" = "Role:stage-manager RoleBinding:stage-manager " \
  || fail "by default the managed-namespaces Role is not in the release namespace only (got: $default_namespaces)"

listed_namespaces="$(role_namespaces --set 'managedNamespaces={apps,studio-apps,apps}')"
test "$listed_namespaces" = "Role:apps Role:studio-apps RoleBinding:apps RoleBinding:studio-apps " \
  || fail "the managed-namespaces Role is not in exactly the managed namespaces (got: $listed_namespaces)"

# The Role grants only what the operator does with Secrets.
role="$(helm template stage-manager "$chart" --namespace stage-manager --show-only "$role_template")"
grep -A2 -E '^[[:space:]]*resources: \["secrets"\]$' <<<"$role" | grep -E '^[[:space:]]*verbs: \["get", "create", "update"\]$' >/dev/null \
  || fail "the Role grants other verbs on Secrets than get, create and update"

# The RoleBinding names the operator's service account in the release namespace.
binding="$(helm template stage-manager "$chart" --namespace stage-manager --show-only "$role_template" --set 'managedNamespaces={apps}')"
grep -A2 -E '^[[:space:]]*- kind: ServiceAccount$' <<<"$binding" | grep -E '^[[:space:]]*namespace: stage-manager$' >/dev/null \
  || fail "the RoleBinding does not bind the service account of the release namespace"

echo "The chart grants the operator access only in the managed namespaces."
