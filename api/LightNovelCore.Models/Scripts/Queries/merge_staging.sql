WITH src AS (
    SELECT DISTINCT
        publisher_slug AS slug,
        publisher AS name
    FROM lnc_novel_staging
    WHERE
        publisher_slug IS NOT NULL AND 
        publisher IS NOT NULL
), ins AS (
    INSERT INTO lnc_publishers (slug, name, updated_at)
    SELECT slug, name, CURRENT_TIMESTAMP
    FROM src
    ON CONFLICT (slug) DO UPDATE SET 
        name = EXCLUDED.name,
        updated_at = CURRENT_TIMESTAMP
    RETURNING *, (xmax = 0) AS inserted
)
SELECT *
FROM ins
WHERE inserted;

WITH src AS (
    SELECT DISTINCT
        series_slug AS slug,
        series AS title
    FROM lnc_novel_staging
    WHERE series_slug IS NOT NULL
),
ins AS (
    INSERT INTO lnc_series (slug, title, updated_at)
    SELECT slug, title, CURRENT_TIMESTAMP
    FROM src
    ON CONFLICT (slug) DO UPDATE SET 
        title = COALESCE(EXCLUDED.title, lnc_series.title),
        updated_at = CURRENT_TIMESTAMP
    RETURNING *, (xmax = 0) AS inserted
)
SELECT *
FROM ins
WHERE inserted;

WITH resolved AS (
    SELECT
        s.id AS series_id,
        NULLIF(trim(st.volume), '') AS volume,
        NULLIF(trim(st.title), '')  AS title
    FROM lnc_novel_staging st
    JOIN lnc_series s ON s.slug = st.series_slug
    WHERE st.deleted_at IS NULL
      AND st.volume IS NOT NULL
      AND st.title IS NOT NULL
),
src AS (
    SELECT DISTINCT
        series_id,
        volume,
        title
    FROM resolved
    WHERE volume IS NOT NULL AND title IS NOT NULL
),
ins AS (
    INSERT INTO lnc_volumes (series_id, volume, title, updated_at)
    SELECT
        series_id,
        volume,
        title,
        CURRENT_TIMESTAMP
    FROM src
    ON CONFLICT (series_id, volume, title) DO UPDATE SET
        updated_at = CURRENT_TIMESTAMP
    RETURNING *, (xmax = 0) AS inserted
)
SELECT *
FROM ins
WHERE inserted;

WITH resolved AS (
    SELECT
        v.id AS volume_id,
        MD5(
            st.format::text || '-' ||
            COALESCE(st.isbn, '') || '-' ||
            COALESCE(st.url, '') || '-' ||
            TO_CHAR(st.release_date, 'YYYY-MM-DD')
        ) as hash,
        p.id AS publisher_id,
        st.format,
        NULLIF(st.isbn, '') as isbn,
        NULLIF(st.url, '') as url,
        st.release_date
    FROM lnc_novel_staging st
    JOIN lnc_series s ON s.slug = st.series_slug
    JOIN lnc_volumes v ON v.series_id = s.id AND v.volume = st.volume AND v.title  = st.title
    JOIN lnc_publishers p ON p.slug = st.publisher_slug
    WHERE st.deleted_at IS NULL
), ins AS (
    INSERT INTO lnc_publications (
        volume_id,
        hash,
        publisher_id,
        format,
        isbn,
        url,
        release_date,
        updated_at
    )
    SELECT
        volume_id,
        hash,
        publisher_id,
        format,
        isbn,
        url,
        release_date,
        CURRENT_TIMESTAMP
    FROM resolved
    ON CONFLICT (volume_id, hash) DO UPDATE SET
        publisher_id = EXCLUDED.publisher_id,
        format = EXCLUDED.format,
        isbn = EXCLUDED.isbn,
        url = EXCLUDED.url,
        release_date = EXCLUDED.release_date,
        updated_at = CURRENT_TIMESTAMP
    RETURNING *, (xmax = 0) AS inserted
)
SELECT *
FROM ins
WHERE inserted;