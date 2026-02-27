CREATE TABLE IF NOT EXISTS lnc_publications (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	volume_id UUID NOT NULL REFERENCES lnc_volumes(id),
	hash TEXT NOT NULL,
	publisher_id UUID NOT NULL REFERENCES lnc_publishers(id),
	format INTEGER NOT NULL,
	isbn TEXT NULL,
	url TEXT NULL,
	release_date TIMESTAMP NOT NULL,
	created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	deleted_at TIMESTAMP NULL,
	CONSTRAINT lnc_publications_unique UNIQUE (volume_id, hash)
);
