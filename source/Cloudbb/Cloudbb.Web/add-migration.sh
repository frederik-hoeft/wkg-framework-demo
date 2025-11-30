#!/usr/bin/env bash

set -euo pipefail
shopt -s nullglob

# ensure that a valid migration name is provided (PascalCase, starts with a letter, only letters and digits)
migration_name="${1}"
if [[ ! "${migration_name}" =~ ^[A-Z][A-Za-z0-9]*$ ]]; then
    echo "Error: Migration name must be in PascalCase and start with a letter." >&2
    exit 1
fi
# get project file
script_dir="$(/usr/bin/dirname "$(/usr/bin/realpath "${BASH_SOURCE[0]}")")"
project_file="$(/usr/bin/find "${script_dir}" -name "*.csproj" | /usr/bin/head -n 1)"
if [[ ! -f "${project_file}" ]]; then
    echo "Error: Could not find a .csproj file in the script directory." >&2
    exit 1
fi
# add migration
dotnet ef migrations add "${migration_name}" --project "${project_file}" --output-dir 'Data/Migrations'