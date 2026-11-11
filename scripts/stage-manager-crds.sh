#!/usr/bin/env bash
# Generates the custom resource definitions of Stage Manager from the C# types into the Helm chart.
# The CRDs are never written by hand: change the types in Source/DotNET/StageManager and run this.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
chart="$root/Source/Helm/showrunner-operator"
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

cd "$root"
dotnet tool restore >/dev/null
# The generator compiles the project in memory from its restore assets (obj/project.assets.json), so the project
# must be restored and built first; on a clean checkout it otherwise sees no package references at all.
dotnet build "$root/Source/DotNET/StageManager/StageManager.csproj" --configuration Release -p:CratisProxiesSkip=true --nologo -v quiet

# The Debug configuration compiles the inline specifications, which the generator has no test packages for.
Configuration=Release dotnet kubeops generate operator --no-ansi --out "$work" stage-manager "$root/Source/DotNET/StageManager/StageManager.csproj" >/dev/null

mkdir -p "$chart/crds"
rm -f "$chart"/crds/*.yaml
shopt -s nullglob
crds=("$work"/*_showrunner_cratis_io.yaml)
if [ "${#crds[@]}" -eq 0 ]; then
  echo "No custom resource definitions were generated." >&2
  exit 1
fi
for crd in "${crds[@]}"; do
  cp "$crd" "$chart/crds/$(basename "$crd")"
done
echo "Generated ${#crds[@]} custom resource definitions into $chart/crds"
