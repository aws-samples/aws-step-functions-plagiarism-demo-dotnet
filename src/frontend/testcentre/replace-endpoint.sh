#!/usr/bin/env bash
# Used during build to replace the APIGW endpoint value provided to Vue.
# Call like `replace-endpoint.sh foo`, and .env will contain:
# VUE_APP_APIGW_ENDPOINT=foo
echo "VUE_APP_APIGW_ENDPOINT=$1" > .env