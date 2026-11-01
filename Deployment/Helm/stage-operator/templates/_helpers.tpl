{{- define "stage-operator.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "stage-operator.fullname" -}}
{{- if contains .Chart.Name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name .Chart.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{- define "stage-operator.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
{{- default (include "stage-operator.fullname" .) .Values.serviceAccount.name -}}
{{- else -}}
{{- default "default" .Values.serviceAccount.name -}}
{{- end -}}
{{- end -}}

{{- define "stage-operator.labels" -}}
app.kubernetes.io/name: {{ include "stage-operator.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end -}}

{{- define "stage-operator.selectorLabels" -}}
app.kubernetes.io/name: {{ include "stage-operator.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{/* The rules the operator needs; the same list is used by the ClusterRole and by each namespaced Role. */}}
{{- define "stage-operator.rules" -}}
- apiGroups: ["showrunner.cratis.io"]
  resources: ["stageapplications"]
  verbs: ["get", "list", "watch", "update", "patch"]
- apiGroups: ["showrunner.cratis.io"]
  resources: ["stageapplications/status"]
  verbs: ["get", "update", "patch"]
- apiGroups: ["showrunner.cratis.io"]
  resources: ["stageapplications/finalizers"]
  verbs: ["update"]
- apiGroups: ["apps"]
  resources: ["deployments", "statefulsets"]
  verbs: ["get", "list", "watch", "create", "update", "patch", "delete"]
- apiGroups: ["apps"]
  resources: ["deployments/scale", "statefulsets/scale"]
  verbs: ["get", "update", "patch"]
- apiGroups: [""]
  resources: ["services"]
  verbs: ["get", "list", "watch", "create", "update", "patch", "delete"]
- apiGroups: [""]
  resources: ["persistentvolumeclaims"]
  verbs: ["get", "list", "watch", "delete", "deletecollection"]
- apiGroups: ["networking.k8s.io"]
  resources: ["ingresses", "networkpolicies"]
  verbs: ["get", "list", "watch", "create", "update", "patch", "delete"]
- apiGroups: ["metrics.k8s.io"]
  resources: ["pods"]
  verbs: ["get", "list"]
{{- end -}}
