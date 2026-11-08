#!/usr/bin/env bash
# Checks what the Stage Manager chart grants without a cluster: the operator's ClusterRole carries no access to
# Secrets, and the Secrets Role and RoleBinding are rendered in the managed namespaces only (the release namespace
# by default). Needs helm on the path. CI runs it in the "Stage Manager" job of build.yml.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
chart="$root/Source/Helm/showrunner-operator"

fail() { echo "FAILED: $*" >&2; exit 1; }

# The resources and kinds of every rule of the ClusterRole, one per line.
cluster_role="$(helm template stage-manager "$chart" --namespace stage-manager --show-only templates/clusterrole.yaml)"
if grep -E '^[[:space:]]*resources:.*"secrets"' <<<"$cluster_role" >/dev/null; then
  fail "the ClusterRole grants access to Secrets; grant it through the namespaced Secrets Role instead"
fi
if grep -E '^[[:space:]]*resources:.*"\*"' <<<"$cluster_role" >/dev/null; then
  fail "the ClusterRole grants access to every resource"
fi

# The namespaces the Secrets Roles and RoleBindings are rendered in, sorted and unique.
secrets_namespaces() {
  helm template stage-manager "$chart" --namespace stage-manager --show-only templates/secrets-role.yaml "$@" \
    | awk '/^kind:/ { kind = $2 } /^  namespace:/ && (kind == "Role" || kind == "RoleBinding") { print kind ":" $2 }' \
    | sort -u | tr '\n' ' '
}

default_namespaces="$(secrets_namespaces)"
test "$default_namespaces" = "Role:stage-manager RoleBinding:stage-manager " \
  || fail "by default the Secrets Role is not in the release namespace only (got: $default_namespaces)"

listed_namespaces="$(secrets_namespaces --set 'managedNamespaces={apps,studio-apps,apps}')"
test "$listed_namespaces" = "Role:apps Role:studio-apps RoleBinding:apps RoleBinding:studio-apps " \
  || fail "the Secrets Role is not in exactly the managed namespaces (got: $listed_namespaces)"

# The Role grants only what the operator does with Secrets.
role="$(helm template stage-manager "$chart" --namespace stage-manager --show-only templates/secrets-role.yaml)"
grep -E '^[[:space:]]*verbs: \["get", "create", "update"\]$' <<<"$role" >/dev/null \
  || fail "the Secrets Role grants other verbs than get, create and update"

# The RoleBinding names the operator's service account in the release namespace.
grep -E '^[[:space:]]*namespace: stage-manager$' <<<"$(helm template stage-manager "$chart" --namespace stage-manager \
  --show-only templates/secrets-role.yaml --set 'managedNamespaces={apps}')" >/dev/null \
  || fail "the Secrets RoleBinding does not bind the service account of the release namespace"

echo "The chart grants Secrets only in the managed namespaces."
