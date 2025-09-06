#!/bin/bash

SOLUTION_DIR="/Users/ekaterinaparamonova/Desktop/6sem/ppo/fullproject"
OUTPUT_DIR="/Users/ekaterinaparamonova/Desktop/6sem/ppo/publish"
MAIN_PROJECT="Eventity.Web"
CONFIGURATION="Release"

echo "Очистка предыдущей сборки..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

echo "Публикация проекта $MAIN_PROJECT..."
dotnet publish "$SOLUTION_DIR/$MAIN_PROJECT/$MAIN_PROJECT.csproj" \
  -c $CONFIGURATION \
  -o "$OUTPUT_DIR" \
  --no-restore


