#!/usr/bin/env bash
set -e

TABLE="$1"
USER="$2"
SQLITE_DB="$3"

# get column list from postgres
COLS=$(psql -U "$USER" -Atc "
select string_agg(column_name, ',' order by ordinal_position)
from information_schema.columns
where table_name='${TABLE}';
")

# dump sqlite in same column order
sqlite3 "$SQLITE_DB" -csv -header "select ${COLS} from ${TABLE}" > dump.csv

# clear postgres table
psql -U "$USER" -c "truncate table ${TABLE} restart identity;"

# import
psql -U "$USER" -c "\copy ${TABLE}(${COLS}) from 'dump.csv' csv header"
