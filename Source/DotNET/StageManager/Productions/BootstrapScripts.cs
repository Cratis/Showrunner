// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

#pragma warning disable MA0136

/// <summary>
/// The scripts the helper containers of a Production run.
/// </summary>
public static class BootstrapScripts
{
    /// <summary>
    /// The script that runs next to every node of a managed Vault.
    /// </summary>
    /// <remarks>
    /// It never ends. The first node initializes Vault once and keeps what it returns in a Secret. Every node keeps itself unsealed with that key,
    /// unless a cloud key service or a transit Vault does it. The first node then mounts KV v2 at <c>secret/</c> and writes the token Chronicle uses
    /// to a Secret, as its last step so the operator can treat that Secret as "Vault is ready". The development tier hands Chronicle the root token;
    /// the production tier a token whose policy reaches only the key paths of Chronicle. That token is periodic, and every node renews it hourly.
    /// </remarks>
    public const string Vault =
        """
        #!/usr/bin/env bash
        set -uo pipefail

        : "${VAULT_ADDR:=http://127.0.0.1:8200}"
        : "${NAMESPACE:?NAMESPACE is required}"
        : "${POD_NAME:?POD_NAME is required}"
        : "${KEYS_SECRET:?KEYS_SECRET is required}"
        : "${CHRONICLE_SECRET:?CHRONICLE_SECRET is required}"
        : "${TIER:=Development}"
        : "${UNSEAL:=secret}"

        ORDINAL="${POD_NAME##*-}"

        log() { echo "[vault-bootstrap] $*"; }
        seal_status() { curl -s "${VAULT_ADDR}/v1/sys/seal-status"; }
        secret_exists() { kubectl get secret "$1" -n "${NAMESPACE}" >/dev/null 2>&1; }
        secret_value() { kubectl get secret "$1" -n "${NAMESPACE}" -o "jsonpath={.data.$2}" 2>/dev/null | base64 -d; }

        read -r -d '' POLICY <<'HCL'
        path "sys/mounts/secret"    { capabilities = ["read"] }
        path "secret/data/k1/*"     { capabilities = ["create", "read", "update"] }
        path "secret/metadata/k1/*" { capabilities = ["read", "list", "delete"] }
        path "secret/data/~layout"  { capabilities = ["create", "read"] }
        path "secret/metadata/"     { capabilities = ["list"] }
        path "auth/token/renew-self" { capabilities = ["update"] }
        HCL

        log "waiting for the Vault API at ${VAULT_ADDR} ..."
        until [ -n "$(seal_status | jq -r '.initialized // empty')" ]; do sleep 2; done

        initialize() {
            if [ "${UNSEAL}" = "secret" ]; then
                log "initializing Vault (1 key share) ..."
                INIT=$(curl -fsS -X PUT -d '{"secret_shares":1,"secret_threshold":1}' "${VAULT_ADDR}/v1/sys/init") || return 1
                KEY_NAME="unsealKey"; KEY=$(echo "${INIT}" | jq -r '.keys_base64[0]')
            else
                log "initializing Vault (1 recovery key share) ..."
                INIT=$(curl -fsS -X PUT -d '{"recovery_shares":1,"recovery_threshold":1}' "${VAULT_ADDR}/v1/sys/init") || return 1
                KEY_NAME="recoveryKey"; KEY=$(echo "${INIT}" | jq -r '.recovery_keys_base64[0]')
            fi
            ROOT=$(echo "${INIT}" | jq -r '.root_token')
            log "keeping the key and the root token in the Secret ${KEYS_SECRET} ..."
            kubectl create secret generic "${KEYS_SECRET}" -n "${NAMESPACE}" \
                --from-literal="${KEY_NAME}=${KEY}" --from-literal="rootToken=${ROOT}"
        }

        keep_unsealed() {
            [ "${UNSEAL}" = "secret" ] || return 0
            [ "$(seal_status | jq -r '.sealed')" = "true" ] || return 0
            secret_exists "${KEYS_SECRET}" || return 0
            log "unsealing Vault ..."
            curl -fsS -X PUT -d "{\"key\":\"$(secret_value "${KEYS_SECRET}" unsealKey)\"}" "${VAULT_ADDR}/v1/sys/unseal" >/dev/null
        }

        configure() {
            secret_exists "${CHRONICLE_SECRET}" && return 0
            secret_exists "${KEYS_SECRET}" || return 1
            [ "$(curl -s -o /dev/null -w '%{http_code}' "${VAULT_ADDR}/v1/sys/health")" = "200" ] || return 1
            ROOT=$(secret_value "${KEYS_SECRET}" rootToken)

            if ! curl -fsS -H "X-Vault-Token: ${ROOT}" "${VAULT_ADDR}/v1/sys/mounts" | jq -e '."secret/"' >/dev/null 2>&1; then
                log "mounting KV v2 at secret/ ..."
                curl -fsS -H "X-Vault-Token: ${ROOT}" -X POST -d '{"type":"kv","options":{"version":"2"}}' "${VAULT_ADDR}/v1/sys/mounts/secret" >/dev/null || return 1
            fi

            if [ "${TIER}" = "Production" ]; then
                log "writing the policy of Chronicle and its token ..."
                curl -fsS -H "X-Vault-Token: ${ROOT}" -X PUT -d "$(jq -n --arg policy "${POLICY}" '{policy: $policy}')" "${VAULT_ADDR}/v1/sys/policies/acl/chronicle" >/dev/null || return 1
                TOKEN=$(curl -fsS -H "X-Vault-Token: ${ROOT}" -X POST \
                    -d '{"policies":["chronicle"],"no_parent":true,"no_default_policy":true,"period":"168h","renewable":true}' \
                    "${VAULT_ADDR}/v1/auth/token/create" | jq -r '.auth.client_token') || return 1
            else
                TOKEN="${ROOT}"
            fi

            kubectl create secret generic "${CHRONICLE_SECRET}" -n "${NAMESPACE}" --from-literal="token=${TOKEN}" || return 1
            log "Vault is initialized, unsealed and has KV v2 mounted; the token of Chronicle is in ${CHRONICLE_SECRET}."
        }

        renew() {
            [ "${TIER}" = "Production" ] || return 0
            secret_exists "${CHRONICLE_SECRET}" || return 0
            curl -fsS -H "X-Vault-Token: $(secret_value "${CHRONICLE_SECRET}" token)" -X POST "${VAULT_ADDR}/v1/auth/token/renew-self" >/dev/null || log "renewing the token of Chronicle failed; will retry."
        }

        TICK=0
        while true; do
            if [ "${ORDINAL}" = "0" ] && [ "$(seal_status | jq -r '.initialized')" != "true" ]; then
                initialize || log "initializing failed; will retry."
            fi
            keep_unsealed || log "unsealing failed; will retry."
            if [ "${ORDINAL}" = "0" ]; then configure || true; fi
            if [ $((TICK % 720)) -eq 0 ]; then renew; fi
            TICK=$((TICK + 1))
            sleep 5
        done
        """;
#pragma warning restore MA0136
}
