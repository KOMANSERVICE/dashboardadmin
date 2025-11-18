FROM postgres:15

WORKDIR /app

COPY migrations/migrations.sql /migrations/migrations.sql

# Variables d'environnement par défaut (peut être remplacé par docker-compose)
ENV DB_HOST=dashadminDB \
    DB_PORT=5437 \
    DB_USER=${POSTGRES_USER} \
    DB_PASSWORD=${POSTGRES_PASSWORD} \
    DB_NAME=${POSTGRES_DB}

CMD ["sh", "-c", "for f in /migrations/*.sql; do echo 'Running $f'; PGPASSWORD=$DB_PASSWORD psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f $f; done"]
