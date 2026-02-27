#!/bin/sh

app_name="lightnovelcore"
root_dir="$app_name"


create_dirs() {
  mkdir -p "./setup"
  mkdir -p "./redis"
  mkdir -p "./postgres"
  mkdir -p "./file-cache"
  mkdir -p "./logs"
}

create_env() {
  filename="./.env"
  if [ -e "$filename" ] && [ "$force" -ne 1 ]; then
    echo "File $filename already exists. Skipping creation."
    return
  fi

  echo "Creating $filename file..."
  starting_port=10000
  api_port=$((starting_port + 1))
  ui_port=$((starting_port + 2))
  db_port=$((starting_port + 3))
  redis_port=$((starting_port + 4))
  memcached_port=$((starting_port + 5))
  bookcover_port=$((starting_port + 6))

  postgres_username=$(tr -dc 'a-z' < /dev/urandom | head -c 10)
  postgres_password=$(tr -dc 'A-Za-z0-9!?%=' < /dev/urandom | head -c 64)
  redis_password=$(tr -dc 'A-Za-z0-9!?%=' < /dev/urandom | head -c 64)

  cat > "$filename" <<EOF

POSTGRES_USERNAME=$postgres_username
POSTGRES_PASSWORD=$postgres_password
POSTGRES_SCHEMA=$app_name

REDIS_PASSWORD=$redis_password

PORT_API=$api_port
PORT_UI=$ui_port
PORT_DB=$db_port
PORT_REDIS=$redis_port
PORT_MEMCACHED=$memcached_port
PORT_BOOKCOVER=$bookcover_port
EOF
  echo "$filename created"
}

create_compose() {
  filename="./docker-compose.yml"
  if [ -e "$filename" ] && [ "$force" -ne 1 ]; then
    echo "File $filename already exists. Skipping creation."
    return
  fi

  cat > "$filename" <<'EOF'
networks:
  app-network:
    driver: bridge

services:

  app-redis:
    image: redis/redis-stack-server:latest
    restart: unless-stopped
    ports:
      - ${PORT_REDIS}:6379
    environment:
      - REDIS_ARGS=--requirepass ${REDIS_PASSWORD}
    volumes:
      - ./redis:/data
    networks:
      - app-network

  app-db:
    image: postgres:latest
    restart: unless-stopped
    ports:
      - ${PORT_DB}:5432
    volumes:
      - ./postgres:/var/lib/postgresql/data
    environment:
      - POSTGRES_USER=${POSTGRES_USERNAME}
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
      - POSTGRES_DB=${POSTGRES_SCHEMA}
    networks:
      - app-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USERNAME} -d ${POSTGRES_SCHEMA}"]
      interval: 5s
      timeout: 5s
      retries: 5

  app-memcached:
    image: memcached:1.6-alpine
    ports:
      - ${PORT_MEMCACHED}:11211
    networks:
      - app-network
    command: memcached -m 64
    restart: unless-stopped

  app-bookcover:
    build: 
      context: ./bookcover-api
      target: final
    ports:
      - ${PORT_BOOKCOVER}:8000
    environment:
      - MEMCACHED_HOST=app-memcached
    networks:
      - app-network
    depends_on:
      - app-memcached
    restart: unless-stopped
  
  app-api:
    image: ghcr.io/cardboards-box/light-novel-api/api:latest
    restart: unless-stopped
    ports:
      - ${PORT_API}:8080
    volumes:
      - ./file-cache:/app/file-cache
      - ./logs:/app/logs
    environment:
      - Database:ConnectionString=User ID=${POSTGRES_USERNAME};Password=${POSTGRES_PASSWORD};Host=app-db;Database=${POSTGRES_SCHEMA};
      - Redis:Connection=app-redis,password=${REDIS_PASSWORD}
      - Imaging:CacheDir=./file-cache
      - Covers:Url=http://app-bookcover:8000
    networks:
      - app-network
    depends_on:
      - app-db
      - app-redis
      - app-bookcover
  
  app-ui:
    image: ghcr.io/cardboards-box/light-novel-api/ui:latest
    restart: unless-stopped
    ports:
      - ${PORT_UI}:3000
    environment:
      - NUXT_PUBLIC_API_URL=http://app-api:8080
    networks:
      - app-network
    depends_on:
      - app-api
EOF
  echo "$filename created"
}

setup_bookcover_api() {
    if [ -d "./bookcover-api/.git" ]; then
        echo "bookcover-api already exists; pulling latest..."
        (cd ./bookcover-api && git pull)
    elif [ -d "./bookcover-api" ]; then
        echo "bookcover-api exists but is not a git repo; skipping clone."
    else
        git clone https://github.com/w3slley/bookcover-api ./bookcover-api
    fi
}

mkdir -p "./$root_dir"
cd "./$root_dir"

create_dirs
create_env
create_compose
setup_bookcover_api

chmod 777 -R "../$root_dir"

docker pull ghcr.io/cardboards-box/light-novel-api/api:latest
docker pull ghcr.io/cardboards-box/light-novel-api/ui:latest
docker compose up -d

cd ..