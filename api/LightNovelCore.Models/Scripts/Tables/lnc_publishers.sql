CREATE TABLE IF NOT EXISTS lnc_publishers (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	slug TEXT NOT NULL UNIQUE,
	name TEXT NOT NULL,
	icon_url TEXT NULL,
	website TEXT NULL,
	created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	deleted_at TIMESTAMP NULL
);

ALTER TABLE lnc_publishers
ADD COLUMN IF NOT EXISTS
	fts tsvector GENERATED ALWAYS AS (
		to_tsvector('english',
			name
		)
	) STORED;
