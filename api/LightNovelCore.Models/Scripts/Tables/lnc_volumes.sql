CREATE TABLE IF NOT EXISTS lnc_volumes (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	series_id UUID NOT NULL REFERENCES lnc_series(id),
	volume TEXT NOT NULL,
	title TEXT NOT NULL,
	created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	deleted_at TIMESTAMP NULL,
	CONSTRAINT lnc_volumes_unique UNIQUE (series_id, volume, title)
);

ALTER TABLE lnc_volumes
ADD COLUMN IF NOT EXISTS
	fts tsvector GENERATED ALWAYS AS (
		to_tsvector('english',
			title || ' ' ||
			volume
		)
	) STORED;
