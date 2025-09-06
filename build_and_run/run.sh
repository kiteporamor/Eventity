#!/bin/bash

SOLUTION_DIR="/Users/ekaterinaparamonova/Desktop/6sem/ppo/fullproject"
OUTPUT_DIR="/Users/ekaterinaparamonova/Desktop/6sem/ppo/publish"
MAIN_PROJECT="Eventity.Web"
FRONTEND_DIR="/Users/ekaterinaparamonova/Desktop/6sem/ppo/f" 
FRONTEND_CONFIG_PATH="$FRONTEND_DIR/public/frontend-config.json" 

cp "$SOLUTION_DIR/$MAIN_PROJECT/appsettings.json" "$OUTPUT_DIR/"
cp "$SOLUTION_DIR/$MAIN_PROJECT/appsettings.Development.json" "$OUTPUT_DIR/"

API_BASE_URL=$(jq -r '.Kestrel.EndPoints.Http.Url' "$SOLUTION_DIR/$MAIN_PROJECT/appsettings.json")
cat <<EOF > "$FRONTEND_CONFIG_PATH"
{
  "apiBaseUrl": "$API_BASE_URL/api"
}
EOF

export ASPNETCORE_ENVIRONMENT=Development
export Swagger__Enabled=true
export ASPNETCORE_URLS=http://localhost:5001

echo "Запуск приложения на http://localhost:5001 ..."
cd "$OUTPUT_DIR"
dotnet "$MAIN_PROJECT.dll" &

cd "$FRONTEND_DIR"
live-server --port=3000

