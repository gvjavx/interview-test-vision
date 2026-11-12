#!/usr/bin/env bash
set -euo pipefail

BASE="${1:-http://localhost:5000}"
response=$(mktemp)
trap 'rm -f "$response"' EXIT
failures=0

check() {
  local expected=$1 method=$2 path=$3 body=${4:-}
  local args=(-s -o "$response" -w '%{http_code}' -X "$method" "$BASE$path")
  [[ -n $body ]] && args+=(-H 'Content-Type: application/json' -d "$body")
  local status
  status=$(curl "${args[@]}")
  if [[ $status == "$expected" ]]; then
    echo "ok   $method $path -> $status"
  else
    echo "FAIL $method $path -> $status (expected $expected)"
    failures=$((failures + 1))
  fi
}

check 200 GET /health
check 201 POST /api/todos '{"title":"Write README","isDone":false}'
id=$(sed -n 's/.*"id":\([0-9]*\).*/\1/p' "$response")
id=${id:-missing}
check 400 POST /api/todos '{"title":"   "}'
check 200 GET /api/todos
check 200 GET "/api/todos/$id"
check 200 PUT "/api/todos/$id" '{"title":"Write README","isDone":true}'
check 404 PUT /api/todos/0 '{"title":"Missing","isDone":false}'
check 204 DELETE "/api/todos/$id"
check 404 GET "/api/todos/$id"

echo "$failures failure(s)"
exit $((failures > 0))
