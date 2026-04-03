#!/usr/bin/env bash

set -euo pipefail

if [[ -x "/opt/homebrew/opt/dotnet/libexec/dotnet" ]]; then
  export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
  export PATH="$DOTNET_ROOT:$PATH"
fi

exec docfx "$@"
